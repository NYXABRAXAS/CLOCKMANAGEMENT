import * as React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Loader2, Pencil, Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useAppSelector } from "@/app/hooks";
import { religionsApi } from "@/features/religions/api/religionsApi";
import { toApiError } from "@/shared/lib/apiClient";
import type { Religion } from "@/types/religion";

export function ReligionsTab() {
  const currentUser = useAppSelector((s) => s.auth.user);
  const canCreate = currentUser?.permissions.includes("RELIGIONS:create") ?? false;
  const canEdit = currentUser?.permissions.includes("RELIGIONS:edit") ?? false;
  const canDelete = currentUser?.permissions.includes("RELIGIONS:delete") ?? false;

  const queryClient = useQueryClient();
  const { data: religions, isLoading } = useQuery({ queryKey: ["religions"], queryFn: religionsApi.getReligions });

  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [editing, setEditing] = React.useState<Religion | null>(null);
  const [code, setCode] = React.useState("");
  const [name, setName] = React.useState("");
  const [sortOrder, setSortOrder] = React.useState(0);
  const [saving, setSaving] = React.useState(false);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["religions"] });

  const openCreate = () => {
    setEditing(null);
    setCode("");
    setName("");
    setSortOrder((religions?.length ?? 0) + 1);
    setDialogOpen(true);
  };

  const openEdit = (religion: Religion) => {
    setEditing(religion);
    setCode(religion.code);
    setName(religion.name);
    setSortOrder(religion.sortOrder);
    setDialogOpen(true);
  };

  const onSave = async () => {
    setSaving(true);
    try {
      if (editing) {
        await religionsApi.updateReligion(editing.id, { name, sortOrder });
        toast.success("Religion updated.");
      } else {
        await religionsApi.createReligion({ code, name, sortOrder });
        toast.success("Religion created.");
      }
      setDialogOpen(false);
      invalidate();
    } catch (err) {
      toast.error(toApiError(err).message);
    } finally {
      setSaving(false);
    }
  };

  const onDelete = async (religion: Religion) => {
    try {
      await religionsApi.deleteReligion(religion.id);
      toast.success("Religion deleted.");
      invalidate();
    } catch (err) {
      toast.error(toApiError(err).message);
    }
  };

  if (isLoading) {
    return (
      <div className="flex justify-center py-8">
        <Loader2 className="size-6 animate-spin text-muted-foreground" />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-4">
      {canCreate && (
        <Button variant="outline" className="w-fit" onClick={openCreate}>
          <Plus /> Add religion
        </Button>
      )}

      <div className="flex flex-col gap-2">
        {religions?.map((r) => (
          <Card key={r.id}>
            <CardContent className="flex items-center justify-between gap-2 py-3">
              <div>
                <span className="text-sm font-medium">{r.name}</span>
                <span className="ml-2 text-xs text-muted-foreground">{r.code}</span>
              </div>
              <div className="flex gap-2">
                {canEdit && (
                  <Button size="icon" variant="outline" className="size-8" onClick={() => openEdit(r)}>
                    <Pencil className="size-3.5" />
                  </Button>
                )}
                {canDelete && (
                  <Button size="icon" variant="outline" className="size-8" onClick={() => onDelete(r)}>
                    <Trash2 className="size-3.5" />
                  </Button>
                )}
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "Edit religion" : "Add religion"}</DialogTitle>
          </DialogHeader>
          <div className="flex flex-col gap-4">
            <div className="flex flex-col gap-1.5">
              <Label>Code</Label>
              <Input
                value={code}
                onChange={(e) => setCode(e.target.value.toUpperCase())}
                disabled={!!editing}
                placeholder="e.g. ZOROASTRIANISM"
              />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label>Name</Label>
              <Input value={name} onChange={(e) => setName(e.target.value)} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label>Sort order</Label>
              <Input type="number" value={sortOrder} onChange={(e) => setSortOrder(Number(e.target.value))} />
            </div>
          </div>
          <DialogFooter>
            <Button onClick={onSave} disabled={saving || !name || (!editing && !code)}>
              {saving && <Loader2 className="animate-spin" />}
              Save
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
