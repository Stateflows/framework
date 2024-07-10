using Stateflows.Common;
using Stateflows.StateMachines.Events;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class GuardedTransition : IBaseDefaultTransition
    {
        private readonly GlobalValue<int> counter = new("counter");

        public Task<bool> GuardAsync()
        {
            var result = counter.TryGet(out var c) && c == 1;
            return Task.FromResult(result);
        }
    }
}
