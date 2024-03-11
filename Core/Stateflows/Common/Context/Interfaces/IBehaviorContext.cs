using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Context.Interfaces
{
    public interface IBehaviorContext : ISubscriptions
    {
        BehaviorId Id { get; }

        IContextValues Values { get; }

        void Send<TEvent>(TEvent @event)
            where TEvent : Event, new();

        void Publish<TNotification>(TNotification notification)
            where TNotification : Notification, new();
    }
}
