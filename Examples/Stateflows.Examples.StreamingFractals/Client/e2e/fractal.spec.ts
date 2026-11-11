import { expect, test } from "@playwright/test";
import type { Page } from "@playwright/test";
import { easeParameter } from "../src/model.ts";
import type { TileRequest } from "../src/model.ts";

const reached = (page: Page) => expect(page.locator("#status")).toHaveText("Target reached. Move to explore.");
const image = (page: Page) => page.locator("canvas").evaluate(canvas => (canvas as HTMLCanvasElement).toDataURL());
async function stop(page: Page) {
  await page.locator("#stop").click();
  await expect(page.locator("#connection")).toHaveText("Stopped", { timeout: 60000 });
  await expect(page.locator("#in-flight")).toHaveText("0 / 4");
}

test("target settle delay coalesces input; zero releases it and reduced motion skips easing", async ({ page }) => {
  await page.emulateMedia({ reducedMotion: "reduce" });
  const tiles: TileRequest[] = [];
  page.on("request", request => {
    if (request.url().includes("/input/tileRequest")) tiles.push(request.postDataJSON().tokens[0]);
  });
  await page.goto("/");
  await reached(page);
  expect(tiles.length).toBe(24);
  await page.locator("#delay").fill("1000");
  await expect(page.locator("#delay-value")).toHaveText("1000 ms");
  await page.locator("#real").fill("0.1");
  await page.waitForTimeout(300);
  expect(tiles.length).toBe(24);
  await page.locator("#real").fill("0.2");
  await page.locator("#delay").fill("0");
  await reached(page);
  await expect(page.locator("canvas")).toHaveAttribute("data-real", "0.2");
  expect(tiles.length).toBe(48);
  expect(tiles.slice(24).every(tile => tile.real === 0.2 && tile.step === 1)).toBe(true);
  await page.waitForTimeout(300);
  expect(tiles.length).toBe(48);
  await stop(page);
});

test("full-resolution frames present atomically; palette touches only displayed image; pause finishes one frame", async ({ page }) => {
  await page.goto("/");
  await reached(page);
  const original = await image(page);
  let release!: () => void;
  const gate = new Promise<void>(resolve => { release = resolve; });
  const tiles: TileRequest[] = [];
  await page.route("**/input/tileRequest?sendAsStream=true", async route => {
    const tile = route.request().postDataJSON().tokens[0] as TileRequest;
    tiles.push(tile);
    if (tile.tileId === 23) await gate;
    await route.continue();
  });
  try {
    await page.locator("#delay").fill("0");
    await page.locator("#real").fill("0.8");
    await expect(page.locator("#progress")).toHaveAttribute("style", /95\.833/);
    expect(await image(page)).toBe(original);
    await expect(page.locator("canvas")).toHaveAttribute("data-generation", "1");
    expect(tiles.length).toBe(24);
    expect(tiles.every(tile => tile.step === 1 && tile.imageWidth === 768 && tile.imageHeight === 512)).toBe(true);
    const eased = easeParameter({ real: -0.745, imaginary: 0.186 }, { real: 0.8, imaginary: 0.186 });
    expect(tiles.every(tile => tile.real === eased.real && tile.imaginary === eased.imaginary)).toBe(true);
    await page.selectOption("#palette", "ember");
    await expect.poll(() => image(page)).not.toBe(original);
    const recolored = await image(page);
    await page.selectOption("#palette", "aurora");
    expect(await image(page)).toBe(original);
    await page.selectOption("#palette", "ember");
    expect(await image(page)).toBe(recolored);
    await page.locator("#pause").click();
    await expect(page.locator("#status")).toHaveText("Input paused. Finishing the current full frame.");
    release();
    await expect(page.locator("#status")).toHaveText("Input paused. Animation held.");
    await expect(page.locator("canvas")).toHaveAttribute("data-real", String(eased.real));
    expect(await image(page)).not.toBe(recolored);
    await expect(page.locator("#frame-time")).toHaveText(/\d+ ms/);
    await page.waitForTimeout(300);
    expect(tiles.length).toBe(24);
    await page.locator("#real").fill("-0.4");
    await expect(page.locator("#real")).toHaveValue("-0.4");
    await page.waitForTimeout(100);
    expect(tiles.length).toBe(24);
    await page.locator("#pause").click();
    await reached(page);
    await expect(page.locator("canvas")).toHaveAttribute("data-real", "-0.4");
    await stop(page);
  } finally {
    release();
  }
});

