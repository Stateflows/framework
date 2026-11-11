# Julia Live: streaming Activity fractals

A standalone, mouse-driven Julia-set demo. **The Activity computes the fractal on
the server**; the TypeScript/Vite frontend only plans tile geometry, colors
iteration counts, and paints a Canvas 2D image. No React, WebGL, external assets,
or external rendering service is needed.

**Local development only.** This host has no authentication and uses in-memory
Orleans storage and reminders. It refuses to start outside `Development`. Do not
expose it publicly.

## Run

Requires the .NET 10 SDK and Node.js 22.18+ (or Node.js 24).
From the repository root, build the frontend once, then start the host:

```powershell
Push-Location .\Examples\Stateflows.Examples.StreamingFractals\Client
npm ci
npm run build
Pop-Location
dotnet run --project .\Examples\Stateflows.Examples.StreamingFractals
```

Open **http://localhost:5158**. The launch profile sets `Development`. If using
`--no-launch-profile`, set `$env:ASPNETCORE_ENVIRONMENT = "Development"` yourself.
The host uses silo port `11118` and gateway port `30008`, separate from the other
examples. Override `Orleans:SiloPort`, `Orleans:GatewayPort`, and the HTTP URL when
running another copy.

For frontend hot reload, leave the .NET host running, run `npm run dev` in
`Client`, and open the URL Vite prints. Vite proxies `/api` and `/stateflows` to
port `5158`. The built frontend is served directly by ASP.NET Core from `wwwroot`;
build it before `dotnet publish` too. Generated assets and `node_modules` are not
checked in.

## CPU or CUDA tile calculation

**No framework CPU-bound API changes are needed.** Execution placement and the
renderer are application concerns. `CalculateJuliaTileAction` delegates only its
tile calculation to the demo's singleton `IJuliaTileRenderer`. The Activity graph,
four interleaving partitions, root reentrant compression action, gzip/raw encoding,
16-bit little-endian samples, HTTP admission, SSE transport, and frontend remain
the same for both backends. `JuliaRenderer.Render` remains the CPU reference.
Stateflows limits tile-processing concurrency to four active parallel partitions
per invocation; no separate CPU worker gate is needed.

