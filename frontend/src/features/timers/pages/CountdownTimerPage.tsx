import * as React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Loader2, Pause, Play, Plus, RotateCcw, Save, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { countdownTimersApi } from "../api/countdownTimersApi";
import { ALARM_SOUNDS, startAlarmSound, stopAlarmSound } from "@/shared/lib/alarmSound";
import { toApiError } from "@/shared/lib/apiClient";

function pad(n: number) {
  return String(n).padStart(2, "0");
}

function formatHms(totalSeconds: number) {
  const h = Math.floor(totalSeconds / 3600);
  const m = Math.floor((totalSeconds % 3600) / 60);
  const s = totalSeconds % 60;
  return h > 0 ? `${pad(h)}:${pad(m)}:${pad(s)}` : `${pad(m)}:${pad(s)}`;
}

export default function CountdownTimerPage() {
  const queryClient = useQueryClient();
  const { data: presets } = useQuery({ queryKey: ["countdownTimers"], queryFn: countdownTimersApi.list });

  const [hours, setHours] = React.useState(0);
  const [minutes, setMinutes] = React.useState(5);
  const [seconds, setSeconds] = React.useState(0);
  const [soundId, setSoundId] = React.useState("classic");
  const [totalSeconds, setTotalSeconds] = React.useState(0);
  const [remaining, setRemaining] = React.useState(0);
  const [running, setRunning] = React.useState(false);
  const [finished, setFinished] = React.useState(false);
  const [saveLabel, setSaveLabel] = React.useState("");
  const [saving, setSaving] = React.useState(false);

  React.useEffect(() => {
    if (!running) return;
    const id = setInterval(() => {
      setRemaining((prev) => {
        if (prev <= 1) {
          clearInterval(id);
          setRunning(false);
          setFinished(true);
          startAlarmSound(soundId);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(id);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [running]);

  const configuredSeconds = hours * 3600 + minutes * 60 + seconds;

  const onStart = () => {
    if (finished) {
      stopAlarmSound();
      setFinished(false);
    }
    if (remaining === 0) {
      setTotalSeconds(configuredSeconds);
      setRemaining(configuredSeconds);
    }
    if (configuredSeconds === 0 && remaining === 0) return;
    setRunning(true);
  };

  const onPause = () => setRunning(false);

  const onReset = () => {
    stopAlarmSound();
    setRunning(false);
    setFinished(false);
    setRemaining(0);
    setTotalSeconds(0);
  };

  const onDismiss = () => {
    stopAlarmSound();
    setFinished(false);
  };

  const loadPreset = (presetId: string) => {
    const preset = presets?.find((p) => p.id === presetId);
    if (!preset) return;
    onReset();
    setHours(Math.floor(preset.durationSeconds / 3600));
    setMinutes(Math.floor((preset.durationSeconds % 3600) / 60));
    setSeconds(preset.durationSeconds % 60);
    setSoundId(preset.soundId);
  };

  const onSavePreset = async () => {
    if (!saveLabel.trim() || configuredSeconds === 0) return;
    setSaving(true);
    try {
      await countdownTimersApi.create({ label: saveLabel.trim(), durationSeconds: configuredSeconds, soundId });
      await queryClient.invalidateQueries({ queryKey: ["countdownTimers"] });
      setSaveLabel("");
      toast.success("Preset saved.");
    } catch (err) {
      toast.error(toApiError(err).message);
    } finally {
      setSaving(false);
    }
  };

  const onDeletePreset = async (id: string) => {
    try {
      await countdownTimersApi.remove(id);
      await queryClient.invalidateQueries({ queryKey: ["countdownTimers"] });
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const progress = totalSeconds > 0 ? ((totalSeconds - remaining) / totalSeconds) * 100 : 0;
  const isActive = running || remaining > 0;

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Countdown Timer</h1>
        <p className="text-sm text-muted-foreground">Set a duration, save it as a preset, and get an audible alert when it ends.</p>
      </div>

      <Card>
        <CardContent className="flex flex-col items-center gap-6 pt-6">
          <p className={`font-mono text-6xl font-bold tabular-nums ${finished ? "text-primary" : ""}`}>
            {formatHms(isActive ? remaining : configuredSeconds)}
          </p>

          {isActive && (
            <div className="h-2 w-full overflow-hidden rounded-full bg-muted">
              <div className="h-full bg-primary transition-all" style={{ width: `${progress}%` }} />
            </div>
          )}

          {finished && <p className="text-lg font-medium text-primary">Time's up!</p>}

          {!isActive && (
            <div className="flex items-end gap-2">
              <div className="flex flex-col gap-1">
                <Label className="text-xs">Hours</Label>
                <Input type="number" min={0} max={23} value={hours} onChange={(e) => setHours(Number(e.target.value))} className="w-20" />
              </div>
              <div className="flex flex-col gap-1">
                <Label className="text-xs">Minutes</Label>
                <Input type="number" min={0} max={59} value={minutes} onChange={(e) => setMinutes(Number(e.target.value))} className="w-20" />
              </div>
              <div className="flex flex-col gap-1">
                <Label className="text-xs">Seconds</Label>
                <Input type="number" min={0} max={59} value={seconds} onChange={(e) => setSeconds(Number(e.target.value))} className="w-20" />
              </div>
              <div className="flex flex-col gap-1">
                <Label className="text-xs">Sound</Label>
                <Select value={soundId} onValueChange={setSoundId}>
                  <SelectTrigger className="w-40">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {ALARM_SOUNDS.map((s) => (
                      <SelectItem key={s.id} value={s.id}>
                        {s.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
          )}

          <div className="flex gap-2">
            {finished ? (
              <Button onClick={onDismiss}>Dismiss</Button>
            ) : running ? (
              <Button variant="outline" onClick={onPause}>
                <Pause /> Pause
              </Button>
            ) : (
              <Button onClick={onStart} disabled={configuredSeconds === 0 && remaining === 0}>
                <Play /> {remaining > 0 ? "Resume" : "Start"}
              </Button>
            )}
            {isActive && (
              <Button variant="outline" onClick={onReset}>
                <RotateCcw /> Reset
              </Button>
            )}
          </div>

          {!isActive && configuredSeconds > 0 && (
            <div className="flex w-full items-end gap-2 border-t pt-4">
              <div className="flex flex-1 flex-col gap-1">
                <Label className="text-xs">Save as preset</Label>
                <Input placeholder="e.g. Tea time" value={saveLabel} onChange={(e) => setSaveLabel(e.target.value)} />
              </div>
              <Button variant="outline" onClick={onSavePreset} disabled={saving || !saveLabel.trim()}>
                {saving ? <Loader2 className="animate-spin" /> : <Save />}
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {presets && presets.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Saved presets</CardTitle>
          </CardHeader>
          <CardContent className="flex flex-col gap-2">
            {presets.map((p) => (
              <div key={p.id} className="flex items-center justify-between rounded-md border p-2.5">
                <button type="button" className="flex flex-1 items-center gap-3 text-left" onClick={() => loadPreset(p.id)}>
                  <Plus className="size-4 text-muted-foreground" />
                  <span className="font-medium">{p.label}</span>
                  <span className="font-mono text-sm text-muted-foreground">{formatHms(p.durationSeconds)}</span>
                </button>
                <Button variant="ghost" size="icon" onClick={() => onDeletePreset(p.id)}>
                  <Trash2 className="size-4" />
                </Button>
              </div>
            ))}
          </CardContent>
        </Card>
      )}
    </div>
  );
}
