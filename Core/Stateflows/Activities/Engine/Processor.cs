using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Context;
using Stateflows.Activities.Models;
using Stateflows.Common.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Stateflows.Activities.Engine
{
    internal class Processor : IEventProcessor
    {
        string IEventProcessor.BehaviorType => BehaviorType.Activity;

        public readonly ActivitiesRegister Register;
        public readonly IEnumerable<IActivityEventHandler> EventHandlers;
        public readonly IServiceProvider ServiceProvider;

        public Processor(
            ActivitiesRegister register,
            IEnumerable<IActivityEventHandler> eventHandlers,
            IServiceProvider serviceProvider
        )
        {
            Register = register;
            ServiceProvider = serviceProvider.CreateScope().ServiceProvider;
            EventHandlers = eventHandlers;
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
                    if (@event is CompoundRequest compoundRequest)
                    {
                        result = EventStatus.Consumed;
                        var results = new List<RequestResult>();
                        foreach (var ev in compoundRequest.Events)
                        {
                            ev.Headers.AddRange(@event.Headers);

                            var status = await ExecuteBehaviorAsync(ev, result, stateflowsContext, graph, executor, context);

                            results.Add(new RequestResult(ev, ev.GetResponse(), status, new EventValidation(true, new List<ValidationResult>())));
                        }

                        compoundRequest.Respond(new CompoundResponse()
                        {
                            Results = results
                        });
                    }
                    else
                    {
                        result = await ExecuteBehaviorAsync(@event, result, stateflowsContext, graph, executor, context);
                    }

                    await storage.DehydrateAsync((await executor.DehydrateAsync()).Context);
                }
            }

            return result;
        }

        private async Task<EventStatus> ExecuteBehaviorAsync<TEvent>(TEvent @event, EventStatus result, StateflowsContext stateflowsContext, Graph graph, Executor executor, RootContext context) where TEvent : Event, new()
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

            stateflowsContext.Version = stateflowsContext.Status switch
            {
                BehaviorStatus.NotInitialized => stateflowsContext.Version,
                BehaviorStatus.Initialized => graph.Version,
                BehaviorStatus.Finalized => graph.Version,
                _ => 0
            };

            return result;
        }
    }
}
