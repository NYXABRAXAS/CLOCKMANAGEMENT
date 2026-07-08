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
import { toApiError } from "@/shared/lib/apiClient";
import type { CalendarEvent, CalendarEventFormValues, RecurrenceFrequency } from "@/types/calendar";
import type { CalendarEventPayload } from "../api/calendarEventsApi";

const COLORS = ["#6366f1", "#ef4444", "#f59e0b", "#22c55e", "#06b6d4", "#ec4899"];

function toLocalInputValue(iso: string) {
  const d = new Date(iso);
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

function defaultValues(event: CalendarEvent | null, defaultDate: Date | null): CalendarEventFormValues {
  if (event) {
    return {
      title: event.title,
      description: event.description ?? "",
      location: event.location ?? "",
      color: event.color ?? COLORS[0],
      isAllDay: event.isAllDay,
      startAt: toLocalInputValue(event.startAt),
      endAt: toLocalInputValue(event.endAt),
      recurrenceFrequency: event.recurrenceFrequency,
      recurrenceInterval: event.recurrenceInterval,
      recurrenceDaysMask: event.recurrenceDaysMask,
      recurrenceEndDate: event.recurrenceEndDate ? event.recurrenceEndDate.slice(0, 10) : "",
    };
  }
  const base = defaultDate ?? new Date();
  base.setMinutes(0, 0, 0);
  const end = new Date(base.getTime() + 60 * 60 * 1000);
  return {
    title: "",
    description: "",
    location: "",
    color: COLORS[0],
    isAllDay: false,
    startAt: toLocalInputValue(base.toISOString()),
    endAt: toLocalInputValue(end.toISOString()),
    recurrenceFrequency: "None",
    recurrenceInterval: 1,
    recurrenceDaysMask: 0,
    recurrenceEndDate: "",
  };
}

export function EventFormDialog({
  open,
  onOpenChange,
  event,
  defaultDate,
  onSave,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  event: CalendarEvent | null;
  defaultDate: Date | null;
  onSave: (id: string | null, payload: CalendarEventPayload) => Promise<void>;
}) {
  const [values, setValues] = React.useState<CalendarEventFormValues>(() => defaultValues(event, defaultDate));
  const [submitting, setSubmitting] = React.useState(false);

  React.useEffect(() => {
    if (open) setValues(defaultValues(event, defaultDate));
  }, [open, event, defaultDate]);

  const toggleDay = (bit: number) => setValues((v) => ({ ...v, recurrenceDaysMask: v.recurrenceDaysMask ^ bit }));

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      await onSave(event?.id ?? null, {
        title: values.title,
        description: values.description || null,
        location: values.location || null,
        color: values.color || null,
        isAllDay: values.isAllDay,
        startAt: new Date(values.startAt).toISOString(),
        endAt: new Date(values.endAt).toISOString(),
        recurrenceFrequency: values.recurrenceFrequency,
        recurrenceInterval: values.recurrenceInterval,
        recurrenceDaysMask: values.recurrenceDaysMask,
        recurrenceEndDate: values.recurrenceEndDate ? new Date(values.recurrenceEndDate).toISOString() : null,
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
          <DialogTitle>{event ? "Edit event" : "New event"}</DialogTitle>
        </DialogHeader>
        <form onSubmit={onSubmit} className="flex max-h-[70vh] flex-col gap-4 overflow-y-auto pr-1">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="event-title">Title</Label>
            <Input id="event-title" value={values.title} onChange={(e) => setValues((v) => ({ ...v, title: e.target.value }))} required />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="event-desc">Description</Label>
            <textarea
              id="event-desc"
              value={values.description}
              onChange={(e) => setValues((v) => ({ ...v, description: e.target.value }))}
              className="flex min-h-16 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="event-location">Location</Label>
            <Input id="event-location" value={values.location} onChange={(e) => setValues((v) => ({ ...v, location: e.target.value }))} />
          </div>

          <div className="flex items-center justify-between rounded-lg border p-3">
            <Label className="text-sm font-medium">All-day</Label>
            <Switch checked={values.isAllDay} onCheckedChange={(checked) => setValues((v) => ({ ...v, isAllDay: checked }))} />
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="flex flex-col gap-1.5">
              <Label>Starts</Label>
              <input
                type="datetime-local"
                value={values.startAt}
                onChange={(e) => setValues((v) => ({ ...v, startAt: e.target.value }))}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
                required
              />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label>Ends</Label>
              <input
                type="datetime-local"
                value={values.endAt}
                onChange={(e) => setValues((v) => ({ ...v, endAt: e.target.value }))}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
                required
              />
            </div>
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
            <Label>Repeats</Label>
            <Select value={values.recurrenceFrequency} onValueChange={(v) => setValues((s) => ({ ...s, recurrenceFrequency: v as RecurrenceFrequency }))}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="None">Does not repeat</SelectItem>
                <SelectItem value="Daily">Daily</SelectItem>
                <SelectItem value="Weekly">Weekly</SelectItem>
                <SelectItem value="Monthly">Monthly</SelectItem>
                <SelectItem value="Yearly">Yearly</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {values.recurrenceFrequency !== "None" && (
            <>
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="flex flex-col gap-1.5">
                  <Label className="text-xs">Every</Label>
                  <Input
                    type="number"
                    min={1}
                    max={99}
                    value={values.recurrenceInterval}
                    onChange={(e) => setValues((v) => ({ ...v, recurrenceInterval: Number(e.target.value) }))}
                  />
                </div>
                <div className="flex flex-col gap-1.5">
                  <Label className="text-xs">Ends on (optional)</Label>
                  <Input
                    type="date"
                    value={values.recurrenceEndDate}
                    onChange={(e) => setValues((v) => ({ ...v, recurrenceEndDate: e.target.value }))}
                  />
                </div>
              </div>

              {values.recurrenceFrequency === "Weekly" && (
                <div className="flex flex-col gap-1.5">
                  <Label className="text-xs">On days</Label>
                  <div className="flex flex-wrap gap-1.5">
                    {DAY_BITS.map((d) => (
                      <button
                        key={d.bit}
                        type="button"
                        onClick={() => toggleDay(d.bit)}
                        className={cn(
                          "size-9 rounded-full border text-xs font-medium transition-colors",
                          (values.recurrenceDaysMask & d.bit) !== 0
                            ? "border-primary bg-primary text-primary-foreground"
                            : "border-input text-muted-foreground hover:bg-accent",
                        )}
                      >
                        {d.short.slice(0, 2)}
                      </button>
                    ))}
                  </div>
                </div>
              )}
            </>
          )}

          <DialogFooter>
            <Button type="submit" disabled={submitting}>
              {submitting && <Loader2 className="animate-spin" />}
              {event ? "Save changes" : "Create event"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
