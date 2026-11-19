# Event serialization hardening and dispatch acknowledgement plan

Status: design decisions approved as recorded below; implementation pending.
No framework or application implementation changes.

## Implementation checkpoints

- Baseline committed: approved plan and red regression tests.
- Startup validation implemented: opt-in builder API, manual event/token types,
  shared Newtonsoft contract analysis, samples, existing visitors and hosted/
  Orleans startup hooks. Nine focused validation tests pass.
- Dispatch/interception/inbox implementation and broader coverage remain pending.

Regression baseline now lives in
`Tests\Orleans.IntegrationTests\EventSerializationRegressionTests.cs` and
`Tests\Orleans.IntegrationTests\OwnerSendSerializationRegressionTests.cs`.
These are red regression tests: defective constructor/member contracts must
prevent startup, and detached runtime serialization failures must be logged
without blocking the sending effect. They fail until the corresponding
hardening is implemented. Valid-contract and low-level serialization controls
remain alongside them; do not make incompatible application events deserialize
successfully merely to satisfy tests.
The tests do not depend on application source or the proposed dispatch API.
When introducing AddStartupSerializationValidation, explicitly enable it in startup
rejection tests and add an opt-out control. Runtime failure-reporting tests must
also run with verification disabled to prove reporting is independent of it.

## Objectives and boundaries

- Detect incompatible event contracts before the host accepts work, with
  actionable diagnostics identifying the event, member and behavior registration.
- Cover exceptions and silent data loss, not just successful JSON deserialization.
- Validate activity token payload contracts as well as event contracts, using
  the same defect/error and unverifiable/warning policy.
- Serialization verification is opt-in through `AddStartupSerializationValidation()`.
  Without it, do not run contract discovery, startup verification, sample
  factories or additional runtime contract-verification guards. Applications may
  enable it conditionally in testing and omit it in production. Normal
  serialization and mandatory dispatch failure reporting remain active.
- Keep context `Send()` nonblocking. Awaiting target processing from a self-send
  would deadlock and is not an acceptable fix.
- Introduce the `DispatchAndReturn` header, equivalent
  `DispatchAndReturnAttribute` on event types, and `EventStatus.Dispatched` to
  separate confirmed dispatch from target processing.
- Report every detached dispatch/processing failure through a defined,
  observable framework path.
- Do not modify the application `Failed` event. Application developers fix their
  contracts using Stateflows diagnostics.
- Leave existing event/behavior registration mechanisms unchanged for event
  verification. Introduce independent discovery and configuration for it.
  The separately approved inbox feature adds behavior capacity configuration.
- Preserve existing processing-awaiting semantics when neither the header nor
  the event-type attribute is present.
  Do not silently change existing stream admission or transaction guarantees.

## Findings supporting the design

### Serialization is shared core behavior, not an Orleans event codec

The normal Orleans path is:

1. `BehaviorProxy.SendAsync()` wraps the payload and forwards the envelope with
   its headers.
2. `GrainBehavior.SendAsync()` calls `ToTypedEventHolder()` again. For an existing
   envelope with non-null headers, this performs a polymorphic JSON clone.
3. Conversion to `OrleansEventHolder` JSON-serializes the payload and headers.
   Orleans-generated codecs transport this envelope, including its JSON strings.
4. The grain converts the envelope back into an event using Newtonsoft.Json,
   before event validation/handler execution. Responses and notifications also
   use these envelope conversions.

Therefore an event can fail in core cloning before any grain call. Adding Orleans
`GenerateSerializer`/`Id` attributes to application events does not fix that.
`StateflowsJsonConverter` also supports legacy storage, staged sends and embedded
behavior trigger cloning, so the shared validation belongs in core/Common.

Relevant implementation:

- `Core\Stateflows.Common\Utilities\StateflowsJsonConverter.cs`
- `Core\Stateflows.Common\Extensions\ObjectExtensions.cs`, `ToTypedEventHolder`
- `Core\Stateflows.Common\Locator\BehaviorProxy.cs`
- `Core\Stateflows\Activities\StateMachines\Extensions\StateMachineActionExtensions.cs`,
  `RunEffectActionAsync`
- `Runtimes\Stateflows.Runtimes.Orleans\Classes\Stateflows\GrainBehavior.cs`
- `Runtimes\Stateflows.Runtimes.Orleans\Structs\OrleansEventHolder.cs`
- `Runtimes\Stateflows.Runtimes.Orleans\Classes\Grains\StateflowsBehavior.cs`

Isolated reproduction confirmed:

