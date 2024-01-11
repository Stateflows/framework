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
        public IBehaviorClassesProvider BehaviorClassesProvider { get; }

        public IStateflowsStorage Storage { get; }

        public BehaviorInstancesHandler(IServiceProvider serviceProvider)
        {
            BehaviorClassesProvider = serviceProvider.GetRequiredService<IBehaviorClassesProvider>();
            Storage = serviceProvider.GetRequiredService<IStateflowsStorage>();
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

                var contexts = await Storage.GetContexts(classes);

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
