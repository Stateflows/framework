﻿namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class ValueTransition : IDefaultTransitionGuard
    {
        private readonly SourceStateValue<int> counter = new("counter");

        public Task<bool> GuardAsync()
        {
            var result = counter.TryGet(out var c) && c == 1;
            return Task.FromResult(result);
        }
    }
}
