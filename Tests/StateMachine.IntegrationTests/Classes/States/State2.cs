using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace StateMachine.IntegrationTests.Classes.States
{
    internal class State2 : IStateEntry, IStateExit
    {
        public static bool EntryFired = false;

        public static bool ExitFired = false;

        private readonly IStateContext stateContext;
        private readonly IBehaviorContext behaviorContext;
        public State2(IStateContext stateContext, IBehaviorContext behaviorContext)
        {
            this.stateContext = stateContext;
            this.behaviorContext = behaviorContext;
        }

        public static void Reset()
        {
            EntryFired = false;
            ExitFired = false;
        }

        public Task OnEntryAsync()
        {
            EntryFired = stateContext != null && behaviorContext?.Id.Instance != null;
            return Task.CompletedTask;
        }

        public Task OnExitAsync()
        {
            ExitFired = stateContext != null && behaviorContext?.Id.Instance != null;
            return Task.CompletedTask;
        }
    }
}
