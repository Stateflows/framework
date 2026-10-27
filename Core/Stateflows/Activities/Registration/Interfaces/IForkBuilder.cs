using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    /// <summary>Configures a fork node and the outgoing object and control flows it activates.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-fork-node" />
    public interface IForkBuilder : IObjectFlowBase<IForkBuilder>, IControlFlowBase<IForkBuilder>;
    
    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-fork-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenForkBuilder :
        IObjectFlowBase<IOverridenForkBuilder>,
        IOverridenObjectFlowBase<IOverridenForkBuilder>,
        IControlFlowBase<IOverridenForkBuilder>,
        IOverridenControlFlowBase<IOverridenForkBuilder>;
}
