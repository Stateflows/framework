# Streaming telemetry Activity

A standalone Orleans-backed streaming data-flow example. **Scalar is the only UI**:
no application frontend, dashboard, background sample generator, or external sensor service.
Submit batches yourself to keep admission, steering, and cancellation visible.

**Development only.** Grain storage and reminders are in memory. Checkpoints survive
grain deactivation within this host, not host restarts. The host refuses to start
outside `Development`. This is a local single-silo demo, with no authentication or
production persistence configuration; do not expose it publicly.

## Run

Requires the .NET 10 SDK. From the repository root:

```powershell
dotnet run --project .\Examples\Stateflows.Examples.StreamingTelemetry\Stateflows.Examples.StreamingTelemetry.csproj
```

Open **http://localhost:5156/scalar/v1**. The OpenAPI document is at
http://localhost:5156/openapi/v1.json. The launch profile supplies `Development`;
`--no-launch-profile` requires setting that environment yourself.
Orleans uses silo port `11116` and gateway port `30006`, separate from the order
fulfillment example. Override `Orleans:SiloPort`, `Orleans:GatewayPort`, and the HTTP
URL when running another copy.

The adjacent `.http` file contains requests for every scenario below. All endpoints
address one Activity instance, for example `demo`; use another ID for an independent
network and configuration.

## The network

```text
SensorSample input pin
    ValidateSamplesAction --invalid----------> RejectedSample output pin
       |
       valid
       |
    CalibrateSamplesAction (reentrant, simulated cancellable I/O)
       |
    ForkNode -----------> ProcessedReading output pin
       |
    DetectAlertsAction -> TelemetryAlert output pin

ConfigureTelemetryAction accept-event action ---> replace live configuration
```

This is an Activity graph, not a hand-written work queue. Input/output pins, object
flows, a fork, accept-event actions, and reentrant action execution are defined in
`TelemetryActivity.cs`. Execution uses DI-activated typed action nodes and a typed
initializer in `TelemetryActions.cs`, with injected input/output token collections,
Activity context, and execution metadata. Builder lambdas only configure the graph;
node names and flow targets are inferred from types rather than string identifiers.
The fork deliberately offers each calibrated reading both
to the normal output and to the alert branch.

- **Validation:** sample/sensor IDs must be nonblank and at most 100 characters,
  raw values must be between -10000 and 10000, and `observedAt` must be supplied.
  Invalid samples produce `RejectedSample`; valid samples in the same batch continue.
  Null samples are rejected by this branch as well.
- **Calibration:** `calibratedValue = rawValue * gain + offset`. Each firing snapshots
  the processing delay before simulated I/O and reads the current configuration
  after it. Updating settings can therefore affect work that was already admitted.
  A delay update affects subsequent firings; it does not shorten an existing delay.
- **Alerts:** values greater than or equal to the threshold produce `TelemetryAlert`.
  Each reading carries the threshold captured during calibration, so its alert uses
  the same configuration snapshot even if settings change again.

Default settings are gain `1`, offset `0`, threshold `50`, and delay `1500ms`.
An ordinary `ConfigureTelemetry` event atomically replaces the immutable configuration
record stored in Activity values. Gain must be `0.01..100`, offset `-1000..1000`,
threshold `-1000000..1000000`, and delay `0..5000ms`. Invalid settings return `400`
before execution and leave the previous configuration unchanged.
Input batches must contain `1..64` samples; empty/oversized batches return `400`.

## 1. Watch output before submitting input

The generated routes are:

| Method and suffix after `/stateflows/activities/Telemetry/{instance}` | Meaning |
|---|---|
| `POST /input/sensorSample?sendAsStream=true` | Admit streamed samples; returns `202 Accepted`. |
| `POST /input/sensorSample` | Ordinary serialized input; waits for processing. |
| `GET /output/processedReading?stream=true` | Stream calibrated readings over SSE. |
| `GET /output/telemetryAlert?stream=true` | Stream alerts over SSE. |
| `GET /output/rejectedSample?stream=true` | Stream domain-validation rejections over SSE. |
| `POST /configureTelemetry` | Ordinary serialized event replacing settings. |
| `POST /checkpoint` | Standalone firing-boundary checkpoint; does not initialize a cold instance. |
| `POST /finalize?mode=Immediate` | Finalize and cancel executing work immediately. |
| `POST /finalize?mode=Queued` | Finalize in ordinary event order. |
| `GET /status` | Query lifecycle state and active-node configuration. |
| `DELETE /` (the instance URL without a trailing suffix) | Reset before reusing a finalized instance. |

Scalar documents all routes and body/response schemas. Its handling of live SSE
depends on the Scalar client version. For reliable live viewing, open separate
PowerShell terminals:

