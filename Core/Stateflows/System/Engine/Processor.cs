using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.System.Engine
{
    internal class Processor : IEventProcessor
    {
        public string BehaviorType => SystemBehavior.Id.Type;
        
        private IEnumerable<ISystemEventHandler> EventHandlers { get; }

        private IServiceProvider ServiceProvider { get; }

        public Processor(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider.CreateScope().ServiceProvider;
            EventHandlers = ServiceProvider.GetServices<ISystemEventHandler>();
        }

        private Task<EventStatus> TryHandleEventAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            var eventHandler = EventHandlers.FirstOrDefault(h => h.EventType.IsInstanceOfType(@event));

            return eventHandler != null
                ? eventHandler.TryHandleEventAsync(@event)
                : Task.FromResult(EventStatus.NotConsumed);
        }

        public Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, TEvent @event, IServiceProvider serviceProvider)
            where TEvent : Event, new()
            => TryHandleEventAsync(@event);
    }
}
