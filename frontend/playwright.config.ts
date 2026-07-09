import { defineConfig, devices } from "@playwright/test";

const FRONTEND_URL = "http://localhost:5173";

// Real E2E: both the real Vite dev server and the real .NET API are started for the run (rather
// than mocking the API), so these tests exercise the same register/login/dashboard flow a real
// user does, against a throwaway SQLite dev database.
export default defineConfig({
  testDir: "./e2e",
  fullyParallel: false,
  retries: process.env.CI ? 1 : 0,
  reporter: "list",
  use: {
    baseURL: FRONTEND_URL,
    trace: "retain-on-failure",
  },
  projects: [{ name: "chromium", use: { ...devices["Desktop Chrome"] } }],
  webServer: [
    {
      command: "npm run dev",
      url: FRONTEND_URL,
      reuseExistingServer: !process.env.CI,
      timeout: 30_000,
    },
    {
      command: "dotnet run --project ../backend/src/STLMS.API/STLMS.API.csproj --urls http://localhost:5080",
      url: "http://localhost:5080/api/v1/health",
      reuseExistingServer: !process.env.CI,
      timeout: 60_000,
      env: { ASPNETCORE_ENVIRONMENT: "Development" },
    },
  ],
});
