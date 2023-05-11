using System;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Extensions
{
    internal static class BuilderExtensions
    {
        public static void AddStateMachineEvents(this IStateMachineBuilder builder, Type stateMachineType)
        {
            if (typeof(StateMachine).GetMethod(Constants.OnInitializeAsync).IsOverridenIn(stateMachineType))
            {
                builder.AddOnInitialize(c =>
                {
                    var context = (c as BaseContext).Context;
                    context.Executor.GetStateMachine(stateMachineType, context)?.OnInitializeAsync();
                });
            }
        }

        public static void AddStateEvents<TState, TReturn>(this IStateEventsBuilderBase<TReturn> builder)
            where TState : State
        {
            if (typeof(State).GetMethod(Constants.OnEntryAsync).IsOverridenIn<TState>())
            {
                builder.AddOnEntry(c => (c as BaseContext).Context.Executor.GetState<TState>(c)?.OnEntryAsync());
            }

            if (typeof(State).GetMethod(Constants.OnExitAsync).IsOverridenIn<TState>())
            {
                builder.AddOnExit(c => (c as BaseContext).Context.Executor.GetState<TState>(c)?.OnExitAsync());
            }
        }

        public static void AddCompositeStateEvents<TCompositeState, TReturn>(this ICompositeStateBuilderBase<TReturn> builder)
            where TCompositeState : CompositeState
        {
            if (typeof(State).GetMethod(Constants.OnInitializeAsync).IsOverridenIn<TCompositeState>())
            {
                builder.AddOnInitialize(c => (c as BaseContext).Context.Executor.GetState<TCompositeState>(c)?.OnInitializeAsync());
            }
        }

        public static void AddTransitionEvents<TTransition, TEvent>(this ITransitionBuilder<TEvent> builder)
            where TTransition : Transition<TEvent>
            where TEvent : Event, new()
        {
            if (typeof(Transition<TEvent>).GetMethod(Constants.GuardAsync).IsOverridenIn<TTransition>())
            {
                builder.AddGuard(c => (c as BaseContext).Context.Executor.GetTransition<TTransition, TEvent>(c)?.GuardAsync());
            }

            if (typeof(Transition<TEvent>).GetMethod(Constants.EffectAsync).IsOverridenIn<TTransition>())
            {
                builder.AddEffect(c => (c as BaseContext).Context.Executor.GetTransition<TTransition, TEvent>(c)?.EffectAsync());
            }
        }
    }
}
