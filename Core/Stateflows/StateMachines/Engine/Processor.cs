using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines.Engine
{
    internal class Processor : IEventProcessor
    {
        public string BehaviorType => nameof(StateMachine);

        public StateMachinesRegister Register { get; }
        public IStateflowsStorage Storage { get; }
        public IEnumerable<IStateMachineEventHandler> EventHandlers { get; }
        public IServiceProvider ServiceProvider { get; }

        public Processor(
            StateMachinesRegister register,
            IEnumerable<IStateMachineEventHandler> eventHandlers,
            IServiceProvider serviceProvider
        )
        {
            Register = register;
            ServiceProvider = serviceProvider.CreateScope().ServiceProvider;
            EventHandlers = eventHandlers;
            Storage = ServiceProvider.GetRequiredService<IStateflowsStorage>();
        }

        private Task<EventStatus> TryHandleEventAsync<TEvent>(EventContext<TEvent> context)
            where TEvent : Event, new()
        {
            var eventHandler = EventHandlers.FirstOrDefault(h => h.EventType.IsInstanceOfType(context.Event));

            return eventHandler != null
                ? eventHandler.TryHandleEventAsync(context)
                : Task.FromResult(EventStatus.NotConsumed);
        }

        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, TEvent @event, IServiceProvider serviceProvider)
            where TEvent : Event, new()
        {
            var result = EventStatus.Undelivered;

            var stateflowsContext = await Storage.Hydrate(id);

            var key = stateflowsContext.Version != 0
                ? $"{id.Name}.{stateflowsContext.Version}"
                : $"{id.Name}.current";

            if (!Register.StateMachines.TryGetValue(key, out var graph))
            {
                return result;
            }

            using (var executor = new Executor(Register, graph, ServiceProvider))
            {
                var context = new RootContext(stateflowsContext);

                if (await executor.HydrateAsync(context))
                {
                    context.SetEvent(@event);

                    var eventContext = new EventContext<TEvent>(context);

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

                    await Storage.Dehydrate((await executor.DehydrateAsync()).Context);

                    context.ClearEvent();
                }
            }

            return result;
        }
    }
}
