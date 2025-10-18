using Stateflows.Common;
using Stateflows.Common.Attributes;
using StateMachine.IntegrationTests.Classes.Events;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class InternalTransition([GlobalValue] IValue<int> counter) : ITransitionEffect<SomeEvent>
    {
        public async Task EffectAsync(SomeEvent @event)
        {
            await counter.UpdateAsync(i => i + 1, 0);
        }
    }
}
