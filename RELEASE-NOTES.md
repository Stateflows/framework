# 0.17.1
## Core

### Introduced: Relay
Notifications published by one Behavior can be relayed by second Behavior - for subscribers and watchers of second Behavior it will be the same as if it is publishing them.

Relay can be started by sending `StartRelay` Event to publisher and stopped by sending `StopRelay` Event.

For embedded Behaviors (Actions and Activities bound to State Machines), relays can be declared the same way as for subscriptions:

```csharp
    // part of State Machine definition
    .AddState("stateA", b => b
        .AddTransition<SomeEvent>("stateB", b => b
            .AddEffectActivity("effect", b => b
                .AddRelay<SomeNotification>()
            )
        )
    )
```

> Relaying is much faster than subscribing and republishing notification Event as it doesn't involve execution of relaying Behavior - it is publishing-as-usual, just using a different Behavior identity.

> Note that relayed notification Event is not processed by relaying Behavior in any way. If there is a need to process an Event before it is republished, subscription should be used. 

### Changed: Time To Live
> Breaking change: by default, published notification Events have no TTL value set. This is a change from previous state when default TTL was set to 60 seconds. Moreover, `Publish()` method doesn't accept `timeToLiveInSeconds` parameter anymore.

There is a new publishing method introduced: `PublishTimed()`, which accepts `timeToLiveInSeconds` parameter.

### Introduced: Retained notifications
Notifications now can be retained within cache, making them infinitely available for subscribers/watchers. Rules of notification Event retention:
- There can be only one retained Event of type at one time; if a retained notification Event is published, previously retained Event of same type is discarded from cache and will no longer be available.
- Retained notification can't have a Time To Live; if TTL is set on publish, it will be ignored.

> Note: retention is designed to replace `RequestAndWatchAsync()` method which is now marked as obsolete as well as its `RequestAndWatchStatusAsync()` version. Instead of providing a support for request Event along with notification Event, simply retain your notification Event, so it will be available for subscribers anytime.

There is a new publishing method introduced: `PublishRetained()`.

Notification Event can be retained different ways:
1. by providing `Retain` header when publishing:
```csharp
context.Publish(new MyNotification(), new List<EventHeader>() { new Retain() });
```
2. by using dedicated publising method `PublishRetained()`:
```csharp
context.PublishRetained(new MyNotification());
```
3. by marking notification Event type with `RetainedAttribute`:
```csharp
[Retain]
public class MyNotification {}
```

> Note: `BehaviorInfo`, `StateMachineInfo` and `ActivityInfo` are retained by default using `RetainAttribute`.

## Extensions

### Enhanced: Stateflows.Extensions.MinimalAPIs
Minimal APIs extension now enables full customization of endpoints using `IEndpointDefinitionInterceptor` interface or builder interface available via `MapStateflowsMinimalAPIsEndpoints()` method:

```csharp
app.MapStateflowsMinimalAPIsEndpoints(b => b
    .ConfigureAllEndpoints(b => b
        .ConfigureHandler(h => h.RequireAuthorization())
    )
    .ConfigureGetAllInstancesEndpoint(b => b
        .Disable()
    )
    .ConfigureStateMachines(b => b
        .ConfigureGetInstancesEndpoint(b => b
            .Disable()
        )
        .ConfigureStateMachine("Doc", b => b
            .Disable()
        )
    )
    .SetApiRoutePrefix("custom")
);
```

This configuration can also be done on the level of a Behavior definition class, too - using interfaces `IStateMachineEndpointsConfiguration`, `IActivityEndpointsConfiguration`, or `IActionEndpointsConfiguration`, respectively to Behavior type.

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
        .AddEndpoints(b =>
        {
            b.AddGet("/invoices", () => { /* endpoint logic here */ });
        })
    )
```
Note that custom endpoints by default **are not executed in the context of a Behavior instance**; their purpose is to put CRUD operations for resources that are logically owned by a Behavior instance under its resource, making REST interface more coherent.

If it is a requirement to run endpoint within context of a behavior, context may be injected using one of dedicated interfaces:
- `IBehaviorEndpointContext` - available in all custom endpoints,
- `IActionEndpointContext` - available in Action endpoints,
- `IStateMachineEndpointContext` - available in State Machine endpoints,
- `IStateEndpointContext` - available in State Machine, State-scoped endpoints,
- `IActivityEndpointContext` - available in Activity endpoints,
- `IActivityNodeEndpointContext` - available in Activity, Structured Activity-scoped endpoints.

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