using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.System.EventHandlers
{
    internal class BehaviorInstancesHandler : ISystemEventHandler
    {
        public readonly IBehaviorClassesProvider BehaviorClassesProvider;
        public readonly IServiceProvider ServiceProvider;

        public BehaviorInstancesHandler(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            BehaviorClassesProvider = serviceProvider.GetRequiredService<IBehaviorClassesProvider>();
        }

        public Type EventType => typeof(BehaviorInstancesRequest);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            if (@event is BehaviorInstancesRequest request)
            {
                var classes = request.BehaviorClasses;
                if (classes == null || !classes.Any())
                {
                    classes = BehaviorClassesProvider.AllBehaviorClasses;
                }

                var storage = ServiceProvider.GetRequiredService<IStateflowsStorage>();

                var contexts = await storage.GetContextsAsync(classes);

                request.Respond(new BehaviorInstancesResponse()
                    {
                        Behaviors = contexts.Select(c => new BehaviorDescriptor()
                            {
                                Id = c.Id,
                                Status = BehaviorStatus.Initialized
                            }
                        ).ToArray()
                    }
                );

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
