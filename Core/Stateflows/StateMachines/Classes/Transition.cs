using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class Transition<TEvent>
        where TEvent : Event
    {
        public IStateContext SourceState { get; internal set; }

        public IStateContext TargetState { get; internal set; }

        public IStateMachineContext StateMachine { get; internal set; }

        public TEvent Event { get; internal set; }

        public virtual Task<bool> GuardAsync()
            => Task.FromResult(true);

        public virtual Task EffectAsync()
            => Task.CompletedTask;
    }
}
