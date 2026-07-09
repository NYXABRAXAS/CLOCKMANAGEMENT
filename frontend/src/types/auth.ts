export interface UserProfile {
  id: string;
  email: string;
  emailVerified: boolean;
  firstName: string;
  lastName: string;
  photoUrl: string | null;
  religionCode: string | null;
  countryCode: string | null;
  timezoneId: string;
  timeFormat: string;
  language: string;
  theme: "light" | "dark" | "system";
  prayerLatitude: number | null;
  prayerLongitude: number | null;
  prayerCalculationMethod: number | null;
  weatherLatitude: number | null;
  weatherLongitude: number | null;
  emailNotificationsEnabled: boolean;
  pushNotificationsEnabled: boolean;
  subscriptionStatus: string;
  twoFactorEnabled: boolean;
  roles: string[];
  permissions: string[];
}

export interface LoginResponse {
  requiresTwoFactor?: boolean;
  challengeToken?: string;
  profile?: UserProfile;
  csrfToken?: string;
}
