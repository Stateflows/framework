namespace Activity.IntegrationTests.Classes.Flow
{
    internal class IntFlow : IFlowGuard<int>
    {
        public Task<bool> GuardAsync(int token)
        {
            return Task.FromResult(true);
        }
    }
}
