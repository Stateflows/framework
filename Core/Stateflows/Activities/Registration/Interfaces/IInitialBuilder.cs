using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IInitialBuilder : IControlFlowBase<IInitialBuilder>;
    
    public interface IOverridenInitialBuilder :
        IControlFlowBase<IOverridenInitialBuilder>,
        IOverridenControlFlowBase<IOverridenInitialBuilder>;
}
