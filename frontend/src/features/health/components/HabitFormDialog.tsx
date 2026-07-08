import * as React from "react";
import { toast } from "sonner";
import { Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";
import { DAY_BITS, EVERYDAY_MASK } from "@/shared/lib/alarmDays";
import { toApiError } from "@/shared/lib/apiClient";
import type { Habit } from "@/types/health";
import type { HabitPayload } from "../api/habitsApi";

function defaultValues(habit: Habit | null) {
  return {
    title: habit?.title ?? "",
    description: habit?.description ?? "",
    emoji: habit?.emoji ?? "✅",
    color: habit?.color ?? "#6366f1",
    repeatDaysMask: habit?.repeatDaysMask ?? EVERYDAY_MASK,
  };
}

const COLORS = ["#6366f1", "#ef4444", "#f59e0b", "#22c55e", "#06b6d4", "#ec4899"];

export function HabitFormDialog({
  open,
  onOpenChange,
  habit,
  onSave,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  habit: Habit | null;
  onSave: (id: string | null, payload: HabitPayload) => Promise<void>;
}) {
  const [values, setValues] = React.useState(() => defaultValues(habit));
  const [submitting, setSubmitting] = React.useState(false);

  React.useEffect(() => {
    if (open) setValues(defaultValues(habit));
  }, [open, habit]);

  const toggleDay = (bit: number) => setValues((v) => ({ ...v, repeatDaysMask: v.repeatDaysMask ^ bit }));

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      await onSave(habit?.id ?? null, {
        title: values.title,
        description: values.description || null,
        emoji: values.emoji || null,
        color: values.color || null,
        repeatDaysMask: values.repeatDaysMask,
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
          <DialogTitle>{habit ? "Edit habit" : "New habit"}</DialogTitle>
        </DialogHeader>
        <form onSubmit={onSubmit} className="flex flex-col gap-4">
          <div className="grid grid-cols-[1fr_auto] gap-3">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="habit-title">Title</Label>
              <Input id="habit-title" value={values.title} onChange={(e) => setValues((v) => ({ ...v, title: e.target.value }))} required />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="habit-emoji">Emoji</Label>
              <Input
                id="habit-emoji"
                value={values.emoji}
                onChange={(e) => setValues((v) => ({ ...v, emoji: e.target.value }))}
                className="w-16 text-center"
                maxLength={4}
              />
            </div>
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="habit-desc">Description</Label>
            <Input id="habit-desc" value={values.description} onChange={(e) => setValues((v) => ({ ...v, description: e.target.value }))} />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label>Color</Label>
            <div className="flex gap-2">
              {COLORS.map((c) => (
                <button
                  key={c}
                  type="button"
                  onClick={() => setValues((v) => ({ ...v, color: c }))}
                  className={cn("size-7 rounded-full border-2", values.color === c ? "border-foreground" : "border-transparent")}
                  style={{ backgroundColor: c }}
                />
              ))}
            </div>
          </div>

          <div className="flex flex-col gap-1.5">
            <Label>Repeat on</Label>
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
              {habit ? "Save changes" : "Create habit"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
