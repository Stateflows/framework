using Stateflows.Common;
using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface IBaseTransition<in TEvent>
        where TEvent : Event, new()
    {
        Task EffectAsync(TEvent @event)
            => Task.CompletedTask;
    }

    public interface IElseTransition<in TEvent> : IBaseTransition<TEvent>
        where TEvent : Event, new()
    { }

    public interface ITransition<in TEvent> : IBaseTransition<TEvent>
        where TEvent : Event, new()
    {
        Task<bool> GuardAsync(TEvent @event)
            => Task.FromResult(true);
    }

    public interface IBaseDefaultTransition
    {
        public virtual Task EffectAsync()
            => Task.CompletedTask;
    }

    public interface IElseDefaultTransition : IBaseDefaultTransition
    { }

    public interface IDefaultTransition : IBaseDefaultTransition
    {
        public virtual Task<bool> GuardAsync()
            => Task.FromResult(true);
    }
}
