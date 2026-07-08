import * as React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Check, Loader2, Pencil, Plus, Trash2, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { useAppSelector } from "@/app/hooks";
import { medicinesApi } from "../api/medicinesApi";
import { MedicineFormDialog } from "../components/MedicineFormDialog";
import { describeRepeatMask } from "@/shared/lib/alarmDays";
import { toApiError } from "@/shared/lib/apiClient";
import type { Medicine } from "@/types/health";

function todayIso() {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}

export default function MedicinesPage() {
  const hour12 = useAppSelector((s) => s.auth.user?.timeFormat) === "12h";
  const queryClient = useQueryClient();
  const today = todayIso();
  const { data: medicines, isLoading } = useQuery({ queryKey: ["medicines"], queryFn: medicinesApi.list });
  const { data: logs } = useQuery({ queryKey: ["medicineLogs", today], queryFn: () => medicinesApi.logsForDate(today) });
  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [editingMedicine, setEditingMedicine] = React.useState<Medicine | null>(null);

  const invalidate = () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: ["medicines"] }),
      queryClient.invalidateQueries({ queryKey: ["medicineLogs"] }),
    ]);

  const openCreate = () => {
    setEditingMedicine(null);
    setDialogOpen(true);
  };
  const openEdit = (m: Medicine) => {
    setEditingMedicine(m);
    setDialogOpen(true);
  };

  const onSave: React.ComponentProps<typeof MedicineFormDialog>["onSave"] = async (id, payload) => {
    if (id) {
      await medicinesApi.update(id, { ...payload, isActive: medicines?.find((m) => m.id === id)?.isActive ?? true });
      toast.success("Medicine updated.");
    } else {
      await medicinesApi.create(payload);
      toast.success("Medicine added.");
    }
    await invalidate();
  };

  const onLogDose = async (medicineId: string, hour: number, minute: number, status: "Taken" | "Skipped") => {
    try {
      await medicinesApi.logDose(medicineId, { scheduledDate: today, scheduledHour: hour, scheduledMinute: minute, status });
      await invalidate();
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const onDelete = async (m: Medicine) => {
    try {
      await medicinesApi.remove(m.id);
      await invalidate();
      toast.success("Medicine removed.");
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const formatTime = (hour: number, minute: number) => {
    const d = new Date();
    d.setHours(hour, minute, 0, 0);
    return new Intl.DateTimeFormat(undefined, { hour: "2-digit", minute: "2-digit", hour12 }).format(d);
  };

  const logStatus = (medicineId: string, hour: number, minute: number) =>
    logs?.find((l) => l.medicineId === medicineId && l.scheduledHour === hour && l.scheduledMinute === minute)?.status;

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Medicines</h1>
          <p className="text-sm text-muted-foreground">Track today's doses. Reminders show here, not as alarms yet.</p>
        </div>
        <Button onClick={openCreate}>
          <Plus /> New medicine
        </Button>
      </div>

      {isLoading && (
        <div className="flex justify-center py-12">
          <Loader2 className="size-8 animate-spin text-muted-foreground" />
        </div>
      )}

      {!isLoading && medicines?.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center text-sm text-muted-foreground">
          No medicines yet. Click "New medicine" to add one.
        </div>
      )}

      <div className="flex flex-col gap-3">
        {medicines?.map((m) => (
          <Card key={m.id}>
            <CardContent className="flex flex-col gap-3 pt-5">
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium">
                    {m.name} {m.dosage && <span className="text-sm text-muted-foreground">({m.dosage})</span>}
                  </p>
                  <p className="text-xs text-muted-foreground">{describeRepeatMask(m.repeatDaysMask)}</p>
                </div>
                <div className="flex items-center gap-0.5">
                  <Button variant="ghost" size="icon" onClick={() => openEdit(m)}>
                    <Pencil className="size-4" />
                  </Button>
                  <Button variant="ghost" size="icon" onClick={() => onDelete(m)}>
                    <Trash2 className="size-4" />
                  </Button>
                </div>
              </div>
              <div className="flex flex-wrap gap-2">
                {m.times.map((t) => {
                  const status = logStatus(m.id, t.hour, t.minute);
                  return (
                    <div key={`${t.hour}:${t.minute}`} className="flex items-center gap-1 rounded-full border px-2 py-1 text-xs">
                      <span className="font-mono">{formatTime(t.hour, t.minute)}</span>
                      {status ? (
                        <span className={status === "Taken" ? "text-green-600" : "text-muted-foreground"}>{status}</span>
                      ) : (
                        <>
                          <button
                            type="button"
                            onClick={() => onLogDose(m.id, t.hour, t.minute, "Taken")}
                            className="flex size-5 items-center justify-center rounded-full text-green-600 hover:bg-green-500/10"
                          >
                            <Check className="size-3.5" />
                          </button>
                          <button
                            type="button"
                            onClick={() => onLogDose(m.id, t.hour, t.minute, "Skipped")}
                            className="flex size-5 items-center justify-center rounded-full text-muted-foreground hover:bg-accent"
                          >
                            <X className="size-3.5" />
                          </button>
                        </>
                      )}
                    </div>
                  );
                })}
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <MedicineFormDialog open={dialogOpen} onOpenChange={setDialogOpen} medicine={editingMedicine} onSave={onSave} />
    </div>
  );
}
