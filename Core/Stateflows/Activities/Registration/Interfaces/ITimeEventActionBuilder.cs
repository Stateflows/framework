using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces;

/// <summary>Configures a time-event node and its outgoing control flow.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-time-event-action-node" />
public interface ITimeEventActionBuilder :
    IObjectFlowBase<ITimeEventActionBuilder>,
    IControlFlowBase<ITimeEventActionBuilder>,
    IExceptionHandlerBase<ITimeEventActionBuilder>;

/// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-time-event-action-node" />
/// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
public interface IOverridenTimeEventActionBuilder :
    IObjectFlowBase<IOverridenTimeEventActionBuilder>,
    IOverridenObjectFlowBase<IOverridenTimeEventActionBuilder>,
    IControlFlowBase<IOverridenTimeEventActionBuilder>,
    IOverridenControlFlowBase<IOverridenTimeEventActionBuilder>,
    IExceptionHandlerBase<IOverridenTimeEventActionBuilder>;

/// <summary>Configures a typed time-event node and its outgoing control flow.</summary>
/// <typeparam name="TTimeEventAction">Typed node implementation implementing <see cref="ITimeEventActionNode" />.
/// </typeparam>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-time-event-action-node" />
public interface ITimeEventActionBuilder<out TTimeEventAction> :
    IObjectFlowBase<ITimeEventActionBuilder<TTimeEventAction>>,
    IControlFlowBase<ITimeEventActionBuilder<TTimeEventAction>>,
    IExceptionHandlerBase<ITimeEventActionBuilder<TTimeEventAction>>,
    IElementBuilderBase<TTimeEventAction, ITimeEventActionBuilder<TTimeEventAction>>
    where TTimeEventAction : class, ITimeEventActionNode;

/// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
/// <typeparam name="TTimeEventAction">Typed node implementation implementing <see cref="ITimeEventActionNode" />.
/// </typeparam>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-time-event-action-node" />
/// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
public interface IOverridenTimeEventActionBuilder<out TTimeEventAction> :
    IObjectFlowBase<IOverridenTimeEventActionBuilder<TTimeEventAction>>,
    IOverridenObjectFlowBase<IOverridenTimeEventActionBuilder<TTimeEventAction>>,
    IControlFlowBase<IOverridenTimeEventActionBuilder<TTimeEventAction>>,
    IOverridenControlFlowBase<IOverridenTimeEventActionBuilder<TTimeEventAction>>,
    IExceptionHandlerBase<IOverridenTimeEventActionBuilder<TTimeEventAction>>,
    IElementBuilderBase<TTimeEventAction, IOverridenTimeEventActionBuilder<TTimeEventAction>>
    where TTimeEventAction : class, ITimeEventActionNode;
