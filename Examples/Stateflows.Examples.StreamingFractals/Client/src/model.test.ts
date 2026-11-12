import { strict as assert } from "node:assert";
import { test } from "node:test";
import { gzipSync } from "node:zlib";
import { color, concurrency, decodeIterations, easeParameter, easingEpsilon, easingFraction,
  imageHeight, imageWidth, inputSettleDelay, TileScheduler } from "./model.ts";

function finish(scheduler: TileScheduler, now = 1000) {
  const tiles = [];
  while (scheduler.active) {
    const batch = scheduler.take(now);
    assert.ok(batch.length > 0);
    assert.ok(scheduler.pending.size <= concurrency);
    tiles.push(...batch);
    for (const tile of batch) assert.equal(scheduler.complete(tile)?.current, true);
  }
  return tiles;
}

function initialized() {
  const scheduler = new TileScheduler();
  scheduler.retarget(0, 0, 0, 0);
  assert.equal(scheduler.begin(0), true);
  finish(scheduler);
  return scheduler;
}

test("one full-resolution frame covers every pixel exactly once with the same c", () => {
  const scheduler = new TileScheduler();
  scheduler.retarget(-0.745, 0.186, 0);
  assert.equal(scheduler.begin(inputSettleDelay - 1), false);
  assert.equal(scheduler.begin(inputSettleDelay), true);
  const tiles = finish(scheduler);
  const coverage = new Uint8Array(imageWidth * imageHeight);
  for (const tile of tiles) {
    assert.equal(tile.step, 1);
    assert.equal(tile.real, -0.745);
    assert.equal(tile.imaginary, 0.186);
    assert.equal(tile.imageWidth, 768);
    assert.equal(tile.imageHeight, 512);
    assert.ok(tile.width * tile.height <= 16384);
    for (let y = tile.y; y < tile.y + tile.height; y++)
      for (let x = tile.x; x < tile.x + tile.width; x++) coverage[y * imageWidth + x]++;
  }
  assert.equal(tiles.length, 24);
  assert.equal(scheduler.completed, 24);
  assert.equal(scheduler.total, 24);
  assert.ok(coverage.every(count => count === 1));
  assert.equal(scheduler.begin(10000), false);
});

test("retargeting midframe never changes its c or queues future frames", () => {
  const scheduler = initialized();
  scheduler.retarget(1, 1, 0, 0);
  scheduler.begin(0);
  const active = { ...scheduler.active! };
  const batch = scheduler.take(0);
  for (let index = 0; index < 100; index++) {
    scheduler.retarget(-index / 100, index / 100, 1, 0);
    assert.equal(scheduler.begin(100000), false);
  }
  assert.equal(scheduler.generation, 2);
  assert.equal(scheduler.take(100000).length, 0);
  assert.equal(scheduler.pending.size, 4);
  for (const tile of batch) scheduler.complete(tile);
  const remaining = finish(scheduler);
  assert.ok(remaining.every(tile => tile.real === active.real && tile.imaginary === active.imaginary));
  assert.deepEqual(scheduler.rendered, active);
  assert.equal(scheduler.begin(100000), true);
  assert.deepEqual(scheduler.active, easeParameter(active, { real: -0.99, imaginary: 0.99 }));
});

test("easing is monotonic, bounded, and cannot overshoot either coordinate", () => {
  for (const target of [{ real: 1.5, imaginary: -1.5 }, { real: -1.5, imaginary: 1.5 }, { real: 0, imaginary: 0 }]) {
    let from = { real: -0.745, imaginary: 0.186 };
    for (let index = 0; index < 100; index++) {
      const next = easeParameter(from, target);
      const before = Math.hypot(target.real - from.real, target.imaginary - from.imaginary);
      const after = Math.hypot(target.real - next.real, target.imaginary - next.imaginary);
      assert.ok(after <= before);
      for (const axis of ["real", "imaginary"] as const) {
        assert.ok(next[axis] >= Math.min(from[axis], target[axis]));
        assert.ok(next[axis] <= Math.max(from[axis], target[axis]));
      }
      if (before > easingEpsilon) assert.ok(Math.abs(after - before * (1 - easingFraction)) < 1e-14);
      from = next;
    }
    assert.deepEqual(from, target);
  }
});

