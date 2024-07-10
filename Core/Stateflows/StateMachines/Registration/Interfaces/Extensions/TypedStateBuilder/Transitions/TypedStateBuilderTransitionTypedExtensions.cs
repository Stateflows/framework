using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedStateBuilderTransitionTypedExtensions
    {
        public static ITypedStateBuilder AddTransition<TEvent, TTransition, TTargetState>(this ITypedStateBuilder builder)
            where TEvent : Event, new()
            where TTransition : class, IBaseTransition<TEvent>
            where TTargetState : class, IVertex
            => AddTransition<TEvent, TTransition>(builder, State<TTargetState>.Name);

        public static ITypedStateBuilder AddTransition<TEvent, TTransition>(this ITypedStateBuilder builder, string targetVertexName)
            where TEvent : Event, new()
            where TTransition : class, IBaseTransition<TEvent>
        {
            (builder as IInternal).Services.RegisterTransition2<TTransition, TEvent>();

            return builder.AddTransition<TEvent>(
                targetVertexName,
                t => t.AddTransitionEvents2<TTransition, TEvent>()
            );
        }

        public static ITypedStateBuilder AddTransition<TEvent, TTargetState>(this ITypedStateBuilder builder, TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : class, IVertex
            => builder.AddTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
