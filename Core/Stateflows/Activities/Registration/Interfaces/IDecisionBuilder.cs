using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public interface IDecisionBuilder<TToken> : IDecisionFlow<TToken, IDecisionBuilder<TToken>>, IElseDecisionFlow<TToken, IDecisionBuilder<TToken>>
        where TToken : Token, new()
    { }

    public interface IDecisionBuilder : IDecisionFlow<IDecisionBuilder>, IElseDecisionFlow<IDecisionBuilder>
    { }
}
