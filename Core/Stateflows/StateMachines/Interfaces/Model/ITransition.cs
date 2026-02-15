using System.Threading.Tasks;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IDefaultTransition : ITransition<Completion>;

    public interface IDefaultTransitionEffect : IDefaultTransition, ITransitionEffect<Completion>
    {
        Task ITransitionEffect<Completion>.EffectAsync(Completion @event)
            => EffectAsync();
        
        Task EffectAsync();
    }

    public interface IDefaultTransitionGuard : IAbstractGuard, IDefaultTransition, ITransitionGuard<Completion>
    {
        Task<bool> IAbstractGuard<Completion>.GuardAsync(Completion @event)
            => GuardAsync();
        
        // Task<bool> GuardAsync();
    }

    public interface IDefaultTransitionDefinition : IDefaultTransition, ITransitionDefinition<Completion>
    {
        void ITransitionDefinition<Completion>.Build(ITransitionBuilder<Completion> builder)
            => Build(builder as IDefaultTransitionBuilder);
        
        void Build(IDefaultTransitionBuilder builder);
    }

    public interface ITransition<in TEvent> : IStateMachineElement;

    public interface ITransitionEffect<in TEvent> : ITransition<TEvent>
    {
        Task EffectAsync(TEvent @event);
    }

    public interface ITransitionGuard<in TEvent> : ITransition<TEvent>, IAbstractGuard<TEvent>;

    public interface ITransitionDefinition<TEvent> : ITransition<TEvent>
    {
        void Build(ITransitionBuilder<TEvent> builder);
    }

    public interface ITransitionGuard : IDefaultTransitionGuard, ITransitionGuard<object>;

    public interface ITransitionEffect : IDefaultTransitionEffect, ITransitionEffect<object>
    {
        Task ITransitionEffect<object>.EffectAsync(object @event)
            => EffectAsync();
    }

    public interface ITransitionAction : IAbstractAction, ITransitionEffect
    {
        Task IDefaultTransitionEffect.EffectAsync()
            => ExecuteAsync();
    }

    public interface ITransitionDefinition : IDefaultTransitionDefinition, ITransitionDefinition<object>
    {
        void ITransitionDefinition<object>.Build(ITransitionBuilder<object> builder)
            => Build(builder);
    }
}
