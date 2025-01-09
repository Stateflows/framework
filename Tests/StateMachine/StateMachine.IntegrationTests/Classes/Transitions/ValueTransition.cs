namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class ValueTransition : IDefaultTransitionGuard
    {
        private readonly SourceStateValue<int> counter = new("counter");
        private readonly SourceStateValue<int?> nullable = new("nullable");
        private readonly SourceStateValue<int?> nulled = new("nulled");

        public Task<bool> GuardAsync()
        {
            var result =
                counter.TryGet(out var c) && c == 1 &&
                nullable.TryGet(out var n) && n == 3 &&
                nulled.TryGet(out var u) && u is null;
            return Task.FromResult(result);
        }
    }
}
