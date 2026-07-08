import { apiClient } from "@/shared/lib/apiClient";
import type { CalendarEvent, CalendarEventOccurrence, RecurrenceFrequency } from "@/types/calendar";

export interface CalendarEventPayload {
  title: string;
  description: string | null;
  location: string | null;
  color: string | null;
  isAllDay: boolean;
  startAt: string;
  endAt: string;
  recurrenceFrequency: RecurrenceFrequency;
  recurrenceInterval: number;
  recurrenceDaysMask: number;
  recurrenceEndDate: string | null;
}

export const calendarEventsApi = {
  list: (rangeStart: Date, rangeEnd: Date) =>
    apiClient
      .get<CalendarEventOccurrence[]>("/calendar-events", {
        params: { rangeStart: rangeStart.toISOString(), rangeEnd: rangeEnd.toISOString() },
      })
      .then((r) => r.data),
  create: (data: CalendarEventPayload) => apiClient.post<CalendarEvent>("/calendar-events", data).then((r) => r.data),
  update: (id: string, data: CalendarEventPayload) => apiClient.put<CalendarEvent>(`/calendar-events/${id}`, data).then((r) => r.data),
  remove: (id: string) => apiClient.delete(`/calendar-events/${id}`).then((r) => r.data),
};
