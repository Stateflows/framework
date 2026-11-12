using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    /// <summary>Configures an initial node and its outgoing control flow.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.4.-Initial-Node" />
    public interface IInitialBuilder : IControlFlowBase<IInitialBuilder>;
    
    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.4.-Initial-Node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IOverridenInitialBuilder :
        IControlFlowBase<IOverridenInitialBuilder>,
        IOverridenControlFlowBase<IOverridenInitialBuilder>;
}
