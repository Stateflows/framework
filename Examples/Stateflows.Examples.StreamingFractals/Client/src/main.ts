import "./style.css";
import { color, concurrency, decodeIterations, imageHeight, imageWidth, TileScheduler } from "./model.ts";
import type { TileRequest, TileResult } from "./model.ts";

function element<T extends HTMLElement>(id: string): T {
  const value = document.getElementById(id);
  if (!value) throw new Error(`Missing UI element: ${id}`);
  return value as T;
}

const canvas = element<HTMLCanvasElement>("fractal");
const context = canvas.getContext("2d");
if (!context) throw new Error("Canvas 2D is not supported by this browser.");
const drawing = context;
const realControl = element<HTMLInputElement>("real");
const imaginaryControl = element<HTMLInputElement>("imaginary");
const detailControl = element<HTMLInputElement>("detail");
const delayControl = element<HTMLInputElement>("delay");
const paletteControl = element<HTMLSelectElement>("palette");
const pauseButton = element<HTMLButtonElement>("pause");
const stopButton = element<HTMLButtonElement>("stop");
const restartButton = element<HTMLButtonElement>("restart");
const status = element("status");
const samples = new Uint16Array(imageWidth * imageHeight);
const limits = new Uint16Array(imageWidth * imageHeight);
const frame = drawing.createImageData(imageWidth, imageHeight);
const nextSamples = new Uint16Array(imageWidth * imageHeight);
const nextLimits = new Uint16Array(imageWidth * imageHeight);
const scheduler = new TileScheduler();
const admissions = new Set<Promise<void>>();
let session = "";
let activityPath = "";
let stream: EventSource | undefined;
let running = false;
let ready = false;
let paused = false;
let inputTimer: ReturnType<typeof setTimeout> | undefined;
let configuration: Promise<void> = Promise.resolve();
let configuring = false;
let generationStarted = 0;
let firstTile = false;
let stale = 0;
let arrivals: number[] = [];
let lastOutput = performance.now();

function connection(text: string, live: boolean): void {
  element("connection").textContent = text;
  element("connection-light").classList.toggle("live", live);
}

