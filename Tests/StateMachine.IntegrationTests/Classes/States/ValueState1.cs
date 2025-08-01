using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.StateMachines.Attributes;

namespace StateMachine.IntegrationTests.Classes.States
{
    internal class ValueSet1 : IValuesSet
    {
        public ValueSet1(
            [StateValue] IValue<int?> nullable,
            [StateValue] IValue<int?> nulled
        )
        {
            this.nullable = nullable;
            this.nulled = nulled;
        }
        
        public readonly IValue<int?> nullable;
        public readonly IValue<int?> nulled;
    }
    
    internal class ValueState1 : IStateEntry
    {
        public ValueState1(
            // [StateValue] IValue<int> counter,
            [ValueName("counter")] StateValue<int> counter,
            // [StateValue] IValue<int?> nullable,
            // [StateValue] IValue<int?> nulled,
            ValueSet1 valueSet
        )
        {
            this.counter = counter;
            // this.nullable = nullable;
            // this.nulled = nulled;
            this.valueSet = valueSet;
        }

        // private readonly IValue<int> counter;
        private readonly StateValue<int> counter;
        // private readonly IValue<int?> nullable;
        // private readonly IValue<int?> nulled;
        private readonly ValueSet1 valueSet;

        public async Task OnEntryAsync()
        {
            var c = await counter.GetOrDefaultAsync(0);

            await counter.SetAsync(c + 1);
            
            await valueSet.nulled.SetAsync(null);
        }
    }
}
