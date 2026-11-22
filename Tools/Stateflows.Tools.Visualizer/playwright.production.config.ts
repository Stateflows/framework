import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  testMatch: 'production.spec.ts',
  workers: 1,
  timeout: 120_000,
  use: {
    baseURL: process.env.PRODUCTION_VISUALIZER_URL ?? 'http://127.0.0.1:5168',
    viewport: { width: 1600, height: 1000 },
    launchOptions: { executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE }
  }
});
