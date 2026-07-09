import { test, expect } from "@playwright/test";

const API_BASE = "http://localhost:5080/api/v1";

async function registerAndVerifyViaApi(request: import("@playwright/test").APIRequestContext, email: string, password: string) {
  const registerResponse = await request.post(`${API_BASE}/auth/register`, {
    data: { firstName: "Playwright", lastName: "User", email, password },
  });
  expect(registerResponse.ok()).toBeTruthy();
  const { devOnlyVerificationToken } = await registerResponse.json();

  const verifyResponse = await request.post(`${API_BASE}/auth/verify-email`, { data: { token: devOnlyVerificationToken } });
  expect(verifyResponse.ok()).toBeTruthy();
}

// Registration/verification are driven directly via the API here (there's no way for a browser
// test to receive a real verification email, and the UI never displays the dev-only token - it's
// only ever in the API response body for exactly this kind of test/manual-check use). The part
// that's actually worth exercising through a real browser - login and landing on the dashboard -
// goes through the real UI.
test("user can log in after registering and verifying, and lands on the dashboard", async ({ page, request }) => {
  const email = `e2e-${Date.now()}@example.com`;
  const password = "Passw0rd!123";
  await registerAndVerifyViaApi(request, email, password);

  await page.goto("/login");
  await page.getByLabel("Email").fill(email);
  await page.getByLabel("Password").fill(password);
  await page.getByRole("button", { name: "Sign In" }).click();

  await expect(page).toHaveURL(/\/dashboard/, { timeout: 10_000 });
  await expect(page.getByRole("heading", { name: /welcome, playwright/i })).toBeVisible();
});

test("registering through the UI redirects to login on success", async ({ page }) => {
  const email = `e2e-${Date.now()}@example.com`;

  await page.goto("/register");
  await page.getByLabel("First name").fill("Playwright");
  await page.getByLabel("Last name").fill("User");
  await page.getByLabel("Email").fill(email);
  await page.getByLabel("Password", { exact: true }).fill("Passw0rd!123");
  await page.getByLabel("Confirm password").fill("Passw0rd!123");
  await page.getByRole("button", { name: "Create Account" }).click();

  await expect(page).toHaveURL(/\/login/, { timeout: 10_000 });
});

test("logging in with the wrong password fails and keeps the user on the login page", async ({ page, request }) => {
  const email = `e2e-${Date.now()}@example.com`;
  await registerAndVerifyViaApi(request, email, "Passw0rd!123");

  await page.goto("/login");
  await page.getByLabel("Email").fill(email);
  await page.getByLabel("Password").fill("TotallyWrongPassword1!");
  await page.getByRole("button", { name: "Sign In" }).click();

  // Give the request a moment to fail, then confirm we never left the login page.
  await page.waitForTimeout(1000);
  await expect(page).toHaveURL(/\/login/);
});

test("unauthenticated visitor hitting a protected route is redirected to login", async ({ page }) => {
  await page.goto("/dashboard");

  await expect(page).toHaveURL(/\/login/);
});
