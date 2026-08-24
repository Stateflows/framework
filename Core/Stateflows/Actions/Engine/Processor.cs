using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions.Context.Classes;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Actions.Registration;
using Stateflows.Common.Context;

namespace Stateflows.Actions.Engine
{
    internal class Processor(
        ActionsRegister register,
        IStateflowsLock stateflowsLock,
        IStateflowsStorage storage,
        IStateflowsValueStorage valueStorage,
        IServiceProvider provider
    ) : IEventProcessor, IStateflowsProcessor
    {
        public string[] BehaviorTypes => register.SupportedClassTypes;

        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions)
        {
            var result = EventStatus.Undelivered;

            using var serviceScope = provider.CreateScope();
            
            var serviceProvider = serviceScope.ServiceProvider;

            var stateflowsContext = await storage.HydrateAsync(id);
            
            var key = stateflowsContext.Version != 0
                ? $"{id.Name}.{stateflowsContext.Version}"
                : $"{id.Name}.current";

            if (!register.Actions.TryGetValue(key, out var action))
            {
                return result;
            }

            try
            {
                await using var lockHandle = await (
                    action.IsStateless
                        ? stateflowsLock.AquireNoLockAsync(id)
                        : stateflowsLock.AquireLockAsync(id)
                );

                var forcedReset = eventHolder.Headers.Values.Any(h => h is ForcedReset);
                if (forcedReset)
                {
                    await valueStorage.ClearAsync(id);
                }
                
                stateflowsContext = action.IsStateless || forcedReset
                    ? new StateflowsContext(id)
                    : await storage.HydrateAsync(id);

                stateflowsContext.ExecutionTriggerHolder = eventHolder;

                if (stateflowsContext.Status == BehaviorStatus.Unknown)
                {
                    stateflowsContext.Status = BehaviorStatus.NotInitialized;
                }

                var embedding = (BehaviorEmbedding?)eventHolder.Headers.Values.FirstOrDefault(h => h is BehaviorEmbedding);
                if (embedding != null)
                {
                    stateflowsContext.ContextOwnerId = embedding.OwnerId;
                    stateflowsContext.ContextParentId = embedding.ParentId;
                }

                var executor = new Executor(register, stateflowsContext, serviceProvider, action);
                
                await executor.HydrateAsync(eventHolder);
                
                var noImplicitInitialization =
                    eventHolder.PayloadType.GetCustomAttributes<NoImplicitInitializationAttribute>().Any() ||
                    eventHolder.Headers.Values.Any(h => h is NoImplicitInitialization);
                
                if (stateflowsContext.Status is BehaviorStatus.NotInitialized or BehaviorStatus.Unknown && !noImplicitInitialization)
                {
                    stateflowsContext.Status = BehaviorStatus.Initialized;

                    var context = new ActionDelegateContext(stateflowsContext, executor, eventHolder, serviceProvider);
                    var inspector = await executor.GetInspectorAsync();
                    inspector.BeforeActionInitialize(context);
                    inspector.AfterActionInitialize(context);
                }

                if (
                    stateflowsContext.Status == BehaviorStatus.Initialized ||
                    eventHolder is EventHolder<BehaviorInfoRequest> ||
                    eventHolder is EventHolder<Finalize> ||
                    eventHolder is EventHolder<Reset>
                )
                {
                    result = await ExecuteBehaviorAsync(eventHolder, result, executor);
                }

                await executor.DehydrateAsync(eventHolder);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                result = EventStatus.Failed;
            }
            finally
            {
                if (stateflowsContext.Status == BehaviorStatus.Initialized)
                {
                    stateflowsContext.Version = action.Version;
                }

                stateflowsContext.LastExecutedAt = DateTime.Now;
                
                if (!action.IsStateless)
                {
                    await storage.DehydrateAsync(stateflowsContext);
                }
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

        public Task<EventStatus> ExecuteBehaviorAsync<TEvent>(EventHolder<TEvent> eventHolder,
            EventStatus result, IStateflowsExecutor stateflowsExecutor)
            => stateflowsExecutor.DoProcessAsync(eventHolder);
    }
}
