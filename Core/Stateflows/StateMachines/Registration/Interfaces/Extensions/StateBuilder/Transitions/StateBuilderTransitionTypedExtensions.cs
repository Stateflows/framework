using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class StateBuilderTransitionTypedExtensions
    {
        public static IStateBuilder AddTransition<TEvent, TTransition, TTargetState>(this IStateBuilder builder)
            where TEvent : Event
            where TTransition : Transition<TEvent>
            where TTargetState : BaseState
            => AddTransition<TEvent, TTransition>(builder, StateInfo<TTargetState>.Name);

        public static IStateBuilder AddTransition<TEvent, TTransition>(this IStateBuilder builder, string targetVertexName)
            where TEvent : Event
            where TTransition : Transition<TEvent>
        {
            (builder as IInternal).Services.RegisterTransition<TTransition, TEvent>();

            return builder.AddTransition<TEvent>(
                targetVertexName,
                t => t.AddTransitionEvents<TTransition, TEvent>()
            );
        }

        public static IStateBuilder AddTransition<TEvent, TTargetState>(this IStateBuilder builder, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event
            where TTargetState : BaseState
            => builder.AddTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
