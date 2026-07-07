import { LogOut } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { useAppDispatch, useAppSelector } from "@/app/hooks";
import { logout } from "@/features/auth/authSlice";
import { useNavigate } from "react-router";

/**
 * Placeholder shell for this milestone (auth UI) - proves the full login -> protected route ->
 * profile loop works end to end. The real widget-rich dashboard is built out in its own
 * milestone.
 */
export default function DashboardPage() {
  const user = useAppSelector((s) => s.auth.user);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const onLogout = async () => {
    await dispatch(logout());
    navigate("/login");
  };

  return (
    <div className="mx-auto flex min-h-screen max-w-3xl flex-col gap-6 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Welcome, {user?.firstName}!</h1>
          <p className="text-sm text-muted-foreground">{user?.email}</p>
        </div>
        <Button variant="outline" onClick={onLogout}>
          <LogOut /> Log out
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Your account</CardTitle>
          <CardDescription>This is a placeholder - the full dashboard lands in the next milestone.</CardDescription>
        </CardHeader>
        <CardContent className="grid gap-2 text-sm">
          <div>
            <span className="font-medium">Roles:</span> {user?.roles.join(", ")}
          </div>
          <div>
            <span className="font-medium">Email verified:</span> {user?.emailVerified ? "Yes" : "No"}
          </div>
          <div>
            <span className="font-medium">Two-factor enabled:</span> {user?.twoFactorEnabled ? "Yes" : "No"}
          </div>
          <div>
            <span className="font-medium">Permissions:</span> {user?.permissions.length}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
