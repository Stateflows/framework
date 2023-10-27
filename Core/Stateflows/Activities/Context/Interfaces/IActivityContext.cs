using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IActivityContext
    {
        ActivityId Id { get; }

        IContextValues Values { get; }

        void Send<TEvent>(TEvent @event)
            where TEvent : Event, new();
    }
}
