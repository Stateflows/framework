using System.Threading.Tasks;
using Stateflows.Activities.Models;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Classes;

namespace Stateflows.Activities.Registration.Builders;

internal class ActivityUtilsBuilder(Graph graph) : IActivityUtilsBuilder
{
    public IActivityUtilsBuilder AddInterceptor(ActivityInterceptorFactoryAsync interceptorFactory)
    {
        graph.InterceptorFactories.Add(interceptorFactory);

        return this;
    }

    public IActivityUtilsBuilder AddInterceptor<TInterceptor>()
        where TInterceptor : class, IActivityInterceptor
    {
        graph.InterceptorFactories.Add(serviceProvider => Task.FromResult<IActivityInterceptor>(StateflowsActivator.CreateClassInstance<TInterceptor>(serviceProvider)));

        return this;
    }

    public IActivityUtilsBuilder AddObserver(ActivityObserverFactoryAsync observerFactory)
    {
        graph.ObserverFactories.Add(observerFactory);

        return this;
    }

    public IActivityUtilsBuilder AddObserver<TObserver>()
        where TObserver : class, IActivityObserver
    {
        graph.ObserverFactories.Add(serviceProvider => Task.FromResult<IActivityObserver>(StateflowsActivator.CreateClassInstance<TObserver>(serviceProvider)));

        return this;
    }

    public IActivityUtilsBuilder AddExceptionHandler(ActivityExceptionHandlerFactoryAsync exceptionHandlerFactory)
    {
        graph.ExceptionHandlerFactories.Add(exceptionHandlerFactory);

        return this;
    }

    public IActivityUtilsBuilder AddExceptionHandler<TExceptionHandler>()
        where TExceptionHandler : class, IActivityExceptionHandler
    {
        graph.ExceptionHandlerFactories.Add(serviceProvider => Task.FromResult<IActivityExceptionHandler>(StateflowsActivator.CreateClassInstance<TExceptionHandler>(serviceProvider)));

        return this;
    }

    public IActivityUtilsBuilder SetResourceName(string resourceName)
    {
        graph.ResourceName = resourceName;

        return this;
    }
}