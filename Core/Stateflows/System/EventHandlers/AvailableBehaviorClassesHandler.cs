using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.System.EventHandlers
{
    internal class AvailableBehaviorClassesHandler : ISystemEventHandler
    {
        public IBehaviorClassesProvider BehaviorClassesProvider { get; }

        public AvailableBehaviorClassesHandler(IServiceProvider serviceProvider)
        {
            BehaviorClassesProvider = serviceProvider.GetRequiredService<IBehaviorClassesProvider>();
        }

        public Type EventType => typeof(AvailableBehaviorClassesRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            if (@event is AvailableBehaviorClassesRequest request)
            {
                request.Respond(new AvailableBehaviorClassesResponse() { AvailableBehaviorClasses = BehaviorClassesProvider.AllBehaviorClasses });

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
