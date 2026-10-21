# JSON state machines and activities

`Stateflows.Extensions.Json` loads declarative state machines and activities without bypassing their
fluent builder APIs. Standalone Draft 2020-12 schemas are embedded in the assembly and included in
the package's `schemas` directory:

- [State machines](state-machine.schema.json), `urn:stateflows:state-machine:1.0`
- [Activities](activity.schema.json), `urn:stateflows:activity:1.0`

## Registration

```csharp
using Stateflows.StateMachines.Json;
using Stateflows.Common.Json;

var options = new JsonOptions()
    .AddType<PaymentReceived>("events.paymentReceived")
    .AddType<AuditEntry>("handlers.auditEntry");

services.AddStateflows(stateflows => stateflows.AddStateMachines(machines =>
    machines.AddStateMachineJson(File.ReadAllText("order.json"), options)));
```

```json
{
  "$schema": "urn:stateflows:state-machine:1.0",
  "formatVersion": "1.0",
  "name": "Order",
  "version": 1,
  "initial": "waiting",
  "vertices": [
    {
      "kind": "state",
      "name": "waiting",
      "onEntry": [
        {
          "clrType": "handlers.auditEntry",
          "configuration": { "message": "Waiting for payment" }
        }
      ],
      "transitions": [
        {
          "kind": "event",
          "triggers": [{ "clrType": "events.paymentReceived" }],
          "target": "paid"
        }
      ]
    },
    { "kind": "final", "name": "paid" }
  ]
}
```

The example's `AuditEntry` must implement `IStateEntry` and `IConfigurable<TConfiguration>`,
where `TConfiguration` accepts the embedded `message` property. Runtime selection and other
Stateflows services are configured by the host as usual.

Aliases are host-owned, ordinal and case-sensitive. No assemblies or CLR aliases can be registered
by a document. The alias registry is snapshotted when a document is parsed.
Both loaders use `Stateflows.Common.Json.JsonOptions`; one registry can be shared across
state-machine and activity documents.

## Pipeline and diagnostics

1. Parse strict JSON, rejecting duplicate properties, comments, trailing commas, and trailing documents.
2. Validate against the embedded schema; reject unknown properties and incompatible element shapes.
3. Resolve every authored CLR alias, validate its selected interface capability, resolve names and
   trigger mappings, and validate configuration and metadata serialization.
4. Only after all binding errors have been checked, construct a graph and invoke selected CLR definitions.
   Add JSON declarations without implicitly replacing definition contributions.
5. Validate behavior references and the expanded graph using the existing SM rules.
6. Register only a valid graph. A failed new version does not replace the current version.

The loader throws `JsonStateMachineException` with a `ValidationReport`. Document rules are
`JSON-001` (parsing), `JSON-002` (schema), `JSON-003` (CLR binding), `JSON-004` (data binding),
and `JSON-005` (expansion/composition). Expanded semantic diagnostics retain their `SM-*` identifiers.
Authored graph elements carry JSON Pointer paths; CLR-contributed elements carry their definition
binding's path. Invalid JSON also reports parser location information in its diagnostic message.

To retain warnings after successful registration:

```csharp
var definition = JsonStateMachineDefinition.Parse(json, options);
stateflows.AddStateMachines(machines =>
{
    var report = definition.Register(machines);
    foreach (var diagnostic in report.Diagnostics)
    {
        // Forward diagnostics to the host's logging or authoring UI.
    }
});
```

`Parse` validates JSON and schema only. `Register` performs CLR binding using the host's current
type mappers before creating the graph. Configuration data may be deserialized during binding;
executable handlers are not instantiated and definition methods are not invoked during that phase.
Reports accumulate errors within each validation phase; later phases do not run after binding errors.
CLR definitions are trusted executable host code: their arbitrary external side effects cannot be rolled back.

## Explicit CLR capabilities

`name`, `initial`, and `target` accept a literal name or `{ "clrType": "alias" }`.
A CLR name reference selects identity only, using existing Stateflows naming rules, never `Type.Name`
or a search for a vertex's implementation class.