test("epsilon is explicit, Euclidean, and snaps to the exact target", () => {
  const target = { real: 0, imaginary: 0 };
  assert.deepEqual(easeParameter({ real: easingEpsilon, imaginary: 0 }, target), target);
  assert.notDeepEqual(easeParameter({ real: easingEpsilon, imaginary: easingEpsilon }, target), target);
  assert.notDeepEqual(easeParameter({ real: easingEpsilon * 1.00001, imaginary: 0 }, target), target);
});

test("worst-range easing converges exactly and stops admitting within 20 frames", () => {
  const scheduler = new TileScheduler();
  scheduler.retarget(-1.5, -1.5, 0, 0);
  scheduler.begin(0);
  finish(scheduler);
  scheduler.retarget(1.5, 1.5, 0, 0);
  let frames = 0;
  while (scheduler.begin(0)) {
    finish(scheduler);
    assert.ok(++frames <= 20);
  }
  assert.deepEqual(scheduler.rendered, scheduler.target);
  assert.equal(scheduler.pending.size, 0);
  assert.equal(scheduler.begin(1e9), false);
  assert.deepEqual(scheduler.take(1e9), []);
});

test("slow frames advance by the same step, never wall-clock elapsed time", () => {
  const fast = initialized();
  const slow = initialized();
  for (const scheduler of [fast, slow]) scheduler.retarget(1, -1, 0, 0);
  fast.begin(0);
  slow.begin(1e9);
  assert.deepEqual(fast.active, slow.active);
  finish(fast, 10);
  finish(slow, 2e9);
  fast.begin(11);
  slow.begin(3e9);
  assert.deepEqual(fast.active, slow.active);
});

test("settle delay coalesces targets, but does not delay each animation frame", () => {
  const scheduler = initialized();
  scheduler.retarget(1, 1, 100, 1000);
  assert.equal(scheduler.begin(1099), false);
  scheduler.retarget(-1, 0.5, 500, 1000);
  assert.equal(scheduler.begin(1100), false);
  assert.equal(scheduler.begin(1500), true);
  finish(scheduler, 1500);
  assert.equal(scheduler.begin(1500), true);
  assert.ok(Math.abs(scheduler.active!.real - -0.5775) < 1e-14);
});

test("changing or disabling settle delay releases only latest destination", () => {
  const scheduler = initialized();
  scheduler.retarget(1, 1, 100, 1000);
  scheduler.settle(200, 0);
  assert.equal(scheduler.begin(200), true);
  assert.deepEqual(scheduler.active, { real: 0.35, imaginary: 0.35 });
});

test("out of order and duplicate/unknown completions cannot release slots or finish early", () => {
  const scheduler = initialized();
  scheduler.retarget(1, 0, 0, 0);
  scheduler.begin(0);
  const batch = scheduler.take(0);
  assert.equal(scheduler.complete({ generation: scheduler.generation, tileId: 99 }), undefined);
  assert.equal(scheduler.pending.size, 4);
  assert.equal(scheduler.complete(batch[3])?.frameComplete, false);
  assert.equal(scheduler.complete(batch[3]), undefined);
  assert.equal(scheduler.completed, 1);
  assert.equal(scheduler.take(0).length, 1);
  assert.equal(scheduler.pending.size, 4);
  for (const entry of [...scheduler.pending.values()]) scheduler.complete(entry.request);
  finish(scheduler);
  assert.deepEqual(scheduler.rendered, { real: 0.35, imaginary: 0 });
});

