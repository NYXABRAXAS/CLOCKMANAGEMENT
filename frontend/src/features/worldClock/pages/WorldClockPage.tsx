import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Loader2 } from "lucide-react";
import { worldClockApi } from "../api/worldClockApi";
import { CityClockCard } from "../components/CityClockCard";
import { AddCityDialog } from "../components/AddCityDialog";
import { toApiError } from "@/shared/lib/apiClient";

export default function WorldClockPage() {
  const queryClient = useQueryClient();
  const { data: pins, isLoading } = useQuery({ queryKey: ["worldClockCities"], queryFn: worldClockApi.getPinnedCities });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["worldClockCities"] });

  const onRemove = async (id: string) => {
    try {
      await worldClockApi.removeCity(id);
      await invalidate();
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const onReorder = async (ids: string[]) => {
    try {
      await worldClockApi.reorder(ids);
      await invalidate();
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  const move = (index: number, direction: -1 | 1) => {
    if (!pins) return;
    const targetIndex = index + direction;
    if (targetIndex < 0 || targetIndex >= pins.length) return;
    const ids = pins.map((p) => p.id);
    [ids[index], ids[targetIndex]] = [ids[targetIndex], ids[index]];
    onReorder(ids);
  };

  return (
    <div className="mx-auto flex max-w-5xl flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">World Clock</h1>
          <p className="text-sm text-muted-foreground">Pin cities to see their current time, date, and sunrise/sunset at a glance.</p>
        </div>
        <AddCityDialog />
      </div>

      {isLoading && (
        <div className="flex justify-center py-12">
          <Loader2 className="size-8 animate-spin text-muted-foreground" />
        </div>
      )}

      {!isLoading && pins?.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center text-sm text-muted-foreground">
          No cities pinned yet. Click "Add city" to get started.
        </div>
      )}

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        {pins?.map((pin, index) => (
          <CityClockCard
            key={pin.id}
            pin={pin}
            onRemove={() => onRemove(pin.id)}
            onMoveUp={() => move(index, -1)}
            onMoveDown={() => move(index, 1)}
            canMoveUp={index > 0}
            canMoveDown={index < pins.length - 1}
          />
        ))}
      </div>
    </div>
  );
}
