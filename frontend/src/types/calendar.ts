export type RecurrenceFrequency = "None" | "Daily" | "Weekly" | "Monthly" | "Yearly";

export interface CalendarEvent {
  id: string;
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

export interface CalendarEventOccurrence {
  event: CalendarEvent;
  occurrenceStart: string;
  occurrenceEnd: string;
}

export interface EventCountdown {
  id: string;
  title: string;
  targetDate: string;
  emoji: string | null;
  color: string | null;
}

export interface CalendarEventFormValues {
  title: string;
  description: string;
  location: string;
  color: string;
  isAllDay: boolean;
  startAt: string;
  endAt: string;
  recurrenceFrequency: RecurrenceFrequency;
  recurrenceInterval: number;
  recurrenceDaysMask: number;
  recurrenceEndDate: string;
}
