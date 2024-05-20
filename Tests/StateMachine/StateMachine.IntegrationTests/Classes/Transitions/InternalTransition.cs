using Stateflows.Common;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class InternalTransition : Transition<SomeEvent>
    {
        private readonly GlobalValue<int> counter = new("counter");

        public override Task EffectAsync()
        {
            counter.Value++;

            return Task.CompletedTask;
        }
    }
}
