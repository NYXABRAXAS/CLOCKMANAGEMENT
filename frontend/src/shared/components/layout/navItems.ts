import { AlarmClock, CalendarDays, Clock, Coffee, Hourglass, LayoutDashboard, Repeat, Settings, Timer, User } from "lucide-react";

export interface NavItem {
  to: string;
  label: string;
  icon: typeof LayoutDashboard;
}

// Each future milestone adds its own route here once it actually exists - no placeholder links
// to pages that don't exist yet.
export const navItems: NavItem[] = [
  { to: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { to: "/world-clock", label: "World Clock", icon: Clock },
  { to: "/timezone-converter", label: "Timezone Converter", icon: Repeat },
  { to: "/alarms", label: "Alarms", icon: AlarmClock },
  { to: "/countdown-timer", label: "Countdown Timer", icon: Hourglass },
  { to: "/stopwatch", label: "Stopwatch", icon: Timer },
  { to: "/pomodoro", label: "Pomodoro", icon: Coffee },
  { to: "/calendar", label: "Calendar", icon: CalendarDays },
  { to: "/settings", label: "Settings", icon: Settings },
  { to: "/profile", label: "Profile", icon: User },
];
