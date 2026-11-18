# Payloads and payload tokens plan

## Goal and scope

Three related changes, planned as one program of work:

1. **Rename** the dynamically defined event concept ("structural event") to
   **payload**: `AddEvents` becomes `AddPayloads`, and the type family, namespaces,
   JSON format, MinimalAPIs surface and docs follow.
2. **Event-consumption parity.** Everywhere a behavior consumes an event, support
   the full semantics: strong-typed CLR event, one or more payload names, and
   multi-type events through `ITypeMapper` (OneOf / UnionTypes). New places:
   state-machine initializers, activity initializers, state deferrals (names with
   guards), plus multi-name support in the places that already take a name.
3. **Payload tokens.** Dynamically defined payloads become first-class data tokens
   in activities (and actions), fully cross-compatible with typed tokens inside
   one activity: flows, guards, transformations, pins, decisions, iterative and
   parallel scopes, `TokenHolder`, `TokensInput`/`TokensOutput`, the injected
   `IInputTokens` / `IOutputTokens` / `IInputOutputTokens` families, contexts,
   observability, JSON definitions.

This document is a plan only. No code, API or documentation has been changed. The
proposed public API is reviewed separately in
[PAYLOADS-API-CHANGES.md](PAYLOADS-API-CHANGES.md); test coverage is planned in
[PAYLOADS-TEST-PLAN.md](PAYLOADS-TEST-PLAN.md); this document owns scope, findings,
sequencing, risks and verification.

Out of scope unless the owner says otherwise:

- Mixing CLR types and payload names inside one declaration (the design must not
  preclude it later; confirmed for the first version, decision D6).
- Compatibility with data persisted before the rename (decision D3).
- Renaming identifiers that say only "Event" (decision D7).

Entities are **in scope** since the owner's second and third rounds of decisions
(D19, D26): entity initializers and mutations accept payload names **and** multi-type
`ITypeMapper` events (OneOf / UnionTypes), which entities do not support today
(work package 2g).

## Decisions already taken