| Contract | Result |
| --- | --- |
| Multiple public parameterized constructors, no selected JSON constructor | JsonSerializationException |
| Single constructor with `errorMessages`, read-only property `Messages` | Deserialization succeeds, but `Messages` becomes null |
| JsonConstructor added without correcting that binding | Same silent data loss |
| Single constructor with matching `messages` parameter | Collection preserved |
| Selected constructor covering all serialized members | Tested members and standard nested exceptions preserved |

These are shapes for framework regression tests, not instructions to change the
application event. Standard exception coverage does not prove arbitrary custom
exception types are safe.

### Two detached sends currently conceal failures

`RunEffectActionAsync()` starts the embedded action without awaiting completion.
The action's `IOwnerBehaviorContext.Send()` also discards its send task.
The reproduction observed a trigger returning `Consumed` and the action returning
normally while the owner's event failed during core cloning.

`StateflowsBehavior.ProcessEventCoreAsync()` also converts the same envelope
multiple times. These conversions reconstruct event objects and can repeatedly
invoke constructors. Decode once per delivery attempt and reuse the payload.

## 1. Shared event contract discovery

Introduce a new, validation-only discovery mechanism. Do not extend
`EventsRegister.RegisterClr()`, change `IEventsRegister`, add validation callbacks
to existing registration/type-mapping builders, or alter event-name collision
handling and behavior registration semantics.

Use the existing visitors API to gather used event types after model building.
Implement validation visitors for the existing behavior-kind visitor contracts;
do not introduce discovery-provider abstractions, model inspection APIs or
direct model traversal as a parallel discovery mechanism.

Collect the visited type usages with CLR type, behavior class/version, usage
and available visitor path information. Keep this validation-only collection
simple; do not build a second event registration system. Visitor traversal must
not rerun registration delegates or invoke mutating type-mapping callbacks.
Do not scan every loaded type as a substitute for discovering actual usages.

Share type analysis across duplicate usages, but retain every usage path for
diagnostics. Keep structural definitions distinct from CLR contracts. If a
required usage is not exposed by existing visitor callbacks, document that
missing coverage in this Markdown file with the affected behavior/event usage,
unavailable callback information and resulting verification limitation.
Do not extend or otherwise change visitor APIs: a separate visitor API rework
is underway on another branch, and this work must avoid conflicts with it.
Do not work around gaps with direct model inspection or registration changes.
Explicit validation-only inputs can cover known types without claiming automatic
visitor coverage.

Include:

- State-machine triggers, mapped concrete triggers, deferrals and initializers.
- Embedded actions/activities, including declared consumed/produced events.
- Activity accept/send/publish nodes, initializers, and declared input/output
  payloads that actually cross an envelope or persistence boundary.
- Activity token types exposed by existing visitors: input/output pins, node
  input/output token declarations and typed object flows, including embedded
  activities. Verify token payloads and their nested contracts, not only the
  surrounding TokensInput/TokensOutput events or token-holder envelopes.
- Entity operation requests/responses and action event/token contracts where
  those form transported payloads.
- `IRequest<TResponse>` response types and structural-request CLR responses.
- Framework envelopes, built-in events and headers, including the new header.
- Nested member, collection element and dictionary key/value contracts.

Apply the same contract analysis and optional representative round trips to
activity tokens. Include token-specific usage paths in diagnostics so developers
can identify the activity, node/pin/flow and payload member where available.
Cover token-holder serialization and tokens inside input/output events, output
notifications and persisted activity state/checkpoints when those paths apply.
Runtime-derived token payloads require the same runtime checks and explicit
coverage warnings as runtime-derived event payloads.

Discover token types only through current visitor callbacks and explicit
validation-only inputs. Record missing token coverage in the Visitor coverage
gaps section; do not extend visitors, inspect models directly or change token
registration to capture it.

Preserve interfaces/abstract declared types without falsely rejecting valid
polymorphism. Track known concrete mappings; report runtime-only coverage as
unverified. Do not claim arbitrary `object`, interface implementations or events
constructed inside user delegates can be discovered statically.

Provide `AddStartupSerializationValidation()` on the Stateflows builder to enable
verification, with an optional configuration callback. Calling it with no
callback enables visitor-discovered event/token verification. The callback
supplements discovery with manual event/token declarations:

```csharp
services.AddStateflows(builder =>
{
    builder.AddStateMachines(/* ... */);
    builder.AddActivities(/* ... */);

    if (environment.IsEnvironment("Testing"))
    {
        builder.AddStartupSerializationValidation(validation => validation
            .AddEvent<OrderCompleted>()
            .AddEvent<FailureNotification>()
            .AddToken<ProcessingResult>());
    }
});
```

Also support `AddEvent(typeof(OrderCompleted))` and
`AddToken(typeof(ProcessingResult))`. Manual declarations include nested
contracts and deduplicate analysis while retaining event/token usage labels.
Use this validation-only configuration for additional concrete types and
verification samples as well. These declarations must not register routable
events or behaviors or change existing registration APIs. Do not infer the
environment inside the library; applications decide whether to call the opt-in.

