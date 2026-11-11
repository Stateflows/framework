import { defineConfig } from "@playwright/test";

const baseURL = `http://localhost:${process.env.FRACTAL_TEST_PORT ?? "5158"}`;

export default defineConfig({
  testDir: "./e2e",
  workers: 1,
  timeout: 120000,
  expect: { timeout: 30000 },
  use: { baseURL, headless: true },
  webServer: {
    command: `dotnet run --project .. --configuration ${process.env.FRACTAL_TEST_CONFIGURATION ?? "Debug"} --no-launch-profile -- --urls ${baseURL}`,
    url: `${baseURL}/api/fractal`,
    env: {
      ASPNETCORE_ENVIRONMENT: "Development",
      Orleans__SiloPort: process.env.FRACTAL_TEST_SILO_PORT ?? "11128",
      Orleans__GatewayPort: process.env.FRACTAL_TEST_GATEWAY_PORT ?? "30018",
    },
    timeout: 120000,
  },
});
