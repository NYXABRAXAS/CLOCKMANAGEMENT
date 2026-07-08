import { Link } from "react-router";
import { AlarmClock, CalendarDays, Clock, Coffee, Hourglass, ListChecks, Moon, Pill, Settings, ShieldCheck, Timer, User as UserIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useAppSelector } from "@/app/hooks";
import { LiveClockWidget } from "../components/LiveClockWidget";

export default function DashboardPage() {
  const user = useAppSelector((s) => s.auth.user);
  if (!user) return null;

  return (
    <div className="mx-auto flex max-w-5xl flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Welcome, {user.firstName}!</h1>
        <p className="text-sm text-muted-foreground">{user.email}</p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        <LiveClockWidget />

        <Card>
          <CardHeader>
            <CardTitle>Account status</CardTitle>
            <CardDescription>A quick snapshot of your account.</CardDescription>
          </CardHeader>
          <CardContent className="grid gap-2 text-sm">
            <div>
              <span className="font-medium">Roles:</span> {user.roles.join(", ")}
            </div>
            <div>
              <span className="font-medium">Email verified:</span> {user.emailVerified ? "Yes" : "No"}
            </div>
            <div className="flex items-center gap-1.5">
              <ShieldCheck className="size-4 text-muted-foreground" />
              <span className="font-medium">Two-factor:</span> {user.twoFactorEnabled ? "Enabled" : "Disabled"}
            </div>
            <div>
              <span className="font-medium">Plan:</span> {user.subscriptionStatus}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Quick links</CardTitle>
            <CardDescription>Jump straight to what you need.</CardDescription>
          </CardHeader>
          <CardContent className="flex flex-col gap-2">
            <Button variant="outline" className="justify-start" asChild>
              <Link to="/world-clock">
                <Clock /> World Clock
              </Link>
            </Button>
            <Button variant="outline" className="justify-start" asChild>
              <Link to="/alarms">
                <AlarmClock /> Alarms
              </Link>
            </Button>
            <Button variant="outline" className="justify-start" asChild>
              <Link to="/countdown-timer">
                <Hourglass /> Countdown Timer
              </Link>
            </Button>
            <Button variant="outline" className="justify-start" asChild>
              <Link to="/stopwatch">
                <Timer /> Stopwatch
              </Link>
            </Button>
            <Button variant="outline" className="justify-start" asChild>
              <Link to="/pomodoro">
                <Coffee /> Pomodoro
              </Link>
            </Button>
            <Button variant="outline" className="justify-start" asChild>
              <Link to="/calendar">
                <CalendarDays /> Calendar
              </Link>
            </Button>
            <Button variant="outline" className="justify-start" asChild>
              <Link to="/medicines">
                <Pill /> Medicines
              </Link>
            </Button>
            <Button variant="outline" className="justify-start" asChild>
              <Link to="/habits">
                <ListChecks /> Habits
              </Link>
            </Button>
            <Button variant="outline" className="justify-start" asChild>
              <Link to="/sleep">
                <Moon /> Sleep Tracker
              </Link>
            </Button>
            <Button variant="outline" className="justify-start" asChild>
              <Link to="/profile">
                <UserIcon /> Edit profile
              </Link>
            </Button>
            <Button variant="outline" className="justify-start" asChild>
              <Link to="/settings">
                <Settings /> Preferences
              </Link>
            </Button>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>More on the way</CardTitle>
          <CardDescription>
            The prayer &amp; festival center and productivity insights land here as each module is built.
          </CardDescription>
        </CardHeader>
      </Card>
    </div>
  );
}
