# Order fulfillment saga

A standalone, API-only example using Stateflows on an Orleans silo. Scalar is the
only interactive UI; there is no application frontend or Orleans dashboard.

**Development only.** Orleans grain storage and reminders are in memory. All
orders, receipts, subscriptions and progress disappear when the host stops.
The host refuses to start outside the `Development` environment. No real payment,
inventory, shipping, email or invoicing service is called.

## Run

Requires the .NET 10 SDK. From the repository root:

```powershell
dotnet run --project .\Examples\Stateflows.Examples.OrderFulfillment\Stateflows.Examples.OrderFulfillment.csproj
```

The launch profile sets `Development` and listens on `http://localhost:5155`.
Open `http://localhost:5155/scalar/v1`; the OpenAPI document is at
`http://localhost:5155/openapi/v1.json`. The adjacent `.http` file contains runnable
requests. Use a new order instance ID for each submission.

The silo uses localhost ports `11115` and `30005`, separate from the existing
Orleans example. Override `Orleans:SiloPort` and `Orleans:GatewayPort` through
configuration when running another copy. This is a single-silo local demo, not
a production cluster configuration.

## Workflow

```text
SubmitOrder
  Processing
    Checking
      AuthorizingPayment -> PaymentReady    (do-Action)
      ReservingStock     -> StockReady      (do-Activity)
    [payment authorized AND stock reserved]
    PreparingShipment                      (do-Activity: packing list -> pack/label)
    CapturingPayment                       (do-Action)
    DispatchingShipment                    (do-Action)
  InTransit                                (do-Action)
    failure -> NeedsAttention --RetryDelivery--> InTransit
  Invoicing                                (transition-effect Activity)
  Completed

CancelOrder in Processing, or a pre-dispatch failure
  -> Compensating                           (do-Action)
     refund captured payment OR void authorization
     release reserved stock
     -> Cancelled / Failed
     failure -> CompensationNeedsAttention --RetryCompensation--> Compensating
```

`Checking` has two orthogonal regions. A guarded default transition reads the
Entity projection and proceeds only after both successful results have been
applied. There is no polling service that drives workflow transitions.

Cancellation is accepted only before dispatch is committed by the state machine.
At the dispatch boundary, whichever event the machine processes first wins:
accepted cancellation compensates the order; after dispatch, cancellation returns
`409`. Exhausted delivery retries never release shipped stock or refund payment.
Instead, an operator retries delivery from `NeedsAttention`.

`Completed`, `Cancelled` and `Failed` are stable business-terminal states, not
UML Final States. This keeps status, summaries and custom diagram endpoints
inspectable. They accept no business commands. API reset/finalize operations are
disabled because they would bypass saga compensation.

## Behavioral group and shared data

| Surface | Responsibility |
|---|---|
| `OrderFulfillment.cs` | Core lifecycle, orthogonal regions, guards, cancellation boundary, compensation and recovery transitions. |
| `OrderStates.cs` | Typed stage states inherit `OrderStage`; its `IStateEntry` implementation uses injected `IStateContext` and `IBehaviorContext` to record the registered state name automatically. |
| `OrderTransitions.cs` | Typed transitions consume `IReasonable` / `IOperationEvent` contracts and inject `IBehaviorContext`. Successful-step transitions combine the operation guard and mutation effect; default-transition guards check shared Entity data. |
| `IOrderData` in `OrderEntity.cs` | One owner-scoped Entity per order: submitted lines, resource states, attempts, invoice and audit history. Fields are not publicly writable. |
| `ReserveStockActivity` | Inventory inspection and reservation; nodes read and mutate the owner's Entity. |
| `PrepareShipmentActivity` | Token flow from packing-list calculation to packing/label preparation, using the same Entity. |
| Typed `IAction` workers in `Workers.cs` | Payment, dispatch, delivery, compensation and audit workers with constructor-injected owner contexts. |
| `IOrderSimulator` / `OrderSimulator` | Replaceable DI service shared by Actions and typed Activity nodes; deterministic delayed outcomes with bounded retries. |
| `IssueInvoiceActivity` | Invocation-scoped transition effect with typed invoice tokens; stores the invoice and sends `InvoiceReady` to the owner. |
| Effect Actions | Invocation-scoped cancellation and completion audit updates through the owner context. |

