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

            InterceptorFactories.AddRange(Executor.ActionModel.InterceptorFactories);
            InterceptorFactories.AddRange(Executor.Register.GlobalInterceptorFactories);

            ObserverFactories.AddRange(Executor.ActionModel.ObserverFactories);
            ObserverFactories.AddRange(Executor.Register.GlobalObserverFactories);
        }

        public async Task BuildAsync()
        {
            Observers = await Task.WhenAll(ObserverFactories.Select(t => t(Executor.ServiceProvider)));
            ReverseObservers = Observers.Reverse().ToArray();
            
            Interceptors = await Task.WhenAll(InterceptorFactories.Select(t => t(Executor.ServiceProvider)));
            ReverseInterceptors = Interceptors.Reverse().ToArray();

            ExceptionHandlers = await Task.WhenAll(ExceptionHandlerFactories.Select(t => t(Executor.ServiceProvider)));
        }

        private readonly List<ActionExceptionHandlerFactoryAsync> ExceptionHandlerFactories = [];

        private readonly List<ActionInterceptorFactoryAsync> InterceptorFactories = [];

        private readonly List<ActionObserverFactoryAsync> ObserverFactories = [];
        
        private IActionInterceptor[] Interceptors;
        private IActionInterceptor[] ReverseInterceptors;
        
        private IActionObserver[] Observers;
        private IActionObserver[] ReverseObservers;

        private IActionExceptionHandler[] ExceptionHandlers;

        public bool BeforeProcessEvent<TEvent>(EventContext<TEvent> context)
        {
            var global = GlobalInterceptor.BeforeProcessEvent(context);
            var local = Interceptors.RunSafe(i => i.BeforeProcessEvent(context), nameof(BeforeProcessEvent), Logger);

            return global && local;
        }

        public void AfterProcessEvent<TEvent>(EventContext<TEvent> context, EventStatus eventStatus)
        {
            ReverseInterceptors.RunSafe(i => i.AfterProcessEvent(context, eventStatus), nameof(AfterProcessEvent), Logger);
            GlobalInterceptor.AfterProcessEvent(context, eventStatus);
        }
        
        public bool OnActionException(ActionDelegateContext context, Exception exception)
            => ExceptionHandlers.RunSafe(h => h.OnActionException(context, exception), nameof(OnActionException), Logger, false);

        public void BeforeActionInitialize(IActionDelegateContext context)
            => Observers.RunSafe(i => i.BeforeActionInitialize(context), nameof(BeforeActionInitialize), Logger);

        public void AfterActionInitialize(IActionDelegateContext context)
            => ReverseObservers.RunSafe(i => i.AfterActionInitialize(context), nameof(AfterActionInitialize), Logger);

        public void BeforeActionFinalize(IActionDelegateContext context)
            => Observers.RunSafe(i => i.BeforeActionFinalize(context), nameof(BeforeActionFinalize), Logger);

        public void AfterActionFinalize(IActionDelegateContext context)
            => ReverseObservers.RunSafe(i => i.AfterActionFinalize(context), nameof(AfterActionFinalize), Logger);
    }
}
