# Streaming Load Lab

A Blazor Web App with **Interactive Server** and a colocated Orleans silo. It
demonstrates streamed Activity input under variable load, while ordinary events
steer the same live Activity. Every connected browser controls **one shared run**.
No load starts automatically.

## Run

Requires .NET 10 SDK. From the repository root:

```powershell
dotnet run -c Release --project Examples\Stateflows.Examples.StreamingLoad --launch-profile StreamingLoad
```

Open **http://localhost:5157**. The launch profile sets Development. Orleans uses
silo port **11117** and gateway **30007**, separate from StreamingTelemetry.
Override them in `appsettings.json` or with `Orleans__SiloPort` and
`Orleans__GatewayPort`.

This is a **development-only, loopback example**, without authentication, durable
storage, restart recovery, or multi-silo coordination of the local generator and
metrics. Do not expose it publicly. All load and controls execute on the server,
not in the browser. Closing the browser does not stop the shared run; use
**Drain & finish**, **Emergency stop**, or stop the host.

## Suggested experiments

1. Start with the **Framework throughput** default: 20,000 target tokens/s,
   256-token batches, 16 workers, no synthetic CPU/delay/allocation, and 4,096
   outstanding tokens. Observe actual **sink completions/s**, not just admissions.
   The same input/action/branch/sink graph is used in both presets.
   Run Release without a debugger for comparisons.
2. Select **Synthetic CPU + I/O preset** to restore the deliberate workload:
   1,000 target tokens/s, 64-token batches, 4 workers, 1 ms CPU, 10 ms async delay,
   and 4 KiB allocation. Four workers occupied for at least 11 ms/token have an
   ideal ceiling of about 364 tokens/s before overhead; this preset intentionally
   demonstrates saturation, not maximum framework throughput.
   Presets change only the browser's draft fields. Start a new run, or apply both
   the producer settings and the workflow event for an existing run.
3. Select **Ramp**, **Burst**, or **Wave**, set a 10-second cycle, and apply the
   producer settings while the run is active. The graph should show the requested
   shape only while processing capacity can keep up.
4. Increase async delay to 200 ms using **Send workflow event**. Admission
   throttles once the outstanding limit is reached, instead of accumulating an
   unbounded queue. Lower delay and raise workers to recover.
5. Set priority to 100%. The acknowledgment shows the revision, **Transactional**
   execution mode, and actual execution-trigger ID. Work already waiting in I/O
   reads the new branch setting before routing to its sink.
6. To stress CPU, increase CPU work gradually to 2-5 ms. To observe allocation
   and GC, increase allocation to 64-256 KiB. These are real allocations and CPU
   work; avoid maximum settings on a shared machine.
7. To explore framework throughput rather than artificial work, use the throughput
   preset, then raise peak rate gradually.
   There is no promised tokens/s benchmark: build configuration, hardware,
   Windows timer granularity, serialization, Orleans, and dashboard sampling all
   affect results. Use a Release build for comparisons.
8. **Pause generator** stops new admission but allows admitted work to finish.
   **Resume generator** resumes without a catch-up flood. **Drain & finish**
   stops admission, waits for all tracked tokens, and queues Activity finalization.
   **Emergency stop** immediately finalizes/cancels the Activity and counts its
   remaining tokens as abandoned. Each new run uses a fresh Activity instance.

Controls are shared across browsers. A browser's input fields are its local draft;
the live cards, run accounting, current producer summary, and acknowledged workflow
settings reflect the shared run. There is no per-browser isolation.

## Activity and execution semantics

`LoadActivity` uses typed nodes and inferred names:

```text
WorkToken input pin
  -> ProcessWorkAction (reentrant)
       -> NormalResult -> CompleteNormalAction
       -> PriorityResult -> CompletePriorityAction

ConfigureWorkflow event
  -> ConfigureWorkflowAction (typed AcceptEventAction)
       -> Activity shared values + live worker limit
```

The generator sends `TokensInput<WorkToken>` with a **Stream** header; `Accepted`
means admission, not completion or durability. Completion counters are updated at
the terminal Activity sinks, never at admission. CPU work runs on the thread pool
so it does not block the Orleans activation scheduler. All batches share one FIFO,
cancellable worker pool; changing the concurrency limit does not require rebuilding
the graph. Decreases allow existing workers to finish. With synthetic work, leases
and settings are per token. When CPU, delay, and allocation are all zero, the action
routes the entire batch synchronously under one lease and one consistent settings
snapshot, without creating per-token tasks or deserializing settings per token.
Every token still traverses the actual Activity flows and terminal sinks.

Workflow controls use `SendAsync(new ConfigureWorkflow(...))` **without** a Stream
header. The typed accept-event action validates the settings, verifies transactional
execution metadata, updates one immutable settings/revision value, and records an
acknowledgment. The UI reports success only after the send returns `Consumed`. CPU/delay/allocation are
captured when each worker starts; routing reads the latest priority setting after
the delay. The classification is deterministic (`sequence % 100`) rather than
random; small batches need not match the requested percentage exactly.

Producer settings change only the generator. They are intentionally separate from
workflow control events.

Both result branches end in local metrics sinks rather than Activity output pins.
This keeps per-token notification history and browser transport out of the measured
path. For notification/SSE output-pin examples, use StreamingTelemetry instead.
There are no per-token HTTP requests or browser updates.

