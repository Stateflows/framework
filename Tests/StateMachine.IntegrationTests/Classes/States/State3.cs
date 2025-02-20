namespace StateMachine.IntegrationTests.Classes.States
{
    internal class State3 : ICompositeStateInitialization, IStateEntry, IStateExit
    {
        public Task OnEntryAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnExitAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnInitializeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
