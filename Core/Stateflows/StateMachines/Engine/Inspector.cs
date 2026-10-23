using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Engine;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Inspection.Classes;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Inspector
    {
        private readonly Executor Executor;

        public readonly CommonInterceptor GlobalInterceptor;

        private readonly ILogger Logger;

        public Inspector(Executor executor, ILogger logger)
        {
            Executor = executor;
            Logger = logger;

            GlobalInterceptor = Executor.ServiceProvider.GetRequiredService<CommonInterceptor>();

            ExceptionHandlerFactories.AddRange(Executor.Graph.ExceptionHandlerFactories);
            ExceptionHandlerFactories.AddRange(Executor.Register.GlobalExceptionHandlerFactories);

            InterceptorFactories.AddRange(Executor.Register.GlobalInterceptorFactories);
            InterceptorFactories.AddRange(Executor.Graph.InterceptorFactories);

            ObserverFactories.AddRange(Executor.Register.GlobalObserverFactories);
            ObserverFactories.AddRange(Executor.Graph.ObserverFactories);
        }

        public async Task BuildAsync(IStateMachineActionContext context)
        {
            Observers = await Task.WhenAll(ObserverFactories.Select(t => t(Executor.ServiceProvider, context)));
            
            Interceptors = await Task.WhenAll(InterceptorFactories.Select(t => t(Executor.ServiceProvider, context)));
            
            ExceptionHandlers = await Task.WhenAll(ExceptionHandlerFactories.Select(t => t(Executor.ServiceProvider, context)));

            Plugins = Executor.ServiceProvider.GetRequiredService<IEnumerable<IStateMachinePlugin>>().ToArray();
        }

        public ActionInspection InitializeInspection;

        public ActionInspection FinalizeInspection;

        public IDictionary<string, StateInspection> InspectionStates { get; } = new Dictionary<string, StateInspection>();

        public IDictionary<Edge, TransitionInspection> InspectionTransitions { get; } = new Dictionary<Edge, TransitionInspection>();

        // private IStateMachineInspection inspection;
        // public IStateMachineInspection Inspection => inspection ??= new StateMachineInspection(Executor, this);

        private readonly List<StateMachineExceptionHandlerFactoryAsync> ExceptionHandlerFactories = [];

        private readonly List<StateMachineInterceptorFactoryAsync> InterceptorFactories = [];

        private readonly List<StateMachineObserverFactoryAsync> ObserverFactories = [];

        private IStateMachineObserver[] Observers;
        private IStateMachineInterceptor[] Interceptors;
        private IStateMachineExceptionHandler[] ExceptionHandlers;
        private IStateMachinePlugin[] Plugins;

        private Task ObserveAsync(Func<Task> next, Func<IStateMachineObserver, Func<Task>, Task> observe)
            => ObserverPipeline.RunAsync<IStateMachineObserver>(Plugins,
                () => ObserverPipeline.RunAsync(Observers, next, observe), observe);

        private Task<TResult> ObserveAsync<TResult>(Func<Task<TResult>> next, Func<IStateMachineObserver, Func<Task<TResult>>, Task> observe)
            => ObserverPipeline.RunAsync<IStateMachineObserver, TResult>(Plugins,
                () => ObserverPipeline.RunAsync(Observers, next, observe), observe);

        public async Task<bool> OnStateMachineInitializeAsync(StateMachineInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
        {
            if (InitializeInspection != null) InitializeInspection.Active = true;
            try
            {
                return await ObserveAsync(next, (observer, inner) => observer.OnStateMachineInitializeAsync(context, implicitInitialization, inner));
            }
            finally
            {
                if (InitializeInspection != null) InitializeInspection.Active = false;
            }
        }

        public async Task OnStateMachineFinalizeAsync(StateMachineActionContext context, Func<Task> next)
        {
            if (FinalizeInspection != null) FinalizeInspection.Active = true;
            try
            {
                await ObserveAsync(next, (observer, inner) => observer.OnStateMachineFinalizeAsync(context, inner));
            }
            finally
            {
                if (FinalizeInspection != null) FinalizeInspection.Active = false;
            }
        }

        public async Task OnStateInitializeAsync(StateActionContext context, Func<Task> next)
        {
            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var inspection)) inspection.BeginAction(Constants.Initialize);
            try
            {
                await ObserveAsync(next, (observer, inner) => observer.OnStateInitializeAsync(context, inner));
            }
            finally
            {
                inspection?.EndAction(Constants.Initialize);
            }
        }

        public async Task OnStateFinalizeAsync(StateActionContext context, Func<Task> next)
        {
            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var inspection)) inspection.BeginAction(Constants.Finalize);
            try
            {
                await ObserveAsync(next, (observer, inner) => observer.OnStateFinalizeAsync(context, inner));
            }
            finally
            {
                inspection?.EndAction(Constants.Finalize);
            }
        }

        public async Task OnStateEntryAsync(StateActionContext context, Func<Task> next)
        {
            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var inspection)) inspection.BeginAction(Constants.Entry);
            try
            {
                await ObserveAsync(next, (observer, inner) => observer.OnStateEntryAsync(context, inner));
            }
            finally
            {
                inspection?.EndAction(Constants.Entry);
            }
        }

        public async Task OnStateExitAsync(StateActionContext context, Func<Task> next)
        {
            if (InspectionStates.TryGetValue(context.Vertex.Identifier, out var inspection)) inspection.BeginAction(Constants.Exit);
            try
            {
                await ObserveAsync(next, (observer, inner) => observer.OnStateExitAsync(context, inner));
            }
            finally
            {
                inspection?.EndAction(Constants.Exit);
            }
        }

        public async Task<bool> OnTransitionGuardAsync<TEvent>(GuardContext<TEvent> context, Func<Task<bool>> next)
        {
            if (InspectionTransitions.TryGetValue(context.Edge, out var inspection)) ((ActionInspection)inspection.Guard).Active = true;
            try
            {
                return await ObserveAsync(next, (observer, inner) => observer.OnTransitionGuardAsync<TEvent>(context, inner));
            }
            finally
            {
                if (inspection != null) ((ActionInspection)inspection.Guard).Active = false;
            }
        }

        public async Task OnTransitionEffectAsync<TEvent>(TransitionContext<TEvent> context, Func<Task> next)
        {
            if (InspectionTransitions.TryGetValue(context.Edge, out var inspection)) ((ActionInspection)inspection.Effect).Active = true;
            try
            {
                await ObserveAsync(next, (observer, inner) => observer.OnTransitionEffectAsync<TEvent>(context, inner));
            }
            finally
            {
                if (inspection != null) ((ActionInspection)inspection.Effect).Active = false;
            }
        }

        public async Task<EventStatus> ProcessEventAsync<TEvent>(Context.Classes.EventContext<TEvent> context, Common.Context.Classes.EventContext<TEvent> commonContext, Func<Task<EventStatus>> next)
        {
            try
            {
                return await GlobalInterceptor.ProcessEventAsync(commonContext, () =>
                    InterceptorPipeline.RunAsync(Plugins,
                        () => InterceptorPipeline.RunAsync(Interceptors, next, (interceptor, inner) => interceptor.ProcessEventAsync(context, inner)), (interceptor, inner) => interceptor.ProcessEventAsync(context, inner)));
            }
            finally
            {
                if (context.Context.EventHolder.IsRespondedTo())
                {
                    await GlobalInterceptor.DoRequestRespondedAsync(
                        context,
                        context.Context.EventHolder,
                        context.Context.EventHolder.GetResponseHolder());
                }
            }
        }

        private async Task<bool> HandleExceptionAsync(
            BaseContext context, Exception exception, Func<IStateMachineExceptionHandler, Task<bool>> handle,
            Func<IStateMachinePlugin, Task<bool>> notify, string method)
        {
            var stateflowsContext = context.Context.Context;
            var acknowledged = await ExecutionExceptionHandling.HandleAsync(
                stateflowsContext, ExceptionHandlers, handle, method, Logger);
            if (!acknowledged)
            {
                await ExecutionExceptionHandling.HandleAsync(stateflowsContext, Plugins, notify, method, Logger);
            }
            stateflowsContext.ExecutionExceptions.Enqueue(exception);
            return acknowledged;
        }

        public Task<bool> OnStateMachineInitializationExceptionAsync(StateMachineInitializationContext context, Exception exception)
            => HandleExceptionAsync(context, exception,
                h => h.OnStateMachineInitializationExceptionAsync(context, exception),
                p => p.OnStateMachineInitializationExceptionAsync(context, exception), nameof(OnStateMachineInitializationExceptionAsync));

        public Task<bool> OnStateMachineFinalizationExceptionAsync(StateMachineActionContext context, Exception exception)
            => HandleExceptionAsync(context, exception,
                h => h.OnStateMachineFinalizationExceptionAsync(context, exception),
                p => p.OnStateMachineFinalizationExceptionAsync(context, exception), nameof(OnStateMachineFinalizationExceptionAsync));

        public Task<bool> OnTransitionGuardExceptionAsync<TEvent>(GuardContext<TEvent> context, Exception exception)
            => HandleExceptionAsync(context, exception,
                h => h.OnTransitionGuardExceptionAsync(context, exception),
                p => p.OnTransitionGuardExceptionAsync(context, exception), nameof(OnTransitionGuardExceptionAsync));

        public Task<bool> OnTransitionEffectExceptionAsync<TEvent>(TransitionContext<TEvent> context, Exception exception)
            => HandleExceptionAsync(context, exception,
                h => h.OnTransitionEffectExceptionAsync(context, exception),
                p => p.OnTransitionEffectExceptionAsync(context, exception), nameof(OnTransitionEffectExceptionAsync));

        public Task<bool> OnStateInitializeExceptionAsync(StateActionContext context, Exception exception)
            => HandleExceptionAsync(context, exception,
                h => h.OnStateInitializationExceptionAsync(context, exception),
                p => p.OnStateInitializationExceptionAsync(context, exception), nameof(OnStateInitializeExceptionAsync));

        public Task<bool> OnStateFinalizeExceptionAsync(StateActionContext context, Exception exception)
            => HandleExceptionAsync(context, exception,
                h => h.OnStateFinalizationExceptionAsync(context, exception),
                p => p.OnStateFinalizationExceptionAsync(context, exception), nameof(OnStateFinalizeExceptionAsync));

        public Task<bool> OnStateEntryExceptionAsync(StateActionContext context, Exception exception)
            => HandleExceptionAsync(context, exception,
                h => h.OnStateEntryExceptionAsync(context, exception),
                p => p.OnStateEntryExceptionAsync(context, exception), nameof(OnStateEntryExceptionAsync));

        public Task<bool> OnStateExitExceptionAsync(StateActionContext context, Exception exception)
            => HandleExceptionAsync(context, exception,
                h => h.OnStateExitExceptionAsync(context, exception),
                p => p.OnStateExitExceptionAsync(context, exception), nameof(OnStateExitExceptionAsync));
    }
}
