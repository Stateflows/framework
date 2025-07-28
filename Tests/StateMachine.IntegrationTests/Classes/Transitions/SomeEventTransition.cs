using StateMachine.IntegrationTests.Classes.Events;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class SomeEventTransition : ITransitionGuard<SomeEvent>, ITransitionEffect<SomeEvent>
    {
        public static bool GuardFired = false;

        public static bool EffectFired = false;

        private readonly IStateMachineContext stateMachineContext;
        private readonly ITransitionContext transitionContext;
        private readonly Stateflows.StateMachines.IExecutionContext executionContext;
        public SomeEventTransition(
            IStateMachineContext stateMachineContext, 
            ITransitionContext transitionContext,
            Stateflows.StateMachines.IExecutionContext executionContext
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

        public Task EffectAsync(SomeEvent @event)
        {
            EffectFired =
                stateMachineContext != null &&
                transitionContext != null &&
                executionContext != null && stateMachineContext.Id.Instance != null;

            return Task.CompletedTask;
        }
    }
}