`Rendering:Backend` defaults to **Cuda**, including in `appsettings.json`.
Use a 64-bit process, a compatible NVIDIA CUDA-capable GPU and NVIDIA
driver, and enough free device memory for the driver context. ILGPU **1.5.3**
compiles this kernel to PTX and uses the CUDA **driver**; the native CUDA Toolkit,
SDK, `nvcc`, and LibDevice are not required. Intel integrated graphics and OpenCL
are not used. The normal run command uses CUDA; it can also be selected explicitly:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project .\Examples\Stateflows.Examples.StreamingFractals --no-launch-profile -- --Rendering:Backend=Cuda --urls http://localhost:5158
```

Or set `$env:Rendering__Backend = "Cuda"` before the normal `dotnet run` command.
Select `--Rendering:Backend=Cpu` (or `$env:Rendering__Backend = "Cpu"`) to use
CPU rendering on machines without CUDA or for comparison. CLI settings override environment variables and JSON. Backend names are
case-insensitive; other names are rejected. `Rendering:CudaDeviceIndex` selects
the zero-based CUDA device (default **0**), not the Windows display-adapter index.
`Rendering:CudaStreams` is **2** by default and must be **1..4**. Device indices
must be nonnegative. Startup logs the selected backend and CUDA device name.
An unavailable device, driver, compilation failure, allocation failure, or invalid
configuration fails startup explicitly: **there is no automatic CPU fallback**.

One shared accelerator and one compiled kernel serve all sessions. A bounded
pool owns reusable streams, 16,384-element `int` device buffers, and matching CPU
readback arrays (64 KiB of each per stream, excluding driver/context overhead).
The result is converted to the existing unsigned 16-bit little-endian byte format.
Only active pool slots schedule thread-pool work; waiters await asynchronously,
without blocking request or UI threads. Pool size is a process-wide CUDA limit,
not a per-client quota or a bound on total pending HTTP admissions.

Queued cancellation skips GPU execution. Once a kernel is launched it cannot be
preempted by a .NET cancellation token: its stream and readback drain, then its
result is discarded if cancelled. Slots are returned only after draining.
Host disposal cancels queued work, drains active calls, then disposes all pooled
resources, accelerator, and context. This is not immediate kernel termination.
The kernel retains the reference's double-precision coordinates and iteration-loop
order. Explicit round-to-nearest FP64 multiplication prevents CUDA driver FMA
contraction; fast math and 32-bit substitutions are not enabled.

This is deliberately a **per-tile calculation experiment**, not full-frame GPU
rendering. Every tile still incurs scheduling, launch, readback, conversion,
compression, Activity, transport, decoding, and canvas costs. The existing
`computeMilliseconds` field includes renderer queueing/scheduling and (for CUDA)
readback/conversion, but excludes subsequent compression and transport. Backend
and device are logged rather than changing the UI protocol. The frame overlay
remains an end-to-end measure. Short or sparse tiles can be slower on CUDA; neither
kernel throughput nor tile-only speedup predicts animation speed.

## Explore

- Move the pointer across the canvas to change the complex parameter `c`. Touch
  and pen use the same Pointer Events path. The real/imaginary sliders also work
  with a keyboard.
- The controls show the **target** complex parameter, separately from the
  **rendered** parameter in the canvas caption. After target input settles, each
  completed frame advances `c` by 35% of the remaining complex-plane difference.
  At Euclidean distance **<= 0.002**, the next frame uses the exact requested
  parameter, then computation stops. The initial frame goes directly to its target.
  The implementation is original and uses no third-party fractal scripts.
- Every frame is **768 x 512**, made of twenty-four center-first 128 x 128
  step-1 tiles with identical `c`. Iteration counts are assembled in offscreen
  arrays; only a complete image is presented, atomically. There are no coarse
  passes, mixed-parameter images, crossfades, or browser-side fractal calculations.
- **Target input settle delay** adjusts target admission from 0 to 1000ms
  (250ms default). Each input replaces the destination and restarts this wait.
  It does **not** delay every animation frame. Zero starts toward the latest
  target as soon as the current frame finishes. Rendering speed is bounded by
  server computation and transport, not a 60fps promise.
- Animation is render-paced: at most one frame is being assembled, no future
  frames are queued, and every next point is based on the last completed point.
  A slow frame advances by the same fraction as a fast frame, never by elapsed
  wall-clock time. Retargeting finishes the current image before using the latest
  settled destination; it does not enqueue intermediate input positions.
  `prefers-reduced-motion: reduce` (sampled when starting a session) bypasses
  easing and renders the exact target in the next frame.
- Change **Detail** to send an ordinary `ConfigureRendering` event, then render
  with the new iteration limit. An unfinished frame is discarded; already
  admitted tiles drain as stale results, retaining their admission slots.
  New frames wait for configuration to finish and old slots to drain. Each
  action snapshots settings and each result carries its own limit. Multiple
  configuration changes remain serialized ordinary events.
- Change **Palette** to recolor only the complete displayed image immediately,
  even while the next frame is partly assembled. It sends no rendering work.
  The next complete image uses the latest palette.
- **Pause input** finishes the currently active full frame (including remaining
  tile admissions), then starts no further frames. Pointer movement is ignored
  while paused; keyboard slider changes retain the latest target without rendering.
  **Resume input** uses those slider values. Detail changes while paused discard
  unfinished images and defer their replacement until resume.
- **Stop session** stops admission, finalizes the Activity immediately, and
  resets its state. Executing actions observe cancellation.
  **Start new session** creates an independent Activity instance.

The overlay shows outstanding tiles (submitted but not yet observed on SSE),
output tiles per second, frame-to-first-tile latency, last full-frame completion
time (including admission and transport), generation ID,
execution mode, and obsolete results ignored. Tile rate includes obsolete
results; it measures stream delivery, not only visible painting.

Expand **What is streaming here?** to see the pipeline and open its actual
PlantUML source. No source or image is sent to a third-party diagram service.

## The network and transport

```text
TileRequest input pin
    -> RenderTiles (interleaving parallel activity, chunkSize: 1, maxConcurrency: 4)
        -> TileRequest input pin
        -> CalculateJuliaTileAction (configured CPU/CUDA tile renderer)
            -> CompressJuliaTileAction in root scope (outgoing crossing flow, reentrant)
                -> root TileResult output pin

