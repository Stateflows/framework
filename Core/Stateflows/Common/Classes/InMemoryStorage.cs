using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    public class InMemoryStorage : IStateflowsStorage
    {
        public Dictionary<string, StateflowsContext> Contexts { get; } = new Dictionary<string, StateflowsContext>();

        public Task<StateflowsContext> Hydrate(BehaviorId id)
        {
            lock (Contexts)
            {
                if (!Contexts.TryGetValue(id.ToString(), out var context))
                {
                    context = new StateflowsContext() { Id = id };
                }

                return Task.FromResult(context);
            }
        }

        public Task Dehydrate(StateflowsContext context)
        {
            var hash = context.Id.ToString();

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
