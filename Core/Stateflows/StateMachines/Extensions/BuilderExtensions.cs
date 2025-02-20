using Stateflows.Common.Classes;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Extensions
{
    internal static class BuilderExtensions
    {
        public static void AddStateEvents<TState, TReturn>(this IStateBuilder builder)
            where TState : class, IState
        {
            if (typeof(IStateEntry).IsAssignableFrom(typeof(TState)))
            {
                builder.AddOnEntry(async c => await ((IStateEntry)await ((BaseContext)c).Context.Executor.GetStateAsync<TState>(c)).OnEntryAsync());
            }

            if (typeof(IStateExit).IsAssignableFrom(typeof(TState)))
            {
                builder.AddOnExit(async c => await ((IStateExit)await ((BaseContext)c).Context.Executor.GetStateAsync<TState>(c)).OnExitAsync());
            }
            
            if (typeof(IStateDefinition).IsAssignableFrom(typeof(TState)))
            {
                ((IStateDefinition)StateflowsActivator.CreateUninitializedInstance<TState>()).Build(builder);
            }
        }

        public static void AddCompositeStateEvents<TCompositeState, TReturn>(this ICompositeStateBuilder builder)
            where TCompositeState : class, ICompositeState
        {
            if (typeof(IStateEntry).IsAssignableFrom(typeof(TCompositeState)))
            {
                builder.AddOnEntry(async c => await ((IStateEntry)await ((BaseContext)c).Context.Executor.GetStateAsync<TCompositeState>(c)).OnEntryAsync());
            }

            if (typeof(IStateExit).IsAssignableFrom(typeof(TCompositeState)))
            {
                builder.AddOnExit(async c => await ((IStateExit)await ((BaseContext)c).Context.Executor.GetStateAsync<TCompositeState>(c)).OnExitAsync());
            }
            
            if (typeof(ICompositeStateInitialization).IsAssignableFrom(typeof(TCompositeState)))
            {
                builder.AddOnInitialize(async c => await ((ICompositeStateInitialization)await ((BaseContext)c).Context.Executor.GetStateAsync<TCompositeState>(c)).OnInitializeAsync());
            }

            if (typeof(ICompositeStateFinalization).IsAssignableFrom(typeof(TCompositeState)))
            {
                builder.AddOnFinalize(async c => await ((ICompositeStateFinalization)await ((BaseContext)c).Context.Executor.GetStateAsync<TCompositeState>(c)).OnFinalizeAsync());
            }
            
            if (typeof(ICompositeStateDefinition).IsAssignableFrom(typeof(TCompositeState)))
            {
                ((ICompositeStateDefinition)StateflowsActivator.CreateUninitializedInstance<TCompositeState>()).Build(builder);
            }
        }

        public static void AddOrthogonalStateEvents<TOrthogonalState, TReturn>(this IOrthogonalStateBuilder builder)
            where TOrthogonalState : class, IOrthogonalState
        {
            if (typeof(IStateEntry).IsAssignableFrom(typeof(TOrthogonalState)))
            {
                builder.AddOnEntry(async c => await ((IStateEntry)await ((BaseContext)c).Context.Executor.GetStateAsync<TOrthogonalState>(c)).OnEntryAsync());
            }

            if (typeof(IStateExit).IsAssignableFrom(typeof(TOrthogonalState)))
            {
                builder.AddOnExit(async c => await ((IStateExit)await ((BaseContext)c).Context.Executor.GetStateAsync<TOrthogonalState>(c)).OnExitAsync());
            }
            
            if (typeof(ICompositeStateInitialization).IsAssignableFrom(typeof(TOrthogonalState)))
            {
                builder.AddOnInitialize(async c => await ((ICompositeStateInitialization)await ((BaseContext)c).Context.Executor.GetStateAsync<TOrthogonalState>(c)).OnInitializeAsync());
            }

            if (typeof(ICompositeStateFinalization).IsAssignableFrom(typeof(TOrthogonalState)))
            {
                builder.AddOnFinalize(async c => await ((ICompositeStateFinalization)await ((BaseContext)c).Context.Executor.GetStateAsync<TOrthogonalState>(c)).OnFinalizeAsync());
            }
            
            if (typeof(IOrthogonalStateDefinition).IsAssignableFrom(typeof(TOrthogonalState)))
            {
                ((IOrthogonalStateDefinition)StateflowsActivator.CreateUninitializedInstance<TOrthogonalState>()).Build(builder);
            }
        }

        public static void AddElseTransitionEvents<TElseTransition, TEvent>(this IElseTransitionBuilder<TEvent> builder)
            where TElseTransition : class, ITransitionEffect<TEvent>
        {
            if (typeof(ITransitionEffect<TEvent>).IsAssignableFrom(typeof(TElseTransition)))
            {
                builder.AddEffect(async c => await (await ((BaseContext)c).Context.Executor.GetTransitionAsync<TElseTransition, TEvent>(c)).EffectAsync(c.Event));
            }
        }

        public static void AddTransitionEvents<TTransition, TEvent>(this ITransitionBuilder<TEvent> builder)
            where TTransition : class, ITransition<TEvent>
        {
            if (typeof(ITransitionGuard<TEvent>).IsAssignableFrom(typeof(TTransition)))
            {
                builder.AddGuard(async c => await ((ITransitionGuard<TEvent>)await ((BaseContext)c).Context.Executor.GetTransitionAsync<TTransition, TEvent>(c)).GuardAsync(c.Event));
            }

            if (typeof(ITransitionEffect<TEvent>).IsAssignableFrom(typeof(TTransition)))
            {
                builder.AddEffect(async c => await ((ITransitionEffect<TEvent>)await ((BaseContext)c).Context.Executor.GetTransitionAsync<TTransition, TEvent>(c)).EffectAsync(c.Event));
            }
            
            if (typeof(ITransitionDefinition<TEvent>).IsAssignableFrom(typeof(TTransition)))
            {
                ((ITransitionDefinition<TEvent>)StateflowsActivator.CreateUninitializedInstance<TTransition>()).Build(builder);
            }
        }

        public static void AddDefaultTransitionEvents<TTransition>(this IDefaultTransitionBuilder builder)
            where TTransition : class, IDefaultTransition
        {
            if (typeof(IDefaultTransitionGuard).IsAssignableFrom(typeof(TTransition)))
            {
                builder.AddGuard(async c => await ((IDefaultTransitionGuard)await ((BaseContext)c).Context.Executor.GetDefaultTransitionAsync<TTransition>(c)).GuardAsync());
            }

            if (typeof(IDefaultTransitionEffect).IsAssignableFrom(typeof(TTransition)))
            {
                builder.AddEffect(async c => await ((IDefaultTransitionEffect)await ((BaseContext)c).Context.Executor.GetDefaultTransitionAsync<TTransition>(c)).EffectAsync());
            }
            
            if (typeof(IDefaultTransitionDefinition).IsAssignableFrom(typeof(TTransition)))
            {
                ((IDefaultTransitionDefinition)StateflowsActivator.CreateUninitializedInstance<TTransition>()).Build(builder);
            }
        }

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
