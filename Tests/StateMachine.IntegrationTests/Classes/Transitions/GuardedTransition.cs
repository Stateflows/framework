using Stateflows.Common;
using Stateflows.Common.Attributes;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class GuardedTransition([GlobalValue] IValue<int> counter) : IDefaultTransitionGuard
    {
        public async Task<bool> GuardAsync()
        {
            var (success, c) = await counter.TryGetAsync();
            var result = success && c == 1;
            return result;
        }
    }

    internal class EffectedTransition : IDefaultTransitionEffect
    {
        public Task EffectAsync()
        {
            return Task.CompletedTask;
        }
    }
}
