import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useAppSelector } from "@/app/hooks";
import { useNow } from "@/shared/lib/useNow";

export function LiveClockWidget() {
  const user = useAppSelector((s) => s.auth.user);
  const now = useNow();
  const timeZone = user?.timezoneId ?? "UTC";
  const hour12 = user?.timeFormat === "12h";

  const time = new Intl.DateTimeFormat(undefined, {
    timeZone,
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
    hour12,
  }).format(now);

  const date = new Intl.DateTimeFormat(undefined, {
    timeZone,
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  }).format(now);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Current time</CardTitle>
      </CardHeader>
      <CardContent>
        <p className="font-mono text-4xl font-semibold tabular-nums">{time}</p>
        <p className="mt-1 text-sm text-muted-foreground">{date}</p>
        <p className="mt-1 text-xs text-muted-foreground">{timeZone}</p>
      </CardContent>
    </Card>
  );
}