| # | Decision |
| --- | --- |
| D1 | Type family: `StructuralEvent`→`Payload`, `StructuralValue`→`PayloadValue`, `StructuralRequest`→`PayloadRequest` (and `<TResponse>`). |
| D2 | Hard rename. No `[Obsolete]` forwarders for old names (framework is 0.x beta). |
| D3 | No compatibility with data persisted before the rename. No `SerializationBinder`/alias work. |
| D4 | Injected non-generic token interfaces keep the existing names and derive from the `Payload` closure: `IInputTokens : IInputTokens<Payload>` etc. Payload names come from the attribute `[TokenName("name")]` on the constructor parameter. |
| D5 | `Payload` used as a plain generic argument without names is rejected by validation everywhere it would *select* tokens: `AddFlow<Payload>`, pins, decisions, iterative/parallel scopes, delegate `AddTransformation<Payload>`, and injected `IInputTokens<Payload>`-style parameters without `[TokenName]`. Refined by D29/D31: a class-based handler (flow guard/transformation, initializer, deferral guard, entity mutation) closed over `Payload` is accepted only through a member that takes its names at the call site. |
| D6 | One-or-more payload names per declaration, everywhere: transitions, accept-event actions, initializers, deferrals, payload flows/pins/decisions/scopes, `[TokenName]`. The set of names is open-ended. Names and CLR types are not mixed in one declaration (design leaves that open for later). Multi-type CLR events and tokens stay with `ITypeMapper`. |
| D7 | Rename every identifier containing "Structural" (including internals such as `Edge.StructuralEventNames`, `Node.StructuralEventName`, `Vertex.StructuralDeferrals`). Keep identifiers that only say "Event" (`AddAcceptEventAction`, `AddTransition(string eventName…)`, `ConsumedEventNames`, `EventHolder`). |
| D8 | Rename namespaces: `Stateflows.Events`→`Stateflows.Payloads`, `Stateflows.Events.Json`→`Stateflows.Payloads.Json`, `JsonEventDefinition`→`JsonPayloadDefinition`, `event.schema.json`→`payload.schema.json`. `Stateflows.Common` stays for core types. |
| D9 | Actions get the same payload token support as activities. |
| D10 | Suspected pre-existing bugs in the typed token collections are fixed first, as a separate prerequisite work package (WP0). |
| D11 | **Revised by D33.** Activity builders expose payload variants as name-first overloads of the existing member names (`AddFlow(tokenNames, …)`, `AddInputPin(tokenNames, …)`, …), not as a distinct `Payload*` family. |
| D12 | `ITokensInput` gets `Add(Payload)` / `AddRange(params Payload[])`; `TokensOutput` reads by name with `GetPayloadTokens(string name)`. Payload instances already carry their name (created by the payload factory). |
| D13 | Typed wildcard consumers (`object`, shared interfaces/base classes) keep `IsAssignableFrom` semantics and therefore also receive payload tokens. Only exact payload selection requires the name-taking overloads. |
| D14 | The JSON extension is extended in the same change (initializers, deferrals, pins, flows, decisions, schemas, README). |
| D15 | Multi-name declarations everywhere (carried over from D6). |
| D16 | Transformations to payloads (delegate and, since D31, class-based) take a single name (sugar) or a set of names; the transformation must return a `Payload` named in the set, otherwise `ArgumentException`. Replaces the earlier "exactly one name" rule. |
| D17 | **Superseded by D29–D31.** (Was: class attributes advertising the names of flow guards and transformations.) Still valid: flows whose flown type is `Payload` use the name-taking `AddFlow<…>` overloads (D11, D33). |
| D18 | **Superseded by D27.** (Was: strict subset checks through the whole pipeline.) |
| D19 | Entity initializers and mutations support payload names (builder, template-attribute and class forms). |
| D20 | `IOptionalInputOutputTokens<T>` / `IOptionalInputOutputToken<T>` add `IOutputTokens<T>`; non-generic forms mirror it. |
| D21 | Typed and payload flows may overlap on one token; behavior follows today's overlapping typed flows. No extra validation. |
| D22 | `PayloadRequest` is a legal token; events sent to an activity may flow down the network as tokens. |
| D23 | **Superseded by D29.** (Was: `PayloadConsumer` / `PayloadProducer` class attributes.) |
| D24 | **Superseded by D29.** (Was: optional call-site names defaulting to the attribute's names.) |
| D25 | Wiki: payloads get a **new separate page** `4.6.-Payloads`; `4.1.-Events` becomes `4.1.-Events` describes typed events and only mentions dynamically defined events, linking to the payloads page (D34). |
| D26 | Entities gain `ITypeMapper` support (OneOf / UnionTypes) for initializers and mutations: builder forms, `[Mutation]` / `[Initializer]` template methods and class forms fan out over `TypeMapper.GetMappedTypes`, converting with `ImplicitConverter.TryConvert<TEvent>` as state-machine `EventContext<TEvent>` does. |
| D27 | Flow checks **match the typed checks** (ACT-026…029), translated from "assignable" to name-set relations: original names play `TokenType`, transformed names play `TargetTokenType` (D30). Supersedes D18 and R13's strict-subset rule. Stages declare no names, so there is no stage-to-stage check. Nodes with no token declarations stay unconstrained, as in the typed checks. |
| D28 | `params string[]` for attribute constructors (`[TokenName]`, `[Mutation]`, `[Initializer]`) and both a single-name sugar and an `IReadOnlyList<string>` form for every name-taking member are confirmed; exceptions: the members whose single-name form collides with an existing typed member are list-only (D33). |
| D29 | **No class-level name attributes and no optional names.** Class-based handlers closed over `Payload` — initializer, deferral-guard and entity mutation classes, flow guards and flow transformations — receive their names as a **required** call-site parameter. Registering such a class through a typed member without names is a definition error. Supersedes D17, D23 and D24. Applying it to initializers, deferral guards and mutations, and not only to flows, is my reading of "no optional names on call site". |
| D30 | A flow carries **two name sets**, exactly like its two token types (`Edge.TokenType` / `TargetTokenType`): **original names**, matched against the source's tokens, and **transformed names**, delivered to the target. Without a transformation, transformed = original; a transformation to a typed token leaves no transformed names; in a chain the last transformation wins (proposed defaults). |
| D31 | Transformation-adding members get overloads with a `transformedNames` parameter (first parameter) when the transformed type is `Payload` (`where TTransformedToken : Payload`): delegate and class `AddTransformation` on all four flow-chain builders, node-level `AddFlow<TToken, TTransformedToken, TFlow…>`, class-based `AddElseFlow` forms, and the payload→payload `AddFlow<TTransformedToken, TFlow…>(tokenNames, …)` forms. Replaces the earlier `AddPayloadTransformation`. Verified by a compile probe: no ambiguity with existing members. |
| D32 | **Vocabulary.** Inside the state-machine, activity, Action and entity models a payload is a **token** or an **event**; `Payload` names only the model-independent definition mechanism (`AddPayloads`, `Payload`, `PayloadRequest`, JSON payload definitions). `PayloadToken` appears only where needed to avoid ambiguity (`IInput.GetPayloadTokens` / `HasPayloadTokens`, `IOutput.PassPayloadTokensOn`, `ITokensOutput.GetPayloadTokens`, Action `AddConsumedPayloadToken` / `AddProducedPayloadToken`). "Trigger" is used only for state-machine transitions; initializers and deferrals consume "events". Parameter, property, visitor and JSON names follow: `tokenNames`, `eventNames`, `iterationTokenNames`, `parallelizationTokenNames`, `outputTokenNames`, `TokenNames`, `TokenNamesDeclaredAsync`. Internal identifiers are not renamed by this rule. |
| D33 | **No `Payload` prefix on builder members.** Payload variants are overloads of the typed member names with the name list as first parameter. A compile probe showed the list forms never collide; the single-name forms of `AddFlow<TTargetNode>`, `UseFlow<TTargetNode>`, `AddFlow<TFlow, TTargetNode>`, `AddFlow<TToken, TTransformedToken, TTransformationFlow>` (both string forms) and the unnamed `Add/UseIterativeActivity<TActivity>` / `Add/UseParallelActivity<TActivity>` collide, so these are **list-only**. |
| D34 | JSON state-machine initializers and deferrals use the key `events` (was `triggers`) with the transition event shape (one `clrType` or a unique list of `eventName`); `triggers` there is rejected. The wiki events page mentions dynamically defined events only briefly and links to `4.6.-Payloads`. |

## Findings that correct or refine the brief

These came from reading the code; several change the shape of the work.

1. **State deferral by name already exists.** `IStateUtils.AddDeferredEvent(string
   eventName, DeferralBuildAction<StructuralEvent>)` is implemented as a default
   interface method
   ([IStateUtils.cs](Core/Stateflows/StateMachines/Registration/Interfaces/Base/IStateUtils.cs)),
   covered by `StructuralEvents.cs` and `VisitorMetadata.cs` tests. Typed deferral
   already maps through `ITypeMapper`
   ([StateBuilder.cs](Core/Stateflows/StateMachines/Registration/Builders/StateBuilder.cs)).
   Real deferral gaps: multi-name (D6), a named overload with a guard class
   (`IDeferralGuard<Payload>`), and JSON (README states initializer and deferral
   declarations "still use `clrType`").
2. **Transitions and accept-event already accept one name from C#.** An edge is
   either named (`Edge.StructuralEventNames`, a list) or type-mapped
   (`ActualTriggerTypes`), never mixed. Multi-name transitions exist only through
   the JSON binder, which sets the list directly
   ([BoundDocument.Construction.cs](Extensions/Stateflows.Extensions.Json/BoundDocument.Construction.cs)).
   D6 promotes that capability to the public C# API.
3. **State-machine initializers are CLR-only but already name-keyed at runtime.**
   `Graph.Initializers` is keyed by event name and looked up with
   `eventHolder.Name`
   ([Executor.cs](Core/Stateflows/StateMachines/Engine/Executor.cs)); the builder
   registers one runtime entry per mapped type. A payload-name initializer is a
   natural fit. `InitializerDeclarations` is keyed by `Type` and `InitializerTypes`
   feeds `GetExpectedEvents`; both need a name-aware counterpart.
4. **Activity initializers appear not to honor `ITypeMapper` at runtime.** The
   builder keys `Graph.Initializers` by `typeof(TInitializationEvent).GetEventName()`
   ([ActivityBuilder.cs](Core/Stateflows/Activities/Registration/Builders/ActivityBuilder.cs)),
   while `DoInitializeActivityAsync`
   ([Executor.cs](Core/Stateflows/Activities/Engine/Executor.cs)) looks up the CLR
   type name of the *runtime* event. `AddInitializer<OneOf<A, B>>` therefore would
   not match an `A`. No test exists. WP2 starts with a failing test to confirm,
   then fixes it as part of parity.
5. **Persistence stores CLR type names.** `StateflowsJsonConverter` uses
   `TypeNameHandling.All` without a binder
   ([StateflowsJsonConverter.cs](Core/Stateflows.Common/Utilities/StateflowsJsonConverter.cs)).
   Renaming types/namespaces invalidates previously persisted contexts, streams
   and token queues (EF Core, Orleans, Mongo). Accepted by D3; release notes must
   say so.
6. **Token selection is spread over ~20 sites** using
   `type.IsAssignableFrom(token.ActualPayloadType)`. See WP3 for the list. Payload
   support needs one shared selector.
7. **Flow identity is type-name based.** `Graph.Build` composes edge identifiers
   from `GetTokenName()` of the source and target token types, and ACT-017
   requires uniqueness. Payload flows need a name-based identifier component.
8. **Non-generic injection fits an existing mechanism.**
   `StateflowsActivator.ResolveParameterValuesAsync` already resolves constructor
   parameters by attribute (`[StateValue]`, `[FromField]`, …) and does not use
   `ActivatorUtilities` for model elements
   ([StateflowsActivator.cs](Core/Stateflows/Common/Classes/StateflowsActivator.cs)).
   `[TokenName]` is another branch there. `Node.ScanForDeclaredTypes` scans the
   constructor for token declarations and must also read the attribute.
9. **Actions reuse the Activities token collections** (same `IInputTokens<>`
   registrations in
   [ActionsDependencyInjection.cs](Core/Stateflows/Actions/ActionsDependencyInjection.cs)).
   Actions can already receive a structural event as a token
   (`StructuralConsumedEvents` test).
10. **`TokenHolder.Name` is already virtual.** A payload-backed holder can expose
    the payload's name without changing the holder hierarchy.
11. **Suspected existing bugs** (not verified, not fixed yet; see WP0):
    `OutputTokens<T>.Clear` tests `token is TToken` against a `TokenHolder`;
    `Contains/Remove/CopyTo` build a holder with a fresh `Guid` so they never match;
    `OptionalInputOutputTokens<T>` / `OptionalInputOutputToken<T>` implement the
    `IOptionalInput*` interfaces instead of `IOptionalInputOutput*`;
    `OptionalInputOutputTokens.PassAllOn` writes to a throwaway collection. Also
    `IOptionalInputOutputTokens<T>` / `IOptionalInputOutputToken<T>` expose only
    `PassAllOn()` / `PassOn()` and do not extend `IOutputTokens<T>`, unlike
    `IInputOutputTokens<T>`; you confirmed this is an omission, so D20 adds the base
    interface (WP0).
12. **Overload ambiguity is real** (D11): `AddFlow<TControlFlow>(string
    targetNodeName, ControlFlowBuildAction = null)` and a hypothetical
    `AddFlow<TTargetNode>(string payloadName, ObjectFlowBuildAction<Payload> = null)`
    share a shape, so `AddFlow<MyNode>("x")` would not compile.
13. **Entities are keyed by type at every level.** `Model.Initializers[typeof(T)]` and
    `Model.Mutations[typeof(T)]` are dictionaries with replace semantics; the entity
    executor looks handlers up by the static `typeof(TEvent)` in
    `TryInitializeAsync` / `DoProcessAsync`
    ([Executor.cs](Core/Stateflows/Entities/Engine/Executor.cs)); expected-event reporting
    and `EntityRegistration.VisitAsync` list event names; `EntitiesBuilder.AnalyzeTemplate`
    discovers `[Mutation]` / `[Initializer]` methods by parameter type; the MinimalAPIs
    `EntityVisitor` keys endpoints by `typeof(TEvent)`. Payload support needs
    name-keyed counterparts in all of these (WP2g). `NoImplicitInitialization` is read
    from the payload's CLR type attribute or the header; payloads can use only the header.
14. **Class-based flow stages carry no name information.** `IFlowGuard<TToken>` and
    `IFlowTransformation<TToken, TTransformed>` classes are registered by type only, so
    a class using `Payload` cannot say which payloads it handles. D29–D31 solve it with
    required names at the registration call: the flow's original names for guards and
    payload-consuming transformations, `transformedNames` for payload-producing ones;
    `Edge` stores both sets beside `TokenType` / `TargetTokenType` (WP4).
15. **C# attributes cannot take `IReadOnlyList<string>`** (CS0181, verified with a
    compile probe). `[TokenName]`, `[Mutation]` and `[Initializer]` take
    `params string[]` and expose an `IReadOnlyList<string>` property.
