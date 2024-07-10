using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class InitializedCompositeStateBuilderTransitionTypedExtensions
    {
        public static IInitializedCompositeStateBuilder AddTransition<TEvent, TTransition, TTargetState>(this IInitializedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TTransition : class, IBaseTransition<TEvent>
            where TTargetState : class, IBaseState
            => AddTransition<TEvent, TTransition>(builder, State<TTargetState>.Name);

        [DebuggerHidden]
        public static IInitializedCompositeStateBuilder AddTransition<TEvent, TTransition>(this IInitializedCompositeStateBuilder builder, string targetVertexName)
            where TEvent : Event, new()
            where TTransition : class, IBaseTransition<TEvent>
        {
            (builder as IInternal).Services.RegisterTransition2<TTransition, TEvent>();

            return builder.AddTransition<TEvent>(
                targetVertexName,
                t => t.AddTransitionEvents2<TTransition, TEvent>()
            );
        }

        public static IInitializedCompositeStateBuilder AddTransition<TEvent, TTargetState>(this IInitializedCompositeStateBuilder builder, TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : class, IBaseState
            => builder.AddTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
