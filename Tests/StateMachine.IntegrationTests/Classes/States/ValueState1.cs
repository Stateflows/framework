using Stateflows.Common.Attributes;

namespace StateMachine.IntegrationTests.Classes.States
{
    internal class ValueState1 : IStateEntry
    {
        public ValueState1(
            [ValueName("counter")] StateValue<int> counter,
            [ValueName("nullable")] StateValue<int?> nullable,
            [ValueName("nulled")] StateValue<int?> nulled
        )
        {
            this.counter = counter;
            this.nullable = nullable;
            this.nulled = nulled;
        }

        private readonly StateValue<int> counter;
        private readonly StateValue<int?> nullable;
        private readonly StateValue<int?> nulled;

        public Task OnEntryAsync()
        {
            if (counter.TryGet(out var c))
            {
                c = 0;
            }

            counter.Set(c + 1);
            
            // nullable.Set(3);
            
            nulled.Set(null);

            return Task.CompletedTask;
        }
    }
}
