using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface IDefaultTransitionEffect
    {
        Task EffectAsync();
    }

    public interface IDefaultTransitionGuard
    {
        Task<bool> GuardAsync();
    }

    public interface ITransitionEffect<in TEvent>
    {
        Task EffectAsync(TEvent @event);
    }

    public interface ITransitionGuard<in TEvent>
    {
        Task<bool> GuardAsync(TEvent @event);
    }

    public interface ITransitionGuard : IDefaultTransitionGuard, ITransitionGuard<object>
    {
        Task<bool> ITransitionGuard<object>.GuardAsync(object @event)
            => GuardAsync();
    }

    public interface ITransitionEffect : IDefaultTransitionEffect, ITransitionEffect<object>
    {
        Task ITransitionEffect<object>.EffectAsync(object @event)
            => EffectAsync();
    }
}
