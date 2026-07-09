import * as React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { ChevronLeft, ChevronRight, Loader2, Lock, LockOpen, Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useAppSelector } from "@/app/hooks";
import { adminApi } from "../api/adminApi";
import { toApiError } from "@/shared/lib/apiClient";

const PAGE_SIZE = 20;
const ROLE_OPTIONS = ["SUPER_ADMIN", "ADMIN", "PREMIUM_USER", "STANDARD_USER", "GUEST"];

export function UsersTab() {
  const currentUser = useAppSelector((s) => s.auth.user);
  const canEdit = currentUser?.permissions.includes("USERS:edit") ?? false;
  const queryClient = useQueryClient();
  const [search, setSearch] = React.useState("");
  const [page, setPage] = React.useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ["admin", "users", search, page],
    queryFn: () => adminApi.getUsers({ search: search || undefined, page, pageSize: PAGE_SIZE }),
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["admin", "users"] });

  const onToggleActive = async (userId: string, isActive: boolean) => {
    try {
      await adminApi.setUserActive(userId, isActive);
      toast.success(isActive ? "User activated." : "User deactivated.");
      invalidate();
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const onUnlock = async (userId: string) => {
    try {
      await adminApi.unlockUser(userId);
      toast.success("Account unlocked.");
      invalidate();
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const onRoleChange = async (userId: string, roleCode: string) => {
    try {
      await adminApi.assignUserRole(userId, roleCode);
      toast.success("Role updated. The user must log out and back in for the change to take effect.");
      invalidate();
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const totalPages = data ? Math.max(1, Math.ceil(data.totalCount / PAGE_SIZE)) : 1;

  return (
    <div className="flex flex-col gap-4">
      <div className="relative max-w-sm">
        <Search className="absolute left-2.5 top-2.5 size-4 text-muted-foreground" />
        <Input
          placeholder="Search by name or email..."
          className="pl-8"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }}
        />
      </div>

      {isLoading ? (
        <div className="flex justify-center py-8">
          <Loader2 className="size-6 animate-spin text-muted-foreground" />
        </div>
      ) : (
        <div className="flex flex-col gap-2">
          {data?.items.map((u) => {
            const isLocked = u.lockedUntil && new Date(u.lockedUntil) > new Date();
            return (
              <Card key={u.id}>
                <CardContent className="flex flex-wrap items-center justify-between gap-3 py-3">
                  <div className="flex flex-col">
                    <span className="text-sm font-medium">
                      {u.firstName} {u.lastName}
                    </span>
                    <span className="text-xs text-muted-foreground">{u.email}</span>
                  </div>

                  <div className="flex items-center gap-2 text-xs">
                    <span
                      className={`rounded-full px-2 py-0.5 ${u.isActive ? "bg-emerald-500/10 text-emerald-600" : "bg-red-500/10 text-red-600"}`}
                    >
                      {u.isActive ? "Active" : "Inactive"}
                    </span>
                    {isLocked && <span className="rounded-full bg-amber-500/10 px-2 py-0.5 text-amber-600">Locked</span>}
                    <span className="text-muted-foreground">{u.subscriptionStatus}</span>
                  </div>

                  <div className="flex items-center gap-2">
                    <Select
                      value={u.roles[0] ?? ""}
                      onValueChange={(v) => onRoleChange(u.id, v)}
                      disabled={!canEdit || u.id === currentUser?.id}
                    >
                      <SelectTrigger className="h-8 w-40 text-xs">
                        <SelectValue placeholder="No role" />
                      </SelectTrigger>
                      <SelectContent>
                        {ROLE_OPTIONS.map((r) => (
                          <SelectItem key={r} value={r}>
                            {r}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>

                    {isLocked && canEdit && (
                      <Button size="icon" variant="outline" className="size-8" onClick={() => onUnlock(u.id)} title="Unlock account">
                        <LockOpen className="size-3.5" />
                      </Button>
                    )}

                    {canEdit && (
                      <Button
                        size="sm"
                        variant={u.isActive ? "outline" : "default"}
                        disabled={u.id === currentUser?.id}
                        onClick={() => onToggleActive(u.id, !u.isActive)}
                      >
                        {u.isActive ? <Lock className="size-3.5" /> : <LockOpen className="size-3.5" />}
                        {u.isActive ? "Deactivate" : "Activate"}
                      </Button>
                    )}
                  </div>
                </CardContent>
              </Card>
            );
          })}
          {data?.items.length === 0 && <p className="py-8 text-center text-sm text-muted-foreground">No users found.</p>}
        </div>
      )}

      {data && data.totalCount > PAGE_SIZE && (
        <div className="flex items-center justify-center gap-3">
          <Button variant="outline" size="icon" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
            <ChevronLeft className="size-4" />
          </Button>
          <span className="text-sm text-muted-foreground">
            Page {page} of {totalPages}
          </span>
          <Button variant="outline" size="icon" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>
            <ChevronRight className="size-4" />
          </Button>
        </div>
      )}
    </div>
  );
}
