using System;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Sync;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Events;

namespace Stateflows.StateMachines.Extensions
{
    public static class BuilderExtensions
    {
        //public static void AddStateMachineEvents(this IInitializedStateMachineBuilder builder, Type stateMachineType)
        //{
        //    if (typeof(StateMachine).GetMethod(nameof(StateMachine.OnInitializeAsync)).IsOverridenIn(stateMachineType))
        //    {
        //        builder.AddInitializer(c =>
        //        {
        //            var context = (c as BaseContext).Context;
        //            return context.Executor.GetStateMachine(stateMachineType, context).OnInitializeAsync();
        //        });
        //    }

        //    if (typeof(StateMachine).GetMethod(nameof(StateMachine.OnFinalizeAsync)).IsOverridenIn(stateMachineType))
        //    {
        //        builder.AddOnFinalize(c =>
        //        {
        //            var context = (c as BaseContext).Context;
        //            context.Executor.GetStateMachine(stateMachineType, context)?.OnFinalizeAsync();
        //        });
        //    }

        //    var baseInterfaceType = typeof(IInitializedBy<>);
        //    foreach (var interfaceType in stateMachineType.GetInterfaces())
        //    {
        //        if (interfaceType.GetGenericTypeDefinition() == baseInterfaceType)
        //        {
        //            var methodInfo = interfaceType.GetMethods().First(m => m.Name == "OnInitializeAsync");
        //            var requestType = interfaceType.GenericTypeArguments[0];
        //            var requestName = Stateflows.Common.EventInfo.GetName(requestType);
        //            (builder as StateMachineBuilder).AddInitializer(requestType, requestName, c =>
        //            {
        //                var stateMachine = c.Executor.GetStateMachine(stateMachineType, c);
        //                return methodInfo.Invoke(stateMachine, new object[] { c.Event }) as Task<bool>;
        //            });
        //        }
        //    }
        //}

        //public static void AddStateEvents<TState, TReturn>(this IStateEvents<TReturn> builder)
        //    where TState : BaseState
        //{
        //    if (typeof(BaseState).GetMethod(nameof(BaseState.OnEntryAsync)).IsOverridenIn<TState>())
        //    {
        //        builder.AddOnEntry(c => (c as BaseContext).Context.Executor.GetState<TState>(c)?.OnEntryAsync());
        //    }

        //    if (typeof(BaseState).GetMethod(nameof(BaseState.OnExitAsync)).IsOverridenIn<TState>())
        //    {
        //        builder.AddOnExit(c => (c as BaseContext).Context.Executor.GetState<TState>(c)?.OnExitAsync());
        //    }
        //}

        public static void AddStateEvents2<TState, TReturn>(this IStateEvents<TReturn> builder)
            where TState : class, IBaseState
        {
            if (typeof(IStateEntry).IsAssignableFrom(typeof(TState)))
            {
                builder.AddOnEntry(c => ((c as BaseContext).Context.Executor.GetState2<TState>(c) as IStateEntry)?.OnEntryAsync());
            }

            if (typeof(IStateExit).IsAssignableFrom(typeof(TState)))
            {
                builder.AddOnExit(c => ((c as BaseContext).Context.Executor.GetState2<TState>(c) as IStateExit)?.OnExitAsync());
            }
        }

        //public static void AddCompositeStateEvents<TCompositeState, TReturn>(this ICompositeStateEvents<TReturn> builder)
        //    where TCompositeState : CompositeState
        //{
        //    if (typeof(CompositeState).GetMethod(nameof(CompositeState.OnInitializeAsync)).IsOverridenIn<TCompositeState>())
        //    {
        //        builder.AddOnInitialize(c => (c as BaseContext).Context.Executor.GetState<TCompositeState>(c)?.OnInitializeAsync());
        //    }

        //    if (typeof(CompositeState).GetMethod(nameof(CompositeState.OnFinalizeAsync)).IsOverridenIn<TCompositeState>())
        //    {
        //        builder.AddOnFinalize(c => (c as BaseContext).Context.Executor.GetState<TCompositeState>(c)?.OnFinalizeAsync());
        //    }
        //}

        public static void AddCompositeStateEvents2<TCompositeState, TReturn>(this ICompositeStateEvents<TReturn> builder)
            where TCompositeState : class, IBaseCompositeState
        {
            if (typeof(ICompositeStateInitialization).IsAssignableFrom(typeof(TCompositeState)))
            {
                builder.AddOnInitialize(c => ((c as BaseContext).Context.Executor.GetState2<TCompositeState>(c) as ICompositeStateInitialization)?.OnInitializeAsync());
            }

            if (typeof(ICompositeStateFinalization).IsAssignableFrom(typeof(TCompositeState)))
            {
                builder.AddOnFinalize(c => ((c as BaseContext).Context.Executor.GetState2<TCompositeState>(c) as ICompositeStateFinalization)?.OnFinalizeAsync());
            }
        }

