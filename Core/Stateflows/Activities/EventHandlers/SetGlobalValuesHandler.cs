using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class SetGlobalValuesHandler : IActivityEventHandler
    {
        public Type EventType => typeof(SetGlobalValues);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is SetGlobalValues @event)
            {
                var values = context.Activity.Values as ContextValuesCollection;
                
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
