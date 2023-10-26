using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
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
        private Executor Executor { get; }

        private CommonInterceptor GlobalInterceptor { get; }

        public Inspector(Executor executor)
        {
            Executor = executor;

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

            await Plugins.RunSafe(o => o.BeforeStateMachineInitializeAsync(context), nameof(BeforeStateMachineInitializeAsync));
            await Inspectors.RunSafe(i => i.BeforeStateMachineInitializeAsync(context), nameof(BeforeStateMachineInitializeAsync));
            await Observers.RunSafe(o => o.BeforeStateMachineInitializeAsync(context), nameof(BeforeStateMachineInitializeAsync));
        }

        public async Task AfterStateMachineInitializeAsync(StateMachineInitializationContext context)
        {
            await Observers.RunSafe(o => o.AfterStateMachineInitializeAsync(context), nameof(AfterStateMachineInitializeAsync));
            await Inspectors.RunSafe(i => i.AfterStateMachineInitializeAsync(context), nameof(AfterStateMachineInitializeAsync));
            await Plugins.RunSafe(o => o.AfterStateMachineInitializeAsync(context), nameof(AfterStateMachineInitializeAsync));

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

            await Plugins.RunSafe(o => o.BeforeStateMachineFinalizeAsync(context), nameof(BeforeStateMachineFinalizeAsync));
            await Inspectors.RunSafe(i => i.BeforeStateMachineFinalizeAsync(context), nameof(BeforeStateMachineFinalizeAsync));
            await Observers.RunSafe(o => o.BeforeStateMachineFinalizeAsync(context), nameof(BeforeStateMachineFinalizeAsync));
        }

        public async Task AfterStateMachineFinalizeAsync(StateMachineActionContext context)
        {
            await Observers.RunSafe(o => o.AfterStateMachineFinalizeAsync(context), nameof(AfterStateMachineFinalizeAsync));
            await Inspectors.RunSafe(i => i.AfterStateMachineFinalizeAsync(context), nameof(AfterStateMachineFinalizeAsync));
            await Plugins.RunSafe(o => o.AfterStateMachineFinalizeAsync(context), nameof(AfterStateMachineFinalizeAsync));

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

            await Plugins.RunSafe(o => o.BeforeStateInitializeAsync(context), nameof(BeforeStateInitializeAsync));
            await Inspectors.RunSafe(i => i.BeforeStateInitializeAsync(context), nameof(BeforeStateInitializeAsync));
            await Observers.RunSafe(o => o.BeforeStateInitializeAsync(context), nameof(BeforeStateInitializeAsync));
        }

        public async Task AfterStateInitializeAsync(StateActionContext context)
        {
            await Observers.RunSafe(o => o.AfterStateInitializeAsync(context), nameof(AfterStateInitializeAsync));
            await Inspectors.RunSafe(i => i.AfterStateInitializeAsync(context), nameof(AfterStateInitializeAsync));
            await Plugins.RunSafe(o => o.AfterStateInitializeAsync(context), nameof(AfterStateInitializeAsync));

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

            await Plugins.RunSafe(o => o.BeforeStateFinalizeAsync(context), nameof(BeforeStateFinalizeAsync));
            await Inspectors.RunSafe(i => i.BeforeStateFinalizeAsync(context), nameof(BeforeStateFinalizeAsync));
            await Observers.RunSafe(o => o.BeforeStateFinalizeAsync(context), nameof(BeforeStateFinalizeAsync));
        }

        public async Task AfterStateFinalizeAsync(StateActionContext context)
        {
            await Observers.RunSafe(o => o.AfterStateFinalizeAsync(context), nameof(AfterStateFinalizeAsync));
            await Inspectors.RunSafe(i => i.AfterStateFinalizeAsync(context), nameof(AfterStateFinalizeAsync));
            await Plugins.RunSafe(o => o.AfterStateFinalizeAsync(context), nameof(AfterStateFinalizeAsync));

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

            await Plugins.RunSafe(o => o.BeforeStateEntryAsync(context), nameof(BeforeStateEntryAsync));
            await Inspectors.RunSafe(i => i.BeforeStateEntryAsync(context), nameof(BeforeStateEntryAsync));
            await Observers.RunSafe(o => o.BeforeStateEntryAsync(context), nameof(BeforeStateEntryAsync));
        }

        public async Task AfterStateEntryAsync(StateActionContext context)
        {
            await Observers.RunSafe(o => o.AfterStateEntryAsync(context), nameof(AfterStateEntryAsync));
            await Inspectors.RunSafe(i => i.AfterStateEntryAsync(context), nameof(AfterStateEntryAsync));
            await Plugins.RunSafe(o => o.AfterStateEntryAsync(context), nameof(AfterStateEntryAsync));

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

            await Plugins.RunSafe(o => o.BeforeStateExitAsync(context), nameof(BeforeStateExitAsync));
            await Inspectors.RunSafe(i => i.BeforeStateExitAsync(context), nameof(BeforeStateExitAsync));
            await Observers.RunSafe(o => o.BeforeStateExitAsync(context), nameof(BeforeStateExitAsync));
        }

        public async Task AfterStateExitAsync(StateActionContext context)
        {
            await Observers.RunSafe(o => o.AfterStateExitAsync(context), nameof(AfterStateExitAsync));
            await Inspectors.RunSafe(i => i.BeforeStateExitAsync(context), nameof(BeforeStateExitAsync));
            await Plugins.RunSafe(o => o.AfterStateExitAsync(context), nameof(AfterStateExitAsync));

            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var stateInspection))
            {
                stateInspection.EndAction(Constants.Exit);
            }
        }

        public async Task BeforeTransitionGuardAsync<TEvent>(GuardContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
            {
                (stateInspection.Guard as ActionInspection).Active = true;
            }

            await Plugins.RunSafe(o => o.BeforeTransitionGuardAsync(context), nameof(BeforeTransitionGuardAsync));
            await Inspectors.RunSafe(i => i.BeforeTransitionGuardAsync(context), nameof(BeforeTransitionGuardAsync));
            await Observers.RunSafe(o => o.BeforeTransitionGuardAsync(context), nameof(BeforeTransitionGuardAsync));
        }

        public async Task AfterGuardAsync<TEvent>(GuardContext<TEvent> context, bool guardResult)
            where TEvent : Event, new()
        {
            await Observers.RunSafe(o => o.AfterTransitionGuardAsync(context, guardResult), nameof(AfterGuardAsync));
            await Inspectors.RunSafe(i => i.AfterTransitionGuardAsync(context, guardResult), nameof(AfterGuardAsync));
            await Plugins.RunSafe(o => o.AfterTransitionGuardAsync(context, guardResult), nameof(AfterGuardAsync));

            if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
            {
                (stateInspection.Guard as ActionInspection).Active = false;
            }
        }

        public async Task BeforeEffectAsync<TEvent>(TransitionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
            {
                (stateInspection.Effect as ActionInspection).Active = true;
            }

            await Plugins.RunSafe(o => o.BeforeTransitionEffectAsync(context), nameof(BeforeEffectAsync));
            await Inspectors.RunSafe(i => i.BeforeTransitionEffectAsync(context), nameof(BeforeEffectAsync));
            await Observers.RunSafe(o => o.BeforeTransitionEffectAsync(context), nameof(BeforeEffectAsync));
        }

        public async Task AfterEffectAsync<TEvent>(TransitionContext<TEvent> context)
            where TEvent : Event, new()
        {
            await Observers.RunSafe(o => o.AfterTransitionEffectAsync(context), nameof(AfterEffectAsync));
            await Inspectors.RunSafe(i => i.AfterTransitionEffectAsync(context), nameof(AfterEffectAsync));
            await Plugins.RunSafe(o => o.AfterTransitionEffectAsync(context), nameof(AfterEffectAsync));

            if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
            {
                (stateInspection.Effect as ActionInspection).Active = false;
            }
        }

        public async Task AfterHydrateAsync(StateMachineActionContext context)
        {
            await Plugins.RunSafe(i => i.AfterHydrateAsync(context), nameof(AfterHydrateAsync));
            await GlobalInterceptor.AfterHydrateAsync(new BehaviorActionContext(context.Context.Context, Executor.ServiceProvider));
            await Interceptors.RunSafe(i => i.AfterHydrateAsync(context), nameof(AfterHydrateAsync));
        }

        public async Task BeforeDehydrateAsync(StateMachineActionContext context)
        {
            await Interceptors.RunSafe(i => i.BeforeDehydrateAsync(context), nameof(BeforeDehydrateAsync));
            await GlobalInterceptor.BeforeDehydrateAsync(new BehaviorActionContext(context.Context.Context, Executor.ServiceProvider));
            await Plugins.RunSafe(i => i.BeforeDehydrateAsync(context), nameof(BeforeDehydrateAsync));
        }

        public async Task<bool> BeforeProcessEventAsync<TEvent>(Context.Classes.EventContext<TEvent> context)
            where TEvent : Event, new()
        {
            var plugin = await Plugins.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEventAsync));
            var global = await GlobalInterceptor.BeforeProcessEventAsync(
                new Common.Context.Classes.EventContext<TEvent>(context.Context.Context, Executor.ServiceProvider, context.Event)
            );
            var local = await Interceptors.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEventAsync));

            return global && local && plugin;
        }

        public async Task AfterProcessEventAsync<TEvent>(Context.Classes.EventContext<TEvent> context)
            where TEvent : Event, new()
        {
            await Interceptors.RunSafe(i => i.AfterProcessEventAsync(context), nameof(AfterProcessEventAsync));
            await GlobalInterceptor.AfterProcessEventAsync(new Common.Context.Classes.EventContext<TEvent>(context.Context.Context, Executor.ServiceProvider, context.Event));
            await Plugins.RunSafe(i => i.AfterProcessEventAsync(context), nameof(AfterProcessEventAsync));
        }

        public async Task OnStateMachineInitializeExceptionAsync<TInitializationRequest>(StateMachineInitializationContext<TInitializationRequest> context, Exception exception)
            where TInitializationRequest : InitializationRequest, new()
        {
            var exceptionContext = new StateMachineInitializationContext(context.InitializationRequest, context.Context);
            await ExceptionHandlers.RunSafe(h => h.OnStateMachineInitializeExceptionAsync(exceptionContext, exception), nameof(OnStateMachineInitializeExceptionAsync));
            await Inspectors.RunSafe(i => i.OnStateMachineInitializeExceptionAsync(exceptionContext, exception), nameof(OnStateMachineInitializeExceptionAsync));
        }

        public async Task OnStateMachineFinalizeExceptionAsync(StateMachineActionContext context, Exception exception)
        {
            await ExceptionHandlers.RunSafe(h => h.OnStateMachineFinalizeExceptionAsync(context, exception), nameof(OnStateMachineFinalizeExceptionAsync));
            await Inspectors.RunSafe(i => i.OnStateMachineFinalizeExceptionAsync(context, exception), nameof(OnStateMachineFinalizeExceptionAsync));
        }

        public async Task OnTransitionGuardExceptionAsync<TEvent>(GuardContext<TEvent> context, Exception exception)
            where TEvent : Event, new()
        {
            await ExceptionHandlers.RunSafe(h => h.OnTransitionGuardExceptionAsync(context, exception), nameof(OnTransitionGuardExceptionAsync));
            await Inspectors.RunSafe(i => i.OnTransitionGuardExceptionAsync(context, exception), nameof(OnTransitionGuardExceptionAsync));
        }

        public async Task OnTransitionEffectExceptionAsync<TEvent>(TransitionContext<TEvent> context, Exception exception)
            where TEvent : Event, new()
        {
            await ExceptionHandlers.RunSafe(h => h.OnTransitionEffectExceptionAsync(context, exception), nameof(OnTransitionEffectExceptionAsync));
            await Inspectors.RunSafe(h => h.OnTransitionEffectExceptionAsync(context, exception), nameof(OnTransitionEffectExceptionAsync));
        }

        public async Task OnStateInitializeExceptionAsync(StateActionContext context, Exception exception)
        {
            await ExceptionHandlers.RunSafe(h => h.OnStateInitializeExceptionAsync(context, exception), nameof(OnStateInitializeExceptionAsync));
            await Inspectors.RunSafe(i => i.OnStateInitializeExceptionAsync(context, exception), nameof(OnStateInitializeExceptionAsync));
        }

        public async Task OnStateFinalizeExceptionAsync(StateActionContext context, Exception exception)
        {
            await ExceptionHandlers.RunSafe(h => h.OnStateFinalizeExceptionAsync(context, exception), nameof(OnStateFinalizeExceptionAsync));
            await Inspectors.RunSafe(i => i.OnStateFinalizeExceptionAsync(context, exception), nameof(OnStateFinalizeExceptionAsync));
        }

        public async Task OnStateEntryExceptionAsync(StateActionContext context, Exception exception)
        {
            await ExceptionHandlers.RunSafe(h => h.OnStateEntryExceptionAsync(context, exception), nameof(OnStateEntryExceptionAsync));
            await Inspectors.RunSafe(i => i.OnStateEntryExceptionAsync(context, exception), nameof(OnStateEntryExceptionAsync));
        }

        public async Task OnStateExitExceptionAsync(StateActionContext context, Exception exception)
        {
            await ExceptionHandlers.RunSafe(h => h.OnStateExitExceptionAsync(context, exception), nameof(OnStateExitExceptionAsync));
            await Inspectors.RunSafe(i => i.OnStateExitExceptionAsync(context, exception), nameof(OnStateExitExceptionAsync));
        }
    }
}
