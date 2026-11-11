using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    /// <summary>Configures a merge node and its outgoing object and control flows.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.3.-Merge-Node" />
    public interface IMergeBuilder : IObjectFlowBase, IControlFlowBase;
    
    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.3.-Merge-Node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IOverridenMergeBuilder :
        // IObjectFlowBase,
        IOverridenObjectFlowBase,
        // IControlFlowBase,
        IOverridenControlFlowBase;
}
