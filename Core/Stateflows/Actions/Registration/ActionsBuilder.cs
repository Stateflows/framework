using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Stateflows.Common.Extensions;
using Stateflows.Actions.Attributes;

namespace Stateflows.Actions.Registration.Builders
{
    internal class ActionsBuilder : IActionsBuilder
    {
        private readonly ActionsRegister Register;
        private readonly bool SystemRegistrations;

        public ActionsBuilder(ActionsRegister register, bool systemRegistrations)
        {
            Register = register;
            SystemRegistrations = systemRegistrations;
        }

        [DebuggerHidden]
        public IActionsBuilder AddFromAssembly(Assembly assembly)
        {
            assembly.GetAttributedTypes<ActionBehaviorAttribute>().ToList().ForEach(@type =>
            {
                if (typeof(IAction).IsAssignableFrom(@type))
                {
                    var attribute = @type.GetCustomAttributes(typeof(ActionBehaviorAttribute)).FirstOrDefault() as ActionBehaviorAttribute;
                    Register.AddAction(attribute?.Name ?? @type.FullName, attribute?.Version ?? 1, @type);
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
            Register.AddAction(actionName, version, actionDelegate, reentrant);

            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddAction<TAction>(string actionName = null, int version = 1, bool reentrant = true)
            where TAction : class, IAction
        {
            Register.AddAction<TAction>(actionName ?? Action<TAction>.Name, version, reentrant);

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
            Register.AddInterceptor<TInterceptor>();
        
            return this;
        }
        
        [DebuggerHidden]
        public IActionsBuilder AddInterceptor(ActionInterceptorFactoryAsync interceptorFactoryAsync)
        {
            Register.AddInterceptor(interceptorFactoryAsync);
        
            return this;
        }
        
        [DebuggerHidden]
        public IActionsBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActionExceptionHandler
        {
            Register.AddExceptionHandler<TExceptionHandler>();
        
            return this;
        }
        
        [DebuggerHidden]
        public IActionsBuilder AddExceptionHandler(ActionExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
        {
            Register.AddExceptionHandler(exceptionHandlerFactoryAsync);
        
            return this;
        }
        #endregion
    }
}
