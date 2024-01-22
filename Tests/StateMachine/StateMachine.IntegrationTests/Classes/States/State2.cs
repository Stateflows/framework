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
            EntryFired = true;
            return Task.CompletedTask;
        }

        public override Task OnExitAsync()
        {
            ExitFired = true;
            return Task.CompletedTask;
        }
    }
}
