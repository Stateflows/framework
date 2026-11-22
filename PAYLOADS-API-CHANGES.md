# Payloads and payload tokens — proposed API changes

This is the API review document for the work described in
[PAYLOADS-PLAN.md](PAYLOADS-PLAN.md); test coverage for it is planned in
[PAYLOADS-TEST-PLAN.md](PAYLOADS-TEST-PLAN.md). It lists **public API only**:
signatures, names, validation rules and observable behavior. No implementation, file
layout or sequencing is described here. Nothing in this document has been implemented.

## How to read this document

- `Payload` means the renamed `StructuralEvent` (decision D1): an instance of a
  dynamically defined DTO, identified by a registered name.
- **Name sets.** Every place that accepts payload names takes
  `IReadOnlyList<string>` (accepts arrays, lists and C# 12 collection expressions:
  `["a", "b"]`). Each such member also has a **single-`string` sugar overload** of
  identical shape, not repeated below, **except** the members listed in sections
  4.4.2 and 4.6 whose single-name form would collide with an existing typed
  member (verified by a compile probe); those take lists only. A set is non-empty,
  has no duplicates and contains only registered names (rules in
  [section 6](#6-validation-rules)).
- **Vocabulary.** Inside the state machine, activity, Action and entity models a
  payload is always called a **token** or an **event**; "payload" names the
  model-independent definition mechanism (`AddPayloads`, `Payload`, `PayloadRequest`,
  `PayloadValue`, `PayloadDefinition`, JSON payload definitions). "Trigger" is used
  only for state-machine transitions; initializers and deferrals consume **events**.
  Model members for payload tokens are overloads of the typed names
  (`AddFlow(tokenNames, …)`, `AddInputPin(tokenNames, …)`), with parameters and
  properties named `tokenNames`, `iterationTokenNames`, `TokenNames`, … The only
  members that say `PayloadToken`, because a name without it would be ambiguous next to
  the typed queries, are: `IInput.GetPayloadTokens` / `HasPayloadTokens`,
  `IOutput.PassPayloadTokensOn`, `ITokensOutput.GetPayloadTokens` and the Action
  declarations `AddConsumedPayloadToken` / `AddProducedPayloadToken`.
- Lists are open: any number of names may be declared (decision D6).
- A declaration is either **CLR-typed** (including `ITypeMapper` multi-type events
  and tokens such as OneOf/UnionTypes) or **payload-named**, never both (decision D6; confirmed
  for the first version). The shapes below do not preclude mixing in a later version.
- **Attribute constructors take `params string[]`, not `IReadOnlyList<string>`.** The
  C# language does not allow `IReadOnlyList<string>` as an attribute constructor
  parameter (error CS0181; verified by compiling a probe), so `TokenNameAttribute`,
  `MutationAttribute` and `InitializerAttribute` take `params string[]` and expose an
  `IReadOnlyList<string>` property. Both `[TokenName("a", "b")]` and
  `[TokenName(["a", "b"])]` compile.
- **Names are always given at the call site (decision D29).** No class-level attribute
  advertises payload names. Wherever a class-based handler (initializer, deferral
  guard, entity mutation, flow guard, flow transformation) is registered for payloads,
  the registration method takes the names as a **required** parameter.
- **Flows carry two name sets (decision D30)**, mirroring the two token types a flow
  has today (`TokenType` and `TargetTokenType`): the **original names**, matched
  against the source node's tokens, and the **transformed names**, matched against
  the target node. Transformation-adding members gain overloads with a
  `transformedNames` parameter, used when the transformed type is `Payload` (decision
  D31; section 4.4).
- Existing typed members are not listed unless they change. Anything not mentioned
  keeps its current signature and behavior, apart from the identifier renames in
  [section 1](#1-renames).
- The provisional defaults I proposed are collected in
  [section 8](#8-proposed-defaults-awaiting-veto); all of them are now confirmed
  (items 10 and 13 as revised by you).

## Contents

1. [Renames](#1-renames)
2. [Payload definition API](#2-payload-definition-api)
3. [Event consumption parity](#3-event-consumption-parity)
4. [Payload tokens](#4-payload-tokens)
5. [Extensions: JSON, MinimalAPIs and others](#5-extensions-json-minimalapis-and-others)
6. [Validation rules](#6-validation-rules)
7. [Breaking and behavioral changes](#7-breaking-and-behavioral-changes)
8. [Proposed defaults awaiting veto](#8-proposed-defaults-awaiting-veto)
9. [Decisions recorded and remaining open questions](#9-decisions-recorded-and-remaining-open-questions)

---

## 1. Renames

Hard renames, no `[Obsolete]` forwarders (D2). No persisted-data compatibility (D3).
Identifiers that contain "Structural" are renamed (D7); identifiers that say only
"Event" stay.

### 1.1 Types and members

| Today | Proposed |
| --- | --- |
| `StructuralValue` | `PayloadValue` |
| `StructuralEvent` | `Payload` |
| `StructuralRequest` | `PayloadRequest` |
| `StructuralRequest<TResponse>` | `PayloadRequest<TResponse>` |
| `StructuralEventExtensions` | `PayloadExtensions` |
| `StructuralFieldsConverter` | `PayloadFieldsConverter` |
| `EventDefinition` | `PayloadDefinition` |
| `FieldDefinition`, `FieldType` | unchanged |
| `IEventFactory` | `IPayloadFactory` |
| `IEventValueBuilder` | `IPayloadValueBuilder` |
| `EventValueBuildAction` | `PayloadValueBuildAction` |
| `IEventsRegister` (`Events`, `GetEvent`) | `IPayloadsRegister` (`Payloads`, `GetPayload`) |
| `EventsRegister` | `PayloadsRegister` |
| `StateflowsBuilder.Events` / `IStateflowsBuilder.AddEvents` | `StateflowsBuilder.Payloads` / `IStateflowsBuilder.AddPayloads` |
| `EventsBuildAction` | `PayloadsBuildAction` |
| `EventBuildAction` | `PayloadBuildAction` |
| `IEventsBuilder` (`AddEvent`, `AddRequest`) | `IPayloadsBuilder` (`AddPayload`, `AddRequest`) |
| `IEventBuilder` | `IPayloadBuilder` |
| `ITransitionGuard<StructuralEvent>` and every other `<StructuralEvent>` type argument | `<Payload>` |
| `DeferralBuildAction<StructuralEvent>`, `TransitionBuildAction<StructuralEvent>`, `AcceptEventActionDelegateAsync<StructuralEvent>`, … | `<Payload>` |
| internal: `Edge.StructuralEventNames`, `Node.StructuralEventName`, `Vertex.StructuralDeferrals` | `PayloadNames`, `PayloadNames`, `PayloadDeferrals` |
| `FieldBuildAction`, `ObjectFieldBuildAction`, `CollectionFieldBuildAction`, `ObjectCollectionFieldBuildAction`, `IFieldsBuilder`, `IFieldBuilder`, `IObjectFieldBuilder`, `ICollectionFieldBuilder`, `IObjectCollectionFieldBuilder` | unchanged |

### 1.2 Namespaces, assemblies and files

| Today | Proposed |
| --- | --- |
| namespace `Stateflows.Events` (registration) | `Stateflows.Payloads` |
| namespace `Stateflows.Events.Json` | `Stateflows.Payloads.Json` |
| `Stateflows.Common` | unchanged (core types stay) |
| `JsonEventDefinition` | `JsonPayloadDefinition` |
| `JsonEventFactoryExtensions`, `EventJson` | `JsonPayloadFactoryExtensions`, `PayloadJson` |
| `event.schema.json` | `payload.schema.json` |
| MinimalAPIs interceptor hooks `Before/AfterStructuralEventEndpointDefinition` | `Before/AfterPayloadEndpointDefinition` |
| MinimalAPIs `Structural*` helper types | `Payload*` equivalents |
| wiki page `4.1.-Events` | `4.1.-Events` keeps typed events; payloads move to a **new, separate page** `4.6.-Payloads` (decision D25) (names and numbering) |

Identifiers that stay as they are: `AddAcceptEventAction`,
`AddTransition(string eventName, …)`, `AddDeferredEvent`, `AddInitializer`,
`ConsumedEventNames`, `EventHolder`, `IEventConsumer<TEvent>`, and parameters named
`eventName`/`eventNames`.

---

## 2. Payload definition API

Unchanged in shape; only names change (section 1.1).

```csharp
IStateflowsBuilder AddPayloads(PayloadsBuildAction buildAction);

public interface IPayloadsBuilder
{
    IPayloadsBuilder AddPayload(string name, PayloadBuildAction buildAction);
    IPayloadsBuilder AddRequest(string name, string responseEventName, PayloadBuildAction buildAction);
    IPayloadsBuilder AddRequest<TResponse>(string name, PayloadBuildAction buildAction);
}
```

Behavior additions:

- One registry serves both roles. A payload name registered with `AddPayloads` can be
  used as an event (transition trigger, initializer, deferral, accept-event) and as a token
  (flows, pins, injection) with no extra registration.
- A payload name must not collide with a CLR event name or a CLR **token** name known
  to the host. Today only event-name collisions are rejected.
- Payload names referenced by any behavior must be registered before the behavior is
  built; otherwise the behavior definition fails validation.

---

## 3. Event consumption parity

Where a behavior consumes an event it gains three forms of declaration with the same
meaning everywhere: **CLR type** (as today), **CLR multi-type via `ITypeMapper`**
(OneOf/UnionTypes), and **one or more payload names** (new in initializers and in
deferral guards, extended to multiple names elsewhere).

| Consumption point | CLR type | `ITypeMapper` multi-type | One payload name | Many payload names |
| --- | --- | --- | --- | --- |
| State-machine transition | today | today | today | **new** |
| Accept-event action | today | today | today | **new** |
| State-machine initializer | today | today | **new** | **new** |
| Activity initializer | today | **fixed** (see 7) | **new** | **new** |
| State deferral | today | today | today (no guard class) | **new**, plus guard-class form |
| Action consumed events | today | n/a | today | **new** |
| Entity initializer | today | **new** (D26) | **new** | **new** |
| Entity mutation | today | **new** (D26) | **new** | **new** |

Class-based handlers (initializer, deferral guard, entity mutation classes) receive
their payload names as a required parameter of the registration method (decision D29,
section 3.9).

### 3.1 State-machine initializers

Added next to the existing `AddInitializer<TInitializationEvent>(…)` and
`AddInitializer<TInitializationEvent, TInitializer>()` on `IStateMachineEvents<TReturn>`
(implemented by every state-machine builder variant that has them today):

```csharp
TReturn AddInitializer(
    IReadOnlyList<string> eventNames,
    Func<IStateMachineInitializationContext<Payload>, Task<bool>> actionAsync);

TReturn AddInitializer<TInitializer>(IReadOnlyList<string> eventNames)
    where TInitializer : class, IInitializer<Payload>;
```

- `eventNames` is required (decision D29). Registering an `IInitializer<Payload>` class
  through the existing `AddInitializer<Payload, TInitializer>()` is a definition error
  (no names; rule V4a).
- `IStateMachineInitializationContext<Payload>.InitializationEvent` is the received
  `Payload` (its `Name` tells which of the declared names arrived).
- Several initializers may share a name; they accumulate and run in registration
  order, as for typed initializers today.
- The set of declared names appears in expected-event reporting of the state machine
  and in visitor metadata (`InitializerAddedAsync<Payload>` already carries a list of
  event names).

### 3.2 Activity initializers

Same shape on `IActivityEvents<TReturn>`:

```csharp
TReturn AddInitializer(
    IReadOnlyList<string> eventNames,
    Func<IActivityInitializationContext<Payload>, Task<bool>> actionAsync);

TReturn AddInitializer<TInitializer>(IReadOnlyList<string> eventNames)
    where TInitializer : class, IInitializer<Payload>;
```

`eventNames` is required, as in section 3.1.
`AddInitializer<OneOf<A, B>>(…)` and other `ITypeMapper` types start matching `A` and
`B` at run time (see section 7).

### 3.3 State deferral

On `IStateUtils<TReturn>`:

```csharp
TReturn AddDeferredEvent(
    IReadOnlyList<string> eventNames,
    DeferralBuildAction<Payload> buildAction = null);

TReturn AddDeferredEvent<TDeferralGuard>(
    IReadOnlyList<string> eventNames,
    DeferralBuildAction<Payload> buildAction = null)
    where TDeferralGuard : class, IDeferralGuard<Payload>;
```

- `eventNames` is required (decision D29). The existing
  `AddDeferredEvent<Payload, TDeferralGuard>()` is a definition error (no names; rule
  V4a).
- `AddDeferredEvent(string eventName, DeferralBuildAction<Payload> = null)` already
  exists (renamed type argument only).
- Inheritance and override rules for deferrals follow the typed ones (name sets act
  like the type key).
- Deferral by `ITypeMapper` type keeps working through the existing typed overloads.

### 3.4 Transitions

Multi-name overloads next to the existing single-name ones (`IStateTransitions<TReturn>`):

```csharp
TReturn AddTransition(IReadOnlyList<string> eventNames, string targetStateName,
    TransitionBuildAction<Payload> transitionBuildAction = null);
TReturn AddInternalTransition(IReadOnlyList<string> eventNames,
    InternalTransitionBuildAction<Payload> transitionBuildAction = null);
TReturn AddElseTransition(IReadOnlyList<string> eventNames, string targetStateName,
    ElseTransitionBuildAction<Payload> transitionBuildAction = null);
TReturn AddElseInternalTransition(IReadOnlyList<string> eventNames,
    ElseInternalTransitionBuildAction<Payload> transitionBuildAction = null);
```

Overrides (`IStateTransitionsOverrides<TReturn>`):

```csharp
TReturn UseTransition(IReadOnlyList<string> eventNames, string targetStateName,
    OverridenTransitionBuildAction<Payload> buildAction);
TReturn UseInternalTransition(IReadOnlyList<string> eventNames,
    OverridenInternalTransitionBuildAction<Payload> buildAction);
TReturn UseElseTransition(IReadOnlyList<string> eventNames, string targetStateName,
    OverridenElseTransitionBuildAction<Payload> buildAction);
TReturn UseElseInternalTransition(IReadOnlyList<string> eventNames,
    OverridenElseInternalTransitionBuildAction<Payload> buildAction);
```

Trigger replacement (`IOverridenTransitionBuilder<TEvent>`):

```csharp
IOverridenTransitionBuilder<Payload> ChangeTrigger(IReadOnlyList<string> eventNames);
```

- A transition with a name set fires when the received `Payload`'s name is any member.
- `Use*Transition(...)` locates the inherited transition whose name **set equals** the
  given set (order-insensitive); the single-name form means a one-element set
 .

### 3.5 Accept-event actions

On `IAcceptEventBase<TReturn>`:

```csharp
TReturn AddAcceptEventAction(
    IReadOnlyList<string> eventNames,
    string actionNodeName,
    AcceptEventActionDelegateAsync<Payload> eventActionAsync,
    AcceptEventActionBuildAction<Payload> buildAction = null);

TReturn AddAcceptEventAction(
    IReadOnlyList<string> eventNames,
    string actionNodeName,
    AcceptEventActionBuildAction<Payload> buildAction = null);
```

On `IOverridenAcceptEventActionBuilder` / accept-event builder:

```csharp
IOverridenAcceptEventActionBuilder<Payload> ChangeAcceptedEvent(IReadOnlyList<string> eventNames);
```

The output token of a named accept-event action is a payload token named after the
received payload, so its outgoing flows select it with the payload flow family
(section 4.4).

### 3.6 Action behaviors

On `IActionUtils<TReturn>`:

```csharp
TReturn AddConsumedEvent(IReadOnlyList<string> eventNames);   // sugar: AddConsumedEvent(string) exists
```

Payload token declarations for Actions are in section 4.9.

### 3.7 `ITypeMapper` (OneOf / UnionTypes)

No new API on the state-machine, activity and Action builders. Mapped CLR types and
payload names are separate declaration forms; `ITypeMapper` is never consulted for
names. **Entities gain `ITypeMapper` support** (decision D26, section 3.8): until now
they had none, so `AddInitializer<OneOf<A, B>>(…)` and `[Mutation] void M(OneOf<A, B> e)`
did not match anything.

### 3.8 Entity initializers and mutations

An entity template consumes events in exactly two places, both gain payload-named
forms (decision D19) and `ITypeMapper` multi-type forms (decision D26). The code-first
builder (`IEntityBuilder<TTemplate>`):

```csharp
IEntityBuilder<TTemplate> AddInitializer(
    IReadOnlyList<string> eventNames,
    InitializerAction<TTemplate, Payload> action);

IEntityBuilder<TTemplate> AddMutation(
    IReadOnlyList<string> eventNames,
    MutationAction<TTemplate, Payload> mutationAction);

// class-based: names required (decision D29)
IEntityBuilder<TTemplate> AddMutation<TMutation>(IReadOnlyList<string> eventNames)
    where TMutation : class, IMutation<TTemplate, Payload>;
```

The existing typed members keep their shapes; with an `ITypeMapper` type they now
match every mapped type:

```csharp
IEntityBuilder<TTemplate> AddInitializer<TInitializationEvent>(
    InitializerAction<TTemplate, TInitializationEvent> action);       // TInitializationEvent may be OneOf<A, B>
IEntityBuilder<TTemplate> AddMutation<TMutationEvent>(
    MutationAction<TTemplate, TMutationEvent> mutationAction);        // TMutationEvent may be OneOf<A, B>
IEntityBuilder<TTemplate> AddMutation<TMutationEvent, TMutation>()
    where TMutation : class, IMutation<TTemplate, TMutationEvent>;
```

The template-interface form (annotated default interface methods analyzed by
`EntitiesBuilder`) gets names on the existing attributes (decision D26):

```csharp
[AttributeUsage(AttributeTargets.Method)]
public sealed class MutationAttribute : Attribute
{
    public MutationAttribute(params string[] eventNames);   // [Mutation] still compiles: empty set
    public IReadOnlyList<string> EventNames { get; }        // empty => CLR-typed method
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class InitializerAttribute : Attribute
{
    public InitializerAttribute(params string[] eventNames);
    public IReadOnlyList<string> EventNames { get; }
}
```

```csharp
public interface IOrderTemplate
{
    [Field] string Status { get; set; }

    [Mutation("order.cancelled", "order.expired")]
    void Close(Payload reason) => Status = reason.Name;

    [Mutation]
    void Resolve(OneOf<Approved, Rejected> decision) => Status = decision.ToString();   // ITypeMapper
}
```

- A method whose single parameter is `Payload` **must** list names; names on a method
  whose parameter is not `Payload` are rejected. `[DefaultInitializer]` is unchanged.
- A method whose parameter is an `ITypeMapper` type (OneOf/UnionTypes) lists no names
  and is invoked for every mapped type, converting the received event to the parameter
  type the way state machines already do. The same holds for the builder forms.
- `IEntityInitializationContext<TTemplate, Payload>.InitializationEvent` and
  `IMutationContext<TTemplate, Payload>.MutationEvent` give the received `Payload`; its
  `Name` tells which of the declared names arrived. The same holds for
  `IMutationObservationContext<TTemplate, Payload>` and the entity visitor callbacks
  (`InitializerAddedAsync<TTemplate, Payload>` / `MutationAddedAsync<TTemplate, Payload>`
  already carry an `eventNames` list; no signature change). For `ITypeMapper` types the
  visitor callbacks keep the declared type argument and report the **mapped** event
  names.
- Handler lookup for payloads is **by name**, not by CLR type. Typed handlers keep
  today's one-handler-per-event-type rule; for payloads it is one handler per
  **name**: a later registration of a name replaces an earlier one, and two annotated
  template methods claiming the same name are a definition error, as for typed
  methods today. For `ITypeMapper` types the same rule applies per **mapped type**:
  two handlers that map to the same event type conflict exactly as two handlers of
  that type do today.
- Names (and mapped event names) appear in the entity's expected-event reporting and
  in the MinimalAPIs entity endpoints (one endpoint per name or mapped type, section
  5.2).
- Initialization rules are unchanged: a payload initializer runs only for a
  not-initialized entity; `NoImplicitInitialization` applies to payload events through
  its header form only (there is no CLR type to carry the attribute).
- `IMutation<TTemplate, Payload>` classes are registered with names at the call site
  (section 3.9).

### 3.9 Class-based handlers: names at the call site

A class that *is* the registered handler of payload events — an initializer, a
deferral guard or an entity mutation — carries no name information of its own
(decision D29). Its names are a **required** parameter of the registration method:

```csharp
builder.AddInitializer<OrderInitializer>(["order.placed", "order.updated"]);   // IInitializer<Payload>
state.AddDeferredEvent<CancelledDeferral>("order.cancelled");                  // IDeferralGuard<Payload>
entity.AddMutation<CloseOrder>(["order.expired", "order.cancelled"]);          // IMutation<IOrderTemplate, Payload>
```

- Registering such a class closed over `Payload` through a typed member (no names, for
  example `AddInitializer<Payload, OrderInitializer>()`) is a definition error (rule
  V4a): the behavior's definition exception (`StateMachineDefinitionException` /
  `DeferralDefinitionException`, `ActivityDefinitionException`; entity registration
  throws `InvalidOperationException` like its other definition errors).
- Classes that sit *inside* a builder that already carries the names — guard, effect
  and handler classes of a transition (`ITransitionGuard<Payload>`,
  `ITransitionEffect<Payload>`), of an accept-event action, and guards and
  transformations of a flow — operate on the names of that declaration and take no
  names of their own.
- Flow guard and transformation classes follow the same principle; flows are in
  section 4.4.
- **Overload note (verified by compiling a probe of every overload in sections 3 and
  4.4).** With names required and placed first, none of the new members ties with an
  existing one (no CS0111 or CS0121), and every `IReadOnlyList<string>` member keeps its
  single-`string` sugar form.

---

## 4. Payload tokens

### 4.1 Model

A payload token is a `TokenHolder<Payload>`; its `Name` is the payload name. A
**token key** selects tokens in every declaration:

- a CLR type (as today): matches tokens whose actual type is assignable to it;
- a non-empty set of payload names: matches payload tokens whose name is in the set.

Typed and payload declarations coexist freely in one activity (rules R1–R11 in the
plan). Typed wildcards (`object`, shared base types and interfaces) keep matching
everything assignable, including payload tokens (D13).

### 4.2 Token transfer (`TokenHolder`, `TokensInput`, `TokensOutput`)

```csharp
// TokenHolder<TToken>.Name now returns Payload.Name when the payload is a Payload.
public interface ITokensInput
{
    ITokensInput Add(Payload payload);
    ITokensInput AddRange(params Payload[] payloads);
    // existing: Add<TToken>(TToken), AddRange<TToken>(params TToken[])
}

public sealed class TokensInput : TokensInputEvent, ITokensInput, IRequest<TokensOutput>
{
    public TokensInput Add(Payload payload);
    public TokensInput AddRange(params Payload[] payloads);
}

public interface ITokensOutput
{
    IEnumerable<Payload> GetPayloadTokens(string name);
    IEnumerable<Payload> GetPayloadTokens(IReadOnlyList<string> names);
    // existing: GetAllOfType<TToken>()
}

public sealed class TokensOutput : TokensOutputEvent, ITokensOutput
{
    public IEnumerable<Payload> GetPayloadTokens(string name);
    public IEnumerable<Payload> GetPayloadTokens(IReadOnlyList<string> names);
    // existing: GetOfType<TToken>() - also returns payload tokens when TToken is assignable
}
```

- Payload instances carry their own name; there is no separate name argument (D12).
- `TokensInput<TToken>` and `TokensOutput<TToken>` have no payload form; `TokensInput`
  and `TokensOutput` are the payload entry points.
- Adding a payload the target pin does not declare is rejected by the receiving
  activity.
- `PayloadRequest` instances are accepted wherever `Payload` is (`PayloadRequest : Payload`)
  and flow through the network as tokens like any other payload; a payload name
  registered with `AddRequest` is therefore a legal token name (decision D22). See
  "Payload requests as tokens" in section 4.4 for response semantics.

### 4.3 Injection: `[TokenName]` and the non-generic interfaces

New parameter attribute, namespace `Stateflows.Common.Attributes`, in the same place
and style as the other parameter attributes (no `From…` alias):

```csharp
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class TokenNameAttribute : Attribute
{
    public TokenNameAttribute(params string[] names);
    public IReadOnlyList<string> Names { get; }
}
```

New non-generic interfaces in `Stateflows.Activities`, each deriving from its generic
twin closed over `Payload` (D4):

```csharp
public interface IInputTokens                : IInputTokens<Payload>;
public interface IInputToken                 : IInputToken<Payload>;
public interface IOptionalInputTokens        : IOptionalInputTokens<Payload>;
public interface IOptionalInputToken         : IOptionalInputToken<Payload>;
public interface IOutputTokens               : IOutputTokens<Payload>;
public interface IInputOutputTokens          : IInputOutputTokens<Payload>;
public interface IInputOutputToken           : IInputOutputToken<Payload>;
public interface IOptionalInputOutputTokens  : IOptionalInputOutputTokens<Payload>;
public interface IOptionalInputOutputToken   : IOptionalInputOutputToken<Payload>;
```

**Change to the existing typed optional input-output interfaces (decision D20).**
`IOptionalInputOutputTokens<TToken>` and `IOptionalInputOutputToken<TToken>` gain
`IOutputTokens<TToken>` as a base interface, matching their non-optional siblings
(the omission was unintentional). The non-generic optional forms above inherit it.

```csharp
public interface IOptionalInputOutputTokens<TToken> : IOptionalInputTokens<TToken>, IOutputTokens<TToken>
{
    void PassAllOn();                       // existing
}

public interface IOptionalInputOutputToken<TToken> : IOptionalInputToken<TToken>, IOutputTokens<TToken>
{
    void PassOn();                          // existing
}
```

The concrete collections implement the new interfaces; `PassAllOn()` of the optional
collections writes to the node's real output (it writes to a discarded collection
today; fixed in plan WP0, see B6/B8).

Usage (activity node classes and Action behaviors; the same constructor-injection
mechanism that serves the typed interfaces today):

```csharp
public class Approve([TokenName("order")] IInputTokens orders,
                     [TokenName("approval", "rejection")] IOutputTokens decisions) : IActionNode { … }
```

Rules:

- **Required:** a non-generic interface parameter without `[TokenName]` is a
  definition error (D5).
- **Forbidden:** `[TokenName]` on a parameter whose type is a generic typed
  interface (`IInputTokens<Order>`, …) is a definition error.
- **Forbidden:** `IInputTokens<Payload>`, `IOutputTokens<Payload>`,
  `IInputToken<Payload>`, `IOptional*<Payload>`, `IInputOutput*<Payload>` (and the
  `PayloadRequest`/`PayloadValue` closures) without `[TokenName]` are a definition
  error.
- Input collections contain the node's payload tokens whose name is in the attribute.
  Single-token forms (`IInputToken`, …) follow the same "first available" rule as
  their typed twins.
- Output collections accept `Payload` instances whose name is in the attribute;
  adding another name throws `ArgumentException`.
- Input/output interface semantics (collection shape, `PassAllOn`, `PassOn`,
  optional behavior) are exactly those of the generic interfaces; any change made to
  the generic ones in the prerequisite bug-fix work package (plan WP0) carries over.
- A node mixes typed and payload interfaces freely in one constructor; they select
  disjoint token sets unless a typed wildcard overlaps (D13, D21).

### 4.4 Flows

Payload flow members are **overloads of the existing names** (`AddFlow`, `UseFlow`,
`AddElseFlow`, `AddTransformation`) whose first parameter is the name list; the
existing typed overloads keep their shapes. Transformations whose transformed type is
`Payload` get overloads with a `transformedNames` parameter (decision D31, below).
Each `IReadOnlyList<string>` member has the single-`string` sugar form, except where
noted in section 4.4.2.

#### 4.4.1 Two name sets per flow

A flow today has two token types: the type it takes from its source (`TokenType`) and
the type it delivers to its target after its transformations (`TargetTokenType`).
Payload names follow exactly the same split (decision D30):

- **original names**: matched against the payload tokens offered by the source node;
- **transformed names**: the names of the payloads the flow delivers to the target.

| Flow | Original names | Transformed names |
| --- | --- | --- |
| typed source, no transformation or transformation to a typed token | none | none |
| typed source, transformation to `Payload` | none | that transformation's `transformedNames` |
| payload source, no transformation | the call-site (or bound) names | **the original names** |
| payload source, transformation to `Payload` | the call-site (or bound) names | that transformation's `transformedNames` |
| payload source, transformation to a typed token | the call-site (or bound) names | none |

- Several transformations may be chained, as today. Each transformation replaces the
  flow's transformed type and transformed names; the **last** one determines what the
  target receives, exactly as the last one determines `TargetTokenType` today
 .
- Guards and transformations declare no names of their own. A guard or transformation
  added to a payload flow receives `Payload` tokens carrying one of the names in effect
  at that point in the chain: the original names before the first transformation, the
  previous transformation's `transformedNames` after it.
- Flow identifiers, the flow context (`IFlowContext`), visitor callbacks and inspection
  expose both sets next to the existing token types (section 4.6).

#### 4.4.2 Node-level members

Node-level (`IObjectFlowBase<TReturn>`; therefore also on data-store builders):

```csharp
// delegate / no stage
TReturn AddFlow(IReadOnlyList<string> tokenNames, string targetNodeName,
    ObjectFlowBuildAction<Payload> buildAction = null);

TReturn AddFlow<TTargetNode>(IReadOnlyList<string> tokenNames,
    ObjectFlowBuildAction<Payload> buildAction = null)
    where TTargetNode : class, IActivityNode;

// class-based guard
TReturn AddFlow<TFlow>(IReadOnlyList<string> tokenNames, string targetNodeName)
    where TFlow : class, IFlow<Payload>;

TReturn AddFlow<TFlow, TTargetNode>(IReadOnlyList<string> tokenNames)
    where TFlow : class, IFlow<Payload>
    where TTargetNode : class, IActivityNode;

// class-based transformation from payload to a typed token
TReturn AddFlow<TTransformedToken, TTransformationFlow>(
    IReadOnlyList<string> tokenNames, string targetNodeName)
    where TTransformationFlow : class, IFlowTransformation<Payload, TTransformedToken>;

TReturn AddFlow<TTransformedToken, TTransformationFlow, TTargetNode>(
    IReadOnlyList<string> tokenNames)
    where TTransformationFlow : class, IFlowTransformation<Payload, TTransformedToken>
    where TTargetNode : class, IActivityNode;

// class-based transformation from payload to payload
TReturn AddFlow<TTransformedToken, TTransformationFlow>(
    IReadOnlyList<string> tokenNames, IReadOnlyList<string> transformedNames,
    string targetNodeName)
    where TTransformedToken : Payload
    where TTransformationFlow : class, IFlowTransformation<Payload, TTransformedToken>;

TReturn AddFlow<TTransformedToken, TTransformationFlow, TTargetNode>(
    IReadOnlyList<string> tokenNames, IReadOnlyList<string> transformedNames)
    where TTransformedToken : Payload
    where TTransformationFlow : class, IFlowTransformation<Payload, TTransformedToken>
    where TTargetNode : class, IActivityNode;
```

From a **typed** source, a class-based transformation to `Payload` uses new overloads
of the existing `AddFlow` members (decision D31):

```csharp
TReturn AddFlow<TToken, TTransformedToken, TTransformationFlow>(
    IReadOnlyList<string> transformedNames, string targetNodeName)
    where TTransformedToken : Payload
    where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>;

TReturn AddFlow<TToken, TTransformedToken, TTransformationFlow, TTargetNode>(
    IReadOnlyList<string> transformedNames)
    where TTransformedToken : Payload
    where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
    where TTargetNode : class, IActivityNode;

// the token-scoped variants (IObjectFlowBase<TReturn, TToken>) and the class-based
// else-flow variants (AddElseFlow<TTransformedToken, TElseTransformationFlow>, …) get
// the same transformedNames overloads
```

- `tokenNames` are the flow's original names, `transformedNames` its transformed
  names (section 4.4.1).
- **Lists only, no single-name form**, for the members whose `string` form would
  have the same signature as, or be ambiguous with, an existing typed member
  (verified by a compile probe): `AddFlow<TTargetNode>(tokenNames, …)` and
  `UseFlow<TTargetNode>(tokenNames, …)` (ambiguous with `AddFlow<TToken>(string
  targetNodeName, …)` / `UseFlow<TToken>(…)`), `AddFlow<TFlow, TTargetNode>(tokenNames)`
  (same signature as `AddFlow<TToken, TFlow>(string)`),
  `AddFlow<TTransformedToken, TTransformationFlow, TTargetNode>(tokenNames)` and
  `AddFlow<TTransformedToken, TTransformationFlow, TTargetNode>(tokenNames,
  transformedNames)` (same signatures as `AddFlow<TToken, TTransformedToken,
  TTransformationFlow>(string)` and its `transformedNames` sugar). Callers write
  `["a"]`.
- The existing members without names stay as they are. Using them with `Payload` (or
  `PayloadRequest`/`PayloadValue`) as `TToken` or `TTransformedToken` is a definition
  error (rule V4a): the names would be unknown.

Overrides (`IOverridenObjectFlowBase<TReturn>`):

```csharp
TReturn UseFlow(IReadOnlyList<string> tokenNames, string targetNodeName,
    ObjectFlowBuildAction<Payload> buildAction = null);
TReturn UseFlow<TTargetNode>(IReadOnlyList<string> tokenNames,
    ObjectFlowBuildAction<Payload> buildAction = null)
    where TTargetNode : class, IActivityNode;
```

`UseFlow` locates the inherited flow to the target whose **original** name set
equals the given set (order-insensitive), as `Use*Transition` does in section 3.4.

Else flows (`IElseObjectFlowBase<TReturn>`):

```csharp
TReturn AddElseFlow(IReadOnlyList<string> tokenNames, string targetNodeName,
    ElseObjectFlowBuildAction<Payload> buildAction = null);
```

Token-scoped builders (pin, decision, data-store and flow-chain builders that are
already parameterized by the token type — `IObjectFlowBase<TReturn, TToken>` and its
override/else variants): for a payload pin or decision the builder is
`…<Payload>` and is **bound to the pin's/decision's name set**. Its existing
`AddFlow`, `UseFlow`, `AddElseFlow`, `AddFlow<TTargetNode>`, class-based guard and
typed-transformation forms work unchanged; the flow's original names are the bound
names and need not be repeated. Transformations to `Payload` use the
`transformedNames` overloads above.

#### 4.4.3 Flow-chain builders

The four flow-chain builders — `IObjectFlowBuilder<TToken>`,
`IObjectFlowBuilderWithWeight<TToken>`, `IElseObjectFlowBuilder<TToken>` and
`IElseObjectFlowBuilderWithWeight<TToken>` — each gain two overloads of
`AddTransformation`, shown for `IObjectFlowBuilder<TToken>` (the others return their
own builder type):

```csharp
// new: delegate transformation to payload
IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(
    IReadOnlyList<string> transformedNames,
    TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
    where TTransformedToken : Payload;

// new: class-based transformation to payload
IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>(
    IReadOnlyList<string> transformedNames)
    where TTransformedToken : Payload
    where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>;

// existing, unchanged; TTransformedToken must not be Payload (definition error, V4a)
IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(
    TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync);
IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken, TTransformation>()
    where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>;

// existing guard members work unchanged on a Payload builder
TReturn AddGuard(params Func<IGuardContext<Payload>, Task<bool>>[] guardAsync);
TReturn AddGuard<TGuard>() where TGuard : class, IFlowGuard<Payload>;
```

```csharp
order.AddFlow<Order>("Invoice", b => b
    .AddTransformation<Payload>("invoice.requested", async c => c.Token.ToInvoiceRequest())
    .AddGuard(async c => c.Token.Name == "invoice.requested"));

node.AddFlow(["order.placed", "order.updated"], "Store", b => b
    .AddTransformation<Payload, OrderToInvoice>(["invoice.requested"]));
```

- `TToken` may be a CLR type (typed → payload) or `Payload` (payload → payload).
- With inference, `AddTransformation("name", async c => …)` resolves to the payload
  overload when the delegate returns a `Payload`.
- The returned builder is `…<TTransformedToken>` (normally `…<Payload>`); its flow's
  transformed names are now `transformedNames` (section 4.4.1).
- The delegate or class must return a `Payload` whose name is in `transformedNames`;
  otherwise the flow fails at run time with `ArgumentException` (decision D16).
- `AddPayloadTransformation`, proposed in an earlier revision, is dropped: the
  `transformedNames` overloads replace it.

#### 4.4.4 Validation

**Validation (rule V11, decision D27).** Payload names are validated with the same
checks the activity model applies to typed tokens, translated from "type *A* is
assignable to type *B*" to "name set *A* is a subset of name set *B*". Original names
take the place of `TokenType`, transformed names the place of `TargetTokenType`.

| Check | Typed tokens today | Payload names |
| --- | --- | --- |
| Flow ↔ source's declared outputs (ACT-028) | declared type *D* accepts the flow's `TokenType` *T* when `D.IsAssignableFrom(T)` **or** `T.IsAssignableFrom(D)` | the original names *O* are accepted when *O* ⊆ *D* **or** *D* ⊆ *O* |
| Flow ↔ target's declared inputs (ACT-027) | the same either-direction rule on `TargetTokenType` | the transformed names *F* are accepted when *F* ⊆ *D* **or** *D* ⊆ *F* |
| Required input (ACT-029) | some incoming edge type *E* satisfies `D.IsAssignableFrom(E)` | some incoming flow has transformed names *F* ⊆ *D* |
| Transitive nodes: join, merge, fork, decision, data store (ACT-026) | each outgoing type is assignable to some incoming type | each original name of an outgoing flow appears in some incoming flow's transformed names |
| Stages inside a flow | compile time: a transformation's output type is the next stage's input type | nothing to check: stages declare no names |
| Nodes that declare nothing | a node that declares no token types (a delegate action, or a class that reads tokens through context queries only) is unconstrained | a node or Action that declares no payload names and no token types is unconstrained |

- "Declared names" are those of payload pins, decisions, iteration/parallelization
  scopes and `[TokenName]` parameters. Pins, decisions and data stores always declare
  their key, so they are always checked.
- Typed and payload declarations are compared by the typed rule where they meet: a
  declaration of `object` or another type assignable from `Payload` accepts payload
  flows (D13), while a node that declares only unrelated typed inputs rejects a
  payload flow, exactly as it rejects an unrelated typed flow today.
- The check runs when the activity is built; diagnostics name the flow, which of its
  name sets failed, and the offending names. The translation of ACT-026 (name-level
  coverage) and the "either direction" containment are my reading of "pipeline checks
  matching typed checks".

At run time each transformation to `Payload` (delegate or class) verifies that the
returned payload's name belongs to its `transformedNames` (`ArgumentException`
otherwise).

#### 4.4.5 Payload requests, stage interfaces and overlap

**Payload requests as tokens (decision D22).** A `PayloadRequest` is a `Payload` and
may be a token: events sent to an activity (including requests) can flow down the
network as tokens. Nothing special is required in the flow API: the name is declared
like any other payload name. Response routing keeps its existing meaning: `Respond` on a request token answers the
original sender through the in-memory response holder of the executing instance, so
it is expected to work only while that instance is alive and not after persistence and
re-hydration; how typed request tokens behave today is characterized before
implementation and payload requests follow it. Request names get no MinimalAPIs pin
endpoints (section 5.2).

Guards (delegate and class forms) see the token through the existing contexts; for
payload flows `c.Token` is the `Payload`:

```csharp
ObjectFlowBuildAction<Payload>   // existing delegate; guard builder members work as for typed flows
IFlowGuard<Payload>              // existing generic interface; sees the names in effect at its position
IFlowTransformation<Payload, TTransformed>  // from payload; registered with the flow's names
IFlowTransformation<TToken, Payload>        // to payload; registered with transformedNames
```

Stage classes carry no attributes; the names always come from the registration call.

**Overlap of typed and payload flows (decision D21).** A typed flow and a payload
flow (or a typed wildcard such as `object`) may both match one token from the same
source. This is allowed and behaves exactly like overlapping typed flows do today
(characterized before implementation); there is no extra validation error.

### 4.5 Pins

```csharp
// IInputBase
TReturn AddInputPin(IReadOnlyList<string> tokenNames,
    Action<IInputPinBuilder<Payload>> buildAction);

// IOutputBase
TReturn AddOutputPin(IReadOnlyList<string> tokenNames, string? pinName = null);

// IActivityOutputBase
TReturn AddOutputPin(IReadOnlyList<string> tokenNames, string? pinName,
    PublishEventActionBuildAction buildAction);
TReturn AddOutputPin(IReadOnlyList<string> tokenNames,
    PublishEventActionBuildAction buildAction);

// IActivitySpecialsOverrides
TReturn UseInputPin(IReadOnlyList<string> tokenNames,
    Action<IOverridenInputPinBuilder<Payload>> buildAction);
```

- Pin node names follow the existing convention for typed pins (default name derived
  from the declaration; `pinName` overrides). Default name for a payload pin is
  derived from its name set in a documented, stable format.
- Non-generic helper classes `InputPin` / `OutputPin` (static name helpers), the
  payload counterparts of `InputPin<TToken>` / `OutputPin<TToken>`:

```csharp
public static class InputPin  { public static string GetName(IReadOnlyList<string> tokenNames); }
public static class OutputPin { public static string GetName(IReadOnlyList<string> tokenNames); }
```

### 4.6 Decisions, iterative and parallel activities

```csharp
// IActivitySpecials
TReturn AddDecision(IReadOnlyList<string> tokenNames, string decisionNodeName,
    DecisionBuildAction<Payload> decisionBuildAction);
TReturn AddDecision(IReadOnlyList<string> tokenNames,
    DecisionBuildAction<Payload> buildAction);

// IActivitySpecialsOverrides
TReturn UseDecision(IReadOnlyList<string> tokenNames, string decisionNodeName,
    OverridenDecisionBuildAction<Payload> decisionBuildAction);
TReturn UseDecision(IReadOnlyList<string> tokenNames,
    OverridenDecisionBuildAction<Payload> buildAction);

// IActivityBase and IReactiveActivityBase
TReturn AddIterativeActivity(IReadOnlyList<string> iterationTokenNames,
    string actionNodeName, IterativeActivityBuildAction buildAction, int chunkSize = 1);
TReturn AddIterativeActivity(IReadOnlyList<string> iterationTokenNames,
    IterativeActivityBuildAction buildAction, int chunkSize = 1);
TReturn AddIterativeActivity<TIterativeActivity>(
    IReadOnlyList<string> iterationTokenNames, IterativeActivityBuildAction buildAction = null,
    int chunkSize = 1) where TIterativeActivity : class, IActivity;   // + named-node overload

TReturn AddParallelActivity(IReadOnlyList<string> parallelizationTokenNames,
    string actionNodeName, ParallelActivityBuildAction buildAction,
    int chunkSize = 1, int maxConcurrency = -1);
TReturn AddParallelActivity(IReadOnlyList<string> parallelizationTokenNames,
    ParallelActivityBuildAction buildAction, int chunkSize = 1, int maxConcurrency = -1);
TReturn AddParallelActivity<TParallelActivity>(
    IReadOnlyList<string> parallelizationTokenNames, ParallelActivityBuildAction buildAction = null,
    int chunkSize = 1, int maxConcurrency = -1) where TParallelActivity : class, IActivity; // + named-node overload

// IActivityOverrides and IReactiveActivityOverrides
TReturn UseIterativeActivity(IReadOnlyList<string> iterationTokenNames,
    string actionNodeName, OverridenIterativeActivityBuildAction buildAction);
TReturn UseIterativeActivity(IReadOnlyList<string> iterationTokenNames,
    OverridenIterativeActivityBuildAction buildAction);
TReturn UseIterativeActivity<TIterativeActivity>(
    IReadOnlyList<string> iterationTokenNames, OverridenIterativeActivityBuildAction buildAction)
    where TIterativeActivity : class, IActivity;
TReturn UseParallelActivity(IReadOnlyList<string> parallelizationTokenNames,
    string actionNodeName, OverridenParallelActivityBuildAction buildAction);
TReturn UseParallelActivity(IReadOnlyList<string> parallelizationTokenNames,
    OverridenParallelActivityBuildAction buildAction);
TReturn UseParallelActivity<TParallelActivity>(
    IReadOnlyList<string> parallelizationTokenNames, OverridenParallelActivityBuildAction buildAction)
    where TParallelActivity : class, IActivity;
```

- The token set iterated or parallelized is the name set; partitioning and
  interleaving by token follow the typed semantics.
- **Lists only, no single-name form**, for `AddIterativeActivity<TIterativeActivity>`,
  `UseIterativeActivity<TIterativeActivity>`, `AddParallelActivity<TParallelActivity>`
  and `UseParallelActivity<TParallelActivity>` without a node name: a `string` first
  parameter would give them the same signature as the existing
  `AddIterativeActivity<TToken>(string actionNodeName, …)` and its siblings (verified
  by a compile probe). Their named-node overloads (`tokenName, structuredActivityName,
  …`) keep the single-name form.

### 4.7 Guard, transformation and flow contexts

Added members (all read-only; existing members unchanged). For typed declarations the
payload-name lists are empty and `TokenType` is the CLR type, for payload declarations
`TokenType` is `typeof(Payload)` and the lists hold the declared names:

```csharp
public interface IFlowContext
{
    IReadOnlyList<string> TokenNames { get; }
    IReadOnlyList<string> TargetTokenNames { get; }
}

public interface IPinContext
{
    IReadOnlyList<string> TokenNames { get; }
}

public interface ITransformationContext<out TToken>
{
    IReadOnlyList<string> InputTokenNames { get; }
    IReadOnlyList<string> OutputTokenNames { get; }
}

public interface IActivityOperationObservationContext
{
    string? TokenName { get; }          // actual token's name: payload name, or the CLR token name
}
```

`IGuardContext<Payload>` and `ITransformationContext<Payload, TTransformed>` expose the
`Payload` through the existing `Token`/`TransformedToken` members.

### 4.8 Token queries and output from node code

```csharp
public interface IInput
{
    IEnumerable<Payload> GetPayloadTokens(string tokenName);
    IEnumerable<Payload> GetPayloadTokens(IReadOnlyList<string> tokenNames);
    bool HasPayloadTokens(string tokenName);
    bool HasPayloadTokens(IReadOnlyList<string> tokenNames);
    // existing: GetTokensOfType<TToken>(), HasTokensOfType<TToken>()
}

public interface IOutput
{
    void PassPayloadTokensOn(IReadOnlyList<string> tokenNames);
    // existing: Output<TToken>(TToken), OutputRange<TToken>(...), PassTokensOfTypeOn<TToken>(), PassAllTokensOn()
}
```

- `Output<Payload>(payload)` / `OutputRange` accept `Payload` instances; the payload's
  name routes it. No `OutputPayload` method is added.
- Runtime queries with the bare type (`GetTokensOfType<Payload>()`,
  `PassTokensOfTypeOn<Payload>()`) stay legal and cover **all** payload tokens; only
  declarations are restricted (D5).
- A payload output whose name is not declared by the node (injected `[TokenName]` set
  or declared outputs) is rejected at run time.

### 4.9 Action behaviors

Actions get the same injection model (4.3) and payload-aware `IInput`/`IOutput`
(4.8) on their context. Declarations on `IActionUtils<TReturn>`:

```csharp
TReturn AddConsumedPayloadToken(IReadOnlyList<string> tokenNames);
TReturn AddProducedPayloadToken(IReadOnlyList<string> tokenNames);
// existing: AddConsumedToken<TToken>(), AddProducedToken<TToken>(), AddConsumedEvent(...), AddProducedEvent<TEvent>()
```

Action input/output over the wire uses the same `TokensInput`/`TokensOutput` additions
as activities (4.2).

### 4.10 Data stores

`IDataStoreBuilder : IObjectFlowBase<IDataStoreBuilder>` inherits the node-level
payload flow family (4.4); no store-specific API is added.

### 4.11 Observability and inspection

Existing typed callbacks remain and are invoked with `TToken = Payload` for payload
elements; one new default (no-op) callback per visitor delivers the names:

```csharp
// IActivityVisitor
Task TokenNamesDeclaredAsync(string elementId, string activityName, int activityVersion,
    TokenNamesDeclarationKind kind, IReadOnlyList<string> tokenNames) => Task.CompletedTask;

public enum TokenNamesDeclarationKind
{
    InputPin, OutputPin,
    FlowSource,                 // the flow's original names
    FlowTarget,                 // the flow's transformed names
    TransformationProduced,     // transformedNames of one transformation in a chain
    Decision, IterativeActivity, ParallelActivity
}

// IActionVisitor
Task InputTokenNamesAddedAsync(string pinId, string actionName, int actionVersion,
    IReadOnlyList<string> tokenNames) => Task.CompletedTask;
Task OutputTokenNamesAddedAsync(string pinId, string actionName, int actionVersion,
    IReadOnlyList<string> tokenNames) => Task.CompletedTask;
```

Inspection:

```csharp
public interface IFlowInspection
{
    IReadOnlyList<string> TokenNames { get; }           // original names; empty for typed sources
    IReadOnlyList<string> TargetTokenNames { get; }     // transformed names; empty for typed targets
    // existing TokenName: for payload flows, the stable identifier of the original name set
}

public interface INodeInspection
{
    IReadOnlyList<string> PinTokenNames => [];  // empty unless the node is a payload pin
    // existing PinTokenType: typeof(Payload) for payload pins
}
```

`InitializerAddedAsync<TEvent>(…, IReadOnlyList<string> eventNames, int order)` is
unchanged; for payload initializers `TEvent` is `Payload` and `eventNames` holds the
declared names. Accept/Send/Publish callbacks already carry `eventNames`.

Typed flow, pin and node identifiers are unchanged. Payload flows use a distinct,
documented identifier form derived from the name sets, built like the typed form: the
original set alone when the flow does not change its token, and original and
transformed parts (as the typed form joins `TokenType` and `TargetTokenType`) when it
does.

---

## 5. Extensions: JSON, MinimalAPIs and others

### 5.1 JSON definitions

Everything in `Stateflows.Extensions.Json` is renamed per section 1.2. Schema
additions (existing keys keep working):

| Document | Declaration | Today | Proposed addition |
| --- | --- | --- | --- |
| state machine | initializer `triggers` | exactly one `{ "clrType": … }` entry | key renamed to **`events`** ("trigger" is reserved for transitions), with the transition event shape: one `clrType` entry **or** a unique list of `{ "eventName": … }` entries; `triggers` is no longer accepted (B12) |
| state machine | deferral `triggers` | exactly one `{ "clrType": … }` entry | key renamed to **`events`**, same shape as the initializer's (B12) |
| activity | initializer | `eventType` | `eventName` (string) or `eventNames` (non-empty array), exclusive with `eventType` |
| activity | accept-event node | one of `eventType`, `eventName` | `eventNames` (array) as a third exclusive option |
| activity | pin, flow, decision, scope | `tokenType` alias | `tokenNames` (non-empty array), exclusive with `tokenType` |
| activity | iterative / parallel | `iterationTokenType` | `iterationTokenNames`, exclusive with it |
| activity | flow transformation | `outputTokenType` | `outputTokenNames` (non-empty array; single `outputTokenName` sugar), exclusive with it |
| activity | flow guard / transformation contracts | bound CLR contract by name | names come from the flow's `tokenNames` and the transformation's `outputTokenNames`, as in C# (section 4.4); the same validation applies |

- Single `tokenName`/`eventName` string forms remain as sugar where an `eventName`
  key already exists.
- Missing, unregistered or duplicate names produce diagnostics in the existing
  `JSON-…` series (new codes assigned at implementation).
- `payload.schema.json` replaces `event.schema.json`; payload definitions are
  registered via `JsonPayloadDefinition.Parse(…).Register(payloadsBuilder)` inside
  `AddPayloads`.

### 5.2 MinimalAPIs

- Renames per section 1.2.
- Payload tokens over HTTP: for each payload name declared by an activity **input
  pin**, `POST /activities/{activityName}/{instance}/input/{tokenName}` accepts a
  body decoded by the payload definition; each **output pin** payload name gets the
  analogous output endpoint. The path segment is the payload name, unique because
  payload names cannot collide with CLR event/token names (section 2). OpenAPI schema
  derives from `PayloadDefinition`. Interceptor hooks mirror the existing token
  endpoint hooks. Payload names registered as requests (`AddRequest`) get
  **no** pin endpoints, because an HTTP-delivered token has no responder.
- Existing typed pin endpoints are unchanged.
- Entity endpoints: each payload name declared by an entity initializer or mutation
  (section 3.8), and each mapped event type of an `ITypeMapper` initializer or
  mutation (decision D26), gets an endpoint analogous to the typed event endpoint of
  the same role (initializer: allowed while not initialized; mutation: while
  initialized), with the same field validation and authorization rules.

### 5.3 Other extensions

No new API. Renames only: FluentValidation, Scheduling, MicrosoftAgentFramework,
OpenTelemetry (payload-aware token attribute reads `TokenName`), PlantUml (payload
pin/flow labels use the name set), Visualizer, Orleans, EF Core / Mongo storage.

---

## 6. Validation rules

Reported as behavior-definition errors when the behavior is registered or built
(error codes assigned at implementation):

| # | Rule |
| --- | --- |
| V1 | A payload name set is non-empty, has no empty/whitespace entries and no duplicates. |
| V2 | Every name is registered via `AddPayloads`. |
| V3 | A payload name does not equal a registered CLR event or token name. |
| V4 | `Payload`, `PayloadRequest`, `PayloadValue` as a type argument that **selects** tokens (`AddFlow<Payload>`, `AddInputPin<Payload>`, `AddDecision<Payload>`, iterative/parallel, `AddTransformation<Payload>` without `transformedNames`, injected generic interfaces) is rejected; use the name-taking overloads (`AddFlow(tokenNames, …)`, `AddInputPin(tokenNames, …)`, …), the `transformedNames` overloads or `[TokenName]`. |
| V4a | A class-based handler closed over `Payload` (`IInitializer<Payload>`, `IDeferralGuard<Payload>`, `IMutation<TTemplate, Payload>`, `IFlowGuard<Payload>`, `IFlowTransformation<Payload, …>`, `IFlowTransformation<…, Payload>`) is accepted only through a member that takes the corresponding names (sections 3.9 and 4.4); registering it through a typed member without names is rejected. |
| V5 | Non-generic injected interface without `[TokenName]` is rejected. |
| V6 | `[TokenName]` on anything other than a non-generic injected token interface is rejected. |
| V7 | Flow identity stays unique within an activity; two flows on the same source with the same name set and target are rejected like duplicate typed flows (ACT-017 behavior). |
| V8 | A transformation to payloads (delegate or class) declares a non-empty `transformedNames` set (V1–V3 apply); it must return a `Payload` named in the set (run-time `ArgumentException` otherwise). |
| V9 | A single transition/initializer/deferral/accept-event declaration is either CLR-typed or name-based. |
| V10 | Initializer, deferral and transition name sets follow typed rules for duplicates (two identical events declared on one source are handled as typed duplicates are today). |
| V11 | Flow checks mirror the typed checks (section 4.4.4): original names vs. the source's declared outputs and transformed names vs. the target's declared inputs, in either direction of containment; a required input has an incoming flow whose transformed names ⊆ its declared names; each original name of an outgoing flow of a transitive node appears in an incoming flow's transformed names; nodes that declare nothing are unconstrained. |
| V12 | Entity: a template method with a `Payload` parameter must list names in `[Mutation]` / `[Initializer]`; names on a method with another parameter type are rejected; two annotated methods claiming one name (or mapping to one event type) are rejected (section 3.8). |

---

## 7. Breaking and behavioral changes

| # | Change | Impact |
| --- | --- | --- |
| B1 | All renames in section 1 (types, namespaces, packages' public surface, schema file name, endpoint hooks). | Source-breaking for every consumer of the old names. No forwarders. |
| B2 | Persisted data (contexts, streams, token queues, in-flight tokens) written before the rename cannot be read (CLR type names are stored). | Data break; accepted (D3). |
| B3 | Activity `AddInitializer<TMapped>(…)` begins to match every mapped type of `ITypeMapper` types (OneOf/UnionTypes). | Bug fix; behavior change for code that worked around it. |
| B4 | `UseTransition(string eventName, …)` matches by name **set equality** instead of "set contains the name". | Only affects JSON-defined multi-name transitions overridden from C#. |
| B5 | Typed wildcard consumers (`object`, base types) now also receive payload tokens when both exist in one activity. | Documented, expected (D13). |
| B6 | Fixes to typed token collections made in the prerequisite work package (e.g. `OutputTokens<T>.Clear/Contains/Remove`, `OptionalInputOutput*` collections that did not write to the node output) change their behavior. | Bug fixes. |
| B7 | `IActivityVisitor`, `IActionVisitor`, inspection interfaces gain members. | Non-breaking (default interface members). |
| B8 | `IOptionalInputOutputTokens<T>` and `IOptionalInputOutputToken<T>` gain `IOutputTokens<T>` as a base interface (D20). | Additive for callers; source-breaking only for third-party implementers of these interfaces (unlikely; they are injected). |
| B9 | Entity handler lookup becomes name-keyed for payload events (typed handlers stay type-keyed) and fans out over every mapped type of an `ITypeMapper` type; `[Mutation]` / `[Initializer]` gain an optional `params string[]` constructor. | Additive; an entity handler declared with OneOf/UnionTypes, which matched nothing before, starts matching (D26). |
| B10 | Class-based handlers closed over `Payload` are registered only through members with required names (V4a). | New capability, no existing code affected (`Payload` was not accepted before). |
| B11 | `AddFlow<TToken, TTransformedToken, TTransformationFlow…>`, the class-based `AddElseFlow` forms and `AddTransformation` on the four flow-chain builders gain `transformedNames` overloads constrained to `TTransformedToken : Payload` (D31). | Additive; verified by a compile probe to leave existing calls unambiguous. |
| B12 | JSON state-machine documents: the initializer and deferral key `triggers` is renamed `events` (vocabulary rule: "trigger" only for transitions). | Breaking for existing JSON documents, like the other renames (D2). |
| B13 | Activity builders gain name-first overloads of the existing member names (`AddFlow`, `UseFlow`, `AddElseFlow`, `AddInputPin`, `UseInputPin`, `AddOutputPin`, `AddDecision`, `UseDecision`, `Add/UseIterativeActivity`, `Add/UseParallelActivity`) and Action/visitor/context/inspection members named with "token" (section 4). | Additive; a compile probe of the full overload set against the existing typed members showed no conflict once the single-name forms listed in sections 4.4.2 and 4.6 are omitted. |

Not breaking: all typed token and typed event APIs, all existing method names on
builders, DI registrations of the typed interfaces, typed flow/pin/node identifiers.

---

## 8. Proposed defaults awaiting veto

Each of these was a decision I made provisionally. **All are confirmed**; items 10 and
13 were revised by you and are recorded in their revised form.

1. `IPayloadsBuilder.AddRequest` / `AddRequest<TResponse>` keep their names.
2. Name-collision rule extended from CLR event names to CLR **token** names.
3. `Use*Transition` with payload names matches by exact name-set equality.
4. Adding a `Payload` with an undeclared name through an injected output collection
   or `Output<Payload>` throws `ArgumentException`.
5. Rule V4 applies to declarations only; runtime context queries with
   `GetTokensOfType<Payload>()` stay legal.
6. No `OutputPayload` / `OutputPayloadRange` on `IOutput`; `Output<Payload>(payload)`
   routes by name. `PassPayloadTokensOn(names)` is added.
7. Context declaration-level name properties are lists (`TokenNames`,
   `InputTokenNames`, …), empty for typed elements; `TokenType` for payload
   elements is `typeof(Payload)`.
8. One generic visitor callback `TokenNamesDeclaredAsync` + `TokenNamesDeclarationKind`
   instead of one callback per element kind.
9. Default pin name and flow identifier forms for name sets: sorted, ordinal, with
   an unambiguous separator, distinct from typed forms.
10. **Revised.** Vocabulary: inside the models a payload is a **token** or an **event**
    (exceptions using `PayloadToken` are listed in "How to read this document");
    "trigger" only for state-machine transitions; initializers and deferrals consume
    events. Builder members are name-first overloads of the typed names. JSON keys:
    `tokenNames`, `eventNames`, `iterationTokenNames`, `parallelizationTokenNames`,
    `outputTokenNames`; state-machine initializers and deferrals use `events` instead
    of `triggers` (B12).
11. MinimalAPIs: one endpoint per payload name on payload pins, segment = token
    name; names registered as requests get no pin endpoints; entity initializers and
    mutations get one endpoint per declared name or mapped type.
12. `[TokenName]` with several names on a single-token interface (`IInputToken`)
    yields the first available matching token.
13. **Revised.** Wiki: payloads get a new page `4.6.-Payloads` (appended, so existing
    `4.2`–`4.5` links stay valid); `4.1.-Events` becomes
    `4.1.-Events`, describes typed events and only **mentions** that events can also
    be defined dynamically, linking to `4.6.-Payloads` for the details.
14. **Confirmed.** Required call-site names (D29) apply to every class-based handler closed over
    `Payload`: initializers, deferral guards and entity mutations as well as flow
    stages. I read "no optional names on call site" that way.
15. **Confirmed.** Name parameters are `IReadOnlyList<string>` with a single-`string` sugar overload
    (D28), and `transformedNames` comes **first** in the new transformation overloads
    (before the delegate, and before `targetNodeName`).
16. Entity `ITypeMapper` support: events are converted to the mapped parameter type
    like state machines do; handler uniqueness is per mapped type; visitor callbacks
    keep the declared type argument and report mapped event names.
17. Translation of the typed checks (section 4.4.4, decision D27): either-direction
    containment of original names against the source and transformed names against
    the target, `F ⊆ D` for required inputs, name-level coverage for transitive nodes
    (ACT-026), undeclared nodes unconstrained.
18. **Confirmed.** Name sets of a flow (section 4.4.1, D30): a payload flow without a transformation
    delivers its original names (transformed = original); a transformation to a typed
    token leaves no transformed names; in a chain the last transformation's names win.
19. The new transformation overloads are constrained `where TTransformedToken : Payload`,
    so `PayloadRequest` subclasses may be produced too (D22).

---

## 9. Decisions recorded and remaining open questions

### 9.1 Resolved by your answers

| # | Question | Decision |
| --- | --- | --- |
| 1 | Optional input-output interfaces without `IOutputTokens<T>` | Unintentional; `IOutputTokens<T>` is added to `IOptionalInputOutput*` (D20, B8, section 4.3). |
| 2 | Mixing CLR types and payload names in one declaration | Not in the first version (D6). |
| 3 | Typed→payload class-based transformation | In the first version; the produced names are the `transformedNames` given at registration (D31, section 4.4). Supersedes the attribute-based answer (D17). |
| 4 | Entities accept payload names | Yes, initializers **and** mutations (D19, section 3.8). |
| 5 | Overlap of typed and payload flows | Allowed, behaves like today's overlapping typed flows (D21). |
| 6 | Payload requests as tokens | Allowed; events sent to an activity may flow down the network as tokens (D22). |
| 7 | Class-based flow registration shape | Name-first overloads of the existing `AddFlow<…>` members when the flown type is `Payload`, with required original names; `transformedNames` overloads when the transformed type is `Payload` (D11, D31, section 4.4). |
| 8 | Where payloads are described in the wiki | A separate new page; the events page only mentions dynamically defined events and links to it (D25, section 8 item 13). |
| 9 | Attribute constructor shape | `params string[]` with an `IReadOnlyList<string>` property (CS0181); applies to `[TokenName]`, `[Mutation]`, `[Initializer]`. |
| 10 | Single-name sugar | Kept next to every list form, except the members whose single-name form collides with an existing typed member (sections 4.4.2, 4.6): lists only there. |
| 11 | Entity attributes and `ITypeMapper` | `[Mutation]` / `[Initializer]` carry names; entities get `ITypeMapper` support (D26, section 3.8). |
| 12 | Class-based initializers, deferral guards, entity mutations | Names are required at the call site; no class-level attributes (D29, section 3.9). Supersedes the attribute and optional-names answer (D23, D24). |
| 13 | Class-level name attributes | None: `[PayloadConsumer]` / `[PayloadProducer]` (and the earlier `[TokenConsumer]` / `[TokenProducer]`) are dropped (D29). |
| 14 | Pipeline checks | Match the typed checks (D27, section 4.4.4); this replaces the strict-subset proposal. |
| 15 | Flow name sets | Two sets per flow, original and transformed, exactly like `TokenType` / `TargetTokenType` (D30, section 4.4.1). |
| 16 | Transformation members producing payloads | Overloads with a `transformedNames` parameter when the transformed type is `Payload` (D31, section 4.4.3). |
| 17 | Vocabulary inside the models | "Token" or "event" only; `PayloadToken` only in the listed exceptions; "trigger" only for transitions; JSON keys `tokenNames`, `iterationTokenNames`, `outputTokenNames`, …, and `events` for state-machine initializers and deferrals (D32, B12). |
| 18 | Activity builder member names | No `Payload` prefix: name-first overloads of the typed member names; lists only where a single-name form would collide (D33, B13). |
| 19 | Section 8 defaults | All confirmed; items 10 and 13 revised. |

### 9.2 Remaining open questions

None.
