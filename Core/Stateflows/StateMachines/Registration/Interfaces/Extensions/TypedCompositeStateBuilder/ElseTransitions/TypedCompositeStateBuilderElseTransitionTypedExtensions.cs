using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedCompositeStateBuilderElseTransitionTypedExtensions
    {
        [DebuggerHidden]
        public static ITypedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition, TTargetState>(this ITypedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
            where TTargetState : class, IVertex
            => AddElseTransition<TEvent, TElseTransition>(builder, State<TTargetState>.Name);

        [DebuggerHidden]
        public static ITypedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition>(this ITypedCompositeStateBuilder builder, string targetStateName)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
            => (builder as IStateBuilder).AddElseTransition<TEvent, TElseTransition>(targetStateName) as ITypedCompositeStateBuilder;

        [DebuggerHidden]
        public static ITypedCompositeStateBuilder AddElseTransition<TEvent, TTargetState>(this ITypedCompositeStateBuilder builder, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : class, IVertex
            => builder.AddElseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
