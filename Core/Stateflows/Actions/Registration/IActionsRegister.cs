using System;
using Stateflows.Actions.Registration;

namespace Stateflows.Actions
{
    public interface IActionsRegister
    {
        void AddAction(string actionName, ActionDelegateAsync actionDelegate, bool reentrant = true)
            => AddAction(actionName, 1, actionDelegate);
        
        void AddAction(string actionName, int version, ActionDelegateAsync actionDelegate, bool reentrant = true);

        void AddAction(string actionName, Type actionType)
            => AddAction(actionName, 1, actionType);
        
        void AddAction(string actionName, int version, Type actionType, bool reentrant = true);
        
        void AddAction<TAction>(string actionName = null, int version = 1, bool reentrant = true)
            where TAction : class, IAction;

        // #region Observability
        // void AddInterceptor(ActionInterceptorFactory interceptorFactory);
        //
        // void AddInterceptor<TInterceptor>()
        //     where TInterceptor : class, IActionInterceptor;
        //
        // void AddExceptionHandler(ActionExceptionHandlerFactory exceptionHandlerFactory);
        //
        // void AddExceptionHandler<TExceptionHandler>()
        //     where TExceptionHandler : class, IActionExceptionHandler;
        //
        // void AddObserver(ActionObserverFactory observerFactory);
        //
        // void AddObserver<TObserver>()
        //     where TObserver : class, IActionObserver;
        //
        // #endregion
    }
}
