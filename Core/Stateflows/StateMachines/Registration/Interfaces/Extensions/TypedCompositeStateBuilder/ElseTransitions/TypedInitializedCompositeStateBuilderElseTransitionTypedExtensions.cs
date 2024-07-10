using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedInitializedCompositeStateBuilderElseTransitionTypedExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition, TTargetState>(this ITypedInitializedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
            where TTargetState : class, IVertex
            => AddElseTransition<TEvent, TElseTransition>(builder, State<TTargetState>.Name);

        public static ITypedInitializedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition>(this ITypedInitializedCompositeStateBuilder builder, string targetVertexName)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
        {
            (builder as IInternal).Services.RegisterElseTransition2<TElseTransition, TEvent>();

            return builder.AddElseTransition<TEvent>(
                targetVertexName,
                t => t.AddElseTransitionEvents2<TElseTransition, TEvent>()
            );
        }

        public static ITypedInitializedCompositeStateBuilder AddElseTransition<TEvent, TTargetState>(this ITypedInitializedCompositeStateBuilder builder, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : class, IVertex
            => builder.AddElseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
