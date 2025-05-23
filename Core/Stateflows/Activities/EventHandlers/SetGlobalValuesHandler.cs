﻿using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class SetGlobalValuesHandler : IActivityEventHandler
    {
        public Type EventType => typeof(SetGlobalValues);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)

        {
            if (context.Event is SetGlobalValues @event)
            {
                var values = context.Behavior.Values as ContextValuesCollection;
                
                values.Values.Clear();
                foreach (var entry in @event.Values)
                {
                    values.Values[entry.Key] = entry.Value;
                }

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
