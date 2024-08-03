using System.Diagnostics;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedCompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        [DebuggerHidden]
        public static ITypedCompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this ITypedCompositeStateBuilder builder)
            where TElseTransition : class, IDefaultTransitionEffect
            where TTargetState : class, IVertex
            => builder.AddElseDefaultTransition<TElseTransition>(State<TTargetState>.Name);

        [DebuggerHidden]
        public static ITypedCompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this ITypedCompositeStateBuilder builder, string targetStateName)
            where TElseTransition : class, IDefaultTransitionEffect
            => (builder as IStateBuilder).AddElseDefaultTransition<TElseTransition>(targetStateName) as ITypedCompositeStateBuilder;

        [DebuggerHidden]
        public static ITypedCompositeStateBuilder AddElseDefaultTransition<TTargetState>(this ITypedCompositeStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => builder.AddElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
