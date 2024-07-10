using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities
{
    public interface IDecisionBuilder<TToken> : IDecisionFlowBase<TToken, IDecisionBuilder<TToken>>, IElseDecisionFlowBase<TToken, IDecisionBuilder<TToken>>
    { }

    public interface IDecisionBuilder : IDecisionFlowBase<IDecisionBuilder>, IElseDecisionFlowBase<IDecisionBuilder>
    { }
}
