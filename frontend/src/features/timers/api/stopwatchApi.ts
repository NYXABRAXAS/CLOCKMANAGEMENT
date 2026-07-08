import { apiClient } from "@/shared/lib/apiClient";
import type { StopwatchLap, StopwatchSession } from "@/types/timers";

export const stopwatchApi = {
  list: () => apiClient.get<StopwatchSession[]>("/stopwatch/sessions").then((r) => r.data),
  save: (data: { label: string; startedAt: string; endedAt: string; totalDurationMs: number; laps: StopwatchLap[] }) =>
    apiClient.post<StopwatchSession>("/stopwatch/sessions", data).then((r) => r.data),
};
