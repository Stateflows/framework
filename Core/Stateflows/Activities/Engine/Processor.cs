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
            try
            {
                var result = EventStatus.Undelivered;

                var stateflowsContext = await Storage.Hydrate(id);

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

                        await Storage.Dehydrate((await executor.DehydrateAsync()).Context);

                        context.ClearEvent();
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }

            //Executor executor = null;
            //RootContext context = null;
            //lock (Executors)
            //{
            //    if (Executors.TryGetValue(id, out executor))
            //    {
            //        context = executor.Context;
            //    }
            //    else
            //    {
            //        executor = new Executor(Register, graph, ServiceProvider);
            //        Executors.Add(id, executor);
            //    }
            //}

            //if (context is null)
            //{
            //    context = new RootContext(await Storage.Hydrate(id));
            //    await executor.HydrateAsync(context);
            //}

            //var eventContext = new EventContext<TEvent>(context, executor.NodeScope, @event);

            //if (await executor.Inspector.BeforeProcessEventAsync(eventContext))
            //{
            //    result = await TryHandleEventAsync(eventContext);

            //    if (result != EventStatus.Consumed)
            //    {
            //        result = await executor.ProcessAsync(@event);
            //    }

            //    await executor.Inspector.AfterProcessEventAsync(eventContext);
            //}

            ////await Storage.Dehydrate((await executor.DehydrateAsync()).Context);

            //return result;
        }
    }
}
