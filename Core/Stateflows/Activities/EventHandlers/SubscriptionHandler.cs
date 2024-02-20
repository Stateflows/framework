﻿using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class SubscriptionHandler : IActivityEventHandler
    {
        public Type EventType => typeof(SubscriptionRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is SubscriptionRequest request)
            {
                var result = context.Activity.GetExecutor().Context.Context.AddSubscriber(request.BehaviorId, request.EventName);

                request.Respond(new SubscriptionResponse() { SubscriptionSuccessful = result });

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
