using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.StateMachines.Attributes;

namespace StateMachine.IntegrationTests.Classes.States
{
    internal class ValueState2: IStateEntry
    {
        private readonly INamespace x;
        public ValueState2(
            [GlobalNamespace] INamespace x
        )
        {
            this.x = x;
        }
        
        public Task OnEntryAsync()
        {
            x.GetNamespace("y").GetValue<int>("z").SetAsync(42);
            return Task.CompletedTask;
        }
    }
}
