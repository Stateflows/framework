using Stateflows.Common;
using Stateflows.StateMachines.Models;

namespace Stateflows.StateMachines.Extensions
{
    internal static class EventExtensions
    {
        public static bool Triggers(this EventHolder eventHolder, Edge edge)
        {
            return
                edge.Trigger == eventHolder.Name &&
                (
                    !(eventHolder.BoxedPayload is TimeEvent timeEvent) || 
                    timeEvent.ConsumerSignature == edge.Signature
                );
        }
    }
}
