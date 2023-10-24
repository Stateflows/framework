using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class TypedCompositeStateBuilderTransitionTypedExtensions
    {
        public static ITypedCompositeStateBuilder AddTransition<TEvent, TTransition, TTargetState>(this ITypedCompositeStateBuilder builder)
            where TEvent : Event
            where TTransition : Transition<TEvent>
            where TTargetState : BaseState
            => AddTransition<TEvent, TTransition>(builder, StateInfo<TTargetState>.Name);

        public static ITypedCompositeStateBuilder AddTransition<TEvent, TTransition>(this ITypedCompositeStateBuilder builder, string targetVertexName)
            where TEvent : Event
            where TTransition : Transition<TEvent>
        {
            (builder as IInternal).Services.RegisterTransition<TTransition, TEvent>();

            return builder.AddTransition<TEvent>(
                targetVertexName,
                t => t.AddTransitionEvents<TTransition, TEvent>()
            );
        }

        public static ITypedCompositeStateBuilder AddTransition<TEvent, TTargetState>(this ITypedCompositeStateBuilder builder, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event
            where TTargetState : BaseState
            => builder.AddTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
