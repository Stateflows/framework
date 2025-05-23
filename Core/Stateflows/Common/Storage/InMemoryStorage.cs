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

        public Task<StateflowsContext> HydrateAsync(BehaviorId behaviorId)
        {
            lock (Contexts)
            {
                var context = Contexts.TryGetValue(behaviorId, out var contextStr)
                    ? StateflowsJsonConverter.DeserializeObject<StateflowsContext>(contextStr)
                    : new StateflowsContext(behaviorId);

                return Task.FromResult(context);
            }
        }

        public Task DehydrateAsync(StateflowsContext context)
        {
            lock (Contexts)
            {
                if (context.Deleted)
                {
                    Contexts.Remove(context.Id);
                }
                else
                {
                    Contexts[context.Id] = StateflowsJsonConverter.SerializePolymorphicObject(context);
                    context.Stored = true;
                }
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<StateflowsContext>> GetAllContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
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

        public async Task<IEnumerable<StateflowsContext>> GetTimeTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
            => (await GetAllContextsAsync(behaviorClasses))
                .Where(context =>
                    context.TriggerTime != null &&
                    context.TriggerTime < DateTime.Now
                )
                .ToArray();

        public async Task<IEnumerable<StateflowsContext>> GetStartupTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
            => (await GetAllContextsAsync(behaviorClasses))
                .Where(context => context.TriggerOnStartup)
                .ToArray();
    }
}