Late-discovered types must be verified before first use; do not mutate a
supposedly complete startup validation catalog without validation.
Discover and validate local behavior contracts on every hosting silo; remote
discovery of a behavior class is not discovery of its full CLR contract.

## 2. Shared Newtonsoft contract verification

Place the analyzer alongside `StateflowsJsonConverter` in Common, with existing
visitor API integration in core. Use the same
effective Newtonsoft settings, resolver and
converter selection as the actual clone/transport operations. Do not duplicate
serializer rules using a parameterless-constructor heuristic.

Produce structured diagnostics with stable rule IDs, severity, type/member path,
behavior usage, serializer path and suggested repair.

### Static checks

- Resolve the actual JSON contract and constructor/creator selection. Detect
  ambiguous constructors, unusable creators and invalid annotated constructors.
- Inspect creator parameter binding using JSON names, types and applicable
  Newtonsoft attributes; distinguish defaulted parameters from lost state.
- Identify serialized members that cannot be restored by a selected constructor,
  setter or supported existing-instance population. In particular, diagnose the
  unmatched parameter/read-only collection case.
- Respect ignored members, opt-in contracts, private attributed members,
  constructor attributes, records, value types and custom converters.
- Recursively inspect nested contracts with cycle detection. A reference cycle
  in the type graph is not itself an error.
- Report potentially lossy settings, such as ignored runtime reference loops,
  without pretending a type graph proves an object has such a loop.
- Handle Newtonsoft special contracts, including `ISerializable` exceptions;
  ordinary DTO constructor rules must not reject them indiscriminately.

Definite contract defects fail startup. Unverifiable contracts emit startup
warnings and do not block startup; no explicit acknowledgement is required.
Potentially lossy or opaque contracts need explicit diagnostics and a documented
coverage state, not a green "verified" result. A custom converter or
runtime-derived value must not be silently treated as proof of round-trip
correctness. Optional verification samples strengthen coverage; supplied sample
round-trip/assertion failures are defects, not unverifiability warnings.

### Representative round-trip verification

Add opt-in sample factories and optional semantic assertions. Samples are
necessary to verify constructor bodies, custom converters, nested runtime types,
exception subtypes and non-default values that reflection cannot prove safe.

For each supplied sample:

1. Exercise the exact shared clone path.
2. In Orleans, exercise the actual outgoing/incoming envelope conversions,
   including declared type, runtime type, headers and metadata.
3. Assert restored type and metadata, and compare normalized serialized state.
   Do not compare raw JSON strings or assume reference equality.
4. Run user-provided assertions for semantic invariants that JSON comparison
   cannot establish. Detect an ignored/dropped non-default collection value.

Do not invoke arbitrary constructors with fabricated null/default arguments,
use uninitialized-object creation, execute behavior handlers, or send production
messages during startup validation. Sample execution is explicit user code;
document side-effect constraints and report factory/assertion failures clearly.
Do not label samples as exhaustive proof of all possible values.

## 3. Startup enforcement and runtime serialization guards

- When opted in, run shared validation once registration/model building is complete. Fail with
  one aggregated report rather than stopping at the first event.
- Honor the shared opt-in in the Orleans package using its
  configuration/lifecycle hooks, before readiness, discovery advertisement and
  default-instance startup sends. Verify actual lifecycle ordering with tests.
- Honor the same opt-in for hosted legacy/standalone configurations that use
  JSON cloning/persistence. Neither runtime registration nor manual event
  registration implicitly enables verification. Keep the analyzer usable
  independently.
- Do not validate unsupported abstract payloads as if they were concrete DTOs;
  expose incomplete coverage through warnings rather than fail startup solely
  because a contract is unverifiable.
- Cache analyzed contracts. Runtime cloning/serialization/deserialization must
  retain original exception details and add event ID, declared/runtime type,
  source/target and operation stage without logging payload contents.
- Verify newly encountered runtime contracts before dispatch. Do not perform
  arbitrary extra reconstruction on every normal send or promise detection of
  every value-dependent loss without a round-trip policy/sample.
- Decode an Orleans envelope once per delivery attempt. Reuse that decoded
  event for classification, validation and execution. Preserve response identity.
- Unsupported or incompatible historical envelopes fail explicitly with a
  diagnostic; preserve the existing legacy-envelope fallback for valid records.

An application cannot obtain absolute correctness for arbitrary user constructors
from startup type inspection. The contract is explicit coverage plus fail-fast
known defects, representative round trips, and visible runtime failures.

## 4. DispatchAndReturn: proposed public semantics

