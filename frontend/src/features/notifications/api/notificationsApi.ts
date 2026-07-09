import { apiClient } from "@/shared/lib/apiClient";
import type { AppNotification } from "@/types/notifications";

export const notificationsApi = {
  getNotifications: () => apiClient.get<AppNotification[]>("/notifications").then((r) => r.data),
  getUnreadCount: () => apiClient.get<{ count: number }>("/notifications/unread-count").then((r) => r.data.count),
  markRead: (id: string) => apiClient.post(`/notifications/${id}/read`).then((r) => r.data),
  markAllRead: () => apiClient.post("/notifications/read-all").then((r) => r.data),
};
