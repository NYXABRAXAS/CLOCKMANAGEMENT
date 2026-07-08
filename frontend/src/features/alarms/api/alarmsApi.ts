import { apiClient } from "@/shared/lib/apiClient";
import type { Alarm, AlarmFormValues } from "@/types/alarm";

export const alarmsApi = {
  list: () => apiClient.get<Alarm[]>("/alarms").then((r) => r.data),
  create: (data: AlarmFormValues) => apiClient.post<Alarm>("/alarms", data).then((r) => r.data),
  update: (id: string, data: AlarmFormValues & { isEnabled: boolean }) => apiClient.put<Alarm>(`/alarms/${id}`, data).then((r) => r.data),
  toggle: (id: string, isEnabled: boolean) => apiClient.post<Alarm>(`/alarms/${id}/toggle`, { isEnabled }).then((r) => r.data),
  remove: (id: string) => apiClient.delete(`/alarms/${id}`).then((r) => r.data),
  snooze: (id: string) => apiClient.post(`/alarms/${id}/snooze`).then((r) => r.data),
  dismiss: (id: string) => apiClient.post(`/alarms/${id}/dismiss`).then((r) => r.data),
};
