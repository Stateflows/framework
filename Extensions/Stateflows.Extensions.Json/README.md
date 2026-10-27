# JSON state machines, activities and structural events

## Structural events and requests

Use `Stateflows.Events.Json.JsonEventDefinition.Parse(json, options).Register(eventsBuilder)`
inside `stateflows.AddEvents(...)`. Registration uses the core event builders, not a separate
JSON-specific event registry. `Parse` binds the complete definition before registration;
invalid definitions throw `System.Text.Json.JsonException`. Core registration errors remain
core exceptions. The standalone Draft 2020-12 [event schema](event.schema.json),
`urn:stateflows:event:1.0`, describes this format and is embedded, copied to the output
directory, and included in the package's `schemas` directory alongside the behavior schemas.
Associate it with event definition files through your editor's schema configuration.
The strict event parser does not accept a `$schema` property inside the definition.

The schema checks document shapes, field types, collection and enum rules, response
reference exclusivity, and built-in validator options. Parsing and core registration
remain authoritative for distinct sibling field names, `Compare` targets, validator
compatibility, ordered/representable limits, .NET regex syntax, and host-owned aliases
and custom validator options. Duplicate JSON properties are checked during parsing.

```csharp
using Stateflows.Common.Json;
using Stateflows.Events;
using Stateflows.Events.Json;

var options = new JsonOptions()
    .AddType<Receipt>("responses.receipt")
    .AddValidator("positive", configuration =>
        new System.ComponentModel.DataAnnotations.RangeAttribute(
            configuration.GetProperty("minimum").GetInt32(), int.MaxValue));

services.AddStateflows(stateflows => stateflows.AddEvents(events =>
    JsonEventDefinition.Parse(File.ReadAllText("order-event.json"), options).Register(events)));

// Resolve IEventFactory from DI. The default factory also implements IEventsRegister.
var payload = eventFactory.CreateFromJson("OrderSubmitted",
    """{"quantity":2,"address":{"city":"Paris","ignored":true},"tags":["new"]}""");
```

```json
{
  "formatVersion": "1.0",
  "name": "OrderSubmitted",
  "response": { "clrType": "responses.receipt" },
  "fields": [
    {
      "name": "quantity",
      "type": "Int32",
      "validation": [
        { "type": "Required" },
        { "type": "Custom", "alias": "positive", "options": { "minimum": 1 } }
      ]
    },
    {
      "name": "address",
      "fields": [{ "name": "city", "type": "String" }]
    },
    {
      "name": "tags",
      "type": "String",
      "isCollection": true,
      "itemValidation": [
        { "type": "StringLength", "options": { "maximumLength": 20 } }
      ]
    },
    {
      "name": "status",
      "type": "Enum",
      "enumValues": ["new", "paid"]
    }
  ]
}
```

The root requires `formatVersion: "1.0"`, a nonblank `name` and a `fields` array (possibly
empty). Omit `response` for an event; use exactly one of `response.eventName` for a
structural response or `response.clrType` for a host-registered `JsonOptions.AddType` alias.
Structural response names are resolved by the core when configuration completes.
CLR names, assembly names and arbitrary type loading are never accepted in place of aliases.

Every field requires a distinct, nonblank `name`. A missing or null `type` denotes an object
and requires nested `fields`. Scalar fields cannot declare `fields`. `isCollection: true`
creates a scalar or object collection. `enumValues` is required only for `Enum` and contains
distinct, nonblank, case-sensitive strings. `validation` applies to the whole field;
`itemValidation` is supported only on scalar collections (the core object-collection builder
has no item-validation API). Nested fields can have their own validation.

Definitions reject unknown properties at every DTO level, unknown field/validator types,
invalid option shapes, duplicate field names and duplicate JSON properties at any depth.
JSON property names and type names are case-sensitive. Comments, trailing commas, additional
root values and nesting beyond 64 levels are rejected for both definitions and payloads.

### Validation allowlist

Each validator has `type`, optional `options` (an object, default `{}`), and optional
`errorMessage`. Only `Custom` additionally accepts `alias`. Built-in options are strictly
allowlisted; unrecognized options are rejected:

| `type` | `options` |
| --- | --- |
| `Required` | `allowEmptyStrings` (boolean, default false) |
| `StringLength` | required `maximumLength`, optional `minimumLength` (default 0), nonnegative integers |
| `MinLength`, `MaxLength` | required nonnegative integer `length` |
| `Range` | required `operandType` (`Int32`, `Int64`, `Decimal`, `Double`), numeric `minimum` and `maximum` |
| `RegularExpression` | required nonempty `pattern`, optional positive `matchTimeoutInMilliseconds` (default 2000) |
| `Compare` | required `otherProperty`, naming a declared sibling field |
| `EmailAddress`, `Url`, `Phone`, `CreditCard` | no options |
| `Custom` | host-defined object passed to the explicitly registered factory |

