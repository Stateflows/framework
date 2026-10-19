using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Engine;
using Stateflows.Common.Context.Classes;
using Stateflows.Entities.Registration;

namespace Stateflows.Entities.Engine
{
    internal class Processor(
        EntitiesRegister register,
        IStateflowsStorage storage,
        IServiceProvider serviceProvider
    ) : IEventProcessor
    {
        public string[] BehaviorTypes => [BehaviorType.Entity];

        // [DebuggerHidden]
        public Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions)
            => ContextExecution.RunAsync(storage, id, exceptions, async execution =>
            {
                var stateflowsContext = execution.Context;

                stateflowsContext.ExecutionTriggerHolder = eventHolder;

                var key = stateflowsContext.Version != 0
                    ? $"{id.Name}.{stateflowsContext.Version}"
                    : $"{id.Name}.current";

                if (!register.Entities.TryGetValue(key, out var registration))
                {
                    return EventStatus.Undelivered;
                }

                var embedding = eventHolder.Headers.Values.FirstOrDefault(h => h is BehaviorEmbedding) as BehaviorEmbedding;
                if (embedding != null)
                {
                    stateflowsContext.ContextOwnerId = embedding.OwnerId;
                    stateflowsContext.ContextParentId = embedding.ParentId;
                }

                using var behaviorContext = new BehaviorContext(stateflowsContext, serviceProvider);
                var inspection = await EntityInspection.CreateAsync(register, registration, behaviorContext);
                var executor = new Executor(registration, stateflowsContext, serviceProvider, inspection);
                var reportedStatus = executor.ReportedStatus;

                var noImplicitInitialization =
                    eventHolder.PayloadType.GetCustomAttributes<NoImplicitInitializationAttribute>().Any() ||
                    eventHolder.Headers.Values.Any(h => h is NoImplicitInitialization);

                var result = await inspection.ProcessEventAsync(eventHolder, async () =>
                {
                    var status = await executor.TryInitializeAsync(eventHolder.Payload);
                    if (status == EventStatus.Failed)
                    {
                        return status;
                    }

                    if (status != EventStatus.Initialized)
                    {
                        if (!executor.Initialized && !noImplicitInitialization)
                        {
                            await executor.EnsureInitializedAsync();
                        }

                        status = executor.Initialized || Executor.IsProcessedWhenNotInitialized(eventHolder)
                            ? await executor.DoProcessAsync(eventHolder)
                            : EventStatus.NotInitialized;
                    }

                    return status;
                });

                if (stateflowsContext.Status == BehaviorStatus.Initialized)
                {
                    stateflowsContext.Version = registration.Version;
                }

                if (executor.ReportedStatus != reportedStatus)
                {
                    var entityInfo = executor.GetEntityInfo();
                    ((IBehaviorContext)behaviorContext).Publish(entityInfo);
                    ((IBehaviorContext)behaviorContext).Publish<BehaviorInfo>(entityInfo);
                }

                return result;
            });

        public Task CancelProcessingAsync(BehaviorId id)
            => Task.CompletedTask;
    }
}
