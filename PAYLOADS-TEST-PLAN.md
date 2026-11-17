# Payloads and payload tokens: test plan

This document plans test coverage for the work described in
[PAYLOADS-PLAN.md](PAYLOADS-PLAN.md) (scope, work packages WP0–WP9, decisions
D1–D34, rules R1–R16) and [PAYLOADS-API-CHANGES.md](PAYLOADS-API-CHANGES.md)
(proposed API, validation rules V1–V12, breaking changes B1–B13). It is a plan only;
no test has been written.

## Contents

1. [Principles](#1-principles)
2. [Where tests live](#2-where-tests-live)
3. [Shared test infrastructure](#3-shared-test-infrastructure)
4. [WP0 — Safety net](#4-wp0--safety-net)
5. [WP1 — Rename](#5-wp1--rename)
6. [WP2 — Event-consumption parity](#6-wp2--event-consumption-parity)
7. [WP2g — Entities](#7-wp2g--entities)
8. [WP3 — Token key and selector foundation](#8-wp3--token-key-and-selector-foundation)
9. [WP4 — Payload flows, guards, transformations, pipeline](#9-wp4--payload-flows-guards-transformations-pipeline)
10. [WP5 — Injection and context APIs](#10-wp5--injection-and-context-apis)
11. [WP6 — Transport and storage](#11-wp6--transport-and-storage)
12. [WP7 — Observability and inspection](#12-wp7--observability-and-inspection)
13. [WP8 — JSON definitions](#13-wp8--json-definitions)
14. [Cross-cutting scenarios](#14-cross-cutting-scenarios)
15. [Gating and traceability](#15-gating-and-traceability)
16. [Out of scope and known gaps](#16-out-of-scope-and-known-gaps)

---

## 1. Principles

- **Three kinds of test, labelled in the test name or a category.**
  - *Characterization*: pins down today's behavior before it is refactored. Written
    against the current code, must pass unchanged after the refactor.
  - *Failing-first*: reproduces a suspected defect; fails on today's code, passes after
    the fix.
  - *Feature*: new behavior, written together with the work package that adds it.
- **Definition errors versus run-time errors.** A definition error is asserted at
  behavior build/registration time, using the exception the behavior model already
  uses (`ActivityDefinitionException`, `StateMachineDefinitionException`,
  `ActionDefinitionException`; entity registration currently throws
  `InvalidOperationException`, to be reconfirmed in WP2g). A run-time error
  (`ArgumentException` for a produced or added name outside the declared set) is
  asserted from the executing behavior. Every validation rule gets a *rejects* test
  and at least one *accepts* test next to it.
- **Both directions.** Each capability is tested positive (works), negative
  (rejected), and for isolation (typed behavior in the same behavior is unaffected).
- **Typed behavior byte-for-byte.** Typed flow identifiers, pin names, visitor output
  and inspection output are asserted by golden values in WP0 and must not change
  (PAYLOADS-PLAN risk "Flow identifier changes").
- **No new test framework.** MSTest, the existing helpers, the existing
  `Assert.Throws` style of `DefinitionRules.cs`, the existing host builders.
- **Each work package ends green.** Gating is described in
  [section 15](#15-gating-and-traceability).

## 2. Where tests live

The solution projects (all in `Tests`) and where new tests go. Existing activity
and state-machine tests are flat under `Tests\`; new feature tests go into a
`Payloads` subfolder, following the precedent of `Tests\Json`.

| Project | Target | New folder | Covers |
| --- | --- | --- | --- |
| `StateMachine.IntegrationTests` | net10.0 | `Tests\Payloads` | state-machine initializers, deferral, transitions, payload events in transitions, initializers and deferrals |
| `Activity.IntegrationTests` | net10.0 | `Tests\Payloads` | activity initializers, accept-event, payload tokens, flows, pins, transformations, injection, pipeline validation |
| `Action.IntegrationTests` | net10.0 | `Tests\Payloads` | Action consumed events, injected payload tokens in Actions |
| `Entity.IntegrationTests` | net10.0 | `Tests\Payloads` | entity initializers/mutations by payload name and by mapped type (D26), mutation classes registered with names (D26), MinimalAPIs entity endpoints |
| `UnionTypes.IntegrationTests` | net11.0 | root (flat) | parity of `ITypeMapper` types with payload names on the new surfaces, including entities |
| `Orleans.IntegrationTests` | net10.0 | root (flat) | token and payload persistence/transport through Orleans |
| `AIAgent.IntegrationTests` | — | root | rename only; typed behavior untouched |
| `Beehaviors`, `OrderFulfillment`, `Streaming*` | — | — | regression only |

JSON tests: `StateMachine.IntegrationTests\Tests\Json` and
`Activity.IntegrationTests\Tests\Json` (plus `ClearScript` for script-defined events).
MinimalAPIs tests spin up a `WebApplication` on a free port, as
`Entity.IntegrationTests\Tests\StructuralMinimalApis.cs` does today.

The nested legacy project `Tests\StateMachine\StateMachine.IntegrationTests`
(net6.0, not part of `Stateflows.sln`) is **not** a target: it is neither run nor
updated by this work. Flagged in
[section 16](#16-out-of-scope-and-known-gaps) because the WP1 grep guard must not
count it.

## 3. Shared test infrastructure

Introduce once, in WP0/WP1, so later packages only add scenarios.

| Helper | Purpose |
| --- | --- |
| Payload registration fixture | One method that registers a standard set of payloads (`order.placed`, `order.shipped`, `order.cancelled`, `legacy.order`, a request `order.status` with a response, a payload with required and optional fields) on `AddPayloads`, plus a factory accessor to create instances. Reused by every project. |
| Payload and typed twin types | CLR twins of the payloads (`OrderPlaced`, `OrderShipped`, …) so the same scenario can be asserted for typed and payload variants side by side. |
| `OneOf` / union fixtures | `OneOf<A, B>` and a UnionTypes union for the parity matrix (already present in `UnionTypeMapperTests.cs` and the OneOf tests; promoted to shared). |
| Class-handler fixtures | Flow guard and transformation classes (`IFlowGuard<Payload>`, `IFlowTransformation<Payload, T>`, `IFlowTransformation<T, Payload>`, `IFlowTransformation<Payload, Payload>`, one class implementing guard and transformation), plus a transformation that deliberately returns an unexpected name. Event-side classes: initializer (`IInitializer<Payload>`, state machine and activity), deferral guard (`IDeferralGuard<Payload>`), entity mutation (`IMutation<TTemplate, Payload>`). No attributes: names always come from the registration (D29). |
| Token-capture node | A node/Action that records every injected token (typed and payload, input and output) with name, so assertions read recorded data instead of scattering counters. |
| `MutatedActivity` builder | Reuse `BuildMutatedActivity` from `DefinitionRules.cs` for definition-rule tests; extend with a state-machine and an entity equivalent. |
| Golden outputs | Stored expected strings for typed flow identifiers, pin names, visitor event sequence, inspection JSON, PlantUml and OpenTelemetry attribute sets for a fixed typed activity (WP0). |
| Free-port host | Reuse the `WebApplication`-on-free-port helper of the MinimalAPIs tests. |

## 4. WP0 — Safety net

Written before any refactor. All characterization tests pass on today's code.

| ID | Kind | Scenario | Notes |
| --- | --- | --- | --- |
| T0.1 | Failing-first | `OutputTokens<T>.Clear` actually removes the tokens it holds (`token is TToken` against a `TokenHolder` never matches today). | Finding 11. |
| T0.2 | Failing-first | `OutputTokens<T>.Contains` / `Remove` / `CopyTo` operate on the tokens actually added (a fresh `Guid` holder never matches today). | Same. |
| T0.3 | Failing-first | `OptionalInputOutputTokens<T>` / `OptionalInputOutputToken<T>` implement the `IOptionalInputOutput*` interfaces (today they implement `IOptionalInput*`). | Compile + resolve-from-DI test. |
| T0.4 | Failing-first | `OptionalInputOutputTokens.PassAllOn` writes to the node's real output (today a throwaway collection); `PassOn` of a single optional token. | D20, B6. |
| T0.5 | Feature | `IOptionalInputOutputTokens<T>` / `IOptionalInputOutputToken<T>` expose the `IOutputTokens<T>` members: `Add`, `AddRange`, `Clear`, `Contains`, `Remove`, `Count`; tokens added end up on the output pin. | D20, B8. Optional sibling in Actions as well. |
| T0.6 | Characterization | Same checks as T0.1–T0.5 for the **Action** copies of the collections (if separate code). | Finding 11, "check Actions". |
| T0.7 | Characterization | Overlapping typed flows and else flows: a token matching two typed flows on one source (assignable types, `object`, base class); which flows fire; ordering; weights; else-flow semantics. | R8, D21. |
| T0.8 | Characterization | Wildcard `object` pins, flows, decisions and injection receive every token type. | D13. |
| T0.9 | Characterization | Class-based guards (`IFlowGuard<T>`), transformations (`IFlowTransformation<T, U>`), a class that is both; delegate-then-class and class-then-delegate pipelines; chained transformations: `TokenType` / `TargetTokenType` and the flow identifier after one and after several transformations. | Baseline for D27, D30. |
| T0.10 | Characterization | Iterative and parallel partitioning by token type (`OfTokenType`), token routing across structured scopes, interleaving/streaming admission (`StreamingAdmission.cs`, `Interleaving.cs`). | WP3 baseline. |
| T0.11 | Characterization | A typed `IRequest<TResponse>` event sent to an activity and flown down the network as a token: delivery to a node, `Respond` while the instance is loaded, behavior after the context is persisted and reloaded (documents whether the response can still be routed). | D22. |
| T0.12 | Failing-first | Activity `AddInitializer<OneOf<A, B>>`: sending an `A` and a `B` both initialize the activity (today the builder keys the initializer by the mapper type's name). Same for a UnionTypes union and `Initializer` expected-event reporting. | Finding 4, B3. |
| T0.13 | Characterization | State-machine `AddInitializer<OneOf<A, B>>` and activity/state-machine/Action transition + accept-event + deferral with `OneOf` / union (today's reference behavior for the parity matrix). | WP2 baseline. |
| T0.14 | Characterization | Persisted-context round trip of an activity with in-flight typed tokens (in-memory and EF where available), plus stored token queues for streams. | WP6 baseline; D3 means only forward-compat of the *new* format is tested later. |
| T0.15 | Characterization | Entity typed initializers and mutations: initialize, mutate, `NoImplicitInitialization`, expected events, visitor `eventNames`, MinimalAPIs endpoints and authorization from `[Mutation]` methods; also the **current** behavior of a `OneOf<A, B>` entity initializer/mutation argument (expected: keyed by the mapper type, not matched by an `A`; the failing-first target of T2g.18). | WP2g baseline (existing `Initialization.cs`, `Mutation.cs`, `MinimalApis*.cs` already cover much; list gaps only). |
| T0.16 | Characterization | Golden values for typed flow identifiers, pin node names, visitor callback order (`VisitorOrdering.cs`), inspection output, PlantUml and OpenTelemetry attributes. | Risk: identifier changes. |
| T0.17 | Characterization | Typed pipeline validation: the ACT-026…029 accept/reject cases (transitive node coverage, either-direction assignability to declared pins, required-input satisfiability, undeclared delegate/context-query-only nodes unconstrained, stage typing at compile time). | Baseline for D27; `DefinitionRules.cs` already holds part of it, list gaps only. |
| T0.18 | Characterization | Overload resolution of the existing registration shapes that the new overloads sit beside: `AddDeferredEvent<TEvent>(buildAction)`, `AddDeferredEvent<TEvent, TGuard>(…)`, `AddInitializer<TEvent>(…)`, `AddFlow<TControlFlow>(string…)`, `AddFlow<TToken, TFlow>(…)`, `AddFlow<TToken, TTransformed, TFlow>(string)`, builder `AddTransformation<T>(delegate)` / `AddTransformation<T, TTransformation>()` still compile and resolve as before (compile-only test project or Roslyn check). | D29, D31; guards against CS0111/CS0121 regressions. |

## 5. WP1 — Rename

No new behavior. Gate: suites unchanged except renames.

| ID | Check |
| --- | --- |
| T1.1 | The whole solution builds; every integration project passes with only renames applied to tests. |
| T1.2 | Grep guard (a script run in CI or a test over source text): no `Structural` identifier remains outside a documented allow-list (the allow-list lives in this repository next to the guard; the nested legacy test project is excluded). Also no `Stateflows.Events` namespace and no `event.schema.json`. |
| T1.3 | Public-API check: the renamed surface matches [PAYLOADS-API-CHANGES.md](PAYLOADS-API-CHANGES.md) section 1 (reflection test listing the public types in `Stateflows.Payloads` / `Stateflows.Common` and asserting the old names are absent). |
| T1.4 | `AddPayloads`, `IPayloadsBuilder`, `AddRequest` / `AddRequest<TResponse>` registration, collision rule (V3: payload name equals CLR event name) and `PayloadFactory` behaviors are exercised by the renamed copies of `StructuralEvents.cs` and `StructuralScheduling.cs`. |
| T1.5 | Persistence contexts written by the renamed types round-trip (new format); loading a pre-rename fixture is **not** tested (D3), but the release-note statement is checked in WP9. |
| T1.6 | JSON schema: `payload.schema.json` validates the renamed sample definitions; old schema name is gone. |
| T1.7 | MinimalAPIs: renamed endpoint hooks (`Before/AfterPayloadEventEndpointDefinition` or the final names in the API report), route shapes and OpenAPI document unchanged except for names. |

## 6. WP2 — Event-consumption parity

Initializers and deferral get the semantics transitions and accept-event actions
already have: a CLR type, a mapped multi-type (`OneOf`/UnionTypes), one payload
name, or a set of payload names.

### 6.1 Matrix

Each cell is one test class section with the scenarios listed in 6.2. `✔` = required,
`n/a` = not offered (V9: a declaration is either CLR-typed or name-based; mixing is
out of scope, D6).

| Consumption point | Typed | `OneOf` | UnionTypes | One name | Many names |
| --- | --- | --- | --- | --- | --- |
| State-machine initializer | ✔ (exists) | ✔ (exists, T0.13) | ✔ | ✔ | ✔ |
| Activity initializer | ✔ (exists) | ✔ (T0.12, fix) | ✔ | ✔ | ✔ |
| State deferral | ✔ (exists) | ✔ (exists) | ✔ | ✔ (exists, single) | ✔ |
| State deferral with guard class | ✔ | ✔ | ✔ | ✔ (`IDeferralGuard<Payload>`) | ✔ |
| Transition (`AddTransition`, `Use*Transition`, `ChangeTrigger`) | ✔ | ✔ | ✔ | ✔ (exists) | ✔ (new; was JSON-only) |
| Accept-event action | ✔ | ✔ | ✔ | ✔ (exists) | ✔ (new) |
| Action consumed events | ✔ | n/a (no mapper today) | n/a | ✔ (exists) | ✔ |
| Entity initializer / mutation | ✔ | ✔ (D26, section 7) | ✔ (D26, section 7) | ✔ | ✔ (section 7) |

Behavior models: **State machine** (including overridden/initialized/finalized builder
variants and composite/orthogonal states), **Activity**, **Action**, and **Entity**.

### 6.2 Scenarios per cell

| ID | Scenario |
| --- | --- |
| T2.1 | The declaration works: the matching event initializes / is deferred / fires the transition / is accepted; the payload carries its fields. |
| T2.2 | Non-matching events get the same `EventStatus` as for typed declarations (`NotInitialized`, `Rejected`, `NotConsumed`, `Deferred` as exist today). |
| T2.3 | Many names: each name works; a name outside the set does not. |
| T2.4 | `OneOf` and union: every member type works (and is the failing-first test in activities). |
| T2.5 | Validation V1–V3: empty set, empty/whitespace entry, duplicate name, unregistered name, name equal to a CLR event name all rejected at definition time with the behavior's definition exception. |
| T2.6 | Validation V9/V10: typed and name-based forms are not mixable in one declaration; duplicate initializers for the same name are handled like duplicate typed initializers (state-machine: today's duplicate rule; activity: today's multiple-initializer accumulation). |
| T2.7 | Multi-initializer accumulation: several initializers (typed + named, different names) on one behavior; `GetExpectedEvents` / `GetExpectedEventNames` returns all of them (names and mapped types); `Processor.CanAdmitStream`-style admission accepts every one. |
| T2.8 | `NoImplicitInitialization` equivalent: for payload initializers only the header applies (named test). |
| T2.9 | Deferral: deferred payload is queued, re-delivered after the state change, ordering with other deferred events; deferral inheritance through composite states (`Vertex.PrepareDeferrals`), orthogonal regions, and override builders; deferral guard class `IDeferralGuard<Payload>` receives the payload; guard false releases the event. |
| T2.10 | Transition names: set matching; `UseTransition` / `Use*Transition` / `ChangeTrigger` with names replaces by name **set equality** (B4); a JSON-defined multi-name transition overridden from C# (the only case affected by B4); `AddTransition` with `OneOf` plus names in separate transitions on one source. |
| T2.11 | Accept-event action with names: a received payload produces a payload token named after the received payload (R11) that can feed an `AddFlow` (verified fully in section 9). `ChangeAcceptedEvent` with names. |
| T2.12 | Expected-events reporting and visitor metadata: `InitializerAddedAsync<Payload>` and deferral callbacks carry `eventNames`; typed callbacks unchanged. |
| T2.13 | Observability: `IStateMachineVisitor` / `IActivityVisitor` emit the new declarations in the same order guarantees as typed (`VisitorOrdering.cs`, `CanonicalVisitors.cs`, `VisitorMetadata.cs`). |
| T2.14 | PlantUml: a named transition and a deferral with names render the name set; typed rendering unchanged. |
| T2.15 | Initializer on `ITypeMapper` + names registered **separately** on one behavior: both reachable (they are separate declarations). |
| T2.16 | Class-based event handlers (D29), for the state-machine and activity initializer classes and the deferral-guard class: registered with one name (sugar) and several names; each name invokes the class; an unlisted name does not; registering the `Payload`-closed class through a typed member without names (`AddInitializer<Payload, TInit>()`, `AddDeferredEvent<Payload, TGuard>()`) rejected with the behavior's definition exception (`StateMachineDefinitionException` / `DeferralDefinitionException` / `ActivityDefinitionException`). |
| T2.17 | Overload resolution (D29, compile-only): `AddInitializer<TInit>(names)`, `AddInitializer<TInit>("n")`, `AddDeferredEvent<TGuard>(names, …)`, `AddDeferredEvent<TGuard>("n", …)` resolve to the new members; existing typed calls (T0.18) resolve unchanged. |
| T2.18 | Classes inside builders that already carry names (transition guard/effect, accept-event classes) receive payloads with the declaration's names and need no extra registration. |

### 6.3 Specific regression guards

- Activity `AddInitializer<OneOf<A, B>>`: documented behavior change (B3) has its own
  test class `ActivityInitializerTypeMapper`, with both the failing-first and the
  "typed single type unchanged" cases.
- The deferral-by-name test in `StructuralEvents.cs` / `VisitorMetadata.cs` keeps
  passing after rename without edits other than names.

## 7. WP2g — Entities

Entity tests go to `Entity.IntegrationTests\Tests\Payloads`.

### 7.1 Builder registration

| ID | Scenario |
| --- | --- |
| T2g.1 | `AddInitializer(names, action)` initializes an entity from a payload with one name and with several names; `InitializationEvent` is the `Payload` with its fields. |
| T2g.2 | `AddMutation(names, action)` mutates an initialized entity; `MutationEvent` is the `Payload`; result and exceptions flow as for typed mutations. |
| T2g.3 | `AddMutation<TMutation>(names)` and `AddMutation<TMutation>("n")` with a class implementing `IMutation<TTemplate, Payload>`: each listed name invokes it, others do not; `AddMutation<Payload, TMutation>()` (no names) → `InvalidOperationException` (D29). |
| T2g.4 | Handler lookup is by `eventHolder.Name`, not by the static `typeof`: two different payload names with two different handlers in one entity; typed and payload handlers in one entity. |
| T2g.5 | One handler per name; registering the same name again replaces the earlier handler (D19), as typed handlers do. |
| T2g.6 | Unregistered name (V2), empty set, duplicate entries, name equal to a CLR event name: rejected at registration. |
| T2g.7 | Payload not matching any handler: same outcome as an unknown typed event (not consumed / not initialized). |

### 7.2 Attributes

| ID | Scenario |
| --- | --- |
| T2g.8 | `[Initializer("a", "b")]` and `[Mutation("a", "b")]` on a template method taking `Payload`: registered under every name. |
| T2g.9 | V12: a method with a `Payload` parameter and no names; names on a method with a non-`Payload` parameter; two annotated methods claiming one name: all rejected. (Mapped-type parameters are covered by T2g.19.) |
| T2g.10 | Existing typed `[Mutation]` / `[Initializer]` methods and `[DefaultInitializer]` unchanged. |
| T2g.11 | Authorization on a `[Mutation]` method with names (used by MinimalAPIs, T2g.16). |

### 7.3 Engine, reporting and interplay

| ID | Scenario |
| --- | --- |
| T2g.12 | `NoImplicitInitialization`: payload initializers only honor the header; a typed event with the attribute is unchanged. |
| T2g.13 | `GetExpectedEventNames`, `EntityRegistration.VisitAsync` and visitor callbacks list the payload names (alongside typed names). |
| T2g.14 | `EntityInspection.MutationAsync` / observation contexts carry the payload mutation; OpenTelemetry entity spans (see `EntityObservability` and Orleans `EntityTracingTests`) include the payload name. |
| T2g.15 | Interaction with fields and projections: payload mutation changes fields; projections reflect it; `FieldAccess`/`FieldSerialization` scenarios with payload-driven changes; transactional operations (`TransactionalEntityOperations.cs`) roll back on failure with payload mutations. |
| T2g.16 | MinimalAPIs: one endpoint per declared payload name for initializers (guarded by `[NotInitialized]`) and mutations (guarded by `[Initialized]` and the `[Mutation]` authorization), request/response shape, status codes, OpenAPI document. Typed endpoints unchanged. |
| T2g.17 | Orleans: the same entity scenarios through the Orleans runtime (`EntityTransactionTests`, `EntityObservabilityTests` style) for one named mutation. |
| T2g.18 | **`ITypeMapper` in entities (D26), builder forms:** `AddInitializer<OneOf<A, B>>` and `AddMutation<OneOf<A, B>>` (and the UnionTypes equivalent, in `UnionTypes.IntegrationTests`): an `A` and a `B` both initialize/mutate; the handler receives the converted value (`ImplicitConverter.TryConvert`); an unrelated type does not match; expected events list every mapped type. Failing-first against T0.15's baseline. |
| T2g.19 | **`ITypeMapper`, template attributes:** `[Initializer]` / `[Mutation]` methods taking `OneOf<A, B>` / a union register under every mapped type; typed and mapped handlers coexist; two annotated methods whose mapped types overlap (or a mapped member equal to a typed handler's type) follow the typed rule (annotated duplicates rejected, a builder registration replaces an earlier one). |
| T2g.20 | **`ITypeMapper`, classes:** `IMutation<TTemplate, OneOf<A, B>>` and `IInitializer<OneOf<A, B>>` (no names needed; typed semantics) registered through `AddMutation<TMutation>()` / `AddInitializer<TInitializer>()`. |
| T2g.21 | **`ITypeMapper`, visitors and endpoints:** visitor callbacks keep the declared type argument and report every mapped event name; MinimalAPIs creates one endpoint per mapped type with the authorization of the `[Mutation]` method; `EntityInspection.MutationAsync` and OpenTelemetry carry the concrete event. |
| T2g.22 | Mapped types and payload names in the same entity: `OneOf` mutation plus a named mutation; a payload name equal to a CLR event name remains rejected (V3). |

## 8. WP3 — Token key and selector foundation

Behavior-preserving; no new feature tests beyond the unit tests of the new internal
types, because the existing suite and the WP0 characterization tests are the oracle.

| ID | Check |
| --- | --- |
| T3.1 | Full existing activity, action and UnionTypes suites pass unchanged. |
| T3.2 | Unit tests of the token selector: typed key by assignability (exact, base, interface, `object`), payload key by name set, mixed collection filtering, null/empty sets, duplicate names, case sensitivity (ordinal), a payload request instance. Through public APIs only where the selector is internal, via the behaviors that use it. |
| T3.3 | Key identifier strings: typed key identifier equals today's `GetTokenName()` output for a large set of types (golden); payload key identifier is sorted, ordinal, unambiguous (`["b","a"]` and `["a","b"]` equal; `["a,b"]` differs from `["a","b"]`), and never collides with a typed identifier. |
| T3.4 | `TokenHolder.Name` for a payload-backed holder returns the payload name; for typed holders unchanged. |
| T3.5 | Every migrated site keeps behavior: pin admission, edge activation, missing required input, handle-edge filtering, partitioning, graph validation messages (golden for ACT-017/026/027/028/029/056/057/058 messages on typed activities). |

## 9. WP4 — Payload flows, guards, transformations, pipeline

Placed in `Activity.IntegrationTests\Tests\Payloads` (and the Action project where
noted).

### 9.1 Payload flows, pins and structure

| ID | Scenario |
| --- | --- |
| T4.1 | `AddFlow(names, target)` and the generic target forms; single-name sugar; many names; a token with a name outside the set does not flow. |
| T4.2 | `UseFlow`, `AddElseFlow`: override replaces a flow; else flow fires when no guard-passing payload flow does; weights. |
| T4.3 | Payload input/output pins (`AddInputPin`, `AddOutputPin`) at activity and node level; pin naming; pin admission by name set; typed and payload pins on one node. |
| T4.4 | Decision nodes with payload branches: name-set matching, else branch, typed and payload branches on one decision. |
| T4.5 | Iterative and parallel scopes with payload names: partitioning by name set (`OfTokenType` replacement), ordering, empty input, exception path. |
| T4.6 | Interleaving and streaming (`Interleaving.cs`, `StreamingAdmission.cs`, `StreamingRouting.cs` equivalents): payload tokens partition and admit like typed. |
| T4.7 | Data stores: payload tokens written through `AddFlow` from a data-store builder; read back; typed and payload in one store. |
| T4.8 | Initial/final: payload flows from an initial node and into a final node; activity result tokens; exception tokens unchanged (R10). |
| T4.9 | Accept-event node with names emits a payload token (R11); that token passes through a payload flow to a payload output pin and out via `TokensOutput`. |
| T4.10 | Guards on payload flows: delegate guards, `IGuardContext<Payload>` members (token payload name, fields), several guards (`AddGuards`, `AddAndGuards`, `AddOrGuards` 2–5 guards). |

### 9.2 Typed and payload coexistence (D13, D21, R6)

| ID | Scenario |
| --- | --- |
| T4.11 | One activity with typed and payload flows through different nodes; neither set receives the other's tokens (disjoint selection). |
| T4.12 | One node declaring typed and payload inputs and outputs together. |
| T4.13 | Wildcard consumers: typed `object` flow/pin/injection receives payload tokens (D13, B5); a base-class or interface consumer implemented by `Payload` (if applicable) behaves likewise. |
| T4.14 | Overlap: a typed `object` flow and a payload flow on the same source both match a payload token; behavior equals WP0 characterization of overlapping typed flows (D21): no validation error, same ordering/else semantics. |
| T4.15 | Bridged flows: typed→payload→typed round trip across several nodes. |
| T4.16 | No mixing (D6): a single payload declaration taking both a CLR type and names is not available (compile-time or rejected), asserted for flows, pins and decisions. |

### 9.3 Payload requests as tokens (D22, R14)

| ID | Scenario |
| --- | --- |
| T4.17 | A request payload (registered with `AddRequest`) sent to an activity enters the network as a token and flows by name. |
| T4.18 | `Respond` from a node holding the request token returns the response to the original sender while the instance is loaded (as characterized in T0.11 for typed). |
| T4.19 | Request token through a persisted context: the observed outcome equals the typed characterization (documented in-memory-only response routing). |
| T4.20 | A request name used on a payload pin gets no MinimalAPIs pin endpoint (T6.9) but still works over the behavior API. |

### 9.4 Transformations producing payloads (D16, D31, R7)

Every row runs on all four flow-chain builders (`IObjectFlowBuilder`,
`IObjectFlowBuilderWithWeight`, `IElseObjectFlowBuilder`,
`IElseObjectFlowBuilderWithWeight`) unless it names a node-level member.

| ID | Scenario |
| --- | --- |
| T4.21 | Delegate `AddTransformation<Payload>(transformedNames, delegate)` and the `string` sugar: typed→payload and payload→payload; payload→typed through the existing `AddTransformation<TTransformed>(delegate)` on a `Payload` builder. With inference, `AddTransformation("n", async c => payload)` binds to the payload overload. |
| T4.22 | A `transformedNames` set: the transformation returns any member of the set; each reaches the target. |
| T4.23 | Run-time produced-name check (D16): a delegate or class returning a `Payload` named outside `transformedNames` → `ArgumentException`; returning `null`; returning a payload with the right name but missing required fields (payload-factory validation). |
| T4.24 | `Payload` as transformed type without names: `AddTransformation<Payload>(delegate)`, `AddTransformation<Payload, TTransformation>()`, `AddFlow<TToken, Payload, TFlow>(target)`, the `TTargetNode` variant and the class-based `AddElseFlow` form → definition error (V4, V4a). |
| T4.25 | `transformedNames` empty, with duplicates, unregistered, or equal to a CLR type name: rejected (V1–V3, V8). |
| T4.26 | `PayloadRequest` subclass as `TTransformedToken` (allowed by the `Payload` constraint; D22): produced request flows as a token. |
| T4.27 | Class `AddTransformation<Payload, TTransformation>(transformedNames)` with `IFlowTransformation<TTyped, Payload>` and `IFlowTransformation<Payload, Payload>`. |
| T4.28 | Node-level class forms: `AddFlow<TToken, Payload, TFlow>(transformedNames, target)` and `AddFlow<TToken, Payload, TFlow, TTargetNode>(transformedNames)`, their token-scoped `IObjectFlowBase<TReturn, TToken>` twins, and the class-based `AddElseFlow<Payload, TFlow…>(transformedNames, …)` forms. |
| T4.29 | `AddFlow<TTyped, TFlow>(names, target)` / `<…, TTargetNode>(names)` (payload→typed) and `AddFlow<Payload, TFlow>(names, transformedNames, target)` / `<…, TTargetNode>(names, transformedNames)` (payload→payload). |
| T4.30 | Chains: typed→payload→typed, payload→payload→payload, payload→typed→payload, mixing delegate and class transformations; guards between transformations see the names in effect at their position. |

### 9.5 Class-based guards and handlers with call-site names (D29, R12, V4a)

Each case runs against the class-handler fixtures in
[section 3](#3-shared-test-infrastructure).

| ID | Scenario |
| --- | --- |
| T4.31 | `IFlowGuard<Payload>` through `AddFlow<TFlow>(names, target)`, `AddFlow<TFlow, TTargetNode>(names)` and their `string` sugar: invoked for every listed name; tokens with other names do not take the flow. |
| T4.32 | `IFlowGuard<Payload>` through the typed `AddFlow<Payload, TFlow>(target)` (no names) → definition error (V4a). |
| T4.33 | A class implementing both `IFlowGuard<Payload>` and `IFlowTransformation<Payload, X>`: both roles verified when registered with names. |
| T4.34 | Builder-level guards on a `Payload` builder: `AddGuard<TGuard>()`, `AddGuards`, `AddAndGuards`, `AddOrGuards` with `IFlowGuard<Payload>` classes, mixed with delegate guards, before and after a transformation. |
| T4.35 | The same guard class registered on two flows with different name sets: each flow's names are applied independently. |
| T4.36 | Call-site names empty, whitespace, duplicated or unregistered: rejected at definition (V1–V3). |
| T4.37 | Token-scoped builder of a payload pin/decision: class guard and transformation forms use the bound names with no call-site names. |
| T4.38 | DI: class-based stages are created per execution through the activator with injected services (existing `IFlow` class tests in `Guard.cs`, `Transformation.cs`, `Typed.cs`). |
| T4.38a | Overload resolution (D31, D33, compile-only): every name-first overload of `AddFlow`, `UseFlow`, `AddElseFlow`, `AddInputPin`, `UseInputPin`, `AddOutputPin`, `AddDecision`, `UseDecision`, `Add/UseIterativeActivity` and `Add/UseParallelActivity` with the list form and, where offered, the `string` sugar; existing typed calls (`AddFlow<TToken>("target", …)`, `AddFlow<TToken, TFlow>("target")`, `AddIterativeActivity<TToken>("node", …)`, …) still bind to the typed members; the list-only members of D33 have no single-name form (a reflection test asserts their absence); the new `transformedNames` overloads of `AddFlow`, `AddElseFlow` and `AddTransformation` do not change the binding of existing calls (T0.18). |

### 9.6 Validation of original and transformed names (D27, D30, R13, R16, V11)

A flow has original names (role of `TokenType`) and transformed names (role of
`TargetTokenType`). The checks mirror the typed ones (ACT-026…029) with sets in place
of types, as tabulated in [PAYLOADS-API-CHANGES.md](PAYLOADS-API-CHANGES.md) section
4.4.4. Every row has an *accept* and a *reject* test.

| ID | Scenario |
| --- | --- |
| T4.39 | Name sets per flow kind (D30, R16): typed flow → none/none; typed→payload → none/`transformedNames`; payload without transformation → original/original; payload→payload → original/`transformedNames`; payload→typed → original/none; a chain → the last transformation's set. Asserted through `IFlowContext`, inspection and the visitor. |
| T4.40 | Original names are what selects source tokens: a payload→payload flow takes only tokens named in its original names, and delivers only names in its transformed names. |
| T4.41 | Transformed names *F* vs. the target's declared names *D* accepted when *F ⊆ D* **or** *D ⊆ F* (ACT-027 analog): payload input pin, payload decision, node with `[TokenName]` inputs; disjoint or partially-overlapping-but-neither-contains rejected; undeclared target unconstrained. A typed→payload flow into a payload pin is checked on its transformed names; a payload→typed flow into a typed pin on its typed target type. |
| T4.42 | Original names *O* vs. source's declared outputs *D* (ACT-028 analog), the same either-direction rule: source declaring names, source undeclared (unconstrained), source with typed outputs only (payload flow rejected unless the typed output is `object`/assignable from `Payload`). |
| T4.43 | Required input (ACT-029 analog): a required `[TokenName("a","b")]` input is satisfied by an incoming flow whose transformed names are `["a"]` or `["a","b"]`; `["a","c"]` or `["c"]` does not satisfy it (rejected when no other flow does); a typed→payload flow with `transformedNames ["a"]` satisfies it. |
| T4.43a | Transitive nodes (ACT-026 analog): join, merge, fork, decision and data store with incoming transformed `["a"]` and outgoing original `["a"]` accepted; outgoing `["b"]` with no incoming flow delivering `b` rejected; several incoming flows jointly covering the outgoing names accepted; typed and payload flows mixed on one fork/merge. |
| T4.43b | Undeclared nodes: delegate actions and class nodes that use only context queries are unconstrained on both sides, as typed ones are today; pins, decisions and data stores are always checked. |
| T4.43c | Typed/payload meeting point: a typed `object` (or assignable-from-`Payload`) declaration accepts payload flows; an unrelated typed declaration (e.g. `string`) rejects a payload flow exactly as an unrelated typed flow is rejected today. |
| T4.44 | Stages declare no names: no definition-time check between a guard/transformation class and the flow's names (a mismatch shows up only through T4.23 at run time). |
| T4.45 | Flows of typed stages only: no payload-specific rule applied; typed validation messages unchanged (T0.17 baseline). |
| T4.46 | Messages: each rejected configuration names the flow, which name set (original or transformed) failed, the offending names and the rule (asserted on key substrings). |
### 9.7 Flow identity and diagnostics

| ID | Scenario |
| --- | --- |
| T4.47 | V7: the flow identifier is built from original and transformed name sets like the typed source/target form; two payload flows on one source with the same name sets and target rejected (ACT-017 behavior); same names on different targets accepted; same name set in a different order is the same set. |
| T4.48 | Typed and payload flows to one target with overlapping semantic content are **not** a duplicate (D21). |
| T4.49 | V3 extended to token names (section 8 default): a payload name equal to a CLR token type name known to the host is rejected. |
| T4.50 | V4: bare `Payload`/`PayloadRequest`/`PayloadValue` in `AddFlow<Payload>`, `AddInputPin<Payload>`, `AddDecision<Payload>`, iterative/parallel scopes rejected; runtime context queries `GetTokensOfType<Payload>()` remain legal (R3). |
| T4.51 | Validation messages for ACT-026/027/028/029/056/057/058 behave identically for typed activities (WP3 goldens). |

## 10. WP5 — Injection and context APIs

Placed in `Activity.IntegrationTests\Tests\Payloads` and
`Action.IntegrationTests\Tests\Payloads`.

### 10.1 Non-generic injected interfaces (D4)

Interfaces under test: `IInputTokens`, `IInputToken`, `IOptionalInputTokens`,
`IOptionalInputToken`, `IOutputTokens`, `IInputOutputTokens`, `IInputOutputToken`,
`IOptionalInputOutputTokens`, `IOptionalInputOutputToken`.

| ID | Scenario (repeat for every interface that applies) |
| --- | --- |
| T5.1 | `[TokenName("a")]` constructor parameter receives only tokens named `a`; typed tokens and payloads named otherwise are ignored. |
| T5.2 | `[TokenName("a","b")]` receives both names; order and counts; `IInputToken` yields the first available (section 8 default). |
| T5.3 | Missing `[TokenName]` on a non-generic interface → definition error (V5). |
| T5.4 | `[TokenName]` on a typed generic interface (`IInputTokens<string>`), on a non-token parameter → definition error (V6). |
| T5.5 | Output interfaces: adding a payload named in the set works; an undeclared name → `ArgumentException` (R9, section 8 default); typed token added through a payload output collection rejected. |
| T5.6 | Optional variants: absent tokens give an empty collection / `null`-equivalent, node still runs; required variants make the node wait. |
| T5.7 | Input/output variants (`IInputOutput*`): pass-through, modify-and-pass, `PassOn`, `PassAllOn`, `Clear`, `Remove`, `Contains` on payload tokens. |
| T5.8 | `IOptionalInputOutputTokens` / `IOptionalInputOutputToken` (D20): `PassAllOn` reaches the output; the added `IOutputTokens` members (`Add`, …) reach the output; same for the non-generic versions. |
| T5.9 | Several parameters in one constructor mixing typed and payload interfaces; two payload parameters with disjoint sets; two with overlapping sets (both receive the shared tokens). |
| T5.10 | Declaration scan: `Node.ScanForDeclaredTypes` records names for each interface kind so graph validation (V11, ACT-027/028/029) sees them: a node declaring payload input `["a"]` rejects an incoming flow carrying `["b"]` and accepts `["a"]`. |
| T5.11 | Actions: `ActionModel.ConsumedTokenTypes` / `ProducedTokenTypes` hold names; the same scenarios T5.1–T5.8 for Action classes and delegate Actions; existing `ConsumedEventNames` / `AddConsumedEvent` unchanged (D9). |
| T5.12 | Special nodes: `OutputPinNode<TEvent>`, `AcceptAndOutputEventActionNode<TEvent>`, Microsoft Agent Framework nodes compile and run with typed collections unchanged. |
| T5.13 | Activator: parameter resolution of the new attribute coexists with `[StateValue]`, `[FromField]` and `ActivatorUtilities`-resolved services in one constructor. |

### 10.2 Context APIs

| ID | Scenario |
| --- | --- |
| T5.14 | `IInput` / `IOutput` payload members: `Output<Payload>(payload)` routes by name; no `OutputPayload` (section 8 default); `PassPayloadTokensOn(names)` passes only matching tokens. |
| T5.15 | Name properties on `IFlowContext`, `IPinContext`, `ITransformationContext` (`TokenNames`, `TargetTokenNames`, `InputTokenNames`, `OutputTokenNames`): lists for payload elements, empty for typed; `TokenType` is `typeof(Payload)` for payload elements (section 8 default). |
| T5.16 | `IActivityOperationObservationContext`, `ITokensAccessor` unchanged for typed; new name properties for payload operations. |
| T5.17 | Activity initialization context and `ActionContext` expose payload tokens through `GetTokensOfType<Payload>()` and by name. |

## 11. WP6 — Transport and storage

### 11.1 Wire format spike (before the rest)

| ID | Scenario |
| --- | --- |
| T6.1 | `TokenHolder<Payload>` (with internal constructors and field envelope) round-trips through the Newtonsoft contexts (default JSON converter), streams, Orleans serialization and HTTP: fields, name, request flag. Fails first if the converter cannot reconstruct the payload. |
| T6.2 | `TokensTransferEvent.Tokens` (`List<TokenHolder>`, `TypeNameHandling.None`) with a mix of typed and payload holders. |

### 11.2 Transport

| ID | Scenario |
| --- | --- |
| T6.3 | `ITokensInput.Add(Payload)` / `AddRange(params Payload[])`; chaining with typed `Add`; `TokensInput<TToken>` / `TokensOutput<TToken>` unchanged. |
| T6.4 | `TokensInput.Add(Payload)` → payload input pin → payload flow → payload output pin → `TokensOutput.GetPayloadTokens(name)`; unknown name returns empty; order preserved; several payloads, several names. |
| T6.5 | Unregistered payload name in `Add(Payload)` (cannot be created without the factory) and payload from a different registration scope. |
| T6.6 | Persistence round trip: an activity with in-flight payload tokens (queued, mid-network, in an iterative scope) persisted and resumed — in-memory store, EF Core store, Orleans storage (`BehaviorContextPersistenceTests`). Typed token round trip (T0.14) still passes. |
| T6.7 | Stream/queue persistence for payload tokens (`StreamingRecovery`, `ActivityStreamingSqlRecoveryTests` style). |
| T6.8 | Orleans: `OrleansEventHolder` / `StateflowsBehavior` handling of `TokensInputEvent` with payload tokens (delivery, dedupe, outbox). |
| T6.9 | MinimalAPIs payload pin endpoints: one endpoint per payload name (segment = name, section 8 default); request decoding by `PayloadDefinition`; validation errors → 400; OpenAPI schema; HATEOAS links; `Before/After…EndpointDefinition` hooks; request-registered names do not get pin endpoints. Typed pin endpoints byte-for-byte unchanged (`ActivityPinEndpoints`, Orleans `ActivityPinMinimalApiTests`, `ActivityPinRouteCollisionTokens`). |
| T6.10 | Route collision: a payload name equal to a typed token's endpoint segment is rejected (R5/V3). |
| T6.11 | `ActivityVisitor` / `ActionVisitor` in MinimalAPIs produce routes for payload pins; Action endpoints for payload consumed events. |
| T6.12 | Microsoft Agent Framework: typed behavior untouched (existing `AIAgent.IntegrationTests` pass); no payload support asserted. |

## 12. WP7 — Observability and inspection

| ID | Scenario |
| --- | --- |
| T7.1 | `IActivityVisitor` / `IActionVisitor` payload callbacks (`TokenNamesDeclaredAsync` + `TokenNamesDeclarationKind`: Pin, Flow, Guard, Transformation, GuardConsumed, TransformationConsumed, TransformationProduced, …): emitted in a defined order, with names; typed callbacks unchanged. Default interface members: a visitor written against the old interface still compiles and runs. |
| T7.2 | Graph visiting order: payload and typed elements interleave in registration order (`VisitorOrdering.cs`). |
| T7.3 | Inspection: `FlowInspection`, `NodeInspection` expose the token key/name; JSON serialization of the inspection for payload flows. |
| T7.4 | OpenTelemetry `ActivityTracer`: spans/attributes for payload flows and nodes carry names; typed attributes unchanged (golden). |
| T7.5 | PlantUml: payload flows and pins render name sets and are distinguishable from typed; typed output byte-for-byte unchanged (`PlantUmlPins.cs`). |
| T7.6 | Visualizer (`layout.ts`, `fixtures.ts`): fixtures for payload flows and pins render without error; existing fixtures unchanged. |
| T7.7 | DevHost/inspection endpoints return payload tokens (names + data) for in-flight tokens. |
| T7.8 | Public API vocabulary guard (D32, D33, reflection over the public surface of `Stateflows.StateMachines`, `Stateflows.Activities`, `Stateflows.Actions`, `Stateflows.Entities`): no new member or parameter name contains `Payload` except the listed `PayloadToken` exceptions and the `Payload` type itself; no new builder member is prefixed `Payload`; `Trigger` appears only on transition members; the D33 list-only members have no `string`-first overload. |

## 13. WP8 — JSON definitions

| ID | Scenario |
| --- | --- |
| T8.1 | State-machine initializers and deferrals under the renamed key `events` (D34, B12) with the transition event shape: `clrType` or a unique list of `eventName`; the old key `triggers` on an initializer or deferral is rejected by the schema and the binder; schema validation accepts and rejects the right documents; binder builds the same behavior as the C# equivalent. |
| T8.2 | Activity initializers and accept-event nodes with `eventNames`; exactly-one-of rule against `eventType`. |
| T8.3 | Payload names on pins, flows, decisions, iterative and parallel scopes (`tokenNames`, `iterationTokenNames`); `exactly-one-of` with `tokenType`; mixing rejected. |
| T8.4 | Flow transformations with `outputTokenNames` (list) and the single `outputTokenName` sugar, exclusive with `outputTokenType`; produced name outside the set at run time → `ArgumentException`. |
| T8.5 | Bound guard/transformation contracts on payload flows: names come from the flow's `tokenNames` and the transformation's `outputTokenNames`; a `Payload`-producing contract without `outputTokenNames` rejected with a JSON diagnostic that cites the path. |
| T8.6 | A JSON-defined activity runs end to end with payload tokens (extend `JsonExecutionTests.cs`): `TokensInput.Add(Payload)` → JSON flows → `TokensOutput`. |
| T8.7 | JSON-defined events (`payload.schema.json`) registered through `AddPayloads` and used as tokens; ClearScript events (`ClearScriptStructuralEvents` successor) as tokens. |
| T8.8 | JSON persistence tests (`JsonStructuralPersistence` successor) with payload tokens. |
| T8.9 | Diagnostics: unknown payload name, bad schema shape, unknown guard/transformation class, all carry JSON paths. |
| T8.10 | The README samples (initializers, deferrals, tokens) are extracted and executed as tests so documentation cannot drift. |

## 14. Cross-cutting scenarios

End-to-end scenarios that tie several packages together; each exists once, in the
Activity project unless noted.

| ID | Scenario |
| --- | --- |
| TX.1 | Order pipeline (typed + payload): an HTTP-style `TokensInput` containing a payload `order.placed` and a typed `Receipt` goes through an activity with a payload guard class, a `Payload→typed` transformation, a typed processing node, a `typed→Payload` transformation and a payload output, with persistence across a mid-flow reload. |
| TX.2 | One activity with every payload feature enabled (pins, decisions, iterative, parallel, data store, accept-event, request token, class stages, delegate stages), asserted for no typed-flow regression. |
| TX.3 | Action behavior consuming a payload event (`AddConsumedEvent`) **and** a payload token, producing a payload token. |
| TX.4 | A state machine whose effect sends a payload to an activity that flows it as a token and publishes a payload event back; state-machine deferral on the published payload name. |
| TX.5 | Entity mutated by a payload that an activity publishes; MinimalAPIs endpoint for the entity and for the activity pin side by side without route collision. |
| TX.6 | Concurrency/streaming: many payload tokens in streamed activities with interleaving and partitioning; ordering and no loss (reuse the existing streaming load harness shape). |
| TX.7 | Performance sanity: token selection with payload keys is not slower than typed keys by more than a documented tolerance on a micro-benchmark (not a pass/fail gate; recorded). |
| TX.8 | Backwards compatibility sweep: every public-API member listed as unchanged in the API report still exists with the same signature (reflection test against a stored snapshot of the typed API). |

## 15. Gating and traceability

### 15.1 Per work package

| Package | Must be green before merge | New test groups that must exist |
| --- | --- | --- |
| WP0 | All integration projects | T0.1–T0.18; failing-first tests either fixed in the same package (T0.1–T0.5, T0.12 moves with WP2b but is committed failing-first, marked and tracked) or marked `Ignore` with a pointer to the package that fixes them. |
| WP1 | All projects (build + tests) | T1.1–T1.7 |
| WP2a–f | All projects | T2.1–T2.18 for the models touched; parity matrix cells for that slice. |
| WP2g | Entity, Orleans, MinimalAPIs-related tests, UnionTypes | T2g.1–T2g.22 |
| WP2h | StateMachine, Activity, Entity | T2.16–T2.18, T2g.3, T4.38a (overload compile checks), T0.18 |
| WP3 | All activity/action/UnionTypes/Orleans projects | T3.1–T3.5; WP0 characterization unchanged |
| WP4 | Activity, Action, Json, UnionTypes | T4.1–T4.51 (including T4.38a, T4.43a–c) |
| WP5 | Activity, Action, Orleans | T5.1–T5.17 |
| WP6 | Activity, Orleans, MinimalAPIs-related, Json | T6.1–T6.12 |
| WP7 | Activity, Action, Orleans, Visualizer (npm) | T7.1–T7.8 |
| WP8 | Json, ClearScript, Activity Json | T8.1–T8.10 |
| WP9 | All; doc link check | Doc tests (README samples T8.10); wiki code snippets compile (extracted) where practical |
| Final | Full solution | TX.1–TX.8 |

The full set of integration projects is:
`StateMachine.IntegrationTests`, `Activity.IntegrationTests`, `Action.IntegrationTests`,
`Entity.IntegrationTests`, `UnionTypes.IntegrationTests`, `Orleans.IntegrationTests`,
`AIAgent.IntegrationTests`, `Beehaviors.IntegrationTests`,
`OrderFulfillment.IntegrationTests`, `StreamingLoad.IntegrationTests`,
`StreamingFractals.IntegrationTests`. Projects that need infrastructure (SQL Server,
Orleans cluster) run in the environment that already runs them today; a package is
not blocked on infrastructure-only suites if they are skipped identically before and
after.

### 15.2 Traceability

| Item | Covered by |
| --- | --- |
| D1–D3, D7, D8 (rename, hard break, namespaces) | T1.1–T1.7 |
| D4, D5 | T5.1–T5.4, T4.24, T4.50 |
| D6, D15 (names everywhere; no mixing) | T2.3, T2.15, T4.16, T7.* |
| D9 (Actions) | T5.11, TX.3 |
| D10 (typed collection bug fixes) | T0.1–T0.6 |
| D11, D12 (payload families, `ITokensInput`) | T4.1–T4.10, T6.3–T6.4 |
| D13 (wildcards) | T0.8, T4.13 |
| D14 (JSON) | T8.1–T8.10 |
| D16 (transformation names, delegate and class) | T4.21–T4.30 |
| D17, D23, D24 | Superseded by D29–D31; no tests |
| D29 (no attributes; required call-site names) | T2.16–T2.18, T2g.3, T4.24, T4.31–T4.38, T8.5 |
| D30 (two name sets per flow) | T4.39–T4.43a, T4.47 |
| D31 (`transformedNames` overloads) | T4.21, T4.27–T4.29, T4.38a, T0.18 |
| D32 (vocabulary) | T7.8 |
| D33 (name-first overloads, list-only members) | T4.38a, T7.8 |
| D34 (JSON `events` key, events page mention) | T8.1 |
| D25 (wiki page) | Doc link check in WP9 (page `4.6.-Payloads`, renamed `4.1.-Events`, updated `seealso` links) |
| D26 (entity `ITypeMapper`) | T0.15, T2g.18–T2g.22 |
| D18 → D27 (pipeline checks match typed checks) | T0.17, T4.39–T4.46 (incl. T4.43a–c), T5.10 |
| D19 (entities) | T2g.1–T2g.22 |
| D20 (`IOutputTokens` on optional IO) | T0.3–T0.5, T0.6, T5.8 |
| D21 (overlap allowed) | T0.7, T4.14, T4.48 |
| D22 (`PayloadRequest` tokens) | T0.11, T4.17–T4.20 |
| D28 (`params string[]` attributes, sugar plus list forms) | T2.16, T2.17, T4.21, T4.31, T4.38a |
| R1–R16 | R1/R2: T3.2, T4.11–T4.13; R3: T4.50; R4: T2.5, T4.25; R5: T4.49, T6.10; R6: T4.12; R7: T4.21–T4.30; R8: T0.7, T4.14; R9: T5.5; R10: T4.8; R11: T2.11, T4.9; R12: T4.31–T4.37, T2.16, T2g.3; R13: T4.39–T4.43c; R14: T4.17–T4.20; R15: T2g.4–T2g.5, T2g.18–T2g.19; R16: T4.39–T4.43a |
| V1–V12 | V1–V3: T2.5, T2g.6, T4.25, T4.36; V4: T4.24, T4.50; V4a: T2.16, T2g.3, T4.24, T4.32; V5/V6: T5.3, T5.4; V7: T4.47; V8: T4.21–T4.25; V9/V10: T2.6; V11: T4.39–T4.46; V12: T2g.9 |
| B1–B11 | B1: T1.3; B2: T1.5; B3: T0.12; B4: T2.10; B5: T4.13; B6: T0.1–T0.6; B7: T7.1; B8: T0.5, T5.8; B9: T2g.4, T2g.8, T2g.18; B10: T2.16, T2g.3, T4.31–T4.32; B11: T4.27–T4.28, T4.38a, T0.18; B12: T8.1; B13: T4.38a, T7.8 |
| Section 8 defaults | Each section 8 default (all confirmed) has at least one test that pins it, so a later change of mind leads directly to the tests to change. |

## 16. Out of scope and known gaps

- **Pre-rename persisted data** is not tested (D3); only the release note is checked.
- **Mixing CLR types and names** in one declaration is not offered (D6) and is only
  tested as "not available".
- **Entities and `ITypeMapper`** are covered by T2g.18–T2g.22 (D26); no tests of
  entity `ITypeMapper` beyond `OneOf` and one UnionTypes union.
- **Microsoft Agent Framework** payload tokens: not offered; typed behavior only.
- **Legacy nested project** `Tests\StateMachine\StateMachine.IntegrationTests` (net6.0,
  47 tracked files, not in `Stateflows.sln`): excluded from runs, edits and the WP1
  grep guard. Worth a separate decision whether to delete it.
- **Infrastructure-dependent suites** (SQL Server, multi-silo Orleans): covered by
  the existing environments; if unavailable locally, the in-memory equivalents plus
  the same scenarios in the Orleans test-cluster project are the gate.
- **Performance** (TX.7) is recorded, not gated.
- **Compile-time-only guarantees** (for example `AllowMultiple=false`, no mixing
  overloads) are verified with a small Roslyn/`CSharpCompilation` harness only if one
  already exists in the repository; otherwise documented and covered by review.
