import * as React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Flag, Loader2, Pause, Play, Save } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { stopwatchApi } from "../api/stopwatchApi";
import { toApiError } from "@/shared/lib/apiClient";
import type { StopwatchLap } from "@/types/timers";

function formatElapsed(ms: number) {
  const totalCentis = Math.floor(ms / 10);
  const centis = totalCentis % 100;
  const totalSeconds = Math.floor(ms / 1000);
  const s = totalSeconds % 60;
  const m = Math.floor(totalSeconds / 60) % 60;
  const h = Math.floor(totalSeconds / 3600);
  const pad = (n: number) => String(n).padStart(2, "0");
  return h > 0 ? `${pad(h)}:${pad(m)}:${pad(s)}.${pad(centis)}` : `${pad(m)}:${pad(s)}.${pad(centis)}`;
}

export default function StopwatchPage() {
  const queryClient = useQueryClient();
  const { data: history } = useQuery({ queryKey: ["stopwatchSessions"], queryFn: stopwatchApi.list });

  const [elapsedMs, setElapsedMs] = React.useState(0);
  const [running, setRunning] = React.useState(false);
  const [laps, setLaps] = React.useState<StopwatchLap[]>([]);
  const [label, setLabel] = React.useState("Session");
  const [saving, setSaving] = React.useState(false);

  const accumulatedRef = React.useRef(0);
  const segmentStartRef = React.useRef(0);
  const startedAtRef = React.useRef<Date | null>(null);

  React.useEffect(() => {
    if (!running) return;
    const id = setInterval(() => setElapsedMs(accumulatedRef.current + (Date.now() - segmentStartRef.current)), 47);
    return () => clearInterval(id);
  }, [running]);

  const onStart = () => {
    if (!startedAtRef.current) startedAtRef.current = new Date();
    segmentStartRef.current = Date.now();
    setRunning(true);
  };

  const onPause = () => {
    accumulatedRef.current += Date.now() - segmentStartRef.current;
    setElapsedMs(accumulatedRef.current);
    setRunning(false);
  };

  const onLap = () => {
    const lastCumulative = laps.at(-1)?.cumulativeDurationMs ?? 0;
    setLaps((prev) => [...prev, { lapNumber: prev.length + 1, lapDurationMs: elapsedMs - lastCumulative, cumulativeDurationMs: elapsedMs }]);
  };

  const onFinish = async () => {
    const finalElapsed = running ? accumulatedRef.current + (Date.now() - segmentStartRef.current) : elapsedMs;
    setRunning(false);
    if (finalElapsed === 0 || !startedAtRef.current) return;

    setSaving(true);
    try {
      await stopwatchApi.save({
        label: label.trim() || "Session",
        startedAt: startedAtRef.current.toISOString(),
        endedAt: new Date().toISOString(),
        totalDurationMs: finalElapsed,
        laps,
      });
      await queryClient.invalidateQueries({ queryKey: ["stopwatchSessions"] });
      toast.success("Session saved.");
      accumulatedRef.current = 0;
      startedAtRef.current = null;
      setElapsedMs(0);
      setLaps([]);
    } catch (err) {
      toast.error(toApiError(err).message);
    } finally {
      setSaving(false);
    }
  };

  const isActive = running || elapsedMs > 0;

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Stopwatch</h1>
        <p className="text-sm text-muted-foreground">Time anything, record laps, and save completed sessions to your history.</p>
      </div>

      <Card>
        <CardContent className="flex flex-col items-center gap-6 pt-6">
          <p className="font-mono text-6xl font-bold tabular-nums">{formatElapsed(elapsedMs)}</p>

          {!isActive && (
            <div className="flex w-full flex-col gap-1.5">
              <Label className="text-xs">Session label</Label>
              <Input value={label} onChange={(e) => setLabel(e.target.value)} />
            </div>
          )}

          <div className="flex gap-2">
            {running ? (
              <>
                <Button variant="outline" onClick={onPause}>
                  <Pause /> Pause
                </Button>
                <Button variant="outline" onClick={onLap}>
                  <Flag /> Lap
                </Button>
              </>
            ) : (
              <Button onClick={onStart}>
                <Play /> {elapsedMs > 0 ? "Resume" : "Start"}
              </Button>
            )}
            {isActive && (
              <Button onClick={onFinish} disabled={saving}>
                {saving ? <Loader2 className="animate-spin" /> : <Save />}
                Finish &amp; save
              </Button>
            )}
          </div>

          {laps.length > 0 && (
            <div className="w-full border-t pt-4">
              <div className="flex flex-col-reverse gap-1 text-sm">
                {laps.map((lap) => (
                  <div key={lap.lapNumber} className="flex items-center justify-between font-mono">
                    <span className="text-muted-foreground">Lap {lap.lapNumber}</span>
                    <span>{formatElapsed(lap.lapDurationMs)}</span>
                    <span className="text-muted-foreground">{formatElapsed(lap.cumulativeDurationMs)}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {history && history.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>History</CardTitle>
          </CardHeader>
          <CardContent className="flex flex-col gap-2">
            {history.map((s) => (
              <div key={s.id} className="flex items-center justify-between rounded-md border p-2.5 text-sm">
                <div>
                  <p className="font-medium">{s.label}</p>
                  <p className="text-xs text-muted-foreground">{new Date(s.startedAt).toLocaleString()}</p>
                </div>
                <span className="font-mono">{formatElapsed(s.totalDurationMs)}</span>
              </div>
            ))}
          </CardContent>
        </Card>
      )}
    </div>
  );
}
