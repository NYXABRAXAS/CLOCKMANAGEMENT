import * as React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { ChevronLeft, ChevronRight, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { calendarEventsApi, type CalendarEventPayload } from "../api/calendarEventsApi";
import { EventFormDialog } from "../components/EventFormDialog";
import { DayEventsDialog } from "../components/DayEventsDialog";
import { CountdownsPanel } from "../components/CountdownsPanel";
import { toApiError } from "@/shared/lib/apiClient";
import type { CalendarEvent, CalendarEventOccurrence } from "@/types/calendar";

const WEEKDAY_LABELS = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

function isSameDay(a: Date, b: Date) {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
}

function buildMonthGrid(monthStart: Date): Date[] {
  const gridStart = new Date(monthStart);
  gridStart.setDate(gridStart.getDate() - gridStart.getDay());
  return Array.from({ length: 42 }, (_, i) => {
    const d = new Date(gridStart);
    d.setDate(gridStart.getDate() + i);
    return d;
  });
}

export default function CalendarPage() {
  const queryClient = useQueryClient();
  const [monthStart, setMonthStart] = React.useState(() => {
    const d = new Date();
    d.setDate(1);
    d.setHours(0, 0, 0, 0);
    return d;
  });
  const [selectedDay, setSelectedDay] = React.useState<Date | null>(null);
  const [dayDialogOpen, setDayDialogOpen] = React.useState(false);
  const [eventDialogOpen, setEventDialogOpen] = React.useState(false);
  const [editingEvent, setEditingEvent] = React.useState<CalendarEvent | null>(null);

  const grid = React.useMemo(() => buildMonthGrid(monthStart), [monthStart]);
  const rangeStart = grid[0];
  const rangeEnd = React.useMemo(() => {
    const d = new Date(grid[41]);
    d.setDate(d.getDate() + 1);
    return d;
  }, [grid]);

  const { data: occurrences } = useQuery({
    queryKey: ["calendarEvents", rangeStart.toISOString(), rangeEnd.toISOString()],
    queryFn: () => calendarEventsApi.list(rangeStart, rangeEnd),
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["calendarEvents"] });

  const occurrencesForDay = (day: Date) => (occurrences ?? []).filter((o) => isSameDay(new Date(o.occurrenceStart), day));

  const onPrevMonth = () => setMonthStart((d) => new Date(d.getFullYear(), d.getMonth() - 1, 1));
  const onNextMonth = () => setMonthStart((d) => new Date(d.getFullYear(), d.getMonth() + 1, 1));
  const onToday = () => {
    const d = new Date();
    d.setDate(1);
    d.setHours(0, 0, 0, 0);
    setMonthStart(d);
  };

  const openDay = (day: Date) => {
    setSelectedDay(day);
    setDayDialogOpen(true);
  };

  const openNewEvent = () => {
    setEditingEvent(null);
    setDayDialogOpen(false);
    setEventDialogOpen(true);
  };

  const openEditEvent = (occurrence: CalendarEventOccurrence) => {
    setEditingEvent(occurrence.event);
    setDayDialogOpen(false);
    setEventDialogOpen(true);
  };

  const onSaveEvent = async (id: string | null, payload: CalendarEventPayload) => {
    if (id) {
      await calendarEventsApi.update(id, payload);
      toast.success("Event updated.");
    } else {
      await calendarEventsApi.create(payload);
      toast.success("Event created.");
    }
    await invalidate();
  };

  const onDeleteEvent = async (occurrence: CalendarEventOccurrence) => {
    try {
      await calendarEventsApi.remove(occurrence.event.id);
      await invalidate();
      toast.success("Event deleted.");
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const today = new Date();

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6 lg:flex-row">
      <div className="flex-1">
        <div className="mb-4 flex items-center justify-between">
          <h1 className="text-2xl font-semibold">{monthStart.toLocaleDateString(undefined, { month: "long", year: "numeric" })}</h1>
          <div className="flex items-center gap-2">
            <Button variant="outline" size="sm" onClick={onToday}>
              Today
            </Button>
            <Button variant="outline" size="icon" onClick={onPrevMonth}>
              <ChevronLeft className="size-4" />
            </Button>
            <Button variant="outline" size="icon" onClick={onNextMonth}>
              <ChevronRight className="size-4" />
            </Button>
            <Button onClick={openNewEvent}>
              <Plus /> Add event
            </Button>
          </div>
        </div>

        <div className="grid grid-cols-7 overflow-hidden rounded-lg border">
          {WEEKDAY_LABELS.map((label) => (
            <div key={label} className="border-b bg-muted/40 p-2 text-center text-xs font-medium text-muted-foreground">
              {label}
            </div>
          ))}
          {grid.map((day, idx) => {
            const dayOccurrences = occurrencesForDay(day);
            const inMonth = day.getMonth() === monthStart.getMonth();
            const isToday = isSameDay(day, today);
            return (
              <button
                key={idx}
                type="button"
                onClick={() => openDay(day)}
                className={cn(
                  "flex min-h-24 flex-col items-start gap-1 border-b border-r p-1.5 text-left align-top",
                  !inMonth && "bg-muted/20 text-muted-foreground",
                )}
              >
                <span className={cn("flex size-6 items-center justify-center rounded-full text-xs", isToday && "bg-primary text-primary-foreground")}>
                  {day.getDate()}
                </span>
                <div className="flex w-full flex-col gap-0.5 overflow-hidden">
                  {dayOccurrences.slice(0, 3).map((o, i) => (
                    <span
                      key={i}
                      className="truncate rounded px-1 py-0.5 text-[10px] font-medium text-white"
                      style={{ backgroundColor: o.event.color ?? "#6366f1" }}
                    >
                      {o.event.title}
                    </span>
                  ))}
                  {dayOccurrences.length > 3 && <span className="text-[10px] text-muted-foreground">+{dayOccurrences.length - 3} more</span>}
                </div>
              </button>
            );
          })}
        </div>
      </div>

      <div className="w-full lg:w-72">
        <CountdownsPanel />
      </div>

      <DayEventsDialog
        open={dayDialogOpen}
        onOpenChange={setDayDialogOpen}
        date={selectedDay}
        occurrences={selectedDay ? occurrencesForDay(selectedDay) : []}
        onAdd={openNewEvent}
        onEdit={openEditEvent}
        onDelete={onDeleteEvent}
      />

      <EventFormDialog
        open={eventDialogOpen}
        onOpenChange={setEventDialogOpen}
        event={editingEvent}
        defaultDate={selectedDay}
        onSave={onSaveEvent}
      />
    </div>
  );
}
