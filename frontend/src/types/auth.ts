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
