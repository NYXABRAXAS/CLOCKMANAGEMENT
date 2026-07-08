import { apiClient } from "@/shared/lib/apiClient";
import type { EventCountdown } from "@/types/calendar";

export const eventCountdownsApi = {
  list: () => apiClient.get<EventCountdown[]>("/event-countdowns").then((r) => r.data),
  create: (data: { title: string; targetDate: string; emoji: string | null; color: string | null }) =>
    apiClient.post<EventCountdown>("/event-countdowns", data).then((r) => r.data),
  remove: (id: string) => apiClient.delete(`/event-countdowns/${id}`).then((r) => r.data),
};
