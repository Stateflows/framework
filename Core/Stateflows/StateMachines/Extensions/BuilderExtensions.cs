using System.Diagnostics;
using Stateflows.Common.Classes;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Extensions
{
    internal static class BuilderExtensions
    {
        [DebuggerHidden]
        public static TReturn AddStateEvents<TState, TReturn>(this IStateBuilder builder)
            where TState : class, IState
        {
            var stateType = typeof(TState);
            if (typeof(IStateEntry).IsAssignableFrom(stateType))
            {
                builder.AddOnEntry(async c => await ((IStateEntry)await ((BaseContext)c).Context.Executor.GetStateAsync<TState>(c)).OnEntryAsync());
            }

            if (typeof(IStateExit).IsAssignableFrom(stateType))
            {
                builder.AddOnExit(async c => await ((IStateExit)await ((BaseContext)c).Context.Executor.GetStateAsync<TState>(c)).OnExitAsync());
            }
            
            if (typeof(IStateDefinition).IsAssignableFrom(stateType))
            {
                stateType.CallStaticMethod(nameof(IStateDefinition.Build), [ typeof(IStateBuilder) ], [ builder ]);
            }

            return (TReturn)builder;
        }

        [DebuggerHidden]
        public static TReturn AddCompositeStateEvents<TCompositeState, TReturn>(this ICompositeStateBuilder builder)
            where TCompositeState : class, ICompositeState
        {
            var compositeStateType = typeof(TCompositeState);
            if (typeof(IStateEntry).IsAssignableFrom(compositeStateType))
            {
                builder.AddOnEntry(async c => await ((IStateEntry)await ((BaseContext)c).Context.Executor.GetStateAsync<TCompositeState>(c)).OnEntryAsync());
            }

            if (typeof(IStateExit).IsAssignableFrom(compositeStateType))
            {
                builder.AddOnExit(async c => await ((IStateExit)await ((BaseContext)c).Context.Executor.GetStateAsync<TCompositeState>(c)).OnExitAsync());
            }
            
            if (typeof(ICompositeStateInitialization).IsAssignableFrom(compositeStateType))
            {
                builder.AddOnInitialize(async c => await ((ICompositeStateInitialization)await ((BaseContext)c).Context.Executor.GetStateAsync<TCompositeState>(c)).OnInitializeAsync());
            }

            if (typeof(ICompositeStateFinalization).IsAssignableFrom(compositeStateType))
            {
                builder.AddOnFinalize(async c => await ((ICompositeStateFinalization)await ((BaseContext)c).Context.Executor.GetStateAsync<TCompositeState>(c)).OnFinalizeAsync());
            }
            
            if (typeof(ICompositeStateDefinition).IsAssignableFrom(compositeStateType))
            {
                compositeStateType.CallStaticMethod(nameof(ICompositeStateDefinition.Build), [ typeof(ICompositeStateBuilder) ], [ builder ]);
            }

            return (TReturn)builder;
        }

        [DebuggerHidden]
        public static TReturn AddOrthogonalStateEvents<TOrthogonalState, TReturn>(this IOrthogonalStateBuilder builder)
            where TOrthogonalState : class, IOrthogonalState
        {
            var orthogonalStateType = typeof(TOrthogonalState);
            if (typeof(IStateEntry).IsAssignableFrom(orthogonalStateType))
            {
                builder.AddOnEntry(async c => await ((IStateEntry)await ((BaseContext)c).Context.Executor.GetStateAsync<TOrthogonalState>(c)).OnEntryAsync());
            }

            if (typeof(IStateExit).IsAssignableFrom(orthogonalStateType))
            {
                builder.AddOnExit(async c => await ((IStateExit)await ((BaseContext)c).Context.Executor.GetStateAsync<TOrthogonalState>(c)).OnExitAsync());
            }
            
            if (typeof(ICompositeStateInitialization).IsAssignableFrom(orthogonalStateType))
            {
                builder.AddOnInitialize(async c => await ((ICompositeStateInitialization)await ((BaseContext)c).Context.Executor.GetStateAsync<TOrthogonalState>(c)).OnInitializeAsync());
            }

            if (typeof(ICompositeStateFinalization).IsAssignableFrom(orthogonalStateType))
            {
                builder.AddOnFinalize(async c => await ((ICompositeStateFinalization)await ((BaseContext)c).Context.Executor.GetStateAsync<TOrthogonalState>(c)).OnFinalizeAsync());
            }
            
            if (typeof(IOrthogonalStateDefinition).IsAssignableFrom(orthogonalStateType))
            {
                orthogonalStateType.CallStaticMethod(nameof(ICompositeStateDefinition.Build), [ typeof(IOrthogonalStateBuilder) ], [ builder ]);
            }

            return (TReturn)builder;
        }

        [DebuggerHidden]
        public static void AddElseTransitionEvents<TElseTransition, TEvent>(this IElseTransitionBuilder<TEvent> builder)
            where TElseTransition : class, ITransitionEffect<TEvent>
        {
            if (typeof(ITransitionEffect<TEvent>).IsAssignableFrom(typeof(TElseTransition)))
            {
                builder.AddEffect(async c => await (await ((BaseContext)c).Context.Executor.GetTransitionAsync<TElseTransition, TEvent>(c)).EffectAsync(c.Event));
            }
        }

        [DebuggerHidden]
        public static void AddTransitionEvents<TTransition, TEvent>(this ITransitionBuilder<TEvent> builder)
            where TTransition : class, ITransition<TEvent>
        {
            var transitionType = typeof(TTransition);
            if (typeof(ITransitionGuard<TEvent>).IsAssignableFrom(transitionType))
            {
                builder.AddGuard(async c => await ((ITransitionGuard<TEvent>)await ((BaseContext)c).Context.Executor.GetTransitionAsync<TTransition, TEvent>(c)).GuardAsync(c.Event));
            }

            if (typeof(ITransitionEffect<TEvent>).IsAssignableFrom(transitionType))
            {
                builder.AddEffect(async c => await ((ITransitionEffect<TEvent>)await ((BaseContext)c).Context.Executor.GetTransitionAsync<TTransition, TEvent>(c)).EffectAsync(c.Event));
            }
            
            if (typeof(ITransitionDefinition<TEvent>).IsAssignableFrom(transitionType))
            {
                transitionType.CallStaticMethod(nameof(ITransitionDefinition<TEvent>.Build), [ typeof(ITransitionBuilder<TEvent>) ], [ builder ]);
            }
        }

        [DebuggerHidden]
        public static void AddDefaultTransitionEvents<TTransition>(this IDefaultTransitionBuilder builder)
            where TTransition : class, IDefaultTransition
        {
            var transitionType = typeof(TTransition);
            if (typeof(IDefaultTransitionGuard).IsAssignableFrom(transitionType))
            {
                builder.AddGuard(async c => await ((IDefaultTransitionGuard)await ((BaseContext)c).Context.Executor.GetDefaultTransitionAsync<TTransition>(c)).GuardAsync());
            }

            if (typeof(IDefaultTransitionEffect).IsAssignableFrom(transitionType))
            {
                builder.AddEffect(async c => await ((IDefaultTransitionEffect)await ((BaseContext)c).Context.Executor.GetDefaultTransitionAsync<TTransition>(c)).EffectAsync());
            }
            
            if (typeof(IDefaultTransitionDefinition).IsAssignableFrom(transitionType))
            {
                transitionType.CallStaticMethod(nameof(IDefaultTransitionDefinition.Build), [ typeof(IDefaultTransitionBuilder) ], [ builder ]);
            }
        }

        [DebuggerHidden]
        public static void AddElseDefaultTransitionEvents<TElseTransition>(this IElseDefaultTransitionBuilder builder)
            where TElseTransition : class, IDefaultTransitionEffect
        {
            if (typeof(IDefaultTransitionEffect).IsAssignableFrom(typeof(TElseTransition)))
            {
                builder.AddEffect(async c => await (await ((BaseContext)c).Context.Executor.GetDefaultTransitionAsync<TElseTransition>(c)).EffectAsync());
            }
        }
    }
}
