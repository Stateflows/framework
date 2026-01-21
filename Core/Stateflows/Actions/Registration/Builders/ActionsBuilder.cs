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
    internal class ActionsBuilder(ActionsRegister register, bool systemRegistrations) : IActionsBuilder
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
            if (register is IIsSystemRegistration registration)
            {
                // Register is a singleton, modify IsSystemRegistration only for the duration of the AddAction call
                var beforeValue = registration.IsSystemRegistration;
                registration.IsSystemRegistration = systemRegistrations;

                register.AddAction(actionName, version, actionDelegate, reentrant);
                registration.IsSystemRegistration = beforeValue;

                return this;
            }

            Register.AddAction(actionName, version, actionDelegate, buildAction);

            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddAction<TAction>(string actionName = null, int version = 1, ActionBuildAction buildAction = null)
            where TAction : class, IAction
        {
            if (register is IIsSystemRegistration registration)
            {
                // Register is a singleton, modify IsSystemRegistration only for the duration of the AddAction call
                var beforeValue = registration.IsSystemRegistration;
                registration.IsSystemRegistration = systemRegistrations;

                register.AddAction<TAction>(actionName ?? Action<TAction>.Name, version, reentrant);
                registration.IsSystemRegistration = beforeValue;

                return this;
            }

            Register.AddAction<TAction>(actionName ?? Action<TAction>.Name, version, buildAction);

            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddAction<TAction>(int version, ActionBuildAction buildAction = null)
            where TAction : class, IAction
            => AddAction<TAction>(null, version, buildAction);

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
            Register.AddObserver<TObserver>();
        
            return this;
        }
        
        [DebuggerHidden]
        public IActionsBuilder AddObserver(ActionObserverFactoryAsync observerFactoryAsync)
        {
            Register.AddObserver(observerFactoryAsync);
        
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
