using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities
{
    public interface IDecisionBuilder<out TToken> :
        IDecisionFlowBase<TToken, IDecisionBuilder<TToken>>,
        IElseDecisionFlowBase<TToken, IDecisionBuilder<TToken>>;
    
    public interface IOverridenDecisionBuilder<out TToken> :
        IDecisionFlowBase<TToken, IOverridenDecisionBuilder<TToken>>,
        IOverridenDecisionFlowBase<TToken, IOverridenDecisionBuilder<TToken>>,
        IElseDecisionFlowBase<TToken, IOverridenDecisionBuilder<TToken>>,
        IOverridenElseDecisionFlowBase<TToken, IOverridenDecisionBuilder<TToken>>;

    public interface IDecisionBuilder : 
        IDecisionFlowBase<IDecisionBuilder>,
        IElseDecisionFlowBase<IDecisionBuilder>;

    public interface IOverridenDecisionBuilder : 
        IOverridenDecisionFlowBase<IOverridenDecisionBuilder>,
        IOverridenElseDecisionFlowBase<IOverridenDecisionBuilder>;
}