16. **A nested legacy test project exists.**
    `Tests/StateMachine/StateMachine.IntegrationTests` (net6.0, 47 tracked files) is
    not part of `Stateflows.sln`; it is a stale duplicate of
    `Tests/StateMachine.IntegrationTests`. The test plan targets the solution
    project and the legacy copy is left alone.

## Target design

### Payload (renamed concept)

`Payload` is the instance type of a dynamically defined DTO (today
`StructuralEvent`): a name plus validated fields, created only through
`IPayloadFactory` (internal constructors), registered with `AddPayloads`, with a
request flavor (`PayloadRequest`). A payload can be an event, a token, or both;
the registry is shared. Payload names are unique in one namespace.

### Token key

Introduce one internal concept, the *token key*, as the only thing the engine,
model and validators use to describe "which tokens":

- **Typed key**: a CLR type; matches by `IsAssignableFrom(ActualPayloadType)` as today.
- **Payload key**: a non-empty set of payload names; matches a token whose
  `BoxedPayload` is a `Payload` whose `Name` is in the set. CLR type is irrelevant.

Everything that today stores a `Type` for pins, flows, declared inputs/outputs and
guards moves to a key. A key has a stable identifier string (typed: token name as
today; payload: sorted names joined with an unambiguous separator) used in edge
identifiers, pin node names, inspection and visitors.

### Payload token

A payload token is a `TokenHolder<Payload>` whose `Name` is the payload name.
`ActualPayloadType` is `Payload` or `PayloadRequest`. No new holder class is
required; the holder's name resolution is the only change in the transport type.

### Cross-compatibility rules

| # | Rule |
| --- | --- |
| R1 | Typed declarations match by type assignability exactly as today (D13). Wildcards such as `object` also receive payload tokens. |
| R2 | Payload declarations match by name set only. |
| R3 | Declaring `Payload`, `PayloadRequest` or `PayloadValue` as a CLR token type in a selecting position is a definition error (D5). Runtime context queries (`GetTokensOfType<Payload>()`) stay legal and return all payload tokens. |
| R4 | Payload names used by a behavior must be registered with `AddPayloads` before the behavior is built. Unknown names are definition errors. |
| R5 | A payload name must not equal a CLR event or token name known to the host. Today this is enforced for events by `EventsRegister`; extend to token names. This also keeps MinimalAPIs pin endpoint segments (`input/{name}`) unique. |
| R6 | One node can declare typed and payload inputs/outputs together; one activity can contain typed, payload, and bridged flows. |
| R7 | A transformation can bridge any combination: typed→typed (today), payload→typed, typed→payload, payload→payload. A payload target declares a set of one or more names (D16); a delegate or class producing a name outside that set is rejected at run time. |
| R8 | Overlapping selections behave like overlapping typed flows today (D21). WP0 pins down that behavior with characterization tests before refactoring. |
| R9 | Adding a payload through an injected output collection or context output whose name is not in the declared set is rejected at run time. |
| R10 | Control tokens, node-reference tokens and exception tokens are unchanged. |
| R11 | Accept-event actions with payload names emit payload tokens named after the received payload, so they can feed name-first `AddFlow` overloads. |
| R12 | Class-based handlers closed over `Payload` (flow guards, flow transformations, initializer, deferral-guard and entity mutation classes) are registered only through members that take their names at the call site (D29). Registering one through a typed member without names is a definition error. Guard and transformation classes operate on the names in effect at their position in the flow; transition guards/effects and accept-event classes on the names of their declaration. |
| R13 | Flow checks match the typed checks (D27; API report section 4.4.4), translated from type assignability to name-set containment, with original names in the role of `TokenType` and transformed names in the role of `TargetTokenType` (D30): original names against the source's declared outputs and transformed names against the target's declared inputs are accepted when one set contains the other (ACT-028/027); a required payload input needs an incoming flow whose transformed names ⊆ the input's names (ACT-029); in transitive nodes every original name of an outgoing flow appears in some incoming flow's transformed names (ACT-026). Stages declare no names, so nothing is checked between them. Nodes that declare nothing are unconstrained; pins, decisions and data stores always declare. A typed `object` declaration accepts payload edges; an unrelated typed declaration rejects them. |
| R14 | `PayloadRequest` is accepted as a token like any payload (D22); `Respond` works only while the instance is in memory and is characterized in WP0 for typed request tokens first. |
| R15 | Entity initializers and mutations with payload names dispatch by `eventHolder.Name`; one handler per name, later registration replaces earlier ones (D19). Mapped CLR types (OneOf / UnionTypes) are keyed per mapped type with the same replace semantics (D26). |

| R16 | A flow has original and transformed name sets (D30). Each transformation to `Payload` replaces the transformed names with its `transformedNames`; a transformation to a typed token clears them; without transformations they equal the original names. |

## Work packages

Packages are ordered; each one ends with the full suite green. Package sizes are
relative.

### WP0 — Safety net (small–medium)

- Fix the typed token collection bugs from finding 11 with tests first
  ([Output.cs](Core/Stateflows/Activities/Collections/Output.cs),
  [InputOutput.cs](Core/Stateflows/Activities/Collections/InputOutput.cs)).
  Check whether the same code is duplicated for Actions. Includes the D20 change:
  `IOptionalInputOutputTokens<T>` / `IOptionalInputOutputToken<T>` extend
  `IOutputTokens<T>` and the concrete classes implement the new surface
  (`PassAllOn` writes to the real output).
- Characterization tests (no behavior change) for: overlapping typed flows and else
  flows (D21), wildcard `object` pins/flows/injection, class-based guards and
  transformations (including one class acting as both), iterative/parallel
  partitioning, token routing across scopes, typed request tokens
  (`IRequest<TResponse>` as a token, `Respond`, persistence of an in-flight request
  token; D22), entity typed initializers/mutations/expected events (baseline for WP2g).
