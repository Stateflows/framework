export const imageWidth = 768;
export const imageHeight = 512;
export const concurrency = 4;
export const inputSettleDelay = 250;
export const easingFraction = 0.35;
export const easingEpsilon = 0.002;

export interface Parameter { real: number; imaginary: number }

export function easeParameter(from: Parameter, target: Parameter, reducedMotion = false): Parameter {
  const real = target.real - from.real;
  const imaginary = target.imaginary - from.imaginary;
  if (reducedMotion || Math.hypot(real, imaginary) <= easingEpsilon) return { ...target };
  return { real: from.real + real * easingFraction, imaginary: from.imaginary + imaginary * easingFraction };
}

export interface TileRequest {
  generation: number;
  tileId: number;
  real: number;
  imaginary: number;
  imageWidth: number;
  imageHeight: number;
  x: number;
  y: number;
  width: number;
  height: number;
  step: number;
}

export interface TileResult {
  generation: number;
  tileId: number;
  x: number;
  y: number;
  width: number;
  height: number;
  step: number;
  columns: number;
  rows: number;
  maxIterations: number;
  iterations: string;
  encoding: "raw" | "gzip";
  computeMilliseconds: number;
  executionMode: string;
}

export class TileScheduler {
  generation = 0;
  completed = 0;
  total = 0;
  readonly pending = new Map<string, { request: TileRequest; sentAt: number }>();
  target: Parameter = { real: -0.745, imaginary: 0.186 };
  rendered: Parameter | undefined;
  active: Parameter | undefined;
  paused = false;
  reducedMotion = false;
  private tiles: TileRequest[] = [];
  private settleAt = 0;
  private refresh = true;

  retarget(real: number, imaginary: number, now: number, delay = inputSettleDelay): void {
    this.target = { real, imaginary };
    this.settleAt = now + delay;
  }

  settle(now: number, delay: number): void {
    this.settleAt = now + delay;
  }

  invalidate(): void {
    // Keep admitted slots occupied until their stale outputs arrive.
    this.active = undefined;
    this.tiles = [];
    this.refresh = true;
  }

  reset(): void {
    this.invalidate();
    this.pending.clear();
    this.rendered = undefined;
    this.completed = this.total = 0;
    this.paused = false;
  }

  begin(now: number): boolean {
    if (this.active || this.pending.size || this.paused || now < this.settleAt) return false;
    if (!this.refresh && this.rendered?.real === this.target.real && this.rendered.imaginary === this.target.imaginary) return false;
    this.active = this.rendered ? easeParameter(this.rendered, this.target, this.reducedMotion) : { ...this.target };
    this.refresh = false;
    this.generation++;
    this.completed = 0;
    const basis = { generation: this.generation, ...this.active, imageWidth, imageHeight };
    let tileId = 0;
    this.tiles = [];
    for (let y = 0; y < imageHeight; y += 128) {
      for (let x = 0; x < imageWidth; x += 128) {
        this.tiles.push({ ...basis, tileId: tileId++, x, y, width: 128, height: 128, step: 1 });
      }
    }
    this.tiles.sort((a, b) => distance(a) - distance(b));
    this.total = tileId;
    return true;
  }

  take(now: number): TileRequest[] {
    const result: TileRequest[] = [];
    while (this.pending.size < concurrency) {
      const request = this.tiles.shift();
      if (!request) break;
      this.pending.set(key(request), { request, sentAt: now });
      result.push(request);
    }
    return result;
  }

  complete(tile: Pick<TileResult, "generation" | "tileId">): { current: boolean; sentAt: number; frameComplete: boolean; request: TileRequest } | undefined {
    const entry = this.pending.get(key(tile));
    if (!entry) return undefined;
    this.pending.delete(key(tile));
    const current = this.active !== undefined && tile.generation === this.generation;
    let frameComplete = false;
    if (current) {
      this.completed++;
      frameComplete = this.completed === this.total;
      if (frameComplete) {
        this.rendered = this.active;
        this.active = undefined;
      }
    }
    return { current, sentAt: entry.sentAt, frameComplete, request: entry.request };
  }
}

function key(tile: Pick<TileRequest, "generation" | "tileId">): string {
  return `${tile.generation}:${tile.tileId}`;
}

function distance(tile: TileRequest): number {
  return (tile.x + tile.width / 2 - imageWidth / 2) ** 2 + (tile.y + tile.height / 2 - imageHeight / 2) ** 2;
}

export async function decodeIterations(tile: Pick<TileResult, "iterations" | "columns" | "rows" | "encoding">): Promise<Uint16Array> {
  const raw = atob(tile.iterations);
  let bytes = Uint8Array.from(raw, character => character.charCodeAt(0));
  if (tile.encoding === "gzip") {
    const stream = new Blob([bytes]).stream().pipeThrough(new DecompressionStream("gzip"));
    bytes = new Uint8Array(await new Response(stream).arrayBuffer());
  } else if (tile.encoding !== "raw") {
    throw new Error(`Unsupported tile encoding: ${tile.encoding}`);
  }
  if (bytes.length !== tile.columns * tile.rows * 2) throw new Error("Tile sample length does not match its dimensions.");
  const values = new Uint16Array(tile.columns * tile.rows);
  for (let index = 0; index < values.length; index++) {
    values[index] = bytes[index * 2] | (bytes[index * 2 + 1] << 8);
  }
  return values;
}

export function color(iteration: number, maxIterations: number, palette: string): [number, number, number] {
  if (iteration >= maxIterations) return [5, 9, 20];
  const t = Math.log2(iteration + 1) / 8;
  const phases = palette === "ember" ? [0, 0.18, 0.36] : palette === "ocean" ? [0.55, 0.32, 0.15] : [0.28, 0.05, 0.7];
  return phases.map(phase => Math.round(127.5 * (1 + Math.cos(2 * Math.PI * (t + phase))))) as [number, number, number];
}
