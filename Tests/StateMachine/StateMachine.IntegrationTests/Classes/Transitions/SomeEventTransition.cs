using Stateflows.StateMachines.Context.Interfaces;

namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class SomeEventTransition : ITransitionGuard<SomeEvent>, ITransitionEffect<SomeEvent>
    {
        public static bool GuardFired = false;

        public static bool EffectFired = false;

        private readonly IStateMachineContext stateMachineContext;
        public SomeEventTransition(IStateMachineContext stateMachineContext)
        {
            this.stateMachineContext = stateMachineContext;
        }

        public static void Reset()
        {
            GuardFired = false;
            EffectFired = false;
        }

        public Task<bool> GuardAsync(SomeEvent @event)
        {
            GuardFired = stateMachineContext != null && stateMachineContext.Id.Instance != null;
            return Task.FromResult(true);
        }

        public Task EffectAsync(SomeEvent @event)
        {
            EffectFired = stateMachineContext != null && stateMachineContext.Id.Instance != null;
            return Task.CompletedTask;
        }
    }
}
