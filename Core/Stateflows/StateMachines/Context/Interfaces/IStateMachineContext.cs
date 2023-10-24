using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IStateMachineContext
    {
        StateMachineId Id { get; }

        IContextValues Values { get; }

        void Send<TEvent>(TEvent @event)
            where TEvent : Event;
    }
}
