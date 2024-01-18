using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IReactiveStructuredActivityBuilder :
        IObjectFlow<IReactiveStructuredActivityBuilder>,
        IControlFlow<IReactiveStructuredActivityBuilder>,
        IReactiveActivity<IReactiveStructuredActivityBuilder>,
        IInitial<IReactiveStructuredActivityBuilder>,
        IFinal<IReactiveStructuredActivityBuilder>,
        IInput<IReactiveStructuredActivityBuilder>,
        IOutput<IReactiveStructuredActivityBuilder>,
        IExceptionHandler<IReactiveStructuredActivityBuilder>,
        INodeOptions<IReactiveStructuredActivityBuilderWithOptions>,
        IStructuredActivityEvents<IReactiveStructuredActivityBuilder>,
        ISendEvent<IReactiveStructuredActivityBuilder>,
        IAcceptEvent<IReactiveStructuredActivityBuilder>
    { }

    public interface IReactiveStructuredActivityBuilderWithOptions :
        IObjectFlow<IReactiveStructuredActivityBuilderWithOptions>,
        IControlFlow<IReactiveStructuredActivityBuilderWithOptions>,
        IReactiveActivity<IReactiveStructuredActivityBuilderWithOptions>,
        IInitial<IReactiveStructuredActivityBuilderWithOptions>,
        IFinal<IReactiveStructuredActivityBuilderWithOptions>,
        IInput<IReactiveStructuredActivityBuilderWithOptions>,
        IOutput<IReactiveStructuredActivityBuilderWithOptions>,
        IExceptionHandler<IReactiveStructuredActivityBuilderWithOptions>,
        IStructuredActivityEvents<IReactiveStructuredActivityBuilderWithOptions>,
        ISendEvent<IReactiveStructuredActivityBuilderWithOptions>,
        IAcceptEvent<IReactiveStructuredActivityBuilderWithOptions>
    { }


}
