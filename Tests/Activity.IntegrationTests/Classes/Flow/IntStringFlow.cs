namespace Activity.IntegrationTests.Classes.Flow
{
    internal class IntStringFlow : IntFlow, IFlowTransformation<int, string>
    {
        public Task<string> TransformAsync(int token)
        {
            return Task.FromResult(token.ToString());
        }
    }
}
