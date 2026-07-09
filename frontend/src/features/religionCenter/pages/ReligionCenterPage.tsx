import { Link } from "react-router";
import { useQuery } from "@tanstack/react-query";
import { Settings } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useAppSelector } from "@/app/hooks";
import { religionsApi } from "@/features/religions/api/religionsApi";
import { DailyQuoteCard } from "../components/DailyQuoteCard";
import { FestivalsPanel } from "../components/FestivalsPanel";
import { PrayerTimesCard } from "../components/PrayerTimesCard";
import { PanchangCard } from "../components/PanchangCard";
import { HebrewDateCard } from "../components/HebrewDateCard";

export default function ReligionCenterPage() {
  const user = useAppSelector((s) => s.auth.user);
  const { data: religions } = useQuery({ queryKey: ["religions"], queryFn: religionsApi.getReligions });
  const userReligion = religions?.find((r) => r.code === user?.religionCode);

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Prayer &amp; Festival Center</h1>
        <p className="text-sm text-muted-foreground">Prayer times, calendars, festivals, and daily reflections.</p>
      </div>

      <DailyQuoteCard />

      {!user?.religionCode && (
        <Card>
          <CardHeader>
            <CardTitle>Set your religion</CardTitle>
            <CardDescription>Choose your religion in Settings to unlock prayer times, calendar views, and personalized festivals.</CardDescription>
          </CardHeader>
          <CardContent>
            <Button asChild>
              <Link to="/settings">
                <Settings /> Go to Settings
              </Link>
            </Button>
          </CardContent>
        </Card>
      )}

      {user?.religionCode === "ISLAM" && <PrayerTimesCard />}
      {user?.religionCode === "HINDUISM" && <PanchangCard />}
      {user?.religionCode === "JUDAISM" && <HebrewDateCard />}

      <FestivalsPanel
        religionId={userReligion?.id}
        title={userReligion ? `${userReligion.name} Festivals` : "Upcoming Festivals"}
      />

      {userReligion && <FestivalsPanel title="Festivals Across All Religions" />}
    </div>
  );
}
