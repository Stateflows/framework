using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Stateflows.Common.Extensions;
using Stateflows.Actions.Attributes;
using Stateflows.Actions.Registration.Interfaces;

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
        public IActionsBuilder AddAction(string actionName, ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null)
            => AddAction(actionName, 1, actionDelegate, buildAction);

        [DebuggerHidden]
        public IActionsBuilder AddAction(string actionName, int version, ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null)
        {
            Register.AddAction(actionName, version, actionDelegate, buildAction);

            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddAction<TAction>(string actionName = null, int version = 1, ActionBuildAction buildAction = null)
            where TAction : class, IAction
        {
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
