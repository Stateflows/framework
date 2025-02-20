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
        [Obsolete("AddFromLoadedAssemblies() is deprecated, use AddFromAssembly(), AddFromAssemblies() or AddAction() instead.")]
        IActionsBuilder AddFromLoadedAssemblies();
        IActionsBuilder AddAction(string actionName, ActionDelegateAsync actionDelegate);
        IActionsBuilder AddAction(string actionName, int version, ActionDelegateAsync actionDelegate);
        IActionsBuilder AddAction<TAction>(string actionName = null, int version = 1)
            where TAction : class, IAction;
        IActionsBuilder AddAction<TAction>(int version)
            where TAction : class, IAction;
        // IActionsBuilder AddInterceptor<TInterceptor>()
        //     where TInterceptor : class, IActionInterceptor;
        // IActionsBuilder AddInterceptor(ActionInterceptorFactory interceptorFactory);
        // IActionsBuilder AddExceptionHandler<TExceptionHandler>()
        //     where TExceptionHandler : class, IActionExceptionHandler;
        // IActionsBuilder AddExceptionHandler(ActionExceptionHandlerFactory exceptionHandlerFactory);
        // IActionsBuilder AddObserver<TObserver>()
        //     where TObserver : class, IActionObserver;
        // IActionsBuilder AddObserver(ActionObserverFactory observerFactory);
    }
}
