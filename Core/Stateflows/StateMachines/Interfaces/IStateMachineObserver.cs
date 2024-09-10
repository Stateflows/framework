using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineObserver
    {
        Task BeforeStateMachineInitializeAsync(IStateMachineInitializationContext context)
            => Task.CompletedTask;

        Task AfterStateMachineInitializeAsync(IStateMachineInitializationContext context, bool initialized)
            => Task.CompletedTask;

        Task BeforeStateMachineFinalizeAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        Task AfterStateMachineFinalizeAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        Task BeforeStateInitializeAsync(IStateActionContext context)
            => Task.CompletedTask;

        Task AfterStateInitializeAsync(IStateActionContext context)
            => Task.CompletedTask;

        Task BeforeStateFinalizeAsync(IStateActionContext context)
            => Task.CompletedTask;

        Task AfterStateFinalizeAsync(IStateActionContext context)
            => Task.CompletedTask;

        Task BeforeStateEntryAsync(IStateActionContext context)
            => Task.CompletedTask;

        Task AfterStateEntryAsync(IStateActionContext context)
            => Task.CompletedTask;

        Task BeforeStateExitAsync(IStateActionContext context)
            => Task.CompletedTask;

        Task AfterStateExitAsync(IStateActionContext context)
            => Task.CompletedTask;

        Task BeforeTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context)
            => Task.CompletedTask;

        Task AfterTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, bool guardResult)
            => Task.CompletedTask;

        Task BeforeTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context)
            => Task.CompletedTask;

        Task AfterTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context)
            => Task.CompletedTask;
    }
}
