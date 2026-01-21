using System.Threading.Tasks;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.Common.Classes;

namespace Stateflows.StateMachine.Registration.Builders;

internal class StateMachineUtilsBuilder(Graph graph) : IStateMachineUtilsBuilder
{
    public IStateMachineUtilsBuilder AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactory)
    {
        graph.InterceptorFactories.Add(interceptorFactory);

        return this;
    }

    public IStateMachineUtilsBuilder AddInterceptor<TInterceptor>()
        where TInterceptor : class, IStateMachineInterceptor
    {
        graph.InterceptorFactories.Add((serviceProvider, context) => Task.FromResult<IStateMachineInterceptor>(StateflowsActivator.CreateClassInstance<TInterceptor>(serviceProvider)));

        return this;
    }

    public IStateMachineUtilsBuilder AddObserver(StateMachineObserverFactoryAsync observerFactory)
    {
        graph.ObserverFactories.Add(observerFactory);

        return this;
    }

    public IStateMachineUtilsBuilder AddObserver<TObserver>()
        where TObserver : class, IStateMachineObserver
    {
        graph.ObserverFactories.Add((serviceProvider, context) => Task.FromResult<IStateMachineObserver>(StateflowsActivator.CreateClassInstance<TObserver>(serviceProvider)));

        return this;
    }

    public IStateMachineUtilsBuilder AddExceptionHandler(StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactory)
    {
        graph.ExceptionHandlerFactories.Add(exceptionHandlerFactory);

        return this;
    }

    public IStateMachineUtilsBuilder AddExceptionHandler<TExceptionHandler>()
        where TExceptionHandler : class, IStateMachineExceptionHandler
    {
        graph.ExceptionHandlerFactories.Add((serviceProvider, context) => Task.FromResult<IStateMachineExceptionHandler>(StateflowsActivator.CreateClassInstance<TExceptionHandler>(serviceProvider)));

        return this;
    }

    public IStateMachineUtilsBuilder SetResourceName(string resourceName)
    {
        graph.ResourceName = resourceName;

        return this;
    }
}