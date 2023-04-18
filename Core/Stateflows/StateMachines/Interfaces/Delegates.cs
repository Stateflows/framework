using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Interfaces
{
    public delegate bool GuardDelegate<TEvent>(IGuardContext<TEvent> context)
        where TEvent : Event;

    public delegate Task<bool> GuardDelegateAsync<TEvent>(IGuardContext<TEvent> context)
        where TEvent : Event;

    public delegate void StateActionDelegate(IStateActionContext context);

    public delegate Task StateActionDelegateAsync(IStateActionContext context);

    public delegate void StateMachineActionDelegate(IStateMachineActionContext context);

    public delegate Task StateMachineActionDelegateAsync(IStateMachineActionContext context);

    public delegate void EffectDelegate<TEvent>(ITransitionContext<TEvent> context)
        where TEvent : Event;

    public delegate Task EffectDelegateAsync<TEvent>(ITransitionContext<TEvent> context)
        where TEvent : Event;
}