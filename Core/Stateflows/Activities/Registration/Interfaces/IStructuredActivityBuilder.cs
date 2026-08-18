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
        INodeOptions<IStructuredActivityBuilder>,
        IStructuredActivityEvents<IStructuredActivityBuilder>,
        ISendEventBase<IStructuredActivityBuilder>;

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
        INodeOptions<IOverridenStructuredActivityBuilder>,
        IStructuredActivityEvents<IOverridenStructuredActivityBuilder>,
        ISendEventBase<IOverridenStructuredActivityBuilder>,
        ISendEventOverrides<IOverridenStructuredActivityBuilder>,
        IActivitySpecialsOverrides<IOverridenStructuredActivityBuilder>;

}
