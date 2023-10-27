using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityEventHandler
    {
        Type EventType { get; }

        Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new();
    }
}
