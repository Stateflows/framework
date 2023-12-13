using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IIterativeActivityBuilder :
        IObjectFlow<IIterativeActivityBuilder>,
        IControlFlow<IIterativeActivityBuilder>,
        IActivity<IIterativeActivityBuilder>,
        IInitial<IIterativeActivityBuilder>,
        IFinal<IIterativeActivityBuilder>,
        IInput<IIterativeActivityBuilder>,
        IOutput<IIterativeActivityBuilder>,
        IExceptionHandler<IIterativeActivityBuilder>,
        INodeOptions<IIterativeActivityBuilderWithOptions>,
        IStructuredActivityEvents<IIterativeActivityBuilder>
    { }

    public interface IIterativeActivityBuilderWithOptions :
        IObjectFlow<IIterativeActivityBuilderWithOptions>,
        IControlFlow<IIterativeActivityBuilderWithOptions>,
        IActivity<IIterativeActivityBuilderWithOptions>,
        IInitial<IIterativeActivityBuilderWithOptions>,
        IFinal<IIterativeActivityBuilderWithOptions>,
        IInput<IIterativeActivityBuilderWithOptions>,
        IOutput<IIterativeActivityBuilderWithOptions>,
        IExceptionHandler<IIterativeActivityBuilderWithOptions>,
        IStructuredActivityEvents<IIterativeActivityBuilderWithOptions>
    { }


}
