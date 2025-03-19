using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Context.Interfaces;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class AcceptEventActionContext<TEvent> : BaseContext, IAcceptEventActionContext<TEvent>

    {
        IActivityContext IActivityActionContext.Activity => Activity;
        
        IBehaviorContext IBehaviorActionContext.Behavior => Activity;

        internal readonly ActionContext ActionContext;

        public AcceptEventActionContext(ActionContext actionContext)
            : base(actionContext)
        {
            ActionContext = actionContext;
            Event = default;

            if (Context.EventHolder is EventHolder<TEvent> holder)
            {
                Event = holder.Payload;
            }
            else
            {
                var @event = Context.EventHolder.BoxedPayload;

                var converter = typeof(TEvent).GetMethod("op_Implicit", new[] { @event.GetType() });

                if (converter != null)
                {
                    @event = converter.Invoke(null, new[] { @event });
                }

                Event = (TEvent)@event;
            }
        }

        public TEvent Event { get; private set; }

        public ICurrentNodeContext CurrentNode => ActionContext.CurrentNode;

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
