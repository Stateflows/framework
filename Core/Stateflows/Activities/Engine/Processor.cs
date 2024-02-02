using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Activities.Engine
{
    internal class Processor : IEventProcessor
    {
        string IEventProcessor.BehaviorType => BehaviorType.Activity;

        public readonly ActivitiesRegister Register;
        public readonly Dictionary<Type, IActivityEventHandler> EventHandlers;
        public readonly IServiceProvider ServiceProvider;

        public Processor(
            ActivitiesRegister register,
            IEnumerable<IActivityEventHandler> eventHandlers,
            IServiceProvider serviceProvider
        )
        {
            Register = register;
            EventHandlers = eventHandlers.ToDictionary(h => h.EventType, h => h);
            ServiceProvider = serviceProvider.CreateScope().ServiceProvider;
        }

        private async Task<EventStatus> TryHandleEventAsync<TEvent>(EventContext<TEvent> context)
            where TEvent : Event, new()
            => EventHandlers.TryGetValue(context.Event.GetType(), out var eventHandler)
                ? await eventHandler.TryHandleEventAsync(context)
                : EventStatus.NotConsumed;

        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, TEvent @event, IServiceProvider serviceProvider)
            where TEvent : Event, new()
        {
            var result = EventStatus.Undelivered;

            var storage = ServiceProvider.GetRequiredService<IStateflowsStorage>();

            var stateflowsContext = await storage.HydrateAsync(id);

            var key = stateflowsContext.Version != 0
                ? $"{id.Name}.{stateflowsContext.Version}"
                : $"{id.Name}.current";

            if (!Register.Activities.TryGetValue(key, out var graph))
            {
                return result;
            }

            using (var executor = new Executor(Register, graph, ServiceProvider))
            {
                var context = new RootContext(stateflowsContext);

                if (await executor.HydrateAsync(context))
                {
                    context.SetEvent(@event);

                    var eventContext = new EventContext<TEvent>(context, executor.NodeScope);

                    if (await executor.Inspector.BeforeProcessEventAsync(eventContext))
                    {
                        result = await TryHandleEventAsync(eventContext);

                        if (result != EventStatus.Consumed)
                        {
                            result = await executor.ProcessAsync(@event);
                        }

                        await executor.Inspector.AfterProcessEventAsync(eventContext);
                    }
                    else
                    {
                        if (executor.Context.ForceConsumed)
                        {
                            result = EventStatus.Consumed;
                        }
                    }

                    stateflowsContext.Status = executor.BehaviorStatus;

                    stateflowsContext.LastExecutedAt = DateTime.Now;

                    var statuses = new BehaviorStatus[] { BehaviorStatus.Initialized, BehaviorStatus.Finalized };

                    if (statuses.Contains(stateflowsContext.Status))
                    {
                        stateflowsContext.Version = graph.Version;
                    }
                    else
                    {
                        if (stateflowsContext.Status == BehaviorStatus.NotInitialized)
                        {
                            stateflowsContext.Version = 0;
                        }
                    }

                    await storage.DehydrateAsync((await executor.DehydrateAsync()).Context);
                }
            }

            return result;
        }
    }
}
