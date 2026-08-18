using System.Linq;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

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

                if (context.TryGetParentBehaviorContext(out var parentBehaviorContext))
                {
                    parentBehaviorContext.Send(context.Event, headers);
                };
            }
        }
    }
}