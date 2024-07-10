using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class StateBuilderTransitionTypedExtensions
    {
        public static IStateBuilder AddTransition<TEvent, TTransition, TTargetState>(this IStateBuilder builder)
            where TEvent : Event, new()
            where TTransition : class, IBaseTransition<TEvent>
            where TTargetState : class, IVertex
            => AddTransition<TEvent, TTransition>(builder, State<TTargetState>.Name);

        public static IStateBuilder AddTransition<TEvent, TTransition>(this IStateBuilder builder, string targetVertexName)
            where TEvent : Event, new()
            where TTransition : class, IBaseTransition<TEvent>
        {
            (builder as IInternal).Services.RegisterTransition2<TTransition, TEvent>();

            return builder.AddTransition<TEvent>(
                targetVertexName,
                t => t.AddTransitionEvents2<TTransition, TEvent>()
            );
        }

        public static IStateBuilder AddTransition<TEvent, TTargetState>(this IStateBuilder builder, TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : class, IVertex
            => builder.AddTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
