import * as React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Pause, Play, SkipForward, Square } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { pomodoroApi } from "../api/pomodoroApi";
import { playChimeOnce } from "@/shared/lib/alarmSound";
import { toApiError } from "@/shared/lib/apiClient";
import type { PomodoroPhase } from "@/types/timers";

const PHASE_LABEL: Record<PomodoroPhase, string> = { Work: "Focus", ShortBreak: "Short break", LongBreak: "Long break" };

function formatMmSs(totalSeconds: number) {
  const m = Math.floor(totalSeconds / 60);
  const s = totalSeconds % 60;
  return `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
}

export default function PomodoroPage() {
  const queryClient = useQueryClient();
  const { data: history } = useQuery({ queryKey: ["pomodoroSessions"], queryFn: pomodoroApi.list });

  const [config, setConfig] = React.useState({ workMinutes: 25, shortBreakMinutes: 5, longBreakMinutes: 15, cyclesBeforeLongBreak: 4 });
  const [sessionId, setSessionId] = React.useState<string | null>(null);
  const [phase, setPhase] = React.useState<PomodoroPhase>("Work");
  const [completedWorkCycles, setCompletedWorkCycles] = React.useState(0);
  const [remainingSeconds, setRemainingSeconds] = React.useState(0);
  const [running, setRunning] = React.useState(false);

  const phaseStartedAtRef = React.useRef<Date>(new Date());
  const sessionIdRef = React.useRef<string | null>(null);
  const phaseRef = React.useRef<PomodoroPhase>("Work");
  const completedCyclesRef = React.useRef(0);

  const durationFor = React.useCallback(
    (p: PomodoroPhase) => (p === "Work" ? config.workMinutes : p === "ShortBreak" ? config.shortBreakMinutes : config.longBreakMinutes) * 60,
    [config],
  );

  const advancePhase = React.useCallback(
    async (completedFully: boolean) => {
      const currentSessionId = sessionIdRef.current;
      const currentPhase = phaseRef.current;
      if (!currentSessionId) return;

      const endedAt = new Date();
      try {
        await pomodoroApi.logPhase(currentSessionId, {
          phase: currentPhase,
          startedAt: phaseStartedAtRef.current.toISOString(),
          endedAt: endedAt.toISOString(),
          completedFully,
        });
      } catch {
        // Best-effort logging - the local timer state is authoritative for the running session.
      }

      let nextCycles = completedCyclesRef.current;
      let nextPhase: PomodoroPhase;
      if (currentPhase === "Work") {
        nextCycles += 1;
        nextPhase = nextCycles % config.cyclesBeforeLongBreak === 0 ? "LongBreak" : "ShortBreak";
      } else {
        nextPhase = "Work";
      }

      completedCyclesRef.current = nextCycles;
      phaseRef.current = nextPhase;
      setCompletedWorkCycles(nextCycles);
      setPhase(nextPhase);
      phaseStartedAtRef.current = new Date();
      setRemainingSeconds(durationFor(nextPhase));
      playChimeOnce("gentle");
      toast.info(`${PHASE_LABEL[nextPhase]} started`);
    },
    [config.cyclesBeforeLongBreak, durationFor],
  );

  React.useEffect(() => {
    if (!running || !sessionId) return;
    const id = setInterval(() => {
      setRemainingSeconds((prev) => {
        if (prev <= 1) {
          void advancePhase(true);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(id);
  }, [running, sessionId, advancePhase]);

  const onStart = async () => {
    try {
      const session = await pomodoroApi.start(config);
      sessionIdRef.current = session.id;
      phaseRef.current = "Work";
      completedCyclesRef.current = 0;
      setSessionId(session.id);
      setPhase("Work");
      setCompletedWorkCycles(0);
      phaseStartedAtRef.current = new Date();
      setRemainingSeconds(config.workMinutes * 60);
      setRunning(true);
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const onPause = () => setRunning(false);
  const onResume = () => setRunning(true);

  const onSkip = async () => {
    setRunning(false);
    await advancePhase(false);
    setRunning(true);
  };

  const onEndSession = async () => {
    setRunning(false);
    if (sessionIdRef.current) {
      try {
        await pomodoroApi.logPhase(sessionIdRef.current, {
          phase: phaseRef.current,
          startedAt: phaseStartedAtRef.current.toISOString(),
          endedAt: new Date().toISOString(),
          completedFully: false,
        });
        await pomodoroApi.end(sessionIdRef.current);
        await queryClient.invalidateQueries({ queryKey: ["pomodoroSessions"] });
      } catch (err) {
        toast.error(toApiError(err).message);
      }
    }
    sessionIdRef.current = null;
    setSessionId(null);
  };

  const isActive = sessionId !== null;

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Pomodoro</h1>
        <p className="text-sm text-muted-foreground">Work in focused intervals with short and long breaks in between.</p>
      </div>

      <Card>
        <CardContent className="flex flex-col items-center gap-6 pt-6">
          {isActive ? (
            <>
              <p className="text-sm font-medium uppercase tracking-wide text-muted-foreground">{PHASE_LABEL[phase]}</p>
              <p className="font-mono text-6xl font-bold tabular-nums">{formatMmSs(remainingSeconds)}</p>
              <p className="text-xs text-muted-foreground">
                Cycle {(completedWorkCycles % config.cyclesBeforeLongBreak) + 1} of {config.cyclesBeforeLongBreak}
              </p>
              <div className="flex gap-2">
                {running ? (
                  <Button variant="outline" onClick={onPause}>
                    <Pause /> Pause
                  </Button>
                ) : (
                  <Button onClick={onResume}>
                    <Play /> Resume
                  </Button>
                )}
                <Button variant="outline" onClick={onSkip}>
                  <SkipForward /> Skip
                </Button>
                <Button variant="destructive" onClick={onEndSession}>
                  <Square /> End session
                </Button>
              </div>
            </>
          ) : (
            <>
              <div className="grid w-full gap-4 sm:grid-cols-2">
                <div className="flex flex-col gap-1.5">
                  <Label className="text-xs">Focus (minutes)</Label>
                  <Input
                    type="number"
                    min={1}
                    max={120}
                    value={config.workMinutes}
                    onChange={(e) => setConfig((c) => ({ ...c, workMinutes: Number(e.target.value) }))}
                  />
                </div>
                <div className="flex flex-col gap-1.5">
                  <Label className="text-xs">Short break (minutes)</Label>
                  <Input
                    type="number"
                    min={1}
                    max={60}
                    value={config.shortBreakMinutes}
                    onChange={(e) => setConfig((c) => ({ ...c, shortBreakMinutes: Number(e.target.value) }))}
                  />
                </div>
                <div className="flex flex-col gap-1.5">
                  <Label className="text-xs">Long break (minutes)</Label>
                  <Input
                    type="number"
                    min={1}
                    max={120}
                    value={config.longBreakMinutes}
                    onChange={(e) => setConfig((c) => ({ ...c, longBreakMinutes: Number(e.target.value) }))}
                  />
                </div>
                <div className="flex flex-col gap-1.5">
                  <Label className="text-xs">Cycles before long break</Label>
                  <Input
                    type="number"
                    min={1}
                    max={12}
                    value={config.cyclesBeforeLongBreak}
                    onChange={(e) => setConfig((c) => ({ ...c, cyclesBeforeLongBreak: Number(e.target.value) }))}
                  />
                </div>
              </div>
              <Button onClick={onStart}>
                <Play /> Start session
              </Button>
            </>
          )}
        </CardContent>
      </Card>

      {history && history.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>History</CardTitle>
          </CardHeader>
          <CardContent className="flex flex-col gap-2">
            {history.map((s) => {
              const workPhases = s.logs.filter((l) => l.phase === "Work" && l.completedFully).length;
              return (
                <div key={s.id} className="flex items-center justify-between rounded-md border p-2.5 text-sm">
                  <div>
                    <p className="font-medium">{new Date(s.startedAt).toLocaleString()}</p>
                    <p className="text-xs text-muted-foreground">
                      {s.workMinutes}m focus / {s.shortBreakMinutes}m break
                    </p>
                  </div>
                  <span className="text-muted-foreground">{workPhases} focus cycles completed</span>
                </div>
              );
            })}
          </CardContent>
        </Card>
      )}
    </div>
  );
}