async function request(path: string, body?: unknown, timeoutMilliseconds = 10000): Promise<void> {
  const response = await fetch(path, {
    method: "POST",
    signal: AbortSignal.timeout(timeoutMilliseconds),
    headers: body === undefined ? undefined : { "Content-Type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  const content = await response.text();
  if (!response.ok) throw new Error(`POST ${path}: ${response.status} ${content}`);
}

function updateParameters(): void {
  const real = Number(realControl.value);
  const imaginary = Number(imaginaryControl.value);
  element("real-value").textContent = real.toFixed(3);
  element("imaginary-value").textContent = imaginary.toFixed(3);
  element("target-parameter").textContent = `Target c = ${real.toFixed(3)} ${imaginary < 0 ? "-" : "+"} ${Math.abs(imaginary).toFixed(3)}i`;
}

function scheduleRender(): void {
  updateParameters();
  scheduler.retarget(Number(realControl.value), Number(imaginaryControl.value), performance.now(), Number(delayControl.value));
  if (!running || !ready || paused) return;
  scheduleAdmission();
}

function scheduleAdmission(): void {
  if (inputTimer !== undefined) clearTimeout(inputTimer);
  inputTimer = setTimeout(() => {
    inputTimer = undefined;
    pump();
  }, Number(delayControl.value));
}

function pump(): void {
  if (!running || !ready || configuring) return;
  if (scheduler.begin(performance.now())) {
    generationStarted = performance.now();
    firstTile = true;
    element("latency").textContent = "--";
    element("generation").textContent = String(scheduler.generation);
  }
  if (scheduler.active) status.textContent = paused
    ? "Input paused. Finishing the current full frame."
    : "Rendering full-resolution frame on the server...";
  else status.textContent = paused
    ? "Input paused. Animation held."
    : scheduler.rendered?.real === scheduler.target.real && scheduler.rendered?.imaginary === scheduler.target.imaginary
      ? "Target reached. Move to explore."
      : "Waiting for target input to settle...";
  for (const tile of scheduler.take(performance.now())) {
    const admission = submit(tile);
    admissions.add(admission);
    void admission.finally(() => admissions.delete(admission));
  }
  element("in-flight").textContent = `${scheduler.pending.size} / ${concurrency}`;
  element("progress").style.width = `${scheduler.total ? scheduler.completed / scheduler.total * 100 : 0}%`;
}

async function submit(tile: TileRequest): Promise<void> {
  const currentSession = session;
  try {
    await request(`${activityPath}/input/tileRequest?sendAsStream=true`, { tokens: [tile] });
  } catch (error) {
    fail(error, currentSession);
  }
}

async function receive(event: MessageEvent<string>, currentSession: string): Promise<void> {
  try {
    const envelope = JSON.parse(event.data) as { payload: TileResult };
    const tile = envelope.payload;
    const pending = scheduler.pending.get(`${tile.generation}:${tile.tileId}`);
    if (!pending) return;
    const expected = pending.request;
    if (tile.step !== 1 || tile.x !== expected.x || tile.y !== expected.y ||
        tile.width !== expected.width || tile.height !== expected.height ||
        tile.columns !== expected.width || tile.rows !== expected.height ||
        !Number.isInteger(tile.maxIterations) || tile.maxIterations < 32 || tile.maxIterations > 1024)
      throw new Error("Output tile does not match its admitted request.");
    const values = await decodeIterations(tile);
    if (!running || session !== currentSession) return;
    const completion = scheduler.complete(tile);
    if (!completion) return;
    lastOutput = performance.now();
    arrivals.push(lastOutput);
    if (!completion.current) {
      element("stale").textContent = String(++stale);
      pump();
      return;
    }
    if (firstTile) {
      firstTile = false;
      element("latency").textContent = `${Math.round(lastOutput - generationStarted)} ms`;
    }
    element("mode").textContent = tile.executionMode;
    applyTile(tile, values);
    if (completion.frameComplete) {
      samples.set(nextSamples);
      limits.set(nextLimits);
      paint();
      const rendered = scheduler.rendered!;
      element("parameter").textContent = `Rendered c = ${rendered.real.toFixed(6)} ${rendered.imaginary < 0 ? "-" : "+"} ${Math.abs(rendered.imaginary).toFixed(6)}i`;
      canvas.dataset.real = String(rendered.real);
      canvas.dataset.imaginary = String(rendered.imaginary);
      canvas.dataset.generation = String(scheduler.generation);
      element("frame-time").textContent = `${Math.round(performance.now() - generationStarted)} ms`;
    }
    pump();
  } catch (error) {
    fail(error, currentSession);
  }
}

function applyTile(tile: TileResult, values: Uint16Array): void {
  for (let row = 0; row < tile.height; row++) {
    for (let column = 0; column < tile.width; column++) {
      const target = (tile.y + row) * imageWidth + tile.x + column;
      nextSamples[target] = values[row * tile.columns + column];
      nextLimits[target] = tile.maxIterations;
    }
  }
}

function paint(): void {
  const palette = Array.from({ length: 1025 }, (_, iteration) => color(iteration, 1025, paletteControl.value));
  const values = samples;
  const image = frame;
  for (let index = 0; index < values.length; index++) {
    const limit = limits[index];
    const rgb = values[index] >= limit ? [5, 9, 20] : palette[values[index]];
    image.data[index * 4] = rgb[0];
    image.data[index * 4 + 1] = rgb[1];
    image.data[index * 4 + 2] = rgb[2];
    image.data[index * 4 + 3] = 255;
  }
  drawing.putImageData(image, 0, 0);
}

function disableInput(): void {
  running = false;
  ready = false;
  if (inputTimer !== undefined) clearTimeout(inputTimer);
  inputTimer = undefined;
  stream?.close();
  pauseButton.disabled = true;
  stopButton.disabled = true;
  detailControl.disabled = true;
  restartButton.hidden = false;
}

function fail(error: unknown, currentSession = session): void {
  if (!running || session !== currentSession) return;
  disableInput();
  connection("Session interrupted", false);
  status.textContent = error instanceof Error ? error.message : String(error);
  console.error(error);
  if (!activityPath) {
    restartButton.disabled = false;
    return;
  }
  // Closing SSE only removes the watch; explicitly cancel the Activity too.
  restartButton.disabled = true;
  void resetSession(currentSession).catch(stopError => {
    if (session === currentSession) status.textContent += ` Cleanup failed: ${String(stopError)}`;
    console.error(stopError);
  }).finally(() => {
    if (session === currentSession) restartButton.disabled = false;
  });
}

async function resetSession(instance: string): Promise<void> {
  // Finish admission calls before resetting, so a late POST cannot revive the old instance.
  await Promise.all([...admissions, configuration.catch(error => {
    console.error(error);
  })]);
  await request(`/api/sessions/${instance}/stop`, undefined, 60000);
}

async function stop(): Promise<void> {
  disableInput();
  restartButton.disabled = true;
  connection("Stopping", false);
  try {
    await resetSession(session);
    scheduler.reset();
    element("in-flight").textContent = `0 / ${concurrency}`;
    connection("Stopped", false);
    status.textContent = "Activity finalized and reset. Start a new session to explore again.";
  } catch (error) {
    connection("Stop failed", false);
    status.textContent = String(error);
    console.error(error);
  } finally {
    restartButton.disabled = false;
  }
}

async function start(): Promise<void> {
  session = crypto.randomUUID();
  activityPath = "";
  running = true;
  ready = false;
  restartButton.hidden = true;
  restartButton.disabled = true;
  pauseButton.disabled = true;
  stopButton.disabled = true;
  detailControl.disabled = true;
  paused = false;
  pauseButton.textContent = "Pause input";
  scheduler.reset();
  scheduler.reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  configuring = false;
  element("in-flight").textContent = `0 / ${concurrency}`;
  element("frame-time").textContent = "--";
  element("latency").textContent = "--";
  element("progress").style.width = "0%";
  arrivals = [];
  stale = 0;
  element("stale").textContent = "0";
  connection("Connecting", false);
  status.textContent = "Opening output stream before admitting work...";
  try {
    const response = await fetch("/api/fractal", { signal: AbortSignal.timeout(10000) });
    if (!response.ok) throw new Error(`Cannot load server contract: ${response.status}`);
    const contract = await response.json() as { outputEvent: string; activityPath: string };
    activityPath = `${contract.activityPath}/${session}`;
    const source = new EventSource(`${activityPath}/output/tileResult?stream=true`);
    stream = source;
    const currentSession = session;
    source.addEventListener(contract.outputEvent, event => {
      if (running && session === currentSession) void receive(event as MessageEvent<string>, currentSession);
    });
    await new Promise<void>((resolve, reject) => {
      const timeout = setTimeout(() => {
        source.close();
        reject(new Error("Timed out opening the output stream."));
      }, 10000);
      source.onopen = () => { clearTimeout(timeout); resolve(); };
      source.onerror = () => { clearTimeout(timeout); reject(new Error("Cannot open the output stream.")); };
    });
    source.onerror = () => fail(new Error("Output stream lost. Outputs are not replayed; start a new session."), currentSession);
    configuration = request(`${activityPath}/configureRendering`, { event: { maxIterations: Number(detailControl.value) } });
    await configuration;
    if (!running) return;
    ready = true;
    connection("Streaming", true);
    element<HTMLAnchorElement>("diagram").href = `/api/sessions/${session}/diagram`;
    pauseButton.disabled = false;
    stopButton.disabled = false;
    detailControl.disabled = false;
    restartButton.disabled = false;
    lastOutput = performance.now();
    scheduleRender();
  } catch (error) {
    fail(error);
  }
}

canvas.addEventListener("pointermove", event => {
  if (!running || !ready || paused) return;
  const bounds = canvas.getBoundingClientRect();
  realControl.value = String(Math.max(-1.5, Math.min(1.5, (event.clientX - bounds.left) / bounds.width * 3 - 1.5)));
  imaginaryControl.value = String(Math.max(-1.5, Math.min(1.5, 1.5 - (event.clientY - bounds.top) / bounds.height * 3)));
  scheduleRender();
});
realControl.addEventListener("input", scheduleRender);
imaginaryControl.addEventListener("input", scheduleRender);
delayControl.addEventListener("input", () => {
  element("delay-value").textContent = `${delayControl.value} ms`;
  if (running && ready) {
    scheduler.settle(performance.now(), Number(delayControl.value));
    scheduleAdmission();
  }
});
detailControl.addEventListener("input", () => {
  element("detail-value").textContent = `${detailControl.value} iterations`;
});
detailControl.addEventListener("change", () => {
  if (!running || !ready) return;
  const iterations = Number(detailControl.value);
  const currentSession = session;
  const path = activityPath;
  scheduler.invalidate();
  configuring = true;
  const updatedConfiguration = configuration.then(async () => {
    if (running && currentSession === session)
      await request(`${path}/configureRendering`, { event: { maxIterations: iterations } });
  });
  configuration = updatedConfiguration;
  void updatedConfiguration.then(() => {
    if (currentSession === session && configuration === updatedConfiguration) {
      configuring = false;
      pump();
    }
  }).catch(error => fail(error, currentSession));
});
paletteControl.addEventListener("change", () => paint());
pauseButton.addEventListener("click", () => {
  paused = !paused;
  scheduler.paused = paused;
  pauseButton.textContent = paused ? "Resume input" : "Pause input";
  if (!paused) scheduleRender();
  pump();
});
stopButton.addEventListener("click", () => { void stop(); });
restartButton.addEventListener("click", () => { void start(); });
window.addEventListener("pagehide", () => {
  stream?.close();
  if (running && session) navigator.sendBeacon(`/api/sessions/${session}/stop`);
});
setInterval(() => {
  const now = performance.now();
  if (running && ready) pump();
  arrivals = arrivals.filter(time => now - time < 1000);
  element("rate").textContent = String(arrivals.length);
  const oldest = Math.min(...Array.from(scheduler.pending.values(), entry => entry.sentAt));
  if (running && scheduler.pending.size > 0 && now - Math.max(oldest, lastOutput) > 15000) {
    fail(new Error("No tile output for 15 seconds. The session may have failed; start a new session."));
  }
}, 250);
paint();
void start();
