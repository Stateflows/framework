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
                builder.AddOnEntry(c => (((BaseContext)c).Context.Executor.GetState<TState>(c) as IStateEntry)?.OnEntryAsync());
            }

            if (typeof(IStateExit).IsAssignableFrom(typeof(TState)))
            {
                builder.AddOnExit(c => (((BaseContext)c).Context.Executor.GetState<TState>(c) as IStateExit)?.OnExitAsync());
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
                builder.AddOnEntry(c => (((BaseContext)c).Context.Executor.GetState<TCompositeState>(c) as IStateEntry)?.OnEntryAsync());
            }

            if (typeof(IStateExit).IsAssignableFrom(typeof(TCompositeState)))
            {
                builder.AddOnExit(c => (((BaseContext)c).Context.Executor.GetState<TCompositeState>(c) as IStateExit)?.OnExitAsync());
            }
            
            if (typeof(ICompositeStateInitialization).IsAssignableFrom(typeof(TCompositeState)))
            {
                builder.AddOnInitialize(c => (((BaseContext)c).Context.Executor.GetState<TCompositeState>(c) as ICompositeStateInitialization)?.OnInitializeAsync());
            }

            if (typeof(ICompositeStateFinalization).IsAssignableFrom(typeof(TCompositeState)))
            {
                builder.AddOnFinalize(c => (((BaseContext)c).Context.Executor.GetState<TCompositeState>(c) as ICompositeStateFinalization)?.OnFinalizeAsync());
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
                builder.AddOnEntry(c => (((BaseContext)c).Context.Executor.GetState<TOrthogonalState>(c) as IStateEntry)?.OnEntryAsync());
            }

            if (typeof(IStateExit).IsAssignableFrom(typeof(TOrthogonalState)))
            {
                builder.AddOnExit(c => (((BaseContext)c).Context.Executor.GetState<TOrthogonalState>(c) as IStateExit)?.OnExitAsync());
            }
            
            if (typeof(ICompositeStateInitialization).IsAssignableFrom(typeof(TOrthogonalState)))
            {
                builder.AddOnInitialize(c => (((BaseContext)c).Context.Executor.GetState<TOrthogonalState>(c) as ICompositeStateInitialization)?.OnInitializeAsync());
            }

            if (typeof(ICompositeStateFinalization).IsAssignableFrom(typeof(TOrthogonalState)))
            {
                builder.AddOnFinalize(c => (((BaseContext)c).Context.Executor.GetState<TOrthogonalState>(c) as ICompositeStateFinalization)?.OnFinalizeAsync());
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
                builder.AddEffect(c => (((BaseContext)c).Context.Executor.GetTransition<TElseTransition, TEvent>(c) as ITransitionEffect<TEvent>)?.EffectAsync(c.Event));
            }
        }

        public static void AddTransitionEvents<TTransition, TEvent>(this ITransitionBuilder<TEvent> builder)
            where TTransition : class, ITransition<TEvent>
        {
            if (typeof(ITransitionGuard<TEvent>).IsAssignableFrom(typeof(TTransition)))
            {
                builder.AddGuard(c => (((BaseContext)c).Context.Executor.GetTransition<TTransition, TEvent>(c) as ITransitionGuard<TEvent>)?.GuardAsync(c.Event));
            }

            if (typeof(ITransitionEffect<TEvent>).IsAssignableFrom(typeof(TTransition)))
            {
                builder.AddEffect(c => (((BaseContext)c).Context.Executor.GetTransition<TTransition, TEvent>(c) as ITransitionEffect<TEvent>)?.EffectAsync(c.Event));
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
                builder.AddGuard(c => (((BaseContext)c).Context.Executor.GetDefaultTransition<TTransition>(c) as IDefaultTransitionGuard)?.GuardAsync());
            }

            if (typeof(IDefaultTransitionEffect).IsAssignableFrom(typeof(TTransition)))
            {
                builder.AddEffect(c => (((BaseContext)c).Context.Executor.GetDefaultTransition<TTransition>(c) as IDefaultTransitionEffect)?.EffectAsync());
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
                builder.AddEffect(c => (((BaseContext)c).Context.Executor.GetDefaultTransition<TElseTransition>(c) as IDefaultTransitionEffect)?.EffectAsync());
            }
        }
    }
}
