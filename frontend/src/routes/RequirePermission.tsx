import { Navigate, Outlet } from "react-router";
import { useAppSelector } from "@/app/hooks";

/// Gates a route subtree on the current user having at least one of the given permissions
/// (e.g. "USERS:view"). Assumes it's nested under ProtectedRoute, so `user` is already known to
/// be loaded by the time this renders - it only adds the permission check on top.
export function RequirePermission({ anyOf }: { anyOf: string[] }) {
  const user = useAppSelector((s) => s.auth.user);
  const hasAccess = anyOf.some((p) => user?.permissions.includes(p));

  if (!hasAccess) return <Navigate to="/dashboard" replace />;

  return <Outlet />;
}
