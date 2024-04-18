using Stateflows.Common;
using Stateflows.StateMachines.Models;

namespace Stateflows.StateMachines.Extensions
{
    internal static class EventExtensions
    {
        public static bool Triggers(this object @event, Edge edge)
        {
            return
                edge.Trigger == @event.GetType().GetEventName() &&
                (
                    !(@event is TimeEvent timeEvent) || 
                    timeEvent.ConsumerSignature == edge.Signature
                );
        }
    }
}