| Location | Selected contract |
|---|---|
| Machine name / definition | `IStateMachine` |
| Simple-state name / definition | `IState` / `IStateDefinition` |
| Composite name / definition | `ICompositeState` / `ICompositeStateDefinition` |
| Orthogonal name / definition | `IOrthogonalState` / `IOrthogonalStateDefinition` |
| Final / history name | `IFinalState` / `IHistory` |
| Other pseudostate name or general vertex reference | `IVertex` |
| `onEntry` / `onExit` | `IStateEntry` / `IStateExit` |
| `onInitialize` / `onFinalize` | `ICompositeStateInitialization` / `ICompositeStateFinalization` |
| Transition definition | `ITransitionDefinition<TEvent>`, including its default specialization |
| Transition guard / effect | `ITransitionGuard<TEvent>` / `ITransitionEffect<TEvent>` |
| Deferral guard | `IDeferralGuard<TEvent>` |
| Default initializer / event initializer | `IDefaultInitializer` / `IInitializer<TEvent>` |
| Machine finalizer | `IFinalizer` |
| Observer / interceptor / exception handler | Corresponding state-machine interface |

Every capability is referenced separately, even if one class implements several.
Selecting a definition does not attach that class's executable interfaces. A definition can itself
explicitly contribute handlers through its builder.

Root, state, composite, and orthogonal `definitions` invoke only the selected static contract.
Transition definitions invoke their instance contract on a constructor-free definition object,
with any explicitly supplied configuration applied before `Build`.

Definitions run in declaration order before JSON additions in their respective scopes. Array order
is preserved for handlers, transitions and effects. Definition-contributed vertices, metadata keys,
initial selections and hosted behaviors cannot be silently replaced by JSON. Explicit overrides
and JSON inheritance/patch operations are deferred.

## Configuration

Every CLR instance-capability binding accepts:

```json
{
  "clrType": "handlers.auditEntry",
  "configurationType": "configuration.audit",
  "configuration": { "message": "Entering" }
}
```

`configurationType` is a registered alias for the `TConfiguration` of an implemented
`IConfigurable<TConfiguration>` contract. It is optional when there is exactly one contract,
required to disambiguate multiple contracts, and invalid without `configuration`.
Unknown configuration members and incompatible values are errors.

Configuration is embedded serializable data, not a host-side callback. It is deserialized afresh
for each configured instance; selecting the same CLR class for entry and exit does not share or
overwrite configuration. Omitted configuration preserves the element's normal initialization.
Static definitions cannot consume instance configuration. Name and trigger references are
identity-only and do not accept configuration.

Configurable script elements use the same binding mechanism. Their scripts are data; script
contents are not inspected or validated.

## States, regions, and pseudostates

Supported kinds: `state`, `composite`, `orthogonal`, `final`, `choice`, `junction`, `fork`, `join`,
and `history`.

- A container's `initial` selects a directly contained state and derives its internal initial kind.
  There is no separate initial pseudostate. A root needs an initial state, supplied by JSON or a definition.
- A composite accepts a single `region`; an orthogonal state accepts ordered `regions`.
  Regions contain `vertices`, optional `initial`, and optional `history`.
- A region's `history` selects a directly contained history vertex; omission preserves first-declared selection.
  Multiple history vertices and final states are supported.
- Regions do not own handlers or hosted behaviors.
- Regional states additionally accept `onInitialize` and `onFinalize`.
- Final states accept only `kind`, `name`, and `metadata`.
- Pseudostates accept metadata and default transitions, but not state handlers, regions or deferrals.
  Fork/join topology, history boundaries, guarded alternatives, and fallback rules are graph validation concerns.

## Transitions, guards, and deferrals

`kind` is `event`, `default`, or `internal`. Event and internal transitions require nonempty `triggers`.
Default transitions use Completion implicitly and forbid `triggers`. Internal transitions forbid
`target` and `local`; targeted transitions require `target` and optionally select `local` (default true).
Completion-triggered internal transitions require a guard.

