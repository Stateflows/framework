using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedCompositeStateBuilderTransitionTypedExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddTransition<TEvent, TTransition, TTargetState>(this ITypedInitializedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TTransition : Transition<TEvent>
            where TTargetState : BaseState
            => AddTransition<TEvent, TTransition>(builder, StateInfo<TTargetState>.Name);

        public static ITypedInitializedCompositeStateBuilder AddTransition<TEvent, TTransition>(this ITypedInitializedCompositeStateBuilder builder, string targetVertexName)
            where TEvent : Event, new()
            where TTransition : Transition<TEvent>
        {
            (builder as IInternal).Services.RegisterTransition<TTransition, TEvent>();

            return builder.AddTransition<TEvent>(
                targetVertexName,
                t => t.AddTransitionEvents<TTransition, TEvent>()
            );
        }

        public static ITypedInitializedCompositeStateBuilder AddTransition<TEvent, TTargetState>(this ITypedInitializedCompositeStateBuilder builder, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : BaseState
            => builder.AddTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
