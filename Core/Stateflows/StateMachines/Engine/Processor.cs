using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Engine;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines.Engine
{
    internal class Processor : IEventProcessor, IStateflowsProcessor
    {
        public string BehaviorType => Constants.StateMachine;

        private readonly StateMachinesRegister Register;
        private readonly IEnumerable<IStateMachineEventHandler> EventHandlers;
        private readonly IStateflowsStorage Storage;
        private readonly IServiceProvider ServiceProvider;
        private readonly IStateflowsValueStorage ValueStorage;

        public Processor(
            StateMachinesRegister register,
            IEnumerable<IStateMachineEventHandler> eventHandlers,
            IStateflowsStorage storage,
            IServiceProvider serviceProvider
        )
        {
            Register = register;
            EventHandlers = eventHandlers;
            Storage = storage;
            ServiceProvider = serviceProvider;
            ValueStorage = ServiceProvider.GetRequiredService<IStateflowsValueStorage>();
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

                using var serviceScope = ServiceProvider.CreateScope();
                
                var serviceProvider = serviceScope.ServiceProvider;

                var forcedReset = eventHolder.Headers.Values.Any(h => h is ForcedReset);
                if (forcedReset)
                {
                    await ValueStorage.ClearAsync(id);
                }

                var stateflowsContext = forcedReset
                    ? new StateflowsContext(id)
                    : await Storage.HydrateAsync(id);

                var key = stateflowsContext.Version != 0
                    ? $"{id.Name}.{stateflowsContext.Version}"
                    : $"{id.Name}.current";

                if (!Register.StateMachines.TryGetValue(key, out var graph))
                {
                    return result;
                }

                var embedding = (BehaviorEmbedding?)eventHolder.Headers.Values.FirstOrDefault(h => h is BehaviorEmbedding);
                if (embedding != null)
                {
                    stateflowsContext.ContextOwnerId = embedding.OwnerId;
                    stateflowsContext.ContextParentId = embedding.ParentId;
                }

                // stateflowsContext.StateflowsValues = (await ValueStorage.LoadAsync(stateflowsContext.ContextOwnerId ?? stateflowsContext.Id)).ToDictionary();

                using var executor = new Executor(Register, graph, serviceProvider, stateflowsContext, eventHolder);
                
                await executor.HydrateAsync();

                try
                {
                    if (await TryCancelAsync(id, eventHolder, executor, result))
                    {
                        result = EventStatus.Cancelled;
                    }
                    else
                    {
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

                    if (await TryCancelAsync(id, eventHolder, executor, result))
                    {
                        result = EventStatus.Cancelled;
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
                var ctx = executor.Context.Context;
                // await ValueStorage.SaveAsync(ctx.ContextOwnerId ?? ctx.Id, ctx.StateflowsValues);
                await Storage.DehydrateAsync(ctx);

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

        public Task CancelProcessingAsync(BehaviorId id)
            => Task.CompletedTask;

        private async Task<bool> TryCancelAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, Executor executor, EventStatus result)
        {
            if (eventHolder.Payload is Finalize)
            {
                return false;
            }
            
            var forceFinalize = await ValueStorage.GetOrDefaultAsync(id, CommonValues.ForceFinalizeKey, false);
            if (forceFinalize)
            {
                await ValueStorage.RemoveAsync(id, CommonValues.ForceFinalizeKey);
                
                executor.BeginScope();
                try
                {
                    var finalize = new Finalize().ToEventHolder();
                    executor.Context.SetEvent(finalize);
                    _ = await executor.ExitAsync();
                }
                finally
                {
                    executor.Context.ClearEvent();
                    executor.EndScope();
                }

                Debug.WriteLine($"Forcefuly quit processing in {id.Name}");
                
                return true;
            }
            
            return false;
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
                        eventHolder.Headers.Values.Any(h => h is NoImplicitInitialization);
                    
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
