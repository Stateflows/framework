using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;
using Stateflows.Common.Engine;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines;

namespace Stateflows.Activities.Engine
{
    internal class Processor : IEventProcessor, IStateflowsProcessor
    {
        string IEventProcessor.BehaviorType => BehaviorType.Activity;

        private readonly ActivitiesRegister Register;
        private readonly IEnumerable<IActivityEventHandler> EventHandlers;
        private readonly IServiceProvider ServiceProvider;
        private readonly IStateflowsStorage Storage;
        private readonly IStateflowsValueStorage ValueStorage;

        public Processor(
            ActivitiesRegister register,
            IEnumerable<IActivityEventHandler> eventHandlers,
            IStateflowsStorage storage,
            IStateflowsValueStorage valueStorage,
            IServiceProvider serviceProvider
        )
        {
            Register = register;
            ServiceProvider = serviceProvider;
            Storage = storage;
            EventHandlers = eventHandlers;
            ValueStorage = valueStorage;
        }

        private Task<EventStatus> TryHandleEventAsync<TEvent>(EventContext<TEvent> context)
        {
            var eventHandler = EventHandlers.FirstOrDefault(h => 
                h.EventType.IsGenericType && context.Event.GetType().IsGenericType
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

            using var serviceScope = ServiceProvider.CreateScope();
            var serviceProvider = serviceScope.ServiceProvider;

            var stateflowsContext = await Storage.HydrateAsync(id);

            var key = stateflowsContext.Version != 0
                ? $"{id.Name}.{stateflowsContext.Version}"
                : $"{id.Name}.current";

            if (!Register.Activities.TryGetValue(key, out var graph))
            {
                return result;
            }

            using var executor = new Executor(Register, graph, serviceProvider);

            var context = new RootContext(stateflowsContext);

            await executor.HydrateAsync(context);

            try
            {
                if (await TryCancelAsync(id, eventHolder, result, executor))
                {
                    result = EventStatus.Cancelled;
                }
                else
                {
                    if (eventHolder is EventHolder<CompoundRequest> compoundRequestHolder)
                    {
                        var compoundRequest = compoundRequestHolder.Payload;
                        result = EventStatus.Consumed;
                        var results = new List<RequestResult>();
                        foreach (var ev in compoundRequest.Events)
                        {
                            ev.Headers.AddRange(eventHolder.Headers);

                            var status = await ev.ExecuteBehaviorAsync(this, result, executor);

                            results.Add(new RequestResult(
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
                        result = await ExecuteBehaviorAsync(eventHolder, result, executor);
                    }
                }

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

                await Storage.DehydrateAsync(executor.Dehydrate().Context);
            }

            return result;
        }

        public Task CancelProcessingAsync(BehaviorId id)
            => Task.CompletedTask;

        private async Task<bool> TryCancelAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, EventStatus result, Executor executor)
        {
            if (eventHolder.Payload is Finalize)
            {
                return false;
            }
            
            var forceFinalize = await ValueStorage.GetOrDefaultAsync(id, CommonValues.ForceFinalizeKey, false);
            if (forceFinalize)
            {
                await ValueStorage.RemoveAsync(id, CommonValues.ForceFinalizeKey);
                
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
                        eventHolder.Headers.Any(h => h is NoImplicitInitialization);

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