Add a serializable `DispatchAndReturn : EventHeader` and an equivalent
`DispatchAndReturnAttribute` for event types in Common. Append `Dispatched` to
`EventStatus` without renumbering existing values.

| Call | Neither header nor attribute | Header or event-type attribute present |
| --- | --- | --- |
| `IBehavior.SendAsync()` | Existing processing result | Await receiver admission; return Dispatched |
| `IBehavior.RequestAsync<T>()` | Existing response/result | Await receiver admission; return Dispatched, no response |
| `IBehavior.DispatchAsync()` | Adds header and calls SendAsync | Same dispatch-only contract |
| Context `Send()` | Remains void and nonblocking | Internally use dispatch-only admission and observe its completion |
| Context dispatch-awaiting API | Always dispatch-only for self/owner/parent safety | Same dispatch-only contract |

Add `Task<SendResult> DispatchAsync<TEvent>(TEvent event,
IDictionary<string, EventHeader>? headers = null)` as a default implementation
directly on `IBehavior`, following the existing interface convenience methods.
Copy the supplied headers into a new dictionary, ensure the DispatchAndReturn
header is present, and call `SendAsync()` with that dictionary. Preserve other
headers and do not mutate a caller-owned or read-only dictionary. The method is
an API convenience, not a second runtime dispatch implementation.

The additive context dispatch-awaiting API remains dispatch-only for injected
`IBehaviorContext`, `IOwnerBehaviorContext` and `IParentBehaviorContext`; delegate
to the located behavior's `DispatchAsync()`. Keep the existing void `Send()`
nonblocking. Use `DispatchAsync()` consistently as the context convenience name,
avoiding a context `SendAsync()` that might suggest waiting for processing.

### Event-type attribute equivalence

Applying `DispatchAndReturnAttribute` to an event type has the same effect as
adding the `DispatchAndReturn` header to each delivery of that type, for both
`SendAsync()` and `RequestAsync()`. Either opt-in enables the mode; both together
are idempotent. Header absence does not override an attributed event.

Resolve the policy from the actual event payload type, not the envelope type or
only the generic call-site type. Centralize policy resolution so proxies, runtime
adapters and context sends agree, including base/interface-typed sends. Cache
type metadata. Define the attribute for class/struct event types with one
instance per type and `Inherited = true`. Check inherited attributes explicitly
when resolving the runtime payload type: an attributed base event makes derived
events dispatch-and-return too. Sending through an interface does not change
that policy; .NET attribute inheritance does not imply interface-attribute
propagation.

Carry the resolved delivery preference in transport metadata so receivers do
not have to rediscover caller policy. Apply the same compatibility checks and
failure semantics regardless of whether the preference came from a header or
attribute. Do not silently drop the mode on unsupported transports.

The attribute is not a serialized payload member and does not participate in
event registration. Its presence does not make an invalid JSON contract valid.
If using the short attribute spelling `[DispatchAndReturn]` is ambiguous with
the header class, document/use `[DispatchAndReturnAttribute]` explicitly.

### Meaning of acknowledgement

`Dispatched` means the intended receiver has accepted an independently owned,
serializable message snapshot into its delivery queue. It does NOT mean:

- Merely scheduling a local task or handing bytes to an Orleans client proxy.
- Validation by behavior-specific business rules, execution or consumption.
- Transaction commit of the target behavior or successful response production.
- Durable delivery across process failure unless backed by an explicit durable
  inbox contract.

Serialization/decoding and header incompatibility errors occur before the ack.
Failure to locate a target, a full queue, shutdown, and transport failures must
remain explicit outcomes/exceptions, never a synthetic `Dispatched`.
State-dependent/business validation remains at processing time; dispatch-only
does not run user validators while the sender is waiting.

For requests, return the existing request result with status `Dispatched` and
no response. Keep existing `.Response` absent/default behavior; no new
`HasResponse` API or response-presence guarantee is required. Users are
responsible for checking status and handling absent responses, which Stateflows
already does not guarantee.

Do not retain a request-response waiter. A dispatch-only request's response is
not returned later through this task; use a separate correlation/notification
pattern if needed. Response-generation failures still enter the failure path.
Audit convenience APIs that immediately dereference `.Response` so they do not
crash, invent responses or introduce processing waits. Do not reject an
otherwise valid dispatch-only RequestAsync merely because it has no response.

Consume the resolved dispatch-control metadata before normal processing so the
current delivery's acknowledgement preference is not accidentally copied to
unrelated child sends. An attributed payload independently opts in again when
explicitly sent or forwarded as a new delivery. Preserve event IDs, sender,
tenant, declared/runtime payload type, TTL and application headers.

### Receiver admission must not depend on receiver execution

