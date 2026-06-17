using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IInputBuilder : IObjectFlowBase<IInputBuilder>;
    
    public interface IOverridenInputBuilder :
        IObjectFlowBase<IOverridenInputBuilder>,
        IOverridenObjectFlowBase<IOverridenInputBuilder>;
}
