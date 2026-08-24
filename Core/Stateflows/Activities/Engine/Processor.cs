using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;
using Stateflows.Common.Context;
using Stateflows.Common.Engine;
using Stateflows.Common.Utilities;

namespace Stateflows.Activities.Engine
{
    internal class Processor(
        ActivitiesRegister register,
        IEnumerable<IActivityEventHandler> eventHandlers,
        IStateflowsStorage storage,
        IStateflowsValueStorage valueStorage,
        IServiceProvider provider
    ) : IEventProcessor, IStateflowsProcessor
    {
        public string[] BehaviorTypes => [ BehaviorType.Activity ];

        private Task<EventStatus> TryHandleEventAsync<TEvent>(EventContext<TEvent> context)
        {
            var eventHandler = eventHandlers.FirstOrDefault(h => 
                h.EventType.IsGenericType && (context.Event?.GetType().IsGenericType ?? false)
                    ? context.Event.GetType().GetGenericTypeDefinition() == h.EventType
                    : h.EventType.IsInstanceOfType(context.Event)
            );
                        
            return eventHandler != null
                ? eventHandler.TryHandleEventAsync(context)
                : Task.FromResult(EventStatus.NotConsumed);
        }

        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions)
        {
            var result = EventStatus.Undelivered;

            using var serviceScope = provider.CreateScope();
            var serviceProvider = serviceScope.ServiceProvider;
            
            var stateflowsContext = eventHolder.Headers.Values.Any(h => h is ForcedReset)
                ? new StateflowsContext(id)
                : await storage.HydrateAsync(id);

            stateflowsContext.ExecutionTriggerHolder = eventHolder;

            var key = stateflowsContext.Version != 0
                ? $"{id.Name}.{stateflowsContext.Version}"
                : $"{id.Name}.current";

            if (!register.Activities.TryGetValue(key, out var graph))
            {
                return result;
            }

            var embedding = (BehaviorEmbedding?)eventHolder.Headers.Values.FirstOrDefault(h => h is BehaviorEmbedding);
            if (embedding != null)
            {
                stateflowsContext.ContextOwnerId = embedding.OwnerId;
                stateflowsContext.ContextParentId = embedding.ParentId;
            }

            using var executor = new Executor(register, graph, serviceProvider);

            var context = new RootContext(stateflowsContext);

            await executor.HydrateAsync(context);

            try
            {
                result = await TryCancelAsync(id, eventHolder, result, executor)
                    ? EventStatus.Cancelled
                    : await ExecuteBehaviorAsync(eventHolder, result, executor);

                if (await TryCancelAsync(id, eventHolder, result, executor))
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

                exceptions.AddRange(context.Exceptions);

                stateflowsContext = executor.Dehydrate().Context;

                await storage.DehydrateAsync(stateflowsContext);
            }

            return result;
        }

        public Task CancelProcessingAsync(BehaviorId id)
        {
            lock (Common.Context.Classes.BaseContext.Instances)
            {
                if (Common.Context.Classes.BaseContext.Instances.TryGetValue(id, out var contextList))
                {
                    foreach (var context in contextList)
                    {
                        context.CancellationTokenSource.Cancel();
                    }
                }
            }
            
            return Task.CompletedTask;
        }

        private async Task<bool> TryCancelAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, EventStatus result, Executor executor)
        {
            if (eventHolder.Payload is Finalize)
            {
                return false;
            }
            
            var forceFinalize = await valueStorage.GetOrDefaultAsync(id, CommonValues.ForceFinalizeKey, false);
            if (forceFinalize)
            {
                await valueStorage.RemoveAsync(id, CommonValues.ForceFinalizeKey);
                
                try
                {
                    var finalize = new Finalize().ToEventHolder();
                    executor.Context.SetEvent(finalize);
                    _ = await executor.CancelAsync();
                }
                finally
                {
                    executor.Context.ClearEvent();
                }

                return true;
            }

            return false;
        }

        Task<EventStatus> IStateflowsProcessor.ExecuteBehaviorAsync<TEvent>(EventHolder<TEvent> eventHolder, EventStatus result, IStateflowsExecutor stateflowsExecutor)
            => ExecuteBehaviorAsync(eventHolder, result, stateflowsExecutor as Executor);

        private async Task<EventStatus> ExecuteBehaviorAsync<TEvent>(
            EventHolder<TEvent> eventHolder,
            EventStatus result,
            Executor executor
        )
        {
            executor.Context.SetEvent(eventHolder);

            var eventContext = new EventContext<TEvent>(executor.Context, executor.NodeScope);
            
            if (executor.Inspector.BeforeProcessEvent(eventContext))
            {
                try
                {
                    var noImplicitInitialization =
                        eventHolder.PayloadType.GetCustomAttributes<NoImplicitInitializationAttribute>().Any() ||
                        eventHolder.Headers.Values.Any(h => h is NoImplicitInitialization);

                    if (!executor.Initialized && !noImplicitInitialization)
                    {
                        result = await executor.InitializeAsync(
                            eventHolder,
                            eventHolder.Payload is TokensInputEvent tokensEvent
                                ? tokensEvent.Tokens
                                : null
                        );
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
                    
                    executor.Inspector.AfterProcessEvent(eventContext, result);
                }
            }
            else
            {
                if (executor.Context.ForceConsumed)
                {
                    result = EventStatus.Consumed;
                }
            }

            return result;
        }
    }
}
