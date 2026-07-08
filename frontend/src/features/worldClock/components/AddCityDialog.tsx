import * as React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Loader2, Plus, Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { citiesApi } from "../api/citiesApi";
import { worldClockApi } from "../api/worldClockApi";
import { toApiError } from "@/shared/lib/apiClient";

export function AddCityDialog() {
  const [open, setOpen] = React.useState(false);
  const [search, setSearch] = React.useState("");
  const [debouncedSearch, setDebouncedSearch] = React.useState("");
  const [addingId, setAddingId] = React.useState<string | null>(null);
  const queryClient = useQueryClient();

  React.useEffect(() => {
    const id = setTimeout(() => setDebouncedSearch(search), 250);
    return () => clearTimeout(id);
  }, [search]);

  const { data: cities, isFetching } = useQuery({
    queryKey: ["cities", debouncedSearch],
    queryFn: () => citiesApi.search(debouncedSearch),
    enabled: open,
  });

  const onAdd = async (cityId: string) => {
    setAddingId(cityId);
    try {
      await worldClockApi.addCity(cityId);
      await queryClient.invalidateQueries({ queryKey: ["worldClockCities"] });
      toast.success("City added.");
    } catch (err) {
      toast.error(toApiError(err).message);
    } finally {
      setAddingId(null);
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <Plus /> Add city
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Add a city</DialogTitle>
        </DialogHeader>
        <div className="relative">
          <Search className="absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
          <Input placeholder="Search city or country..." className="pl-9" value={search} onChange={(e) => setSearch(e.target.value)} autoFocus />
        </div>
        <div className="max-h-80 overflow-y-auto">
          {isFetching && (
            <div className="flex justify-center py-6">
              <Loader2 className="size-5 animate-spin text-muted-foreground" />
            </div>
          )}
          {!isFetching && cities?.length === 0 && <p className="py-6 text-center text-sm text-muted-foreground">No cities found.</p>}
          <div className="flex flex-col">
            {cities?.map((city) => (
              <button
                key={city.id}
                type="button"
                disabled={addingId === city.id}
                onClick={() => onAdd(city.id)}
                className="flex items-center justify-between rounded-md px-3 py-2 text-left text-sm hover:bg-accent disabled:opacity-50"
              >
                <span>
                  {city.name} <span className="text-muted-foreground">- {city.country}</span>
                </span>
                {addingId === city.id ? <Loader2 className="size-4 animate-spin" /> : <Plus className="size-4 text-muted-foreground" />}
              </button>
            ))}
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
