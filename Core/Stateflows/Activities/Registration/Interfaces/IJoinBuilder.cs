using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IJoinBuilder : IObjectFlowBase, IControlFlowBase;
    
    public interface IOverridenJoinBuilder :
        IObjectFlowBase,
        IOverridenObjectFlowBase,
        IControlFlowBase,
        IOverridenControlFlowBase;
}