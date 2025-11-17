using System.Threading.Tasks;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IDefaultTransition : ITransition<Completion>
    { }

    public interface IDefaultTransitionEffect : IDefaultTransition, ITransitionEffect<Completion>
    {
        Task ITransitionEffect<Completion>.EffectAsync(Completion @event)
            => EffectAsync();
        
        Task EffectAsync();
    }

    public interface IDefaultTransitionGuard : IDefaultTransition, ITransitionGuard<Completion>
    {
        Task<bool> IGuard<Completion>.GuardAsync(Completion @event)
            => GuardAsync();
        
        Task<bool> GuardAsync();
    }

    public interface IDefaultTransitionDefinition : IDefaultTransition, ITransitionDefinition<Completion>
    {
        void ITransitionDefinition<Completion>.Build(ITransitionBuilder<Completion> builder)
            => Build(builder as IDefaultTransitionBuilder);
        
        void Build(IDefaultTransitionBuilder builder);
    }

    public interface ITransition<in TEvent> { }

    public interface ITransitionEffect<in TEvent> : ITransition<TEvent>
    {
        Task EffectAsync(TEvent @event);
    }

    public interface ITransitionGuard<in TEvent> : ITransition<TEvent>, IGuard<TEvent>;

    public interface ITransitionDefinition<TEvent> : ITransition<TEvent>
    {
        void Build(ITransitionBuilder<TEvent> builder);
    }

    public interface ITransitionGuard : IDefaultTransitionGuard, ITransitionGuard<object>
    {
        Task<bool> IGuard<object>.GuardAsync(object @event)
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
