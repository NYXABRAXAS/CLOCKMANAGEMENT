import { useAppSelector } from "@/app/hooks";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { UsersTab } from "../components/UsersTab";
import { RolesTab } from "../components/RolesTab";
import { AuditLogsTab } from "../components/AuditLogsTab";
import { ReligionsTab } from "../components/ReligionsTab";

export default function AdminPage() {
  const user = useAppSelector((s) => s.auth.user);
  const permissions = user?.permissions ?? [];

  const canSeeUsers = permissions.includes("USERS:view");
  const canSeeRoles = permissions.includes("ROLES:view");
  const canSeeAuditLogs = permissions.includes("AUDIT_LOGS:view");
  const canSeeReligions = permissions.includes("RELIGIONS:view");

  const defaultTab = canSeeUsers ? "users" : canSeeRoles ? "roles" : canSeeAuditLogs ? "audit-logs" : "religions";

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Admin Panel</h1>
        <p className="text-sm text-muted-foreground">Manage users, roles, permissions, and reference data.</p>
      </div>

      <Tabs defaultValue={defaultTab}>
        <TabsList>
          {canSeeUsers && <TabsTrigger value="users">Users</TabsTrigger>}
          {canSeeRoles && <TabsTrigger value="roles">Roles &amp; Permissions</TabsTrigger>}
          {canSeeAuditLogs && <TabsTrigger value="audit-logs">Audit Log</TabsTrigger>}
          {canSeeReligions && <TabsTrigger value="religions">Religions</TabsTrigger>}
        </TabsList>

        {canSeeUsers && (
          <TabsContent value="users" className="mt-4">
            <UsersTab />
          </TabsContent>
        )}
        {canSeeRoles && (
          <TabsContent value="roles" className="mt-4">
            <RolesTab />
          </TabsContent>
        )}
        {canSeeAuditLogs && (
          <TabsContent value="audit-logs" className="mt-4">
            <AuditLogsTab />
          </TabsContent>
        )}
        {canSeeReligions && (
          <TabsContent value="religions" className="mt-4">
            <ReligionsTab />
          </TabsContent>
        )}
      </Tabs>
    </div>
  );
}
