using System;
using System.Threading.Tasks;
using Stateflows.Actions.Registration;
using Stateflows.Actions.Registration.Interfaces;

namespace Stateflows.Actions
{
    public interface IActionsRegister
    {
        void AddAction(string actionName, ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null)
            => AddAction(actionName, 1, actionDelegate, buildAction);
        
        void AddAction(string actionName, int version, ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null);

        void AddAction(string actionName, Type actionType, ActionBuildAction buildAction = null)
            => AddAction(actionName, 1, actionType, buildAction);
        
        void AddAction(string actionName, int version, Type actionType, ActionBuildAction buildAction = null);
        
        void AddAction<TAction>(string actionName = null, int version = 1, ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction;

        Task VisitActionsAsync(IActionVisitor visitor);

        #region Observability
        void AddObserver(ActionObserverFactoryAsync observerFactoryAsync);
        
        void AddObserver<TObserver>()
            where TObserver : class, IActionObserver;
        
        void AddInterceptor(ActionInterceptorFactoryAsync interceptorFactoryAsync);
        
        void AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActionInterceptor;

        void AddExceptionHandler(ActionExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);
        
        void AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActionExceptionHandler;
        #endregion
    }
}
