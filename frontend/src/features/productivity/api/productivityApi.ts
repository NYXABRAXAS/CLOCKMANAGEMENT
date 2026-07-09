import { apiClient } from "@/shared/lib/apiClient";
import type { ProductivitySummary } from "@/types/productivity";

function dateStr(d: Date) {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}

export const productivityApi = {
  getSummary: (from: Date, to: Date) =>
    apiClient.get<ProductivitySummary>("/productivity/summary", { params: { from: dateStr(from), to: dateStr(to) } }).then((r) => r.data),
};
