using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Registers machine definitions and global middleware, and visits registered definition metadata.</summary>
    /// <remarks>Prefer the fluent registration builder for application configuration.
    /// See <see href="https://github.com/Stateflows/framework/wiki/behaviors">registration</see>.</remarks>
    public interface IStateMachinesRegister
    {
        /// <summary>Registers a fluent machine definition at version 1.</summary>
        /// <param name="stateMachineName">The behavior class name.</param>
        /// <param name="buildAction">The callback defining its structure and behavior.</param>
        void AddStateMachine(string stateMachineName, StateMachineBuildAction buildAction)
            => AddStateMachine(stateMachineName, 1, buildAction);
        
        /// <summary>Registers a versioned fluent machine definition.</summary>
        /// <param name="stateMachineName">The behavior class name.</param>
        /// <param name="version">The definition version.</param>
        /// <param name="buildAction">The callback defining its structure and behavior.</param>
        void AddStateMachine(string stateMachineName, int version, StateMachineBuildAction buildAction);

        /// <summary>Registers a typed definition at version 1 using an explicit behavior name.</summary>
        /// <param name="stateMachineName">The behavior class name.</param>
        /// <param name="stateMachineType">The type implementing the machine definition contract.</param>
        /// <param name="buildAction">Optional local utilities and middleware configuration.</param>
        void AddStateMachine(string stateMachineName, Type stateMachineType, StateMachineUtilsBuildAction buildAction = null)
            => AddStateMachine(stateMachineName, 1, stateMachineType, buildAction);
        
        /// <summary>Registers a versioned typed definition using an explicit behavior name.</summary>
        /// <param name="stateMachineName">The behavior class name.</param>
        /// <param name="version">The definition version.</param>
        /// <param name="stateMachineType">The type implementing the machine definition contract.</param>
        /// <param name="buildAction">Optional local utilities and middleware configuration.</param>
        void AddStateMachine(string stateMachineName, int version, Type stateMachineType, StateMachineUtilsBuildAction buildAction = null);

        /// <summary>Registers a typed definition by invoking its static build contract.</summary>
        /// <typeparam name="TStateMachine">The machine definition type.</typeparam>
        /// <param name="stateMachineName">An explicit name, or null to use the type's resolved name.</param>
        /// <param name="version">The definition version.</param>
        /// <param name="buildAction">Optional local utilities and middleware configuration.</param>
        void AddStateMachine<TStateMachine>(string stateMachineName = null, int version = 1, StateMachineUtilsBuildAction buildAction = null)
            where TStateMachine : class, IStateMachine;

        /// <summary>Registers a factory for global machine event middleware.</summary>
        /// <param name="interceptorFactory">Creates middleware using scoped services and machine context.</param>
        void AddInterceptor(StateMachineInterceptorFactory interceptorFactory);
        /// <summary>Registers an asynchronous factory for global machine event middleware.</summary>
        /// <param name="interceptorFactoryAsync">Asynchronously creates middleware using scoped services and machine context.</param>
        void AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync);

        /// <summary>Registers dependency-injected global machine event middleware.</summary>
        /// <typeparam name="TInterceptor">The middleware implementation.</typeparam>
        void AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor;

        /// <summary>Registers a factory for global failure acknowledgement, not execution recovery.</summary>
        /// <param name="exceptionHandlerFactory">Creates the failure handler.</param>
        void AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory);
        /// <summary>Registers an asynchronous factory for global failure acknowledgement.</summary>
        /// <param name="exceptionHandlerFactoryAsync">Asynchronously creates the failure handler.</param>
        void AddExceptionHandler(StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);

        /// <summary>Registers a dependency-injected global failure acknowledgement handler.</summary>
        /// <typeparam name="TExceptionHandler">The failure handler implementation.</typeparam>
        void AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler;

        /// <summary>Registers a factory for global machine lifecycle observation.</summary>
        /// <param name="observerFactory">Creates an observer using scoped services and machine context.</param>
        void AddObserver(StateMachineObserverFactory observerFactory);
        /// <summary>Registers an asynchronous factory for global machine lifecycle observation.</summary>
        /// <param name="observerFactoryAsync">Asynchronously creates an observer using scoped services and machine context.</param>
        void AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync);

        /// <summary>Registers a dependency-injected global machine lifecycle observer.</summary>
        /// <typeparam name="TObserver">The observer implementation.</typeparam>
        void AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver;

        /// <summary>Visits the metadata of all registered machine definitions.</summary>
        /// <param name="visitor">The registration metadata visitor, not a runtime observer.</param>
        /// <returns>A task completing when the definitions have been visited.</returns>
        Task VisitStateMachinesAsync(IStateMachineVisitor visitor);
        /// <summary>Visits one registered machine definition at the specified version.</summary>
        /// <param name="stateMachineName">The registered machine name.</param>
        /// <param name="version">The definition version to visit.</param>
        /// <param name="visitor">The registration metadata visitor.</param>
        /// <returns>A task completing when the definition has been visited.</returns>
        Task VisitStateMachineAsync(string stateMachineName, int version, IStateMachineVisitor visitor);
    }
}