Range limits are checked against the selected numeric type and ordered; range conversion
uses invariant culture. Regex syntax and timeout are checked during parsing. No arbitrary
attribute constructors, property setters, resource types or CLR validators can be selected
by documents. Custom factories are trusted host code and must validate their own untrusted
options; their errors propagate. Factories run during `Parse`, and their returned attribute
instances are used during registration. Custom validators do not imply any generated
OpenAPI constraint support.

`CreateFromJson` performs shape/type decoding and delegates construction to
`IEventFactory.Create(name, fields)`. It does **not** run DataAnnotations: the normal core
event validation pipeline does that. Missing fields stay missing; explicit null is accepted
for every field and collection item, leaving `Required` to enforce presence/non-null.

### Payload decoding

Only declared fields are decoded; unknown fields are discarded recursively, including inside
object arrays. Duplicate JSON properties are still rejected even in discarded data.
Objects become core `StructuralValue` instances, and arrays become core collections.
There is no `$type` or other CLR metadata interpretation.

| Field types | JSON representation |
| --- | --- |
| `String`, `Enum` | string; enums must exactly match a declared value |
| `Boolean` | JSON boolean only |
| `Int32`, `Int64` | JSON integer token, checked against the corresponding CLR bounds |
| `Decimal`, `Double` | JSON number, decoded as the declared CLR type; out-of-range values and nonfinite doubles rejected |
| `Guid` | string in hyphenated `D` format |
| `DateTime`, `DateTimeOffset` | ISO 8601 string supported by `System.Text.Json`, preserving its normal kind/offset semantics |
| `TimeSpan` | invariant `c` format string, e.g. `"1.02:03:04"` |
| `DateOnly` | invariant `"yyyy-MM-dd"` string |
| `TimeOnly` | invariant `"HH:mm:ss"` with optional fractional seconds (up to seven digits) |
| `Binary` | canonical padded base64 string, no whitespace |
| object / collection | JSON object / array respectively |

Strings never coerce to numbers or booleans. Invalid payloads throw
`System.Text.Json.JsonException` with the field path for conversion failures.
The three-argument extension requires the factory to implement `IEventsRegister`, as the
default core implementation does. A custom factory with a separate registry can use
`factory.CreateFromJson(name, json, register)`.

### Referencing structural events from JSON behaviors

Register structural event definitions with `AddEvents` **before** registering behaviors that
reference them. Names are resolved against the host's event registry during behavior
registration; missing names produce `JSON-003` diagnostics at the `eventName` reference.
Documents cannot implicitly declare events by referring to them.

State-machine event/internal transitions accept either a CLR alias or a structural name
in each trigger reference, never both:

```json
{
  "kind": "event",
  "triggers": [
    { "eventName": "orderSubmitted" },
    { "clrType": "events.cancelOrder" }
  ],
  "target": "processed",
  "guard": { "clrType": "handlers.orderGuard" },
  "effects": [{ "clrType": "handlers.orderEffect" }]
}
```

Existing `clrType` formats are unchanged. A mixed list retains every declared trigger:
the CLR subset keeps its multi-trigger edge and each structural name receives a separate
name-routed edge with the same target, guards, effects, metadata and else/local settings.
Structural events with other names do not match these named edges. Duplicate named
triggers, mixing Completion with named triggers, or combining a CLR `StructuralEvent` /
`StructuralRequest` wildcard with named triggers are rejected.
An `else` transition containing a structural reference must declare exactly one trigger:
the core allows only one else edge per vertex. Mixed/multiple named else triggers fail
binding rather than silently dropping triggers.

Guards/effects for named triggers implement `ITransitionGuard<StructuralEvent>` /
`ITransitionEffect<StructuralEvent>`. In a mixed list the handler must also satisfy the
existing CLR trigger contracts; the loader checks the full list before constructing any
edges. Transition definitions must expose one contract covering the complete trigger set,
as before, and are applied to each expanded edge. Named transition visitors receive the
registered `EventDefinition`, not a CLR wildcard. This named JSON syntax applies to
transitions only: initializer and deferral trigger declarations still use `clrType`.

Activity accept-event nodes use exactly one of `eventType` (the existing CLR alias) or
`eventName` (the registered structural name), keeping the existing implementation binding:

```json
{
  "kind": "acceptEvent",
  "name": "receiveOrder",
  "eventName": "orderSubmitted",
  "implementation": { "clrType": "handlers.receiveOrder" },
  "flows": [{ "kind": "control", "target": "done" }]
}
```

`handlers.receiveOrder` must implement `IAcceptEventActionNode<StructuralEvent>`. Its
`ExecuteAsync(StructuralEvent, CancellationToken)` receives the decoded structural payload;
normal host DI and `IConfigurable<T>` binding still apply. The loader calls the core named
accept-event builder overload, so routing and visitor descriptors use the registered
event's name and definition. Time/send/publish nodes and activity initializers retain
their existing CLR-only `eventType` syntax.

## Behavior definitions

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
