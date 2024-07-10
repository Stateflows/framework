using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class InitializedCompositeStateBuilderElseTransitionTypedExtensions
    {
        public static IInitializedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition, TTargetState>(this IInitializedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
            where TTargetState : class, IBaseState
            => AddElseTransition<TEvent, TElseTransition>(builder, State<TTargetState>.Name);

        [DebuggerHidden]
        public static IInitializedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition>(this IInitializedCompositeStateBuilder builder, string targetVertexName)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
            => (builder as IStateBuilder).AddElseTransition<TEvent, TElseTransition>(targetVertexName) as IInitializedCompositeStateBuilder;

        public static IInitializedCompositeStateBuilder AddElseTransition<TEvent, TTargetState>(this IInitializedCompositeStateBuilder builder, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : class, IBaseState
            => builder.AddElseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
