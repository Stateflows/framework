using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Models;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;
using Stateflows.Common.Extensions;

namespace Stateflows.Activities.Engine
{
    internal class Processor : IEventProcessor
    {
        string IEventProcessor.BehaviorType => BehaviorType.Activity;

        public readonly ActivitiesRegister Register;
        public readonly IEnumerable<IActivityEventHandler> EventHandlers;
        public readonly IServiceProvider ServiceProvider;
        public readonly MethodInfo Method;

        public Processor(
            ActivitiesRegister register,
            IEnumerable<IActivityEventHandler> eventHandlers,
            IServiceProvider serviceProvider
        )
        {
            Register = register;
            ServiceProvider = serviceProvider.CreateScope().ServiceProvider;
            EventHandlers = eventHandlers;
            Method = typeof(IActivityEventHandler).GetMethod(nameof(IActivityEventHandler.TryHandleEventAsync));
        }

        private Task<EventStatus> TryHandleEventAsync(EventContext<object> context)
        {
            var eventHandler = EventHandlers.FirstOrDefault(h => h.EventType.IsInstanceOfType(context.Event));
                        
            return eventHandler != null
                ? Method.InvokeAsync<EventStatus>(eventHandler.EventType, eventHandler, context)
                : Task.FromResult(EventStatus.NotConsumed);
        }

        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions)
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

                if (eventHolder is EventHolder<CompoundRequest> compoundRequestHolder)
                {
                    var compoundRequest = compoundRequestHolder.Payload;
                    result = EventStatus.Consumed;
                    var results = new List<RequestResult>();
                    foreach (var ev in compoundRequest.Events)
                    {
                        ev.Headers.AddRange(eventHolder.Headers);

                        var status = await ExecuteBehaviorAsyncMethod.InvokeAsync<EventStatus>(ev.PayloadType, this, ev, result, stateflowsContext, graph, executor, context);

                        results.Add(new RequestResult(
                            ev,
                            ev.GetResponseHolder(),
                            status,
                            new EventValidation(true, new List<ValidationResult>())
                        ));
                    }

                    if (!compoundRequest.IsRespondedTo())
                    {
                        compoundRequest.Respond(new CompoundResponse()
                        {
                            Results = results
                        });
                    }
                }
                else
                {
                    result = await ExecuteBehaviorAsync(eventHolder, result, stateflowsContext, graph, executor, context);
                }

                exceptions.AddRange(context.Exceptions);

                await storage.DehydrateAsync((await executor.DehydrateAsync()).Context);
            }

            return result;
        }

        private readonly MethodInfo ExecuteBehaviorAsyncMethod = typeof(Processor).GetMethod(nameof(ExecuteBehaviorAsync), BindingFlags.Instance | BindingFlags.NonPublic);

        private async Task<EventStatus> ExecuteBehaviorAsync<TEvent>(EventHolder<TEvent> eventHolder, EventStatus result, StateflowsContext stateflowsContext, Graph graph, Executor executor, RootContext context)
        {
            context.SetEvent(eventHolder);

            var eventContext = new EventContext<object>(context, executor.NodeScope);

            if (await executor.Inspector.BeforeProcessEventAsync(eventContext))
            {
                var attributes = eventHolder.PayloadType.GetCustomAttributes<NoImplicitInitializationAttribute>();
                if (!executor.Initialized && !attributes.Any())
                {
                    result = await executor.InitializeAsync(eventHolder);
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
                            result = await executor.ProcessAsync(eventHolder);
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
