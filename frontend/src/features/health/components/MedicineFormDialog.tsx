import * as React from "react";
import { toast } from "sonner";
import { Loader2, Plus, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";
import { DAY_BITS, EVERYDAY_MASK } from "@/shared/lib/alarmDays";
import { toApiError } from "@/shared/lib/apiClient";
import type { Medicine, MedicineTime } from "@/types/health";
import type { MedicinePayload } from "../api/medicinesApi";

function todayIso() {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}

function defaultValues(medicine: Medicine | null) {
  return {
    name: medicine?.name ?? "",
    dosage: medicine?.dosage ?? "",
    notes: medicine?.notes ?? "",
    startDate: medicine?.startDate ?? todayIso(),
    endDate: medicine?.endDate ?? "",
    repeatDaysMask: medicine?.repeatDaysMask ?? EVERYDAY_MASK,
    times: medicine?.times.length ? medicine.times : [{ hour: 8, minute: 0 }],
  };
}

export function MedicineFormDialog({
  open,
  onOpenChange,
  medicine,
  onSave,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  medicine: Medicine | null;
  onSave: (id: string | null, payload: MedicinePayload) => Promise<void>;
}) {
  const [values, setValues] = React.useState(() => defaultValues(medicine));
  const [submitting, setSubmitting] = React.useState(false);

  React.useEffect(() => {
    if (open) setValues(defaultValues(medicine));
  }, [open, medicine]);

  const toggleDay = (bit: number) => setValues((v) => ({ ...v, repeatDaysMask: v.repeatDaysMask ^ bit }));

  const updateTime = (index: number, field: keyof MedicineTime, value: number) =>
    setValues((v) => ({ ...v, times: v.times.map((t, i) => (i === index ? { ...t, [field]: value } : t)) }));

  const addTime = () => setValues((v) => ({ ...v, times: [...v.times, { hour: 12, minute: 0 }] }));
  const removeTime = (index: number) => setValues((v) => ({ ...v, times: v.times.filter((_, i) => i !== index) }));

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      await onSave(medicine?.id ?? null, {
        name: values.name,
        dosage: values.dosage || null,
        notes: values.notes || null,
        startDate: values.startDate,
        endDate: values.endDate || null,
        repeatDaysMask: values.repeatDaysMask,
        times: values.times,
      });
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
          <DialogTitle>{medicine ? "Edit medicine" : "New medicine"}</DialogTitle>
        </DialogHeader>
        <form onSubmit={onSubmit} className="flex max-h-[70vh] flex-col gap-4 overflow-y-auto pr-1">
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="med-name">Name</Label>
              <Input id="med-name" value={values.name} onChange={(e) => setValues((v) => ({ ...v, name: e.target.value }))} required />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="med-dosage">Dosage</Label>
              <Input id="med-dosage" placeholder="e.g. 500mg" value={values.dosage} onChange={(e) => setValues((v) => ({ ...v, dosage: e.target.value }))} />
            </div>
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="med-notes">Notes</Label>
            <Input id="med-notes" value={values.notes} onChange={(e) => setValues((v) => ({ ...v, notes: e.target.value }))} />
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="med-start">Start date</Label>
              <Input id="med-start" type="date" value={values.startDate} onChange={(e) => setValues((v) => ({ ...v, startDate: e.target.value }))} required />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="med-end">End date (optional)</Label>
              <Input id="med-end" type="date" value={values.endDate} onChange={(e) => setValues((v) => ({ ...v, endDate: e.target.value }))} />
            </div>
          </div>

          <div className="flex flex-col gap-1.5">
            <Label>Reminder times</Label>
            {values.times.map((t, i) => (
              <div key={i} className="flex items-center gap-2">
                <input
                  type="time"
                  value={`${String(t.hour).padStart(2, "0")}:${String(t.minute).padStart(2, "0")}`}
                  onChange={(e) => {
                    const [h, m] = e.target.value.split(":").map(Number);
                    updateTime(i, "hour", h);
                    updateTime(i, "minute", m);
                  }}
                  className="flex h-9 rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
                />
                {values.times.length > 1 && (
                  <Button type="button" variant="ghost" size="icon" onClick={() => removeTime(i)}>
                    <X className="size-4" />
                  </Button>
                )}
              </div>
            ))}
            <Button type="button" variant="outline" size="sm" className="w-fit" onClick={addTime}>
              <Plus className="size-3.5" /> Add time
            </Button>
          </div>

          <div className="flex flex-col gap-1.5">
            <Label>Days</Label>
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
          </div>

          <DialogFooter>
            <Button type="submit" disabled={submitting}>
              {submitting && <Loader2 className="animate-spin" />}
              {medicine ? "Save changes" : "Create medicine"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
