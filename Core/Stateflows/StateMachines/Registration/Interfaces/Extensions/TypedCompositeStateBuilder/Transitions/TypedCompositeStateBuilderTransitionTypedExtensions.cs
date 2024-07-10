using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedCompositeStateBuilderTransitionTypedExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddTransition<TEvent, TTransition, TTargetState>(this ITypedInitializedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TTransition : class, IBaseTransition<TEvent>
            where TTargetState : class, IVertex
            => AddTransition<TEvent, TTransition>(builder, State<TTargetState>.Name);

        public static ITypedInitializedCompositeStateBuilder AddTransition<TEvent, TTransition>(this ITypedInitializedCompositeStateBuilder builder, string targetVertexName)
            where TEvent : Event, new()
            where TTransition : class, IBaseTransition<TEvent>
        {
            (builder as IInternal).Services.RegisterTransition2<TTransition, TEvent>();

            return builder.AddTransition<TEvent>(
                targetVertexName,
                t => t.AddTransitionEvents2<TTransition, TEvent>()
            );
        }

        public static ITypedInitializedCompositeStateBuilder AddTransition<TEvent, TTargetState>(this ITypedInitializedCompositeStateBuilder builder, TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : class, IVertex
            => builder.AddTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
