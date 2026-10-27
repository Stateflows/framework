using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities;

/// <summary>Configures external object flows for a reactive structured activity.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
public interface IReactiveStructuredActivityExternalsBuilder :
    IObjectFlowBase<IReactiveStructuredActivityBuilder>,
    IControlFlowBase<IReactiveStructuredActivityBuilder>,
    INodeOptions<IReactiveStructuredActivityBuilder>;

/// <summary>Configures a reactive structured activity's graph and external flows.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
public interface IReactiveStructuredActivityBuilder :
    IReactiveStructuredActivityExternalsBuilder,
    // IObjectFlowBase<IReactiveStructuredActivityBuilder>,
    // IControlFlowBase<IReactiveStructuredActivityBuilder>,
    IReactiveActivityBase<IReactiveStructuredActivityBuilder>,
    IActivitySpecials<IReactiveStructuredActivityBuilder>,
    IInitialBase<IReactiveStructuredActivityBuilder>,
    IFinalBase<IReactiveStructuredActivityBuilder>,
    IInputBase<IReactiveStructuredActivityBuilder>,
    IOutputBase<IReactiveStructuredActivityBuilder>,
    IExceptionHandlerBase<IReactiveStructuredActivityBuilder>,
    // INodeOptions<IReactiveStructuredActivityBuilder>,
    IStructuredActivityEvents<IReactiveStructuredActivityBuilder>,
    ISendEventBase<IReactiveStructuredActivityBuilder>,
    IPublishEventBase<IReactiveStructuredActivityBuilder>,
    IAcceptEventBase<IReactiveStructuredActivityBuilder>;

/// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
/// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
public interface IOverridenReactiveStructuredActivityExternalsBuilder :
    IObjectFlowBase<IOverridenReactiveStructuredActivityBuilder>,
    IControlFlowBase<IOverridenReactiveStructuredActivityBuilder>,
    INodeOptions<IOverridenReactiveStructuredActivityBuilder>;

/// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
/// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
public interface IOverridenReactiveStructuredActivityBuilder :
    IOverridenReactiveStructuredActivityExternalsBuilder,
    // IObjectFlowBase<IOverridenReactiveStructuredActivityBuilder>,
    // IControlFlowBase<IOverridenReactiveStructuredActivityBuilder>,
    IReactiveActivityBase<IOverridenReactiveStructuredActivityBuilder>,
    IReactiveActivityOverrides<IOverridenReactiveStructuredActivityBuilder>,
    IActivitySpecials<IOverridenReactiveStructuredActivityBuilder>,
    IInitialBase<IOverridenReactiveStructuredActivityBuilder>,
    IFinalBase<IOverridenReactiveStructuredActivityBuilder>,
    IInputBase<IOverridenReactiveStructuredActivityBuilder>,
    IOutputBase<IOverridenReactiveStructuredActivityBuilder>,
    IExceptionHandlerBase<IOverridenReactiveStructuredActivityBuilder>,
    // INodeOptions<IOverridenReactiveStructuredActivityBuilder>,
    IStructuredActivityEvents<IOverridenReactiveStructuredActivityBuilder>,
    ISendEventBase<IOverridenReactiveStructuredActivityBuilder>,
    ISendEventOverrides<IOverridenReactiveStructuredActivityBuilder>,
    IPublishEventBase<IOverridenReactiveStructuredActivityBuilder>,
    IPublishEventOverrides<IOverridenReactiveStructuredActivityBuilder>,
    IAcceptEventBase<IOverridenReactiveStructuredActivityBuilder>,
    IAcceptEventOverrides<IOverridenReactiveStructuredActivityBuilder>,
    IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilder>;