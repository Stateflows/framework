using Stateflows.Common;
using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface IBaseTransition<in TEvent>
        where TEvent : Event, new()
    { }

    public interface ITransitionEffect<in TEvent> : IBaseTransition<TEvent>
        where TEvent : Event, new()
    {
        Task EffectAsync(TEvent @event);
    }

    public interface ITransitionGuard<in TEvent> : IBaseTransition<TEvent>
        where TEvent : Event, new()
    {
        Task<bool> GuardAsync(TEvent @event);
    }

    public interface IBaseDefaultTransition
    { }

    public interface IDefaultTransitionEffect : IBaseDefaultTransition
    {
        Task EffectAsync();
    }

    public interface IDefaultTransitionGuard : IBaseDefaultTransition
    {
        Task<bool> GuardAsync();
    }
}
