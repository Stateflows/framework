using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Stateflows.Actions.Attributes;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Actions.Registration.Interfaces;

namespace Stateflows.Actions.Registration.Builders
{
    internal class ActionsBuilder(ActionsRegister register, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null) : IActionsBuilder
    {
        [DebuggerHidden]
        public IActionsBuilder AddFromAssembly(Assembly assembly)
        {
            assembly.GetAttributedTypes<ActionBehaviorAttribute>().ToList().ForEach(@type =>
            {
                if (typeof(IAction).IsAssignableFrom(@type))
                {
                    var attribute = @type.GetCustomAttributes(typeof(ActionBehaviorAttribute)).FirstOrDefault() as ActionBehaviorAttribute;
                    register.AddAction(attribute?.Name ?? @type.FullName, attribute?.Version ?? 1, @type);
                }
            });

            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                AddFromAssembly(assembly);
            }

            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddFromLoadedAssemblies()
            => AddFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        [DebuggerHidden]
        public IActionsBuilder AddAction(string actionName, ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null)
            => AddAction(actionName, 1, actionDelegate, buildAction);

        [DebuggerHidden]
        public IActionsBuilder AddAction(string actionName, int version, ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null)
        {
            if (register is IOwnedRegistration registration)
            {
                // Register is a singleton, modify IsSystemRegistration only for the duration of the AddAction call
                var originalOwnerClass = registration.OwnerClass;
                var originalParentClass = registration.ParentClass;
                registration.OwnerClass = ownerClass;
                registration.ParentClass = parentClass;

                register.AddAction(actionName, version, actionDelegate, buildAction);
                
                registration.OwnerClass = originalOwnerClass;
                registration.ParentClass = originalParentClass;

                return this;
            }

            register.AddAction(actionName, version, actionDelegate, buildAction);

            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddAction<TAction>(string actionName = null, int version = 1, ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction
        {
            var actionType = typeof(TAction);
            if (typeof(IActionConfiguration).IsAssignableFrom(actionType))
            {
                var originalBuildAction = buildAction;
                buildAction = b =>
                {
                    actionType.CallStaticMethod(nameof(IActionConfiguration.Configure), [typeof(IActionBuilder)], [b]);
                    originalBuildAction?.Invoke(b);
                };
            }
            
            if (register is IOwnedRegistration registration)
            {
                // Register is a singleton, modify IsSystemRegistration only for the duration of the AddAction call
                var originalOwnerClass = registration.OwnerClass;
                var originalParentClass = registration.ParentClass;
                registration.OwnerClass = ownerClass;
                registration.ParentClass = parentClass;

                register.AddAction(actionName ?? Action<TAction>.Name, version, buildAction);
                
                registration.OwnerClass = originalOwnerClass;
                registration.ParentClass = originalParentClass;

                return this;
            }

            register.AddAction(actionName ?? Action<TAction>.Name, version, buildAction);

            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddAction<TAction>(int version, ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction
            => AddAction<TAction>(null, version, buildAction);

        [DebuggerHidden]
        public IActionsBuilder AddAction<TAction>(ActionBuildAction<TAction> buildAction)
            where TAction : class, IAction
            => AddAction<TAction>(1, buildAction);

        #region Observability
        [DebuggerHidden]
        public IActionsBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActionInterceptor
        {
            register.AddInterceptor<TInterceptor>();

            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddInterceptor(ActionInterceptorFactoryAsync interceptorFactoryAsync)
        {
            register.AddInterceptor(interceptorFactoryAsync);

            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddObserver<TObserver>()
            where TObserver : class, IActionObserver
        {
            register.AddObserver<TObserver>();
        
            return this;
        }
        
        [DebuggerHidden]
        public IActionsBuilder AddObserver(ActionObserverFactoryAsync observerFactoryAsync)
        {
            register.AddObserver(observerFactoryAsync);
        
            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActionExceptionHandler
        {
            register.AddExceptionHandler<TExceptionHandler>();

            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddExceptionHandler(ActionExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
        {
            register.AddExceptionHandler(exceptionHandlerFactoryAsync);

            return this;
        }
        #endregion
    }
}
