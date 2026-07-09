export interface PrayerTimes {
  fajr: string;
  sunrise: string;
  dhuhr: string;
  asr: string;
  maghrib: string;
  isha: string;
  hijriDay: string;
  hijriMonth: string;
  hijriYear: string;
  qiblaDirectionDegrees: number;
}

export interface PrayerLog {
  date: string;
  prayerName: string;
  completed: boolean;
}

export interface PanchangResult {
  tithiNumber: number;
  tithiName: string;
  paksha: string;
  nakshatraNumber: number;
  nakshatraName: string;
  isApproximate: boolean;
}

export interface HebrewDate {
  hebrewYear: number;
  hebrewMonth: string;
  hebrewDay: number;
  formatted: string;
  events: string[];
}

export interface Festival {
  id: string;
  religionCode: string;
  religionName: string;
  name: string;
  description: string | null;
  date: string;
  emoji: string | null;
  daysAway: number;
}

export interface DailyQuote {
  text: string;
  source: string | null;
}
