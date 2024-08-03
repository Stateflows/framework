using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedFinalizedCompositeStateBuilderElseTransitionTypedExtensions
    {
        [DebuggerHidden]
        public static ITypedFinalizedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition, TTargetState>(this ITypedFinalizedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
            where TTargetState : class, IVertex
            => AddElseTransition<TEvent, TElseTransition>(builder, State<TTargetState>.Name);

        [DebuggerHidden]
        public static ITypedFinalizedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition>(this ITypedFinalizedCompositeStateBuilder builder, string targetStateName)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
            => (builder as IStateBuilder).AddElseTransition<TEvent, TElseTransition>(targetStateName) as ITypedFinalizedCompositeStateBuilder;

        [DebuggerHidden]
        public static ITypedFinalizedCompositeStateBuilder AddElseTransition<TEvent, TTargetState>(this ITypedFinalizedCompositeStateBuilder builder, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : class, IVertex
            => builder.AddElseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
