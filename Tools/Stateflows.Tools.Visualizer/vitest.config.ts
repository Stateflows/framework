import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: { include: ['development/**/*.test.ts', 'src/**/*.test.ts'], maxWorkers: 1 }
});
