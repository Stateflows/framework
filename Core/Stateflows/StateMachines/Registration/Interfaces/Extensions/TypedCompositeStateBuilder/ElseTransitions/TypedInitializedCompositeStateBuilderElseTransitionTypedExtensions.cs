using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedInitializedCompositeStateBuilderElseTransitionTypedExtensions
    {
        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition, TTargetState>(this ITypedInitializedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
            where TTargetState : class, IVertex
            => AddElseTransition<TEvent, TElseTransition>(builder, State<TTargetState>.Name);

        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition>(this ITypedInitializedCompositeStateBuilder builder, string targetStateName)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
            => (builder as IStateBuilder).AddElseTransition<TEvent, TElseTransition>(targetStateName) as ITypedInitializedCompositeStateBuilder;

        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddElseTransition<TEvent, TTargetState>(this ITypedInitializedCompositeStateBuilder builder, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : class, IVertex
            => builder.AddElseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
