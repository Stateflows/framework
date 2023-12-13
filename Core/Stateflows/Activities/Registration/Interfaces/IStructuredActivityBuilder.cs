using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IStructuredActivityBuilder :
        IObjectFlow<IStructuredActivityBuilder>,
        IControlFlow<IStructuredActivityBuilder>,
        IActivity<IStructuredActivityBuilder>,
        IInitial<IStructuredActivityBuilder>,
        IFinal<IStructuredActivityBuilder>,
        IInput<IStructuredActivityBuilder>,
        IOutput<IStructuredActivityBuilder>,
        IExceptionHandler<IStructuredActivityBuilder>,
        INodeOptions<IStructuredActivityBuilderWithOptions>,
        IStructuredActivityEvents<IStructuredActivityBuilder>,
        ISendEvent<IStructuredActivityBuilder>,
        IAcceptEvent<IStructuredActivityBuilder>
    { }

    public interface IStructuredActivityBuilderWithOptions :
        IObjectFlow<IStructuredActivityBuilderWithOptions>,
        IControlFlow<IStructuredActivityBuilderWithOptions>,
        IActivity<IStructuredActivityBuilderWithOptions>,
        IInitial<IStructuredActivityBuilderWithOptions>,
        IFinal<IStructuredActivityBuilderWithOptions>,
        IInput<IStructuredActivityBuilderWithOptions>,
        IOutput<IStructuredActivityBuilderWithOptions>,
        IExceptionHandler<IStructuredActivityBuilderWithOptions>,
        IStructuredActivityEvents<IStructuredActivityBuilderWithOptions>,
        ISendEvent<IStructuredActivityBuilderWithOptions>,
        IAcceptEvent<IStructuredActivityBuilderWithOptions>
    { }


}
