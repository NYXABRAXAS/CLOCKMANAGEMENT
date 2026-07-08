export interface MedicineTime {
  hour: number;
  minute: number;
}

export interface Medicine {
  id: string;
  name: string;
  dosage: string | null;
  notes: string | null;
  startDate: string;
  endDate: string | null;
  repeatDaysMask: number;
  isActive: boolean;
  times: MedicineTime[];
}

export type MedicineLogStatus = "Taken" | "Skipped";

export interface MedicineLog {
  medicineId: string;
  scheduledDate: string;
  scheduledHour: number;
  scheduledMinute: number;
  status: MedicineLogStatus;
}

export interface Habit {
  id: string;
  title: string;
  description: string | null;
  emoji: string | null;
  color: string | null;
  repeatDaysMask: number;
  isActive: boolean;
  currentStreak: number;
  longestStreak: number;
  completedToday: boolean;
}

export interface ToggleHabitLogResult {
  habit: Habit;
  newlyEarnedAchievementCodes: string[];
}

export type SleepQuality = "Poor" | "Fair" | "Good" | "Excellent";

export interface SleepLog {
  id: string;
  date: string;
  bedTime: string;
  wakeTime: string;
  durationMinutes: number;
  quality: SleepQuality | null;
  notes: string | null;
}

export interface UserAchievement {
  code: string;
  title: string;
  description: string | null;
  emoji: string | null;
  earnedAt: string;
}
