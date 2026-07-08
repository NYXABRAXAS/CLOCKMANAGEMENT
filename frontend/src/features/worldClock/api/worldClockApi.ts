import { apiClient } from "@/shared/lib/apiClient";
import type { WorldClockCity } from "@/types/city";

export const worldClockApi = {
  getPinnedCities: () => apiClient.get<WorldClockCity[]>("/world-clock").then((r) => r.data),
  addCity: (cityId: string) => apiClient.post<{ id: string }>("/world-clock", { cityId }).then((r) => r.data),
  removeCity: (worldClockCityId: string) => apiClient.delete(`/world-clock/${worldClockCityId}`).then((r) => r.data),
  reorder: (orderedIds: string[]) => apiClient.put("/world-clock/reorder", { orderedIds }).then((r) => r.data),
};
