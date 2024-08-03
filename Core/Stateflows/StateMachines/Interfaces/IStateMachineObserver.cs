using System.Threading.Tasks;
using Stateflows.Common;
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

        Task BeforeTransitionGuardAsync(IGuardContext<Event> context)
            => Task.CompletedTask;

        Task AfterTransitionGuardAsync(IGuardContext<Event> context, bool guardResult)
            => Task.CompletedTask;

        Task BeforeTransitionEffectAsync(ITransitionContext<Event> context)
            => Task.CompletedTask;

        Task AfterTransitionEffectAsync(ITransitionContext<Event> context)
            => Task.CompletedTask;
    }
}