ConfigureRendering accept-event action -> immutable settings in Activity values
```

This is one activation-owned Activity network per browser session, not a bespoke
server work queue or one Activity per tile. New requests join the active
RenderTiles invocation as independent partitions without reinitializing it.
When its partitions drain, the invocation finalizes; later requests start a new
invocation. The calculation action itself is not reentrant.

Each result crosses directly to a reentrant compression action in the immediately
enclosing root scope, then proceeds to live publication, rather than waiting in a parallel output pin until all partitions
finish. This outgoing expansion crossing is a Stateflows extension; ordinary
parallel output-pin aggregation is unchanged. The frontend waits for the output
watch to open **before** initializing or admitting input.
Tile output uses the default nonretained, zero-TTL notification policy: live
delivery only, without notification history or subscription retries.

| Route | Meaning |
|---|---|
| `GET /api/fractal` | Output event name and Activity route prefix used by the client. |
| `POST /stateflows/activities/Julia/{id}/input/tileRequest?sendAsStream=true` | One tile in `{"tokens":[...]}`; `202` means volatile admission, not completed computation. |
| `GET /stateflows/activities/Julia/{id}/output/tileResult?stream=true` | Output-pin notifications over SSE. |
| `POST /stateflows/activities/Julia/{id}/configureRendering` | `{"event":{"maxIterations":256}}`; ordinary serialized configuration. |
| `POST /api/sessions/{id}/stop` | Immediate Activity finalization, including cancellation, followed by reset. |
| `GET /api/sessions/{id}/diagram` | PlantUML text for the started Activity. |

Omit `sendAsStream` to compare ordinary transactional input: the POST waits for
processing and results report `Transactional`, rather than `Streamed`.
`sendAsStream` selects execution semantics; `stream=true` selects SSE transport.

SSE uses a named event, advertised by `/api/fractal`, and an envelope with tile
payloads at `payload`. Each output token is published as its own `TileResult`
notification. `iterations` is base64-encoded binary, with `encoding` equal to
`gzip` or `raw`. The compression action applies gzip at `CompressionLevel.Fastest`
to each tile and keeps raw bytes when gzip would not reduce the byte length.
After decompression, the payload contains **unsigned 16-bit little-endian**
escape-iteration counts, row-major, with `columns * rows` samples.
The browser uses `DecompressionStream` and marks a tile complete only after
decoding; late decodes from a stopped session are ignored. Unsupported encodings
and corrupt payloads explicitly interrupt the session. A browser supporting
`DecompressionStream("gzip")` is required. Compression is lossless, so local
palette changes still work. Counts equal to `maxIterations` represent points that did not escape.
This avoids JSON arrays of individual pixels. Coordinates, sampling step,
generation ID, tile ID, execution mode, and renderer duration accompany each tile.

## Freshness and bounds

Input is coalesced by the configurable settle delay. The browser retains only
the latest destination and allows at most **four outstanding tiles**, including
stale configuration-invalidated tiles. All tiles in an active frame continue
without another settle wait. Generation IDs reject obsolete output and
duplicate/unknown completions cannot release admission slots. Tile geometry and
sample lengths are checked before accepting a completion.

**Retargeting is not cancellation.** The active frame finishes; configuration
invalidation allows at most four admitted obsolete tiles to finish and ignores
their output. Explicit session finalization cancels work. RenderTiles limits
active partitions to four per invocation; the browser admission limit is not
a server-side per-client quota. There is no cross-session global CPU cap.

Validation rejects null/empty/multi-tile batches, non-finite/out-of-range complex
parameters, invalid geometry, images over 960 x 640, unsupported sampling steps,
tiles over 16,384 samples, and iteration limits outside 32..1024 before admission.

SSE is future-only, bounded, and has no replay cursor. On connection loss or a
15-second output stall the client explicitly interrupts the session, attempts
finalization/reset, and offers a fresh session instead of silently resuming a
potentially incomplete image. Tile/configuration HTTP calls time out after 10
seconds; finalization/reset has a separate 60-second cleanup budget.
Closing a watch alone does not cancel the Activity. Page exit sends a best-effort
stop beacon; it is not guaranteed cleanup.

The stop endpoint awaits finalization and reset directly, returning their event
results without issuing an additional status query on the now-cold instance.

Outputs are live-only notifications, not an accumulated image archive. Stop resets
the instance; abandoned tabs and failed cleanup can retain in-memory Activity
state until host shutdown. Nothing is restart-durable or exactly-once. Rendering
performance depends on renderer, iteration count, and transport; this is not a
browser-GPU frame-rate benchmark.

## Checks

From `Client`:

```powershell
npm test
npx playwright install chromium
npm run test:e2e
```

Browser checks start/stop a separate host; HTTP port `5158` and test silo/gateway
ports `11128`/`30018` must be free. Set `FRACTAL_TEST_PORT`,
`FRACTAL_TEST_SILO_PORT`, and `FRACTAL_TEST_GATEWAY_PORT` to use different ports. They
exercise actual server-produced pixels, atomic full-resolution frames under
held tile admissions, intermediate parameters, newest-target retargeting,
exact convergence, reduced motion, settle delay, palettes during assembly,
pause/resume, configuration, cancellation/restart, and lost-stream recovery.
Set `FRACTAL_TEST_CONFIGURATION=Release` to avoid overwriting a running Debug
host's locked assemblies. Build/test the backend in that configuration too.

From the repository root:

```powershell
dotnet test .\Tests\StreamingFractals.IntegrationTests
```

The server checks cover known Julia points, cancellation, input/work bounds,
HTTP validation, streamed and ordinary execution, SSE payloads, configuration,
diagram generation, finalization, and reset. The host test uses ephemeral silo
and gateway ports.

CUDA-specific tests are opt-in so ordinary tests can run on machines without
CUDA. When enabled they **require actual CUDA hardware** and fail rather than
skip or fall back if initialization fails:

```powershell
$env:STREAMING_FRACTALS_CUDA_TESTS = "1"
dotnet test .\Tests\StreamingFractals.IntegrationTests -c Release
```

These add byte-for-byte CPU/CUDA parity across complex parameters, iteration
limits 32/64/256/1024, steps 1/2/4/8, edge/partial tiles, concurrently queued
partitions, cancellation, buffer reuse/shutdown, and the same HTTP/SSE lifecycle
test on CUDA. Invalid configuration and missing-device failure tests run even
without opting into GPU rendering.

Run the same four browser checks against CUDA, without duplicating test cases:

```powershell
Push-Location .\Examples\Stateflows.Examples.StreamingFractals\Client
$env:Rendering__Backend = "Cuda"
$env:FRACTAL_TEST_CONFIGURATION = "Release"
$env:FRACTAL_TEST_PORT = "5168"
$env:FRACTAL_TEST_SILO_PORT = "11138"
$env:FRACTAL_TEST_GATEWAY_PORT = "30028"
npm run test:e2e
Pop-Location
```

For a bounded local **warm-tile** measurement (eight warmups, 25 sequential
samples per backend and workload), enable the benchmark separately:

```powershell
$env:STREAMING_FRACTALS_CUDA_TESTS = "1"
$env:STREAMING_FRACTALS_BENCHMARK = "1"
dotnet test .\Tests\StreamingFractals.IntegrationTests -c Release --filter TestCategory=Benchmark --logger "trx;LogFileName=cuda-rendering.trx"
```

The TRX under the test project's `TestResults` records device, geometry,
iteration limit, CPU/CUDA median and p95 durations, and median ratios. CUDA timing
includes scheduling, readback and little-endian conversion, not just the kernel.
Initialization/compilation is excluded; compression, transport, and full-frame
timings are also excluded. Results are machine/load-specific, not performance
guarantees. Clear the opt-in environment variables after experimenting.
