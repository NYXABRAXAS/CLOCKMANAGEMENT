import { apiClient } from "@/shared/lib/apiClient";
import { fetchAndDownload } from "@/shared/lib/downloadBlob";
import type { Habit, ToggleHabitLogResult } from "@/types/health";

export interface HabitPayload {
  title: string;
  description: string | null;
  emoji: string | null;
  color: string | null;
  repeatDaysMask: number;
}

export const habitsApi = {
  list: () => apiClient.get<Habit[]>("/habits").then((r) => r.data),
  create: (data: HabitPayload) => apiClient.post<Habit>("/habits", data).then((r) => r.data),
  update: (id: string, data: HabitPayload & { isActive: boolean }) => apiClient.put<Habit>(`/habits/${id}`, data).then((r) => r.data),
  toggleLog: (id: string, date: string, completed: boolean) =>
    apiClient.post<ToggleHabitLogResult>(`/habits/${id}/log`, { date, completed }).then((r) => r.data),
  remove: (id: string) => apiClient.delete(`/habits/${id}`).then((r) => r.data),
  exportCsv: () => fetchAndDownload(apiClient, "/habits/export", "habit-logs.csv"),
};
