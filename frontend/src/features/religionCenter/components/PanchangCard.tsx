import { useQuery } from "@tanstack/react-query";
import { Loader2, Moon } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { religionCenterApi } from "../api/religionCenterApi";

export function PanchangCard() {
  const { data: panchang, isLoading } = useQuery({ queryKey: ["panchang", "today"], queryFn: () => religionCenterApi.getPanchang(new Date()) });

  return (
    <Card>
      <CardHeader>
        <CardTitle>Today's Panchang</CardTitle>
        <p className="text-xs text-amber-600 dark:text-amber-500">
          Approximate - computed from simplified lunar-day arithmetic, not a full ephemeris. Treat as a rough guide.
        </p>
      </CardHeader>
      <CardContent>
        {isLoading && <Loader2 className="mx-auto size-6 animate-spin text-muted-foreground" />}
        {panchang && (
          <div className="grid grid-cols-2 gap-4">
            <div className="flex items-center gap-3">
              <Moon className="size-8 text-indigo-500" />
              <div>
                <p className="text-sm text-muted-foreground">Tithi</p>
                <p className="font-semibold">{panchang.tithiName}</p>
              </div>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Paksha</p>
              <p className="font-semibold">{panchang.paksha}</p>
            </div>
            <div className="col-span-2">
              <p className="text-sm text-muted-foreground">Nakshatra</p>
              <p className="font-semibold">{panchang.nakshatraName}</p>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
