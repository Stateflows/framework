using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces;

public interface IReactiveStructuredActivityBuilder :
    IObjectFlowBase<IReactiveStructuredActivityBuilder>,
    IControlFlowBase<IReactiveStructuredActivityBuilder>,
    IReactiveActivityBase<IReactiveStructuredActivityBuilder>,
    IActivitySpecials<IReactiveStructuredActivityBuilder>,
    IInitialBase<IReactiveStructuredActivityBuilder>,
    IFinalBase<IReactiveStructuredActivityBuilder>,
    IInputBase<IReactiveStructuredActivityBuilder>,
    IOutputBase<IReactiveStructuredActivityBuilder>,
    IExceptionHandlerBase<IReactiveStructuredActivityBuilder>,
    INodeOptions<IReactiveStructuredActivityBuilderWithOptions>,
    IStructuredActivityEvents<IReactiveStructuredActivityBuilder>,
    ISendEventBase<IReactiveStructuredActivityBuilder>,
    IPublishEventBase<IReactiveStructuredActivityBuilder>,
    IAcceptEventBase<IReactiveStructuredActivityBuilder>;

public interface IReactiveStructuredActivityBuilderWithOptions :
    IObjectFlowBase<IReactiveStructuredActivityBuilderWithOptions>,
    IControlFlowBase<IReactiveStructuredActivityBuilderWithOptions>,
    IReactiveActivityBase<IReactiveStructuredActivityBuilderWithOptions>,
    IActivitySpecials<IReactiveStructuredActivityBuilderWithOptions>,
    IInitialBase<IReactiveStructuredActivityBuilderWithOptions>,
    IFinalBase<IReactiveStructuredActivityBuilderWithOptions>,
    IInputBase<IReactiveStructuredActivityBuilderWithOptions>,
    IOutputBase<IReactiveStructuredActivityBuilderWithOptions>,
    IExceptionHandlerBase<IReactiveStructuredActivityBuilderWithOptions>,
    IStructuredActivityEvents<IReactiveStructuredActivityBuilderWithOptions>,
    ISendEventBase<IReactiveStructuredActivityBuilderWithOptions>,
    IPublishEventBase<IReactiveStructuredActivityBuilderWithOptions>,
    IAcceptEventBase<IReactiveStructuredActivityBuilderWithOptions>;

public interface IOverridenReactiveStructuredActivityBuilder :
    IObjectFlowBase<IOverridenReactiveStructuredActivityBuilder>,
    IControlFlowBase<IOverridenReactiveStructuredActivityBuilder>,
    IReactiveActivityBase<IOverridenReactiveStructuredActivityBuilder>,
    IReactiveActivityOverrides<IOverridenReactiveStructuredActivityBuilder>,
    IActivitySpecials<IOverridenReactiveStructuredActivityBuilder>,
    IInitialBase<IOverridenReactiveStructuredActivityBuilder>,
    IFinalBase<IOverridenReactiveStructuredActivityBuilder>,
    IInputBase<IOverridenReactiveStructuredActivityBuilder>,
    IOutputBase<IOverridenReactiveStructuredActivityBuilder>,
    IExceptionHandlerBase<IOverridenReactiveStructuredActivityBuilder>,
    INodeOptions<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IStructuredActivityEvents<IOverridenReactiveStructuredActivityBuilder>,
    ISendEventBase<IOverridenReactiveStructuredActivityBuilder>,
    ISendEventOverrides<IOverridenReactiveStructuredActivityBuilder>,
    IPublishEventBase<IOverridenReactiveStructuredActivityBuilder>,
    IPublishEventOverrides<IOverridenReactiveStructuredActivityBuilder>,
    IAcceptEventBase<IOverridenReactiveStructuredActivityBuilder>,
    IAcceptEventOverrides<IOverridenReactiveStructuredActivityBuilder>,
    IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilder>;

public interface IOverridenReactiveStructuredActivityBuilderWithOptions :
    IObjectFlowBase<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IControlFlowBase<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IReactiveActivityBase<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IReactiveActivityOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IActivitySpecials<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IInitialBase<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IFinalBase<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IInputBase<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IOutputBase<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IExceptionHandlerBase<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IStructuredActivityEvents<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    ISendEventBase<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    ISendEventOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IPublishEventBase<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IPublishEventOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IAcceptEventBase<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IAcceptEventOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>,
    IActivitySpecialsOverrides<IOverridenReactiveStructuredActivityBuilderWithOptions>;