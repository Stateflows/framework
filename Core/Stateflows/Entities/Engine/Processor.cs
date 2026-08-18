using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Entities.Registration;

namespace Stateflows.Entities.Engine
{
    internal class Processor(
        EntitiesRegister register,
        IStateflowsStorage storage,
        IServiceProvider serviceProvider
    ) : IEventProcessor
    {
        public string BehaviorType => global::Stateflows.BehaviorType.Entity;

        [DebuggerHidden]
        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions)
        {
            try
            {
                var stateflowsContext = await storage.HydrateAsync(id);

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

                var executor = new Executor(registration, stateflowsContext, serviceProvider);

                var result = executor.TryInitialize(eventHolder.Payload);

                stateflowsContext.Status = executor.BehaviorStatus;
            
                if (stateflowsContext.Status != BehaviorStatus.Initialized)
                {
                    executor.EnsureInitialized();
                    stateflowsContext.Status = executor.BehaviorStatus;
                }

                if (result != EventStatus.Initialized)
                {
                    result = executor.DoProcessAsync(eventHolder);
                    if (result == EventStatus.Consumed)
                    {
                        stateflowsContext.Status = BehaviorStatus.Initialized;
                    }
                }
                
                if (stateflowsContext.Status == BehaviorStatus.Initialized)
                {
                    stateflowsContext.Version = registration.Version;
                }


                stateflowsContext.LastExecutedAt = DateTime.Now;

                await storage.DehydrateAsync(stateflowsContext);

                return result;
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                return EventStatus.Failed;
            }
        }

        public Task CancelProcessingAsync(BehaviorId id)
            => Task.CompletedTask;
    }
}


