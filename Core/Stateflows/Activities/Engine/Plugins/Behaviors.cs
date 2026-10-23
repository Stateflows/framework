using System;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Engine;

internal class Behaviors : ActivityPlugin
{
    public override async Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
    {
        var status = EventStatus.Failed;
        try
        {
            return status = await next();
        }
        finally
        {
            if (context.Behavior.IsEmbedded)
            {
                if (status == EventStatus.NotConsumed)
                {
                    var headers = context.Headers
                        .Where(p => p.Value is not BehaviorEmbedding)
                        .ToDictionary();

                    var noBubblingAttribute = context.Event.GetType().GetCustomAttribute<NoBubblingAttribute>();
                    if (!headers.Values.Any(h => h is NoBubbling) && noBubblingAttribute == null)
                    {
                        headers[nameof(NoForwarding)] = new NoForwarding();

                        if (context.TryGetParentBehaviorContext(out var parentBehaviorContext))
                        {
                            parentBehaviorContext.Send(context.Event, headers);
                        }
                    }
                }
            }
        }

    }
}