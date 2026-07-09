import { useQuery } from "@tanstack/react-query";
import { Loader2 } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { religionCenterApi } from "../api/religionCenterApi";

function daysAwayLabel(days: number) {
  if (days === 0) return "Today";
  if (days === 1) return "Tomorrow";
  return `In ${days} days`;
}

export function FestivalsPanel({ religionId, title }: { religionId?: string; title?: string }) {
  const { data: festivals, isLoading } = useQuery({
    queryKey: ["festivals", religionId ?? "all"],
    queryFn: () => religionCenterApi.getFestivals(120, religionId),
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle>{title ?? "Upcoming Festivals"}</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-2">
        {isLoading && <Loader2 className="mx-auto size-5 animate-spin text-muted-foreground" />}
        {!isLoading && festivals?.length === 0 && <p className="text-sm text-muted-foreground">No upcoming festivals in the next 4 months.</p>}
        {festivals?.map((f) => (
          <div key={f.id} className="flex items-center justify-between rounded-md border p-2.5 text-sm">
            <div className="flex items-center gap-2">
              {f.emoji && <span className="text-lg">{f.emoji}</span>}
              <div>
                <p className="font-medium">{f.name}</p>
                <p className="text-xs text-muted-foreground">
                  {f.religionName} · {new Date(`${f.date}T00:00:00`).toLocaleDateString(undefined, { month: "short", day: "numeric", year: "numeric" })}
                </p>
              </div>
            </div>
            <span className="text-xs text-muted-foreground">{daysAwayLabel(f.daysAway)}</span>
          </div>
        ))}
      </CardContent>
    </Card>
  );
}
