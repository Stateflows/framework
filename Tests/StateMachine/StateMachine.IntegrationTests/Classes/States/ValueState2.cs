namespace StateMachine.IntegrationTests.Classes.States
{
    internal class ValueState2 : State
    {
        public override Task OnEntryAsync()
        {
            return Task.CompletedTask;
        }
    }
}
