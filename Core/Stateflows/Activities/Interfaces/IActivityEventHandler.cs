using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityEventHandler
    {
        Type EventType { get; }

        Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context);
    }
}
