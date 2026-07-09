import { useQuery } from "@tanstack/react-query";
import { Loader2, Star } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { religionCenterApi } from "../api/religionCenterApi";

export function HebrewDateCard() {
  const { data: hebrewDate, isLoading } = useQuery({ queryKey: ["hebrewDate", "today"], queryFn: () => religionCenterApi.getHebrewDate(new Date()) });

  return (
    <Card>
      <CardHeader>
        <CardTitle>Hebrew Date</CardTitle>
      </CardHeader>
      <CardContent>
        {isLoading && <Loader2 className="mx-auto size-6 animate-spin text-muted-foreground" />}
        {hebrewDate && (
          <div className="flex items-start gap-3">
            <Star className="mt-1 size-6 text-blue-500" />
            <div>
              <p className="text-lg font-semibold">
                {hebrewDate.hebrewDay} {hebrewDate.hebrewMonth} {hebrewDate.hebrewYear}
              </p>
              <p className="text-sm text-muted-foreground" dir="rtl">
                {hebrewDate.formatted}
              </p>
              {hebrewDate.events.length > 0 && (
                <ul className="mt-2 flex flex-col gap-0.5 text-sm">
                  {hebrewDate.events.map((e) => (
                    <li key={e} className="text-primary">
                      {e}
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
