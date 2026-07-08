import * as React from "react";
import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Loader2 } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { worldClockApi } from "../api/worldClockApi";
import { zonedWallTimeToUtc } from "@/shared/lib/timezone";

function nowAsDateTimeLocalValue() {
  const now = new Date();
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}T${pad(now.getHours())}:${pad(now.getMinutes())}`;
}

export default function TimezoneConverterPage() {
  const { data: pins, isLoading } = useQuery({ queryKey: ["worldClockCities"], queryFn: worldClockApi.getPinnedCities });
  const [fromCityId, setFromCityId] = React.useState<string>("");
  const [dateTimeValue, setDateTimeValue] = React.useState(nowAsDateTimeLocalValue());

  React.useEffect(() => {
    if (!fromCityId && pins && pins.length > 0) setFromCityId(pins[0].id);
  }, [pins, fromCityId]);

  const fromPin = pins?.find((p) => p.id === fromCityId);

  const utcInstant = React.useMemo(() => {
    if (!fromPin) return null;
    const match = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})$/.exec(dateTimeValue);
    if (!match) return null;
    const [, y, mo, d, h, mi] = match;
    return zonedWallTimeToUtc(Number(y), Number(mo), Number(d), Number(h), Number(mi), fromPin.city.timezoneId);
  }, [fromPin, dateTimeValue]);

  if (isLoading) {
    return (
      <div className="flex justify-center py-12">
        <Loader2 className="size-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (!pins || pins.length < 2) {
    return (
      <div className="mx-auto max-w-2xl">
        <h1 className="mb-2 text-2xl font-semibold">Timezone Converter</h1>
        <div className="rounded-lg border border-dashed p-12 text-center text-sm text-muted-foreground">
          Pin at least two cities on the World Clock page to convert times between them.
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Timezone Converter</h1>
        <p className="text-sm text-muted-foreground">Pick a city and time, and see what time it is everywhere else you've pinned.</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>From</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-4 sm:grid-cols-2">
          <div className="flex flex-col gap-1.5">
            <Label>City</Label>
            <Select value={fromCityId} onValueChange={setFromCityId}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {pins.map((p) => (
                  <SelectItem key={p.id} value={p.id}>
                    {p.city.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="datetime">Date &amp; time</Label>
            <input
              id="datetime"
              type="datetime-local"
              value={dateTimeValue}
              onChange={(e) => setDateTimeValue(e.target.value)}
              className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
            />
          </div>
        </CardContent>
      </Card>

      <div className="grid gap-4 md:grid-cols-2">
        {pins
          .filter((p) => p.id !== fromCityId)
          .map((p) => {
            const converted = utcInstant
              ? new Intl.DateTimeFormat(undefined, {
                  timeZone: p.city.timezoneId,
                  hour: "2-digit",
                  minute: "2-digit",
                  hour12: false,
                }).format(utcInstant)
              : "-";
            const convertedDate = utcInstant
              ? new Intl.DateTimeFormat(undefined, {
                  timeZone: p.city.timezoneId,
                  weekday: "short",
                  month: "short",
                  day: "numeric",
                }).format(utcInstant)
              : "";

            return (
              <Card key={p.id}>
                <CardContent className="flex items-center justify-between pt-5">
                  <div>
                    <p className="font-medium">{p.city.name}</p>
                    <p className="text-xs text-muted-foreground">{p.city.country}</p>
                  </div>
                  <div className="flex items-center gap-2 text-right">
                    <ArrowRight className="size-4 text-muted-foreground" />
                    <div>
                      <p className="font-mono text-xl font-semibold tabular-nums">{converted}</p>
                      <p className="text-xs text-muted-foreground">{convertedDate}</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            );
          })}
      </div>
    </div>
  );
}
