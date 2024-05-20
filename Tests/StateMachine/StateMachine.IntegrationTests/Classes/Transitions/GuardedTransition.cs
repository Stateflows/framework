using Stateflows.Common;
using Stateflows.StateMachines.Events;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class GuardedTransition : Transition<CompletionEvent>
    {
        private readonly GlobalValue<int> counter = new("counter");

        public override Task<bool> GuardAsync()
            => Task.FromResult(counter.IsSet && counter.Value == 1);
    }
}
