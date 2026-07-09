import * as React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Award, Download, Flame, Loader2, Pencil, Plus, Trash2, Trophy } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { habitsApi } from "../api/habitsApi";
import { achievementsApi } from "../api/achievementsApi";
import { HabitFormDialog } from "../components/HabitFormDialog";
import { describeRepeatMask } from "@/shared/lib/alarmDays";
import { toApiError } from "@/shared/lib/apiClient";
import type { Habit } from "@/types/health";

function todayIso() {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}

export default function HabitsPage() {
  const queryClient = useQueryClient();
  const { data: habits, isLoading } = useQuery({ queryKey: ["habits"], queryFn: habitsApi.list });
  const { data: achievements } = useQuery({ queryKey: ["achievements"], queryFn: achievementsApi.mine });
  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [editingHabit, setEditingHabit] = React.useState<Habit | null>(null);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["habits"] });

  const openCreate = () => {
    setEditingHabit(null);
    setDialogOpen(true);
  };
  const openEdit = (habit: Habit) => {
    setEditingHabit(habit);
    setDialogOpen(true);
  };

  const onSave: React.ComponentProps<typeof HabitFormDialog>["onSave"] = async (id, payload) => {
    if (id) {
      await habitsApi.update(id, { ...payload, isActive: habits?.find((h) => h.id === id)?.isActive ?? true });
      toast.success("Habit updated.");
    } else {
      await habitsApi.create(payload);
      toast.success("Habit created.");
    }
    await invalidate();
  };

  const onToggleToday = async (habit: Habit) => {
    try {
      const result = await habitsApi.toggleLog(habit.id, todayIso(), !habit.completedToday);
      await invalidate();
      if (result.newlyEarnedAchievementCodes.length > 0) {
        const freshAchievements = await queryClient.fetchQuery({ queryKey: ["achievements"], queryFn: achievementsApi.mine });
        for (const code of result.newlyEarnedAchievementCodes) {
          const a = freshAchievements.find((x) => x.code === code);
          toast.success(`Achievement unlocked: ${a ? `${a.emoji ?? "🏆"} ${a.title}` : code}`, { duration: 6000 });
        }
      }
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const onDelete = async (habit: Habit) => {
    try {
      await habitsApi.remove(habit.id);
      await invalidate();
      toast.success("Habit deleted.");
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const onExport = async () => {
    try {
      await habitsApi.exportCsv();
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Habits</h1>
          <p className="text-sm text-muted-foreground">Check in daily and build streaks.</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={onExport}>
            <Download /> Export CSV
          </Button>
          <Button onClick={openCreate}>
            <Plus /> New habit
          </Button>
        </div>
      </div>

      {isLoading && (
        <div className="flex justify-center py-12">
          <Loader2 className="size-8 animate-spin text-muted-foreground" />
        </div>
      )}

      {!isLoading && habits?.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center text-sm text-muted-foreground">
          No habits yet. Click "New habit" to start tracking one.
        </div>
      )}

      <div className="flex flex-col gap-3">
        {habits?.map((habit) => (
          <Card key={habit.id}>
            <CardContent className="flex items-center justify-between gap-3 pt-5">
              <button
                type="button"
                onClick={() => onToggleToday(habit)}
                className={`flex size-11 shrink-0 items-center justify-center rounded-full border-2 text-xl transition-colors ${
                  habit.completedToday ? "border-primary bg-primary/10" : "border-input"
                }`}
                style={habit.completedToday ? { borderColor: habit.color ?? undefined } : undefined}
              >
                {habit.emoji ?? "✅"}
              </button>
              <div className="min-w-0 flex-1">
                <p className="truncate font-medium">{habit.title}</p>
                <p className="text-xs text-muted-foreground">{describeRepeatMask(habit.repeatDaysMask)}</p>
              </div>
              <div className="flex items-center gap-3 text-sm text-muted-foreground">
                <span className="flex items-center gap-1">
                  <Flame className="size-4 text-orange-500" /> {habit.currentStreak}
                </span>
                <span className="flex items-center gap-1">
                  <Trophy className="size-4 text-amber-500" /> {habit.longestStreak}
                </span>
              </div>
              <div className="flex items-center gap-0.5">
                <Button variant="ghost" size="icon" onClick={() => openEdit(habit)}>
                  <Pencil className="size-4" />
                </Button>
                <Button variant="ghost" size="icon" onClick={() => onDelete(habit)}>
                  <Trash2 className="size-4" />
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {achievements && achievements.length > 0 && (
        <Card>
          <CardContent className="flex flex-wrap gap-3 pt-5">
            {achievements.map((a) => (
              <div key={a.code} className="flex items-center gap-2 rounded-full border px-3 py-1.5 text-sm">
                <Award className="size-4 text-amber-500" />
                <span>
                  {a.emoji} {a.title}
                </span>
              </div>
            ))}
          </CardContent>
        </Card>
      )}

      <HabitFormDialog open={dialogOpen} onOpenChange={setDialogOpen} habit={editingHabit} onSave={onSave} />
    </div>
  );
}
