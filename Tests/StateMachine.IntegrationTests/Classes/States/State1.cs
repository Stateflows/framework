using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;
using StateMachine.IntegrationTests.Classes.Events;

namespace StateMachine.IntegrationTests.Classes.States
{
    internal class State1 : IStateEntry, IStateExit
    {
        public static bool EntryFired = false;

        public static bool ExitFired = false;

        private readonly IStateContext stateContext;
        private readonly IBehaviorContext behaviorContext;
        private readonly Stateflows.StateMachines.IExecutionContext executionContext;
        public State1(
            IStateContext stateContext,
            IBehaviorContext behaviorContext,
            Stateflows.StateMachines.IExecutionContext executionContext
        )
        {
            this.stateContext = stateContext;
            this.behaviorContext = behaviorContext;
            this.executionContext = executionContext;
        }

        public static void Reset()
        {
            EntryFired = false;
            ExitFired = false;
        }

        public Task OnEntryAsync()
        {
            EntryFired = stateContext != null && behaviorContext?.Id.Instance != null && executionContext != null;
            return Task.CompletedTask;
        }

        public Task OnExitAsync()
        {
            ExitFired = stateContext != null && behaviorContext?.Id.Instance != null && executionContext != null;
            return Task.CompletedTask;
        }
    }
}
