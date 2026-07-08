import { apiClient } from "@/shared/lib/apiClient";
import type { City } from "@/types/city";

export const citiesApi = {
  search: (search?: string) => apiClient.get<City[]>("/cities", { params: { search } }).then((r) => r.data),
};
