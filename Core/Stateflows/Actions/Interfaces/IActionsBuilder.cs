using System;
using System.Reflection;
using System.Collections.Generic;
using Stateflows.Actions.Registration;

namespace Stateflows.Actions
{
    public interface IActionsBuilder
    {
        IActionsBuilder AddFromAssembly(Assembly assembly);
        IActionsBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);
        IActionsBuilder AddAction(string actionName, ActionDelegateAsync actionDelegate, bool reentrant = true);
        IActionsBuilder AddAction(string actionName, int version, ActionDelegateAsync actionDelegate, bool reentrant = true);
        IActionsBuilder AddAction<TAction>(string actionName = null, int version = 1, bool reentrant = true)
            where TAction : class, IAction;
        IActionsBuilder AddAction<TAction>(int version, bool reentrant = true)
            where TAction : class, IAction;
        IActionsBuilder AddAction<TAction>(bool reentrant)
            where TAction : class, IAction;
        IActionsBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActionInterceptor;
        IActionsBuilder AddInterceptor(ActionInterceptorFactoryAsync interceptorFactoryAsync);
        IActionsBuilder AddInterceptor(ActionInterceptorFactory interceptorFactory)
            => AddInterceptor(async serviceProvider => interceptorFactory(serviceProvider));
        IActionsBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActionExceptionHandler;
        IActionsBuilder AddExceptionHandler(ActionExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);
        IActionsBuilder AddExceptionHandler(ActionExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(async serviceProvider => exceptionHandlerFactory(serviceProvider));
    }
}
