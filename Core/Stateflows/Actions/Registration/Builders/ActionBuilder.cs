using System.Threading.Tasks;
using Stateflows.Actions.Models;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.Common.Classes;

namespace Stateflows.Actions.Registration.Builders;

internal class ActionBuilder(ActionModel model) : IActionBuilder
{
    public IActionBuilder AddInterceptor(ActionInterceptorFactoryAsync interceptorFactory)
    {
        model.InterceptorFactories.Add(interceptorFactory);
        
        return this;
    }

    public IActionBuilder AddInterceptor<TInterceptor>()
        where TInterceptor : class, IActionInterceptor
    {
        model.InterceptorFactories.Add(serviceProvider => Task.FromResult<IActionInterceptor>(StateflowsActivator.CreateClassInstance<TInterceptor>(serviceProvider)));
        
        return this;
    }

    public IActionBuilder AddObserver(ActionObserverFactoryAsync observerFactory)
    {
        model.ObserverFactories.Add(observerFactory);
        
        return this;
    }

    public IActionBuilder AddObserver<TObserver>()
        where TObserver : class, IActionObserver
    {
        model.ObserverFactories.Add(serviceProvider => Task.FromResult<IActionObserver>(StateflowsActivator.CreateClassInstance<TObserver>(serviceProvider)));
        
        return this;
    }

    public IActionBuilder AddExceptionHandler(ActionExceptionHandlerFactoryAsync exceptionHandlerFactory)
    {
        model.ExceptionHandlerFactories.Add(exceptionHandlerFactory);
        
        return this;
    }

    public IActionBuilder AddExceptionHandler<TExceptionHandler>()
        where TExceptionHandler : class, IActionExceptionHandler
    {
        model.ExceptionHandlerFactories.Add(serviceProvider => Task.FromResult<IActionExceptionHandler>(StateflowsActivator.CreateClassInstance<TExceptionHandler>(serviceProvider)));
        
        return this;
    }

    public IActionBuilder SetResourceName(string resourceName)
    {
        model.ResourceName = resourceName;
        
        return this;
    }

    public IActionBuilder SetIsStateless(bool isStateless)
    {
        model.IsStateless = isStateless;
        
        return this;
    }
}