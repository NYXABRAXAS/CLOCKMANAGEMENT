import { createBrowserRouter, Navigate } from "react-router";
import { ProtectedRoute, GuestOnlyRoute } from "./ProtectedRoute";
import { AppShell } from "@/shared/components/layout/AppShell";
import LoginPage from "@/features/auth/pages/LoginPage";
import RegisterPage from "@/features/auth/pages/RegisterPage";
import ForgotPasswordPage from "@/features/auth/pages/ForgotPasswordPage";
import ResetPasswordPage from "@/features/auth/pages/ResetPasswordPage";
import VerifyEmailPage from "@/features/auth/pages/VerifyEmailPage";
import DashboardPage from "@/features/dashboard/pages/DashboardPage";
import SettingsPage from "@/features/settings/pages/SettingsPage";
import ProfilePage from "@/features/profile/pages/ProfilePage";
import WorldClockPage from "@/features/worldClock/pages/WorldClockPage";
import TimezoneConverterPage from "@/features/worldClock/pages/TimezoneConverterPage";

export const router = createBrowserRouter([
  {
    element: <GuestOnlyRoute />,
    children: [
      { path: "/login", element: <LoginPage /> },
      { path: "/register", element: <RegisterPage /> },
      { path: "/forgot-password", element: <ForgotPasswordPage /> },
      { path: "/reset-password/:token", element: <ResetPasswordPage /> },
    ],
  },
  { path: "/verify-email/:token", element: <VerifyEmailPage /> },
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <AppShell />,
        children: [
          { path: "/dashboard", element: <DashboardPage /> },
          { path: "/world-clock", element: <WorldClockPage /> },
          { path: "/timezone-converter", element: <TimezoneConverterPage /> },
          { path: "/settings", element: <SettingsPage /> },
          { path: "/profile", element: <ProfilePage /> },
        ],
      },
    ],
  },
  { path: "/", element: <Navigate to="/dashboard" replace /> },
  { path: "*", element: <Navigate to="/dashboard" replace /> },
]);
