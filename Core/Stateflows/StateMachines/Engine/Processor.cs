using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Classes;
using System.Reflection;

namespace Stateflows.StateMachines.Engine
{
    internal class Processor : IEventProcessor
    {
        public string BehaviorType => Constants.StateMachine;

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

        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, TEvent @event, IServiceProvider serviceProvider, List<Exception> exceptions)
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

                try
                {
                    if (@event is CompoundRequest compoundRequest)
                    {
                        result = EventStatus.Consumed;
                        var results = new List<RequestResult>();
                        foreach (var ev in compoundRequest.Events)
                        {
                            ev.Headers.AddRange(@event.Headers);

                            executor.Context.SetEvent(ev);

                            executor.BeginScope();
                            try
                            {
                                var status = await ExecuteBehaviorAsync(ev, result, stateflowsContext, graph, executor);

                                results.Add(new RequestResult(ev, ev.GetResponse(), status, new EventValidation(true, new List<ValidationResult>())));
                            }
                            finally
                            {
                                executor.EndScope();

                                executor.Context.ClearEvent();
                            }
                        }

                        if (compoundRequest.Response == null)
                        {
                            compoundRequest.Respond(new CompoundResponse()
                            {
                                Results = results
                            });
                        }
                    }
                    else
                    {
                        executor.BeginScope();
                        try
                        {
                            result = await ExecuteBehaviorAsync(@event, result, stateflowsContext, graph, executor);
                        }
                        finally
                        {
                            executor.EndScope();
                        }
                    }
                }
                finally
                {
                    exceptions.AddRange(executor.Context.Exceptions);

                    await executor.DehydrateAsync();
                }

                // out of try-finally to make sure that context won't be saved when execution fails
                await storage.DehydrateAsync(executor.Context.Context);
            }

            return result;
        }

        private async Task<EventStatus> ExecuteBehaviorAsync<TEvent>(TEvent @event, EventStatus result, StateflowsContext stateflowsContext, Graph graph, Executor executor) where TEvent : Event, new()
        {
            var eventContext = new EventContext<TEvent>(executor.Context);

            if (await executor.Inspector.BeforeProcessEventAsync(eventContext))
            {
                try
                {
                    var attributes = @event.GetType().GetCustomAttributes<NoImplicitInitializationAttribute>();
                    if (!executor.Initialized && !attributes.Any())
                    {
                        result = await executor.InitializeAsync(@event);
                    }

                    if (result != EventStatus.Initialized)
                    {
                        var handlingResult = await TryHandleEventAsync(eventContext);

                        if (executor.Initialized)
                        {
                            if (handlingResult != EventStatus.Consumed && handlingResult != EventStatus.NotInitialized)
                            {
                                result = await executor.ProcessAsync(@event);
                            }
                            else
                            {
                                result = handlingResult;
                            }
                        }
                        else
                        {
                            result = result == EventStatus.NotInitialized
                                ? EventStatus.NotInitialized
                                : attributes.Any()
                                    ? handlingResult
                                    : EventStatus.Rejected;
                        }
                    }
                }
                finally
                {
                    await executor.Inspector.AfterProcessEventAsync(eventContext);
                }
            }
            else
            {
                if (executor.Context.ForceStatus != null)
                {
                    result = (EventStatus)executor.Context.ForceStatus;
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
