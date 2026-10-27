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

            InterceptorFactories.AddRange(Executor.Register.GlobalInterceptorFactories);
            InterceptorFactories.AddRange(Executor.Graph.InterceptorFactories);

            ObserverFactories.AddRange(Executor.Register.GlobalObserverFactories);
            ObserverFactories.AddRange(Executor.Graph.ObserverFactories);
        }

        public async Task BuildAsync()
        {
            Observers = await Task.WhenAll(ObserverFactories.Select(t => t(Executor.NodeScope.ServiceProvider)));
            
            Interceptors = await Task.WhenAll(InterceptorFactories.Select(t => t(Executor.NodeScope.ServiceProvider)));
            
            ExceptionHandlers = await Task.WhenAll(ExceptionHandlerFactories.Select(t => t(Executor.NodeScope.ServiceProvider)));
        }

        public IDictionary<Node, NodeInspection> InspectionNodes { get; } = new Dictionary<Node, NodeInspection>();
        public IDictionary<Edge, FlowInspection> InspectionFlows { get; } = new Dictionary<Edge, FlowInspection>();

        private readonly List<ActivityExceptionHandlerFactoryAsync> ExceptionHandlerFactories = [];

        private readonly List<ActivityInterceptorFactoryAsync> InterceptorFactories = [];

        private readonly List<ActivityObserverFactoryAsync> ObserverFactories = [];

        private IActivityObserver[] Observers;

        private IActivityInterceptor[] Interceptors;

        private IActivityExceptionHandler[] ExceptionHandlers;

        private IEnumerable<IActivityPlugin>? plugins;
        public IEnumerable<IActivityPlugin> Plugins
            => plugins ??= Executor.NodeScope.ServiceProvider.GetRequiredService<IEnumerable<IActivityPlugin>>();

        private AcceptEvents? acceptEventsPlugin;
        public AcceptEvents AcceptEventsPlugin
            => acceptEventsPlugin ??= Executor.NodeScope.ServiceProvider.GetRequiredService<AcceptEvents>();

        public async Task<EventStatus> ProcessEventAsync<TEvent>(EventContext<TEvent> context, Func<Task<EventStatus>> next)
        {
            try
            {
                return await GlobalInterceptor.ProcessEventAsync(context, () =>
                    InterceptorPipeline.RunAsync(Plugins.ToArray(),
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

        private Task ObserveAsync(Func<Task> next, Func<IActivityObserver, Func<Task>, Task> observe)
            => ObserverPipeline.RunAsync(Observers,
                () => ObserverPipeline.RunAsync<IActivityObserver>(Plugins.ToArray(), next, observe), observe);

        private Task<TResult> ObserveAsync<TResult>(Func<Task<TResult>> next, Func<IActivityObserver, Func<Task<TResult>>, Task> observe)
            => ObserverPipeline.RunAsync(Observers,
                () => ObserverPipeline.RunAsync<IActivityObserver, TResult>(Plugins.ToArray(), next, observe), observe);

        public Task<bool> OnActivityInitializeAsync(IActivityInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
            => ObserveAsync(next, (observer, inner) => observer.OnActivityInitializeAsync(context, implicitInitialization, inner));

        public Task OnActivityFinalizeAsync(ActivityActionContext context, Func<Task> next)
            => ObserveAsync(next, (observer, inner) => observer.OnActivityFinalizeAsync(context, inner));

        public Task OnNodeInitializeAsync(ActionContext context, Func<Task> next)
            => ObserveAsync(next, (observer, inner) => observer.OnNodeInitializeAsync(context, inner));

        public Task OnNodeFinalizeAsync(ActionContext context, Func<Task> next)
            => ObserveAsync(next, (observer, inner) => observer.OnNodeFinalizeAsync(context, inner));

        public Task OnNodeExecuteAsync(ActionContext context, Func<Task> next)
            => ObserveAsync(next, (observer, inner) => observer.OnNodeExecuteAsync(context, inner));

        public Task OnNodeActivateAsync(ActionContext context, bool activated, Func<Task> next)
            => ObserveAsync(next, (observer, inner) => observer.OnNodeActivateAsync(context, activated, inner));

        public Task<IActivityAfterFlowContext> OnFlowActivateAsync(FlowContext context, Func<Task<IActivityAfterFlowContext>> next)
            => ObserveAsync(next, (observer, inner) => observer.OnFlowActivateAsync(context, inner));

        public Task<bool> OnFlowGuardAsync<TToken>(TokenFlowContext<TToken> context, Func<Task<bool>> next)
            => ObserveAsync(next, (observer, inner) => observer.OnFlowGuardAsync<TToken>(context, inner));

        public Task<TTransformedToken> OnFlowTransformAsync<TToken, TTransformedToken>(TokenFlowContext<TToken> context, Func<Task<TTransformedToken>> next)
            => ObserveAsync(next, (observer, inner) => observer.OnFlowTransformAsync<TToken, TTransformedToken>(context, inner));

        private async Task<bool> HandleExceptionAsync(
            Exception exception, Func<IActivityExceptionHandler, Task<bool>> handle,
            Func<IActivityPlugin, Task<bool>> notify, string method)
        {
            var context = Executor.Context.Context;
            var acknowledged = await ExecutionExceptionHandling.HandleAsync(context, ExceptionHandlers, handle, method, Logger);
            await ExecutionExceptionHandling.HandleAsync(context, Plugins, notify, method, Logger);
            context.ExecutionExceptions.Enqueue(exception);
            return acknowledged;
        }

        public Task<bool> OnActivityInitializationExceptionAsync<TEvent>(BaseContext context, EventHolder<TEvent> initializationEventHolder, Exception exception)
        {
            var exceptionContext = new ActivityInitializationContext<TEvent>(context.Context, context.NodeScope, initializationEventHolder, null);
            return HandleExceptionAsync(exception,
                h => h.OnActivityInitializationExceptionAsync(exceptionContext, exception),
                p => p.OnActivityInitializationExceptionAsync(exceptionContext, exception), nameof(OnActivityInitializationExceptionAsync));
        }

        public Task<bool> OnActivityFinalizationExceptionAsync(ActivityActionContext context, Exception exception)
            => HandleExceptionAsync(exception,
                h => h.OnActivityFinalizationExceptionAsync(context, exception),
                p => p.OnActivityFinalizationExceptionAsync(context, exception), nameof(OnActivityFinalizationExceptionAsync));

        public Task<bool> OnNodeInitializationExceptionAsync(ActivityNodeContext context, Exception exception)
            => HandleExceptionAsync(exception,
                h => h.OnNodeInitializationExceptionAsync(context, exception),
                p => p.OnNodeInitializationExceptionAsync(context, exception), nameof(OnNodeInitializationExceptionAsync));

        public Task<bool> OnNodeFinalizationExceptionAsync(ActivityNodeContext context, Exception exception)
            => HandleExceptionAsync(exception,
                h => h.OnNodeFinalizationExceptionAsync(context, exception),
                p => p.OnNodeFinalizationExceptionAsync(context, exception), nameof(OnNodeFinalizationExceptionAsync));

        public Task<bool> OnNodeExecutionExceptionAsync(ActivityNodeContext context, Exception exception)
            => HandleExceptionAsync(exception,
                h => h.OnNodeExecutionExceptionAsync(context, exception),
                p => p.OnNodeExecutionExceptionAsync(context, exception), nameof(OnNodeExecutionExceptionAsync));

        public Task<bool> OnFlowGuardExceptionAsync<TToken>(IGuardContext<TToken> context, Exception exception)
            => HandleExceptionAsync(exception,
                h => h.OnFlowGuardExceptionAsync(context, exception),
                p => p.OnFlowGuardExceptionAsync(context, exception), nameof(OnFlowGuardExceptionAsync));

        public Task<bool> OnFlowTransformationExceptionAsync<TToken, TTransformedToken>(ITransformationContext<TToken> context, Exception exception)
            => HandleExceptionAsync(exception,
                h => h.OnFlowTransformationExceptionAsync<TToken, TTransformedToken>(context, exception),
                p => p.OnFlowTransformationExceptionAsync<TToken, TTransformedToken>(context, exception), nameof(OnFlowTransformationExceptionAsync));
    }
}