```json
{
  "kind": "event",
  "triggers": [
    { "clrType": "events.paymentReceived" },
    { "clrType": "events.paymentConfirmed" }
  ],
  "target": "paid",
  "guard": {
    "all": [
      { "clrType": "guards.amountAccepted" },
      {
        "any": [
          { "clrType": "guards.hasCredit" },
          { "clrType": "guards.hasVoucher" }
        ]
      }
    ]
  },
  "effects": [{ "clrType": "effects.recordPayment" }],
  "metadata": { "description": "Accept payment" }
}
```

A trigger list defines one logical transition, not multiple independently ordered transitions.
The host's type mappers are honored, including OneOf when `AddOneOf()` is registered before loading.
Duplicate or overlapping mapped types inside one trigger list are rejected.
CLR handlers must support every mapped trigger, either through compatible event interfaces or a
declared union interface. A transition definition needs one definition contract covering the whole set.
Language-specific union syntax is not part of the JSON format.

`else: true` marks a fallback, forbids authored guards, and is evaluated after ordinary alternatives.
Otherwise declaration order determines evaluation order. `all` / `any` are ordered, nonempty,
short-circuiting guard groups.

Deferrals are ordered declarations containing `triggers` and an optional `guard`.
Multiple triggers share one guard expression; omitting it means unconditional deferral.
Forbidden deferred event types and conflicts with outgoing transitions use existing graph validation.

## Behavior-class handlers and hosted behaviors

Entry, exit, regional lifecycle, effect, and guard slots can reference registered actions or activities:

```json
{
  "behaviorClass": {
    "type": "Action",
    "name": "AuditOrder"
  }
}
```

This is an alternative to a CLR handler binding, not an alternative spelling of a CLR class.
The enclosing slot selects invocation semantics. Guards use the existing delegated Boolean-result
protocol; producing the result is the behavior's responsibility. The loader does not analyze
arbitrary behavior code to prove its output.

A state's single `do` slot also uses a behavior-class reference. Simple states can host
actions, activities or submachines; composite and orthogonal states support do-actions and do-activities, not submachines.
Custom registered action behavior kinds are accepted in `do` as supported by the existing graph validator.
Behavior-backed handler slots support standard `Action` and `Activity` kinds.

Referenced behaviors must already be registered when expansion completes. References currently
require version 1, matching existing hosted-behavior discovery. `BehaviorClass` contains type and name,
not an instance ID or version. No inline behavior definitions or automatic CLR behavior registration
are inferred from these references.

## Metadata and polymorphic data

Every vertex and transition, including final states and pseudostates, accepts `metadata`,
mapped to `Dictionary<string, object>`. Native JSON values need no discriminator.
Explicit CLR data types use host-registered `$type` aliases:

```json
{
  "metadata": {
    "description": "Waiting",
    "display": { "$type": "metadata.display", "label": "Waiting", "color": "blue" },
    "count": { "$type": "system.int32", "$value": 42 },
    "durations": { "$type": "metadata.timeSpans", "$value": ["00:00:01", "00:00:05"] }
  }
}
```

Typed objects contain `$type` and serialized properties. Typed scalars and collections use
`$type` with `$value` only. Nested polymorphic values are supported in metadata and configuration.
Any host-permitted serializable CLR data type may be selected; there is no hardcoded metadata type list.
Unknown aliases, assembly-qualified discriminators and incompatible data are errors.
`$id`, `$ref`, and `$values` are reserved; reference-preserving serialization is not supported.

Untyped metadata objects become dictionaries, arrays become object arrays, and scalar values retain
their JSON data meaning (date-looking strings are still strings). Use `$type` when CLR numeric or
other exact data types matter. The top-level machine and regions do not introduce metadata stores
that do not exist in the current model.

## Activities

Register a document with host-owned aliases for each CLR capability, token, event, exception,
and configuration type:

