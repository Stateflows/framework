using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IReactiveStructuredActivityBuilder :
        IObjectFlowBase<IReactiveStructuredActivityBuilder>,
        IControlFlowBase<IReactiveStructuredActivityBuilder>,
        IReactiveActivity<IReactiveStructuredActivityBuilder>,
        IInitial<IReactiveStructuredActivityBuilder>,
        IFinal<IReactiveStructuredActivityBuilder>,
        IInput<IReactiveStructuredActivityBuilder>,
        IOutput<IReactiveStructuredActivityBuilder>,
        IExceptionHandlerBase<IReactiveStructuredActivityBuilder>,
        INodeOptions<IReactiveStructuredActivityBuilderWithOptions>,
        IStructuredActivityEvents<IReactiveStructuredActivityBuilder>,
        ISendEvent<IReactiveStructuredActivityBuilder>,
        IAcceptEvent<IReactiveStructuredActivityBuilder>
    { }

    public interface IReactiveStructuredActivityBuilderWithOptions :
        IObjectFlowBase<IReactiveStructuredActivityBuilderWithOptions>,
        IControlFlowBase<IReactiveStructuredActivityBuilderWithOptions>,
        IReactiveActivity<IReactiveStructuredActivityBuilderWithOptions>,
        IInitial<IReactiveStructuredActivityBuilderWithOptions>,
        IFinal<IReactiveStructuredActivityBuilderWithOptions>,
        IInput<IReactiveStructuredActivityBuilderWithOptions>,
        IOutput<IReactiveStructuredActivityBuilderWithOptions>,
        IExceptionHandlerBase<IReactiveStructuredActivityBuilderWithOptions>,
        IStructuredActivityEvents<IReactiveStructuredActivityBuilderWithOptions>,
        ISendEvent<IReactiveStructuredActivityBuilderWithOptions>,
        IAcceptEvent<IReactiveStructuredActivityBuilderWithOptions>
    { }


}
