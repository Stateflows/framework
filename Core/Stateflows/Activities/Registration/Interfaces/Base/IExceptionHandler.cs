using System;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IExceptionHandler<out TReturn>
    {
        TReturn AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            where TException : Exception;
    }
}
