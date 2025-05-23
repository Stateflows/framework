using Stateflows.Common;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class GuardedTransition : IDefaultTransitionGuard
    {
        private readonly GlobalValue<int> counter = new("counter");

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