test("retargets newest destination without future-frame backlog, converges exactly, and configures/restarts", async ({ page }) => {
  const errors: string[] = [];
  const tiles: TileRequest[] = [];
  let configurations = 0;
  page.on("pageerror", error => errors.push(error.message));
  page.on("request", request => {
    if (request.url().includes("/input/tileRequest")) tiles.push(request.postDataJSON().tokens[0]);
    if (request.url().includes("/configureRendering")) configurations++;
  });
  await page.goto("/");
  await reached(page);
  await expect(page.locator("#mode")).toHaveText("Streamed");
  expect(Number.parseInt((await page.locator("#latency").textContent())!)).toBeLessThan(3000);
  const colors = await page.locator("canvas").evaluate(canvas => {
    const data = (canvas as HTMLCanvasElement).getContext("2d")!.getImageData(0, 0, 768, 512).data;
    const unique = new Set<number>();
    for (let index = 0; index < data.length; index += 4) unique.add((data[index] << 16) | (data[index + 1] << 8) | data[index + 2]);
    return unique.size;
  });
  expect(colors).toBeGreaterThan(20);
  let release!: () => void;
  const gate = new Promise<void>(resolve => { release = resolve; });
  await page.route("**/input/tileRequest?sendAsStream=true", async route => {
    const tile = route.request().postDataJSON().tokens[0] as TileRequest;
    if (tile.generation === 2) await gate;
    await route.continue();
  });
  try {
    await page.locator("#delay").fill("0");
    await page.locator("#real").fill("0.8");
    await expect.poll(() => tiles.length).toBe(28);
    await page.locator("#real").fill("1.2");
    await page.locator("#real").fill("-0.2");
    await page.locator("#imaginary").fill("0.4");
    await page.waitForTimeout(200);
    expect(tiles.length).toBe(28);
    await expect(page.locator("#in-flight")).toHaveText("4 / 4");
    await expect(page.locator("#generation")).toHaveText("2");
    release();
    await reached(page);
    await expect(page.locator("canvas")).toHaveAttribute("data-real", "-0.2");
    await expect(page.locator("canvas")).toHaveAttribute("data-imaginary", "0.4");
    await expect(page.locator("#real")).toHaveValue("-0.2");
    await expect(page.locator("#imaginary")).toHaveValue("0.4");
    const generations = new Map<number, TileRequest[]>();
    for (const tile of tiles) {
      const frame = generations.get(tile.generation) ?? [];
      frame.push(tile);
      generations.set(tile.generation, frame);
    }
    let previous = { real: -0.745, imaginary: 0.186 };
    for (const [generation, frame] of generations) {
      expect(frame.length).toBe(24);
      expect(frame.every(tile => tile.step === 1 && tile.real === frame[0].real && tile.imaginary === frame[0].imaginary)).toBe(true);
      const expected = generation === 1 ? previous : easeParameter(previous,
        generation === 2 ? { real: 0.8, imaginary: 0.186 } : { real: -0.2, imaginary: 0.4 });
      expect({ real: frame[0].real, imaginary: frame[0].imaginary }).toEqual(expected);
      previous = expected;
    }
    const count = tiles.length;
    await page.selectOption("#palette", "ocean");
    await page.waitForTimeout(300);
    expect(tiles.length).toBe(count);
    await page.locator("#pause").click();
    await page.locator("#detail").fill("512");
    await page.locator("#detail").dispatchEvent("change");
    await expect.poll(() => configurations).toBe(2);
    await page.waitForTimeout(200);
    expect(tiles.length).toBe(count);
    await page.locator("#pause").click();
    await expect.poll(() => tiles.length).toBe(count + 24);
    await reached(page);
    await expect(page.locator("#detail-value")).toHaveText("512 iterations");
    await stop(page);
    await page.locator("#restart").click();
    await expect(page.locator("#connection")).toHaveText("Streaming");
    await reached(page);
    await stop(page);
    expect(errors).toEqual([]);
  } finally {
    release();
  }
});

test("pointer controls, stop during admission, and ended-stream recovery use independent sessions", async ({ page }) => {
  await page.emulateMedia({ reducedMotion: "reduce" });
  await page.goto("/");
  await reached(page);
  const sessions = new Set<string>();
  const tiles: TileRequest[] = [];
  page.on("request", request => {
    if (request.url().includes("/input/tileRequest")) {
      sessions.add(new URL(request.url()).pathname.split("/")[4]);
      tiles.push(request.postDataJSON().tokens[0]);
    }
  });
  const box = await page.locator("canvas").boundingBox();
  if (!box) throw new Error("Missing canvas bounds.");
  await page.locator("#delay").fill("0");
  await page.mouse.move(box.x + box.width * 0.3, box.y + box.height * 0.6);
  await expect.poll(() => tiles.length).toBeGreaterThan(0);
  await stop(page);
  await page.route("**/output/tileResult?stream=true", route => route.fulfill({
    status: 200, contentType: "text/event-stream", body: ": stream ended\n\n",
  }));
  await page.locator("#restart").click();
  await expect(page.locator("#connection")).toHaveText("Session interrupted");
  await expect(page.locator("#pause")).toBeDisabled();
  await expect(page.locator("#status")).toContainText(/stream/i);
  await page.unroute("**/output/tileResult?stream=true");
  await expect(page.locator("#restart")).toBeEnabled({ timeout: 60000 });
  await page.locator("#restart").click();
  await expect(page.locator("#connection")).toHaveText("Streaming");
  await reached(page);
  expect(sessions.size).toBe(2);
  await stop(page);
});
