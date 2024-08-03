namespace Activity.IntegrationTests.Classes.Flow
{
    internal class ControlFlow : IControlFlowGuard
    {
        public Task<bool> GuardAsync()
        {
            return Task.FromResult(true);
        }
    }
}
