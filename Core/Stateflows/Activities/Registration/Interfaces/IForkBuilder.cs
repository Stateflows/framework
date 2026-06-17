using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IForkBuilder : IObjectFlowBase<IForkBuilder>, IControlFlowBase<IForkBuilder>;
    
    public interface IOverridenForkBuilder :
        IObjectFlowBase<IOverridenForkBuilder>,
        IOverridenObjectFlowBase<IOverridenForkBuilder>,
        IControlFlowBase<IOverridenForkBuilder>,
        IOverridenControlFlowBase<IOverridenForkBuilder>;
}
