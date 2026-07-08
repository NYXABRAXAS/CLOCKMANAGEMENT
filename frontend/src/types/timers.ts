export interface CountdownTimerPreset {
  id: string;
  label: string;
  durationSeconds: number;
  soundId: string;
}

export interface StopwatchLap {
  lapNumber: number;
  lapDurationMs: number;
  cumulativeDurationMs: number;
}

export interface StopwatchSession {
  id: string;
  label: string;
  startedAt: string;
  endedAt: string;
  totalDurationMs: number;
  laps: StopwatchLap[];
}

export type PomodoroPhase = "Work" | "ShortBreak" | "LongBreak";

export interface PomodoroLog {
  id: string;
  phase: PomodoroPhase;
  startedAt: string;
  endedAt: string;
  completedFully: boolean;
}

export interface PomodoroSession {
  id: string;
  workMinutes: number;
  shortBreakMinutes: number;
  longBreakMinutes: number;
  cyclesBeforeLongBreak: number;
  startedAt: string;
  endedAt: string | null;
  logs: PomodoroLog[];
}
