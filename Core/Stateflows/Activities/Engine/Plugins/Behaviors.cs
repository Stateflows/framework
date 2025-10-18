using System.Linq;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;
using Stateflows.StateMachines;

namespace Stateflows.Activities.Engine;

internal class Behaviors : ActivityPlugin
{
    public override void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
    {
        if (context.Behavior.IsEmbedded)
        {
            if (eventStatus == EventStatus.NotConsumed)
            {
                var headers = context.Headers.ToList();
                if (!headers.Any(h => h is NoForwarding))
                {
                    headers.Add(new NoForwarding());
                }

                context.Behavior.Send(context.Event, headers);
            }

            if (
                context.Behavior.Id.Type == BehaviorType.StateMachine && 
                eventStatus is EventStatus.Consumed or EventStatus.Initialized
            )
            {
                context.Behavior.Send(new Completion());
            }
        }
    }
}