```powershell
curl.exe -N "http://localhost:5156/stateflows/activities/Telemetry/demo/output/processedReading?stream=true"
curl.exe -N "http://localhost:5156/stateflows/activities/Telemetry/demo/output/telemetryAlert?stream=true"
curl.exe -N "http://localhost:5156/stateflows/activities/Telemetry/demo/output/rejectedSample?stream=true"
```

Run one command per terminal. Each GET installs an independent future-only watch
and stays open. It does not initialize or execute the Activity. Establish the
connections before submitting input; output is not replayed on reconnect.

SSE `data` is a `TokensOutput<TToken>` notification envelope, not a bare sample:
read payloads from `payload.tokens[].payload`. A notification may contain multiple
tokens. Without `stream=true`, the output GET waits for one future notification
and returns `{"tokens":[...]}`; it does not query historical output. A one-shot watch
must be issued on another connection before the input that produces its result.

## 2. Admit samples and steer them

In Scalar or the `.http` file, submit:

```http
POST /stateflows/activities/Telemetry/demo/input/sensorSample?sendAsStream=true
Content-Type: application/json

{
  "tokens": [
    { "sampleId": "sample-1", "sensorId": "oven", "value": 40, "observedAt": "2026-11-01T20:00:00Z" },
    { "sampleId": "sample-2", "sensorId": "oven", "value": 60, "observedAt": "2026-11-01T20:00:01Z" },
    { "sampleId": "bad-sample", "sensorId": "oven", "value": 20000, "observedAt": "2026-11-01T20:00:02Z" }
  ]
}
```

Admission returns `202` without waiting for the simulated I/O. Before the delay
elapses, send the ordinary event:

```http
POST /stateflows/activities/Telemetry/demo/configureTelemetry
Content-Type: application/json

{
  "event": {
    "gain": 2,
    "offset": 5,
    "alertThreshold": 80,
    "processingDelayMilliseconds": 1500
  }
}
```

If configuration finishes before calibration reads its settings, readings become
`85` and `125` and both generate alerts at threshold `80`. Otherwise the first
batch uses the earlier settings; submit another batch to observe the update.
For easier manual steering, first configure a `5000ms` delay, then admit a batch
and update gain/threshold within that window.

Readings show `executionMode: "Streamed"` and `executionTriggerId: null`.
There is no fabricated source-event lineage. Multiple streamed batches can overlap
inside the **one activation-owned Activity network** because CalibrateSamplesAction is reentrant.
This example imposes no strict per-sensor output ordering or sample-ID deduplication;
correlate notifications using `sampleId`, and use distinct IDs for experiments.

`sendAsStream` chooses Activity execution semantics; `stream` chooses the output
HTTP transport. They are not interchangeable.

## 3. Compare ordinary input

Submit the same shape with `sendAsStream=false` or omit the option. The POST waits
for its transactional work to finish. Its output notifications show
`executionMode: "Transactional"` and a non-null trigger ID. The same output SSE
connections observe both modes.

Configuration uses ordinary event ordering, but **the live configuration is not
isolated from streaming readers**. Streamed branches do not write values in this
example. This deliberate shared-state behavior illustrates steering; it is not a
claim of transactional isolation or rollback for the entire streaming network.
Do not follow stream admission with a status query when measuring admission
latency: status queries can wait behind ordinary event processing.

## 4. Checkpoint and cancel

`POST /checkpoint` waits for executing firing bodies to reach the checkpoint
barrier. It then checkpoints values and the network marking; it does not forcibly
serialize halfway through `Task.Delay`. A `200` means the runtime committed the
checkpoint to the configured **in-memory** storage. On a cold instance, the runtime
can checkpoint empty state without initializing or executing the network.
Checkpointing is not a receipt for durable delivery of individual SSE notifications.

To demonstrate cancellation, configure a `5000ms` delay, admit a streamed batch,
and immediately call `POST /finalize?mode=Immediate`. `CalibrateSamplesAction` observes
its cancellation token. Already published notifications are not retracted, but
canceled work does not produce successful readings. Use `mode=Queued` for normal
event-order finalization rather than immediate intervention.

Closing a Scalar/curl output connection only removes its watch; it does **not**
cancel the Activity. Reset with `DELETE /stateflows/activities/Telemetry/demo` or
use a new instance ID to start again. Reinitialization restores the default settings.

## Scope and guarantees

Stream admission is volatile and may precede execution failures. Output SSE is
future-only, has a bounded consumer buffer, and offers no reconnect cursor or
exactly-once delivery. Quiescent network persistence and explicit checkpoints do
not make this in-memory host restart-safe. Samples and outputs accumulate in
Activity state; this is a short-running demonstration, not a perpetual telemetry
archive. Use external persistence/consumers and lifecycle management in real systems.

No automatic sample producer is included: a background generator would hide the
HTTP input admission and make Scalar-driven comparisons less clear.