- Failing-first test for activity `AddInitializer<OneOf<…>>` (finding 4).
- Persisted-context round trip of an activity with in-flight tokens (baseline for
  WP6).

### WP1 — Rename (large, mechanical)

One branch, no behavior change, `git mv` so history follows files.

- Core types: [Payloads](Core/Stateflows.Common/Payloads) folder (value, event,
  request, converter, definition, factory, register, extensions) and
  [Events registration](Core/Stateflows/Events) (`EventsRegister`, builders,
  DI) → `Payloads` folders and names per the API report. `StateflowsBuilder.Events`
  becomes `Payloads`.
- Internals containing "Structural" (D7): `Edge.StructuralEventNames`,
  `Node.StructuralEventName`, `Vertex.StructuralDeferrals`, `DeferralTypes`
  markers, executor/validator references, builder parameters
  (`structuralEventName`), `TransitionBuildAction<StructuralEvent>`,
  `DeferralBuildAction<StructuralEvent>`, `AcceptEventActionDelegateAsync<StructuralEvent>`.
- State-machine and activity builder surfaces keep their method names; only the
  `StructuralEvent` type argument and docs change.
- Extensions: Json (types, namespace, schema file, README, JSON-diagnostic text),
  MinimalAPIs (`Structural\*` folder, `StructuralEventEndpoints`,
  `StructuralRequestReader`, `StructuralOpenApi`, `StructuralJson`, interceptor
  hooks `Before/AfterStructuralEventEndpointDefinition`,
  `EndpointRouteRegistry`, visitors), FluentValidation, Scheduling
  (`CronEventValidator`), MicrosoftAgentFramework (formatter, `AIAgentAction`),
  OpenTelemetry, ClearScript tests.
- Tests and examples: about a dozen `Structural*` test files
  (StateMachine, Activity, Action, Entity, Orleans, AIAgent, Json, ClearScript),
  Examples.
- XML doc `seealso` links to the wiki page `4.1.-Events`
  (wiki page rename and new payloads page listed in WP9, D25).
- Exit: solution builds; all tests pass; a repo grep for `Structural` returns only
  intentional leftovers (documented list); release note states the persistence
  break (D3).

### WP2 — Event-consumption parity (large)

All with one-or-more names (D6) and CLR/ITypeMapper multi-type (entities too since
D26; sub-package 2g). Class-based handlers take their names at the call site (D29,
sub-package 2h).

**2a State-machine initializers.** New named overloads on
`IStateMachineEvents<TReturn>`
([IStateMachineEvents.cs](Core/Stateflows/StateMachines/Registration/Interfaces/Base/IStateMachineEvents.cs))
and implementation in
[StateMachineBuilder.cs](Core/Stateflows/StateMachines/Registration/Builders/StateMachineBuilder.cs).
Runtime: keyed by name already. Need: a name-keyed declaration alongside
`InitializerDeclarations` (typed key collides, since many named initializers share
the `Payload` type), `InitializerTypes`/expected-events for names, graph
validation, visitor metadata (existing `InitializerAddedAsync<TEvent>` with
`TEvent = Payload` and its `eventNames` list suffices), implementation fan-out to
every builder variant implementing `IStateMachineEvents<…>` (overridden,
initialized, finalized variants).

**2b Activity initializers.** Named overloads on `IActivityEvents<TReturn>`
([IActivityEvents.cs](Core/Stateflows/Activities/Registration/Interfaces/Base/IActivityEvents.cs)).
Fix the runtime lookup so mapped CLR types and names both work
(`Executor.DoInitializeActivityAsync`; builder keying; `Graph.Build`
`InitializerEventNames`; `Processor.CanAdmitStream`,
`NoImplicitInitialization`; `GetExpectedEvents`/`GetExpectedEventNames`). This is a
behavior fix for OneOf/UnionTypes users; call it out in release notes.

**2c Deferral.** Multi-name overload, named overload with guard class, inheritance
(`Vertex.PrepareDeferrals`), composite/orthogonal builder fan-out
([CompositeStateBuilder.cs](Core/Stateflows/StateMachines/Registration/Builders/CompositeStateBuilder.cs)
has ~30 explicit implementations), queue re-evaluation and expected-event reporting.

**2d Transitions and accept-event.** Multi-name overloads for
`IStateTransitions` / `IStateTransitionsOverrides` / `ITransitionBuilder.ChangeTrigger`
and accept-event actions (`AcceptEventNodeBuilder.ChangeAcceptedEvent`,
`BaseActivityBuilder.AddAcceptEventAction`). `Node.StructuralEventName` (single)
becomes a name list; `IsAcceptedBy`, `Graph.Build` `EffectiveEventNames` and the
JSON binder move to the public path. The `Edge` list already exists.

