using System;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Extensions
{
    internal static class IServiceProviderExtensions
    {
        public static TState GetState<TState>(this IServiceProvider provider, IStateActionContext context)
            where TState : State
        {
            var state = provider.GetService<TState>();

            state.StateMachine = context.StateMachine;
            state.CurrentState = context.CurrentState;

            return state;
        }

        public static TTransition GetTransition<TTransition, TEvent>(this IServiceProvider provider, ITransitionContext<TEvent> context)
            where TTransition : Transition<TEvent>
            where TEvent : Event
        {
            var transition = provider.GetService<TTransition>();
            transition.StateMachine = context.StateMachine;
            transition.SourceState = context.SourceState;
            transition.TargetState = context.TargetState;
            transition.Event = context.Event;

            return transition;
        }
    }
}
