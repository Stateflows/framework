using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Engine;
using Stateflows.Common.Extensions;
using Stateflows.Actions.Context.Classes;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Actions.Registration;

namespace Stateflows.Actions.Engine
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

            ExceptionHandlerFactories.AddRange(Executor.ActionModel.ExceptionHandlerFactories);
            ExceptionHandlerFactories.AddRange(Executor.Register.GlobalExceptionHandlerFactories);

            InterceptorFactories.AddRange(Executor.Register.GlobalInterceptorFactories);
            InterceptorFactories.AddRange(Executor.ActionModel.InterceptorFactories);

            ObserverFactories.AddRange(Executor.Register.GlobalObserverFactories);
            ObserverFactories.AddRange(Executor.ActionModel.ObserverFactories);
        }

        public async Task BuildAsync()
        {
            Observers = await Task.WhenAll(ObserverFactories.Select(t => t(Executor.ServiceProvider)));
            
            Interceptors = await Task.WhenAll(InterceptorFactories.Select(t => t(Executor.ServiceProvider)));

            ExceptionHandlers = await Task.WhenAll(ExceptionHandlerFactories.Select(t => t(Executor.ServiceProvider)));
        }

        private readonly List<ActionExceptionHandlerFactoryAsync> ExceptionHandlerFactories = [];

        private readonly List<ActionInterceptorFactoryAsync> InterceptorFactories = [];

        private readonly List<ActionObserverFactoryAsync> ObserverFactories = [];
        
        private IActionInterceptor[] Interceptors;
        
        private IActionObserver[] Observers;

        private IActionExceptionHandler[] ExceptionHandlers;

        public async Task<EventStatus> ProcessEventAsync<TEvent>(EventContext<TEvent> context, Func<Task<EventStatus>> next)
        {
            try
            {
                return await GlobalInterceptor.ProcessEventAsync(context, () =>
                    InterceptorPipeline.RunAsync(Interceptors, next, (interceptor, inner) => interceptor.ProcessEventAsync(context, inner)));
            }
            finally
            {
                if (context.RootContext.EventHolder.IsRespondedTo())
                {
                    await GlobalInterceptor.DoRequestRespondedAsync(
                        context,
                        context.RootContext.EventHolder,
                        context.RootContext.EventHolder.GetResponseHolder());
                }
            }
        }

        public Task<bool> OnActionExceptionAsync(ActionDelegateContext context, Exception exception)
            => ExecutionExceptionHandling.HandleAsync(
                context.Context, ExceptionHandlers,
                h => h.OnActionExceptionAsync(context, exception), nameof(OnActionExceptionAsync), Logger);

        public Task OnActionInitializeAsync(IActionDelegateContext context, Func<Task> next)
            => ObserverPipeline.RunAsync(Observers, next, (observer, inner) => observer.OnActionInitializeAsync(context, inner));

        public Task OnActionFinalizeAsync(IActionDelegateContext context, Func<Task> next)
            => ObserverPipeline.RunAsync(Observers, next, (observer, inner) => observer.OnActionFinalizeAsync(context, inner));
    }
}
