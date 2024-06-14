using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class TokenFlowContext<TToken> : FlowContext,
        IGuardContext,
        ITransformationContext<TToken>,
        IRootContext
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        public TokenFlowContext(TokenPipelineContext context, TToken token)
            : base(context)
        {
            Token = token;
        }

        public TToken Token { get; private set; }
    }
}
