import { apiClient } from "@/shared/lib/apiClient";
import type { UserProfile } from "@/types/auth";

export interface UpdateSettingsPayload {
  countryCode: string | null;
  timezoneId: string;
  timeFormat: string;
  language: string;
  theme: "light" | "dark" | "system";
  religionCode: string | null;
  prayerLatitude: number | null;
  prayerLongitude: number | null;
}

export const settingsApi = {
  updateSettings: (data: UpdateSettingsPayload) => apiClient.put<UserProfile>("/settings", data).then((r) => r.data),
};
