using System.Reflection;
using System.Collections.Generic;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachinesBuilder
    {
        IStateMachinesBuilder AddFromAssembly(Assembly assembly);
        IStateMachinesBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);
        IStateMachinesBuilder AddFromLoadedAssemblies();
        IStateMachinesBuilder AddStateMachine(string stateMachineName, StateMachineBuilderAction buildAction);
        IStateMachinesBuilder AddStateMachine(string stateMachineName, int version, StateMachineBuilderAction buildAction);
        IStateMachinesBuilder AddStateMachine<TStateMachine>(string stateMachineName = null, int version = 1)
            where TStateMachine : StateMachine;
        IStateMachinesBuilder AddStateMachine<TStateMachine>(int version)
            where TStateMachine : StateMachine;
        IStateMachinesBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor;
        IStateMachinesBuilder AddInterceptor(StateMachineInterceptorFactory interceptorFactory);
        IStateMachinesBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler;
        IStateMachinesBuilder AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory);
        IStateMachinesBuilder AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver;
        IStateMachinesBuilder AddObserver(StateMachineObserverFactory observerFactory);
    }
}
