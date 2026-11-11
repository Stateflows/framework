using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    /// <summary>Configures a fork node and the outgoing object and control flows it activates.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.8.-Fork-Node" />
    public interface IForkBuilder : IObjectFlowBase<IForkBuilder>, IControlFlowBase<IForkBuilder>;
    
    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.8.-Fork-Node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IOverridenForkBuilder :
        IObjectFlowBase<IOverridenForkBuilder>,
        IOverridenObjectFlowBase<IOverridenForkBuilder>,
        IControlFlowBase<IOverridenForkBuilder>,
        IOverridenControlFlowBase<IOverridenForkBuilder>;
}
