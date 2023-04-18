using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Inspection.Classes;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Inspector
    {
        private Executor Executor { get; }

        public Inspector(Executor executor)
        {
            Executor = executor;

            ExceptionHandlerFactories.AddRange(Executor.Graph.ExceptionHandlerFactories);
            ExceptionHandlerFactories.AddRange(Executor.Register.GlobalExceptionHandlerFactories);

            InterceptorFactories.AddRange(Executor.Graph.InterceptorFactories);
            InterceptorFactories.AddRange(Executor.Register.GlobalInterceptorFactories);

            ObserverFactories.AddRange(Executor.Graph.ObserverFactories);
            ObserverFactories.AddRange(Executor.Register.GlobalObserverFactories);
        }

        public ActionInspection InitializeInspection;

        public IDictionary<Vertex, StateInspection> InspectionStates { get; } = new Dictionary<Vertex, StateInspection>();

        public IDictionary<Edge, TransitionInspection> InspectionTransitions { get; } = new Dictionary<Edge, TransitionInspection>();


        public IStateMachineInspection inspection;

        public IStateMachineInspection Inspection => inspection ?? (inspection = new StateMachineInspection(Executor));

        private readonly List<ExceptionHandlerFactory> ExceptionHandlerFactories = new List<ExceptionHandlerFactory>();

        private readonly List<InterceptorFactory> InterceptorFactories = new List<InterceptorFactory>();

        private readonly List<ObserverFactory> ObserverFactories = new List<ObserverFactory>();

        private List<IStateMachineObserver> observers;
        private List<IStateMachineObserver> Observers
            => observers ?? (observers = ObserverFactories.Select(t => t(Executor.ServiceProvider)).ToList());

        private List<IStateMachineInterceptor> interceptors;
        private List<IStateMachineInterceptor> Interceptors
            => interceptors ?? (interceptors = InterceptorFactories.Select(t => t(Executor.ServiceProvider)).ToList());

        private List<IStateMachineExceptionHandler> exceptionHandlers;
        private List<IStateMachineExceptionHandler> ExceptionHandlers
            => exceptionHandlers ?? (exceptionHandlers = ExceptionHandlerFactories.Select(t => t(Executor.ServiceProvider)).ToList());

        public IEnumerable<IStateMachineInspector> inspectors;
        public IEnumerable<IStateMachineInspector> Inspectors
            => inspectors ?? (inspectors = Executor.ServiceProvider.GetService<IEnumerable<IStateMachineInspector>>());

        public async Task BeforeStateMachineInitializeAsync(StateMachineActionContext context)
        {
            if (InitializeInspection != null)
            {
                InitializeInspection.Active = true;
            }

            await Inspectors.RunSafe(i => i.BeforeStateMachineInitializeAsync(context), nameof(BeforeStateMachineInitializeAsync));

            await Observers.RunSafe(o => o.BeforeStateMachineInitializeAsync(context), nameof(BeforeStateMachineInitializeAsync));
        }

        public async Task AfterStateMachineInitializeAsync(StateMachineActionContext context)
        {
            await Observers.RunSafe(o => o.AfterStateMachineInitializeAsync(context), nameof(AfterStateMachineInitializeAsync));
            await Inspectors.RunSafe(i => i.AfterStateMachineInitializeAsync(context), nameof(AfterStateMachineInitializeAsync));

            if (InitializeInspection != null)
            {
                InitializeInspection.Active = false;
            }
        }
        public async Task BeforeStateInitializeAsync(StateActionContext context)
        {
            if (InspectionStates.TryGetValue(context.Vertex, out var stateInspection))
            {
                stateInspection.BeginAction(Constants.Initialize);
            }

            await Inspectors.RunSafe(i => i.BeforeStateInitializeAsync(context), nameof(BeforeStateInitializeAsync));
            await Observers.RunSafe(o => o.BeforeStateInitializeAsync(context), nameof(BeforeStateInitializeAsync));
        }

        public async Task AfterStateInitializeAsync(StateActionContext context)
        {
            await Observers.RunSafe(o => o.AfterStateInitializeAsync(context), nameof(AfterStateInitializeAsync));
            await Inspectors.RunSafe(i => i.AfterStateInitializeAsync(context), nameof(AfterStateInitializeAsync));

            if (InspectionStates.TryGetValue(context.Vertex, out var stateInspection))
            {
                stateInspection.EndAction(Constants.Initialize);
            }
        }

        public async Task BeforeStateEntryAsync(StateActionContext context)
        {
            if (InspectionStates.TryGetValue(context.Vertex, out var stateInspection))
            {
                stateInspection.BeginAction(Constants.Entry);
            }

            await Inspectors.RunSafe(i => i.BeforeStateEntryAsync(context), nameof(BeforeStateEntryAsync));
            await Observers.RunSafe(o => o.BeforeStateEntryAsync(context), nameof(BeforeStateEntryAsync));
        }

        public async Task AfterStateEntryAsync(StateActionContext context)
        {
            await Observers.RunSafe(o => o.AfterStateEntryAsync(context), nameof(AfterStateEntryAsync));
            await Inspectors.RunSafe(i => i.AfterStateEntryAsync(context), nameof(AfterStateEntryAsync));

            if (InspectionStates.TryGetValue(context.Vertex, out var stateInspection))
            {
                stateInspection.EndAction(Constants.Entry);
            }
        }

        public async Task BeforeStateExitAsync(StateActionContext context)
        {
            if (InspectionStates.TryGetValue(context.Vertex, out var stateInspection))
            {
                stateInspection.BeginAction(Constants.Exit);
            }

            await Inspectors.RunSafe(i => i.BeforeStateExitAsync(context), nameof(BeforeStateExitAsync));
            await Observers.RunSafe(o => o.BeforeStateExitAsync(context), nameof(BeforeStateExitAsync));
        }

        public async Task AfterStateExitAsync(StateActionContext context)
        {
            await Observers.RunSafe(o => o.AfterStateExitAsync(context), nameof(AfterStateExitAsync));
            await Inspectors.RunSafe(i => i.BeforeStateExitAsync(context), nameof(BeforeStateExitAsync));

            if (InspectionStates.TryGetValue(context.Vertex, out var stateInspection))
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

            await Inspectors.RunSafe(i => i.BeforeTransitionGuardAsync(context), nameof(BeforeTransitionGuardAsync));
            await Observers.RunSafe(o => o.BeforeTransitionGuardAsync(context), nameof(BeforeTransitionGuardAsync));
        }

        public async Task AfterGuardAsync<TEvent>(GuardContext<TEvent> context, bool guardResult)
            where TEvent : Event, new()
        {
            await Observers.RunSafe(o => o.AfterTransitionGuardAsync(context, guardResult), nameof(AfterGuardAsync));
            await Inspectors.RunSafe(i => i.AfterTransitionGuardAsync(context, guardResult), nameof(AfterGuardAsync));

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

            await Inspectors.RunSafe(i => i.BeforeTransitionEffectAsync(context), nameof(BeforeEffectAsync));
            await Observers.RunSafe(o => o.BeforeTransitionEffectAsync(context), nameof(BeforeEffectAsync));
        }

        public async Task AfterEffectAsync<TEvent>(TransitionContext<TEvent> context)
            where TEvent : Event, new()
        {
            await Observers.RunSafe(o => o.AfterTransitionEffectAsync(context), nameof(AfterEffectAsync));
            await Inspectors.RunSafe(i => i.AfterTransitionEffectAsync(context), nameof(AfterEffectAsync));

            if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
            {
                (stateInspection.Effect as ActionInspection).Active = false;
            }
        }

        public Task AfterHydrateAsync(StateMachineActionContext context)
            => Interceptors.RunSafe(i => i.AfterHydrateAsync(context), nameof(AfterHydrateAsync));

        public Task BeforeDehydrateAsync(StateMachineActionContext context)
            => Interceptors.RunSafe(i => i.BeforeDehydrateAsync(context), nameof(BeforeDehydrateAsync));

        public Task AfterProcessEventAsync<TEvent>(EventContext<TEvent> context)
            where TEvent : Event, new()
            => Interceptors.RunSafe(i => i.AfterProcessEventAsync(context), nameof(AfterProcessEventAsync));

        public Task<bool> BeforeProcessEventAsync<TEvent>(EventContext<TEvent> context)
            where TEvent : Event, new()
            => Interceptors.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEventAsync));

        public Task OnStateMachineInitializeExceptionAsync(StateMachineActionContext context, Exception exception)
            => Task.WhenAll(
                ExceptionHandlers.RunSafe(h => h.OnStateMachineInitializeExceptionAsync(context, exception), nameof(OnStateMachineInitializeExceptionAsync)),
                Inspectors.RunSafe(i => i.OnStateMachineInitializeExceptionAsync(context, exception), nameof(OnStateMachineInitializeExceptionAsync))
            );

        public Task OnTransitionGuardExceptionAsync<TEvent>(GuardContext<TEvent> context, Exception exception)
            where TEvent : Event, new()
            => Task.WhenAll(
                ExceptionHandlers.RunSafe(h => h.OnTransitionGuardExceptionAsync(context, exception), nameof(OnTransitionGuardExceptionAsync)),
                Inspectors.RunSafe(i => i.OnTransitionGuardExceptionAsync(context, exception), nameof(OnTransitionGuardExceptionAsync))
            );

        public Task OnTransitionEffectExceptionAsync<TEvent>(TransitionContext<TEvent> context, Exception exception)
            where TEvent : Event, new()
            => Task.WhenAll(
                ExceptionHandlers.RunSafe(h => h.OnTransitionEffectExceptionAsync(context, exception), nameof(OnTransitionEffectExceptionAsync)),
                Inspectors.RunSafe(h => h.OnTransitionEffectExceptionAsync(context, exception), nameof(OnTransitionEffectExceptionAsync))
            );

        public Task OnStateInitializeExceptionAsync(StateActionContext context, Exception exception)
            => Task.WhenAll(
                ExceptionHandlers.RunSafe(h => h.OnStateInitializeExceptionAsync(context, exception), nameof(OnStateInitializeExceptionAsync)),
                Inspectors.RunSafe(i => i.OnStateInitializeExceptionAsync(context, exception), nameof(OnStateInitializeExceptionAsync))
            );

        public Task OnStateEntryExceptionAsync(StateActionContext context, Exception exception)
            => Task.WhenAll(
                ExceptionHandlers.RunSafe(h => h.OnStateEntryExceptionAsync(context, exception), nameof(OnStateEntryExceptionAsync)),
                Inspectors.RunSafe(i => i.OnStateEntryExceptionAsync(context, exception), nameof(OnStateEntryExceptionAsync))
            );

        public Task OnStateExitExceptionAsync(StateActionContext context, Exception exception)
            => Task.WhenAll(
                ExceptionHandlers.RunSafe(h => h.OnStateExitExceptionAsync(context, exception), nameof(OnStateExitExceptionAsync)),
                Inspectors.RunSafe(i => i.OnStateExitExceptionAsync(context, exception), nameof(OnStateExitExceptionAsync))
            );
    }
}
