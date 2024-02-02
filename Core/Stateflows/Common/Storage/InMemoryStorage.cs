using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Context;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Trace.Models;

namespace Stateflows.Common.Storage
{
    public class InMemoryStorage : IStateflowsStorage
    {
        private readonly Dictionary<BehaviorId, string> Contexts = new Dictionary<BehaviorId, string>();

        public Task<StateflowsContext> HydrateAsync(BehaviorId behaviorId)
        {
            lock (Contexts)
            {
                var context = Contexts.TryGetValue(behaviorId, out var contextStr)
                    ? StateflowsJsonConverter.DeserializeObject<StateflowsContext>(contextStr)
                    : new StateflowsContext() { Id = behaviorId };

                return Task.FromResult(context);
            }
        }

        public Task DehydrateAsync(StateflowsContext context)
        {
            lock (Contexts)
            {
                Contexts[context.Id] = StateflowsJsonConverter.SerializePolymorphicObject(context);
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<StateflowsContext>> GetContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        {
            IEnumerable<StateflowsContext> result;

            lock (Contexts)
            {
                result = Contexts.Keys
                    .Where(key => behaviorClasses.Contains(key.BehaviorClass))
                    .Select(key => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(Contexts[key]))
                    .ToArray();
            }

            return Task.FromResult(result);
        }

        public async Task<IEnumerable<StateflowsContext>> GetContextsToTimeTriggerAsync(IEnumerable<BehaviorClass> behaviorClasses)
            => (await GetContextsAsync(behaviorClasses)).Where(context =>
                context.TriggerTime != null &&
                context.TriggerTime < DateTime.Now
            );

        public Task SaveTraceAsync(BehaviorTrace behaviorTrace)
            => Task.CompletedTask;

        public Task<IEnumerable<BehaviorTrace>> GetTracesAsync(BehaviorId behaviorId)
            => Task.FromResult(Array.Empty<BehaviorTrace>() as IEnumerable<BehaviorTrace>);
    }
}
