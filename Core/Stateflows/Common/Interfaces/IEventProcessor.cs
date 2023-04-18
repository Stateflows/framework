using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    internal interface IEventProcessor
    {
        string BehaviorType { get; }

        Task<bool> ProcessEventAsync<TEvent>(BehaviorId id, TEvent @event)
            where TEvent : Event, new();
    }
}
