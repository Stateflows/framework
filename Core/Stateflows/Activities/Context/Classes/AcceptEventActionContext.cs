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

        public TEvent Event => Context.Event as TEvent;

        public INodeContext CurrentNode => ActionContext.CurrentNode;

        public IEnumerable<Token> Input => ActionContext.InputTokens;

        public void Output<TToken>(TToken token)
            where TToken : Token, new()
            => ActionContext.Output<TToken>(token);

        public void OutputRange<TToken>(IEnumerable<TToken> tokens)
            where TToken : Token, new()
            => ActionContext.OutputRange<TToken>(tokens);

        public void OutputRangeAsGroup<TToken>(IEnumerable<TToken> tokens)
            where TToken : Token, new()
            => ActionContext.OutputRangeAsGroup<TToken>(tokens);

        public void PassTokensOfTypeOn<TToken>()
            where TToken : Token, new()
            => ActionContext.PassTokensOfTypeOn<TToken>();

        public void PassTokensOfTypeOnAsGroup<TToken>()
            where TToken : Token, new()
            => ActionContext.PassTokensOfTypeOnAsGroup<TToken>();

        public void PassAllOn()
            => ActionContext.PassAllOn();
    }
}
