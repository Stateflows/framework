using System;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.System
{
    public interface ISystemEventHandler
    {
        Type EventType { get; }

        Task<EventStatus> TryHandleEventAsync<TEvent>(TEvent @event)
            where TEvent : Event, new();
    }
}
