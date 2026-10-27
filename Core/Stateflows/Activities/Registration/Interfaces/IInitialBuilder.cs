using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    /// <summary>Configures an initial node and its outgoing control flow.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-initial-node" />
    public interface IInitialBuilder : IControlFlowBase<IInitialBuilder>;
    
    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-initial-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenInitialBuilder :
        IControlFlowBase<IOverridenInitialBuilder>,
        IOverridenControlFlowBase<IOverridenInitialBuilder>;
}
