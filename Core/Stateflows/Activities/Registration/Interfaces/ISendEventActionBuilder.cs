using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface ISendEventActionBuilder : IControlFlowBase<ISendEventActionBuilder>;
    
    public interface IOverridenSendEventActionBuilder :
        IControlFlowBase<IOverridenSendEventActionBuilder>,
        IOverridenControlFlowBase<IOverridenSendEventActionBuilder>;
}
