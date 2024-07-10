using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedStateBuilderElseTransitionTypedExtensions
    {
        public static ITypedStateBuilder AddElseTransition<TEvent, TElseTransition, TTargetState>(this ITypedStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
            where TTargetState : class, IBaseState
            => AddElseTransition<TEvent, TElseTransition>(builder, State<TTargetState>.Name);

        public static ITypedStateBuilder AddElseTransition<TEvent, TElseTransition>(this ITypedStateBuilder builder, string targetVertexName)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
        {
            (builder as IInternal).Services.RegisterElseTransition2<TElseTransition, TEvent>();

            return builder.AddElseTransition<TEvent>(
                targetVertexName,
                t => t.AddElseTransitionEvents2<TElseTransition, TEvent>()
            );
        }

        public static ITypedStateBuilder AddElseTransition<TEvent, TTargetState>(this ITypedStateBuilder builder, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : class, IBaseState
            => builder.AddElseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
