using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities.Registration.Interfaces;

/// <summary>Configures the flows and exception handlers of a typed event-accepting node.</summary>
/// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
public interface IAcceptEventActionBuilder<TEvent> :
    IObjectFlowBase<IAcceptEventActionBuilder<TEvent>>,
    IControlFlowBase<IAcceptEventActionBuilder<TEvent>>,
    IExceptionHandlerBase<IAcceptEventActionBuilder<TEvent>>;

/// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
/// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
/// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
public interface IOverridenAcceptEventActionBuilder<in TEvent> :
    IObjectFlowBase<IOverridenAcceptEventActionBuilder<TEvent>>,
    IOverridenObjectFlowBase<IOverridenAcceptEventActionBuilder<TEvent>>,
    IControlFlowBase<IOverridenAcceptEventActionBuilder<TEvent>>,
    IOverridenControlFlowBase<IOverridenAcceptEventActionBuilder<TEvent>>,
    IExceptionHandlerBase<IOverridenAcceptEventActionBuilder<TEvent>>
{
    /// <summary>Changes this node to accept a structural event with the specified name.</summary>
    /// <param name="eventName">Registered structural-event name accepted or published by this node.</param>
    /// <returns>A builder typed for the structural event.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
    IOverridenAcceptEventActionBuilder<StructuralEvent> ChangeAcceptedEvent(string eventName);

    /// <summary>Changes this node to accept a narrower event type compatible with the inherited event type.</summary>
    /// <typeparam name="TAcceptedEvent">Event payload type accepted, created, or sent by this node.</typeparam>
    /// <returns>A builder typed for the new accepted event.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
    IOverridenAcceptEventActionBuilder<TAcceptedEvent> ChangeAcceptedEvent<TAcceptedEvent>()
        where TAcceptedEvent : TEvent;
}

/// <summary>Configures a typed event-accepting action implementation and its flows.</summary>
/// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
/// <typeparam name="TAcceptEventAction">Typed node implementation implementing
/// <see cref="IAcceptEventActionNode&lt;TEvent&gt;" />.</typeparam>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
public interface IAcceptEventActionBuilder<in TEvent, out TAcceptEventAction> :
    IObjectFlowBase<IAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IControlFlowBase<IAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IExceptionHandlerBase<IAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IElementBuilderBase<TAcceptEventAction, IAcceptEventActionBuilder<TEvent, TAcceptEventAction>>
    where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>;

/// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
/// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
/// <typeparam name="TAcceptEventAction">Typed node implementation implementing
/// <see cref="IAcceptEventActionNode&lt;TEvent&gt;" />.</typeparam>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
/// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
public interface IOverridenAcceptEventActionBuilder<in TEvent, out TAcceptEventAction> :
    IObjectFlowBase<IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IOverridenObjectFlowBase<IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IControlFlowBase<IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IOverridenControlFlowBase<IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IExceptionHandlerBase<IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IElementBuilderBase<TAcceptEventAction, IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>
    where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
{
    /// <summary>Changes this node to accept a structural event with the specified name.</summary>
    /// <param name="eventName">Registered structural-event name accepted or published by this node.</param>
    /// <returns>A builder typed for the structural event.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
    IOverridenAcceptEventActionBuilder<StructuralEvent> ChangeAcceptedEvent(string eventName);

    /// <summary>Changes this node to accept a narrower event type compatible with the inherited event type.</summary>
    /// <typeparam name="TAcceptedEvent">Event payload type accepted, created, or sent by this node.</typeparam>
    /// <returns>A builder typed for the new accepted event.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
    IOverridenAcceptEventActionBuilder<TAcceptedEvent> ChangeAcceptedEvent<TAcceptedEvent>()
        where TAcceptedEvent : TEvent;
}