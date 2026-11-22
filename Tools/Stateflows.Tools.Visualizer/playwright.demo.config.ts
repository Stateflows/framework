import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  testMatch: 'executable.spec.ts',
  workers: 1,
  timeout: 60_000,
  use: {
    baseURL: 'http://127.0.0.1:5167',
    viewport: { width: 1600, height: 1000 },
    launchOptions: { executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE }
  },
  webServer: {
    command: 'dotnet run --project ..\\Stateflows.Tools.Visualizer.DevHost\\Stateflows.Tools.Visualizer.DevHost.csproj --launch-profile Visualizer -p:GeneratePackageOnBuild=false',
    url: 'http://127.0.0.1:5167/api/definitions',
    timeout: 120_000,
    reuseExistingServer: false
  }
});
