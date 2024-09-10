using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class AcceptEventActionContext<TEvent> : BaseContext, IAcceptEventActionContext<TEvent>

    {
        IActivityContext IActivityActionContext.Activity => Activity;

        internal readonly ActionContext ActionContext;

        public AcceptEventActionContext(ActionContext actionContext)
            : base(actionContext)
        {
            ActionContext = actionContext;
        }

        public TEvent Event => (Context.EventHolder as EventHolder<TEvent>).Payload;

        public INodeContext CurrentNode => ActionContext.CurrentNode;

        public IEnumerable<object> Input => ActionContext.InputTokens;

        public void Output<TToken>(TToken token)
            => ActionContext.Output<TToken>(token);

        public void OutputRange<TToken>(IEnumerable<TToken> tokens)
            => ActionContext.OutputRange<TToken>(tokens);

        public void PassTokensOfTypeOn<TToken>()
            => ActionContext.PassTokensOfTypeOn<TToken>();

        public void PassAllTokensOn()
            => ActionContext.PassAllTokensOn();

        public IEnumerable<TToken> GetTokensOfType<TToken>()
            => ActionContext.GetTokensOfType<TToken>();
    }
}
