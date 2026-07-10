import * as React from "react";
import { useQuery } from "@tanstack/react-query";
import { AlarmClock, BellOff, Clock3 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useAppSelector } from "@/app/hooks";
import { useNow } from "@/shared/lib/useNow";
import { startAlarmSound, stopAlarmSound } from "@/shared/lib/alarmSound";
import { alarmsApi } from "../api/alarmsApi";
import type { Alarm } from "@/types/alarm";

function partsInZone(date: Date, timeZone: string) {
  const parts = new Intl.DateTimeFormat("en-US", {
    timeZone,
    hourCycle: "h23",
    weekday: "short",
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  }).formatToParts(date);
  const get = (type: string) => parts.find((p) => p.type === type)?.value ?? "";
  return {
    hour: Number(get("hour")),
    minute: Number(get("minute")),
    dateKey: `${get("year")}-${get("month")}-${get("day")}`,
    weekday: get("weekday"), // "Sun", "Mon", ...
  };
}

const WEEKDAY_BIT: Record<string, number> = { Sun: 1, Mon: 2, Tue: 4, Wed: 8, Thu: 16, Fri: 32, Sat: 64 };

// How late this tab can notice an alarm it missed (e.g. the tab was hidden/backgrounded, or the
// browser throttled its timers) and still ring it, rather than only matching the exact live-
// ticking minute. Doesn't cross midnight - an alarm at 23:58 won't catch up at 00:02, which is an
// accepted simplification (matches the same-calendar-day dedupe model below) rather than a fix
// for every possible miss.
const CATCH_UP_WINDOW_MINUTES = 5;

interface MathChallenge {
  a: number;
  b: number;
}

export function AlarmRingingOverlay() {
  const user = useAppSelector((s) => s.auth.user);
  const now = useNow(1000);
  const { data: alarms } = useQuery({
    queryKey: ["alarms"],
    queryFn: alarmsApi.list,
    enabled: !!user,
    refetchInterval: 60_000,
  });

  const [ringing, setRinging] = React.useState<Alarm[]>([]);
  const [challenges, setChallenges] = React.useState<Record<string, MathChallenge>>({});
  const [answers, setAnswers] = React.useState<Record<string, string>>({});

  const firedTodayRef = React.useRef<Map<string, string>>(new Map());
  const snoozedUntilRef = React.useRef<Map<string, number>>(new Map());

  React.useEffect(() => {
    if (!user || !alarms) return;
    const timeZone = user.timezoneId;

    for (const alarm of alarms) {
      if (!alarm.isEnabled) continue;

      const snoozedUntil = snoozedUntilRef.current.get(alarm.id);
      if (snoozedUntil !== undefined) {
        if (now.getTime() < snoozedUntil) continue; // still snoozing
        snoozedUntilRef.current.delete(alarm.id);
        fire(alarm);
        continue;
      }

      const { hour, minute, dateKey, weekday } = partsInZone(now, timeZone);
      const minutesSinceScheduled = hour * 60 + minute - (alarm.hour * 60 + alarm.minute);
      if (minutesSinceScheduled < 0 || minutesSinceScheduled > CATCH_UP_WINDOW_MINUTES) continue;

      const isOneTime = alarm.repeatDaysMask === 0;
      const scheduledToday = isOneTime || (alarm.repeatDaysMask & (WEEKDAY_BIT[weekday] ?? 0)) !== 0;
      if (!scheduledToday) continue;

      if (firedTodayRef.current.get(alarm.id) === dateKey) continue;
      firedTodayRef.current.set(alarm.id, dateKey);
      fire(alarm);
    }

    function fire(alarm: Alarm) {
      setRinging((prev) => (prev.some((a) => a.id === alarm.id) ? prev : [...prev, alarm]));
      if (alarm.challengeType === "Math") {
        setChallenges((prev) => {
          if (prev[alarm.id]) return prev;
          const a = 1 + Math.floor(Math.random() * 20);
          const b = 1 + Math.floor(Math.random() * 20);
          return { ...prev, [alarm.id]: { a, b } };
        });
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [now, alarms, user]);

  React.useEffect(() => {
    if (ringing.length > 0) {
      startAlarmSound(ringing[0].soundId);
    } else {
      stopAlarmSound();
    }
  }, [ringing]);

  React.useEffect(() => stopAlarmSound, []);

  const clearAlarmState = (id: string) => {
    setRinging((prev) => prev.filter((a) => a.id !== id));
    setChallenges((prev) => {
      const next = { ...prev };
      delete next[id];
      return next;
    });
    setAnswers((prev) => {
      const next = { ...prev };
      delete next[id];
      return next;
    });
  };

  const onSnooze = async (alarm: Alarm) => {
    snoozedUntilRef.current.set(alarm.id, Date.now() + alarm.snoozeMinutes * 60_000);
    clearAlarmState(alarm.id);
    try {
      await alarmsApi.snooze(alarm.id);
    } catch {
      // Best-effort logging - the local snooze/re-ring already happened regardless.
    }
  };

  const onDismiss = async (alarm: Alarm) => {
    clearAlarmState(alarm.id);
    try {
      await alarmsApi.dismiss(alarm.id);
    } catch {
      // Best-effort logging.
    }
  };

  if (ringing.length === 0) return null;
  const alarm = ringing[0];
  const challenge = challenges[alarm.id];
  const answer = answers[alarm.id] ?? "";
  const challengeSolved = !challenge || Number(answer) === challenge.a + challenge.b;

  return (
    <div className="fixed inset-0 z-100 flex items-center justify-center bg-background/95 backdrop-blur-sm">
      <div className="flex w-full max-w-sm flex-col items-center gap-4 rounded-xl border bg-card p-8 text-center shadow-lg">
        <AlarmClock className="size-12 animate-pulse text-primary" />
        <div>
          <p className="text-3xl font-semibold">{alarm.label}</p>
          <p className="text-sm text-muted-foreground">
            {String(alarm.hour).padStart(2, "0")}:{String(alarm.minute).padStart(2, "0")}
          </p>
        </div>

        {challenge && (
          <div className="w-full">
            <p className="mb-1.5 text-sm font-medium">
              Solve to dismiss: {challenge.a} + {challenge.b} = ?
            </p>
            <Input
              type="number"
              value={answer}
              onChange={(e) => setAnswers((prev) => ({ ...prev, [alarm.id]: e.target.value }))}
              autoFocus
            />
          </div>
        )}

        <div className="flex w-full gap-2">
          {alarm.snoozeEnabled && (
            <Button variant="outline" className="flex-1" onClick={() => onSnooze(alarm)}>
              <Clock3 /> Snooze {alarm.snoozeMinutes}m
            </Button>
          )}
          <Button className="flex-1" disabled={!challengeSolved} onClick={() => onDismiss(alarm)}>
            <BellOff /> Dismiss
          </Button>
        </div>
      </div>
    </div>
  );
}
