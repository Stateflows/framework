using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities
{
    public interface IDecisionBuilder<TToken> : IObjectFlow<TToken, IDecisionBuilder<TToken>>, IElseObjectFlow<TToken, IDecisionBuilder<TToken>>
        where TToken : Token, new()
    { }

    public interface IDecisionBuilder : IControlFlow<IDecisionBuilder>, IElseControlFlow<IDecisionBuilder>
    { }
}
