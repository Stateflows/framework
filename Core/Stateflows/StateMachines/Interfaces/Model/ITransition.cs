using Stateflows.Common;
using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface ITransition<in TEvent>
        where TEvent : Event, new()
    { }

    public interface ITransitionEffect<in TEvent> : ITransition<TEvent>
        where TEvent : Event, new()
    {
        Task EffectAsync(TEvent @event);
    }

    public interface ITransitionGuard<in TEvent> : ITransition<TEvent>
        where TEvent : Event, new()
    {
        Task<bool> GuardAsync(TEvent @event);
    }
}
