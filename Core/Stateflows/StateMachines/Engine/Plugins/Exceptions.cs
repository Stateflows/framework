using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Exceptions : StateMachinePlugin
    {
        private readonly IStateMachineLocator locator;
        public Exceptions(IStateMachineLocator locator)
        {
            this.locator = locator;
        }
        
        private Task<bool> HandleException(StateMachineId stateMachineId, Exception exception)
        {
            if (locator.TryLocateStateMachine(stateMachineId, out var stateMachine))
            {
                stateMachine.SendAsync(exception);
            }
            
            return Task.FromResult(true);
        }
        
        public override Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
            => HandleException(context.StateMachine.Id, exception);

        public override Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
            => HandleException(context.StateMachine.Id, exception);

        public override Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => HandleException(context.StateMachine.Id, exception);

        public override Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => HandleException(context.StateMachine.Id, exception);

        public override Task<bool> OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
            => HandleException(context.StateMachine.Id, exception);

        public override Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
            => HandleException(context.StateMachine.Id, exception);

        public override Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
            => HandleException(context.StateMachine.Id, exception);

        public override Task<bool> OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
            => HandleException(context.StateMachine.Id, exception);
    }
}
