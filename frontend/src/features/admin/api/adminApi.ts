import { apiClient } from "@/shared/lib/apiClient";
import { fetchAndDownload } from "@/shared/lib/downloadBlob";
import type { AdminAuditLog, AdminPermission, AdminRole, AdminUser, PagedResult } from "@/types/admin";

export const adminApi = {
  getUsers: (params: { search?: string; page: number; pageSize: number }) =>
    apiClient.get<PagedResult<AdminUser>>("/admin/users", { params }).then((r) => r.data),
  exportUsersCsv: () => fetchAndDownload(apiClient, "/admin/users/export", "users.csv"),
  setUserActive: (userId: string, isActive: boolean) =>
    apiClient.put(`/admin/users/${userId}/active`, { isActive }).then((r) => r.data),
  unlockUser: (userId: string) => apiClient.post(`/admin/users/${userId}/unlock`).then((r) => r.data),
  assignUserRole: (userId: string, roleCode: string) =>
    apiClient.put(`/admin/users/${userId}/role`, { roleCode }).then((r) => r.data),

  getRoles: () => apiClient.get<AdminRole[]>("/admin/roles").then((r) => r.data),
  getPermissions: () => apiClient.get<AdminPermission[]>("/admin/permissions").then((r) => r.data),
  setRolePermission: (roleId: string, permissionId: string, granted: boolean) =>
    apiClient.put(`/admin/roles/${roleId}/permissions/${permissionId}`, { granted }).then((r) => r.data),

  getAuditLogs: (params: { page: number; pageSize: number }) =>
    apiClient.get<PagedResult<AdminAuditLog>>("/admin/audit-logs", { params }).then((r) => r.data),
  exportAuditLogsCsv: () => fetchAndDownload(apiClient, "/admin/audit-logs/export", "audit-log.csv"),
};
