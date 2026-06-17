using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IStructuredActivityBuilder :
        IObjectFlowBase<IStructuredActivityBuilder>,
        IControlFlowBase<IStructuredActivityBuilder>,
        IActivityBase<IStructuredActivityBuilder>,
        IActivitySpecials<IStructuredActivityBuilder>,
        IInitialBase<IStructuredActivityBuilder>,
        IFinalBase<IStructuredActivityBuilder>,
        IInputBase<IStructuredActivityBuilder>,
        IOutputBase<IStructuredActivityBuilder>,
        IExceptionHandlerBase<IStructuredActivityBuilder>,
        INodeOptions<IStructuredActivityBuilderWithOptions>,
        IStructuredActivityEvents<IStructuredActivityBuilder>,
        ISendEventBase<IStructuredActivityBuilder>;

    public interface IStructuredActivityBuilderWithOptions :
        IObjectFlowBase<IStructuredActivityBuilderWithOptions>,
        IControlFlowBase<IStructuredActivityBuilderWithOptions>,
        IActivityBase<IStructuredActivityBuilderWithOptions>,
        IActivitySpecials<IStructuredActivityBuilderWithOptions>,
        IInitialBase<IStructuredActivityBuilderWithOptions>,
        IFinalBase<IStructuredActivityBuilderWithOptions>,
        IInputBase<IStructuredActivityBuilderWithOptions>,
        IOutputBase<IStructuredActivityBuilderWithOptions>,
        IExceptionHandlerBase<IStructuredActivityBuilderWithOptions>,
        IStructuredActivityEvents<IStructuredActivityBuilderWithOptions>,
        ISendEventBase<IStructuredActivityBuilderWithOptions>;

    public interface IOverridenStructuredActivityBuilder :
        IObjectFlowBase<IOverridenStructuredActivityBuilder>,
        IControlFlowBase<IOverridenStructuredActivityBuilder>,
        IActivityBase<IOverridenStructuredActivityBuilder>,
        IActivityOverrides<IOverridenStructuredActivityBuilder>,
        IActivitySpecials<IOverridenStructuredActivityBuilder>,
        IInitialBase<IOverridenStructuredActivityBuilder>,
        IFinalBase<IOverridenStructuredActivityBuilder>,
        IInputBase<IOverridenStructuredActivityBuilder>,
        IOutputBase<IOverridenStructuredActivityBuilder>,
        IExceptionHandlerBase<IOverridenStructuredActivityBuilder>,
        INodeOptions<IOverridenStructuredActivityBuilderWithOptions>,
        IStructuredActivityEvents<IOverridenStructuredActivityBuilder>,
        ISendEventBase<IOverridenStructuredActivityBuilder>,
        ISendEventOverrides<IOverridenStructuredActivityBuilder>,
        IActivitySpecialsOverrides<IOverridenStructuredActivityBuilder>;

    public interface IOverridenStructuredActivityBuilderWithOptions :
        IObjectFlowBase<IOverridenStructuredActivityBuilderWithOptions>,
        IControlFlowBase<IOverridenStructuredActivityBuilderWithOptions>,
        IActivityBase<IOverridenStructuredActivityBuilderWithOptions>,
        IActivityOverrides<IOverridenStructuredActivityBuilderWithOptions>,
        IActivitySpecials<IOverridenStructuredActivityBuilderWithOptions>,
        IInitialBase<IOverridenStructuredActivityBuilderWithOptions>,
        IFinalBase<IOverridenStructuredActivityBuilderWithOptions>,
        IInputBase<IOverridenStructuredActivityBuilderWithOptions>,
        IOutputBase<IOverridenStructuredActivityBuilderWithOptions>,
        IExceptionHandlerBase<IOverridenStructuredActivityBuilderWithOptions>,
        IStructuredActivityEvents<IOverridenStructuredActivityBuilderWithOptions>,
        ISendEventBase<IOverridenStructuredActivityBuilderWithOptions>,
        IActivitySpecialsOverrides<IOverridenStructuredActivityBuilder>;

}
