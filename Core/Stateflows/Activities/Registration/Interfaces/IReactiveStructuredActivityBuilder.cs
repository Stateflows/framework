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
    IAcceptEvent<IReactiveStructuredActivityBuilder>;

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
    IAcceptEvent<IReactiveStructuredActivityBuilderWithOptions>;