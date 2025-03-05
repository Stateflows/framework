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

        public async Task AfterHydrateAsync(ActionDelegateContext context)
        {
            // await Plugins.RunSafe(p => p.AfterHydrateAsync(context), nameof(AfterHydrateAsync), Logger);
            await Interceptors.RunSafe(i => i.AfterHydrateAsync(context), nameof(AfterHydrateAsync), Logger);
        }

        public async Task BeforeDehydrateAsync(ActionDelegateContext context)
        {
            await Interceptors.RunSafe(i => i.BeforeDehydrateAsync(context), nameof(BeforeDehydrateAsync), Logger);
            // await Plugins.RunSafe(p => p.BeforeDehydrateAsync(context), nameof(BeforeDehydrateAsync), Logger);
        }

        public async Task<bool> BeforeProcessEventAsync<TEvent>(EventContext<TEvent> context)
        {
            // var plugin = await Plugins.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEventAsync), Logger);
            var global = await GlobalInterceptor.BeforeProcessEventAsync(
                new Common.Context.Classes.EventContext<TEvent>(context.RootContext.Context, Executor.ServiceProvider, context.Event)
            );
            var local = await Interceptors.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEventAsync), Logger);

            return global && local/* && plugin*/;
        }

        public async Task AfterProcessEventAsync<TEvent>(EventContext<TEvent> context)
        {
            await Interceptors.RunSafe(i => i.AfterProcessEventAsync(context), nameof(AfterProcessEventAsync), Logger);
            await GlobalInterceptor.AfterProcessEventAsync(new Common.Context.Classes.EventContext<TEvent>(context.RootContext.Context, Executor.ServiceProvider, context.Event));
            // await Plugins.RunSafe(p => p.AfterProcessEventAsync(context), nameof(AfterProcessEventAsync), Logger);
        }
        
        public Task<bool> OnActionExceptionAsync(ActionDelegateContext context, Exception exception)
            => ExceptionHandlers.RunSafe(h => h.OnActionExceptionAsync(context, exception), nameof(OnActionExceptionAsync), Logger, false);
    }
}
