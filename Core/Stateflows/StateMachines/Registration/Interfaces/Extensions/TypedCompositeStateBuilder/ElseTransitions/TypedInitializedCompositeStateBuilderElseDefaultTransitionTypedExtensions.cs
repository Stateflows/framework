using System.Diagnostics;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedInitializedCompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this ITypedInitializedCompositeStateBuilder builder)
            where TElseTransition : class, IDefaultTransitionEffect
            where TTargetState : class, IVertex
            => builder.AddElseDefaultTransition<TElseTransition>(State<TTargetState>.Name);

        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this ITypedInitializedCompositeStateBuilder builder, string targetStateName)
            where TElseTransition : class, IDefaultTransitionEffect
            => (builder as IStateBuilder).AddElseDefaultTransition<TElseTransition>(targetStateName) as ITypedInitializedCompositeStateBuilder;

        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddElseDefaultTransition<TTargetState>(this ITypedInitializedCompositeStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => builder.AddElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
