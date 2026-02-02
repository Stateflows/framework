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
        ISendEventBase<IStructuredActivityBuilder>
    { }

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
        ISendEventBase<IStructuredActivityBuilderWithOptions>
    { }


}
