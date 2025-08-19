using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Common.Engine;
using Stateflows.Common.Extensions;
using Stateflows.Actions.Context.Classes;
using Stateflows.Actions.Registration;
using Stateflows.Common;

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
        }

        public async Task BuildAsync()
        {
            Interceptors = await Task.WhenAll(InterceptorFactories.Select(t => t(Executor.ServiceProvider)));
            ExceptionHandlers = await Task.WhenAll(ExceptionHandlerFactories.Select(t => t(Executor.ServiceProvider)));
        }

        private readonly List<ActionExceptionHandlerFactoryAsync> ExceptionHandlerFactories = new List<ActionExceptionHandlerFactoryAsync>();

        private readonly List<ActionInterceptorFactoryAsync> InterceptorFactories = new List<ActionInterceptorFactoryAsync>();
        
        private IEnumerable<IActionInterceptor> Interceptors;

        private IEnumerable<IActionExceptionHandler> ExceptionHandlers;

        // private IEnumerable<IActionPlugin> plugins;
        // public IEnumerable<IActionPlugin> Plugins
        //     => plugins ??= Executor.ServiceProvider.GetService<IEnumerable<IActionPlugin>>();

        public void AfterHydrate(ActionDelegateContext context)
        {
            // await Plugins.RunSafe(p => p.AfterHydrateAsync(context), nameof(AfterHydrate), Logger);
            Interceptors.RunSafe(i => i.AfterHydrate(context), nameof(AfterHydrate), Logger);
        }

        public void BeforeDehydrate(ActionDelegateContext context)
        {
            Interceptors.RunSafe(i => i.BeforeDehydrate(context), nameof(BeforeDehydrate), Logger);
            // await Plugins.RunSafe(p => p.BeforeDehydrateAsync(context), nameof(BeforeDehydrate), Logger);
        }

        public bool BeforeProcessEvent<TEvent>(EventContext<TEvent> context)
        {
            // var plugin = await Plugins.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEvent), Logger);
            // var global = await GlobalInterceptor.BeforeProcessEvent(
            //     new Common.Context.Classes.EventContext<TEvent>(context.RootContext.Context, Executor.ServiceProvider, context.Event)
            // );
            var local = Interceptors.RunSafe(i => i.BeforeProcessEvent(context), nameof(BeforeProcessEvent), Logger);

            return /*global &&*/ local/* && plugin*/;
        }

        public void AfterProcessEvent<TEvent>(EventContext<TEvent> context, EventStatus eventStatus)
        {
            Interceptors.RunSafe(i => i.AfterProcessEvent(context, eventStatus), nameof(AfterProcessEvent), Logger);
            // await GlobalInterceptor.AfterProcessEvent(new Common.Context.Classes.EventContext<TEvent>(context.RootContext.Context, Executor.ServiceProvider, context.Event), eventStatus);
            // await Plugins.RunSafe(p => p.AfterProcessEventAsync(context), nameof(AfterProcessEvent), Logger);
        }
        
        public bool OnActionException(ActionDelegateContext context, Exception exception)
            => ExceptionHandlers.RunSafe(h => h.OnActionException(context, exception), nameof(OnActionException), Logger, false);
    }
}
