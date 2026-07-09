export interface ProductivityComponents {
  habitsPercent: number | null;
  medicinesPercent: number | null;
  sleepScore: number | null;
  focusMinutes: number;
  prayersPercent: number | null;
}

export interface ProductivityDay {
  date: string;
  score: number | null;
  components: ProductivityComponents;
}

export interface ProductivitySummary {
  days: ProductivityDay[];
  averageScore: number | null;
  currentStreak: number;
  bestStreak: number;
  totalFocusMinutes: number;
  totalHabitCheckIns: number;
  totalPrayersLogged: number;
}
