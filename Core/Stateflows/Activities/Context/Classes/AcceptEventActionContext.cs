using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class AcceptEventActionContext<TEvent> : ActionContext, IAcceptEventActionContext<TEvent>
    {
        IBehaviorContext IBehaviorActionContext.Behavior => Activity;

        private readonly ActionContext ActionContext;

        public AcceptEventActionContext(ActionContext actionContext)
            : base(actionContext.Context, actionContext.NodeScope, actionContext.Node, actionContext.InputTokens)
        {
            ActionContext = actionContext;
            Event = default;
            OutputTokens = actionContext.OutputTokens;
            InputTokens = actionContext.InputTokens;

            if (Context.EventHolder is EventHolder<TEvent> holder)
            {
                Event = holder.Payload;
            }
            else
            {
                var @event = Context.EventHolder.BoxedPayload;

                var converter = typeof(TEvent).GetMethod("op_Implicit", [ @event.GetType() ]);

                if (converter != null)
                {
                    @event = converter.Invoke(null, [ @event ]);
                }

                Event = (TEvent)@event;
            }
        }

        public TEvent Event { get; private set; }

        ICurrentNodeContext IActivityNodeContext.Node => ((IActivityNodeContext)ActionContext).Node;

        public IEnumerable<object> Input => ActionContext.InputTokens;
    }
}
