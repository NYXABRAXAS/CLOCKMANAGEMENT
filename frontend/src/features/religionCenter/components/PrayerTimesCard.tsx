import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Check, Compass, Loader2 } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { religionCenterApi } from "../api/religionCenterApi";
import { toApiError } from "@/shared/lib/apiClient";

const PRAYERS: { key: "fajr" | "dhuhr" | "asr" | "maghrib" | "isha"; name: string }[] = [
  { key: "fajr", name: "Fajr" },
  { key: "dhuhr", name: "Dhuhr" },
  { key: "asr", name: "Asr" },
  { key: "maghrib", name: "Maghrib" },
  { key: "isha", name: "Isha" },
];

export function PrayerTimesCard() {
  const queryClient = useQueryClient();
  const today = new Date();
  const { data: times, isLoading, error } = useQuery({ queryKey: ["prayerTimes", "today"], queryFn: () => religionCenterApi.getPrayerTimes(today) });
  const { data: logs } = useQuery({ queryKey: ["prayerLogs", "today"], queryFn: () => religionCenterApi.getPrayerLogs(today) });

  const onToggle = async (prayerName: string, completed: boolean) => {
    try {
      await religionCenterApi.logPrayer(today, prayerName, completed);
      await queryClient.invalidateQueries({ queryKey: ["prayerLogs"] });
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const isCompleted = (name: string) => logs?.find((l) => l.prayerName === name)?.completed ?? false;

  if (isLoading) {
    return (
      <Card>
        <CardContent className="flex justify-center py-8">
          <Loader2 className="size-6 animate-spin text-muted-foreground" />
        </CardContent>
      </Card>
    );
  }

  if (error || !times) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Prayer Times</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">{toApiError(error).message || "Set your prayer location in Settings to see prayer times."}</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Prayer Times</CardTitle>
        <p className="text-xs text-muted-foreground">
          {times.hijriDay} {times.hijriMonth} {times.hijriYear} AH
        </p>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
          {PRAYERS.map((p) => {
            const completed = isCompleted(p.name);
            return (
              <button
                key={p.key}
                type="button"
                onClick={() => onToggle(p.name, !completed)}
                className={`flex flex-col items-center gap-1 rounded-lg border p-3 transition-colors ${completed ? "border-primary bg-primary/10" : ""}`}
              >
                <span className="text-xs font-medium text-muted-foreground">{p.name}</span>
                <span className="font-mono text-lg font-semibold">{times[p.key]}</span>
                {completed && <Check className="size-3.5 text-primary" />}
              </button>
            );
          })}
          <div className="flex flex-col items-center gap-1 rounded-lg border border-dashed p-3">
            <span className="text-xs font-medium text-muted-foreground">Sunrise</span>
            <span className="font-mono text-lg font-semibold">{times.sunrise}</span>
          </div>
        </div>

        <div className="flex items-center gap-3 border-t pt-4">
          <div className="relative flex size-12 items-center justify-center rounded-full border-2">
            <Compass className="absolute size-6 text-muted-foreground" />
            <div
              className="absolute h-5 w-0.5 origin-bottom bg-primary"
              style={{ transform: `rotate(${times.qiblaDirectionDegrees}deg)`, bottom: "50%" }}
            />
          </div>
          <div>
            <p className="text-sm font-medium">Qibla direction</p>
            <p className="text-xs text-muted-foreground">{Math.round(times.qiblaDirectionDegrees)}° from North</p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
