using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Context.Interfaces
{
    public interface IBehaviorContext
    {
        BehaviorId Id { get; }

        IContextValues Values { get; }

        void Send<TEvent>(TEvent @event)
            where TEvent : Event, new();
    }
}
