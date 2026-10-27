using System.Threading.Tasks;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Marks an automatic transition evaluated on completion rather than an externally supplied event.</summary>
    public interface IDefaultTransition : ITransition<Completion>;

    /// <summary>Supplies an event-independent effect for a completion transition.</summary>
    public interface IDefaultTransitionEffect : IDefaultTransition, ITransitionEffect<Completion>
    {
        /// <inheritdoc/>
        Task ITransitionEffect<Completion>.EffectAsync(Completion @event)
            => EffectAsync();
        
        /// <summary>Executes the effect after the completion transition has been selected.</summary>
        /// <returns>A task representing completion of the effect.</returns>
        Task EffectAsync();
    }

    /// <summary>Supplies an event-independent predicate for a completion transition.</summary>
    public interface IDefaultTransitionGuard : IAbstractGuard, IDefaultTransition, ITransitionGuard<Completion>
    {
        /// <inheritdoc/>
        Task<bool> IAbstractGuard<Completion>.GuardAsync(Completion @event)
            => GuardAsync();
        
        // Task<bool> GuardAsync();
    }

    /// <summary>Configures guards and effects for a completion transition using its specialized builder.</summary>
    public interface IDefaultTransitionDefinition : IDefaultTransition, ITransitionDefinition<Completion>
    {
        /// <inheritdoc/>
        void ITransitionDefinition<Completion>.Build(ITransitionBuilder<Completion> builder)
            => Build(builder as IDefaultTransitionBuilder);
        
        /// <summary>Defines the completion transition's guard and effect behavior.</summary>
        /// <param name="builder">The builder scoped to this completion transition.</param>
        void Build(IDefaultTransitionBuilder builder);
    }

    /// <summary>Marks a typed transition definition, guard, or effect.</summary>
    /// <typeparam name="TEvent">The event payload handled by the transition.</typeparam>
    public interface ITransition<in TEvent> : IStateMachineElement;

    /// <summary>Supplies the effect awaited when a transition is taken.</summary>
    /// <typeparam name="TEvent">The trigger payload type.</typeparam>
    public interface ITransitionEffect<in TEvent> : ITransition<TEvent>
    {
        /// <summary>Executes the effect for a selected transition.</summary>
        /// <param name="event">The triggering event.</param>
        /// <returns>A task representing completion of the effect.</returns>
        Task EffectAsync(TEvent @event);
    }

    /// <summary>Supplies a predicate whose true result makes the candidate transition eligible.</summary>
    /// <typeparam name="TEvent">The trigger payload type.</typeparam>
    public interface ITransitionGuard<in TEvent> : ITransition<TEvent>, IAbstractGuard<TEvent>;

    /// <summary>Configures a typed transition's guards and effects during registration.</summary>
    /// <typeparam name="TEvent">The trigger payload type.</typeparam>
    public interface ITransitionDefinition<TEvent> : ITransition<TEvent>
    {
        /// <summary>Defines behavior for this transition without changing its source or target.</summary>
        /// <param name="builder">The builder scoped to this transition.</param>
        void Build(ITransitionBuilder<TEvent> builder);
    }

    /// <summary>Shares an event-independent predicate between ordinary and completion transitions.</summary>
    public interface ITransitionGuard : IDefaultTransitionGuard, ITransitionGuard<object>;

    /// <summary>Shares an event-independent effect between ordinary and completion transitions.</summary>
    public interface ITransitionEffect : IDefaultTransitionEffect, ITransitionEffect<object>
    {
        /// <inheritdoc/>
        Task ITransitionEffect<object>.EffectAsync(object @event)
            => EffectAsync();
    }

    /// <summary>Adapts an abstract action to the transition-effect contract.</summary>
    public interface ITransitionAction : IAbstractAction, ITransitionEffect
    {
        /// <inheritdoc/>
        Task IDefaultTransitionEffect.EffectAsync()
            => ExecuteAsync();
    }

    /// <summary>Shares one definition between event-independent and completion transitions.</summary>
    public interface ITransitionDefinition : IDefaultTransitionDefinition, ITransitionDefinition<object>
    {
        /// <inheritdoc/>
        void ITransitionDefinition<object>.Build(ITransitionBuilder<object> builder)
            => Build(builder);
    }
}
