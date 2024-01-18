using Stateflows.Common;
using Stateflows.StateMachines.Models;

namespace Stateflows.StateMachines.Extensions
{
    internal static class EventExtensions
    {
        public static bool Triggers(this Event @event, Edge edge)
        {
            return
                edge.Trigger == @event.Name &&
                (
                    !(@event is TimeEvent timeEvent) || 
                    timeEvent.ConsumerIdentifier == edge.Identifier
                );
        }
    }
}
