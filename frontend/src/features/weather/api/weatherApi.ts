import { apiClient } from "@/shared/lib/apiClient";
import type { Weather } from "@/types/weather";

export const weatherApi = {
  getWeather: () => apiClient.get<Weather>("/weather").then((r) => r.data),
};
