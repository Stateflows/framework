using Stateflows.Common;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedStateBuilderDefaultTransitionTypedExtensions
    {
        public static ITypedStateBuilder AddDefaultTransition<TTransition, TTargetState>(this ITypedStateBuilder builder)
            where TTransition : class, IBaseDefaultTransition
            where TTargetState : class, IVertex
            => builder.AddDefaultTransition<TTransition>(State<TTargetState>.Name);

        public static ITypedStateBuilder AddDefaultTransition<TTransition>(this ITypedStateBuilder builder, string targetVertexName)
            where TTransition : class, IBaseDefaultTransition
        {
            (builder as IInternal).Services.RegisterDefaultTransition2<TTransition>();

            return builder.AddDefaultTransition(
                targetVertexName,
                t => t.AddDefaultTransitionEvents2<TTransition>()
            );
        }
        //=> builder.AddTransition<CompletionEvent, TTransition>(targetVertexName);

        public static ITypedStateBuilder AddDefaultTransition<TTargetState>(this ITypedStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => builder.AddDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
