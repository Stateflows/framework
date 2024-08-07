﻿using Stateflows.Common;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class InternalTransition : ITransitionEffect<SomeEvent>
    {
        private readonly GlobalValue<int> counter = new("counter");

        public Task EffectAsync(SomeEvent @event)
        {
            if (!counter.TryGet(out var c))
            {
                c = 0;
            }

            counter.Set(c + 1);

            return Task.CompletedTask;
        }
    }
}
