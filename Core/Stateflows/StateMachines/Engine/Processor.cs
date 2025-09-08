using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines.Engine
{
    internal class Processor : IEventProcessor, IStateflowsProcessor
    {
        public string BehaviorType => Constants.StateMachine;

        private readonly StateMachinesRegister Register;
        private readonly IEnumerable<IStateMachineEventHandler> EventHandlers;
        private readonly IServiceProvider ServiceProvider;

        public Processor(
            StateMachinesRegister register,
            IEnumerable<IStateMachineEventHandler> eventHandlers,
            IServiceProvider serviceProvider
        )
        {
            Register = register;
            EventHandlers = eventHandlers;
            ServiceProvider = serviceProvider;
        }

        [DebuggerHidden]
        private Task<EventStatus> TryHandleEventAsync<TEvent>(EventContext<TEvent> context)
        {
            var eventHandler = EventHandlers.FirstOrDefault(h => h.EventType.IsInstanceOfType(context.Event));

            return eventHandler != null
                ? eventHandler.TryHandleEventAsync<TEvent>(context)
                : Task.FromResult(EventStatus.NotConsumed);
        }
        
        [DebuggerHidden]
        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions)
        {
            try
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

                using var executor = new Executor(Register, graph, serviceProvider, stateflowsContext, eventHolder);
                
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

                    executor.Dehydrate();
                }

                // out of try-finally to make sure that context won't be saved when execution fails
                await storage.DehydrateAsync(executor.Context.Context);

                return result;
            }
            catch (Exception e)
            {
                if (!(e is BehaviorExecutionException))
                {
                    Trace.WriteLine($"⦗→s⦘ State Machine '{id.Name}:{id.Instance}': exception '{e.GetType().FullName}' thrown with message '{e.Message}'");
                }

                return EventStatus.Failed;
            }
        }

        Task<EventStatus> IStateflowsProcessor.ExecuteBehaviorAsync<TEvent>(EventHolder<TEvent> eventHolder, EventStatus result, IStateflowsExecutor stateflowsExecutor)
            => ExecuteBehaviorAsync(eventHolder, result, stateflowsExecutor as Executor);

        [DebuggerHidden]
        private async Task<EventStatus> ExecuteBehaviorAsync<TEvent>(
            EventHolder<TEvent> eventHolder,
            EventStatus result,
            Executor executor
        )
        {
            // Trace.WriteLine($"⦗→s⦘ State Machine '{executor.Context.Id.Name}:{executor.Context.Id.Instance}': resetting StateHasChanged flag");
            executor.StateHasChanged = false;
            
            var eventContext = new EventContext<TEvent>(executor.Context);
            var commonEventContext = new Common.Context.Classes.EventContext<TEvent>(
                eventContext.Context.Context,
                executor.ServiceProvider,
                eventHolder
            );
            
            if (executor.Inspector.BeforeProcessEvent(eventContext, commonEventContext))
            {
                try
                {
                    var noImplicitInitialization =
                        eventHolder.PayloadType.GetCustomAttributes<NoImplicitInitializationAttribute>().Any() ||
                        eventHolder.Headers.Any(h => h is NoImplicitInitialization);
                    
                    if (!executor.Initialized && !typeof(Exception).IsAssignableFrom(eventHolder.PayloadType))
                    {
                        result = await executor.InitializeAsync(eventHolder, noImplicitInitialization);
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
                                : noImplicitInitialization
                                    ? handlingResult
                                    : EventStatus.Rejected;
                        }
                    }
                }
                finally
                {
                    if (result == EventStatus.Undelivered)
                    {
                        result = EventStatus.Failed;
                    }
                    
                    executor.Inspector.AfterProcessEvent(eventContext, commonEventContext, result);
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
