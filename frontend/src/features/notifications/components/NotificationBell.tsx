import { Bell, CheckCheck } from "lucide-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { notificationsApi } from "../api/notificationsApi";

const POLL_INTERVAL_MS = 30_000;

function timeAgo(iso: string) {
  const seconds = Math.floor((Date.now() - new Date(iso).getTime()) / 1000);
  if (seconds < 60) return "just now";
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  return `${Math.floor(hours / 24)}d ago`;
}

export function NotificationBell() {
  const queryClient = useQueryClient();
  const { data: unreadCount } = useQuery({
    queryKey: ["notifications", "unread-count"],
    queryFn: notificationsApi.getUnreadCount,
    refetchInterval: POLL_INTERVAL_MS,
  });
  const { data: notifications } = useQuery({
    queryKey: ["notifications", "list"],
    queryFn: notificationsApi.getNotifications,
    refetchInterval: POLL_INTERVAL_MS,
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["notifications"] });
  };

  const onMarkRead = async (id: string) => {
    await notificationsApi.markRead(id);
    invalidate();
  };

  const onMarkAllRead = async () => {
    await notificationsApi.markAllRead();
    invalidate();
  };

  const hasUnread = (unreadCount ?? 0) > 0;

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="relative">
          <Bell className="size-5" />
          {hasUnread && (
            <span className="absolute right-1.5 top-1.5 flex size-2 rounded-full bg-red-500" />
          )}
          <span className="sr-only">Notifications</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-80">
        <DropdownMenuLabel className="flex items-center justify-between">
          <span>Notifications</span>
          {hasUnread && (
            <button
              type="button"
              onClick={onMarkAllRead}
              className="flex items-center gap-1 text-xs font-normal text-muted-foreground hover:text-foreground"
            >
              <CheckCheck className="size-3.5" /> Mark all read
            </button>
          )}
        </DropdownMenuLabel>
        <DropdownMenuSeparator />
        {!notifications || notifications.length === 0 ? (
          <p className="px-2 py-4 text-center text-sm text-muted-foreground">No notifications yet.</p>
        ) : (
          <div className="flex max-h-80 flex-col overflow-y-auto">
            {notifications.map((n) => (
              <DropdownMenuItem
                key={n.id}
                className="flex flex-col items-start gap-0.5 whitespace-normal"
                onClick={() => !n.isRead && onMarkRead(n.id)}
              >
                <div className="flex w-full items-center justify-between gap-2">
                  <span className={`text-sm ${n.isRead ? "font-normal" : "font-semibold"}`}>{n.title}</span>
                  {!n.isRead && <span className="size-1.5 shrink-0 rounded-full bg-red-500" />}
                </div>
                <span className="text-xs text-muted-foreground">{n.message}</span>
                <span className="text-[11px] text-muted-foreground">{timeAgo(n.createdAt)}</span>
              </DropdownMenuItem>
            ))}
          </div>
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
