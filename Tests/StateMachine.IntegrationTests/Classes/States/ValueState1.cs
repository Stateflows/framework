using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.StateMachines.Attributes;

namespace StateMachine.IntegrationTests.Classes.States
{
    internal class ValueState1 : IStateEntry
    {
        public ValueState1(
            // [StateValue] IValue<int> counter,
            [ValueName("counter")] StateValue<int> counter,
            [StateValue] IValue<int?> nullable,
            [StateValue] IValue<int?> nulled
        )
        {
            this.counter = counter;
            this.nullable = nullable;
            this.nulled = nulled;
        }

        // private readonly IValue<int> counter;
        private readonly StateValue<int> counter;
        private readonly IValue<int?> nullable;
        private readonly IValue<int?> nulled;

        public async Task OnEntryAsync()
        {
            var c = await counter.GetOrDefaultAsync(0);

            await counter.SetAsync(c + 1);
            
            await nulled.SetAsync(null);
        }
    }
}
