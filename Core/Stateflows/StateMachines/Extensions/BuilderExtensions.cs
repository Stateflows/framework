using System;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Extensions
{
    internal static class BuilderExtensions
    {
        public static void AddStateMachineEvents(this IInitializedStateMachineBuilder builder, Type stateMachineType)
        {
            if (typeof(StateMachine).GetMethod(Constants.OnInitializeAsync).IsOverridenIn(stateMachineType))
            {
                builder.AddOnInitialize(c =>
                {
                    var context = (c as BaseContext).Context;
                    return context.Executor.GetStateMachine(stateMachineType, context).OnInitializeAsync();
                });
            }

            if (typeof(StateMachine).GetMethod(Constants.OnFinalizeAsync).IsOverridenIn(stateMachineType))
            {
                builder.AddOnFinalize(c =>
                {
                    var context = (c as BaseContext).Context;
                    context.Executor.GetStateMachine(stateMachineType, context)?.OnFinalizeAsync();
                });
            }

            var baseInterfaceType = typeof(IInitializedBy<>);
            foreach (var interfaceType in stateMachineType.GetInterfaces())
            {
                if (interfaceType.GetGenericTypeDefinition() == baseInterfaceType)
                {
                    var methodInfo = interfaceType.GetMethods().First(m => m.Name == "OnInitializeAsync");
                    var requestType = interfaceType.GenericTypeArguments[0];
                    var requestName = Stateflows.Common.EventInfo.GetName(requestType);
                    (builder as StateMachineBuilder).AddInitializer(requestName, c =>
                    {
                        var stateMachine = c.Executor.GetStateMachine(stateMachineType, c);
                        return methodInfo.Invoke(stateMachine, new object[] { c.Event }) as Task<bool>;
                    });
                }
            }
        }

        public static void AddStateEvents<TState, TReturn>(this IStateEvents<TReturn> builder)
            where TState : BaseState
        {
            if (typeof(BaseState).GetMethod(Constants.OnEntryAsync).IsOverridenIn<TState>())
            {
                builder.AddOnEntry(c => (c as BaseContext).Context.Executor.GetState<TState>(c)?.OnEntryAsync());
            }

            if (typeof(BaseState).GetMethod(Constants.OnExitAsync).IsOverridenIn<TState>())
            {
                builder.AddOnExit(c => (c as BaseContext).Context.Executor.GetState<TState>(c)?.OnExitAsync());
            }
        }

        public static void AddCompositeStateEvents<TCompositeState, TReturn>(this ICompositeStateEvents<TReturn> builder)
            where TCompositeState : CompositeState
        {
            if (typeof(CompositeState).GetMethod(Constants.OnInitializeAsync).IsOverridenIn<TCompositeState>())
            {
                builder.AddOnInitialize(c => (c as BaseContext).Context.Executor.GetState<TCompositeState>(c)?.OnInitializeAsync());
            }

            if (typeof(CompositeState).GetMethod(Constants.OnFinalizeAsync).IsOverridenIn<TCompositeState>())
            {
                builder.AddOnFinalize(c => (c as BaseContext).Context.Executor.GetState<TCompositeState>(c)?.OnFinalizeAsync());
            }
        }

        public static void AddElseTransitionEvents<TElseTransition, TEvent>(this IElseTransitionBuilder<TEvent> builder)
            where TElseTransition : ElseTransition<TEvent>
            where TEvent : Event, new()
        {
            if (typeof(Transition<TEvent>).GetMethod(Constants.EffectAsync).IsOverridenIn<TElseTransition>())
            {
                builder.AddEffect(c => (c as BaseContext).Context.Executor.GetElseTransition<TElseTransition, TEvent>(c)?.EffectAsync());
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