**2e JSON.** In
[state-machine.schema.json](Extensions/Stateflows.Extensions.Json/state-machine.schema.json)
initializers and deferrals currently use the key `triggers` with the `triggers` definition
(exactly one `clrType`); rename the key to `events` (D34) and switch it to the
`eventTriggers` definition already used by transitions (one `clrType`, or a
unique list of `eventName` entries); `triggers` on initializers and deferrals is
rejected. In
`activity.schema.json` add `eventNames` for initializers and accept-event nodes.
Binder support in `BoundDocument*`; README ("initializer and deferral trigger
declarations still use `clrType`" is removed).

**2f ITypeMapper.** No change for state machines and activities: names and mapped
types stay separate declaration forms (D6). Entities **gain** `ITypeMapper` support
in 2g (D26).

**2g Entity initializers and mutations (D19, D26).** The entity model is keyed by type at
every level (finding 13). Work:

- Builder: `IEntityBuilder<TTemplate>` — `AddInitializer(names, InitializerAction<TTemplate, Payload>)`,
  `AddMutation(names, MutationAction<TTemplate, Payload>)`,
  `AddMutation<TMutation>(names = null) where TMutation : class, IMutation<TTemplate, Payload>`
  with `IReadOnlyList<string>` and single-`string` sugar forms; implementation in
  [EntityBuilder.cs](Core/Stateflows/Entities/Registration/Builders/EntityBuilder.cs).
- `ITypeMapper` (D26): typed builder forms (`AddInitializer<TEvent>`,
  `AddMutation<TEvent>`) fan out over `TypeMapper.GetMappedTypes` like
  `StateMachineBuilder.AddInitializer` and `StateBuilder.AddDeferredEvent` do, with
  runtime conversion through `ImplicitConverter.TryConvert<TEvent>` as in
  `EventContext<TEvent>`. Today `EntityBuilder.AddInitializer` (~line 50) and
  `AddMutation` (~line 88) key by `typeof(TEvent)` and cast `(TEvent)eventObj`.
  Handler uniqueness is per mapped type; the visitor reports the declared type
  argument and mapped event names. Failing-first tests mirror finding 4.
- Attributes: `[Mutation]` / `[Initializer]` get `params string[] eventNames` and an
  `EventNames` property; `EntitiesBuilder.AnalyzeTemplate` registers annotated methods
  whose parameter is `Payload` under each name, and methods whose parameter is a
  mapped type (OneOf / UnionTypes) under each mapped type; annotated duplicates are
  rejected, a later builder registration replaces an earlier one (typed semantics).
- Model: name-keyed `Initializers` / `Mutations` entries beside the type-keyed ones.
- Executor: `TryInitializeAsync` and `DoProcessAsync` look up by
  `eventHolder.Name` for payload events
  ([Executor.cs](Core/Stateflows/Entities/Engine/Executor.cs));
  `GetExpectedEventNames`, `EntityRegistration.VisitAsync` and the visitor
  `eventNames` lists include names; `Processor` honors `NoImplicitInitialization`
  from the header only.
- Contexts: `IEntityInitializationContext.InitializationEvent` and
  `IMutationContext.MutationEvent` carry the `Payload`;
  `EntityInspection.MutationAsync` and the observation contexts accept it.
- MinimalAPIs `EntityVisitor`: one endpoint per declared name or mapped type;
  authorization read from the `[Mutation]` method by name
  ([EntityVisitor.cs](Extensions/Stateflows.Extensions.MinimalAPIs/Visitors/EntityVisitor.cs)).
- Class-based entity handlers (`IMutation<TTemplate, Payload>`) use
  `AddMutation<TMutation>(names)` with required names (2h).

**2h Class-based handlers with required names (D29).** Registration methods for
initializer classes (state machine, activity, entity), deferral-guard classes and
entity mutation classes take `IReadOnlyList<string>` names (plus `string` sugar) as a
required parameter: `AddInitializer<TInitializer>(names)`,
`AddDeferredEvent<TGuard>(names, …)`, `AddMutation<TMutation>(names)`. The existing
typed members reject `Payload` (`AddInitializer<Payload, TInit>()`,
`AddDeferredEvent<Payload, TGuard>()`, `AddMutation<Payload, TMutation>()`) with the
behavior's definition exception (R12). A compile probe of the full overload set shows
no CS0111/CS0121 conflicts; include compile-only overload resolution tests to keep it
so.

Exit: parity matrix tests (initializer × {typed, OneOf, UnionTypes, one name, many
names} × {state machine, activity, entity}; same for deferral and transition); entity
named initializer/mutation tests; expected-events reporting; visitor metadata;
class-handler tests (names given, one and several; `Payload` through a typed member
rejected).

### WP3 — Token key and selector foundation (medium, behavior-preserving)

Introduce the token key, the shared selector and the payload-aware
`TokenHolder.Name`, then migrate every matching site to it. No new user feature;
the full existing suite must stay green. Sites (from the code review, line numbers
approximate):

- [Executor.cs](Core/Stateflows/Activities/Engine/Executor.cs): pin admission
  (~1113, ~1177), edge activation (~1715), missing required input tokens (~1938),
  `DoHandleEdgeAsync` token filtering (~2074).
- [Executor.Interleaving.cs](Core/Stateflows/Activities/Engine/Executor.Interleaving.cs):
  `OfTokenType` partitioning (~227, ~252).
- [Node.cs](Core/Stateflows/Activities/Models/Node.cs): `AcceptsInput`,
  `PinTokenType`, `GetIncomingTokenTypes`/`GetOutgoingTokenTypes`,
  `ScanForDeclaredTypes`; [Edge.cs](Core/Stateflows/Activities/Models/Edge.cs):
  `TokenType`, `TargetTokenType`, `TokenPipeline`, `Operations`.
- [Graph.cs](Core/Stateflows/Activities/Models/Graph.cs): identifier composition
  (~120–140), validation (ACT-017, ACT-026, pin/declared-type checks ~187–272,
  ~535–546, ~596, ~772, ~822–893).
- [Processor.cs](Core/Stateflows/Activities/Engine/Processor.cs) (~535
  `Graph.AcceptsInput`); `ActivityInitializationContext` (~64) and `ActionContext`
  (~57).
- [IEnumerableExtensions.cs](Core/Stateflows/Activities/Utils/IEnumerableExtensions.cs):
  `IsOfTokenType`, `OfTokenType`, `ToTokensOfType`, `GetEffectivePayloadType`.

### WP4 — Payload flows, pins, decisions, guards, transformations (large)

- Name-first overloads per D33 and the API report: object flows (`AddFlow`,
  `UseFlow`, `AddElseFlow`, class-based guard/transformation forms),
  input/output pins, decisions, iterative/parallel activities, overrides
  (`Use*`), data stores. Implement through the same fan-out used for typed
  members (`IObjectFlowBase` family, `NodeBuilder`, `BaseActivityBuilder`,
  `FlowBuilder<TToken>`, pin and decision builders).
- Model: `Node` and `Edge` carry keys instead of types. `Edge` gets a source key and
  a target key, mirroring `TokenType` / `TargetTokenType`, so a payload edge stores
  its original and transformed name sets (D30, R16); `Edge.Operations` records each
  transformation's `transformedNames`. `IFlowContext`, the visitor
  (`FlowConfigurationAddedAsync` and the new payload callback) and `FlowInspection`
  expose both sets. Payload edges get name-based identifiers built like the typed
  `TokenType`/`TargetTokenType` form (finding 7); pin node names (today
  `$"{Node.Name}.{InputPin<TToken>.Name}"`) get a name-based form via non-generic
  `InputPin`/`OutputPin` helpers.
- Guards: `ObjectFlowBuildAction<Payload>` / `IGuardContext<Payload>`; class-based
  guards through `AddFlow<TFlow, …>(tokenNames, …)` and builder-level
  `AddGuard<TGuard>` / `AddGuards` / `AddAndGuards` / `AddOrGuards` on a
  `Payload`-bound builder; guards declare no names (D29, R12).
- Transformations (D31): `AddTransformation<TTransformedToken>(transformedNames, delegate)`
  and `AddTransformation<TTransformedToken, TTransformation>(transformedNames)`, both
  `where TTransformedToken : Payload`, on `IObjectFlowBuilder`,
  `IObjectFlowBuilderWithWeight`, `IElseObjectFlowBuilder` and
  `IElseObjectFlowBuilderWithWeight` and their `FlowBuilder` implementation;
  `transformedNames` overloads of node-level `AddFlow<TToken, TTransformedToken, TFlow…>`
  and the class-based `AddElseFlow` forms (also on the token-scoped
  `IObjectFlowBase<TReturn, TToken>` variants); `AddFlow<TTransformedToken, TFlow…>(tokenNames, …)`
  for payload→typed and, with `transformedNames`, payload→payload. Each stores its names
  as the edge's new transformed key (R16) and checks the produced name at run time
  (D16, R7). The existing members without names reject `Payload` as transformed type.
- Validation: new diagnostics for D5 (bare `Payload`), R4 (unknown name), R5
  (collision), duplicate/empty names, mixed typed/payload misuse on the same
  declaration, flow identity uniqueness (ACT-017) for payload sets; `Payload` through a
  typed member without names (R12); the checks that match the typed ones (R13, D27)
  on original and transformed names against pin/decision/node declarations and source
  outputs in
  [Graph.cs](Core/Stateflows/Activities/Models/Graph.cs) next to ACT-026/027/028/029/056/057/058;
  runtime produced-name check per transformation.
- Engine: edge activation, pin routing and interleaving use the selector (WP3).
  Accept-event nodes with names emit payload tokens (R11). `PayloadRequest` tokens
  flow like any payload (R14).

### WP5 — Injection and context APIs (medium–large)

- Attribute `[TokenName("a", "b", …)]` (parameter target) beside the other
  parameter attributes in
  [Core/Stateflows/Common/Attributes](Core/Stateflows/Common/Attributes).
- Non-generic interfaces (D4): `IInputTokens`, `IInputToken`,
  `IOptionalInputTokens`, `IOptionalInputToken`, `IOutputTokens`,
  `IInputOutputTokens`, `IInputOutputToken`, `IOptionalInputOutputTokens`,
  `IOptionalInputOutputToken` (the last two extend `IOutputTokens`, D20). Concrete
  payload-filtering collections implementing both the generic-over-`Payload` and the
  non-generic interface.
- `StateflowsActivator.ResolveParameterValuesAsync`: a branch that creates the
  payload collection bound to the attribute's names. Non-generic interfaces
  without the attribute, and typed interfaces with the attribute, are definition
  errors (D5/R3). No open-generic DI registration is required for the non-generic
  forms; `ActivitiesDependencyInjection` / `ActionsDependencyInjection` keep the
  typed registrations.
- `Node.ScanForDeclaredTypes` reads the constructor attribute and records keys in
  `InputTokenTypes` / `OptionalInputTokenTypes` / `OutputTokenTypes` (renamed to
  key lists, internal).
- Context APIs: payload-aware `IInput` / `IOutput` members, name properties on
  `IFlowContext`, `IPinContext`, `ITransformationContext`,
  `IActivityOperationObservationContext`; `ITokensAccessor` unchanged.
- Actions: `ActionModel.ConsumedTokenTypes/ProducedTokenTypes` become keys;
  action executor and `ActionContext` use the selector; action builder surface
  mirrors the activity attribute/injection model (D9). Existing
  `ConsumedEventNames`/`AddConsumedEvent` stay.
- Also check: `OutputPinNode<TEvent>`, `AcceptAndOutputEventActionNode<TEvent>`
  and the Microsoft Agent Framework nodes that inject typed collections.

### WP6 — Transport and storage (medium)

- `ITokensInput`/`TokensInput`/`TokensOutput` additions (D12);
  `TokensInput<TToken>`/`TokensOutput<TToken>` unchanged.
- Wire format: `TokensTransferEvent.Tokens` is `List<TokenHolder>` with
  `TypeNameHandling.None`; early spike to prove `TokenHolder<Payload>` (with the
  field-envelope converter and internal constructors) round-trips through
  Newtonsoft contexts, streams, Orleans and HTTP.
- Orleans: `OrleansEventHolder`, `StateflowsBehavior` special cases for
  `TokensInputEvent` (~360, ~626, ~649, ~718–724, ~798).
- EF Core and other storages: polymorphic JSON round trip of persisted contexts
  containing payload tokens.
- MinimalAPIs: payload pin endpoints analogous to
  [ActivityPinEndpoints.cs](Extensions/Stateflows.Extensions.MinimalAPIs/Extensions/ActivityPinEndpoints.cs)
  (typed `TokensInput<TToken>` today), request decoding by `PayloadDefinition`,
  OpenAPI schema, HATEOAS links, interceptor hooks, `ActivityVisitor` /
  `ActionVisitor`. Names registered as requests get no pin endpoints (confirmed).
- Microsoft Agent Framework: verify typed behavior is untouched; payload support
  only if the owner asks.

### WP7 — Observability and inspection (medium)

- `IActivityVisitor` gains payload-aware callbacks as default interface members
  (non-breaking): pins, flows, flow configuration, guards, transformations.
  `Graph.Visiting.cs` emits them.
- Inspection models (`FlowInspection`, `NodeInspection`) expose token key/name.
- Consumers: OpenTelemetry `ActivityTracer`, PlantUml
  `ActivityPlantUmlGenerator`, MinimalAPIs visitors, Visualizer (`layout.ts`,
  `fixtures.ts`), DevHost.

### WP8 — JSON definitions for payload tokens (medium)

`activity.schema.json` and `BoundActivityDocument`: payload names (array) as an
alternative to the `tokenType` alias on pins, flows, decisions, iterative and
parallel scopes, `exactly-one-of` rule like `eventType`/`eventName` for
accept-event; flow transformations take `outputTokenNames` (set, with single-name
sugar). Bound guard/transformation contracts for payload flows
(`IFlowGuard<Payload>`, `IFlowTransformation<Payload, …>`) take their names from the
flow's `tokenNames` and the transformation's `outputTokenNames`, as in C# (D29,
D31). JSON keys follow D32 (`tokenNames`, `eventNames`, `iterationTokenNames`,
`parallelizationTokenNames`, `outputTokenNames`). README sections for tokens,
initializers and deferrals.