Do not implement this as `Task.Run(SendAsync)`, a one-way grain call, an early
return while discarding the processing task, or general grain reentrancy.
An ordinary grain method that waits in the mailbox behind the sender's active
self-call still deadlocks.

Recommended Orleans design to prototype:

- A narrow, transaction-suppressed, interleavable admission entry point on the
  behavior grain. It snapshots/decodes once, reserves queue capacity and
  acknowledges without touching active behavior execution state.
- Queue admission must not acquire a behavior execution/resource permit, await
  the current turn/transaction, or run user validators/handlers.
- Schedule draining through Orleans-supported scheduling/lifecycle mechanisms.
  Process sequentially through the existing execution/transaction path, without
  making the admission RPC wait for that drain.
- Keep admission synchronization/state separate from processing state. Do not
  reuse the stream path blindly: it can await activity initialization, ordinary
  activity idleness and resource permits.
- Prove active same-grain self-send works. If grain lifecycle/scheduling cannot
  satisfy these invariants cleanly, use a separate inbox grain; do not broaden
  reentrancy or share transactional state unsafely to force it to work.

For the legacy runtime, separate `EnqueueEventAsync()` acknowledgement from the
current wait on `ExecutionToken.Handled`. A queued item must own a processing
scope and completion observer that outlive the caller's scope. Standalone and
other transports must implement real admission or explicitly reject the header;
never silently await processing or fake acknowledgement.

### Capacity, durability, transactions and ordering

- Configure inbox capacity globally as the default, with a behavior-level
  override. The global default is 128 pending messages per behavior instance.
  Resolve effective capacity as behavior override, then global
  default; this is a per-behavior-instance inbox bound, not a shared global
  capacity pool.
- Add `SetInboxCapacity(int capacity)` to all behavior builder interfaces exactly
  along the fluent surfaces exposing `SetResourceName(string resourceName)`:
  actions, activities, state machines and entities, including typed,
  overridden/finalized and utility builder variants. Preserve each interface's
  fluent return type. Carry the optional override in the corresponding model
  and runtime metadata without coupling serialization discovery to registration.
- Put the default in shared runtime configuration with consistent effective
  behavior across hosted runtimes. Validate capacities as positive integers;
  document that the capacity counts pending inbox entries, not the currently
  executing entry. Reserve a slot atomically at admission and release it when
  dequeued; do not allow an unbounded auxiliary dispatch queue.
- Use bounded inbox admission with explicit configuration, validated at startup.
  Return `Overloaded` promptly when full. Waiting for space released only by the
  sender's current execution is another self-send deadlock.
- Do not confuse inbox capacity with existing resource concurrency/waiter limits.
  One receiver with all execution permits held must still acknowledge or reject
  promptly. Reserve capacity only; acquire execution permits in the drain.
- Preserve queue admission order for one target; do not claim a new global order
  across independent senders or existing processing-awaiting calls. Explicitly
  test and document interaction with ordinary calls, reset and finalize.
- The approved initial guarantee is receiver-confirmed, volatile admission, not
  durability. Bound shutdown draining, observe undrained/lost work, and document
  crash loss. Adding persistence is a separate reviewed delivery guarantee,
  not an implicit consequence of `Dispatched`.
- Persistent inboxes/recovery are out of scope for this release. Reuse existing
  event IDs and specify retry/deduplication behavior without promising
  exactly-once external effects.
- An acknowledgement timeout can mean delivery outcome is unknown, not that no
  enqueue occurred. Preserve identity on retries and test duplicate admissions.
- Reject `DispatchAndReturn + JoinTransaction`: dispatch-only cannot promise
  processing/commit in a transaction that its sender is waiting to finish.
  Suppress ambient transaction propagation for independent admission.
- `StageSending` retains after-commit semantics. Its dispatch acknowledgement
  cannot be awaited before source commit; do not disguise staging as dispatch.
  Use the existing staging/outbox mechanism for source-commit coupling.
- Initially reject `DispatchAndReturn + Stream` explicitly. Preserve existing
  `Accepted` stream semantics rather than invent precedence between two
  admission protocols.
- These incompatibilities apply equally to attribute-enabled deliveries.
- A processing operation may finish before the ack reaches its caller; the
  guarantee is that processing is not awaited, not that it starts afterwards.

## 5. No silent detached failures

Introduce one shared detached-send supervision mechanism, with runtime-specific
completion adapters rather than scattered `_ =` calls.

### Around-dispatch interception and default reporting

Add `EventDispatchAsync` to `IBehaviorInterceptor`, using the existing
`ProcessEventAsync` around-continuation technique and exposing the event and
headers as `NotificationPublishedAsync` does. The proposed signature is
`Task<EventStatus> EventDispatchAsync<TEvent>(IBehaviorActionContext context,
TEvent event, IDictionary<string, EventHeader> headers,
Func<Task<EventStatus>> next)`.

