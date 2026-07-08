import { ArrowDown, ArrowUp, Sunrise, Sunset, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useAppSelector } from "@/app/hooks";
import { useNow } from "@/shared/lib/useNow";
import { getSunTimes } from "@/shared/lib/solar";
import type { WorldClockCity } from "@/types/city";

function formatOffset(now: Date, timeZone: string) {
  const parts = new Intl.DateTimeFormat("en-US", { timeZone, timeZoneName: "shortOffset" }).formatToParts(now);
  return parts.find((p) => p.type === "timeZoneName")?.value ?? "";
}

export function CityClockCard({
  pin,
  onRemove,
  onMoveUp,
  onMoveDown,
  canMoveUp,
  canMoveDown,
}: {
  pin: WorldClockCity;
  onRemove: () => void;
  onMoveUp: () => void;
  onMoveDown: () => void;
  canMoveUp: boolean;
  canMoveDown: boolean;
}) {
  const hour12 = useAppSelector((s) => s.auth.user?.timeFormat) === "12h";
  const now = useNow();
  const { city } = pin;

  const time = new Intl.DateTimeFormat(undefined, {
    timeZone: city.timezoneId,
    hour: "2-digit",
    minute: "2-digit",
    hour12,
  }).format(now);

  const dateStr = new Intl.DateTimeFormat(undefined, {
    timeZone: city.timezoneId,
    weekday: "short",
    month: "short",
    day: "numeric",
  }).format(now);

  const sun = getSunTimes(now, city.latitude, city.longitude);
  const timeFmt = (d: Date) => new Intl.DateTimeFormat(undefined, { timeZone: city.timezoneId, hour: "2-digit", minute: "2-digit", hour12 }).format(d);

  return (
    <Card>
      <CardHeader className="flex-row items-start justify-between space-y-0">
        <div>
          <CardTitle>{city.name}</CardTitle>
          <p className="text-xs text-muted-foreground">
            {city.country} &middot; {formatOffset(now, city.timezoneId)}
          </p>
        </div>
        <div className="flex items-center gap-0.5">
          <Button variant="ghost" size="icon" className="size-7" disabled={!canMoveUp} onClick={onMoveUp}>
            <ArrowUp className="size-3.5" />
          </Button>
          <Button variant="ghost" size="icon" className="size-7" disabled={!canMoveDown} onClick={onMoveDown}>
            <ArrowDown className="size-3.5" />
          </Button>
          <Button variant="ghost" size="icon" className="size-7" onClick={onRemove}>
            <X className="size-3.5" />
          </Button>
        </div>
      </CardHeader>
      <CardContent>
        <p className="font-mono text-3xl font-semibold tabular-nums">{time}</p>
        <p className="mt-1 text-sm text-muted-foreground">{dateStr}</p>
        <div className="mt-3 flex gap-4 text-xs text-muted-foreground">
          <span className="flex items-center gap-1">
            <Sunrise className="size-3.5" /> {timeFmt(sun.sunrise)}
          </span>
          <span className="flex items-center gap-1">
            <Sunset className="size-3.5" /> {timeFmt(sun.sunset)}
          </span>
        </div>
      </CardContent>
    </Card>
  );
}
