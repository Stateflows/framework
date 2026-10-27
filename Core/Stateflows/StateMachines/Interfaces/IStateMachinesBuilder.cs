using System;
using System.Reflection;
using System.Collections.Generic;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Registers state-machine classes and global state-machine middleware.</summary>
    /// <remarks>All registration methods return this builder. Global middleware wraps machine-local middleware.
    /// See <see href="https://github.com/Stateflows/framework/wiki/behaviors">registration</see>
    /// and <see href="https://github.com/Stateflows/framework/wiki/observability">observability</see>.</remarks>
    public interface IStateMachinesBuilder
    {
        /// <summary>Discovers and registers concrete typed state-machine definitions in an assembly.</summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>This registration builder.</returns>
        IStateMachinesBuilder AddFromAssembly(Assembly assembly);
        /// <summary>Discovers and registers concrete typed state-machine definitions across assemblies.</summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <returns>This registration builder.</returns>
        IStateMachinesBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);
        /// <summary>Registers a fluent state-machine definition at the default version, 1.</summary>
        /// <param name="stateMachineName">The behavior class name.</param>
        /// <param name="buildAction">The callback defining the machine.</param>
        /// <returns>This registration builder.</returns>
        IStateMachinesBuilder AddStateMachine(string stateMachineName, StateMachineBuildAction buildAction);
        /// <summary>Registers a versioned fluent state-machine definition.</summary>
        /// <param name="stateMachineName">The behavior class name.</param>
        /// <param name="version">The definition version.</param>
        /// <param name="buildAction">The callback defining the machine.</param>
        /// <returns>This registration builder.</returns>
        IStateMachinesBuilder AddStateMachine(string stateMachineName, int version, StateMachineBuildAction buildAction);
        /// <summary>Registers a typed definition and optionally adds machine-local utilities.</summary>
        /// <typeparam name="TStateMachine">The type whose static Build method defines the machine.</typeparam>
        /// <param name="stateMachineName">An explicit name, or null to use the type's resolved behavior name.</param>
        /// <param name="version">The definition version.</param>
        /// <param name="buildAction">Optional callback configuring local middleware and utilities.</param>
        /// <returns>This registration builder.</returns>
        IStateMachinesBuilder AddStateMachine<TStateMachine>(string stateMachineName = null, int version = 1, StateMachineUtilsBuildAction buildAction = null)
            where TStateMachine : class, IStateMachine;
        /// <summary>Registers a typed definition at a specified version using its resolved behavior name.</summary>
        /// <typeparam name="TStateMachine">The machine definition type.</typeparam>
        /// <param name="version">The definition version.</param>
        /// <param name="buildAction">Optional callback configuring local middleware and utilities.</param>
        /// <returns>This registration builder.</returns>
        IStateMachinesBuilder AddStateMachine<TStateMachine>(int version, StateMachineUtilsBuildAction buildAction = null)
            where TStateMachine : class, IStateMachine;
        /// <summary>Registers dependency-injected event middleware for every state machine.</summary>
        /// <typeparam name="TInterceptor">The interceptor implementation type.</typeparam>
        /// <returns>This registration builder.</returns>
        IStateMachinesBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor;
        /// <summary>Registers a factory for global event-processing middleware.</summary>
        /// <param name="interceptorFactory">Creates the interceptor with scoped services and the machine context.</param>
        /// <returns>This registration builder.</returns>
        IStateMachinesBuilder AddInterceptor(StateMachineInterceptorFactory interceptorFactory);
        /// <summary>Registers an asynchronous factory for global event-processing middleware.</summary>
        /// <param name="interceptorFactoryAsync">Asynchronously creates the interceptor with scoped services and the machine context.</param>
        /// <returns>This registration builder.</returns>
        IStateMachinesBuilder AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync);
        /// <summary>Registers a dependency-injected failure acknowledgement handler for every state machine.</summary>
        /// <typeparam name="TExceptionHandler">The exception handler implementation.</typeparam>
        /// <returns>This registration builder.</returns>
        IStateMachinesBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler;
        /// <summary>Registers a factory for a global failure acknowledgement handler, not a recovery handler.</summary>
        /// <param name="exceptionHandlerFactory">Creates the handler using scoped services and the machine context.</param>
        /// <returns>This registration builder.</returns>
        IStateMachinesBuilder AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory);
        /// <summary>Registers an asynchronous factory for a global failure acknowledgement handler.</summary>
        /// <param name="exceptionHandlerFactoryAsync">Asynchronously creates the handler using scoped services and the machine context.</param>
        /// <returns>This registration builder.</returns>
        IStateMachinesBuilder AddExceptionHandler(StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);
        /// <summary>Registers a dependency-injected lifecycle observer for every state machine.</summary>
        /// <typeparam name="TObserver">The observer implementation.</typeparam>
        /// <returns>This registration builder.</returns>
        IStateMachinesBuilder AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver;
        /// <summary>Registers a factory for a global lifecycle observer.</summary>
        /// <param name="observerFactory">Creates the observer using scoped services and the machine context.</param>
        /// <returns>This registration builder.</returns>
        IStateMachinesBuilder AddObserver(StateMachineObserverFactory observerFactory);
        /// <summary>Registers an asynchronous factory for a global lifecycle observer.</summary>
        /// <param name="observerFactoryAsync">Asynchronously creates the observer using scoped services and the machine context.</param>
        /// <returns>This registration builder.</returns>
        IStateMachinesBuilder AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync);
    }
}
