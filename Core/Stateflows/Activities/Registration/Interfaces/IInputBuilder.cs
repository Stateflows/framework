using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    /// <summary>Configures an input node and the typed object flows that receive its input tokens.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-input-node" />
    public interface IInputBuilder : IObjectFlowBase<IInputBuilder>;
    
    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-input-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenInputBuilder :
        IObjectFlowBase<IOverridenInputBuilder>,
        IOverridenObjectFlowBase<IOverridenInputBuilder>;
}