### WP9 — Documentation (medium)

Wiki repo `W:\Source\Stateflows-wiki\framework.wiki` (82 numbered pages, no
`_Sidebar`): `4.1.-Events` (rename the page to `4.1.-Events`,
keep typed events, replace the dynamic-event content with a brief mention linking to
`4.6.-Payloads`; D34), a **new page
`4.6.-Payloads`** (payload definition, names in transitions, initializers, deferrals,
accept-event, payload tokens; D25) with the existing `4.2`–`4.5` links untouched,
`6.3.-JSON-Definitions`, `6.4.-Minimal-APIs`,
`6.5.-OneOf`, `5.4.2.4.*` (transitions), `5.4.1.2.-In-State-Event-Deferrals`,
`5.5.1.5.-Tokens`, `5.5.2.1.6`/`.7` (pins), `5.5.2.2.*` (flows),
`5.5.2.1.14` (data store), `5.2.-Dependency-Injection`, `5.5.-Activities`,
`5.5.2.1.10` (accept event), `5.5.2.1.12` (publish event), `5.4.2.2.2`
(composite state), initializer pages, and the entity pages `5.7.-Entities` /
`5.7.2.-Mutations` (named payload initializers, mapped-type initializers and
mutations). New wiki content
for class-based handlers with call-site names (D29), the original/transformed name
sets of flows and the `transformedNames` overloads (D30, D31), and the
non-generic injected token interfaces. READMEs: Json (highest priority),
UnionTypes, Examples, Visualizer. XML docs on every new public member.
Release notes: renames, persistence break (D3), activity initializer fix.

## Test plan

The detailed plan (matrices, file placement, gating per work package, the
characterization and failing-first lists) is in
[PAYLOADS-TEST-PLAN.md](PAYLOADS-TEST-PLAN.md). Summary:

| Area | Tests |
| --- | --- |
| Rename | Existing suites (StateMachine, Activity, Action, Entity, Orleans, AIAgent, Json, ClearScript, UnionTypes) pass unchanged except for renames. |
| Parity | Matrix per behavior model: typed / OneOf / UnionTypes / one name / many names × initializer, deferral, transition, accept-event; invalid/duplicate/unregistered names; expected-events and visitor output. |
| Entities | Named payload initializers and mutations and mapped-type (OneOf / UnionTypes) initializers and mutations, through the builder, through `[Initializer]` / `[Mutation]` attributes and through mutation classes; duplicates, unregistered names, expected events, MinimalAPIs endpoints, fields and projections. |
| Class-based handlers | Initializer, deferral-guard, entity mutation and flow classes registered with call-site names (one, several); `Payload` through a typed member rejected (D29); compile-only overload resolution checks. |
| Selector refactor | Existing activity suite green; characterization tests from WP0. |
| Payload tokens | Mixed typed and payload flows in one activity; wildcard `object` receiving payloads (D13); overlapping typed/payload flows (D21); guards; else flows; decisions; input/output pins; iterative and parallel scopes; interleaving and streams; data stores; initial/final flows; payload requests as tokens (D22). |
| Transformations and name sets | `transformedNames` overloads (delegate and class) on all four flow-chain builders and on node-level `AddFlow` / `AddElseFlow` (typed and name-first); `Payload→T`, `T→Payload`, `Payload→Payload`, chains; original vs. transformed name matching; produced-name check; `Payload` without names rejected; identifiers, context, visitor and inspection report both sets; checks matching the typed ones (ACT-026…029 translated to name sets, undeclared nodes unconstrained; D16, D27, D30, D31). |
| Injection | Every non-generic interface with one and several names; missing attribute; typed interface with attribute; wrong name on output; optional variants including the `IOutputTokens` addition (D20); action nodes; Action behaviors. |
| Transport | `TokensInput.Add(Payload)` → payload pin → `TokensOutput.GetPayloadTokens`; persistence round trip (in-memory, EF, Orleans); MinimalAPIs pin endpoints; JSON-defined activities with payload tokens. |
| Observability | Visitor metadata, inspection, OpenTelemetry spans, PlantUml output, Visualizer fixtures. |
| Regression | Full solution build and all integration test projects (UnionTypes targets net11.0); grep guard for leftover `Structural`. |

