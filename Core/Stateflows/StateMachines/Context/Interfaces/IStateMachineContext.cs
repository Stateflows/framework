using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IStateMachineContext
    {
        StateMachineId Id { get; }

        IContextValues GlobalValues { get; }

        void Send<TEvent>(TEvent @event)
            where TEvent : Event, new();
    }
}
