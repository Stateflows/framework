using Stateflows.StateMachines.Attributes;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class ValueTransition : IDefaultTransitionGuard
    {
        private readonly int counter;
        private readonly int? nullable;
        private readonly int? nulled;
        public ValueTransition(
            [SourceStateValue] int counter,
            [SourceStateValue] int? nullable,
            [SourceStateValue] int? nulled
        )
        {
            this.counter = counter;
            this.nullable = nullable;
            this.nulled = nulled;
        }

        public Task<bool> GuardAsync()
        {
            var result =
                counter == 1 &&
                nullable == 3 &&
                nulled is null;
            return Task.FromResult(result);
        }
    }
}
