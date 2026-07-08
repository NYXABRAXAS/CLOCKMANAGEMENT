import * as React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Loader2, Moon, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { sleepApi } from "../api/sleepApi";
import { toApiError } from "@/shared/lib/apiClient";
import type { SleepQuality } from "@/types/health";

const NONE = "__none__";

function todayIso() {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}

function defaultBedTime() {
  const d = new Date();
  d.setDate(d.getDate() - 1);
  d.setHours(22, 30, 0, 0);
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

function defaultWakeTime() {
  const d = new Date();
  d.setHours(7, 0, 0, 0);
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

function formatDuration(minutes: number) {
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  return `${h}h ${m}m`;
}

export default function SleepPage() {
  const queryClient = useQueryClient();
  const { data: logs, isLoading } = useQuery({ queryKey: ["sleepLogs"], queryFn: sleepApi.list });

  const [bedTime, setBedTime] = React.useState(defaultBedTime());
  const [wakeTime, setWakeTime] = React.useState(defaultWakeTime());
  const [quality, setQuality] = React.useState<SleepQuality | typeof NONE>(NONE);
  const [notes, setNotes] = React.useState("");
  const [saving, setSaving] = React.useState(false);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      await sleepApi.save({
        date: todayIso(),
        bedTime: new Date(bedTime).toISOString(),
        wakeTime: new Date(wakeTime).toISOString(),
        quality: quality === NONE ? null : quality,
        notes: notes || null,
      });
      await queryClient.invalidateQueries({ queryKey: ["sleepLogs"] });
      toast.success("Sleep logged.");
      setNotes("");
    } catch (err) {
      toast.error(toApiError(err).message);
    } finally {
      setSaving(false);
    }
  };

  const onDelete = async (id: string) => {
    try {
      await sleepApi.remove(id);
      await queryClient.invalidateQueries({ queryKey: ["sleepLogs"] });
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const avgMinutes = logs && logs.length > 0 ? Math.round(logs.slice(0, 7).reduce((sum, l) => sum + l.durationMinutes, 0) / Math.min(7, logs.length)) : null;

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Sleep Tracker</h1>
        <p className="text-sm text-muted-foreground">Log last night's sleep and track your average.</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Log last night</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={onSubmit} className="flex flex-col gap-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="flex flex-col gap-1.5">
                <Label>Bed time</Label>
                <input
                  type="datetime-local"
                  value={bedTime}
                  onChange={(e) => setBedTime(e.target.value)}
                  className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
                  required
                />
              </div>
              <div className="flex flex-col gap-1.5">
                <Label>Wake time</Label>
                <input
                  type="datetime-local"
                  value={wakeTime}
                  onChange={(e) => setWakeTime(e.target.value)}
                  className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
                  required
                />
              </div>
            </div>
            <div className="flex flex-col gap-1.5">
              <Label>Quality</Label>
              <Select value={quality} onValueChange={(v) => setQuality(v as SleepQuality)}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value={NONE}>Not rated</SelectItem>
                  <SelectItem value="Poor">Poor</SelectItem>
                  <SelectItem value="Fair">Fair</SelectItem>
                  <SelectItem value="Good">Good</SelectItem>
                  <SelectItem value="Excellent">Excellent</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="flex flex-col gap-1.5">
              <Label>Notes</Label>
              <Input value={notes} onChange={(e) => setNotes(e.target.value)} placeholder="Optional" />
            </div>
            <Button type="submit" disabled={saving}>
              {saving && <Loader2 className="animate-spin" />}
              Save
            </Button>
          </form>
        </CardContent>
      </Card>

      {avgMinutes !== null && (
        <Card>
          <CardContent className="flex items-center gap-3 pt-5">
            <Moon className="size-6 text-indigo-500" />
            <div>
              <p className="text-sm text-muted-foreground">7-day average</p>
              <p className="text-xl font-semibold">{formatDuration(avgMinutes)}</p>
            </div>
          </CardContent>
        </Card>
      )}

      {isLoading && (
        <div className="flex justify-center py-8">
          <Loader2 className="size-6 animate-spin text-muted-foreground" />
        </div>
      )}

      <div className="flex flex-col gap-2">
        {logs?.map((log) => (
          <Card key={log.id}>
            <CardContent className="flex items-center justify-between py-3">
              <div>
                <p className="text-sm font-medium">{new Date(`${log.date}T00:00:00`).toLocaleDateString(undefined, { weekday: "short", month: "short", day: "numeric" })}</p>
                <p className="text-xs text-muted-foreground">
                  {new Date(log.bedTime).toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit" })} -{" "}
                  {new Date(log.wakeTime).toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit" })}
                  {log.quality ? ` · ${log.quality}` : ""}
                </p>
              </div>
              <div className="flex items-center gap-3">
                <span className="font-mono text-sm">{formatDuration(log.durationMinutes)}</span>
                <Button variant="ghost" size="icon" onClick={() => onDelete(log.id)}>
                  <Trash2 className="size-4" />
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
