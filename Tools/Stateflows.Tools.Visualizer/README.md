# Stateflows visualizer

The Svelte component is transport-independent. Its host supplies a diagram document,
runtime observations and selection; the component owns presentation and layout.
The development host is an unfinished inspection harness, not a production dashboard.

## Executable demo

From this directory run `npm run demo`, then open **http://127.0.0.1:5167**.
This builds the frontend and starts the Development-only .NET host with in-memory
Orleans storage. Requires Node.js and the .NET 10 SDK or later. No separate Vite
process is needed. Stop with Ctrl+C; restarting loses instances and captured history.
HTTP port 5167 and Orleans ports 11127/30017 must be available.

**Transition routing** switches between rounded orthogonal, straight-segment
polyline, and cubic spline routes. The reusable component accepts
`presentation.routing: 'ORTHOGONAL' | 'POLYLINE' | 'SPLINES'` (orthogonal by default).
**Flow direction** switches between horizontal (`presentation.direction: 'RIGHT'`,
the default) and vertical (`'DOWN'`), independently of routing mode.
**State ordering** offers Automatic, Prefer definition order (the lab default),
and Preserve definition order. `presentation.stateOrdering` accepts `AUTOMATIC`,
`PREFER_DEFINITION`, or `PRESERVE_DEFINITION`; the reusable component defaults to
Automatic. Definition order means visitor/registration order within each scope,
including inherited registrations. Preserve prevents sibling reordering during
crossing minimization and enforces component order, but transition dependencies
still determine layers and additional crossings may result.
Settings appear beside **Find** and survive model switches and embedded navigation;
model-specific collapse state is reset. Hosts can supply the optional `settings`
snippet to place their own controls in that toolbar.
Changing routing, direction, or ordering relayouts the diagram without changing
definition or runtime state. Spline mode smoothly joins ELK's polyline waypoints
with cubic bends, including external ancestor boundary loops, while preserving
transition endpoints.

**Find** focuses all matching node names, transition labels, and grouped operation
rows without changing selection. **Clear focus** removes the highlight. Hosts can
control the subset with `focusedIds` and receive `onfocuschange(ids)`; operation
groups use their `selectionId`. Focus uses `--sfv-focus` and `--sfv-focus-bg`.

**Triggers** hides unchecked transitions without changing node positions, routes,
or compartment heights. Internal-transition rows retain invisible space; generated
initial arrows are never filtered. Multi-trigger transitions remain visible when
any trigger is checked; completion triggers appear as `default`. Hosts can supply
`DiagramEdge.triggers`, control `hiddenTriggers`, and receive
`ontriggerfilterchange(triggers)`. Filtering only affects presentation, not runtime
execution or installed breakpoints. Local focus and filters reset on model changes.

### Production definition verification

The host automatically imports the upgraded Lifecare definitions when the sibling
`Lifecare-SystemAdministration` repository is present (eleven concrete state
machines, excluding `ProcessBase` from the catalog). Start the lab normally to
inspect them in the **Model** selector. For a different checkout location, supply
`LifecareProject` explicitly:

```powershell
dotnet run --project Tools\Stateflows.Tools.Visualizer.DevHost -p:GeneratePackageOnBuild=false "-p:LifecareProject=W:\Source\Lifecare-SystemAdministration\Lifecare-Clearing.Api\Lifecare-Clearing.Api.csproj" --launch-profile Visualizer
```

This registers only definitions and an empty resource group required to build
their embedded behaviors; it does not configure production dependencies or run
business operations. Builds without the sibling repository omit these models.
The referenced application's static web assets are
excluded from this host.

Against a running host, execute `npx playwright test --config
playwright.production.config.ts` in the frontend directory. Set
`PRODUCTION_VISUALIZER_URL` to its URL (defaults to `http://127.0.0.1:5168`).
The suite covers eighteen ordering/routing/direction combinations per model, breakpoint
controls, operation selection, collapsing, and embedded behavior navigation.

The demo selects the registered `ReviewProcess` state machine when the backend
loads. The **Model** selector also includes `NestedCalculation`, the existing
`Telemetry` C# activity, and the synthetic fixtures.

