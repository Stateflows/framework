namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineUtilsBuilderBase<TReturn>
    {
        TReturn AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor;

        TReturn AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver;

        TReturn AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler;
    }
}
