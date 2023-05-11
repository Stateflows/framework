using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    public class InMemoryStorage : IStateflowsStorage
    {
        public Dictionary<int, StateflowsContext> Contexts { get; } = new Dictionary<int, StateflowsContext>();

        public Task<StateflowsContext> Hydrate(BehaviorId id)
        {
            lock (Contexts)
            {
                if (!Contexts.TryGetValue(id.GetHashCode(), out var context))
                {
                    context = new StateflowsContext() { Id = id };
                }

                return Task.FromResult(context);
            }
        }

        public Task Dehydrate(StateflowsContext context)
        {
            var hash = context.Id.GetHashCode();

            lock (Contexts)
            {
                Contexts[hash] = context;
            }

            return Task.CompletedTask;
        }

        public Task AddTimeTokens(TimeToken[] timeTokens)
        {
            foreach (var timeToken in timeTokens)
            {
                timeToken.Id = new Random().Next(int.MaxValue).ToString();
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<TimeToken>> GetTimeTokens(IEnumerable<BehaviorClass> behaviorClasses)
            => Task.FromResult(new List<TimeToken>() as IEnumerable<TimeToken>);

        public Task ClearTimeTokens(BehaviorId behaviorId, IEnumerable<string> ids)
            => Task.CompletedTask;
    }
}
