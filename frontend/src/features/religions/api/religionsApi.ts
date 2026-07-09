import { apiClient } from "@/shared/lib/apiClient";
import type { Religion } from "@/types/religion";

export const religionsApi = {
  getReligions: () => apiClient.get<Religion[]>("/religions").then((r) => r.data),
  createReligion: (data: { code: string; name: string; sortOrder: number }) =>
    apiClient.post<Religion>("/religions", data).then((r) => r.data),
  updateReligion: (id: string, data: { name: string; sortOrder: number }) =>
    apiClient.put<Religion>(`/religions/${id}`, data).then((r) => r.data),
  deleteReligion: (id: string) => apiClient.delete(`/religions/${id}`).then((r) => r.data),
};
