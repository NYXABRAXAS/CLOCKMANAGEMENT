import { apiClient } from "@/shared/lib/apiClient";
import { fetchAndDownload } from "@/shared/lib/downloadBlob";
import type { Medicine, MedicineLog, MedicineLogStatus, MedicineTime } from "@/types/health";

export interface MedicinePayload {
  name: string;
  dosage: string | null;
  notes: string | null;
  startDate: string;
  endDate: string | null;
  repeatDaysMask: number;
  times: MedicineTime[];
}

export const medicinesApi = {
  list: () => apiClient.get<Medicine[]>("/medicines").then((r) => r.data),
  logsForDate: (date: string) => apiClient.get<MedicineLog[]>("/medicines/logs", { params: { date } }).then((r) => r.data),
  create: (data: MedicinePayload) => apiClient.post<Medicine>("/medicines", data).then((r) => r.data),
  update: (id: string, data: MedicinePayload & { isActive: boolean }) => apiClient.put<Medicine>(`/medicines/${id}`, data).then((r) => r.data),
  logDose: (id: string, data: { scheduledDate: string; scheduledHour: number; scheduledMinute: number; status: MedicineLogStatus }) =>
    apiClient.post(`/medicines/${id}/log`, data).then((r) => r.data),
  remove: (id: string) => apiClient.delete(`/medicines/${id}`).then((r) => r.data),
  exportCsv: () => fetchAndDownload(apiClient, "/medicines/export", "medicine-logs.csv"),
};
