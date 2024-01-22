using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Context;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Storage
{
    public class InMemoryStorage : IStateflowsStorage
    {
        private readonly Dictionary<BehaviorId, string> Contexts = new Dictionary<BehaviorId, string>();

        public Task<StateflowsContext> Hydrate(BehaviorId id)
        {
            lock (Contexts)
            {
                var context = Contexts.TryGetValue(id, out var contextStr)
                    ? StateflowsJsonConverter.DeserializeObject<StateflowsContext>(contextStr)
                    : new StateflowsContext() { Id = id };

                return Task.FromResult(context);
            }
        }

        public Task Dehydrate(StateflowsContext context)
        {
            lock (Contexts)
            {
                Contexts[context.Id] = StateflowsJsonConverter.SerializePolymorphicObject(context);
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<StateflowsContext>> GetContexts(IEnumerable<BehaviorClass> behaviorClasses)
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

        public async Task<IEnumerable<StateflowsContext>> GetContextsToTimeTrigger(IEnumerable<BehaviorClass> behaviorClasses)
            => (await GetContexts(behaviorClasses)).Where(context =>
                context.TriggerTime != null &&
                context.TriggerTime < DateTime.Now
            );
    }
}