```csharp
using Stateflows.Activities.Json;
using Stateflows.Common.Json;

var options = new JsonOptions()
    .AddType<ProduceOrder>("nodes.produce")
    .AddType<ValidateOrder>("nodes.validate")
    .AddType<Order>("tokens.order")
    .AddType<ValidOrderGuard>("flows.valid")
    .AddType<NormalizeOrder>("flows.normalize")
    .AddType<NormalizeConfiguration>("configuration.normalize");

services.AddStateflows(stateflows => stateflows.AddActivities(activities =>
    activities.AddActivityJson(File.ReadAllText("order-activity.json"), options)));
```

An activity document declares root nodes in `nodes`; structured, parallel, and iterative nodes own
recursive `nodes` arrays. Special initial/input/output/final nodes use the builder's generated names.
An optional `name` on a special node is a document-local target alias only; it does not change the
name reported by the builder or visitors. Every flow is authored under its source node and resolves
its target in that same scope.

```json
{
  "$schema": "urn:stateflows:activity:1.0",
  "formatVersion": "1.0",
  "name": "Order",
  "definitions": [{ "clrType": "activities.orderDefaults" }],
  "nodes": [
    { "kind": "initial", "flows": [{ "kind": "control", "target": "produce" }] },
    {
      "kind": "action",
      "name": "produce",
      "implementation": { "clrType": "nodes.produce" },
      "flows": [{
        "kind": "object",
        "tokenType": "tokens.order",
        "target": "validate",
        "pipeline": [
          { "kind": "guard", "handler": { "clrType": "flows.valid" } },
          {
            "kind": "transform",
            "outputTokenType": "tokens.order",
            "handler": {
              "clrType": "flows.normalize",
              "configuration": { "trimLines": true }
            }
          }
        ]
      }]
    },
    {
      "kind": "action",
      "name": "validate",
      "implementation": { "clrType": "nodes.validate" },
      "flows": [{ "kind": "control", "target": "finished" }]
    },
    { "kind": "final", "name": "finished" }
  ]
}
```

Supported node kinds are `initial`, `input`, `output`, `final`, `action`, `decision`, `fork`,
`join`, `merge`, `dataStore`, `acceptEvent`, `timeEvent`, `sendEvent`, `publishEvent`, `structured`,
`parallel`, and `iterative`. Flow kinds are `control`, `object`, `elseControl`, and `elseObject`.
Object-flow `pipeline` stages run in array order. A guard must implement `IFlowGuard<T>` for the
token type at that point; a transformation must implement `IFlowTransformation<TInput,TOutput>`.
The output token alias declares the transformation's result type and becomes the type seen by later
stages and the destination. Control flows accept control guards only; else flows cannot contain
guards. Weights default to one and must be nonnegative.

CLR `definitions` are additive. Root definitions implement `IActivity` and expose static
`Build(IActivityBuilder)`; nested structured definitions implement `IStructuredActivityNodeDefinition`
and expose static `Build(IStructuredActivityBuilder)`. They run in array order before JSON
declarations in their scope. The loader rejects duplicate declarations instead of silently replacing
definition contributions. Overrides are not supported.

Node implementations and flow stages select one explicit CLR role each. A type implementing
additional Stateflows interfaces does not cause those other roles to be selected by the document.
For typed action nodes, required/optional input and output token contracts follow the existing
builder's constructor-interface scanning and are checked by `Graph.Build()`. Event type aliases are
resolved through the host's configured type mapper. Activity nodes and flows currently have no
metadata surface, so activity JSON does not define metadata.

`JsonActivityDefinition.Parse` validates strict JSON and schema shape. `Register` validates all
host aliases, interface roles, generic token/event compatibility, and configuration before creating
the activity. Definitions and JSON nodes are then composed using the public activity builders, so
visitor-based integrations observe the same registrations as fluent-built activities. Finally,
`Graph.Build()` validates the expanded activity. Failures throw `JsonActivityException` with an
accumulated `ModelValidationReport`.
