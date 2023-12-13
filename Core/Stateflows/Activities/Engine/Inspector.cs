using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Engine;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Models;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Inspection.Classes;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Activities.Registration.Interfaces;
using Microsoft.Extensions.Logging;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.Activities.Engine
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

            GlobalInterceptor = Executor.NodeScope.ServiceProvider.GetRequiredService<CommonInterceptor>();
            //GlobalInterceptor = new Interceptor(Executor.NodeScope.ServiceProvider);


            //ExceptionHandlerFactories.AddRange(Executor.Graph.ExceptionHandlerFactories);
            //ExceptionHandlerFactories.AddRange(Executor.Register.GlobalExceptionHandlerFactories);

            //InterceptorFactories.AddRange(Executor.Graph.InterceptorFactories);
            //InterceptorFactories.AddRange(Executor.Register.GlobalInterceptorFactories);

            //ObserverFactories.AddRange(Executor.Graph.ObserverFactories);
            //ObserverFactories.AddRange(Executor.Register.GlobalObserverFactories);
        }

        public IDictionary<Node, NodeInspection> InspectionNodes { get; } = new Dictionary<Node, NodeInspection>();

        public IDictionary<Edge, FlowInspection> InspectionFlows { get; } = new Dictionary<Edge, FlowInspection>();


        public IActivityInspection inspection;

        public IActivityInspection Inspection => inspection ??= new ActivityInspection(Executor);

        private readonly List<ExceptionHandlerFactory> ExceptionHandlerFactories = new List<ExceptionHandlerFactory>();

        private readonly List<InterceptorFactory> InterceptorFactories = new List<InterceptorFactory>();

        private readonly List<ObserverFactory> ObserverFactories = new List<ObserverFactory>();

        private IEnumerable<IActivityObserver> observers;
        private IEnumerable<IActivityObserver> Observers
            => observers ??= ObserverFactories.Select(t => t(Executor.NodeScope.ServiceProvider));

        private IEnumerable<IActivityInterceptor> interceptors;
        private IEnumerable<IActivityInterceptor> Interceptors
            => interceptors ??= InterceptorFactories.Select(t => t(Executor.NodeScope.ServiceProvider));

        private IEnumerable<IActivityExceptionHandler> exceptionHandlers;
        private IEnumerable<IActivityExceptionHandler> ExceptionHandlers
            => exceptionHandlers ??= ExceptionHandlerFactories.Select(t => t(Executor.NodeScope.ServiceProvider));

        public IEnumerable<IActivityInspector> inspectors;
        public IEnumerable<IActivityInspector> Inspectors
            => inspectors ??= Executor.NodeScope.ServiceProvider.GetService<IEnumerable<IActivityInspector>>();

        public IEnumerable<IActivityPlugin> plugins;
        public IEnumerable<IActivityPlugin> Plugins
            => plugins ??= Executor.NodeScope.ServiceProvider.GetService<IEnumerable<IActivityPlugin>>();

        //public async Task BeforeStateMachineInitializeAsync(StateMachineActionContext context)
        //{
        //    if (InitializeInspection != null)
        //    {
        //        InitializeInspection.Active = true;
        //    }

        //    await Inspectors.RunSafe(i => i.BeforeStateMachineInitializeAsync(context), nameof(BeforeStateMachineInitializeAsync));

        //    await Observers.RunSafe(o => o.BeforeStateMachineInitializeAsync(context), nameof(BeforeStateMachineInitializeAsync));
        //}

        //public async Task AfterStateMachineInitializeAsync(StateMachineActionContext context)
        //{
        //    await Observers.RunSafe(o => o.AfterStateMachineInitializeAsync(context), nameof(AfterStateMachineInitializeAsync));
        //    await Inspectors.RunSafe(i => i.AfterStateMachineInitializeAsync(context), nameof(AfterStateMachineInitializeAsync));

        //    if (InitializeInspection != null)
        //    {
        //        InitializeInspection.Active = false;
        //    }
        //}
        //public async Task BeforeStateInitializeAsync(StateActionContext context)
        //{
        //    if (InspectionStates.TryGetValue(context.Vertex, out var stateInspection))
        //    {
        //        stateInspection.BeginAction(Constants.Initialize);
        //    }

        //    await Inspectors.RunSafe(i => i.BeforeStateInitializeAsync(context), nameof(BeforeStateInitializeAsync));
        //    await Observers.RunSafe(o => o.BeforeStateInitializeAsync(context), nameof(BeforeStateInitializeAsync));
        //}

        //public async Task AfterStateInitializeAsync(StateActionContext context)
        //{
        //    await Observers.RunSafe(o => o.AfterStateInitializeAsync(context), nameof(AfterStateInitializeAsync));
        //    await Inspectors.RunSafe(i => i.AfterStateInitializeAsync(context), nameof(AfterStateInitializeAsync));

        //    if (InspectionStates.TryGetValue(context.Vertex, out var stateInspection))
        //    {
        //        stateInspection.EndAction(Constants.Initialize);
        //    }
        //}

        //public async Task BeforeStateEntryAsync(StateActionContext context)
        //{
        //    if (InspectionStates.TryGetValue(context.Vertex, out var stateInspection))
        //    {
        //        stateInspection.BeginAction(Constants.Entry);
        //    }

        //    await Inspectors.RunSafe(i => i.BeforeStateEntryAsync(context), nameof(BeforeStateEntryAsync));
        //    await Observers.RunSafe(o => o.BeforeStateEntryAsync(context), nameof(BeforeStateEntryAsync));
        //}

        //public async Task AfterStateEntryAsync(StateActionContext context)
        //{
        //    await Observers.RunSafe(o => o.AfterStateEntryAsync(context), nameof(AfterStateEntryAsync));
        //    await Inspectors.RunSafe(i => i.AfterStateEntryAsync(context), nameof(AfterStateEntryAsync));

        //    if (InspectionStates.TryGetValue(context.Vertex, out var stateInspection))
        //    {
        //        stateInspection.EndAction(Constants.Entry);
        //    }
        //}

        //public async Task BeforeStateExitAsync(StateActionContext context)
        //{
        //    if (InspectionStates.TryGetValue(context.Vertex, out var stateInspection))
        //    {
        //        stateInspection.BeginAction(Constants.Exit);
        //    }

        //    await Inspectors.RunSafe(i => i.BeforeStateExitAsync(context), nameof(BeforeStateExitAsync));
        //    await Observers.RunSafe(o => o.BeforeStateExitAsync(context), nameof(BeforeStateExitAsync));
        //}

        //public async Task AfterStateExitAsync(StateActionContext context)
        //{
        //    await Observers.RunSafe(o => o.AfterStateExitAsync(context), nameof(AfterStateExitAsync));
        //    await Inspectors.RunSafe(i => i.BeforeStateExitAsync(context), nameof(BeforeStateExitAsync));

        //    if (InspectionStates.TryGetValue(context.Vertex, out var stateInspection))
        //    {
        //        stateInspection.EndAction(Constants.Exit);
        //    }
        //}

        //public async Task BeforeTransitionGuardAsync<TEvent>(GuardContext<TEvent> context)
        //    where TEvent : Event
        //{
        //    if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
        //    {
        //        (stateInspection.Guard as ActionInspection).Active = true;
        //    }

        //    await Inspectors.RunSafe(i => i.BeforeTransitionGuardAsync(context), nameof(BeforeTransitionGuardAsync));
        //    await Observers.RunSafe(o => o.BeforeTransitionGuardAsync(context), nameof(BeforeTransitionGuardAsync));
        //}

        //public async Task AfterGuardAsync<TEvent>(GuardContext<TEvent> context, bool guardResult)
        //    where TEvent : Event
        //{
        //    await Observers.RunSafe(o => o.AfterTransitionGuardAsync(context, guardResult), nameof(AfterGuardAsync));
        //    await Inspectors.RunSafe(i => i.AfterTransitionGuardAsync(context, guardResult), nameof(AfterGuardAsync));

        //    if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
        //    {
        //        (stateInspection.Guard as ActionInspection).Active = false;
        //    }
        //}

        //public async Task BeforeEffectAsync<TEvent>(TransitionContext<TEvent> context)
        //    where TEvent : Event
        //{
        //    if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
        //    {
        //        (stateInspection.Effect as ActionInspection).Active = true;
        //    }

        //    await Inspectors.RunSafe(i => i.BeforeTransitionEffectAsync(context), nameof(BeforeEffectAsync));
        //    await Observers.RunSafe(o => o.BeforeTransitionEffectAsync(context), nameof(BeforeEffectAsync));
        //}

        //public async Task AfterEffectAsync<TEvent>(TransitionContext<TEvent> context)
        //    where TEvent : Event
        //{
        //    await Observers.RunSafe(o => o.AfterTransitionEffectAsync(context), nameof(AfterEffectAsync));
        //    await Inspectors.RunSafe(i => i.AfterTransitionEffectAsync(context), nameof(AfterEffectAsync));

        //    if (InspectionTransitions.TryGetValue(context.Edge, out var stateInspection))
        //    {
        //        (stateInspection.Effect as ActionInspection).Active = false;
        //    }
        //}

        public async Task AfterHydrateAsync(ActivityActionContext context)
        {
            await Plugins.RunSafe(p => p.AfterHydrateAsync(context), nameof(AfterHydrateAsync), Logger);
            await Interceptors.RunSafe(i => i.AfterHydrateAsync(context), nameof(AfterHydrateAsync), Logger);
        }

        public async Task BeforeDehydrateAsync(ActivityActionContext context)
        {
            await Interceptors.RunSafe(i => i.BeforeDehydrateAsync(context), nameof(BeforeDehydrateAsync), Logger);
            await Plugins.RunSafe(p => p.BeforeDehydrateAsync(context), nameof(BeforeDehydrateAsync), Logger);
        }

        public async Task<bool> BeforeProcessEventAsync<TEvent>(Context.Classes.EventContext<TEvent> context)
            where TEvent : Event, new()
        {
            var plugin = await Plugins.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEventAsync), Logger);
            var global = await GlobalInterceptor.BeforeProcessEventAsync(
                new Common.Context.Classes.EventContext<TEvent>(context.Context.Context, Executor.NodeScope.ServiceProvider, context.Event)
            );
            var local = await Interceptors.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEventAsync), Logger);

            return global && local && plugin;
        }

        public async Task AfterProcessEventAsync<TEvent>(Context.Classes.EventContext<TEvent> context)
            where TEvent : Event, new()
        {
            await Interceptors.RunSafe(i => i.AfterProcessEventAsync(context), nameof(AfterProcessEventAsync), Logger);
            await GlobalInterceptor.AfterProcessEventAsync(new Common.Context.Classes.EventContext<TEvent>(context.Context.Context, Executor.NodeScope.ServiceProvider, context.Event));
            await Plugins.RunSafe(p => p.AfterProcessEventAsync(context), nameof(AfterProcessEventAsync), Logger);
        }

        public async Task BeforeActivityInitializeAsync(ActivityInitializationContext context)
        {
            await Plugins.RunSafe(p => p.BeforeActivityInitializeAsync(context), nameof(BeforeActivityInitializeAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeActivityInitializeAsync(context), nameof(BeforeActivityInitializeAsync), Logger);
            await Observers.RunSafe(o => o.BeforeActivityInitializeAsync(context), nameof(BeforeActivityInitializeAsync), Logger);
        }

        public async Task AfterActivityInitializationAsync(ActivityInitializationContext context)
        {
            await Observers.RunSafe(o => o.AfterActivityInitializationAsync(context), nameof(AfterActivityInitializationAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterActivityInitializationAsync(context), nameof(AfterActivityInitializationAsync), Logger);
            await Plugins.RunSafe(p => p.AfterActivityInitializationAsync(context), nameof(AfterActivityInitializationAsync), Logger);
        }

        public async Task BeforeNodeInitializationAsync(ActionContext context)
        {
            await Plugins.RunSafe(p => p.BeforeNodeInitializationAsync(context), nameof(BeforeNodeInitializationAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeNodeInitializationAsync(context), nameof(BeforeNodeInitializationAsync), Logger);
            await Observers.RunSafe(o => o.BeforeNodeInitializationAsync(context), nameof(BeforeNodeInitializationAsync), Logger);
        }

        public async Task AfterNodeInitializationAsync(ActionContext context)
        {
            await Observers.RunSafe(o => o.AfterNodeInitializationAsync(context), nameof(AfterNodeInitializationAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterNodeInitializationAsync(context), nameof(AfterNodeInitializationAsync), Logger);
            await Plugins.RunSafe(p => p.AfterNodeInitializationAsync(context), nameof(AfterNodeInitializationAsync), Logger);
        }

        public async Task BeforeNodeFinalizationAsync(ActionContext context)
        {
            await Plugins.RunSafe(p => p.BeforeNodeFinalizationAsync(context), nameof(BeforeNodeFinalizationAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeNodeFinalizationAsync(context), nameof(BeforeNodeFinalizationAsync), Logger);
            await Observers.RunSafe(o => o.BeforeNodeFinalizationAsync(context), nameof(BeforeNodeFinalizationAsync), Logger);
        }

        public async Task AfterNodeFinalizationAsync(ActionContext context)
        {
            await Observers.RunSafe(o => o.AfterNodeFinalizationAsync(context), nameof(AfterNodeFinalizationAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterNodeFinalizationAsync(context), nameof(AfterNodeFinalizationAsync), Logger);
            await Plugins.RunSafe(p => p.AfterNodeFinalizationAsync(context), nameof(AfterNodeFinalizationAsync), Logger);
        }

        public async Task BeforeNodeExecuteAsync(ActionContext context)
        {
            await Plugins.RunSafe(p => p.BeforeNodeExecuteAsync(context), nameof(BeforeNodeExecuteAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeNodeExecuteAsync(context), nameof(BeforeNodeExecuteAsync), Logger);
            await Observers.RunSafe(o => o.BeforeNodeExecuteAsync(context), nameof(BeforeNodeExecuteAsync), Logger);
        }

        public async Task AfterNodeExecuteAsync(ActionContext context)
        {
            await Observers.RunSafe(o => o.AfterNodeExecuteAsync(context), nameof(AfterNodeExecuteAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterNodeExecuteAsync(context), nameof(AfterNodeExecuteAsync), Logger);
            await Plugins.RunSafe(p => p.AfterNodeExecuteAsync(context), nameof(AfterNodeExecuteAsync), Logger);
        }


        //public Task AfterProcessEventAsync<TEvent>(EventContext<TEvent> context)
        //    where TEvent : Event
        //    => Interceptors.RunSafe(i => i.AfterProcessEventAsync(context), nameof(AfterProcessEventAsync));

        //public Task<bool> BeforeProcessEventAsync<TEvent>(EventContext<TEvent> context)
        //    where TEvent : Event
        //    => Interceptors.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEventAsync));





        public async Task OnActivityInitializationExceptionAsync<TInitializationRequest>(ActivityInitializationContext<TInitializationRequest> context, Exception exception)
            where TInitializationRequest : InitializationRequest, new()
        {
            var exceptionContext = new ActivityInitializationContext(context.Context, context.NodeScope, context.InitializationRequest);
            await ExceptionHandlers.RunSafe(h => h.OnActivityInitializationExceptionAsync(exceptionContext, exception), nameof(OnActivityInitializationExceptionAsync), Logger);
            await Inspectors.RunSafe(i => i.OnActivityInitializationExceptionAsync(exceptionContext, exception), nameof(OnActivityInitializationExceptionAsync), Logger);
        }

        public async Task OnActivityFinalizationExceptionAsync(IActivityFinalizationInspectionContext context, Exception exception)
        {
            await ExceptionHandlers.RunSafe(h => h.OnActivityFinalizationExceptionAsync(context, exception), nameof(OnActivityFinalizationExceptionAsync), Logger);
            await Inspectors.RunSafe(i => i.OnActivityFinalizationExceptionAsync(context, exception), nameof(OnActivityFinalizationExceptionAsync), Logger);
        }

        public async Task OnNodeInitializationExceptionAsync(IActivityNodeInspectionContext context, Exception exception)
        {
            await ExceptionHandlers.RunSafe(h => h.OnNodeInitializationExceptionAsync(context, exception), nameof(OnNodeInitializationExceptionAsync), Logger);
            await Inspectors.RunSafe(i => i.OnNodeInitializationExceptionAsync(context, exception), nameof(OnNodeInitializationExceptionAsync), Logger);
        }

        public async Task OnNodeFinalizationExceptionAsync(IActivityNodeInspectionContext context, Exception exception)
        {
            await ExceptionHandlers.RunSafe(h => h.OnNodeFinalizationExceptionAsync(context, exception), nameof(OnNodeFinalizationExceptionAsync), Logger);
            await Inspectors.RunSafe(i => i.OnNodeFinalizationExceptionAsync(context, exception), nameof(OnNodeFinalizationExceptionAsync), Logger);
        }

        public async Task OnNodeExecutionExceptionAsync(IActivityNodeInspectionContext context, Exception exception)
        {
            await ExceptionHandlers.RunSafe(h => h.OnNodeExecutionExceptionAsync(context, exception), nameof(OnNodeExecutionExceptionAsync), Logger);
            await Inspectors.RunSafe(i => i.OnNodeExecutionExceptionAsync(context, exception), nameof(OnNodeExecutionExceptionAsync), Logger);
        }

        public async Task OnFlowGuardExceptionAsync(IGuardInspectionContext context, Exception exception)
        {
            await ExceptionHandlers.RunSafe(h => h.OnFlowGuardExceptionAsync(context, exception), nameof(OnFlowGuardExceptionAsync), Logger);
            await Inspectors.RunSafe(i => i.OnFlowGuardExceptionAsync(context, exception), nameof(OnFlowGuardExceptionAsync), Logger);
        }

        public async Task OnFlowTransformationExceptionAsync(ITransformationInspectionContext context, Exception exception)
        {
            await ExceptionHandlers.RunSafe(h => h.OnFlowTransformationExceptionAsync(context, exception), nameof(OnFlowGuardExceptionAsync), Logger);
            await Inspectors.RunSafe(i => i.OnFlowTransformationExceptionAsync(context, exception), nameof(OnFlowGuardExceptionAsync), Logger);
        }




        //public Task AfterDispatchEventAsync<TEvent>(EventContext<TEvent> context)
        //    where TEvent : Event
        //    => Interceptors.RunSafe(i => i.AfterDispatchEventAsync(context), nameof(AfterDispatchEventAsync));

        //public Task<bool> BeforeDispatchEventAsync<TEvent>(EventContext<TEvent> context)
        //    where TEvent : Event
        //    => Interceptors.RunSafe(i => i.BeforeDispatchEventAsync(context), nameof(BeforeDispatchEventAsync));

        //public Task OnStateMachineInitializeExceptionAsync(StateMachineActionContext context, Exception exception)
        //    => Task.WhenAll(
        //        ExceptionHandlers.RunSafe(h => h.OnStateMachineInitializeExceptionAsync(context, exception), nameof(OnStateMachineInitializeExceptionAsync)),
        //        Inspectors.RunSafe(i => i.OnStateMachineInitializeExceptionAsync(context, exception), nameof(OnStateMachineInitializeExceptionAsync))
        //    );

        //public Task OnTransitionGuardExceptionAsync<TEvent>(GuardContext<TEvent> context, Exception exception)
        //    where TEvent : Event
        //    => Task.WhenAll(
        //        ExceptionHandlers.RunSafe(h => h.OnTransitionGuardExceptionAsync(context, exception), nameof(OnTransitionGuardExceptionAsync)),
        //        Inspectors.RunSafe(i => i.OnTransitionGuardExceptionAsync(context, exception), nameof(OnTransitionGuardExceptionAsync))
        //    );

        //public Task OnTransitionEffectExceptionAsync<TEvent>(TransitionContext<TEvent> context, Exception exception)
        //    where TEvent : Event
        //    => Task.WhenAll(
        //        ExceptionHandlers.RunSafe(h => h.OnTransitionEffectExceptionAsync(context, exception), nameof(OnTransitionEffectExceptionAsync)),
        //        Inspectors.RunSafe(h => h.OnTransitionEffectExceptionAsync(context, exception), nameof(OnTransitionEffectExceptionAsync))
        //    );

        //public Task OnStateInitializeExceptionAsync(StateActionContext context, Exception exception)
        //    => Task.WhenAll(
        //        ExceptionHandlers.RunSafe(h => h.OnStateInitializeExceptionAsync(context, exception), nameof(OnStateInitializeExceptionAsync)),
        //        Inspectors.RunSafe(i => i.OnStateInitializeExceptionAsync(context, exception), nameof(OnStateInitializeExceptionAsync))
        //    );

        //public Task OnStateEntryExceptionAsync(StateActionContext context, Exception exception)
        //    => Task.WhenAll(
        //        ExceptionHandlers.RunSafe(h => h.OnStateEntryExceptionAsync(context, exception), nameof(OnStateEntryExceptionAsync)),
        //        Inspectors.RunSafe(i => i.OnStateEntryExceptionAsync(context, exception), nameof(OnStateEntryExceptionAsync))
        //    );

        //public Task OnStateExitExceptionAsync(StateActionContext context, Exception exception)
        //    => Task.WhenAll(
        //        ExceptionHandlers.RunSafe(h => h.OnStateExitExceptionAsync(context, exception), nameof(OnStateExitExceptionAsync)),
        //        Inspectors.RunSafe(i => i.OnStateExitExceptionAsync(context, exception), nameof(OnStateExitExceptionAsync))
        //    );
    }
}
