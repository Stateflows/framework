using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IMergeBuilder : IObjectFlowBase, IControlFlowBase;
    
    public interface IOverridenMergeBuilder :
        // IObjectFlowBase,
        IOverridenObjectFlowBase,
        // IControlFlowBase,
        IOverridenControlFlowBase;
}
