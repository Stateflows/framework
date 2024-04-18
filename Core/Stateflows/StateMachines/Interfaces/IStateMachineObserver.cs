using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineObserver
    {
        Task BeforeStateMachineInitializeAsync(IStateMachineInitializationContext context);
        Task AfterStateMachineInitializeAsync(IStateMachineInitializationContext context);
        Task BeforeStateMachineFinalizeAsync(IStateMachineActionContext context);
        Task AfterStateMachineFinalizeAsync(IStateMachineActionContext context);
        Task BeforeStateInitializeAsync(IStateActionContext context);
        Task AfterStateInitializeAsync(IStateActionContext context);
        Task BeforeStateFinalizeAsync(IStateActionContext context);
        Task AfterStateFinalizeAsync(IStateActionContext context);
        Task BeforeStateEntryAsync(IStateActionContext context);
        Task AfterStateEntryAsync(IStateActionContext context);
        Task BeforeStateExitAsync(IStateActionContext context);
        Task AfterStateExitAsync(IStateActionContext context);
        Task BeforeTransitionGuardAsync<TEvent>(IGuardContext<TEvent> context);
        Task AfterTransitionGuardAsync<TEvent>(IGuardContext<TEvent> context, bool guardResult);
        Task BeforeTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context);
        Task AfterTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context);
    }
}
