using Stateflows.Common.Attributes;

namespace StateMachine.IntegrationTests.Classes.States
{
    internal class ValueState1 : IStateEntry
    {
        public ValueState1([ValueName("counter")] StateValue<int> counter)
        {
            this.counter = counter;
        }

        private readonly StateValue<int> counter;

        public Task OnEntryAsync()
        {
            if (counter.TryGet(out var c))
            {
                c = 0;
            }

            counter.Set(c + 1);

            return Task.CompletedTask;
        }
    }
}
