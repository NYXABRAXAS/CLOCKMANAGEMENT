import * as React from "react";
import { toast } from "sonner";
import { Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { cn } from "@/lib/utils";
import { DAY_BITS } from "@/shared/lib/alarmDays";
import { ALARM_SOUNDS } from "@/shared/lib/alarmSound";
import { toApiError } from "@/shared/lib/apiClient";
import type { Alarm, AlarmChallengeType, AlarmFormValues } from "@/types/alarm";

function defaultValues(alarm: Alarm | null): AlarmFormValues {
  if (alarm) {
    return {
      label: alarm.label,
      hour: alarm.hour,
      minute: alarm.minute,
      repeatDaysMask: alarm.repeatDaysMask,
      soundId: alarm.soundId,
      snoozeEnabled: alarm.snoozeEnabled,
      snoozeMinutes: alarm.snoozeMinutes,
      challengeType: alarm.challengeType,
    };
  }
  const now = new Date();
  return {
    label: "Alarm",
    hour: now.getHours(),
    minute: now.getMinutes(),
    repeatDaysMask: 0,
    soundId: "classic",
    snoozeEnabled: true,
    snoozeMinutes: 9,
    challengeType: "None",
  };
}

export function AlarmFormDialog({
  open,
  onOpenChange,
  alarm,
  onSave,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  alarm: Alarm | null;
  onSave: (id: string | null, values: AlarmFormValues) => Promise<void>;
}) {
  const [values, setValues] = React.useState<AlarmFormValues>(() => defaultValues(alarm));
  const [submitting, setSubmitting] = React.useState(false);

  React.useEffect(() => {
    if (open) setValues(defaultValues(alarm));
  }, [open, alarm]);

  const timeValue = `${String(values.hour).padStart(2, "0")}:${String(values.minute).padStart(2, "0")}`;

  const toggleDay = (bit: number) => {
    setValues((v) => ({ ...v, repeatDaysMask: v.repeatDaysMask ^ bit }));
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      await onSave(alarm?.id ?? null, values);
      onOpenChange(false);
    } catch (err) {
      toast.error(toApiError(err).message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{alarm ? "Edit alarm" : "New alarm"}</DialogTitle>
        </DialogHeader>
        <form onSubmit={onSubmit} className="flex flex-col gap-4">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="alarm-label">Label</Label>
            <Input id="alarm-label" value={values.label} onChange={(e) => setValues((v) => ({ ...v, label: e.target.value }))} required />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="alarm-time">Time</Label>
            <input
              id="alarm-time"
              type="time"
              value={timeValue}
              onChange={(e) => {
                const [h, m] = e.target.value.split(":").map(Number);
                setValues((v) => ({ ...v, hour: h, minute: m }));
              }}
              className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
              required
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label>Repeat</Label>
            <div className="flex flex-wrap gap-1.5">
              {DAY_BITS.map((d) => (
                <button
                  key={d.bit}
                  type="button"
                  onClick={() => toggleDay(d.bit)}
                  className={cn(
                    "size-9 rounded-full border text-xs font-medium transition-colors",
                    (values.repeatDaysMask & d.bit) !== 0
                      ? "border-primary bg-primary text-primary-foreground"
                      : "border-input text-muted-foreground hover:bg-accent",
                  )}
                >
                  {d.short.slice(0, 2)}
                </button>
              ))}
            </div>
            <p className="text-xs text-muted-foreground">{values.repeatDaysMask === 0 ? "One-time - fires once, then turns off." : "Repeats weekly."}</p>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="flex flex-col gap-1.5">
              <Label>Sound</Label>
              <Select value={values.soundId} onValueChange={(v) => setValues((s) => ({ ...s, soundId: v }))}>
                <SelectTrigger>
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
            <div className="flex flex-col gap-1.5">
              <Label>Dismiss challenge</Label>
              <Select value={values.challengeType} onValueChange={(v) => setValues((s) => ({ ...s, challengeType: v as AlarmChallengeType }))}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="None">None</SelectItem>
                  <SelectItem value="Math">Solve a math problem</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="flex items-center justify-between rounded-lg border p-3">
            <div>
              <p className="text-sm font-medium">Snooze</p>
              <p className="text-xs text-muted-foreground">Allow snoozing this alarm.</p>
            </div>
            <Switch checked={values.snoozeEnabled} onCheckedChange={(checked) => setValues((v) => ({ ...v, snoozeEnabled: checked }))} />
          </div>

          {values.snoozeEnabled && (
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="snooze-minutes">Snooze duration (minutes)</Label>
              <Input
                id="snooze-minutes"
                type="number"
                min={1}
                max={30}
                value={values.snoozeMinutes}
                onChange={(e) => setValues((v) => ({ ...v, snoozeMinutes: Number(e.target.value) }))}
              />
            </div>
          )}

          <DialogFooter>
            <Button type="submit" disabled={submitting}>
              {submitting && <Loader2 className="animate-spin" />}
              {alarm ? "Save changes" : "Create alarm"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
