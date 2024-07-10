using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IAcceptEventActionBuilder :
        IObjectFlowBase<IAcceptEventActionBuilder>,
        IControlFlowBase<IAcceptEventActionBuilder>,
        IExceptionHandlerBase<IAcceptEventActionBuilder>
    { }
}
