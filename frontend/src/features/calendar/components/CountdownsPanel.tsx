import * as React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Loader2, Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { eventCountdownsApi } from "../api/eventCountdownsApi";
import { toApiError } from "@/shared/lib/apiClient";

function daysUntil(targetDate: string) {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const target = new Date(`${targetDate}T00:00:00`);
  return Math.round((target.getTime() - today.getTime()) / 86_400_000);
}

export function CountdownsPanel() {
  const queryClient = useQueryClient();
  const { data: countdowns, isLoading } = useQuery({ queryKey: ["eventCountdowns"], queryFn: eventCountdownsApi.list });
  const [open, setOpen] = React.useState(false);
  const [title, setTitle] = React.useState("");
  const [targetDate, setTargetDate] = React.useState("");
  const [emoji, setEmoji] = React.useState("");
  const [saving, setSaving] = React.useState(false);

  const onAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim() || !targetDate) return;
    setSaving(true);
    try {
      await eventCountdownsApi.create({ title: title.trim(), targetDate, emoji: emoji || null, color: null });
      await queryClient.invalidateQueries({ queryKey: ["eventCountdowns"] });
      setTitle("");
      setTargetDate("");
      setEmoji("");
      setOpen(false);
      toast.success("Countdown added.");
    } catch (err) {
      toast.error(toApiError(err).message);
    } finally {
      setSaving(false);
    }
  };

  const onDelete = async (id: string) => {
    try {
      await eventCountdownsApi.remove(id);
      await queryClient.invalidateQueries({ queryKey: ["eventCountdowns"] });
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  return (
    <Card>
      <CardHeader className="flex-row items-center justify-between space-y-0">
        <CardTitle>Countdowns</CardTitle>
        <Dialog open={open} onOpenChange={setOpen}>
          <DialogTrigger asChild>
            <Button variant="outline" size="icon" className="size-8">
              <Plus className="size-4" />
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>New countdown</DialogTitle>
            </DialogHeader>
            <form onSubmit={onAdd} className="flex flex-col gap-4">
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="cd-title">Title</Label>
                <Input id="cd-title" value={title} onChange={(e) => setTitle(e.target.value)} placeholder="e.g. Birthday" required />
              </div>
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="cd-date">Target date</Label>
                <Input id="cd-date" type="date" value={targetDate} onChange={(e) => setTargetDate(e.target.value)} required />
              </div>
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="cd-emoji">Emoji (optional)</Label>
                <Input id="cd-emoji" value={emoji} onChange={(e) => setEmoji(e.target.value)} placeholder="🎂" maxLength={4} />
              </div>
              <DialogFooter>
                <Button type="submit" disabled={saving}>
                  {saving && <Loader2 className="animate-spin" />}
                  Add
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      </CardHeader>
      <CardContent className="flex flex-col gap-2">
        {isLoading && <Loader2 className="mx-auto size-5 animate-spin text-muted-foreground" />}
        {!isLoading && countdowns?.length === 0 && <p className="text-sm text-muted-foreground">No countdowns yet.</p>}
        {countdowns?.map((c) => {
          const days = daysUntil(c.targetDate);
          return (
            <div key={c.id} className="flex items-center justify-between rounded-md border p-2.5">
              <div className="flex items-center gap-2">
                {c.emoji && <span className="text-lg">{c.emoji}</span>}
                <div>
                  <p className="text-sm font-medium">{c.title}</p>
                  <p className="text-xs text-muted-foreground">
                    {days === 0 ? "Today!" : days > 0 ? `${days} day${days === 1 ? "" : "s"} to go` : `${-days} day${days === -1 ? "" : "s"} ago`}
                  </p>
                </div>
              </div>
              <Button variant="ghost" size="icon" className="size-7" onClick={() => onDelete(c.id)}>
                <Trash2 className="size-3.5" />
              </Button>
            </div>
          );
        })}
      </CardContent>
    </Card>
  );
}
