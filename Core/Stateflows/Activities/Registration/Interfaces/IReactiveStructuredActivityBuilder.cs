using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities;

public interface IReactiveStructuredActivityExternalsBuilder :
    IObjectFlowBase<IReactiveStructuredActivityBuilder>,
    IControlFlowBase<IReactiveStructuredActivityBuilder>,
    INodeOptions<IReactiveStructuredActivityBuilder>;

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

public interface IOverridenReactiveStructuredActivityExternalsBuilder :
    IObjectFlowBase<IOverridenReactiveStructuredActivityBuilder>,
    IControlFlowBase<IOverridenReactiveStructuredActivityBuilder>,
    INodeOptions<IOverridenReactiveStructuredActivityBuilder>;

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