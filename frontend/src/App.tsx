import { useEffect } from "react";
import { Provider } from "react-redux";
import { QueryClientProvider } from "@tanstack/react-query";
import { RouterProvider } from "react-router";
import { Toaster } from "sonner";
import { store } from "@/app/store";
import { useAppDispatch } from "@/app/hooks";
import { queryClient } from "@/shared/lib/queryClient";
import { ThemeProvider } from "@/shared/components/ThemeProvider";
import { router } from "@/routes/router";
import { bootstrapAuth } from "@/features/auth/authSlice";

function AuthBootstrap() {
  const dispatch = useAppDispatch();
  useEffect(() => {
    dispatch(bootstrapAuth());
  }, [dispatch]);
  return null;
}

function App() {
  return (
    <Provider store={store}>
      <QueryClientProvider client={queryClient}>
        <ThemeProvider>
          <AuthBootstrap />
          <RouterProvider router={router} />
          <Toaster richColors position="top-right" />
        </ThemeProvider>
      </QueryClientProvider>
    </Provider>
  );
}

export default App;
