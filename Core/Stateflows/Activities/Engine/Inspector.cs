﻿using System;
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

        public async Task BuildAsync()
        {
            Observers = await Task.WhenAll(ObserverFactories.Select(t => t(Executor.NodeScope.ServiceProvider)));
            Interceptors = await Task.WhenAll(InterceptorFactories.Select(t => t(Executor.NodeScope.ServiceProvider)));
            ExceptionHandlers = await Task.WhenAll(ExceptionHandlerFactories.Select(t => t(Executor.NodeScope.ServiceProvider)));
        }

        public IDictionary<Node, NodeInspection> InspectionNodes { get; } = new Dictionary<Node, NodeInspection>();

        public IDictionary<Edge, FlowInspection> InspectionFlows { get; } = new Dictionary<Edge, FlowInspection>();


        private IActivityInspection inspection;

        public IActivityInspection Inspection => inspection ??= new ActivityInspection(Executor, this);

        private readonly List<ActivityExceptionHandlerFactoryAsync> ExceptionHandlerFactories = new List<ActivityExceptionHandlerFactoryAsync>();

        private readonly List<ActivityInterceptorFactoryAsync> InterceptorFactories = new List<ActivityInterceptorFactoryAsync>();

        private readonly List<ActivityObserverFactoryAsync> ObserverFactories = new List<ActivityObserverFactoryAsync>();

        private IEnumerable<IActivityObserver> Observers;

        private IEnumerable<IActivityInterceptor> Interceptors;

        private IEnumerable<IActivityExceptionHandler> ExceptionHandlers;

        private IEnumerable<IActivityInspector> inspectors;
        public IEnumerable<IActivityInspector> Inspectors
            => inspectors ??= Executor.NodeScope.ServiceProvider.GetService<IEnumerable<IActivityInspector>>();

        private IEnumerable<IActivityPlugin> plugins;
        public IEnumerable<IActivityPlugin> Plugins
            => plugins ??= Executor.NodeScope.ServiceProvider.GetService<IEnumerable<IActivityPlugin>>();

        private AcceptEvents acceptEventsPlugin;
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

        public async Task<bool> BeforeProcessEventAsync<TEvent>(EventContext<TEvent> context)
        {
            var plugin = await Plugins.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEventAsync), Logger);
            var global = await GlobalInterceptor.BeforeProcessEventAsync(
                new Common.Context.Classes.EventContext<TEvent>(context.Context.Context, Executor.NodeScope.ServiceProvider, context.Event)
            );
            var local = await Interceptors.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEventAsync), Logger);

            return global && local && plugin;
        }

        public async Task AfterProcessEventAsync<TEvent>(EventContext<TEvent> context)
        {
            await Interceptors.RunSafe(i => i.AfterProcessEventAsync(context), nameof(AfterProcessEventAsync), Logger);
            await GlobalInterceptor.AfterProcessEventAsync(new Common.Context.Classes.EventContext<TEvent>(context.Context.Context, Executor.NodeScope.ServiceProvider, context.Event));
            await Plugins.RunSafe(p => p.AfterProcessEventAsync(context), nameof(AfterProcessEventAsync), Logger);
        }

        public async Task BeforeActivityInitializationAsync(IActivityInitializationInspectionContext context)
        {
            await Plugins.RunSafe(p => p.BeforeActivityInitializeAsync(context), nameof(BeforeActivityInitializationAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeActivityInitializeAsync(context), nameof(BeforeActivityInitializationAsync), Logger);
            await Observers.RunSafe(o => o.BeforeActivityInitializeAsync(context), nameof(BeforeActivityInitializationAsync), Logger);
        }

        public async Task AfterActivityInitializationAsync(IActivityInitializationInspectionContext context, bool initialized)
        {
            await Observers.RunSafe(o => o.AfterActivityInitializeAsync(context, initialized), nameof(AfterActivityInitializationAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterActivityInitializeAsync(context, initialized), nameof(AfterActivityInitializationAsync), Logger);
            await Plugins.RunSafe(p => p.AfterActivityInitializeAsync(context, initialized), nameof(AfterActivityInitializationAsync), Logger);
        }

        public async Task BeforeActivityFinalizationAsync(ActivityActionContext context)
        {
            await Plugins.RunSafe(p => p.BeforeActivityFinalizeAsync(context), nameof(BeforeActivityFinalizationAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeActivityFinalizeAsync(context), nameof(BeforeActivityFinalizationAsync), Logger);
            await Observers.RunSafe(o => o.BeforeActivityFinalizeAsync(context), nameof(BeforeActivityFinalizationAsync), Logger);
        }

        public async Task AfterActivityFinalizationAsync(ActivityActionContext context)
        {
            await Observers.RunSafe(o => o.AfterActivityFinalizeAsync(context), nameof(AfterActivityFinalizationAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterActivityFinalizeAsync(context), nameof(AfterActivityFinalizationAsync), Logger);
            await Plugins.RunSafe(p => p.AfterActivityFinalizeAsync(context), nameof(AfterActivityFinalizationAsync), Logger);
        }

        public async Task BeforeNodeInitializationAsync(ActionContext context)
        {
            await Plugins.RunSafe(p => p.BeforeNodeInitializeAsync(context), nameof(BeforeNodeInitializationAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeNodeInitializeAsync(context), nameof(BeforeNodeInitializationAsync), Logger);
            await Observers.RunSafe(o => o.BeforeNodeInitializeAsync(context), nameof(BeforeNodeInitializationAsync), Logger);
        }

        public async Task AfterNodeInitializationAsync(ActionContext context)
        {
            await Observers.RunSafe(o => o.AfterNodeInitializeAsync(context), nameof(AfterNodeInitializationAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterNodeInitializeAsync(context), nameof(AfterNodeInitializationAsync), Logger);
            await Plugins.RunSafe(p => p.AfterNodeInitializeAsync(context), nameof(AfterNodeInitializationAsync), Logger);
        }

        public async Task BeforeNodeFinalizationAsync(ActionContext context)
        {
            await Plugins.RunSafe(p => p.BeforeNodeFinalizeAsync(context), nameof(BeforeNodeFinalizationAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeNodeFinalizeAsync(context), nameof(BeforeNodeFinalizationAsync), Logger);
            await Observers.RunSafe(o => o.BeforeNodeFinalizeAsync(context), nameof(BeforeNodeFinalizationAsync), Logger);
        }

        public async Task AfterNodeFinalizationAsync(ActionContext context)
        {
            await Observers.RunSafe(o => o.AfterNodeFinalizeAsync(context), nameof(AfterNodeFinalizationAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterNodeFinalizeAsync(context), nameof(AfterNodeFinalizationAsync), Logger);
            await Plugins.RunSafe(p => p.AfterNodeFinalizeAsync(context), nameof(AfterNodeFinalizationAsync), Logger);
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

        public async Task BeforeNodeActivateAsync(ActionContext context, bool activated)
        {
            await Plugins.RunSafe(p => p.BeforeNodeActivateAsync(context, activated), nameof(BeforeNodeActivateAsync), Logger);
            await Inspectors.RunSafe(i => i.BeforeNodeActivateAsync(context, activated), nameof(BeforeNodeActivateAsync), Logger);
            await Observers.RunSafe(o => o.BeforeNodeActivateAsync(context, activated), nameof(BeforeNodeActivateAsync), Logger);
        }

        public async Task AfterNodeActivateAsync(ActionContext context)
        {
            await Observers.RunSafe(o => o.AfterNodeActivateAsync(context), nameof(AfterNodeActivateAsync), Logger);
            await Inspectors.RunSafe(i => i.AfterNodeActivateAsync(context), nameof(AfterNodeActivateAsync), Logger);
            await Plugins.RunSafe(p => p.AfterNodeActivateAsync(context), nameof(AfterNodeActivateAsync), Logger);
        }

        private static bool ShouldPropagateException(Graph graph, bool handled)
            => !handled;

        public async Task<bool> OnActivityInitializationExceptionAsync<TEvent>(BaseContext context, EventHolder<TEvent> initializationEventHolder, Exception exception)
        {
            var exceptionContext = new ActivityInitializationContext<TEvent>(context.Context, context.NodeScope, initializationEventHolder, null);
            var handled = await ExceptionHandlers.RunSafe(h => h.OnActivityInitializationExceptionAsync(exceptionContext, exception), nameof(OnActivityInitializationExceptionAsync), Logger, false);
            await Inspectors.RunSafe(i => i.OnActivityInitializationExceptionAsync(exceptionContext, exception), nameof(OnActivityInitializationExceptionAsync), Logger);

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.Exceptions.Add(exception);
            }

            return handled;
        }

        public async Task<bool> OnActivityFinalizationExceptionAsync(ActivityActionContext context, Exception exception)
        {
            var handled = await ExceptionHandlers.RunSafe(h => h.OnActivityFinalizationExceptionAsync(context, exception), nameof(OnActivityFinalizationExceptionAsync), Logger, false);
            await Inspectors.RunSafe(i => i.OnActivityFinalizationExceptionAsync(context, exception), nameof(OnActivityFinalizationExceptionAsync), Logger);

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.Exceptions.Add(exception);
            }

            return handled;
        }

        public async Task<bool> OnNodeInitializationExceptionAsync(ActivityNodeContext context, Exception exception)
        {
            var handled = await ExceptionHandlers.RunSafe(h => h.OnNodeInitializationExceptionAsync(context, exception), nameof(OnNodeInitializationExceptionAsync), Logger, false);
            await Inspectors.RunSafe(i => i.OnNodeInitializationExceptionAsync(context, exception), nameof(OnNodeInitializationExceptionAsync), Logger);

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.Exceptions.Add(exception);
            }

            return handled;
        }

        public async Task<bool> OnNodeFinalizationExceptionAsync(ActivityNodeContext context, Exception exception)
        {
            var handled = await ExceptionHandlers.RunSafe(h => h.OnNodeFinalizationExceptionAsync(context, exception), nameof(OnNodeFinalizationExceptionAsync), Logger, false);
            await Inspectors.RunSafe(i => i.OnNodeFinalizationExceptionAsync(context, exception), nameof(OnNodeFinalizationExceptionAsync), Logger);

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.Exceptions.Add(exception);
            }

            return handled;
        }

        public async Task<bool> OnNodeExecutionExceptionAsync(ActivityNodeContext context, Exception exception)
        {
            var handled = await ExceptionHandlers.RunSafe(h => h.OnNodeExecutionExceptionAsync(context, exception), nameof(OnNodeExecutionExceptionAsync), Logger, false);
            await Inspectors.RunSafe(i => i.OnNodeExecutionExceptionAsync(context, exception), nameof(OnNodeExecutionExceptionAsync), Logger);

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.Exceptions.Add(exception);
            }

            return handled;
        }

        public async Task<bool> OnFlowGuardExceptionAsync(IGuardInspectionContext context, Exception exception)
        {
            var handled = await ExceptionHandlers.RunSafe(h => h.OnFlowGuardExceptionAsync(context, exception), nameof(OnFlowGuardExceptionAsync), Logger, false);
            await Inspectors.RunSafe(i => i.OnFlowGuardExceptionAsync(context, exception), nameof(OnFlowGuardExceptionAsync), Logger);

            //if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            //{
            //    context.Context.Exceptions.Add(exception);
            //}

            return handled;
        }

        public async Task<bool> OnFlowTransformationExceptionAsync(ITransformationInspectionContext context, Exception exception)
        {
            var handled = await ExceptionHandlers.RunSafe(h => h.OnFlowTransformationExceptionAsync(context, exception), nameof(OnFlowTransformationExceptionAsync), Logger, false);
            await Inspectors.RunSafe(i => i.OnFlowTransformationExceptionAsync(context, exception), nameof(OnFlowTransformationExceptionAsync), Logger);

            //if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            //{
            //    context.Context.Exceptions.Add(exception);
            //}

            return handled;
        }
    }
}
