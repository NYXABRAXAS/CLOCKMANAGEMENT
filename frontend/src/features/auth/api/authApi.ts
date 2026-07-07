import { apiClient, setCsrfToken } from "@/shared/lib/apiClient";
import type { LoginResponse, UserProfile } from "@/types/auth";

export const authApi = {
  register: (data: { firstName: string; lastName: string; email: string; password: string }) =>
    apiClient
      .post<{ userId: string; email: string; verificationEmailSent: boolean; devOnlyVerificationToken: string | null }>(
        "/auth/register",
        data,
      )
      .then((r) => r.data),

  login: async (data: { email: string; password: string; rememberMe: boolean }) => {
    const res = await apiClient.post<LoginResponse>("/auth/login", data);
    if (res.data.csrfToken) setCsrfToken(res.data.csrfToken);
    return res.data;
  },

  verifyTwoFactorLogin: async (data: { challengeToken: string; code: string; rememberMe: boolean }) => {
    const res = await apiClient.post<{ profile: UserProfile; csrfToken: string }>("/auth/login/2fa", data);
    setCsrfToken(res.data.csrfToken);
    return res.data;
  },

  logout: () => apiClient.post("/auth/logout").then((r) => r.data as { success: boolean }),

  me: async () => {
    const res = await apiClient.get<{ profile: UserProfile; csrfToken: string }>("/auth/me");
    setCsrfToken(res.data.csrfToken);
    return res.data.profile;
  },

  forgotPassword: (email: string) =>
    apiClient
      .post<{ message: string; devOnlyResetToken: string | null }>("/auth/forgot-password", { email })
      .then((r) => r.data),

  resetPassword: (data: { token: string; newPassword: string }) =>
    apiClient.post<{ message: string }>("/auth/reset-password", data).then((r) => r.data),

  verifyEmail: (token: string) => apiClient.post<{ message: string }>("/auth/verify-email", { token }).then((r) => r.data),

  resendVerification: (email: string) =>
    apiClient
      .post<{ message: string; devOnlyVerificationToken: string | null }>("/auth/resend-verification", { email })
      .then((r) => r.data),

  setupTwoFactor: () => apiClient.post<{ secret: string; qrCodePngBase64: string }>("/auth/2fa/setup").then((r) => r.data),

  verifyTwoFactorSetup: (code: string) => apiClient.post<{ message: string }>("/auth/2fa/verify-setup", { code }).then((r) => r.data),

  disableTwoFactor: (code: string) => apiClient.post<{ message: string }>("/auth/2fa/disable", { code }).then((r) => r.data),

  sessions: () =>
    apiClient
      .get<{ id: string; deviceName: string | null; ipAddress: string | null; lastActiveAt: string; isCurrent: boolean }[]>(
        "/auth/sessions",
      )
      .then((r) => r.data),

  revokeSession: (sessionId: string) => apiClient.delete(`/auth/sessions/${sessionId}`).then((r) => r.data),
};
