import * as React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Loader2 } from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useAppSelector } from "@/app/hooks";
import { adminApi } from "../api/adminApi";
import { toApiError } from "@/shared/lib/apiClient";

const ACTIONS = ["view", "create", "edit", "delete"] as const;

export function RolesTab() {
  const currentUser = useAppSelector((s) => s.auth.user);
  const canEdit = currentUser?.permissions.includes("ROLES:edit") ?? false;
  const queryClient = useQueryClient();

  const { data: roles, isLoading: rolesLoading } = useQuery({ queryKey: ["admin", "roles"], queryFn: adminApi.getRoles });
  const { data: permissions, isLoading: permsLoading } = useQuery({ queryKey: ["admin", "permissions"], queryFn: adminApi.getPermissions });
  const [selectedRoleId, setSelectedRoleId] = React.useState<string>("");

  React.useEffect(() => {
    if (!selectedRoleId && roles && roles.length > 0) setSelectedRoleId(roles[0].id);
  }, [roles, selectedRoleId]);

  const selectedRole = roles?.find((r) => r.id === selectedRoleId);
  const grantedIds = new Set(selectedRole?.permissionIds ?? []);
  const isSuperAdmin = selectedRole?.code === "SUPER_ADMIN";

  const modules = React.useMemo(() => {
    if (!permissions) return [];
    const set = new Set(permissions.map((p) => p.module));
    return [...set].sort();
  }, [permissions]);

  const findPermission = (module: string, action: string) => permissions?.find((p) => p.module === module && p.action === action);

  const onToggle = async (permissionId: string, granted: boolean) => {
    if (!selectedRole) return;
    try {
      await adminApi.setRolePermission(selectedRole.id, permissionId, granted);
      queryClient.invalidateQueries({ queryKey: ["admin", "roles"] });
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  if (rolesLoading || permsLoading) {
    return (
      <div className="flex justify-center py-8">
        <Loader2 className="size-6 animate-spin text-muted-foreground" />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-col gap-1.5 sm:max-w-xs">
        <Select value={selectedRoleId} onValueChange={setSelectedRoleId}>
          <SelectTrigger>
            <SelectValue placeholder="Select a role" />
          </SelectTrigger>
          <SelectContent>
            {roles?.map((r) => (
              <SelectItem key={r.id} value={r.id}>
                {r.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {selectedRole && (
        <Card>
          <CardHeader>
            <CardTitle>{selectedRole.name}</CardTitle>
            <CardDescription>
              {isSuperAdmin
                ? "Super Admin always has full access - its permissions can't be changed."
                : selectedRole.description ?? "Toggle which modules and actions this role can access."}
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid gap-2">
              <div className="grid grid-cols-[1fr_repeat(4,3.5rem)] gap-2 border-b pb-2 text-xs font-medium text-muted-foreground">
                <span>Module</span>
                {ACTIONS.map((a) => (
                  <span key={a} className="text-center capitalize">
                    {a}
                  </span>
                ))}
              </div>
              {modules.map((module) => (
                <div key={module} className="grid grid-cols-[1fr_repeat(4,3.5rem)] items-center gap-2 py-1 text-sm">
                  <span>{module}</span>
                  {ACTIONS.map((action) => {
                    const permission = findPermission(module, action);
                    if (!permission) return <span key={action} />;
                    return (
                      <span key={action} className="flex justify-center">
                        <Checkbox
                          checked={grantedIds.has(permission.id)}
                          disabled={!canEdit || isSuperAdmin}
                          onCheckedChange={(checked) => onToggle(permission.id, checked === true)}
                        />
                      </span>
                    );
                  })}
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
