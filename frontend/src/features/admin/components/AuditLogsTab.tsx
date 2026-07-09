import * as React from "react";
import { useQuery } from "@tanstack/react-query";
import { toast } from "sonner";
import { ChevronLeft, ChevronRight, Download, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { adminApi } from "../api/adminApi";
import { toApiError } from "@/shared/lib/apiClient";

const PAGE_SIZE = 25;

export function AuditLogsTab() {
  const [page, setPage] = React.useState(1);
  const { data, isLoading } = useQuery({
    queryKey: ["admin", "audit-logs", page],
    queryFn: () => adminApi.getAuditLogs({ page, pageSize: PAGE_SIZE }),
  });

  const totalPages = data ? Math.max(1, Math.ceil(data.totalCount / PAGE_SIZE)) : 1;

  const onExport = async () => {
    try {
      await adminApi.exportAuditLogsCsv();
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
      <div className="flex justify-end">
        <Button variant="outline" onClick={onExport}>
          <Download /> Export CSV
        </Button>
      </div>

      {data?.items.length === 0 ? (
        <p className="py-8 text-center text-sm text-muted-foreground">
          No audit log entries yet - they appear here as admin actions (role changes, account activation, etc.) are performed.
        </p>
      ) : (
        <div className="flex flex-col gap-2">
          {data?.items.map((log) => (
            <Card key={log.id}>
              <CardContent className="flex flex-wrap items-center justify-between gap-2 py-3 text-sm">
                <div className="flex flex-col">
                  <span className="font-medium">
                    {log.action} <span className="font-normal text-muted-foreground">{log.entityType}</span>
                  </span>
                  {log.description && <span className="text-xs text-muted-foreground">{log.description}</span>}
                </div>
                <div className="flex flex-col items-end text-xs text-muted-foreground">
                  <span>{log.actorEmail ?? "System"}</span>
                  <span>{new Date(log.createdAt).toLocaleString()}</span>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {data && data.totalCount > PAGE_SIZE && (
        <div className="flex items-center justify-center gap-3">
          <Button variant="outline" size="icon" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
            <ChevronLeft className="size-4" />
          </Button>
          <span className="text-sm text-muted-foreground">
            Page {page} of {totalPages}
          </span>
          <Button variant="outline" size="icon" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>
            <ChevronRight className="size-4" />
          </Button>
        </div>
      )}
    </div>
  );
}
