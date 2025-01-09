using Stateflows.Common;

namespace StateMachine.IntegrationTests.Classes.States
{
    internal class ValueState1 : IStateEntry
    {
        private readonly StateValue<int> counter = new("counter");
        private readonly StateValue<int?> nullable = new("nullable");
        private readonly StateValue<int?> nulled = new("nulled");

        public Task OnEntryAsync()
        {
            if (counter.TryGet(out var c))
            {
                c = 0;
            }

            counter.Set(c + 1);
            
            nullable.Set(3);
            
            nulled.Set(null);

            return Task.CompletedTask;
        }
    }
}
