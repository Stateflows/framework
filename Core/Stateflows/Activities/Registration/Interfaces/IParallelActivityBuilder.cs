using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IParallelActivityBuilder :
        IObjectFlow<IParallelActivityBuilder>,
        IControlFlow<IParallelActivityBuilder>,
        IActivity<IParallelActivityBuilder>,
        IInitial<IParallelActivityBuilder>,
        IFinal<IParallelActivityBuilder>,
        IInput<IParallelActivityBuilder>,
        IOutput<IParallelActivityBuilder>,
        IExceptionHandler<IParallelActivityBuilder>,
        INodeOptions<IParallelActivityBuilderWithOptions>,
        IStructuredActivityEvents<IParallelActivityBuilder>
    { }

    public interface IParallelActivityBuilderWithOptions :
        IObjectFlow<IParallelActivityBuilderWithOptions>,
        IControlFlow<IParallelActivityBuilderWithOptions>,
        IActivity<IParallelActivityBuilderWithOptions>,
        IInitial<IParallelActivityBuilderWithOptions>,
        IFinal<IParallelActivityBuilderWithOptions>,
        IInput<IParallelActivityBuilderWithOptions>,
        IOutput<IParallelActivityBuilderWithOptions>,
        IExceptionHandler<IParallelActivityBuilderWithOptions>,
        IStructuredActivityEvents<IParallelActivityBuilderWithOptions>
    { }


}
