import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  testMatch: 'synthetic.spec.ts',
  workers: 1,
  timeout: 60_000,
  use: {
    baseURL: 'http://127.0.0.1:5178',
    viewport: { width: 1600, height: 1000 },
    launchOptions: { executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE }
  },
  webServer: {
    command: 'npm run dev -- --port 5178 --strictPort',
    url: 'http://127.0.0.1:5178',
    reuseExistingServer: false
  }
});
