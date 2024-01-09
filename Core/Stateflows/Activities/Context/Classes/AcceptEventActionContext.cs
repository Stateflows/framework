using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class AcceptEventActionContext<TEvent> : BaseContext, IAcceptEventActionContext<TEvent>
        where TEvent : Event, new()
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        internal readonly ActionContext ActionContext;

        public AcceptEventActionContext(ActionContext actionContext)
            : base(actionContext)
        {
            ActionContext = actionContext;
        }

        public TEvent @event = null;
        public TEvent Event
            => @event ??= InputTokens.OfType<EventToken<TEvent>>().First(t => t.Event is TEvent).Event;

        public INodeContext CurrentNode => ActionContext.CurrentNode;

        public IEnumerable<Token> InputTokens => ActionContext.InputTokens;

        public void OutputToken<TToken>(TToken token)
            where TToken : Token, new()
            => ActionContext.OutputToken<TToken>(token);

        public void OutputTokensRange<TToken>(IEnumerable<TToken> tokens)
            where TToken : Token, new()
            => ActionContext.OutputTokensRange<TToken>(tokens);

        public void OutputTokensRangeAsGroup<TToken>(IEnumerable<TToken> tokens)
            where TToken : Token, new()
            => ActionContext.OutputTokensRangeAsGroup<TToken>(tokens);

        public void PassTokensOfType<TToken>()
            where TToken : Token, new()
            => ActionContext.PassTokensOfType<TToken>();

        public void PassTokensOfTypeAsGroup<TToken>()
            where TToken : Token, new()
            => ActionContext.PassTokensOfTypeAsGroup<TToken>();

        public void PassAllTokens()
            => ActionContext.PassAllTokens();
    }
}
