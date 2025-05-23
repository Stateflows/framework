# 0.17.0
## Core

### Introduced: Local and external Transitions
For the purposes of fine-tuning specific Transition topologies, it is now possible to decide if Transition should be [local or not](https://github.com/Stateflows/framework/wiki/Evaluation-of-Transitions#local-transitions):
```csharp
    .AddTransition<EventClass>("target", b => b
        .SetIsLocal(false)
    )
```
All Transitions are local by default.

### Introduced: Caching
Pending notifications were kept in memory, which was a bug from the perspective of clustered environment. New interface `IStateflowsCache` was introduced to enable distributed caching of notifications.

### Enhanced: Activity info
Activity info now contains (alongside with list of expected events) a tree of active nodes in current configuration. Active node is a node which is enabled to accept an Event and all of its parents.

### Fixed: Activity accept event nodes
Several bugs on accept event nodes in Activity (including time event nodes) were fixed; in particular it is now possible to await for time events within structured activity nodes.

### Enhanced: Tracing messages
Tracing messages are now more consistent and verbose, especially around exceptions handling.

## Extensions

### Introduced: Stateflows.Extensions.MinimalAPIs
Based on ASP.NET Minimal APIs, new extension now does two main things:
- exposes REST-compliant API interface to interact with Behaviors,
- enables extending that interface with custom endpoints, defined directly within models.

Although this is not considered a typical transport layer (no client library is implemented nor planned), extension provides complete REST API to work with Stateflows:
- querying of defined Behavior classes,
- CRUD to manage Behavior instances,
- interact with Behavior instances using endpoints dedicated for each accepted Event type.

Custom endpoints can be defined on the level of Behavior (those are available as long as Behavior instance is initialized) or on the level of State / Node (those are available only when given State / Node is active):
```csharp
    .AddState<Paid>(b => b
        .AddEndpoints(b => b
            .AddGet("/invoices", () => { /* endpoint logic here */ })
        )
    )
```
Note that custom endpoints **are not executed in the context of a Behavior instance**; their purpose is to put CRUD operations for resources that are logically owned by a Behavior instance under its resource, making REST interface more coherent.

### Introduced: Stateflows.Extensions.OpenTelemetry
With this extension all interactions with Behaviors are logged and all internal works of State Machine and Activity models are traced accordingly to OpenTelemetry standards. Purpose of this extensions is to enhance debugging and auditing by enabling a structured insight into complexities of models and their execution.

Custom code within model can also add its own logs and traces; those will be added in the context of Stateflows traces, enabling detailed traceability.

Node that OpenTelemetry must be configured in your app in order to gather logs and traces from Stateflows Behaviors.

### Introduced: Stateflows.Extensions.Scheduling
Stateflows provides simple scheduling based on `TimeEvent` triggers in State Machines and Activities out of the box, but this standard solution has limitations:
- only `TimeEvent` descendants can be scheduled as triggers,
- schedule must be statically defined within model.

This extension enables persistent, dynamic scheduling of any Event, based on several options:
- Event will be sent at exact time,
- Event will be sent after given delay,
- Event will be sent repeatedly using given interval,
- Event will be sent repeatedly using given cron configuration.

Event can be scheduled by the Behavior instance itself or by any of its clients:
```csharp
// to schedule sending an Event
var (success, scheduleId) = await context.ScheduleDelayAsync(new TimeSpan(0, 12, 0, 0), new Reject());

// to unschedule previous schedule
await context.UnscheduleAsync(scheduleId);
```
Note that scheduling in this extension is implemented as State Machine Behavior. All its operations (scheduling, unscheduling, sending scheduled Events) will be visible in logs/traces.