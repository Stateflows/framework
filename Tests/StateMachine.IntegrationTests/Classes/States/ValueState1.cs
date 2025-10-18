using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.StateMachines.Attributes;

namespace StateMachine.IntegrationTests.Classes.States
{
    internal record ValueSet1(
        [StateValue] IValue<int?> nullable,
        [StateValue] IValue<int?> nulled
    ) : IValueSet
    { }
    
    internal class ValueState1 : IStateEntry
    {
        public ValueState1(
            [StateValue] IValue<int> counter,
            ValueSet1 valueSet
        )
        {
            this.counter = counter;
            this.valueSet = valueSet;
        }

        private readonly IValue<int> counter;
        private readonly ValueSet1 valueSet;

        public async Task OnEntryAsync()
        {
            var c = await counter.GetOrDefaultAsync(0);

            await counter.SetAsync(c + 1);
            
            await valueSet.nulled.SetAsync(null);
        }
    }
}
