using Stateflows.Common;
using StateMachine.IntegrationTests.Classes.Events;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class InternalTransition : ITransitionEffect<SomeEvent>
    {
        private readonly GlobalValue<int> counter = new("counter");

        public async Task EffectAsync(SomeEvent @event)
        {
            await counter.UpdateAsync(i => i + 1, 0);
        }
    }
}
