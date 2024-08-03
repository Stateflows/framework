using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IStructuredActivityBuilder :
        IObjectFlowBase<IStructuredActivityBuilder>,
        IControlFlowBase<IStructuredActivityBuilder>,
        IActivity<IStructuredActivityBuilder>,
        IInitial<IStructuredActivityBuilder>,
        IFinal<IStructuredActivityBuilder>,
        IInput<IStructuredActivityBuilder>,
        IOutput<IStructuredActivityBuilder>,
        IExceptionHandlerBase<IStructuredActivityBuilder>,
        INodeOptions<IStructuredActivityBuilderWithOptions>,
        IStructuredActivityEvents<IStructuredActivityBuilder>,
        ISendEvent<IStructuredActivityBuilder>
    { }

    public interface IStructuredActivityBuilderWithOptions :
        IObjectFlowBase<IStructuredActivityBuilderWithOptions>,
        IControlFlowBase<IStructuredActivityBuilderWithOptions>,
        IActivity<IStructuredActivityBuilderWithOptions>,
        IInitial<IStructuredActivityBuilderWithOptions>,
        IFinal<IStructuredActivityBuilderWithOptions>,
        IInput<IStructuredActivityBuilderWithOptions>,
        IOutput<IStructuredActivityBuilderWithOptions>,
        IExceptionHandlerBase<IStructuredActivityBuilderWithOptions>,
        IStructuredActivityEvents<IStructuredActivityBuilderWithOptions>,
        ISendEvent<IStructuredActivityBuilderWithOptions>
    { }


}
