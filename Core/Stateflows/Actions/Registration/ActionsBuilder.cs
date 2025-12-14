using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Stateflows.Actions.Attributes;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;

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
        public IActionsBuilder AddAction(string actionName, ActionDelegateAsync actionDelegate, bool reentrant = true)
            => AddAction(actionName, 1, actionDelegate, reentrant);

        [DebuggerHidden]
        public IActionsBuilder AddAction(string actionName, int version, ActionDelegateAsync actionDelegate, bool reentrant = true)
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

            register.AddAction(actionName, version, actionDelegate, reentrant);

            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddAction<TAction>(string actionName = null, int version = 1, bool reentrant = true)
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

            register.AddAction<TAction>(actionName ?? Action<TAction>.Name, version, reentrant);

            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddAction<TAction>(int version, bool reentrant = true)
            where TAction : class, IAction
            => AddAction<TAction>(null, version, reentrant);

        [DebuggerHidden]
        public IActionsBuilder AddAction<TAction>(bool reentrant = true)
            where TAction : class, IAction
            => AddAction<TAction>(null, 1, reentrant);

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
