using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    /// <summary>Configures a join node and its outgoing object and control flows.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.9.-Join-Node" />
    public interface IJoinBuilder : IObjectFlowBase, IControlFlowBase;
    
    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.9.-Join-Node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IOverridenJoinBuilder :
        IObjectFlowBase,
        IOverridenObjectFlowBase,
        IControlFlowBase,
        IOverridenControlFlowBase;
}