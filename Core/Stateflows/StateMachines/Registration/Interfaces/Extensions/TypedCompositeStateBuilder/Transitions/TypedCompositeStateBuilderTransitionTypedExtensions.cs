using Stateflows.Common;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class TypedCompositeStateBuilderTransitionTypedExtensions
    {
        public static ITypedCompositeStateBuilder AddTransition<TEvent, TTransition, TTargetState>(this ITypedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TTransition : Transition<TEvent>
            where TTargetState : State
            => AddTransition<TEvent, TTransition>(builder, StateInfo<TTargetState>.Name);

        public static ITypedCompositeStateBuilder AddTransition<TEvent, TTransition>(this ITypedCompositeStateBuilder builder, string targetStateName)
            where TEvent : Event, new()
            where TTransition : Transition<TEvent>
        {
            var self = builder as IStateBuilderInternal;
            self.Services.RegisterTransition<TTransition, TEvent>();

            self.AddTransition<TEvent>(
                targetStateName,
                t => t.AddTransitionEvents<TTransition, TEvent>()
            );

            return builder;
        }

        public static ITypedCompositeStateBuilder AddTransition<TEvent, TTargetState>(this ITypedCompositeStateBuilder builder, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : State
            => builder.AddTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
