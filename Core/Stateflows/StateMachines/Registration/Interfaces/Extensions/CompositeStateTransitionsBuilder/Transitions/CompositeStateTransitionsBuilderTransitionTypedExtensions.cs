using Stateflows.Common;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class CompositeStateTransitionsBuilderTransitionTypedExtensions
    {
        public static ICompositeStateTransitionsBuilder AddTransition<TEvent, TTransition, TTargetState>(this ICompositeStateTransitionsBuilder builder)
            where TEvent : Event, new()
            where TTransition : Transition<TEvent>
            where TTargetState : State
            => AddTransition<TEvent, TTransition>(builder, StateInfo<TTargetState>.Name);

        public static ICompositeStateTransitionsBuilder AddTransition<TEvent, TTransition>(this ICompositeStateTransitionsBuilder builder, string targetStateName)
            where TEvent : Event, new()
            where TTransition : Transition<TEvent>
        {
            var self = builder as IStateBuilderInternal;
            self.Services.RegisterTransition<TTransition, TEvent>();

            self.AddTransition<TEvent>(
                targetStateName,
                t => t
                    .AddGuard(c => (c as BaseContext).Context.Executor.ServiceProvider.GetTransition<TTransition, TEvent>(c)?.GuardAsync())
                    .AddEffect(c => (c as BaseContext).Context.Executor.ServiceProvider.GetTransition<TTransition, TEvent>(c)?.EffectAsync())
            );

            return builder;
        }

        public static ICompositeStateTransitionsBuilder AddTransition<TEvent, TTargetState>(this ICompositeStateTransitionsBuilder builder, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : State
            => builder.AddTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
