import { apiClient } from "@/shared/lib/apiClient";
import type { Religion } from "@/types/religion";

export const religionsApi = {
  getReligions: () => apiClient.get<Religion[]>("/religions").then((r) => r.data),
};
