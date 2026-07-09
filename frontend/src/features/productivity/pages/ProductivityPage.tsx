import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { Loader2, Flame, Trophy, Timer, ListChecks, Sparkles } from "lucide-react";
import { Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { productivityApi } from "../api/productivityApi";

const RANGE_DAYS = 30;
const PRODUCTIVE_THRESHOLD = 60;

function startOfRange() {
  const to = new Date();
  const from = new Date();
  from.setDate(from.getDate() - (RANGE_DAYS - 1));
  return { from, to };
}

function scoreColor(score: number | null) {
  if (score === null) return "text-muted-foreground";
  if (score >= PRODUCTIVE_THRESHOLD) return "text-emerald-500";
  if (score >= 35) return "text-amber-500";
  return "text-red-500";
}

export default function ProductivityPage() {
  const { from, to } = useMemo(startOfRange, []);
  const { data, isLoading, error } = useQuery({
    queryKey: ["productivity", "summary", from.toDateString(), to.toDateString()],
    queryFn: () => productivityApi.getSummary(from, to),
  });

  if (isLoading) {
    return (
      <div className="flex justify-center py-16">
        <Loader2 className="size-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (error || !data) {
    return (
      <Card>
        <CardContent className="py-8 text-center text-sm text-muted-foreground">Couldn't load your productivity data.</CardContent>
      </Card>
    );
  }

  const today = data.days.at(-1);
  const chartData = data.days.map((d) => ({
    date: d.date.slice(5),
    score: d.score === null ? null : Math.round(d.score),
  }));

  return (
    <div className="mx-auto flex max-w-5xl flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Productivity Dashboard</h1>
        <p className="text-sm text-muted-foreground">
          A rolling score built from your habits, medicines, sleep, focus sessions{data.totalPrayersLogged > 0 || today?.components.prayersPercent !== null ? ", and prayers" : ""} over the last {RANGE_DAYS} days.
        </p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardDescription className="flex items-center gap-1.5">
              <Sparkles className="size-3.5" /> Today's score
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className={`text-3xl font-semibold ${scoreColor(today?.score ?? null)}`}>
              {today?.score !== null && today?.score !== undefined ? Math.round(today.score) : "—"}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardDescription className="flex items-center gap-1.5">
              <Flame className="size-3.5" /> Current streak
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-semibold">
              {data.currentStreak} <span className="text-base font-normal text-muted-foreground">day{data.currentStreak === 1 ? "" : "s"}</span>
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardDescription className="flex items-center gap-1.5">
              <Trophy className="size-3.5" /> Best streak
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-semibold">
              {data.bestStreak} <span className="text-base font-normal text-muted-foreground">day{data.bestStreak === 1 ? "" : "s"}</span>
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardDescription className="flex items-center gap-1.5">
              <Timer className="size-3.5" /> Focus minutes ({RANGE_DAYS}d)
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-semibold">{data.totalFocusMinutes}</p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Score trend</CardTitle>
          <CardDescription>Days with no applicable activity are shown as gaps, not zeros.</CardDescription>
        </CardHeader>
        <CardContent className="h-64">
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={chartData} margin={{ left: -20, right: 12, top: 8 }}>
              <defs>
                <linearGradient id="scoreFill" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="var(--primary)" stopOpacity={0.35} />
                  <stop offset="95%" stopColor="var(--primary)" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
              <XAxis dataKey="date" tick={{ fontSize: 11, fill: "var(--muted-foreground)" }} tickLine={false} axisLine={false} interval="preserveStartEnd" />
              <YAxis domain={[0, 100]} tick={{ fontSize: 11, fill: "var(--muted-foreground)" }} tickLine={false} axisLine={false} width={32} />
              <Tooltip
                contentStyle={{ background: "var(--card)", border: "1px solid var(--border)", borderRadius: 8, fontSize: 12 }}
                labelStyle={{ color: "var(--foreground)" }}
                formatter={(value) => [value === null || value === undefined ? "No data" : value, "Score"]}
              />
              <Area type="monotone" dataKey="score" stroke="var(--primary)" fill="url(#scoreFill)" strokeWidth={2} connectNulls={false} />
            </AreaChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-1.5">
            <ListChecks className="size-4" /> Today's breakdown
          </CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          <BreakdownItem label="Habits" value={today?.components.habitsPercent} suffix="%" />
          <BreakdownItem label="Medicines" value={today?.components.medicinesPercent} suffix="%" />
          <BreakdownItem label="Sleep" value={today?.components.sleepScore} suffix="/100" />
          <BreakdownItem label="Focus" value={today?.components.focusMinutes} suffix=" min" alwaysShow />
          {today?.components.prayersPercent !== null && today?.components.prayersPercent !== undefined && (
            <BreakdownItem label="Prayers" value={today.components.prayersPercent} suffix="%" />
          )}
        </CardContent>
      </Card>
    </div>
  );
}

function BreakdownItem({ label, value, suffix, alwaysShow }: { label: string; value?: number | null; suffix: string; alwaysShow?: boolean }) {
  const hasValue = value !== null && value !== undefined;
  return (
    <div className="rounded-lg border p-3">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="text-lg font-semibold">{hasValue ? `${Math.round(value)}${suffix}` : alwaysShow ? `0${suffix}` : "—"}</p>
    </div>
  );
}