- `ReviewProcess`: **initialize**, then **approve** or **reject**; approval enters
  orthogonal payment/stock regions through a fork; rejection traverses a junction.
  Another **approve** deliberately throws `InvalidOperationException` in
  `StockReady` entry to exercise failure visualization; **synchronize** completes both branches
  through a join and decision (the framework's choice pseudostate).
  **refresh** exercises Review's internal transition; **finish** leaves Checking through the decision; **cancel** leaves Processing,
  and **resume** re-enters it through history.
- `NestedCalculation`: **Run calculation** executes the nested doubling network.
- `Telemetry`: **initialize**, optionally **configure** defaults, then **Send samples**
  submits two actual sensor samples (75 and 20) to the existing telemetry activity.

Use **New instance** for a fresh execution. Commands switch to live mode before
execution; SSE supplies real operations to the diagram and capture list. Click an
observation to inspect its execution frame. State-machine current states come from
a status query after each demo command. Activity badges are observed activation
and execution results, not an authoritative current token marking. Captures are
bounded and do not constitute transaction commit receipts.

For frontend hot reload, run the .NET host separately using its `Visualizer` launch
profile and `npm run dev` here; Vite proxies `/api` to port 5167.
After `npm run build`, `npm run test:demo` starts the real host and exercises C#
commands, SSE capture and overlays through Chromium.

## Definition boundary

Core visitors remain callback-based. They do not return a graph or publish a complete
definition snapshot. The development host's `DefinitionCatalog` assembles its own
transport documents from individual callbacks:

- Activity node callbacks expose runtime identities, parent identities, options,
  pin types and event names.
- Activity flow callbacks expose resolved endpoints, final token types, weights,
  else branches and cross-scope semantics.
- State-machine callbacks expose the root initial state, vertices, regions and
  transitions, including runtime transition identities and local/external semantics.
- Per-operation callbacks identify typed entry/exit/do handlers, guards and effects;
  composed guards retain their Boolean grouping and negation.

Enriched callback overloads default to the original callbacks, both on the
interfaces and convenience base classes. Existing visitors continue to receive one
notification per element; consumers needing additional metadata override the
enriched overload instead. Metadata is read when a built registration is visited,
not captured prematurely while builders are running. Callbacks are not guaranteed
to arrive in parent-before-child order.

The host owns its `ActivityDefinition` and `StateMachineDefinition` records. Neither
is a core contract. Other visitors can produce PlantUML, endpoints, catalogs or
different models without depending on the visualizer's document shape.

## State-machine notation

Initial selections appear as UML initial pseudostates and default arrows, not
state-name bullets. Their circles match their routing bounds, and these generated
elements are not selectable. States separate names, textual internal work and nested
configuration into compartments. A sole region is omitted visually; orthogonal
regions are compartments separated by dashed lines, each independently collapsible
using its header control. Regions stack vertically where routing permits; cross-region
connections can require side-by-side compartments with vertical separators.
Internal transitions appear in the work compartment,
while their original identities remain in the document for inspection. The work
compartment lists initialization, entry, exit and finalization alongside do behaviors,
deferrals and internal transitions. Events, substates and individual regions have
matching header controls and collapse independently. Collapsing the whole state
hides every compartment; expanding it restores their individual collapse settings.
Expanded work and substate compartments have no textual header. States with multiple
regions have no separate substate control: each region collapses to a single header
line. States containing only internal work can also collapse to their name.
These settings are presentation-only (`collapsedIds`, `collapsedOperationIds` and
`collapsedSubstateIds`).

Transitions use `trigger [guard] / effect`, without a label frame, and have rounded
bends. Between a state and its descendant, local transitions stay within the state;
external transitions leave its boundary before re-entering. Typed handlers
show their class names; a state's own lifecycle implementation shows method-style
labels such as `initialization / onInitialization()` and `entry / onEntry()`.
Anonymous delegates are explicitly labeled `action`, `guard`
or `effect`; their C# bodies are not analyzed. Presentation-generated initial
nodes/arrows are not executable registrations or runtime observation targets.
Fork and join bars are perpendicular to the layout flow: vertical for `RIGHT`,
horizontal for `DOWN`.
Selection highlights follow each marker's own contour instead of a rectangular
outline, with the same 3px gap and 3px outline width as states. Expanded event rows start at the same vertical offset as collapsed
compartment labels.

Embedded behaviors have a borderless inline navigation button on their `do` row,
with a distinct Action, Activity or State Machine icon. Collapsing the events
compartment or whole state hides the button with that row.
`DiagramNode.embeddedBehavior` carries its registered type/name and
optional version; the renderer reports clicks through `onopenbehavior(behavior,
elementId)` so the host controls navigation. An unavailable definition is reported
explicitly rather than opening a different model. The lab's Review and Checking
states embed `ReviewPreparation` activities, registered automatically at their
embedding sites; their icons open those visitor-backed activity definitions.
Done embeds the `CompletionAudit` action and Rejected embeds the `RejectionHandling`
submachine. Their icons open the corresponding visitor-backed definitions too.
An action is displayed as a single opaque C# implementation, not a fabricated graph
of its internals. Deferrals use `Trigger [guard] / defer`, omitting brackets when
unconditional; typed names, negation and nested Boolean grouping are retained.

Routing uses ELK's orthogonal router. `DiagramPresentation.direction` currently
exposes `RIGHT` or `DOWN`; spacing, port constraints and other ELK options are
internal layout policy. Per-edge waypoints and manual routing are not exposed.

## Operations and breakpoints

`DiagramNode.operations` supplies semantic rows alongside the legacy `details`
text. Rows sharing `selectionId` select together (for example, every entry handler).
`onselect` reports `kind: 'operation'`, the owning node and the runtime operation.
Internal transitions retain their transition identity and independent guard/effect
parts, even though their labels appear inside a state.

The reusable component defaults to `allowBreakpoints={false}`. A host opting in
supplies controlled `breakpoints: BreakpointTarget[]` and
`onbreakpointchange(target, enabled)`. Hover/focus previews a translucent red
circle; installed breakpoints are solid. Lifecycle and deferral rows target their
state operation; transition guard and effect parts target distinct operations.
Every registered transition has a breakpoint before its trigger, including internal,
default and regular transitions without handlers. This shares `transition.guard`
with the guard-part control, pausing before guard evaluation, even when no guards
are registered. Effect-part controls remain independent.
Default transitions show italic `default` while breakpoint
controls are enabled. Generated initial arrows remain non-executable.
Registered state headers also expose `state.entry` breakpoints, including states
without entry handlers. These share the same target as entry rows and remain
available when compartments or the whole state are collapsed.
`DiagramNode.entryBreakpoint` opts a node into this entry control; registered
pseudostates expose it immediately before their name too. Regions do not.
Most pseudostates use the entry callback; merged junctions and history restore
points pause before the incoming (merged for junctions) effect operation.
Joins pause once after synchronization, before their outgoing guard evaluation.
Junction routes execute as a merged transition; each drawn segment can pause
before that merged guard/effect operation, using its registered identity.
Breakpoint gutters and the italic default hint exist only while controls are
enabled. Text baselines stay unchanged; horizontal spacing adjusts with the slots.
Breakpoint updates do not relayout the diagram. Legacy detail-only rows are
selectable but do not invent executable breakpoint targets.

In the development host, **Enable breakpoints** activates these controls for
registered state-machine definitions. Breakpoints apply to the selected
definition/version and instance only. Global observers pause *before* the selected
operation, awaiting a gate outside Orleans; the SSE debugger stream reports the
pause and **Resume** releases it through a direct Minimal API endpoint.
Do-behavior breakpoints pause before dispatching the embedded behavior, not inside
its C# implementation. Deferral breakpoints pause before evaluating the deferral
rule. Multiple handlers of an operation pause together, not individually.

Pauses wait until Resume (or breakpoint removal or host shutdown), without an
application safety timeout. Orleans request and transaction timeouts still apply:
this is a volatile, single-process development debugger, not a distributed
debugging protocol or CLR debugger. Keep pauses short; closing the browser does
not resume execution. Installed breakpoints remain active if setting controls are
hidden. The host has no authentication and must not be exposed as a production
debugging service.

## Synthetic laboratory

Run `npm run dev` in this directory and open the local Vite URL. Synthetic fixtures
are available immediately, even without the .NET development host. Choose them in
the **Model** selector. Backend failures are displayed but do not prevent fixture use.
Use individual scope toggles or **Collapse top-level scopes** / **Expand all scopes**.
Large fixtures initially provide an overview; zoom and pan to inspect individual elements.

| Family | Small | Medium | Large | Deep |
|---|---|---|---|---|
| State machine | 19 nodes / 19 edges | 152 / 159 | 608 / 639 | 36 / 24 |
| Activity | 21 nodes / 22 edges | 168 / 183 | 672 / 735 | 36 / 24 |

State-machine fixtures include orthogonal regions, nested composite states,
ancestor/descendant transitions, internal transitions, self-loops, parallel edges,
history and choices. Activity fixtures include structured/parallel/iterative scopes,
typed pins, transformations, weighted and else flows, data stores, fork/join and
cross-scope connections. Deep fixtures exercise twelve nested scopes with repeated
display labels, long edge labels and connections spanning eleven boundaries.

These are deterministic renderer-input fixtures, not executable framework
registrations or claims of valid business semantics. No runtime observations are
fabricated. The development host's registered samples exercise actual execution.
Counts include generated initial nodes/arrows. State-machine internal transitions
remain document edges but render textually rather than as extra arrows.

`npm test` checks identities, endpoints, containment, finite routes, collapsed
cross-boundary connections and a 20-second per-layout regression ceiling.
`npm run test:e2e` exercises rendering and collapse/expand in Chromium without a
backend. Install its browser with `npx playwright install chromium` if absent;
`PLAYWRIGHT_CHROMIUM_EXECUTABLE` can select an existing Chromium executable.
