using Stateflows.Common;

namespace StateMachine.IntegrationTests.Classes.States
{
    internal class ValueState1 : IStateEntry
    {
        private readonly StateValue<int> counter = new("counter");

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
