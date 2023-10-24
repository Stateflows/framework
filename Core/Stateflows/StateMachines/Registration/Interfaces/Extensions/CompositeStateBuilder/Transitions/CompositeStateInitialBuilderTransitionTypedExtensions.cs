using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class CompositeStateInitialBuilderTransitionTypedExtensions
    {
        public static ICompositeStateInitialBuilder AddTransition<TEvent, TTransition, TTargetState>(this ICompositeStateInitialBuilder builder)
            where TEvent : Event
            where TTransition : Transition<TEvent>
            where TTargetState : State
            => AddTransition<TEvent, TTransition>(builder, StateInfo<TTargetState>.Name);

        public static ICompositeStateInitialBuilder AddTransition<TEvent, TTransition>(this ICompositeStateInitialBuilder builder, string targetVertexName)
            where TEvent : Event
            where TTransition : Transition<TEvent>
        {
            (builder as IInternal).Services.RegisterTransition<TTransition, TEvent>();

            return builder.AddTransition<TEvent>(
                targetVertexName,
                t => t.AddTransitionEvents<TTransition, TEvent>()
            );
        }

        public static ICompositeStateInitialBuilder AddTransition<TEvent, TTargetState>(this ICompositeStateInitialBuilder builder, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event
            where TTargetState : State
            => builder.AddTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
