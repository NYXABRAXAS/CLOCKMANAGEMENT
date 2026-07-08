import { Pencil, Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import type { CalendarEventOccurrence } from "@/types/calendar";

export function DayEventsDialog({
  open,
  onOpenChange,
  date,
  occurrences,
  onAdd,
  onEdit,
  onDelete,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  date: Date | null;
  occurrences: CalendarEventOccurrence[];
  onAdd: () => void;
  onEdit: (occurrence: CalendarEventOccurrence) => void;
  onDelete: (occurrence: CalendarEventOccurrence) => void;
}) {
  if (!date) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{date.toLocaleDateString(undefined, { weekday: "long", month: "long", day: "numeric", year: "numeric" })}</DialogTitle>
        </DialogHeader>
        <div className="flex flex-col gap-2">
          {occurrences.length === 0 && <p className="py-4 text-center text-sm text-muted-foreground">No events on this day.</p>}
          {occurrences.map((o, idx) => (
            <div key={`${o.event.id}-${idx}`} className="flex items-center justify-between rounded-md border p-2.5">
              <div className="flex items-center gap-2.5">
                <span className="size-2.5 shrink-0 rounded-full" style={{ backgroundColor: o.event.color ?? "#6366f1" }} />
                <div>
                  <p className="text-sm font-medium">{o.event.title}</p>
                  <p className="text-xs text-muted-foreground">
                    {o.event.isAllDay
                      ? "All day"
                      : `${new Date(o.occurrenceStart).toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit" })} - ${new Date(
                          o.occurrenceEnd,
                        ).toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit" })}`}
                    {o.event.location ? ` · ${o.event.location}` : ""}
                  </p>
                </div>
              </div>
              <div className="flex items-center gap-0.5">
                <Button variant="ghost" size="icon" className="size-7" onClick={() => onEdit(o)}>
                  <Pencil className="size-3.5" />
                </Button>
                <Button variant="ghost" size="icon" className="size-7" onClick={() => onDelete(o)}>
                  <Trash2 className="size-3.5" />
                </Button>
              </div>
            </div>
          ))}
          <Button variant="outline" className="mt-2" onClick={onAdd}>
            <Plus /> Add event
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
