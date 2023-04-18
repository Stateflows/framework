using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineObserver
    {
        Task BeforeStateMachineInitializeAsync(IStateMachineActionContext context);
        Task AfterStateMachineInitializeAsync(IStateMachineActionContext context);
        Task BeforeStateInitializeAsync(IStateActionContext context);
        Task AfterStateInitializeAsync(IStateActionContext context);
        Task BeforeStateEntryAsync(IStateActionContext context);
        Task AfterStateEntryAsync(IStateActionContext context);
        Task BeforeStateExitAsync(IStateActionContext context);
        Task AfterStateExitAsync(IStateActionContext context);
        Task BeforeTransitionGuardAsync(IGuardContext<Event> context);
        Task AfterTransitionGuardAsync(IGuardContext<Event> context, bool guardResult);
        Task BeforeTransitionEffectAsync(ITransitionContext<Event> context);
        Task AfterTransitionEffectAsync(ITransitionContext<Event> context);
    }
}
