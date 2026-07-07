import { Navigate, Outlet } from "react-router";
import { Loader2 } from "lucide-react";
import { useAppSelector } from "@/app/hooks";

export function ProtectedRoute() {
  const { user, status } = useAppSelector((s) => s.auth);

  if (status === "idle" || status === "loading") {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Loader2 className="size-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!user) return <Navigate to="/login" replace />;

  return <Outlet />;
}

export function GuestOnlyRoute() {
  const { user, status } = useAppSelector((s) => s.auth);

  if (status === "authenticated" && user) return <Navigate to="/dashboard" replace />;

  return <Outlet />;
}