The terminal `next` performs actual dispatch through receiver admission, not
merely schedules a task. An interceptor can await `next` inside try/catch and
log, trace or report exceptions; it can also inspect unsuccessful statuses.
Retain the complete internal SendResult/validation data while exposing the
status-shaped continuation, rather than discarding validation to build the
pipeline. Follow the processing pipeline's registration order and protections
against repeated or unfinished continuation calls.

Wire this through `CommonInterceptor` and add a virtual `next()` default to
`BehaviorInterceptor`. Provide a default interface implementation calling
`next()` so existing direct `IBehaviorInterceptor` implementations remain source
compatible. Do not reuse fire-and-forget notification observation for this
around callback.

Place the pipeline before cloning/serialization and target lookup so the
original constructor failure and missing-target failure occur inside `next`.
Cover self/owner/parent and embedded dispatch without double interception
through nested convenience methods/proxies. Behavior-originated dispatch has an
`IBehaviorActionContext`; external calls still get default logging, and must not
fabricate a behavior execution context.

Default error reporting is mandatory even with no custom interceptor:
`ILogger.LogError` AND `Trace.WriteLine($"⦗→s⦘ Error message details")`, replacing
the placeholder with useful event ID/type, source/target, stage, status and
exception details, not serialized payload data. Route default reports through a
shared reporting boundary to avoid duplicate logs from nested layers. Emit the
default report where the actual dispatch failure is detected so a custom
interceptor handling it does not accidentally suppress baseline reporting.
Log negative admission statuses as failures as well as thrown exceptions.

This callback wraps dispatch, not detached processing after acknowledgement.
Post-ack failures use receiver-side processing interception/supervision and the
same default error reporting. They cannot be caught retroactively by a
completed EventDispatchAsync callback.

- Context `Send()` starts dispatch-only admission and registers its task with
  that supervisor. A developer needing immediate dispatch error propagation
  uses `DispatchAsync()`.
- Observe synchronous exceptions, faulted/cancelled tasks and unsuccessful
  admission statuses. Missing behavior lookup must be reported too.
- On the receiver, observe detached processing exceptions AND non-success
  terminal statuses, including invalid/rejected/aborted/failed/overloaded
  outcomes. Define status classification explicitly; successful deferral or
  initialization is not a failure.
- Report structured error logs, the mandatory trace output and dispatch
  interception/receiver-side diagnostics with
  event ID, source/target/tenant, stage, status/validation and exception cause.
  Use metrics/tracing without high-cardinality metric labels or payload logging.
- Do not make delivery-error reporting depend on sending another behavior event.
  Guard failing reporting callbacks; log their failure without hiding the
  original. Around interceptors retain their explicit exception-handling
  semantics, just as in ProcessEventAsync.
- Supervision owns appropriate lifetime/scope and shutdown accounting. Never
  leave callbacks accessing disposed caller scopes, AsyncLocal response holders,
  execution contexts or ambient transactions.
- Audit self/owner/parent context sends in common, action, activity and
  state-machine contexts; embedded effect action dispatch; staged delivery;
  deferred/background dispatch and relevant notification/outbox forwarding.
  Reuse existing supervised paths rather than changing reliable paths needlessly.
- A failure after `Dispatched` cannot fault the already-completed caller task.
  It MUST reach the receiver-side failure path. This is observable asynchronous
  failure, not a fictitious processing success.

Changing context `Send()` to request admission changes where completion is
observed, not its void/nonblocking contract. Do not make the enclosing transition
await target processing, and do not silently swallow exceptions to keep it green.

## 6. Compatibility and related surfaces

- Keep `Accepted` for stream admission and `Dispatched` for ordinary dispatch
  acknowledgement; update enum documentation and all relevant status switches.
- Update Minimal APIs to map Dispatched to HTTP 202. Avoid an immediate follow-up
  status query/notification wait that accidentally waits on the target and
  defeats dispatch-only semantics.
- Audit C#/TypeScript clients, result serializers, absent-response handling,
  HTTP transports, observability and status-based retry/success classification.
  Unsupported runtimes/transports must report unsupported use explicitly.
- Treat the header/attribute/status and admission grain method as a coordinated protocol
  change. Define mixed-version behavior; older receivers must not be interpreted
  as having acknowledged dispatch. Do not send unsupported RPCs blindly.
- Keep ordinary `SendAsync`/`RequestAsync`, structural events, old stored
  envelopes, response identity, transaction behavior and stream contracts intact.
- Document constructor binding with Newtonsoft attributes, startup diagnostic
  examples, validation-only sample configuration, coverage limits, admission guarantees,
  nonblocking contexts, result statuses and failure observation.

