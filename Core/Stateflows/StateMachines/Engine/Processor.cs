using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines.Engine
{
    internal class Processor : IEventProcessor, IStateflowsProcessor
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
            ServiceProvider = serviceProvider;
            EventHandlers = eventHandlers;
        }

        private Task<EventStatus> TryHandleEventAsync<TEvent>(EventContext<TEvent> context)
        {
            var eventHandler = EventHandlers.FirstOrDefault(h => h.EventType.IsInstanceOfType(context.Event));

            return eventHandler != null
                ? eventHandler.TryHandleEventAsync(context)
                : Task.FromResult(EventStatus.NotConsumed);
        }

        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions)
        {
            var result = EventStatus.Undelivered;

            var serviceProvider = ServiceProvider.CreateScope().ServiceProvider;

            var storage = serviceProvider.GetRequiredService<IStateflowsStorage>();

            var stateflowsContext = await storage.HydrateAsync(id);

            var key = stateflowsContext.Version != 0
                ? $"{id.Name}.{stateflowsContext.Version}"
                : $"{id.Name}.current";

            if (!Register.StateMachines.TryGetValue(key, out var graph))
            {
                return result;
            }

            using (var executor = new Executor(Register, graph, serviceProvider, stateflowsContext, eventHolder))
            {
                await executor.HydrateAsync();

                try
                {
                    if (eventHolder is EventHolder<CompoundRequest> compoundRequestHolder)
                    {
                        var compoundRequest = compoundRequestHolder.Payload;
                        result = EventStatus.Consumed;
                        var results = new List<RequestResult>();
                        foreach (var ev in compoundRequest.Events)
                        {
                            ev.Headers.AddRange(eventHolder.Headers);

                            executor.Context.SetEvent(ev);

                            executor.BeginScope();
                            try
                            {
                                var status = await ev.ExecuteBehaviorAsync(this, result, executor);

                                results.Add(new RequestResult(
                                    ev,
                                    ev.IsRequest()
                                        ? ev.GetResponseHolder()
                                        : null,
                                    status,
                                    new EventValidation(true, new List<ValidationResult>())
                                ));
                            }
                            finally
                            {
                                executor.EndScope();

                                executor.Context.ClearEvent();
                            }
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
                        executor.BeginScope();
                        try
                        {
                            result = await ExecuteBehaviorAsync(eventHolder, result, executor);
                        }
                        finally
                        {
                            executor.EndScope();
                        }
                    }
                }
                finally
                {
                    if (stateflowsContext.Status == BehaviorStatus.Initialized)
                    {
                        stateflowsContext.Version = graph.Version;
                    }

                    stateflowsContext.Status = executor.BehaviorStatus;

                    stateflowsContext.LastExecutedAt = DateTime.Now;

                    exceptions.AddRange(executor.Context.Exceptions);

                    await executor.DehydrateAsync();
                }

                // out of try-finally to make sure that context won't be saved when execution fails
                await storage.DehydrateAsync(executor.Context.Context);
            }

            return result;
        }

        Task<EventStatus> IStateflowsProcessor.ExecuteBehaviorAsync<TEvent>(EventHolder<TEvent> eventHolder, EventStatus result, IStateflowsExecutor stateflowsExecutor)
            => ExecuteBehaviorAsync(eventHolder, result, stateflowsExecutor as Executor);

        private async Task<EventStatus> ExecuteBehaviorAsync<TEvent>(
            EventHolder<TEvent> eventHolder,
            EventStatus result,
            Executor executor
        )
        {
            var eventContext = new EventContext<TEvent>(executor.Context);

            if (await executor.Inspector.BeforeProcessEventAsync(eventContext))
            {
                try
                {
                    var attributes = eventHolder.GetType().GetCustomAttributes<NoImplicitInitializationAttribute>();
                    if (!executor.Initialized && !attributes.Any() && !typeof(Exception).IsAssignableFrom(eventHolder.PayloadType))
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

            return result;
        }
    }
}
