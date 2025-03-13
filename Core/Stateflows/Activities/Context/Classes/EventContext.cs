using Stateflows.Common.Interfaces;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class EventContext<TEvent> : BaseContext, Interfaces.IEventContext<TEvent>, IRootContext, IStateflowsEventContext<TEvent>
    {
        IActivityContext IActivityActionContext.Activity => Activity;
        
        IBehaviorContext IBehaviorActionContext.Behavior => Activity;

        public EventContext(RootContext context, NodeScope nodeScope)
            : base(context, nodeScope)
        {
            Event = default;

            if (context.EventHolder is EventHolder<TEvent> holder)
            {
                Event = holder.Payload;
            }
            else
            {
                if (ImplicitConverter.TryConvert<TEvent>(context.EventHolder.BoxedPayload, out var @event))
                {
                    Event = @event;
                }
                else
                {
                    throw new StateflowsRuntimeException($"Failed to convert event of type {context.EventHolder.BoxedPayload.GetType()} to {typeof(TEvent)}");
                }
            }
        }

        public TEvent Event { get; private set; }
    }
}