test("configuration invalidation drains stale slots before a fresh frame; no partial point is rendered", () => {
  const scheduler = initialized();
  scheduler.retarget(1, 0, 0, 0);
  scheduler.begin(0);
  const batch = scheduler.take(0);
  scheduler.invalidate();
  assert.equal(scheduler.begin(0), false);
  assert.deepEqual(scheduler.rendered, { real: 0, imaginary: 0 });
  assert.equal(scheduler.take(0).length, 0);
  for (const tile of batch) assert.equal(scheduler.complete(tile)?.current, false);
  assert.equal(scheduler.complete(batch[0]), undefined);
  assert.equal(scheduler.begin(0), true);
  assert.equal(scheduler.active!.real, 0.35);
});

test("detail refresh at unchanged target renders once and then stops", () => {
  const scheduler = initialized();
  scheduler.invalidate();
  assert.equal(scheduler.begin(0), true);
  finish(scheduler);
  assert.equal(scheduler.begin(0), false);
});

test("pause finishes the active frame but starts none; resume uses latest sliders", () => {
  const scheduler = initialized();
  scheduler.retarget(1, 0, 0, 0);
  scheduler.begin(0);
  scheduler.paused = true;
  scheduler.retarget(-1, 1, 0, 0);
  finish(scheduler);
  assert.deepEqual(scheduler.rendered, { real: 0.35, imaginary: 0 });
  assert.equal(scheduler.begin(0), false);
  scheduler.invalidate();
  assert.equal(scheduler.begin(0), false);
  scheduler.paused = false;
  assert.equal(scheduler.begin(0), true);
  assert.deepEqual(scheduler.active, easeParameter({ real: 0.35, imaginary: 0 }, scheduler.target));
});

test("reduced motion goes straight to the requested point after current frame", () => {
  const scheduler = initialized();
  scheduler.reducedMotion = true;
  scheduler.retarget(1.234, -0.567, 0, 0);
  scheduler.begin(0);
  finish(scheduler);
  assert.deepEqual(scheduler.rendered, scheduler.target);
  assert.equal(scheduler.begin(0), false);
});

test("reset forgets frame state but never reuses a generation or accepts old output", () => {
  const scheduler = initialized();
  scheduler.invalidate();
  scheduler.begin(0);
  const old = scheduler.take(0);
  scheduler.reset();
  scheduler.begin(0);
  assert.equal(scheduler.generation, 3);
  for (const tile of old) assert.equal(scheduler.complete(tile), undefined);
  assert.equal(scheduler.completed, 0);
});

test("raw and gzip iteration transport preserve unsigned little-endian samples", async () => {
  const tile = { columns: 2, rows: 1, iterations: btoa("\x01\x00\x00\x04"), encoding: "raw" as const };
  assert.deepEqual([...await decodeIterations(tile)], [1, 1024]);
  const compressed = { ...tile, iterations: gzipSync(Buffer.from([1, 0, 0, 4])).toString("base64"), encoding: "gzip" as const };
  assert.deepEqual([...await decodeIterations(compressed)], [1, 1024]);
  await assert.rejects(decodeIterations({ ...compressed, rows: 2 }), /length/);
  await assert.rejects(decodeIterations({ ...tile, rows: 2 }), /length/);
  await assert.rejects(decodeIterations({ ...compressed, iterations: tile.iterations }));
  await assert.rejects(decodeIterations(JSON.parse(JSON.stringify({ ...tile, encoding: "unknown" }))), /Unsupported/);
});

test("local palettes preserve interiors and never mutate iteration samples", () => {
  const values = new Uint16Array([0, 10, 256]);
  for (const palette of ["aurora", "ember", "ocean"]) {
    assert.deepEqual(color(256, 256, palette), [5, 9, 20]);
    for (let iteration = 0; iteration < 1024; iteration++)
      assert.ok(color(iteration, 1024, palette).every(channel => Number.isInteger(channel) && channel >= 0 && channel <= 255));
    values.forEach(iteration => color(iteration, 256, palette));
  }
  assert.deepEqual([...values], [0, 10, 256]);
  assert.notDeepEqual(color(10, 256, "aurora"), color(10, 256, "ember"));
});
