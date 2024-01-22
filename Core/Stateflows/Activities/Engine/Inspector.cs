using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Engine;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Inspection.Classes;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Activities.Registration.Interfaces;

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

            ExceptionHandlerFactories.AddRange(Executor.Graph.ExceptionHandlerFactories);
            ExceptionHandlerFactories.AddRange(Executor.Register.GlobalExceptionHandlerFactories);

            InterceptorFactories.AddRange(Executor.Graph.InterceptorFactories);
            InterceptorFactories.AddRange(Executor.Register.GlobalInterceptorFactories);

            ObserverFactories.AddRange(Executor.Graph.ObserverFactories);
            ObserverFactories.AddRange(Executor.Register.GlobalObserverFactories);
        }

        public IDictionary<Node, NodeInspection> InspectionNodes { get; } = new Dictionary<Node, NodeInspection>();

        public IDictionary<Edge, FlowInspection> InspectionFlows { get; } = new Dictionary<Edge, FlowInspection>();


        public IActivityInspection inspection;

        public IActivityInspection Inspection => inspection ??= new ActivityInspection(Executor);

        private readonly List<ActivityExceptionHandlerFactory> ExceptionHandlerFactories = new List<ActivityExceptionHandlerFactory>();

        private readonly List<ActivityInterceptorFactory> InterceptorFactories = new List<ActivityInterceptorFactory>();

        private readonly List<ActivityObserverFactory> ObserverFactories = new List<ActivityObserverFactory>();

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

        public AcceptEvents acceptEventsPlugin;
        public AcceptEvents AcceptEventsPlugin
            => acceptEventsPlugin ??= Executor.NodeScope.ServiceProvider.GetService<AcceptEvents>();

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

        public async Task BeforeActivityInitializationAsync(ActivityInitializationContext context)
        {
            await Plugins.RunSafe(p => p.BeforeActivityInitializationAsync(context), nameof(BeforeActivityInitializationAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeActivityInitializationAsync(context), nameof(BeforeActivityInitializationAsync), Logger);
            await Observers.RunSafe(o => o.BeforeActivityInitializationAsync(context), nameof(BeforeActivityInitializationAsync), Logger);
        }

        public async Task AfterActivityInitializationAsync(ActivityInitializationContext context)
        {
            await Observers.RunSafe(o => o.AfterActivityInitializationAsync(context), nameof(AfterActivityInitializationAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterActivityInitializationAsync(context), nameof(AfterActivityInitializationAsync), Logger);
            await Plugins.RunSafe(p => p.AfterActivityInitializationAsync(context), nameof(AfterActivityInitializationAsync), Logger);
        }

        public async Task BeforeActivityFinalizationAsync(ActivityActionContext context)
        {
            await Plugins.RunSafe(p => p.BeforeActivityFinalizationAsync(context), nameof(BeforeActivityFinalizationAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeActivityFinalizationAsync(context), nameof(BeforeActivityFinalizationAsync), Logger);
            await Observers.RunSafe(o => o.BeforeActivityFinalizationAsync(context), nameof(BeforeActivityFinalizationAsync), Logger);
        }

        public async Task AfterActivityFinalizationAsync(ActivityActionContext context)
        {
            await Observers.RunSafe(o => o.AfterActivityFinalizationAsync(context), nameof(AfterActivityFinalizationAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterActivityFinalizationAsync(context), nameof(AfterActivityFinalizationAsync), Logger);
            await Plugins.RunSafe(p => p.AfterActivityFinalizationAsync(context), nameof(AfterActivityFinalizationAsync), Logger);
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

        public async Task OnActivityInitializationExceptionAsync(BaseContext context, InitializationRequest initializationRequest, Exception exception)
        {
            var exceptionContext = new ActivityInitializationContext(context, initializationRequest);
            await ExceptionHandlers.RunSafe(h => h.OnActivityInitializationExceptionAsync(exceptionContext, exception), nameof(OnActivityInitializationExceptionAsync), Logger);
            await Inspectors.RunSafe(i => i.OnActivityInitializationExceptionAsync(exceptionContext, exception), nameof(OnActivityInitializationExceptionAsync), Logger);
        }

        public async Task OnActivityFinalizationExceptionAsync(ActivityActionContext context, Exception exception)
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
    }
}
