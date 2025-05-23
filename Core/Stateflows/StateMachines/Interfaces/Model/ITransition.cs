using System.Threading.Tasks;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IDefaultTransition { }

    public interface IDefaultTransitionEffect : IDefaultTransition
    {
        Task EffectAsync();
    }

    public interface IDefaultTransitionGuard : IDefaultTransition
    {
        Task<bool> GuardAsync();
    }

    public interface IDefaultTransitionDefinition : IDefaultTransition
    {
        void Build(IDefaultTransitionBuilder builder);
    }

    public interface ITransition<in TEvent> { }

    public interface ITransitionEffect<in TEvent> : ITransition<TEvent>
    {
        Task EffectAsync(TEvent @event);
    }

    public interface ITransitionGuard<in TEvent> : ITransition<TEvent>
    {
        Task<bool> GuardAsync(TEvent @event);
    }

    public interface ITransitionDefinition<TEvent> : ITransition<TEvent>
    {
        void Build(ITransitionBuilder<TEvent> builder);
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

    public interface ITransitionDefinition : IDefaultTransitionDefinition, ITransitionDefinition<object>
    {
        void ITransitionDefinition<object>.Build(ITransitionBuilder<object> builder)
            => Build(builder);
    }
}
