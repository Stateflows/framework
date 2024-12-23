using Stateflows.Common;

namespace StateMachine.IntegrationTests.Classes.States
{
    internal class State1 : IStateEntry, IStateExit
    {
        public static bool EntryFired = false;

        public static bool ExitFired = false;

        private readonly IVertexContext vertexContext;
        private readonly IStateMachineContext stateMachineContext;
        private readonly Stateflows.StateMachines.IExecutionContext executionContext;
        public State1(IVertexContext vertexContext, IStateMachineContext stateMachineContext, Stateflows.StateMachines.IExecutionContext executionContext)
        {
            this.vertexContext = vertexContext;
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
            EntryFired = vertexContext != null && stateMachineContext?.Id.Instance != null && executionContext != null;
            return Task.CompletedTask;
        }

        public Task OnExitAsync()
        {
            ExitFired = vertexContext != null && stateMachineContext?.Id.Instance != null && executionContext != null;
            return Task.CompletedTask;
        }
    }
}