        //public static void AddElseTransitionEvents<TElseTransition, TEvent>(this IElseTransitionBuilder<TEvent> builder)
        //    where TElseTransition : ElseTransition<TEvent>
        //    where TEvent : Event, new()
        //{
        //    if (typeof(BaseTransition<TEvent>).GetMethod(nameof(BaseTransition<TEvent>.EffectAsync)).IsOverridenIn<TElseTransition>())
        //    {
        //        builder.AddEffect(c => (c as BaseContext).Context.Executor.GetElseTransition<TElseTransition, TEvent>(c)?.EffectAsync(c.Event));
        //    }
        //}

        public static void AddElseTransitionEvents2<TElseTransition, TEvent>(this IElseTransitionBuilder<TEvent> builder)
            where TElseTransition : class, ITransitionEffect<TEvent>
            where TEvent : Event, new()
        {
            if (typeof(ITransitionEffect<TEvent>).IsAssignableFrom(typeof(TElseTransition)))
            {
                builder.AddEffect(c => ((c as BaseContext).Context.Executor.GetTransition2<TElseTransition, TEvent>(c) as ITransitionEffect<TEvent>)?.EffectAsync(c.Event));
            }
        }

        //public static void AddElseDefaultTransitionEvents2<TElseDefaultTransition>(this IElseDefaultTransitionBuilder builder)
        //    where TElseDefaultTransition : class, ITransitionEffect<CompletionEvent>
        //{
        //    if (typeof(IBaseDefaultTransition).GetMethod(nameof(IBaseDefaultTransition.EffectAsync)).IsImplementedIn<TElseDefaultTransition>())
        //    {
        //        builder.AddEffect(c => (c as BaseContext).Context.Executor.GetElseDefaultTransition2<TElseDefaultTransition>(c)?.EffectAsync());
        //    }
        //}

        //public static void AddTransitionEvents<TTransition, TEvent>(this ITransitionBuilder<TEvent> builder)
        //    where TTransition : Transition<TEvent>
        //    where TEvent : Event, new()
        //{
        //    if (typeof(BaseTransition<TEvent>).GetMethod(nameof(BaseTransition<TEvent>.GuardAsync)).IsOverridenIn<TTransition>())
        //    {
        //        builder.AddGuard(c => (c as BaseContext).Context.Executor.GetTransition<TTransition, TEvent>(c)?.GuardAsync(c.Event));
        //    }

        //    if (typeof(BaseTransition<TEvent>).GetMethod(nameof(BaseTransition<TEvent>.EffectAsync)).IsOverridenIn<TTransition>())
        //    {
        //        builder.AddEffect(c => (c as BaseContext).Context.Executor.GetTransition<TTransition, TEvent>(c)?.EffectAsync(c.Event));
        //    }
        //}

        public static void AddTransitionEvents2<TTransition, TEvent>(this ITransitionBuilder<TEvent> builder)
            where TTransition : class, IBaseTransition<TEvent>
            where TEvent : Event, new()
        {
            if (typeof(ITransitionGuard<TEvent>).IsAssignableFrom(typeof(TTransition)))
            {
                builder.AddGuard(c => ((c as BaseContext).Context.Executor.GetTransition2<TTransition, TEvent>(c) as ITransitionGuard<TEvent>)?.GuardAsync(c.Event));
            }

            if (typeof(ITransitionEffect<TEvent>).IsAssignableFrom(typeof(TTransition)))
            {
                builder.AddEffect(c => ((c as BaseContext).Context.Executor.GetTransition2<TTransition, TEvent>(c) as ITransitionEffect<TEvent>)?.EffectAsync(c.Event));
            }
        }

        public static void AddDefaultTransitionEvents2<TTransition>(this IDefaultTransitionBuilder builder)
            where TTransition : class, IBaseDefaultTransition
        {
            if (typeof(IDefaultTransitionGuard).IsAssignableFrom(typeof(TTransition)))
            {
                builder.AddGuard(c => ((c as BaseContext).Context.Executor.GetDefaultTransition2<TTransition>(c) as IDefaultTransitionGuard)?.GuardAsync());
            }

            if (typeof(IDefaultTransitionEffect).IsAssignableFrom(typeof(TTransition)))
            {
                builder.AddEffect(c => ((c as BaseContext).Context.Executor.GetDefaultTransition2<TTransition>(c) as IDefaultTransitionEffect)?.EffectAsync());
            }
        }
    }
}