States and transition targets use typed overloads. The named choice and worker
diagram lookups use `State<T>.Name` where their APIs require a string. Container
and parallel leaf states use marker types without stage-entry behavior.

Children use `TryGetOwnerBehaviorContext`, then checked
`TryGetProjectionAsync<IOrderData, OrderSnapshot>` /
`TryMutateAsync<IOrderData, TMutation>` operations. They do not share mutable
in-memory service objects or use Behavior Values as a substitute for the Entity.
`OrderData` helpers turn unsuccessful Entity operations into explicit errors.

Do-Actions and effect Actions are registered by type, just like Activities.
Workers inject `IOwnerBehaviorContext` and, where needed, `IOrderSimulator`.
Activity nodes implement `IActionNode` and inject their owner, simulator and
input/output token interfaces. The simulator is registered as a transient service;
it holds no per-order mutable state and can be replaced when testing workers.

Transitions still subscribe to concrete event types. Contravariant typed effects
and guards reuse their interface contracts without broadening the accepted events.
`CancelOrderTransition` and `FailOrderTransition` reuse `StopOrder` with distinct
cancellation semantics. Operation-specific transitions inherit `ApplySuccessfulStep`,
which implements both the guard and effect, and are registered directly as
`AddTransition<StepSucceeded, PaymentAuthorized, PaymentReady>()` (and equivalents).
The effect commits the successful operation only after its guard accepts it.
These effects remain awaited by the state machine.

`OrderSnapshot` is published with `PublishScope.Owner`, so notification clients
watch the order rather than needing an embedded Entity ID. Arrays and records
are replaced on mutation so projection dependencies see the changes.

**Effect dispatch is asynchronous.** Resource acquisition is committed through
awaited inline transition effects, not by assuming an embedded effect Action has
already finished. Simulators only calculate outcomes and record attempts.
Therefore a delayed result arriving after cancellation cannot acquire resources.
Invoice work explicitly acknowledges completion with `InvoiceReady`; audit
effect Actions are eventually consistent and may append after a terminal state
has been entered.

## Deterministic scenarios

Select `scenario` on `SubmitOrder`; no random outcomes or external setup are
involved.

| Scenario | Outcome |
|---|---|
| `HappyPath` | Authorization, reservation, preparation, capture, dispatch, delivery and invoicing succeed. |
| `PaymentDeclined` | Authorization permanently fails; any accepted reservation is released. |
| `OutOfStock` | Reservation permanently fails; any accepted authorization is voided. |
| `PreparationFailed` | Preparation permanently fails; void authorization and release stock. |
| `CaptureFailed` | Three transient capture failures; void authorization and release stock. |
| `DispatchFailed` | Dispatch permanently fails after capture; refund payment and release stock. |
| `TransientFailures` | The first attempt of each operation fails transiently; the second succeeds. |
| `DeliveryNeedsAttention` | The first three delivery attempts fail; `RetryDelivery` allows attempt four to succeed. |
| `CompensationNeedsAttention` | Dispatch fails; the first three refund attempts fail. `RetryCompensation` completes refund and stock release. |

Each worker execution allows at most **three attempts**. Permanent business
failures are not retried. Transient failures use a bounded, linear backoff of
`callbackDelayMilliseconds * attempt` between attempts. Attempt numbers are
cumulative per operation across operator retries; each retry starts another
bounded execution, not an infinite automatic loop.

`callbackDelayMilliseconds` defaults to `500` and accepts `100` through `5000`.
Use `2000` or more to observe stages and cancel through Scalar. Worker code
observes its execution cancellation token, but cancellation correctness does not
depend on an in-flight simulated request stopping immediately.

## HTTP contract

All order-specific routes below start with
`/stateflows/stateMachines/OrderFulfillment/{instance}`.

| Method / suffix | Purpose |
|---|---|
| `POST /submitOrder` | Initialize a new order from a validated command. |
| `POST /cancelOrder` | Request compensation before dispatch; body contains a reason. |
| `POST /retryDelivery` | Retry delivery only from `NeedsAttention`. |
| `POST /retryCompensation` | Retry unfinished compensation only from `CompensationNeedsAttention`. |
| `GET /status` | Current Stateflows lifecycle, active states and expected events; `?stream=true` uses SSE. |
| `GET /summary` | Shared Entity projection, including attempts, invoice and audit entries. |
| `GET /notifications?names=OrderSnapshot&stream=true` | Stream changed order projections published through the owner. |
| `GET /diagram` | State machine PlantUML source. |
| `GET /diagrams/reservation` | Reservation Activity PlantUML after that worker has started. |
| `GET /diagrams/preparation` | Preparation Activity PlantUML after that worker has started. |

