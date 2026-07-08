import * as React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Loader2, Pencil, Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Switch } from "@/components/ui/switch";
import { useAppSelector } from "@/app/hooks";
import { alarmsApi } from "../api/alarmsApi";
import { AlarmFormDialog } from "../components/AlarmFormDialog";
import { describeRepeatMask } from "@/shared/lib/alarmDays";
import { toApiError } from "@/shared/lib/apiClient";
import type { Alarm, AlarmFormValues } from "@/types/alarm";

export default function AlarmsPage() {
  const hour12 = useAppSelector((s) => s.auth.user?.timeFormat) === "12h";
  const queryClient = useQueryClient();
  const { data: alarms, isLoading } = useQuery({ queryKey: ["alarms"], queryFn: alarmsApi.list });
  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [editingAlarm, setEditingAlarm] = React.useState<Alarm | null>(null);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["alarms"] });

  const openCreate = () => {
    setEditingAlarm(null);
    setDialogOpen(true);
  };

  const openEdit = (alarm: Alarm) => {
    setEditingAlarm(alarm);
    setDialogOpen(true);
  };

  const onSave = async (id: string | null, values: AlarmFormValues) => {
    if (id) {
      await alarmsApi.update(id, { ...values, isEnabled: alarms?.find((a) => a.id === id)?.isEnabled ?? true });
    } else {
      await alarmsApi.create(values);
    }
    await invalidate();
    toast.success(id ? "Alarm updated." : "Alarm created.");
  };

  const onToggle = async (alarm: Alarm, isEnabled: boolean) => {
    try {
      await alarmsApi.toggle(alarm.id, isEnabled);
      await invalidate();
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const onDelete = async (alarm: Alarm) => {
    try {
      await alarmsApi.remove(alarm.id);
      await invalidate();
      toast.success("Alarm deleted.");
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const formatTime = (hour: number, minute: number) => {
    const d = new Date();
    d.setHours(hour, minute, 0, 0);
    return new Intl.DateTimeFormat(undefined, { hour: "2-digit", minute: "2-digit", hour12 }).format(d);
  };

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Alarms</h1>
          <p className="text-sm text-muted-foreground">Alarms ring while this tab is open, with an optional snooze or dismiss challenge.</p>
        </div>
        <Button onClick={openCreate}>
          <Plus /> New alarm
        </Button>
      </div>

      {isLoading && (
        <div className="flex justify-center py-12">
          <Loader2 className="size-8 animate-spin text-muted-foreground" />
        </div>
      )}

      {!isLoading && alarms?.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center text-sm text-muted-foreground">
          No alarms yet. Click "New alarm" to create one.
        </div>
      )}

      <div className="flex flex-col gap-3">
        {alarms?.map((alarm) => (
          <Card key={alarm.id}>
            <CardContent className="flex items-center justify-between gap-4 pt-5">
              <div className="min-w-0">
                <p className="font-mono text-2xl font-semibold tabular-nums">{formatTime(alarm.hour, alarm.minute)}</p>
                <p className="truncate text-sm font-medium">{alarm.label}</p>
                <p className="text-xs text-muted-foreground">{describeRepeatMask(alarm.repeatDaysMask)}</p>
              </div>
              <div className="flex shrink-0 items-center gap-1">
                <Switch checked={alarm.isEnabled} onCheckedChange={(checked) => onToggle(alarm, checked)} />
                <Button variant="ghost" size="icon" onClick={() => openEdit(alarm)}>
                  <Pencil className="size-4" />
                </Button>
                <Button variant="ghost" size="icon" onClick={() => onDelete(alarm)}>
                  <Trash2 className="size-4" />
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <AlarmFormDialog open={dialogOpen} onOpenChange={setDialogOpen} alarm={editingAlarm} onSave={onSave} />
    </div>
  );
}
