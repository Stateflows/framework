using Stateflows.Common;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Extensions
{
    internal static class BuilderExtensions
    {
        public static void AddStateEvents<TState, TReturn>(this IStateEvents<TReturn> builder)
            where TState : class, IBaseState
        {
            if (typeof(IStateEntry).IsAssignableFrom(typeof(TState)))
            {
                builder.AddOnEntry(c => ((c as BaseContext).Context.Executor.GetState<TState>(c) as IStateEntry)?.OnEntryAsync());
            }

            if (typeof(IStateExit).IsAssignableFrom(typeof(TState)))
            {
                builder.AddOnExit(c => ((c as BaseContext).Context.Executor.GetState<TState>(c) as IStateExit)?.OnExitAsync());
            }
        }

        public static void AddCompositeStateEvents<TCompositeState, TReturn>(this ICompositeStateEvents<TReturn> builder)
            where TCompositeState : class, IBaseCompositeState
        {
            if (typeof(ICompositeStateInitialization).IsAssignableFrom(typeof(TCompositeState)))
            {
                builder.AddOnInitialize(c => ((c as BaseContext).Context.Executor.GetState<TCompositeState>(c) as ICompositeStateInitialization)?.OnInitializeAsync());
            }

            if (typeof(ICompositeStateFinalization).IsAssignableFrom(typeof(TCompositeState)))
            {
                builder.AddOnFinalize(c => ((c as BaseContext).Context.Executor.GetState<TCompositeState>(c) as ICompositeStateFinalization)?.OnFinalizeAsync());
            }
        }

        public static void AddElseTransitionEvents<TElseTransition, TEvent>(this IElseTransitionBuilder<TEvent> builder)
            where TElseTransition : class, ITransitionEffect<TEvent>
            where TEvent : Event, new()
        {
            if (typeof(ITransitionEffect<TEvent>).IsAssignableFrom(typeof(TElseTransition)))
            {
                builder.AddEffect(c => ((c as BaseContext).Context.Executor.GetTransition<TElseTransition, TEvent>(c) as ITransitionEffect<TEvent>)?.EffectAsync(c.Event));
            }
        }

        public static void AddTransitionEvents<TTransition, TEvent>(this ITransitionBuilder<TEvent> builder)
            where TTransition : class, IBaseTransition<TEvent>
            where TEvent : Event, new()
        {
            if (typeof(ITransitionGuard<TEvent>).IsAssignableFrom(typeof(TTransition)))
            {
                builder.AddGuard(c => ((c as BaseContext).Context.Executor.GetTransition<TTransition, TEvent>(c) as ITransitionGuard<TEvent>)?.GuardAsync(c.Event));
            }

            if (typeof(ITransitionEffect<TEvent>).IsAssignableFrom(typeof(TTransition)))
            {
                builder.AddEffect(c => ((c as BaseContext).Context.Executor.GetTransition<TTransition, TEvent>(c) as ITransitionEffect<TEvent>)?.EffectAsync(c.Event));
            }
        }

        public static void AddDefaultTransitionEvents<TTransition>(this IDefaultTransitionBuilder builder)
            where TTransition : class, IBaseDefaultTransition
        {
            if (typeof(IDefaultTransitionGuard).IsAssignableFrom(typeof(TTransition)))
            {
                builder.AddGuard(c => ((c as BaseContext).Context.Executor.GetDefaultTransition<TTransition>(c) as IDefaultTransitionGuard)?.GuardAsync());
            }

            if (typeof(IDefaultTransitionEffect).IsAssignableFrom(typeof(TTransition)))
            {
                builder.AddEffect(c => ((c as BaseContext).Context.Executor.GetDefaultTransition<TTransition>(c) as IDefaultTransitionEffect)?.EffectAsync());
            }
        }

        public static void AddElseDefaultTransitionEvents<TElseTransition>(this IElseDefaultTransitionBuilder builder)
            where TElseTransition : class, IDefaultTransitionEffect
        {
            if (typeof(IDefaultTransitionEffect).IsAssignableFrom(typeof(TElseTransition)))
            {
                builder.AddEffect(c => ((c as BaseContext).Context.Executor.GetDefaultTransition<TElseTransition>(c) as IDefaultTransitionEffect)?.EffectAsync());
            }
        }
    }
}
