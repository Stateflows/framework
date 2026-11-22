using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Extensions
{
    internal static class BuilderExtensions
    {
        private static void StateType(object builder, System.Type type, string operation)
        {
            var vertex = ((IVertexBuilder)builder).Vertex;
            var count = operation switch
            {
                Constants.Entry => vertex.Entry.Actions.Count,
                Constants.Exit => vertex.Exit.Actions.Count,
                Constants.Initialization => vertex.Initialize.Actions.Count,
                Constants.Finalization => vertex.Finalize.Actions.Count,
                _ => throw new System.ArgumentOutOfRangeException(nameof(operation))
            };
            vertex.Graph.RecordStateActionType(vertex, operation, count - 1, type);
        }

        private static void TransitionType(object builder, System.Type type, string operation)
        {
            var edge = ((IEdgeBuilder)builder).Edge;
            var count = operation == Constants.Guard ? edge.Guards.Actions.Count : edge.Effects.Actions.Count;
            edge.Graph.RecordTransitionActionType(edge, operation, count - 1, type);
        }
        [DebuggerHidden]
        public static TReturn AddStateEvents<TState, TReturn>(this IStateBuilder builder)
            where TState : class, IState
        {
            ((IVertexBuilder)builder).Vertex.ImplementationType = typeof(TState);
            var stateType = typeof(TState);
            if (typeof(IStateEntry).IsAssignableFrom(stateType))
            {
                builder.AddOnEntry(async c => await ((IStateEntry)await ((BaseContext)c).Context.Executor.GetStateAsync<TState>(c)).OnEntryAsync());
                StateType(builder, stateType, Constants.Entry);
            }

            if (typeof(IStateExit).IsAssignableFrom(stateType))
            {
                builder.AddOnExit(async c => await ((IStateExit)await ((BaseContext)c).Context.Executor.GetStateAsync<TState>(c)).OnExitAsync());
                StateType(builder, stateType, Constants.Exit);
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
            ((IVertexBuilder)builder).Vertex.ImplementationType = typeof(TCompositeState);
            var compositeStateType = typeof(TCompositeState);
            if (typeof(IStateEntry).IsAssignableFrom(compositeStateType))
            {
                builder.AddOnEntry(async c => await ((IStateEntry)await ((BaseContext)c).Context.Executor.GetStateAsync<TCompositeState>(c)).OnEntryAsync());
                StateType(builder, compositeStateType, Constants.Entry);
            }

            if (typeof(IStateExit).IsAssignableFrom(compositeStateType))
            {
                builder.AddOnExit(async c => await ((IStateExit)await ((BaseContext)c).Context.Executor.GetStateAsync<TCompositeState>(c)).OnExitAsync());
                StateType(builder, compositeStateType, Constants.Exit);
            }
            
            if (typeof(ICompositeStateInitialization).IsAssignableFrom(compositeStateType))
            {
                builder.AddOnInitialize(async c => await ((ICompositeStateInitialization)await ((BaseContext)c).Context.Executor.GetStateAsync<TCompositeState>(c)).OnInitializeAsync());
                StateType(builder, compositeStateType, Constants.Initialization);
            }

            if (typeof(ICompositeStateFinalization).IsAssignableFrom(compositeStateType))
            {
                builder.AddOnFinalize(async c => await ((ICompositeStateFinalization)await ((BaseContext)c).Context.Executor.GetStateAsync<TCompositeState>(c)).OnFinalizeAsync());
                StateType(builder, compositeStateType, Constants.Finalization);
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
            ((IVertexBuilder)builder).Vertex.ImplementationType = typeof(TOrthogonalState);
            var orthogonalStateType = typeof(TOrthogonalState);
            if (typeof(IStateEntry).IsAssignableFrom(orthogonalStateType))
            {
                builder.AddOnEntry(async c => await ((IStateEntry)await ((BaseContext)c).Context.Executor.GetStateAsync<TOrthogonalState>(c)).OnEntryAsync());
                StateType(builder, orthogonalStateType, Constants.Entry);
            }

            if (typeof(IStateExit).IsAssignableFrom(orthogonalStateType))
            {
                builder.AddOnExit(async c => await ((IStateExit)await ((BaseContext)c).Context.Executor.GetStateAsync<TOrthogonalState>(c)).OnExitAsync());
                StateType(builder, orthogonalStateType, Constants.Exit);
            }
            
            if (typeof(ICompositeStateInitialization).IsAssignableFrom(orthogonalStateType))
            {
                builder.AddOnInitialize(async c => await ((ICompositeStateInitialization)await ((BaseContext)c).Context.Executor.GetStateAsync<TOrthogonalState>(c)).OnInitializeAsync());
                StateType(builder, orthogonalStateType, Constants.Initialization);
            }

            if (typeof(ICompositeStateFinalization).IsAssignableFrom(orthogonalStateType))
            {
                builder.AddOnFinalize(async c => await ((ICompositeStateFinalization)await ((BaseContext)c).Context.Executor.GetStateAsync<TOrthogonalState>(c)).OnFinalizeAsync());
                StateType(builder, orthogonalStateType, Constants.Finalization);
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
            ((IEdgeBuilder)builder).Edge.ImplementationType = typeof(TElseTransition);
            if (typeof(ITransitionEffect<TEvent>).IsAssignableFrom(typeof(TElseTransition)))
            {
                builder.AddEffect(async c => await (await ((BaseContext)c).Context.Executor.GetTransitionAsync<TElseTransition, TEvent>(c)).EffectAsync(c.Event));
                TransitionType(builder, typeof(TElseTransition), Constants.Effect);
            }
        }

        [DebuggerHidden]
        public static void AddTransitionEvents<TTransition, TEvent>(this ITransitionBuilder<TEvent> builder)
            where TTransition : class, ITransition<TEvent>
        {
            ((IEdgeBuilder)builder).Edge.ImplementationType = typeof(TTransition);
            var transitionType = typeof(TTransition);
            if (typeof(ITransitionGuard<TEvent>).IsAssignableFrom(transitionType))
            {
                builder.AddGuard(async c => await ((ITransitionGuard<TEvent>)await ((BaseContext)c).Context.Executor.GetTransitionAsync<TTransition, TEvent>(c)).GuardAsync(c.Event));
                TransitionType(builder, transitionType, Constants.Guard);
            }

            if (typeof(ITransitionEffect<TEvent>).IsAssignableFrom(transitionType))
            {
                builder.AddEffect(async c => await ((ITransitionEffect<TEvent>)await ((BaseContext)c).Context.Executor.GetTransitionAsync<TTransition, TEvent>(c)).EffectAsync(c.Event));
                TransitionType(builder, transitionType, Constants.Effect);
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
                TransitionType(builder, transitionType, Constants.Guard);
            }

            if (typeof(IDefaultTransitionEffect).IsAssignableFrom(transitionType))
            {
                builder.AddEffect(async c => await ((IDefaultTransitionEffect)await ((BaseContext)c).Context.Executor.GetDefaultTransitionAsync<TTransition>(c)).EffectAsync());
                TransitionType(builder, transitionType, Constants.Effect);
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
                TransitionType(builder, typeof(TElseTransition), Constants.Effect);
            }
        }
    }
}
