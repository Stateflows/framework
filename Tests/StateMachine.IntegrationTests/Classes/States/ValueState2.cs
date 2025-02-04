namespace StateMachine.IntegrationTests.Classes.States
{
    internal class ValueState2: IStateEntry
    {
        public Task OnEntryAsync()
        {
            return Task.CompletedTask;
        }
    }
}
