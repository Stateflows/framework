﻿using System;
using System.Reflection;
using System.Collections.Generic;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachinesBuilder
    {
        IStateMachinesBuilder AddFromAssembly(Assembly assembly);
        IStateMachinesBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);
        [Obsolete("AddFromLoadedAssemblies() is deprecated, use AddFromAssembly(), AddFromAssemblies() or AddStateMachine() instead.")]
        IStateMachinesBuilder AddFromLoadedAssemblies();
        IStateMachinesBuilder AddStateMachine(string stateMachineName, StateMachineBuildAction buildAction);
        IStateMachinesBuilder AddStateMachine(string stateMachineName, int version, StateMachineBuildAction buildAction);
        IStateMachinesBuilder AddStateMachine<TStateMachine>(string stateMachineName = null, int version = 1)
            where TStateMachine : class, IStateMachine;
        IStateMachinesBuilder AddStateMachine<TStateMachine>(int version)
            where TStateMachine : class, IStateMachine;
        IStateMachinesBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor;
        IStateMachinesBuilder AddInterceptor(StateMachineInterceptorFactory interceptorFactory);
        IStateMachinesBuilder AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync);
        IStateMachinesBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler;
        IStateMachinesBuilder AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory);
        IStateMachinesBuilder AddExceptionHandler(StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);
        IStateMachinesBuilder AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver;
        IStateMachinesBuilder AddObserver(StateMachineObserverFactory observerFactory);
        IStateMachinesBuilder AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync);
    }
}
