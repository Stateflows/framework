using Stateflows.Common;

namespace StateMachine.IntegrationTests.Classes.States
{
    internal class ValueState1 : State
    {
        private readonly StateValue<int> counter = new("counter");

        public override Task OnEntryAsync()
        {
            counter.Value++;

            return Task.CompletedTask;
        }
    }
}
