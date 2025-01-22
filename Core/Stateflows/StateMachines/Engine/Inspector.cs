using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Engine;
using Stateflows.Common.Extensions;
using Stateflows.Common.Context.Classes;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Inspection.Classes;
using Stateflows.StateMachines.Inspection.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Inspector
    {
        private readonly Executor Executor;

        private readonly CommonInterceptor GlobalInterceptor;

        private readonly ILogger Logger;

        public Inspector(Executor executor, ILogger logger)
        {
            Executor = executor;
            Logger = logger;

            GlobalInterceptor = Executor.ServiceProvider.GetRequiredService<CommonInterceptor>();

            ExceptionHandlerFactories.AddRange(Executor.Graph.ExceptionHandlerFactories);
            ExceptionHandlerFactories.AddRange(Executor.Register.GlobalExceptionHandlerFactories);

            InterceptorFactories.AddRange(Executor.Graph.InterceptorFactories);
            InterceptorFactories.AddRange(Executor.Register.GlobalInterceptorFactories);

            ObserverFactories.AddRange(Executor.Graph.ObserverFactories);
            ObserverFactories.AddRange(Executor.Register.GlobalObserverFactories);
        }

        public ActionInspection InitializeInspection;

        public ActionInspection FinalizeInspection;

        public IDictionary<string, StateInspection> InspectionStates { get; } = new Dictionary<string, StateInspection>();

        public IDictionary<Edge, TransitionInspection> InspectionTransitions { get; } = new Dictionary<Edge, TransitionInspection>();

        public IStateMachineInspection inspection;

        public IStateMachineInspection Inspection => inspection ??= new StateMachineInspection(Executor);

        private readonly List<StateMachineExceptionHandlerFactory> ExceptionHandlerFactories = new List<StateMachineExceptionHandlerFactory>();

        private readonly List<StateMachineInterceptorFactory> InterceptorFactories = new List<StateMachineInterceptorFactory>();

        private readonly List<StateMachineObserverFactory> ObserverFactories = new List<StateMachineObserverFactory>();

        private IEnumerable<IStateMachineObserver> observers;
        private IEnumerable<IStateMachineObserver> Observers
            => observers ??= ObserverFactories.Select(t => t(Executor.ServiceProvider));

        private IEnumerable<IStateMachineInterceptor> interceptors;
        private IEnumerable<IStateMachineInterceptor> Interceptors
            => interceptors ??= InterceptorFactories.Select(t => t(Executor.ServiceProvider));

        private IEnumerable<IStateMachineExceptionHandler> exceptionHandlers;
        private IEnumerable<IStateMachineExceptionHandler> ExceptionHandlers
            => exceptionHandlers ??= ExceptionHandlerFactories.Select(t => t(Executor.ServiceProvider));

        public IEnumerable<IStateMachineInspector> inspectors;
        public IEnumerable<IStateMachineInspector> Inspectors
            => inspectors ??= Executor.ServiceProvider.GetService<IEnumerable<IStateMachineInspector>>();

        public IEnumerable<IStateMachinePlugin> plugins;
        public IEnumerable<IStateMachinePlugin> Plugins
            => plugins ??= Executor.ServiceProvider.GetService<IEnumerable<IStateMachinePlugin>>();

        public async Task BeforeStateMachineInitializeAsync(StateMachineInitializationContext context)
        {
            if (InitializeInspection != null)
            {
                InitializeInspection.Active = true;
            }

            await Plugins.RunSafe(o => o.BeforeStateMachineInitializeAsync(context), nameof(BeforeStateMachineInitializeAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeStateMachineInitializeAsync(context), nameof(BeforeStateMachineInitializeAsync), Logger);
            await Observers.RunSafe(o => o.BeforeStateMachineInitializeAsync(context), nameof(BeforeStateMachineInitializeAsync), Logger);
        }

        public async Task AfterStateMachineInitializeAsync(StateMachineInitializationContext context, bool initialized)
        {
            await Observers.RunSafe(o => o.AfterStateMachineInitializeAsync(context, initialized), nameof(AfterStateMachineInitializeAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterStateMachineInitializeAsync(context, initialized), nameof(AfterStateMachineInitializeAsync), Logger);
            await Plugins.RunSafe(o => o.AfterStateMachineInitializeAsync(context, initialized), nameof(AfterStateMachineInitializeAsync), Logger);

            if (InitializeInspection != null)
            {
                InitializeInspection.Active = false;
            }
        }

        public async Task BeforeStateMachineFinalizeAsync(StateMachineActionContext context)
        {
            if (FinalizeInspection != null)
            {
                FinalizeInspection.Active = true;
            }

            await Plugins.RunSafe(o => o.BeforeStateMachineFinalizeAsync(context), nameof(BeforeStateMachineFinalizeAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeStateMachineFinalizeAsync(context), nameof(BeforeStateMachineFinalizeAsync), Logger);
            await Observers.RunSafe(o => o.BeforeStateMachineFinalizeAsync(context), nameof(BeforeStateMachineFinalizeAsync), Logger);
        }

        public async Task AfterStateMachineFinalizeAsync(StateMachineActionContext context)
        {
            await Observers.RunSafe(o => o.AfterStateMachineFinalizeAsync(context), nameof(AfterStateMachineFinalizeAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterStateMachineFinalizeAsync(context), nameof(AfterStateMachineFinalizeAsync), Logger);
            await Plugins.RunSafe(o => o.AfterStateMachineFinalizeAsync(context), nameof(AfterStateMachineFinalizeAsync), Logger);

            if (FinalizeInspection != null)
            {
                FinalizeInspection.Active = false;
            }
        }

        public async Task BeforeStateInitializeAsync(StateActionContext context)
        {
            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.BeginAction(Constants.Initialize);
            }

            await Plugins.RunSafe(o => o.BeforeStateInitializeAsync(context), nameof(BeforeStateInitializeAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeStateInitializeAsync(context), nameof(BeforeStateInitializeAsync), Logger);
            await Observers.RunSafe(o => o.BeforeStateInitializeAsync(context), nameof(BeforeStateInitializeAsync), Logger);
        }

        public async Task AfterStateInitializeAsync(StateActionContext context)
        {
            await Observers.RunSafe(o => o.AfterStateInitializeAsync(context), nameof(AfterStateInitializeAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterStateInitializeAsync(context), nameof(AfterStateInitializeAsync), Logger);
            await Plugins.RunSafe(o => o.AfterStateInitializeAsync(context), nameof(AfterStateInitializeAsync), Logger);

            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.EndAction(Constants.Initialize);
            }
        }

        public async Task BeforeStateFinalizeAsync(StateActionContext context)
        {
            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.BeginAction(Constants.Finalize);
            }

            await Plugins.RunSafe(o => o.BeforeStateFinalizeAsync(context), nameof(BeforeStateFinalizeAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeStateFinalizeAsync(context), nameof(BeforeStateFinalizeAsync), Logger);
            await Observers.RunSafe(o => o.BeforeStateFinalizeAsync(context), nameof(BeforeStateFinalizeAsync), Logger);
        }

        public async Task AfterStateFinalizeAsync(StateActionContext context)
        {
            await Observers.RunSafe(o => o.AfterStateFinalizeAsync(context), nameof(AfterStateFinalizeAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterStateFinalizeAsync(context), nameof(AfterStateFinalizeAsync), Logger);
            await Plugins.RunSafe(o => o.AfterStateFinalizeAsync(context), nameof(AfterStateFinalizeAsync), Logger);

            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.EndAction(Constants.Finalize);
            }
        }

        public async Task BeforeStateEntryAsync(StateActionContext context)
        {
            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.BeginAction(Constants.Entry);
            }

            await Plugins.RunSafe(o => o.BeforeStateEntryAsync(context), nameof(BeforeStateEntryAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeStateEntryAsync(context), nameof(BeforeStateEntryAsync), Logger);
            await Observers.RunSafe(o => o.BeforeStateEntryAsync(context), nameof(BeforeStateEntryAsync), Logger);
        }

        public async Task AfterStateEntryAsync(StateActionContext context)
        {
            await Observers.RunSafe(o => o.AfterStateEntryAsync(context), nameof(AfterStateEntryAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterStateEntryAsync(context), nameof(AfterStateEntryAsync), Logger);
            await Plugins.RunSafe(o => o.AfterStateEntryAsync(context), nameof(AfterStateEntryAsync), Logger);

            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.EndAction(Constants.Entry);
            }
        }

        public async Task BeforeStateExitAsync(StateActionContext context)
        {
            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.BeginAction(Constants.Exit);
            }

            await Plugins.RunSafe(o => o.BeforeStateExitAsync(context), nameof(BeforeStateExitAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeStateExitAsync(context), nameof(BeforeStateExitAsync), Logger);
            await Observers.RunSafe(o => o.BeforeStateExitAsync(context), nameof(BeforeStateExitAsync), Logger);
        }

        public async Task AfterStateExitAsync(StateActionContext context)
        {
            await Observers.RunSafe(o => o.AfterStateExitAsync(context), nameof(AfterStateExitAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeStateExitAsync(context), nameof(BeforeStateExitAsync), Logger);
            await Plugins.RunSafe(o => o.AfterStateExitAsync(context), nameof(AfterStateExitAsync), Logger);

            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.EndAction(Constants.Exit);
            }
        }

        public async Task BeforeTransitionGuardAsync<TEvent>(GuardContext<TEvent> context)

        {
            if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
            {
                (stateInspection.Guard as ActionInspection).Active = true;
            }

            await Plugins.RunSafe(o => o.BeforeTransitionGuardAsync(context), nameof(BeforeTransitionGuardAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeTransitionGuardAsync(context), nameof(BeforeTransitionGuardAsync), Logger);
            await Observers.RunSafe(o => o.BeforeTransitionGuardAsync(context), nameof(BeforeTransitionGuardAsync), Logger);
        }

        public async Task AfterGuardAsync<TEvent>(GuardContext<TEvent> context, bool guardResult)

        {
            await Observers.RunSafe(o => o.AfterTransitionGuardAsync(context, guardResult), nameof(AfterGuardAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterTransitionGuardAsync(context, guardResult), nameof(AfterGuardAsync), Logger);
            await Plugins.RunSafe(o => o.AfterTransitionGuardAsync(context, guardResult), nameof(AfterGuardAsync), Logger);

            if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
            {
                (stateInspection.Guard as ActionInspection).Active = false;
            }
        }

        public async Task BeforeEffectAsync<TEvent>(TransitionContext<TEvent> context)

        {
            if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
            {
                (stateInspection.Effect as ActionInspection).Active = true;
            }

            await Plugins.RunSafe(o => o.BeforeTransitionEffectAsync(context), nameof(BeforeEffectAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeTransitionEffectAsync(context), nameof(BeforeEffectAsync), Logger);
            await Observers.RunSafe(o => o.BeforeTransitionEffectAsync(context), nameof(BeforeEffectAsync), Logger);
        }

        public async Task AfterEffectAsync<TEvent>(TransitionContext<TEvent> context)

        {
            await Observers.RunSafe(o => o.AfterTransitionEffectAsync(context), nameof(AfterEffectAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterTransitionEffectAsync(context), nameof(AfterEffectAsync), Logger);
            await Plugins.RunSafe(o => o.AfterTransitionEffectAsync(context), nameof(AfterEffectAsync), Logger);

            if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
            {
                (stateInspection.Effect as ActionInspection).Active = false;
            }
        }

        public async Task AfterHydrateAsync(StateMachineActionContext context)
        {
            await Plugins.RunSafe(i => i.AfterHydrateAsync(context), nameof(AfterHydrateAsync), Logger);
            await GlobalInterceptor.AfterHydrateAsync(new BehaviorActionContext(context.Context.Context, Executor.ServiceProvider));
            await Interceptors.RunSafe(i => i.AfterHydrateAsync(context), nameof(AfterHydrateAsync), Logger);
        }

        public async Task BeforeDehydrateAsync(StateMachineActionContext context)
        {
            await Interceptors.RunSafe(i => i.BeforeDehydrateAsync(context), nameof(BeforeDehydrateAsync), Logger);
            await GlobalInterceptor.BeforeDehydrateAsync(new BehaviorActionContext(context.Context.Context, Executor.ServiceProvider));
            await Plugins.RunSafe(i => i.BeforeDehydrateAsync(context), nameof(BeforeDehydrateAsync), Logger);
        }

        public async Task<bool> BeforeProcessEventAsync<TEvent>(Context.Classes.EventContext<TEvent> context)
        {
            var plugin = await Plugins.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEventAsync), Logger);
            var global = await GlobalInterceptor.BeforeProcessEventAsync(
                new Common.Context.Classes.EventContext<TEvent>(context.Context.Context, Executor.ServiceProvider, context.Event)
            );
            var local = await Interceptors.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEventAsync), Logger);

            return global && local && plugin;
        }

        public async Task AfterProcessEventAsync<TEvent>(Context.Classes.EventContext<TEvent> context)
        {
            await Interceptors.RunSafe(i => i.AfterProcessEventAsync(context), nameof(AfterProcessEventAsync), Logger);
            await GlobalInterceptor.AfterProcessEventAsync(new Common.Context.Classes.EventContext<TEvent>(context.Context.Context, Executor.ServiceProvider, context.Event));
            await Plugins.RunSafe(i => i.AfterProcessEventAsync(context), nameof(AfterProcessEventAsync), Logger);
        }

        private static bool ShouldPropagateException(Graph graph, bool handled)
            => !handled;

        public async Task<bool> OnStateMachineInitializationExceptionAsync(StateMachineInitializationContext context, Exception exception)
        {
            var handled = await ExceptionHandlers.RunSafe(h => h.OnStateMachineInitializationExceptionAsync(context, exception), nameof(OnStateMachineInitializationExceptionAsync), Logger, false);
            await Inspectors.RunSafe(i => i.OnStateMachineInitializationExceptionAsync(context, exception), nameof(OnStateMachineInitializationExceptionAsync), Logger);

            if (!handled)
            {
                await Plugins.RunSafe(i => i.OnStateMachineInitializationExceptionAsync(context, exception), nameof(OnStateMachineInitializationExceptionAsync), Logger);
            }

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }

        public async Task<bool> OnStateMachineFinalizationExceptionAsync(StateMachineActionContext context, Exception exception)
        {
            var handled = await ExceptionHandlers.RunSafe(h => h.OnStateMachineFinalizationExceptionAsync(context, exception), nameof(OnStateMachineFinalizationExceptionAsync), Logger, false);
            await Inspectors.RunSafe(i => i.OnStateMachineFinalizationExceptionAsync(context, exception), nameof(OnStateMachineFinalizationExceptionAsync), Logger);

            if (!handled)
            {
                await Plugins.RunSafe(i => i.OnStateMachineFinalizationExceptionAsync(context, exception), nameof(OnStateMachineFinalizationExceptionAsync), Logger);
            }

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }

        public async Task<bool> OnTransitionGuardExceptionAsync<TEvent>(GuardContext<TEvent> context, Exception exception)

        {
            var handled = await ExceptionHandlers.RunSafe(h => h.OnTransitionGuardExceptionAsync(context, exception), nameof(OnTransitionGuardExceptionAsync), Logger, false);
            await Inspectors.RunSafe(i => i.OnTransitionGuardExceptionAsync(context, exception), nameof(OnTransitionGuardExceptionAsync), Logger);

            if (!handled)
            {
                await Plugins.RunSafe(i => i.OnTransitionGuardExceptionAsync(context, exception), nameof(OnTransitionGuardExceptionAsync), Logger);
            }

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }

        public async Task<bool> OnTransitionEffectExceptionAsync<TEvent>(TransitionContext<TEvent> context, Exception exception)

        {
            var handled = await ExceptionHandlers.RunSafe(h => h.OnTransitionEffectExceptionAsync(context, exception), nameof(OnTransitionEffectExceptionAsync), Logger, false);
            await Inspectors.RunSafe(h => h.OnTransitionEffectExceptionAsync(context, exception), nameof(OnTransitionEffectExceptionAsync), Logger);

            if (!handled)
            {
                await Plugins.RunSafe(h => h.OnTransitionEffectExceptionAsync(context, exception), nameof(OnTransitionEffectExceptionAsync), Logger);
            }

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }

        public async Task<bool> OnStateInitializeExceptionAsync(StateActionContext context, Exception exception)
        {
            var handled = await ExceptionHandlers.RunSafe(h => h.OnStateInitializationExceptionAsync(context, exception), nameof(OnStateInitializeExceptionAsync), Logger, false);
            await Inspectors.RunSafe(i => i.OnStateInitializeExceptionAsync(context, exception), nameof(OnStateInitializeExceptionAsync), Logger);

            if (!handled)
            {
                await Plugins.RunSafe(i => i.OnStateInitializationExceptionAsync(context, exception), nameof(OnStateInitializeExceptionAsync), Logger);
            }

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }

        public async Task<bool> OnStateFinalizeExceptionAsync(StateActionContext context, Exception exception)
        {
            var handled = await ExceptionHandlers.RunSafe(h => h.OnStateFinalizationExceptionAsync(context, exception), nameof(OnStateFinalizeExceptionAsync), Logger, false);
            await Inspectors.RunSafe(i => i.OnStateFinalizeExceptionAsync(context, exception), nameof(OnStateFinalizeExceptionAsync), Logger);

            if (!handled)
            {
                await Plugins.RunSafe(i => i.OnStateFinalizationExceptionAsync(context, exception), nameof(OnStateFinalizeExceptionAsync), Logger);
            }

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }

        public async Task<bool> OnStateEntryExceptionAsync(StateActionContext context, Exception exception)
        {
            var handled = await ExceptionHandlers.RunSafe(h => h.OnStateEntryExceptionAsync(context, exception), nameof(OnStateEntryExceptionAsync), Logger, false);
            await Inspectors.RunSafe(i => i.OnStateEntryExceptionAsync(context, exception), nameof(OnStateEntryExceptionAsync), Logger);

            if (!handled)
            {
                await Plugins.RunSafe(i => i.OnStateEntryExceptionAsync(context, exception), nameof(OnStateEntryExceptionAsync), Logger);
            }

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }

        public async Task<bool> OnStateExitExceptionAsync(StateActionContext context, Exception exception)
        {
            var handled = await ExceptionHandlers.RunSafe(h => h.OnStateExitExceptionAsync(context, exception), nameof(OnStateExitExceptionAsync), Logger, false);
            await Inspectors.RunSafe(i => i.OnStateExitExceptionAsync(context, exception), nameof(OnStateExitExceptionAsync), Logger);

            if (!handled)
            {
                await Plugins.RunSafe(i => i.OnStateExitExceptionAsync(context, exception), nameof(OnStateExitExceptionAsync), Logger);
            }

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.AddException(exception);
            }

            return handled;
        }
    }
}
