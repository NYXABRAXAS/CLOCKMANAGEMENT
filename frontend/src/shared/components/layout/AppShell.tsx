import { Outlet } from "react-router";
import { useAppDispatch, useAppSelector } from "@/app/hooks";
import { sidebarSet } from "@/app/uiSlice";
import { DesktopSidebar, MobileSidebar } from "./Sidebar";
import { Topbar } from "./Topbar";

export function AppShell() {
  const dispatch = useAppDispatch();
  const sidebarOpen = useAppSelector((s) => s.ui.sidebarOpen);

  return (
    <div className="flex min-h-screen">
      <DesktopSidebar />
      <MobileSidebar open={sidebarOpen} onClose={() => dispatch(sidebarSet(false))} />
      <div className="flex min-w-0 flex-1 flex-col">
        <Topbar />
        <main className="flex-1 overflow-y-auto bg-muted/30 p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
