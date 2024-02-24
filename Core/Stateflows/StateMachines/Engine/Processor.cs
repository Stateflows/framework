using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.Common.Context;
using Stateflows.StateMachines.Models;
using Stateflows.Common.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Stateflows.StateMachines.Engine
{
    internal class Processor : IEventProcessor
    {
        public string BehaviorType => nameof(StateMachine);

        public readonly StateMachinesRegister Register;
        public readonly IEnumerable<IStateMachineEventHandler> EventHandlers;
        public readonly IServiceProvider ServiceProvider;

        public Processor(
            StateMachinesRegister register,
            IEnumerable<IStateMachineEventHandler> eventHandlers,
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

            if (!Register.StateMachines.TryGetValue(key, out var graph))
            {
                return result;
            }

            using (var executor = new Executor(Register, graph, ServiceProvider, stateflowsContext, @event))
            {
                await executor.HydrateAsync();

                if (@event is CompoundRequest compoundRequest)
                {
                    result = EventStatus.Consumed;
                    var results = new List<RequestResult>();
                    foreach (var ev in compoundRequest.Events)
                    {
                        ev.Headers.AddRange(@event.Headers);

                        var status = await ExecuteBehaviorAsync(ev, result, stateflowsContext, graph, executor);

                        results.Add(new RequestResult(ev, ev.GetResponse(), status, new EventValidation(true, new List<ValidationResult>())));
                    }

                    compoundRequest.Respond(new CompoundResponse()
                    {
                         Results = results
                    });
                }
                else
                {
                    result = await ExecuteBehaviorAsync(@event, result, stateflowsContext, graph, executor);
                }

                await executor.DehydrateAsync();

                await storage.DehydrateAsync(executor.Context.Context);

                executor.Context.ClearEvent();
            }

            return result;
        }

        private async Task<EventStatus> ExecuteBehaviorAsync<TEvent>(TEvent @event, EventStatus result, StateflowsContext stateflowsContext, Graph graph, Executor executor) where TEvent : Event, new()
        {
            executor.RebuildVerticesStack();

            executor.Context.SetEvent(@event);

            var eventContext = new EventContext<TEvent>(executor.Context);

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
                BehaviorStatus.NotInitialized => 0,
                BehaviorStatus.Initialized => graph.Version,
                BehaviorStatus.Finalized => graph.Version,
                _ => 0
            };

            executor.Context.ClearEvent();

            return result;
        }
    }
}