## 7. Implementation sequence

1. Move the isolated reproduction shapes into maintained framework regression
   tests, including the embedded effect action to owner path. Do not reference
   application source from repository tests.
2. Gather used events through the existing visitors API into a simple
   validation-only type/usage collection. Add separate verification configuration
   without changing existing registration or introducing model inspection APIs.
   Record visitor coverage gaps in this file without changing visitor APIs.
   Add the shared contract analyzer, startup integration and
   explicit runtime error context, sample verification and the
   one-decode-per-attempt correction.
3. Add header/attribute/status contracts, default-interface DispatchAsync and a minimal Orleans admission
   prototype. Prove deadlock freedom, resource independence and scope isolation
   before committing to the inbox implementation.
4. Implement globally configured/behavior-overridden inbox capacity, receiver
   queue ownership, processing supervision, lifecycle and legacy admission.
   Reject unsupported transport combinations explicitly.
5. Add EventDispatchAsync around interception and mandatory default logger/trace
   reporting. Add context DispatchAsync and migrate void context sends/embedded
   dispatch to the shared supervisor without changing their nonblocking nature.
6. Wire HTTP/client/status/diagnostic/documentation surfaces and compatibility
   handling. Verify the review decisions below before finalizing public APIs.

## 8. Required acceptance tests

### Serialization/startup

- Without AddStartupSerializationValidation, no automatic discovery/contract analysis
  or sample execution occurs. Existing startup remains unchanged, while actual
  dispatch failures are still reported.
- With AddStartupSerializationValidation(), visitor discovery is verified without
  manual declarations; callback-declared events/tokens supplement discovery.
  Conditional testing-only configuration does not enable validation in a host
  that omits the call.
- Multiple constructor ambiguity fails startup before readiness/default sends.
- Single matching constructor and immutable records pass.
- Mismatched creator parameters/read-only members produce actionable diagnostics.
- Adding JsonConstructor alone does not hide a binding error.
- Nested collections, dictionaries, custom JSON names, ignored members, custom
  converters, structural events, mapped runtime types and request responses.
- Defective activity token constructors/member bindings fail validation, including
  nested token payloads; valid immutable token contracts pass. Exercise tokens
  inside TokensInput/TokensOutput, output notifications and applicable persisted
  activity/checkpoint paths rather than testing only standalone token DTOs.
- Discover activity pin/node/flow token usages exposed by current visitors,
  including embedded activities. Undiscoverable token usages are documented as
  coverage gaps; opaque/runtime-derived token contracts warn without blocking
  startup solely for unverifiability.
- Shared types retain every usage path; embedded produced events are included.
- Discovery leaves existing registration APIs, registrations, name collision
  rules and type-mapping behavior unchanged. Validation-only type/sample inputs
  do not register routable events or behaviors, and discovery does not rerun
  registration delegates or mutate completed definitions.
- Event discovery uses existing visitor traversal, including embedded behaviors,
  rather than new discovery providers, direct model inspection or assembly scans.
  Visitor APIs remain unchanged, and any unavailable coverage is documented in
  this file rather than silently reported as verified.
- Representative non-default samples catch dropped messages and overwritten
  constructor state. Faulting sample factories produce explicit diagnostics.
- Arbitrary application constructors are not executed by static validation.
- Historical valid envelope fallback remains readable; corrupt envelopes fail
  visibly. Custom exception contracts are not assumed safe from base tests.
- A counting constructor confirms no repeated decoding within one attempt;
  request/response association still works.

### Dispatch, supervision and deadlock

- Await self-dispatch inside a busy state-machine handler; acknowledgement
  completes while a test gate prevents that handler from finishing. Release it
  and prove the message processes exactly as the queue contract specifies.
- Repeat for embedded effect action to owner, parent, activity and legacy paths,
  including a resource concurrency limit of one.
- Block target processing deliberately: SendAsync/RequestAsync with either opt-in
  return Dispatched, without a response; ordinary calls remain incomplete.
- Header-only, attribute-only and both together have equivalent results; test
  runtime-derived payloads sent through base/interface references, inherited
  attributes from base classes, proxy forwarding and transport metadata.
- IBehavior's default DispatchAsync delegates to SendAsync with the header,
  preserves all other headers and leaves caller-owned/read-only headers
  unchanged. Existing implementations need not implement the convenience API.
- Global capacity defaults and behavior overrides work for all four behavior
  kinds, including fluent builder variants. Invalid capacities fail explicitly,
  separate behavior instances have separate bounds, and capacity slots do not
  leak on dequeue/failure/shutdown.
- Queue full/shutdown return promptly, including self-send with a saturated
  queue. No admission waits on its own execution permit or source transaction.
