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
    internal class Processor : IEventProcessor, IStateflowsProcessor
    {
        string IEventProcessor.BehaviorType => BehaviorType.Action;

        private readonly ActionsRegister Register;
        private readonly IStateflowsLock StateflowsLock;
        private readonly IStateflowsStorage Storage;
        private readonly IStateflowsValueStorage ValueStorage;
        private readonly IServiceProvider ServiceProvider;

        public Processor(
            ActionsRegister register,
            IStateflowsLock stateflowsLock,
            IStateflowsStorage storage,
            IStateflowsValueStorage valueStorage,
            IServiceProvider serviceProvider
        )
        {
            Register = register;
            StateflowsLock = stateflowsLock;
            Storage = storage;
            ValueStorage = valueStorage;
            ServiceProvider = serviceProvider;
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

            if (!Register.Actions.TryGetValue(key, out var action))
            {
                return result;
            }

            try
            {
                await using var lockHandle = await (
                    action.IsStateless
                        ? StateflowsLock.AquireNoLockAsync(id)
                        : StateflowsLock.AquireLockAsync(id)
                );

                var forcedReset = eventHolder.Headers.Values.Any(h => h is ForcedReset);
                if (forcedReset)
                {
                    await ValueStorage.ClearAsync(id);
                }
                
                stateflowsContext = action.IsStateless || forcedReset
                    ? new StateflowsContext(id)
                    : await Storage.HydrateAsync(id);

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

                if (!action.IsStateless || stateflowsContext.ContextOwnerId != null)
                {
                    // stateflowsContext.StateflowsValues = (await ValueStorage.LoadAsync(stateflowsContext.ContextOwnerId ?? stateflowsContext.Id)).ToDictionary();
                }

                var executor = new Executor(Register, stateflowsContext, serviceProvider, action);
                
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
            finally
            {
                if (stateflowsContext.Status == BehaviorStatus.Initialized)
                {
                    stateflowsContext.Version = action.Version;
                }

                // stateflowsContext.Status = BehaviorStatus.Initialized;

                stateflowsContext.LastExecutedAt = DateTime.Now;

                // exceptions.AddRange(context.Exceptions);

                if (!action.IsStateless || stateflowsContext.ContextOwnerId != null)
                {
                    // await ValueStorage.SaveAsync(stateflowsContext.ContextOwnerId ?? stateflowsContext.Id, stateflowsContext.StateflowsValues);
                }
                
                if (!action.IsStateless)
                {
                    await Storage.DehydrateAsync(stateflowsContext);
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
