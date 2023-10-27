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
        public string BehaviorType => nameof(Activity);

        public ActivitiesRegister Register { get; }
        public IStateflowsStorage Storage { get; }
        public Dictionary<Type, IActivityEventHandler> EventHandlers { get; }
        public Dictionary<BehaviorId, Executor> Executors { get; } = new Dictionary<BehaviorId, Executor>();
        public IServiceProvider ServiceProvider { get; }

        public Processor(
            ActivitiesRegister register,
            IEnumerable<IActivityEventHandler> eventHandlers,
            IServiceProvider serviceProvider
        )
        {
            Register = register;
            EventHandlers = eventHandlers.ToDictionary(h => h.EventType, h => h);
            ServiceProvider = serviceProvider.CreateScope().ServiceProvider;
            Storage = ServiceProvider.GetRequiredService<IStateflowsStorage>();
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

            if (!Register.Activities.TryGetValue(id.Name, out var graph))
            {
                return result;
            }
            Executor executor = null;
            RootContext context = null;
            lock (Executors)
            {
                if (Executors.TryGetValue(id, out executor))
                {
                    context = executor.Context;
                }
                else
                {
                    executor = new Executor(Register, graph, ServiceProvider);
                    Executors.Add(id, executor);
                }
            }

            if (context is null)
            {
                context = new RootContext(await Storage.Hydrate(id));
                await executor.HydrateAsync(context);
            }

            var eventContext = new EventContext<TEvent>(context, executor.NodeScope, @event);

            if (await executor.Observer.BeforeProcessEventAsync(eventContext))
            {
                result = await TryHandleEventAsync(eventContext);

                if (result != EventStatus.Consumed)
                {
                    result = await executor.ProcessAsync(@event);
                }

                await executor.Observer.AfterProcessEventAsync(eventContext);
            }

            //await Storage.Dehydrate((await executor.DehydrateAsync()).Context);

            return result;
        }
    }
}
