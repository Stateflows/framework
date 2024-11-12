using System;
using System.Linq;
using Stateflows.Common;
using Stateflows.StateMachines.Models;

namespace Stateflows.StateMachines.Extensions
{
    internal static class EventExtensions
    {
        public static bool Triggers(this EventHolder eventHolder, Edge edge)
            => eventHolder.PayloadType.IsSubclassOf(typeof(Exception))
                ? edge.ActualTriggerTypes.Any(type => eventHolder.PayloadType.IsSubclassOf(type))
                : edge.ActualTriggers.Contains(eventHolder.Name) &&
                    (
                        !(eventHolder.BoxedPayload is TimeEvent timeEvent) || 
                        timeEvent.ConsumerSignature == edge.Signature
                    );
    }
}
