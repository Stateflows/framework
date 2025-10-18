using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class TokenFlowContext<TToken> : FlowContext,
        IGuardContext,
        IGuardContext<TToken>,
        ITransformationContext<TToken>
    {
        public TokenFlowContext(TokenPipelineContext context, TToken token)
            : base(context)
        {
            Token = token;
        }

        public TToken Token { get; private set; }
    }

    internal class TokenFlowContext<TToken, TTransformedToken> : TokenFlowContext<TToken>,
        ITransformationContext<TToken, TTransformedToken>
    {
        public TokenFlowContext(TokenPipelineContext context, TToken token, TTransformedToken transformedToken) : base(context, token)
        {
            TransformedToken = transformedToken;
        }

        public TTransformedToken TransformedToken { get; private set; }
    }
}
