using Stateflows.Common;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class GuardedTransition : IDefaultTransitionGuard
    {
        private readonly GlobalValue<int> counter = new("counter");

        public Task<bool> GuardAsync()
        {
            var result = counter.TryGet(out var c) && c == 1;
            return Task.FromResult(result);
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
