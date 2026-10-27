using System;
using System.Threading.Tasks;
using Stateflows.Actions.Registration;
using Stateflows.Actions.Registration.Interfaces;

namespace Stateflows.Actions
{
    /// <summary>Registers action definitions and global action observability components.</summary>
    /// <remarks>Application code should normally use <see cref="ActionsDependencyInjection"/> and its <c>AddActions</c> extension rather than resolving this low-level registration contract directly.</remarks>
    public interface IActionsRegister
    {
        /// <summary>Registers an action delegate under its default version.</summary>
        /// <param name="actionName">The action's registration name.</param>
        /// <param name="actionDelegate">The action implementation.</param>
        /// <param name="buildAction">Optional callback for action configuration.</param>
        void AddAction(string actionName, ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null)
            => AddAction(actionName, 1, actionDelegate, buildAction);
        
        /// <summary>Registers a versioned action delegate.</summary>
        /// <param name="actionName">The action's registration name.</param>
        /// <param name="version">The definition version.</param>
        /// <param name="actionDelegate">The action implementation.</param>
        /// <param name="buildAction">Optional callback for action configuration.</param>
        void AddAction(string actionName, int version, ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null);

        /// <summary>Registers an action type under its default version.</summary>
        /// <param name="actionName">The action's registration name.</param>
        /// <param name="actionType">The concrete action type.</param>
        /// <param name="buildAction">Optional callback for action configuration.</param>
        void AddAction(string actionName, Type actionType, ActionBuildAction buildAction = null)
            => AddAction(actionName, 1, actionType, buildAction);
        
        /// <summary>Registers an action type and version.</summary>
        /// <param name="actionName">The action's registration name.</param>
        /// <param name="version">The definition version.</param>
        /// <param name="actionType">The concrete action type.</param>
        /// <param name="buildAction">Optional callback for action configuration.</param>
        void AddAction(string actionName, int version, Type actionType, ActionBuildAction buildAction = null);
        
        /// <summary>Registers a typed action using its inferred name unless a name is supplied.</summary>
        /// <typeparam name="TAction">The action implementation type.</typeparam>
        /// <param name="actionName">Optional registration name.</param>
        /// <param name="version">The definition version.</param>
        /// <param name="buildAction">Optional callback for action configuration.</param>
        void AddAction<TAction>(string actionName = null, int version = 1, ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction;

        /// <summary>Visits all registered action definitions.</summary>
        /// <param name="visitor">The visitor that receives registration metadata.</param>
        Task VisitActionsAsync(IActionVisitor visitor);

        #region Observability
        /// <summary>Registers an asynchronously constructed global action observer.</summary>
        /// <param name="observerFactoryAsync">The observer factory.</param>
        void AddObserver(ActionObserverFactoryAsync observerFactoryAsync);
        
        /// <summary>Registers a global action observer implementation type.</summary>
        /// <typeparam name="TObserver">The observer type.</typeparam>
        void AddObserver<TObserver>()
            where TObserver : class, IActionObserver;
        
        /// <summary>Registers an asynchronously constructed global action interceptor.</summary>
        /// <param name="interceptorFactoryAsync">The interceptor factory.</param>
        void AddInterceptor(ActionInterceptorFactoryAsync interceptorFactoryAsync);
        
        /// <summary>Registers a global action interceptor implementation type.</summary>
        /// <typeparam name="TInterceptor">The interceptor type.</typeparam>
        void AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActionInterceptor;

        /// <summary>Registers an asynchronously constructed global action exception handler.</summary>
        /// <param name="exceptionHandlerFactoryAsync">The handler factory.</param>
        void AddExceptionHandler(ActionExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);
        
        /// <summary>Registers a global action exception-handler implementation type.</summary>
        /// <typeparam name="TExceptionHandler">The exception-handler type.</typeparam>
        void AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActionExceptionHandler;
        #endregion
    }
}
