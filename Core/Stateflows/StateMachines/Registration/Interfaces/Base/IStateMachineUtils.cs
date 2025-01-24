namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineUtils<out TReturn>
    {
        /// <summary>
        /// Adds an interceptor to the state machine.
        /// </summary>
        /// <param name="interceptorFactory">The factory method to create the interceptor.</param>
        TReturn AddInterceptor(StateMachineInterceptorFactory interceptorFactory);

        /// <summary>
        /// Adds an interceptor to the state machine.
        /// </summary>
        /// <typeparam name="TInterceptor">The type of the interceptor.</typeparam>
        TReturn AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor;

        /// <summary>
        /// Adds an observer to the state machine.
        /// </summary>
        /// <param name="observerFactory">The factory method to create the observer.</param>
        TReturn AddObserver(StateMachineObserverFactory observerFactory);

        /// <summary>
        /// Adds an observer to the state machine.
        /// </summary>
        /// <typeparam name="TObserver">The type of the observer.</typeparam>
        TReturn AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver;

        /// <summary>
        /// Adds an exception handler to the state machine.
        /// </summary>
        /// <param name="exceptionHandlerFactory">The factory method to create the exception handler.</param>
        TReturn AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory);

        /// <summary>
        /// Adds an exception handler to the state machine.
        /// </summary>
        /// <typeparam name="TExceptionHandler">The type of the exception handler.</typeparam>
        TReturn AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler;
    }
}
