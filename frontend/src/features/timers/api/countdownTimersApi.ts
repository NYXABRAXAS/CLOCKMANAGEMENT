import { apiClient } from "@/shared/lib/apiClient";
import type { CountdownTimerPreset } from "@/types/timers";

export const countdownTimersApi = {
  list: () => apiClient.get<CountdownTimerPreset[]>("/countdown-timers").then((r) => r.data),
  create: (data: { label: string; durationSeconds: number; soundId: string }) =>
    apiClient.post<CountdownTimerPreset>("/countdown-timers", data).then((r) => r.data),
  remove: (id: string) => apiClient.delete(`/countdown-timers/${id}`).then((r) => r.data),
};
