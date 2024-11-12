using Stateflows.Common;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class SomeEventGuard2 : ITransitionGuard<SomeEvent>
    {
        public static bool GuardFired = false;

        public static bool EffectFired = false;

        private readonly IStateMachineContext stateMachineContext;
        private readonly ITransitionContext transitionContext;
        private readonly IExecutionContext executionContext;
        public SomeEventGuard2(
            IStateMachineContext stateMachineContext, 
            ITransitionContext transitionContext, 
            IExecutionContext executionContext
        )
        {
            this.stateMachineContext = stateMachineContext;
            this.transitionContext = transitionContext;
            this.executionContext = executionContext;
        }

        public static void Reset()
        {
            GuardFired = false;
            EffectFired = false;
        }

        public Task<bool> GuardAsync(SomeEvent @event)
        {
            GuardFired =
                stateMachineContext != null &&
                transitionContext != null &&
                executionContext != null && stateMachineContext.Id.Instance != null;

            return Task.FromResult(true);
        }
    }
}
