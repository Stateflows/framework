using Stateflows.StateMachines.Context.Interfaces;

namespace StateMachine.IntegrationTests.Classes.States
{
    internal class State1 : IStateEntry, IStateExit
    {
        public static bool EntryFired = false;

        public static bool ExitFired = false;

        private readonly IStateContext stateContext;
        private readonly IStateMachineContext stateMachineContext;
        private readonly IExecutionContext executionContext;
        public State1(IStateContext stateContext, IStateMachineContext stateMachineContext, IExecutionContext executionContext)
        {
            this.stateContext = stateContext;
            this.stateMachineContext = stateMachineContext;
            this.executionContext = executionContext;
        }

        public static void Reset()
        {
            EntryFired = false;
            ExitFired = false;
        }

        public Task OnEntryAsync()
        {
            EntryFired = stateContext != null && stateMachineContext?.Id.Instance != null && executionContext != null;
            return Task.CompletedTask;
        }

        public Task OnExitAsync()
        {
            ExitFired = stateContext != null && stateMachineContext?.Id.Instance != null && executionContext != null;
            return Task.CompletedTask;
        }
    }
}
