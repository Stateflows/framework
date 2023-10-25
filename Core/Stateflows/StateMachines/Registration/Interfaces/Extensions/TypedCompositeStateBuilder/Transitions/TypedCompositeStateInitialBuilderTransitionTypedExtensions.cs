using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class TypedCompositeStateInitialBuilderTransitionTypedExtensions
    {
        public static ITypedCompositeStateInitialBuilder AddTransition<TEvent, TTransition, TTargetState>(this ITypedCompositeStateInitialBuilder builder)
            where TEvent : Event, new()
            where TTransition : Transition<TEvent>
            where TTargetState : BaseState
            => AddTransition<TEvent, TTransition>(builder, StateInfo<TTargetState>.Name);

        public static ITypedCompositeStateInitialBuilder AddTransition<TEvent, TTransition>(this ITypedCompositeStateInitialBuilder builder, string targetVertexName)
            where TEvent : Event, new()
            where TTransition : Transition<TEvent>
        {
            (builder as IInternal).Services.RegisterTransition<TTransition, TEvent>();

            return builder.AddTransition<TEvent>(
                targetVertexName,
                t => t.AddTransitionEvents<TTransition, TEvent>()
            );
        }

        public static ITypedCompositeStateInitialBuilder AddTransition<TEvent, TTargetState>(this ITypedCompositeStateInitialBuilder builder, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : BaseState
            => builder.AddTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
