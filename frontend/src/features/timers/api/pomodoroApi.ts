import { apiClient } from "@/shared/lib/apiClient";
import type { PomodoroPhase, PomodoroSession } from "@/types/timers";

export const pomodoroApi = {
  list: () => apiClient.get<PomodoroSession[]>("/pomodoro/sessions").then((r) => r.data),
  start: (data: { workMinutes: number; shortBreakMinutes: number; longBreakMinutes: number; cyclesBeforeLongBreak: number }) =>
    apiClient.post<PomodoroSession>("/pomodoro/sessions", data).then((r) => r.data),
  logPhase: (sessionId: string, data: { phase: PomodoroPhase; startedAt: string; endedAt: string; completedFully: boolean }) =>
    apiClient.post(`/pomodoro/sessions/${sessionId}/phases`, data).then((r) => r.data),
  end: (sessionId: string) => apiClient.post(`/pomodoro/sessions/${sessionId}/end`).then((r) => r.data),
};
