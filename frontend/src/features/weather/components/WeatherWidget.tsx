import { Link } from "react-router";
import { useQuery } from "@tanstack/react-query";
import { Settings } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { weatherApi } from "../api/weatherApi";

export function WeatherWidget() {
  const { data, isLoading, error } = useQuery({ queryKey: ["weather"], queryFn: weatherApi.getWeather, retry: false });

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Weather</CardTitle>
        </CardHeader>
        <CardContent className="text-sm text-muted-foreground">Loading...</CardContent>
      </Card>
    );
  }

  if (error || !data) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Weather</CardTitle>
        </CardHeader>
        <CardContent className="flex flex-col gap-2">
          <p className="text-sm text-muted-foreground">Set your weather location in Settings to see local conditions.</p>
          <Button variant="outline" size="sm" className="w-fit" asChild>
            <Link to="/settings">
              <Settings /> Go to Settings
            </Link>
          </Button>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Weather</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-3">
        <div className="flex items-center gap-3">
          <span className="text-4xl">{data.icon}</span>
          <div>
            <p className="text-2xl font-semibold">{Math.round(data.tempC)}°C</p>
            <p className="text-sm text-muted-foreground">{data.condition}</p>
          </div>
        </div>
        <div className="flex gap-4 text-xs text-muted-foreground">
          <span>Feels like {Math.round(data.feelsLikeC)}°C</span>
          <span>Humidity {data.humidity}%</span>
          <span>Wind {Math.round(data.windSpeedKph)} km/h</span>
        </div>
        <div className="flex justify-between border-t pt-2">
          {data.forecast.slice(1).map((day) => (
            <div key={day.date} className="flex flex-col items-center gap-0.5">
              <span className="text-xs text-muted-foreground">
                {new Date(day.date).toLocaleDateString(undefined, { weekday: "short" })}
              </span>
              <span className="text-lg">{day.icon}</span>
              <span className="text-xs">
                {Math.round(day.maxTempC)}°/{Math.round(day.minTempC)}°
              </span>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
