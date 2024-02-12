namespace StateMachine.IntegrationTests.Classes.States
{
    internal class State2 : State
    {
        public static bool EntryFired = false;

        public static bool ExitFired = false;

        public static void Reset()
        {
            EntryFired = false;
            ExitFired = false;
        }

        public override Task OnEntryAsync()
        {
            EntryFired = Context != null && Context.StateMachine.Id.Instance != null;
            return Task.CompletedTask;
        }

        public override Task OnExitAsync()
        {
            ExitFired = Context != null && Context.StateMachine.Id.Instance != null;
            return Task.CompletedTask;
        }
    }
}
