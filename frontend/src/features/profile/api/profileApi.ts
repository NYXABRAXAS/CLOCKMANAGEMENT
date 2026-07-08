import { apiClient } from "@/shared/lib/apiClient";
import type { UserProfile } from "@/types/auth";

export const profileApi = {
  updateProfile: (data: { firstName: string; lastName: string }) =>
    apiClient.put<UserProfile>("/profile", data).then((r) => r.data),

  changePassword: (data: { currentPassword: string; newPassword: string }) =>
    apiClient.post<{ message: string }>("/profile/change-password", data).then((r) => r.data),

  uploadPhoto: (file: File) => {
    const formData = new FormData();
    formData.append("file", file);
    return apiClient
      .post<UserProfile>("/profile/photo", formData, { headers: { "Content-Type": "multipart/form-data" } })
      .then((r) => r.data);
  },
};
