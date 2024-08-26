using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Utils;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Models;
using Stateflows.Activities.Events;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;

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

        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, TEvent @event, IServiceProvider serviceProvider, List<Exception> exceptions)
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

                await executor.HydrateAsync(context);

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
                    result = await ExecuteBehaviorAsync(@event, result, stateflowsContext, graph, executor, context);
                }

                exceptions.AddRange(context.Exceptions);

                await storage.DehydrateAsync((await executor.DehydrateAsync()).Context);
            }

            return result;
        }

        private async Task<EventStatus> ExecuteBehaviorAsync<TEvent>(EventHolder<TEvent> eventHolder, EventStatus result, StateflowsContext stateflowsContext, Graph graph, Executor executor, RootContext context)
        {
            context.SetEvent(eventHolder);

            var eventContext = new EventContext<TEvent>(context, executor.NodeScope);

            if (await executor.Inspector.BeforeProcessEventAsync(eventContext))
            {
                Event currentEvent = eventHolder;

                IEnumerable<TokenHolder> input = null;

                if (eventHolder is ExecutionRequest executionRequest)
                {
                    if (executor.Graph.Interactive || executor.BehaviorStatus != BehaviorStatus.NotInitialized)
                    {
                        return EventStatus.NotConsumed;
                    }

                    context.SetEvent(executionRequest.InitializationEvent);
                        
                    currentEvent = executionRequest.InitializationEvent;

                    input = executionRequest.InputTokens?.ToArray() ?? Array.Empty<TokenHolder>();
                }
                
                var attributes = currentEvent.GetType().GetCustomAttributes<NoImplicitInitializationAttribute>();
                if (!executor.Initialized && !attributes.Any())
                {
                    result = await executor.InitializeAsync(currentEvent, input);
                }

                if (result != EventStatus.Initialized)
                {
                    var handlingResult = await TryHandleEventAsync(eventContext);

                    if (executor.Initialized)
                    {
                        if (
                            handlingResult != EventStatus.Consumed &&
                            handlingResult != EventStatus.Rejected &&
                            handlingResult != EventStatus.NotInitialized
                        )
                        {
                            result = await executor.ProcessAsync(currentEvent);
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

                if (eventHolder is ExecutionRequest executionRequest2)
                {
                    context.ClearEvent();

                    executionRequest2.Respond(new ExecutionResponse() { OutputTokens = await executor.GetResultAsync() });
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
