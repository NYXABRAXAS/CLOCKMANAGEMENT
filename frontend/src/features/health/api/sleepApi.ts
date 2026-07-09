import { apiClient } from "@/shared/lib/apiClient";
import { fetchAndDownload } from "@/shared/lib/downloadBlob";
import type { SleepLog, SleepQuality } from "@/types/health";

export const sleepApi = {
  list: () => apiClient.get<SleepLog[]>("/sleep-logs").then((r) => r.data),
  save: (data: { date: string; bedTime: string; wakeTime: string; quality: SleepQuality | null; notes: string | null }) =>
    apiClient.post<SleepLog>("/sleep-logs", data).then((r) => r.data),
  remove: (id: string) => apiClient.delete(`/sleep-logs/${id}`).then((r) => r.data),
  exportCsv: () => fetchAndDownload(apiClient, "/sleep-logs/export", "sleep-logs.csv"),
};
