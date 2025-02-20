using System;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachinesRegister
    {
        void AddStateMachine(string stateMachineName, StateMachineBuildAction buildAction)
            => AddStateMachine(stateMachineName, 1, buildAction);
        
        void AddStateMachine(string stateMachineName, int version, StateMachineBuildAction buildAction);

        void AddStateMachine(string stateMachineName, Type stateMachineType)
            => AddStateMachine(stateMachineName, 1, stateMachineType);
        
        void AddStateMachine(string stateMachineName, int version, Type stateMachineType);

        void AddStateMachine<TStateMachine>(string stateMachineName = null, int version = 1)
            where TStateMachine : class, IStateMachine;

        void AddInterceptor(StateMachineInterceptorFactory interceptorFactory);

        void AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor;

        void AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory);

        void AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler;

        void AddObserver(StateMachineObserverFactory observerFactory);

        void AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver;
    }
}
