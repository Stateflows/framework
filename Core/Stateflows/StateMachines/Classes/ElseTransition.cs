using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.StateMachines
{
    public abstract class ElseTransition<TEvent> : BaseTransition<TEvent>
        where TEvent : Event, new()
    {
        public override sealed Task<bool> GuardAsync()
            => Task.FromResult(true);
    }
}