## Sequencing

1. WP0 safety net.
2. WP1 rename (all later work uses final names).
3. WP2 parity (including 2g entities and 2h class-based handlers) and WP3 foundation
   can proceed in parallel after WP1; WP2 is independent of tokens.
4. WP4 and WP5 after WP3; WP5 can start once the key type exists.
5. WP6 after WP4/WP5; start the wire-format spike earlier.
6. WP7 and WP8 after WP4; WP9 continuous, finished last.

Suggested slicing: WP0, WP1, WP2a–h (one change each), WP3, WP4, WP5, WP6, WP7,
WP8, WP9. Every slice leaves the repository building with all tests green.

## Risks

| Risk | Mitigation |
| --- | --- |
| Rename is huge and cuts across every project | Mechanical, IDE-assisted, single branch, grep guard, build and tests before and after. |
| Persisted data breaks | Accepted (D3); release note; consider a version bump signal. |
| Overlapping typed/payload selection surprises | WP0 characterization tests; explicit R8/D21; documentation of overlap behavior. |
| Wire format of `TokenHolder<Payload>` differs between Newtonsoft, Orleans and HTTP | Early spike in WP6; round-trip tests in each runtime. |
| Interface fan-out (every builder variant implements `IStateMachineEvents<…>`, `IObjectFlowBase<…>`, …) | Generate repetitive members from the typed ones; compile-time completeness; reuse the deferral pattern. |
| New names (`Payload*` families) widen the API | API review (companion document) before implementation. |
| Activity initializer fix changes behavior for existing OneOf users | Failing-first test; release note. |
| Flow identifier changes affect visitors, MinimalAPIs routes and stored identifiers | Typed identifiers remain byte-for-byte identical; payload identifiers use a distinct form. |
| Action and Activity token collections share code | Fix once (WP0); keep Actions in the same packages. |
| Flow validation (D27, matching the typed checks) lets sensible-looking mismatches through because nodes and stages declare no names (same as the typed checks), or rejects name-set cases whose typed analog is ambiguous (either-direction containment) | Documented check table in the API report; unconstrained-when-undeclared rule; validation tests for pass and fail on original and transformed names. |
| Guard or transformation classes cannot be checked against the names they are registered with (D29), so a class may receive or return names it does not expect | Runtime produced-name check throws `ArgumentException`; guards see only names in the flow's set; tests for each side. |
| The new `transformedNames` overloads (D31) introduce ambiguity with existing members | Compile probe of the full overload set passed with no CS0111/CS0121; compile-only overload resolution tests guard it. |
| Entity `ITypeMapper` support (D26) changes dispatch in a type-keyed model | Failing-first tests; per-mapped-type uniqueness; WP0 typed baseline. |
| Entity dispatch is type-keyed in several places (finding 13); a missed site silently ignores payload events | Entity matrix tests per site (initialize, mutate, expected events, visitor, MinimalAPIs, inspection); WP0 typed baseline. |
| `PayloadRequest` tokens and `Respond` after persistence/reload | WP0 characterization of typed request tokens first; documented in-memory-only response routing. |
| Optional input-output interface change (D20) breaks third-party implementers | Interfaces are framework-injected; release note; WP0 tests for the corrected `PassAllOn`. |

## Open items

None. Every item in [PAYLOADS-API-CHANGES.md](PAYLOADS-API-CHANGES.md) section 8 is
confirmed (items 10 and 13 as revised: D32, D34) and section 9 records how your
answers resolved the earlier questions.

## Decisions taken during implementation

Design questions found during implementation are decided by choosing the option
closest to the typed behavior, and recorded here.

