using Stateflows.StateMachines.Context.Interfaces;

namespace StateMachine.IntegrationTests.Classes.States
{
    internal class State2 : IStateEntry, IStateExit
    {
        public static bool EntryFired = false;

        public static bool ExitFired = false;

        private readonly IVertexContext vertexContext;
        private readonly IStateMachineContext stateMachineContext;
        public State2(IVertexContext vertexContext, IStateMachineContext stateMachineContext)
        {
            this.vertexContext = vertexContext;
            this.stateMachineContext = stateMachineContext;
        }

        public static void Reset()
        {
            EntryFired = false;
            ExitFired = false;
        }

        public Task OnEntryAsync()
        {
            EntryFired = vertexContext != null && stateMachineContext?.Id.Instance != null;
            return Task.CompletedTask;
        }

        public Task OnExitAsync()
        {
            ExitFired = vertexContext != null && stateMachineContext?.Id.Instance != null;
            return Task.CompletedTask;
        }
    }
}
