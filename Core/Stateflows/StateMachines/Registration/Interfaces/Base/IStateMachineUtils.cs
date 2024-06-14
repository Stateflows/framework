namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineUtils<out TReturn>
    {
        TReturn AddInterceptor(StateMachineInterceptorFactory interceptorFactory);

        TReturn AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor;

        TReturn AddObserver(StateMachineObserverFactory observerFactory);

        TReturn AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver;

        TReturn AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory);

        TReturn AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler;
    }
}
