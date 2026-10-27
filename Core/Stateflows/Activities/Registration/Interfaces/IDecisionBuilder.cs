using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities
{
    /// <summary>Configures a decision node over object tokens of the specified type.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-decision-node" />
    public interface IDecisionBuilder<out TToken> :
        IDecisionFlowBase<TToken, IDecisionBuilder<TToken>>,
        IElseDecisionFlowBase<TToken, IDecisionBuilder<TToken>>;
    
    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-decision-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenDecisionBuilder<out TToken> :
        IDecisionFlowBase<TToken, IOverridenDecisionBuilder<TToken>>,
        IOverridenDecisionFlowBase<TToken, IOverridenDecisionBuilder<TToken>>,
        IElseDecisionFlowBase<TToken, IOverridenDecisionBuilder<TToken>>,
        IOverridenElseDecisionFlowBase<TToken, IOverridenDecisionBuilder<TToken>>;

    /// <summary>Configures a decision node over control flow without typed tokens.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-decision-node" />
    public interface IDecisionBuilder : 
        IDecisionFlowBase<IDecisionBuilder>,
        IElseDecisionFlowBase<IDecisionBuilder>;

    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-decision-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenDecisionBuilder : 
        IOverridenDecisionFlowBase<IOverridenDecisionBuilder>,
        IOverridenElseDecisionFlowBase<IOverridenDecisionBuilder>;
}
