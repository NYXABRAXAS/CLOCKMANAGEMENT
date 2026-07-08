import { apiClient } from "@/shared/lib/apiClient";
import type { UserAchievement } from "@/types/health";

export const achievementsApi = {
  mine: () => apiClient.get<UserAchievement[]>("/achievements/mine").then((r) => r.data),
};