## Profiles and safety bounds

| Profile | Fraction of the configured peak rate |
|---|---|
| Steady | 100% |
| Ramp | Repeating linear 10% to 100% |
| Burst | 100% for the first 20% of each cycle, then 10% |
| Wave | Smooth sinusoidal 10% to 100% |

Cycles use elapsed run time, including paused time; changing profile/period does
not reset that clock. The producer ticks every 20 ms, uses elapsed-time rate
budgeting, and caps accumulated budget to prevent a catch-up flood.

Configurable `SafetyLimits` enforce hard ceilings:
100,000 tokens/s, 64 workers, and 100,000 outstanding tokens. The application rejects
larger ceilings at startup. Batches are limited to 256, CPU work to 20 ms/token,
delay to 2,000 ms/token, and allocation to 256 KiB/token. These are **limits, not
recommended simultaneous settings**; resource use can still be substantial.
The outstanding bound includes tokens reserved for the in-flight admission call.
Lowering it below the current outstanding count is rejected; pause/drain first.
Orleans has a separate default limit of 128 queued/in-flight streamed events;
that limit counts batches, not tokens. The producer accumulates full batches where
possible, flushing partial batches after at most 100 ms of budget accumulation
(scheduling/backpressure may add latency), instead of sending each tick's small
token set immediately. This reduces per-event serialization and checkpoint overhead.
On an explicit `Overloaded` response, nothing was admitted: the generator retains
the same bounded batch and waits 20 ms before another admission attempt. It does
not generate replacement tokens or discard them. Overloaded **batch attempts** are
counted separately from rejected **tokens**. Pause/drain/stop cancel this admission
wait and release its unadmitted reservations. Errors or ambiguous send failures
fault the run rather than retrying an event that might have been admitted.

Errors are logged and surfaced on the dashboard. A faulted run requires emergency
stop before restarting; no silent automatic restart or retry loop is used.
The emergency action is cooperative: CPU loops and I/O honor cancellation. It is
not an OS kill switch or a guarantee of responsiveness under total host starvation.
The framework may log cancelled streamed invocations during emergency finalization;
these are expected cancellations, accounted for as abandoned tokens.

## Reading the metrics

- **Admitted/completed tokens/s**: differences in token counters divided by actual
  elapsed sample time. Samples are approximately one second apart.
- **Outstanding**: reserved/admitted tokens not yet observed at a terminal sink.
  At a drained run, admitted = completed = normal + priority.
  After stop, admitted = completed + abandoned, excluding rejected admissions.
  Tokens reserved but not admitted are released when generation stops and do not
  count as admitted, rejected, or abandoned.
- **Latency**: time from generator reservation to terminal sink. Mean is cumulative
  for the run; p95 uses the last 4,096 completions. A batch publishes its downstream
  results after its workers finish, so large batches can produce bursty completions
  and increase observed latency.
- **CPU**: delta in host-process CPU time divided by elapsed wall time and logical
  processor count. 100% means all CPUs available to the process are busy.
- **Memory**: process resident working set and current managed heap; neither is
  whole-machine memory. The CLR may keep committed/resident memory after draining.
- **Allocation rate**: delta in process-wide allocated bytes per elapsed second.
  **GC counts** are cumulative since process startup.

Resource metrics include **Blazor + Orleans + the generator + Activity execution**.
History is bounded to 120 samples; only outstanding token IDs/timestamps and the
bounded latency window are retained by metrics. A new run resets run counters and
charts, not process resource/GC counters.

## Tests

```powershell
dotnet test Tests\StreamingLoad.IntegrationTests -c Release --filter "TestCategory!=Performance"
dotnet test Tests\StreamingLoad.IntegrationTests -c Release --filter "TestCategory=Performance" --logger "console;verbosity=detailed"
```

The performance case warms up for 3 seconds, then measures sink-completed tokens
over an actual 10-second interval at a 50,000/s target, 256-token batches, 16
zero-work workers, and a 4,096 outstanding bound. It reports process allocation
per completion and normalized process CPU, checks at least 2,000 completions/s,
and verifies bounded backlog, no rejected tokens, and exact drain accounting.
It measures the colocated Orleans/Activity pipeline plus generator and metrics,
not an isolated engine microbenchmark or a browser-rendering benchmark.

A local Release comparison on a shared 16-logical-CPU Windows host measured:

| Same zero-work scenario | Completed tokens/s | Process allocated bytes/completion |
|---|---:|---:|
| Original per-token task/settings path | 5,936 | 76,200 |
| Batched zero-work path with admission waiting (three runs) | 6,255-10,542 | 53,572-64,483 |

The original path was measured once; the optimized path was measured three times.
All optimized runs had zero rejected tokens and exact drain accounting. The spread
means this is not evidence of a fixed speedup or a portable performance guarantee.
Run the measurement on your own host without a debugger; unrelated host load,
GC mode, and hardware affect the result. The target rate is not the measured throughput.

Coverage includes profile shapes and bounds, accounting and stale/duplicate
completion isolation, bounded history, dynamic worker limits/cancellation, real
Orleans streaming, in-flight transactional steering, pause/resume, drain,
emergency stop, restart, and invalid controls.
Additional cases cover preset isolation, overload admission waiting/cancellation,
and transactional transitions between zero-work and synthetic processing.
