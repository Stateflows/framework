using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities
{
    public interface IDecisionBuilder<TToken> : IDecisionFlow<TToken, IDecisionBuilder<TToken>>, IElseDecisionFlow<TToken, IDecisionBuilder<TToken>>
    { }

    public interface IDecisionBuilder : IDecisionFlow<IDecisionBuilder>, IElseDecisionFlow<IDecisionBuilder>
    { }
}
