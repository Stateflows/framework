using Stateflows.Common;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class InternalTransition : Transition<SomeEvent>
    {
        private readonly GlobalValue<int> counter = new("counter");

        public override Task EffectAsync()
        {
            if (!counter.TryGet(out var c))
            {
                c = 0;
            }

            counter.Set(c + 1);

            return Task.CompletedTask;
        }
    }
}
