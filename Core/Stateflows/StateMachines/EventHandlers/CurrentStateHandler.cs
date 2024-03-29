﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class CurrentStateHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(CurrentStateRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is CurrentStateRequest request)
            {
                var executor = context.StateMachine.GetExecutor();

                var response = new CurrentStateResponse()
                {
                    StatesStack = executor.GetStateStack(),
                    ExpectedEvents = executor.GetExpectedEvents()
                        .Where(type => !type.IsSubclassOf(typeof(TimeEvent)))
                        .Where(type => type != typeof(CompletionEvent))
                        .Select(type => type.GetEventName())
                        .ToArray(),
                    BehaviorStatus = executor.BehaviorStatus
                };

                request.Respond(response);

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