- Serialization failure before ack faults an awaited dispatch; void Send logs
  and reports it without blocking the enclosing handler.
- EventDispatchAsync surrounds actual dispatch and can catch its exception,
  inspect event/headers/status, and preserves processing-pipeline continuation
  rules. Interceptor order, default-interface/base-class compatibility and
  single interception per dispatch are verified.
- With no custom interceptor, failures emit both ILogger error entries and
  the required trace prefix. Custom exception handling does not erase baseline
  reports, and nested reporting does not duplicate them. Tests must account for
  Trace calls being conditional on the TRACE compilation symbol.
- After-ack handler/validator/transaction failures and negative statuses reach
  receiver-side supervision and existing processing interception. Reporting
  callback failure is itself logged.
- No shared mutable payload/headers, leaked response AsyncLocal, disposed DI
  scope access, leaked leases or unobserved background task exceptions.
- Same-silo and two-silo cases, reactivation, shutdown and admission timeout
  uncertainty; validate documented volatile-admission and possible crash-loss
  semantics without assuming persistent recovery.
- JoinTransaction/Stream combinations fail explicitly; existing staged sends,
  stream Accepted behavior, transactions and behavior without either opt-in remain intact.
- Dispatch-only requests return Dispatched and an absent/default response using
  the existing result contract; do not wait for or synthesize a reply.
- Minimal APIs return 202 without waiting for processing via metadata queries.
- Older/unsupported runtime paths never return fabricated Dispatched results.

Use completion gates for timing assertions, not a latency-only proxy or sleeps.
Run focused Common/core/Orleans tests first, then directly affected HTTP/client,
transaction, staging and streaming suites.

## Approved review decisions

1. Core/Common analysis with opt-in startup enforcement is approved. Enable it
   only by calling AddStartupSerializationValidation, optionally with manual event/token
   configuration; applications may omit it in production. Event verification
   uses existing visitors and separate validation configuration, not changes to
   existing event registration or new model inspection mechanisms. Definite
   defects fail startup; unverifiable contracts warn and allow startup without
   requiring explicit acknowledgement. Visitor APIs must remain unchanged;
   record missing coverage in this Markdown file to avoid conflicts with the
   visitor rework on another branch.
   The same validation scope and severity rules include activity token payloads.
2. Add DispatchAsync directly on IBehavior as a default interface method that
   adds the header and calls SendAsync. Context dispatch awaiting is
   dispatch-only; void Send remains nonblocking.
3. Volatile receiver admission is sufficient for this release.
4. Dispatch-only RequestAsync returns Dispatched without a response. Handling
   absent responses is the user's responsibility; do not require HasResponse.
5. Inbox capacity has a global default and per-behavior override through
   SetInboxCapacity(int capacity), on every builder surface matching
   SetResourceName(string resourceName). The global default is 128 pending
   messages per behavior instance.
6. Add IBehaviorInterceptor.EventDispatchAsync with around continuation and
   event/header access; actual dispatch runs in next. Default failure reporting
   uses both ILogger.LogError and Trace.WriteLine with the `⦗→s⦘` prefix.
7. Check inherited DispatchAndReturnAttribute instances on event types.

## Remaining implementation details

- Finalize the shared options API for the approved global capacity default of
  128 and behavior overrides.
- Finalize the EventDispatchAsync signature against existing pipeline/result
  plumbing, preserving event/header access, around semantics and validation.
- Verify queue ordering relative to ordinary calls and mixed-version deployment
  handling without adding durability or exactly-once guarantees.

## Visitor coverage gaps

Existing visitor limitations (APIs deliberately unchanged):

- Action visitors expose consumed events/tokens through CustomEventAddedAsync
  but not ProducedEventTypes/ProducedTokenTypes. Declare produced payloads with
  AddEvent/AddToken in the validation callback.
- Arbitrary sends/publications inside user code are not discoverable; add them
  manually. This includes concrete values held in object/interface members.
- Activity visitors expose pins, flows, transformations and explicit event
  nodes, but not every internal node input/output declaration or persisted
  runtime checkpoint payload type. Declare additional contracts manually.
- Resolved type-mapper concrete expansions are not exposed consistently by
  visitor callbacks; the validator does not invoke mapping callbacks as a
  workaround. Declare concrete variants manually.
- Entity visitors expose field/projection templates and mutation/initializer
  payloads, not every generated transport wrapper or runtime implementation of
  projection interfaces. Opaque contracts warn; supply concrete types/samples.
- Deferral-specific usage locations and built-in/header transport inventories
  are not exhaustively exposed. No complete-coverage claim is made.

Discovery validates all usages surfaced by the implemented callbacks, including
separately registered embedded behaviors. Missing metadata is not filled by
direct model inspection or registration changes.
