﻿using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class BehaviorStatusRequestHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(BehaviorInfoRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            if (context.Event is BehaviorInfoRequest request)
            {
                var executor = context.Behavior.GetExecutor();

                request.Respond(new BehaviorInfo()
                {
                    Id = executor.Context.Id,
                    BehaviorStatus = executor.BehaviorStatus,
                    ExpectedEvents = executor.GetExpectedEventNames(),
                });

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
