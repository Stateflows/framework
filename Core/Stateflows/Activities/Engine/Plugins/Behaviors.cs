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
                var headers = context.Headers
                    .Where(p => p.Value is not BehaviorEmbedding)
                    .ToDictionary();
                
                headers[nameof(NoForwarding)] = new NoForwarding();

                context.Behavior.Send(context.Event, headers);
            }
        }
    }
}