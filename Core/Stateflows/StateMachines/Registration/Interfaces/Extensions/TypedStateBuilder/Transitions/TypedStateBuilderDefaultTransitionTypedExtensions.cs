using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedStateBuilderDefaultTransitionTypedExtensions
    {
        //public static ITypedStateBuilder AddDefaultTransition<TTransition, TTargetState>(this ITypedStateBuilder builder)
        //    where TTransition : Transition<CompletionEvent>
        //    where TTargetState : BaseState
        //    => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        //public static ITypedStateBuilder AddDefaultTransition<TTransition>(this ITypedStateBuilder builder, string targetVertexName)
        //    where TTransition : Transition<CompletionEvent>
        //    => builder.AddTransition<CompletionEvent, TTransition>(targetVertexName);

        //public static ITypedStateBuilder AddDefaultTransition<TTargetState>(this ITypedStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
        //    where TTargetState : BaseState
        //    => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
