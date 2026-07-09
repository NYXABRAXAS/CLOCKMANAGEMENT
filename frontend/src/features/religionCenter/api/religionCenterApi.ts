import { apiClient } from "@/shared/lib/apiClient";
import type { DailyQuote, Festival, HebrewDate, PanchangResult, PrayerLog, PrayerTimes } from "@/types/religionCenter";

function dateStr(d: Date) {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}

export const religionCenterApi = {
  getPrayerTimes: (date: Date) => apiClient.get<PrayerTimes>("/prayer-times", { params: { date: dateStr(date) } }).then((r) => r.data),
  getPrayerLogs: (date: Date) => apiClient.get<PrayerLog[]>("/prayer-times/logs", { params: { date: dateStr(date) } }).then((r) => r.data),
  logPrayer: (date: Date, prayerName: string, completed: boolean) =>
    apiClient.post("/prayer-times/log", { date: dateStr(date), prayerName, completed }).then((r) => r.data),

  getPanchang: (date: Date) => apiClient.get<PanchangResult>("/panchang", { params: { date: dateStr(date) } }).then((r) => r.data),

  getHebrewDate: (date: Date) => apiClient.get<HebrewDate>("/hebrew-calendar", { params: { date: dateStr(date) } }).then((r) => r.data),

  getFestivals: (daysAhead = 90, religionId?: string) =>
    apiClient.get<Festival[]>("/festivals", { params: { daysAhead, religionId } }).then((r) => r.data),

  getDailyQuote: () => apiClient.get<DailyQuote | null>("/quotes/today").then((r) => r.data),
};
