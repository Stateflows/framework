using System.Threading.Tasks;
using Stateflows.Actions.Models;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.Actions.Registration.Interfaces.Base;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;

namespace Stateflows.Actions.Registration.Builders;

internal class ActionBuilder(ActionModel model) : IActionBuilder
{
    public ActionModel Model => model;
    
    public IActionBuilder AddInterceptor(ActionInterceptorFactoryAsync interceptorFactory)
    {
        model.InterceptorFactories.Add(interceptorFactory);

        return this;
    }

    IActionBuilder IActionObservability<IActionBuilder>.AddInterceptor<TInterceptor>()
        => AddInterceptor<TInterceptor>() as IActionBuilder;

    IActionBuilder IActionObservability<IActionBuilder>.AddInterceptor(ActionInterceptorFactoryAsync interceptorFactory)
        => AddInterceptor(interceptorFactory) as IActionBuilder;

    public IActionBuilder AddInterceptor<TInterceptor>()
        where TInterceptor : class, IActionInterceptor
    {
        model.InterceptorFactories.Add(serviceProvider =>
            Task.FromResult<IActionInterceptor>(
                StateflowsActivator.CreateClassInstance<TInterceptor>(serviceProvider)));

        return this;
    }

    public IActionBuilder AddObserver(ActionObserverFactoryAsync observerFactory)
    {
        model.ObserverFactories.Add(observerFactory);

        return this;
    }

    IActionBuilder IActionObservability<IActionBuilder>.AddObserver<TObserver>()
        => AddObserver<TObserver>() as IActionBuilder;

    IActionBuilder IActionObservability<IActionBuilder>.AddObserver(ActionObserverFactoryAsync observerFactory)
        => AddObserver(observerFactory) as IActionBuilder;

    public IActionBuilder AddObserver<TObserver>()
        where TObserver : class, IActionObserver
    {
        model.ObserverFactories.Add(serviceProvider =>
            Task.FromResult<IActionObserver>(StateflowsActivator.CreateClassInstance<TObserver>(serviceProvider)));

        return this;
    }

    public IActionBuilder AddExceptionHandler(ActionExceptionHandlerFactoryAsync exceptionHandlerFactory)
    {
        model.ExceptionHandlerFactories.Add(exceptionHandlerFactory);

        return this;
    }

    IActionBuilder IActionObservability<IActionBuilder>.AddExceptionHandler<TExceptionHandler>()
        => AddExceptionHandler<TExceptionHandler>() as IActionBuilder;

    IActionBuilder IActionObservability<IActionBuilder>.AddExceptionHandler(
        ActionExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
        => AddExceptionHandler(exceptionHandlerFactoryAsync) as IActionBuilder;

    public IActionBuilder AddExceptionHandler<TExceptionHandler>()
        where TExceptionHandler : class, IActionExceptionHandler
    {
        model.ExceptionHandlerFactories.Add(serviceProvider =>
            Task.FromResult<IActionExceptionHandler>(
                StateflowsActivator.CreateClassInstance<TExceptionHandler>(serviceProvider)));

        return this;
    }

    public IActionBuilder SetResourceName(string resourceName)
    {
        model.ResourceName = resourceName;

        return this;
    }

    IActionBuilder IActionUtils<IActionBuilder>.SetIsStateless(bool isStateless)
        => SetIsStateless(isStateless) as IActionBuilder;

    IActionBuilder IActionUtils<IActionBuilder>.SetResourceName(string resourceName)
        => SetResourceName(resourceName) as IActionBuilder;

    public IActionBuilder SetIsStateless(bool isStateless)
    {
        model.IsStateless = isStateless;

        return this;
    }
}

internal class ActionBuilder<TAction>(ActionModel model) : ActionBuilder(model), IActionBuilder<TAction>
    where TAction : class, IAction
{
    public IActionBuilder<TAction> Configure(System.Action<TAction> action)
    {
        throw new System.NotImplementedException();
    }

    IActionBuilder<TAction> IActionUtils<IActionBuilder<TAction>>.SetResourceName(string resourceName)
        => SetResourceName(resourceName) as IActionBuilder<TAction>;

    IActionBuilder<TAction> IActionUtils<IActionBuilder<TAction>>.SetIsStateless(bool isStateless)
        => SetIsStateless(isStateless) as IActionBuilder<TAction>;

    IActionBuilder<TAction> IActionObservability<IActionBuilder<TAction>>.AddInterceptor(ActionInterceptorFactoryAsync interceptorFactory)
        => AddInterceptor(interceptorFactory) as IActionBuilder<TAction>;

    IActionBuilder<TAction> IActionObservability<IActionBuilder<TAction>>.AddInterceptor<TInterceptor>()
        => AddInterceptor<TInterceptor>() as IActionBuilder<TAction>;

    IActionBuilder<TAction> IActionObservability<IActionBuilder<TAction>>.AddObserver(ActionObserverFactoryAsync observerFactory)
        => AddObserver(observerFactory) as IActionBuilder<TAction>;

    IActionBuilder<TAction> IActionObservability<IActionBuilder<TAction>>.AddObserver<TObserver>()
        => AddObserver<TObserver>() as IActionBuilder<TAction>;

    IActionBuilder<TAction> IActionObservability<IActionBuilder<TAction>>.AddExceptionHandler(ActionExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
        => AddExceptionHandler(exceptionHandlerFactoryAsync) as IActionBuilder<TAction>;

    IActionBuilder<TAction> IActionObservability<IActionBuilder<TAction>>.AddExceptionHandler<TExceptionHandler>()
        => AddExceptionHandler<TExceptionHandler>() as IActionBuilder<TAction>;
}