| # | Work package | Question | Decision |
| --- | --- | --- | --- |
| I1 | WP0 | `IInputOutputTokens<T>` and the optional variants are both an input `IEnumerable<T>` and an output `ICollection<T>`; LINQ (`Any`, `Count()`, `ToList`) takes the `ICollection<T>` fast path and therefore sees the **output** side. | Left unchanged (existing typed behavior); payload variants mirror it. Tests enumerate with `foreach`. Worth a separate look later. |
| I2 | WP0 | `OutputTokens<T>.Contains/Remove/CopyTo/Clear` compared envelopes by fresh `Guid`/holder type, so they never matched. | Compare payload values with `EqualityComparer<T>.Default` over tokens of type `T`; `Remove` removes the first match; `Clear` removes only tokens of `T`. |
| I3 | WP1 | MinimalAPIs `StructuralJson` and Json-extension `EventJson` both map to `PayloadJson`; `StructuralRequestPayload` would become `PayloadRequestPayload`. | Both `PayloadJson` classes are `internal` in different assemblies, so both keep the name. The request DTO is named `PayloadRequestBody`. The namespace of `CronEvent` (Scheduling) stays `Stateflows.Events` because it is a typed event, not a payload. Remaining "structural" wording refers to structured nodes or state-machine structure and is intentional. |
| I4 | WP2a | How to surface V1 (empty set, empty/whitespace entry, duplicate) and V2 (unregistered name). | V1 throws `ArgumentException` at the builder call (like `ThrowIfNullOrEmpty`); V2 throws `ArgumentException` from `GetPayload` while the graph is built, exactly as named transitions and deferrals already do. Only payload-typed generic misuse (`AddInitializer<Payload>(…)`) throws the behavior's definition exception. |
| I5 | WP2a | Which status a non-initializing event gets when only explicit initializers exist. | Unchanged engine path: `Rejected` (an unlisted payload and `Initialize` alike), same as typed initializers. Activities likewise keep their typed status for unlisted events (tests compare against an unlisted typed event). |
| I6 | WP2b | Several activity initializer declarations matching one received event name (typed + `ITypeMapper` overlap, or typed + mapped). | All matching declarations run in registration order and must all accept (like accumulated actions within one declaration); the first refusal stops. Expected events report **mapped** types (previously the declared `OneOf` type itself). |
| I7 | WP2c | How to extend an inherited payload deferral (API doc says "name sets act like the type key" but lists no override member). | Added `UseDeferredEvent(IReadOnlyList<string>, OverridenDeferralBuildAction<Payload>)` and `string` sugar on `IStateUtilsOverrides<TReturn>`; the inherited declaration is located by name-set equality (as B4 for transitions); a missing one throws the override exception. |
| I8 | WP2c | A name already deferred by the same state (single-name `AddDeferredEvent` threw a raw dictionary `ArgumentException`). | `DeferralDefinitionException` "Event 'x' is already deferred by 'state'", the same as the typed path; checked for the whole set before anything is registered. |
| I9 | WP2c | MAF `AgenticStateBuilder` wraps a builder, so default interface members that cast `this` to the internal vertex builder fail on it. | It forwards the named deferral members explicitly to the wrapped builder; later named members on wrapped interfaces get the same forwarding. |
| I10 | WP2d | A named transition needs a stable trigger key; it is used in edge names, duplicate checks and signatures. | The key is `PayloadNameSet.Key`: names sorted ordinally and joined with `\|`. The same set in another order is therefore a duplicate. JSON multi-name transitions use the same key; before, they used a JSON array string. A JSON-defined transition cannot be overridden from C# (`UseStateMachine<T>` needs a class), so T2.10's JSON-override case is not applicable. |
| I11 | WP2d | Spec 3.5 lists `AddAcceptEventAction(names, …)` and `ChangeAcceptedEvent(names)`, but the existing named override `UseAcceptEventAction(string, …)` had no list form. | Added `UseAcceptEventAction(IReadOnlyList<string>, …)` matching by set equality (same as B4 for transitions); the string form delegates to it. A mismatch keeps the existing `InvalidOperationException`. Action `AddConsumedEvent(names)` is a default interface member that registers each name. |
| I12 | WP2e | JSON diagnostics for named initializers, deferrals and activity `eventNames` were not specified. | Shape errors (empty list, duplicate or empty name, the legacy `triggers` key on initializers or deferrals) are `JSON-002` schema errors. Unknown names are `JSON-003` at `…/eventNames/<index>` (activity) or `…/events/<index>/eventName` (state machine). A named deferral of an already deferred event is `SM-035`, as for typed deferrals. Activity initializers and accept nodes keep `eventName` as sugar next to `eventNames`. |
| I13 | WP2g | Entities have no build phase that consults the payload registry, and handler lookup was keyed by CLR type. | Entity handlers are keyed by **effective event name** (CLR event name, each mapped type's event name, or each payload name). Payload names get shape validation in the builder (V1, `ArgumentException`; `InvalidOperationException` for attribute names) and registry validation in `EntitiesDependencyInjection.Build` (V2: unknown name → `InvalidOperationException` naming entity, kind and name; V3: typed and mapped entity events are registered as CLR names, so a payload with the same name is rejected). Typed members closed over `Payload` throw `InvalidOperationException`. A later builder registration replaces an earlier one per name. Duplicate checks between annotated template methods now compare effective names (so `OneOf<A, B>` and `A` conflict), and their message names the event and both methods. Visitor callbacks are emitted once per registration with the names still mapped to it; the id suffix is the type's event name or `PayloadNameSet.Key`. MinimalAPIs `RegisterPayloadEndpoint` gained an optional `authorization` argument, so `[Mutation("…")]` methods keep their accessor authorization. |
| I14 | WP2h | Spec 3.9 mentions accept-event action classes that work with the declaration's names, but the C# builder has no class form of `AddAcceptEventAction` that takes names (spec 3.5 lists only delegate forms). | No new C# member was added. `IAcceptEventActionNode<Payload>` classes are reachable through JSON (`JsonNamedEventBehaviors`); T2.18 is covered for transition guard and effect classes (`PayloadTransitions`) and for flows in WP4. T4.38a (flow and pin overload resolution) moves to WP4, where those members are introduced. |
| I15 | WP3 | How a token key relates to the existing `Type` fields on `Edge` and `Node`. | `Edge`/`Node` keep their CLR `Type` fields and gain name lists (`TokenNames`, `TargetTokenNames`, `PinTokenNames`) plus computed `TokenKey`s. A payload key's Type is `typeof(Payload)`, so typed `object` declarations accept payload tokens (R13) and unrelated typed declarations reject them. Declared node token lists became `List<TokenKey>`; flow identifiers use `PayloadNameSet.Key` for payload keys. |
| I16 | WP4a | The name-taking flow and transformation overloads are default interface methods on public builder interfaces, so they can't reach builder internals directly. | An internal thread-static scope (`PayloadFlowScope`) holds the validated names while the existing member runs. The first edge or transformation created inside the scope takes them. `AddFlow<Payload>`, `AddFlow<Payload, TGuard>`, `AddTransformation<Payload>` and similar members called without names throw `ActivityDefinitionException` (V4a). The one exception: when the source is a payload pin or payload decision, the flow inherits that node's names. |
| I17 | WP4a | A transformation can return a payload whose name isn't in the declared transformed names. | It's checked at run time. A mismatch throws `ArgumentException` inside the transformation call, so it goes through the normal exception-handling path and the token doesn't reach the target. |
| I18 | WP4a | `AddFlow<TFlow>(string names, string target)` and `AddFlow<TToken>(string target, build)` can be ambiguous with a `null` build argument. | Accepted. The name-first overloads require `TFlow : IFlow<Payload>`, so they bind only for payload flow classes. |
| I19 | WP4a | Which arity-3 overloads get string sugar. | String sugar is provided wherever there's an explicit target name. The `TTargetNode` variants and typed→payload `TTargetNode` variants take name lists only. Override `UseFlow` exists only on overridden decisions (and is tested with payload decisions in WP4b); overridden actions have no `UseFlow` for typed tokens either. A class action without declared tokens rejects named flows with ACT-027, the same as typed flows; class token declarations by name come in WP5. |
## Source landmarks

- Payload model: [Payloads](Core/Stateflows.Common/Payloads),
  [Events registration](Core/Stateflows/Events),
  [StateflowsJsonConverter.cs](Core/Stateflows.Common/Utilities/StateflowsJsonConverter.cs).
- Tokens: [TokenHolder.cs](Core/Stateflows.Common/Classes/TokenHolder.cs),
  [TokensTransfer.cs](Core/Stateflows.Common/Events/TokensTransfer.cs),
  [Collections](Core/Stateflows/Activities/Collections).
- Activity engine and model:
  [Executor.cs](Core/Stateflows/Activities/Engine/Executor.cs),
  [Processor.cs](Core/Stateflows/Activities/Engine/Processor.cs),
  [Node.cs](Core/Stateflows/Activities/Models/Node.cs),
  [Edge.cs](Core/Stateflows/Activities/Models/Edge.cs),
  [Graph.cs](Core/Stateflows/Activities/Models/Graph.cs).
- Activity builders:
  [Registration](Core/Stateflows/Activities/Registration),
  [IObjectFlowBase.cs](Core/Stateflows/Activities/Registration/Interfaces/Base/IObjectFlowBase.cs).
- State-machine event consumption:
  [StateMachineBuilder.cs](Core/Stateflows/StateMachines/Registration/Builders/StateMachineBuilder.cs),
  [StateBuilder.cs](Core/Stateflows/StateMachines/Registration/Builders/StateBuilder.cs),
  [EventExtensions.cs](Core/Stateflows/StateMachines/Extensions/EventExtensions.cs).
- Activator and attributes:
  [StateflowsActivator.cs](Core/Stateflows/Common/Classes/StateflowsActivator.cs),
  [Attributes](Core/Stateflows/Common/Attributes).
- Actions: [Actions](Core/Stateflows/Actions).
- Entities: [Entities](Core/Stateflows/Entities)
  ([Executor.cs](Core/Stateflows/Entities/Engine/Executor.cs),
  [Processor.cs](Core/Stateflows/Entities/Engine/Processor.cs),
  [EntityRegistration.cs](Core/Stateflows/Entities/Registration/EntityRegistration.cs),
  [EntityInspection.cs](Core/Stateflows/Entities/Engine/EntityInspection.cs),
  [Registration](Core/Stateflows/Entities/Registration),
  [Attributes](Core/Stateflows/Entities/Attributes)),
  [EntityVisitor.cs](Extensions/Stateflows.Extensions.MinimalAPIs/Visitors/EntityVisitor.cs).
- Flow builders and validation:
  [IFlowBuilder.cs](Core/Stateflows/Activities/Registration/Interfaces/IFlowBuilder.cs),
  [InputOutput.cs](Core/Stateflows/Activities/Collections/InputOutput.cs),
  [InputOutputInterfaces.cs](Core/Stateflows/Activities/Collections/InputOutputInterfaces.cs).
- Extensions: [Json](Extensions/Stateflows.Extensions.Json),
  [MinimalAPIs](Extensions/Stateflows.Extensions.MinimalAPIs),
  [OneOf](Extensions/Stateflows.Extensions.OneOf),
  [UnionTypes](Extensions/Stateflows.Extensions.UnionTypes),
  [MicrosoftAgentFramework](Extensions/Stateflows.Extensions.MicrosoftAgentFramework),
  [OpenTelemetry](Extensions/Stateflows.Extensions.OpenTelemetry).
- Runtimes: [Orleans](Runtimes/Stateflows.Runtimes.Orleans).
- Tests: [Tests](Tests).
