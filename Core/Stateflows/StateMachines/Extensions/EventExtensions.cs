using System;
using System.Collections.Generic;
using System.Linq;
using Stateflows.Activities.Models;
using Stateflows.Common;
using Edge = Stateflows.StateMachines.Models.Edge;

namespace Stateflows.StateMachines.Extensions
{
    internal static class EventExtensions
    {
        public static bool Triggers(this EventHolder eventHolder, Edge edge)
        {
            var edgeGuardIdentifier = $"{edge.Identifier}.";
            var guardResponse = eventHolder.Headers.Values.OfType<TransitionGuardResponse>().FirstOrDefault();
            if (guardResponse != null && !guardResponse.GuardIdentifier.StartsWith(edgeGuardIdentifier))
            {
                return false;
            }
            if (edge.PayloadNames != null)
                return eventHolder.BoxedPayload is Payload && edge.PayloadNames.Contains(eventHolder.Name);
            
            return edge.PolymorphicTriggers
                ? eventHolder.BoxedPayload is TimeEvent timeEvent1
                    ? timeEvent1.ConsumerSignature == edge.Signature
                    : edge.ActualTriggerTypes.Any(type => type.IsAssignableFrom(eventHolder.PayloadType))
                : (edge.ActualTriggerTypes.Contains(eventHolder.PayloadType) ||
                   edge.ActualTriggerTypes.Any(type => typeof(Exception).IsAssignableFrom(type) && type.IsAssignableFrom(eventHolder.PayloadType))) &&
                  (
                      !(eventHolder.BoxedPayload is TimeEvent timeEvent2) ||
                      timeEvent2.ConsumerSignature == edge.Signature
                  );
        }

        public static bool IsAcceptedBy(this EventHolder eventHolder, Node node)
            => node.PayloadName != null
                ? eventHolder.BoxedPayload is Payload && eventHolder.Name == node.PayloadName
                : node.ActualEventTypes.Any(type => type.IsAssignableFrom(eventHolder.PayloadType));
        
        public static bool IsAcceptedBy(this EventHolder eventHolder, Node node, Dictionary<string, Guid> nodeTimeEvents)
            => eventHolder.BoxedPayload is TimeEvent timeEvent
                ? (
                    nodeTimeEvents.TryGetValue(node.Identifier, out var timeEventId) &&
                    timeEvent.Id == timeEventId
                )
                : eventHolder.IsAcceptedBy(node);
    }
}