Nonempty event commands use the Stateflows envelope:

```json
{
  "event": {
    "customerEmail": "buyer@example.test",
    "lines": [
      { "sku": "BOOK", "quantity": 2, "unitPrice": 12.50 }
    ],
    "scenario": "HappyPath",
    "callbackDelayMilliseconds": 500
  }
}
```

Empty commands such as `RetryDelivery` accept `{}`. Successful submission returns
`201`; accepted transitions return `200`, invalid commands `400`, and commands not
accepted in the current state `409`. Acceptance is not background completion:
read the summary or watch status/notifications for the eventual outcome.
Stateflows envelopes include `statusText` even when their status enum is numeric.
Summaries and custom endpoints require an initialized order; they return `404`
for a missing instance.

FluentValidation rejects invalid emails, missing/null/duplicate lines, unknown
scenarios, invalid delays, nonpositive quantities/prices and prices with more
than two decimal places. Orders have at most 50 unique SKU lines and quantities
up to 1000 per line. Totals are `decimal` sums with no tax, exchange rates,
shipping fees or currency conversion.

Worker commands and Entity mutations are deliberately not exposed. The generic
Activity, Action and Entity endpoints are disabled, and internal callback/mutation
events carry `[NoApiMapping]`. Only use fake data: the local development API has
no authentication, and console traces can contain workflow metadata.

## Telemetry and diagrams

The Stateflows OpenTelemetry extension instruments all four behavior kinds.
ASP.NET Core instrumentation adds HTTP spans. The `Stateflows` meter reports
execution counts, duration and running/waiting work; `Stateflows.Orleans` is
subscribed for runtime failure/admission diagnostics, which need not appear on a
healthy run.

Traces and metrics use console exporters by default. To export to your own local
collector, set `OTEL_EXPORTER_OTLP_ENDPOINT` (and, when appropriate,
`OTEL_EXPORTER_OTLP_PROTOCOL`) before starting; the host then uses OTLP instead.
No telemetry backend, collector or dashboard is required by the example.

Diagram endpoints return only PlantUML text. They never fetch the public
PlantUML rendering URLs. Render locally or with a trusted private renderer.
An Activity must have started before its diagram is available; diagram reads do
not initialize workers.

## Verification

```powershell
dotnet test .\Tests\OrderFulfillment.IntegrationTests\OrderFulfillment.IntegrationTests.csproj -p:GeneratePackageOnBuild=false
dotnet test .\Tests\StateMachine.IntegrationTests\StateMachine.IntegrationTests.csproj --filter FullyQualifiedName~NestedInitialOrthogonalState -p:GeneratePackageOnBuild=false
```

The integration suite starts the real Orleans runtime with in-memory providers
and exercises HTTP commands, all scenarios, retry limits, compensation,
cancellation in every pre-dispatch phase and at the dispatch race, concurrent
order isolation, validation, API exposure, Scalar, PlantUML and telemetry.

This is not a production saga implementation. Inventory is simulated per order,
not deducted from a shared warehouse; callbacks and backoff use in-process
asynchronous work, not crash-resumable scheduling. Durable Orleans storage alone
would not make these workers recoverable. Real adapters require idempotent
provider operations, correlated/deduplicated callbacks, timeouts, reconciliation,
durable retry scheduling, authentication and a deployment/storage design.

## Reference

The design follows the framework wiki:
[Behavioral Groups](https://github.com/Stateflows/framework/wiki/Behavioral-Groups),
[Entities](https://github.com/Stateflows/framework/wiki/Entities),
[Orleans Runtime](https://github.com/Stateflows/framework/wiki/Orleans-Runtime),
[Minimal APIs](https://github.com/Stateflows/framework/wiki/Extension-Minimal-APIs),
[FluentValidation](https://github.com/Stateflows/framework/wiki/Extension-FluentValidation),
[OpenTelemetry](https://github.com/Stateflows/framework/wiki/Extension-OpenTelemetry)
and [PlantUML](https://github.com/Stateflows/framework/wiki/Extension-PlantUML).
