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
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Classes;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Engine
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

        private IEnumerable<IActivityPlugin> plugins;
        public IEnumerable<IActivityPlugin> Plugins
            => plugins ??= Executor.NodeScope.ServiceProvider.GetService<IEnumerable<IActivityPlugin>>();

        private AcceptEvents acceptEventsPlugin;
        public AcceptEvents AcceptEventsPlugin
            => acceptEventsPlugin ??= Executor.NodeScope.ServiceProvider.GetService<AcceptEvents>();

        public void AfterHydrate(ActivityActionContext context)
        {
            Plugins.RunSafe(p => p.AfterHydrate(context), nameof(AfterHydrate), Logger);
            Interceptors.RunSafe(i => i.AfterHydrate(context), nameof(AfterHydrate), Logger);
        }

        public void BeforeDehydrate(ActivityActionContext context)
        {
            Interceptors.RunSafe(i => i.BeforeDehydrate(context), nameof(BeforeDehydrate), Logger);
            Plugins.RunSafe(p => p.BeforeDehydrate(context), nameof(BeforeDehydrate), Logger);
        }

        public bool BeforeProcessEvent<TEvent>(EventContext<TEvent> context)
        {
            var plugin = Plugins.RunSafe(i => i.BeforeProcessEvent(context), nameof(BeforeProcessEvent), Logger);
            var global = GlobalInterceptor.BeforeProcessEvent(
                new Common.Context.Classes.EventContext<TEvent>(
                    context.Context.Context,
                    Executor.NodeScope.ServiceProvider,
                    (EventHolder<TEvent>)context.Context.EventHolder
                )
            );
            var local = Interceptors.RunSafe(i => i.BeforeProcessEvent(context), nameof(BeforeProcessEvent), Logger);

            return global && local && plugin;
        }

        public void AfterProcessEvent<TEvent>(EventContext<TEvent> context, EventStatus eventStatus)
        {
            Interceptors.RunSafe(i => i.AfterProcessEvent(context, eventStatus), nameof(AfterProcessEvent), Logger);
            GlobalInterceptor.AfterProcessEvent(
                new Common.Context.Classes.EventContext<TEvent>(
                    context.Context.Context,
                    Executor.NodeScope.ServiceProvider,
                    (EventHolder<TEvent>)context.Context.EventHolder
                ),
                eventStatus
            );
            Plugins.RunSafe(p => p.AfterProcessEvent(context, eventStatus), nameof(AfterProcessEvent), Logger);
        }

        public void BeforeActivityInitialize(IActivityInitializationContext context, bool implicitInitialization)
        {
            Observers.RunSafe(p => p.BeforeActivityInitialize(context, implicitInitialization), nameof(BeforeActivityInitialize), Logger);
            Plugins.RunSafe(o => o.BeforeActivityInitialize(context, implicitInitialization), nameof(BeforeActivityInitialize), Logger);
        }

        public void AfterActivityInitialize(IActivityInitializationContext context, bool implicitInitialization, bool initialized)
        {
            Plugins.RunSafe(o => o.AfterActivityInitialize(context, implicitInitialization, initialized), nameof(AfterActivityInitialize), Logger);
            Observers.RunSafe(p => p.AfterActivityInitialize(context, implicitInitialization, initialized), nameof(AfterActivityInitialize), Logger);
        }

        public void BeforeActivityFinalize(ActivityActionContext context)
        {
            Observers.RunSafe(p => p.BeforeActivityFinalize(context), nameof(BeforeActivityFinalize), Logger);
            Plugins.RunSafe(o => o.BeforeActivityFinalize(context), nameof(BeforeActivityFinalize), Logger);
        }

        public void AfterActivityFinalize(ActivityActionContext context)
        {
            Plugins.RunSafe(o => o.AfterActivityFinalize(context), nameof(AfterActivityFinalize), Logger);
            Observers.RunSafe(p => p.AfterActivityFinalize(context), nameof(AfterActivityFinalize), Logger);
        }

        public void BeforeNodeInitialize(ActionContext context)
        {
            Observers.RunSafe(p => p.BeforeNodeInitialize(context), nameof(BeforeNodeInitialize), Logger);
            Plugins.RunSafe(o => o.BeforeNodeInitialize(context), nameof(BeforeNodeInitialize), Logger);
        }

        public void AfterNodeInitialize(ActionContext context)
        {
            Plugins.RunSafe(o => o.AfterNodeInitialize(context), nameof(AfterNodeInitialize), Logger);
            Observers.RunSafe(p => p.AfterNodeInitialize(context), nameof(AfterNodeInitialize), Logger);
        }

        public void BeforeNodeFinalize(ActionContext context)
        {
            Observers.RunSafe(p => p.BeforeNodeFinalize(context), nameof(BeforeNodeFinalize), Logger);
            Plugins.RunSafe(o => o.BeforeNodeFinalize(context), nameof(BeforeNodeFinalize), Logger);
        }

        public void AfterNodeFinalize(ActionContext context)
        {
            Plugins.RunSafe(o => o.AfterNodeFinalize(context), nameof(AfterNodeFinalize), Logger);
            Observers.RunSafe(p => p.AfterNodeFinalize(context), nameof(AfterNodeFinalize), Logger);
        }

        public void BeforeNodeExecute(ActionContext context)
        {
            Observers.RunSafe(p => p.BeforeNodeExecute(context), nameof(BeforeNodeExecute), Logger);
            Plugins.RunSafe(o => o.BeforeNodeExecute(context), nameof(BeforeNodeExecute), Logger);
        }

        public void AfterNodeExecute(ActionContext context)
        {
            Plugins.RunSafe(o => o.AfterNodeExecute(context), nameof(AfterNodeExecute), Logger);
            Observers.RunSafe(p => p.AfterNodeExecute(context), nameof(AfterNodeExecute), Logger);
        }

        public void BeforeNodeActivate(ActionContext context, bool activated)
        {
            Observers.RunSafe(p => p.BeforeNodeActivate(context, activated), nameof(BeforeNodeActivate), Logger);
            Plugins.RunSafe(o => o.BeforeNodeActivate(context, activated), nameof(BeforeNodeActivate), Logger);
        }

        public void AfterNodeActivate(ActionContext context)
        {
            Plugins.RunSafe(o => o.AfterNodeActivate(context), nameof(AfterNodeActivate), Logger);
            Observers.RunSafe(p => p.AfterNodeActivate(context), nameof(AfterNodeActivate), Logger);
        }

        public void BeforeFlowActivate(FlowContext context)
        {
            Observers.RunSafe(p => p.BeforeFlowActivate(context), nameof(BeforeFlowActivate), Logger);
            Plugins.RunSafe(o => o.BeforeFlowActivate(context), nameof(BeforeFlowActivate), Logger);
        }

        public void AfterFlowActivate(FlowContext context, bool activated)
        {
            Plugins.RunSafe(o => o.AfterFlowActivate(context, activated), nameof(AfterFlowActivate), Logger);
            Observers.RunSafe(p => p.AfterFlowActivate(context, activated), nameof(AfterFlowActivate), Logger);
        }

        public void BeforeFlowGuard<TToken>(TokenFlowContext<TToken> context)
        {
            Observers.RunSafe(p => p.BeforeFlowGuard(context), nameof(BeforeFlowGuard), Logger);
            Plugins.RunSafe(o => o.BeforeFlowGuard(context), nameof(BeforeFlowGuard), Logger);
        }

        public void AfterFlowGuard<TToken>(TokenFlowContext<TToken> context, bool guardResult)
        {
            Plugins.RunSafe(o => o.AfterFlowGuard(context, guardResult), nameof(AfterFlowGuard), Logger);
            Observers.RunSafe(p => p.AfterFlowGuard(context, guardResult), nameof(AfterFlowGuard), Logger);
        }

        public void BeforeFlowTransform<TToken, TTransformedToken>(TokenFlowContext<TToken> context)
        {
            Observers.RunSafe(p => p.BeforeFlowTransform<TToken, TTransformedToken>(context), nameof(BeforeFlowTransform), Logger);
            Plugins.RunSafe(o => o.BeforeFlowTransform<TToken, TTransformedToken>(context), nameof(BeforeFlowTransform), Logger);
        }

        public void AfterFlowTransform<TToken, TTransformedToken>(TokenFlowContext<TToken, TTransformedToken> context)
        {
            Plugins.RunSafe(o => o.AfterFlowTransform(context), nameof(AfterFlowTransform), Logger);
            Observers.RunSafe(p => p.AfterFlowTransform(context), nameof(AfterFlowTransform), Logger);
        }

        private static bool ShouldPropagateException(Graph graph, bool handled)
            => !handled;

        public bool OnActivityInitializationException<TEvent>(BaseContext context, EventHolder<TEvent> initializationEventHolder, Exception exception)
        {
            var exceptionContext = new ActivityInitializationContext<TEvent>(context.Context, context.NodeScope, initializationEventHolder, null);
            var handled = ExceptionHandlers.RunSafe(h => h.OnActivityInitializationException(exceptionContext, exception), nameof(OnActivityInitializationException), Logger, false);
            Plugins.RunSafe(h => h.OnActivityInitializationException(exceptionContext, exception), nameof(OnActivityInitializationException), Logger, false);

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.Exceptions.Add(exception);
            }

            return handled;
        }

        public bool OnActivityFinalizationException(ActivityActionContext context, Exception exception)
        {
            var handled = ExceptionHandlers.RunSafe(h => h.OnActivityFinalizationException(context, exception), nameof(OnActivityFinalizationException), Logger, false);
            Plugins.RunSafe(h => h.OnActivityFinalizationException(context, exception), nameof(OnActivityFinalizationException), Logger, false);

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.Exceptions.Add(exception);
            }

            return handled;
        }

        public bool OnNodeInitializationException(ActivityNodeContext context, Exception exception)
        {
            var handled = ExceptionHandlers.RunSafe(h => h.OnNodeInitializationException(context, exception), nameof(OnNodeInitializationException), Logger, false);
            Plugins.RunSafe(h => h.OnNodeInitializationException(context, exception), nameof(OnNodeInitializationException), Logger, false);

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.Exceptions.Add(exception);
            }

            return handled;
        }

        public bool OnNodeFinalizationException(ActivityNodeContext context, Exception exception)
        {
            var handled = ExceptionHandlers.RunSafe(h => h.OnNodeFinalizationException(context, exception), nameof(OnNodeFinalizationException), Logger, false);
            Plugins.RunSafe(h => h.OnNodeFinalizationException(context, exception), nameof(OnNodeFinalizationException), Logger, false);

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.Exceptions.Add(exception);
            }

            return handled;
        }

        public bool OnNodeExecutionException(ActivityNodeContext context, Exception exception)
        {
            var handled = ExceptionHandlers.RunSafe(h => h.OnNodeExecutionException(context, exception), nameof(OnNodeExecutionException), Logger, false);
            Plugins.RunSafe(h => h.OnNodeExecutionException(context, exception), nameof(OnNodeExecutionException), Logger, false);

            if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            {
                context.Context.Exceptions.Add(exception);
            }

            return handled;
        }

        public bool OnFlowGuardException<TToken>(IGuardContext<TToken> context, Exception exception)
        {
            var handled = ExceptionHandlers.RunSafe(h => h.OnFlowGuardException(context, exception), nameof(OnFlowGuardException), Logger, false);
            Plugins.RunSafe(h => h.OnFlowGuardException(context, exception), nameof(OnFlowGuardException), Logger, false);

            //if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            //{
            //    context.Context.Exceptions.Add(exception);
            //}

            return handled;
        }

        public bool OnFlowTransformationException<TToken, TTransformedToken>(ITransformationContext<TToken> context, Exception exception)
        {
            var handled = ExceptionHandlers.RunSafe(h => h.OnFlowTransformationException<TToken, TTransformedToken>(context, exception), nameof(OnFlowTransformationException), Logger, false);
            Plugins.RunSafe(h => h.OnFlowTransformationException<TToken, TTransformedToken>(context, exception), nameof(OnFlowTransformationException), Logger, false);

            //if (ShouldPropagateException(context.Context.Executor.Graph, handled))
            //{
            //    context.Context.Exceptions.Add(exception);
            //}

            return handled;
        }
    }
}
