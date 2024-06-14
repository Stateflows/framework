using Stateflows.StateMachines.Events;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class ValueTransition : Transition<CompletionEvent>
    {
        private readonly SourceStateValue<int> counter = new("counter");

        public override Task<bool> GuardAsync()
        {
            var result = counter.TryGet(out var c) && c == 1;
            return Task.FromResult(result);
        }
    }
}
