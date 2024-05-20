using Stateflows.StateMachines.Events;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class ValueTransition : Transition<CompletionEvent>
    {
        private readonly SourceStateValue<int> counter = new("counter");

        public override Task<bool> GuardAsync()
            => Task.FromResult(counter.IsSet && counter.Value == 1);
    }
}
