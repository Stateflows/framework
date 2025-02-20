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

        public ActionsBuilder(ActionsRegister register)
        {
            Register = register;
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
        public IActionsBuilder AddAction(string activityName, ActionDelegateAsync actionDelegate)
            => AddAction(activityName, 1, actionDelegate);

        [DebuggerHidden]
        public IActionsBuilder AddAction(string activityName, int version, ActionDelegateAsync actionDelegate)
        {
            Register.AddAction(activityName, version, actionDelegate);

            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddAction<TAction>(string activityName = null, int version = 1)
            where TAction : class, IAction
        {
            Register.AddAction<TAction>(activityName ?? Action<TAction>.Name, version);

            return this;
        }

        [DebuggerHidden]
        public IActionsBuilder AddAction<TAction>(int version)
            where TAction : class, IAction
            => AddAction<TAction>(null, version);

        // #region Observability
        // [DebuggerHidden]
        // public IActionsBuilder AddInterceptor<TInterceptor>()
        //     where TInterceptor : class, IActionInterceptor
        // {
        //     Register.AddInterceptor<TInterceptor>();
        //
        //     return this;
        // }
        //
        // [DebuggerHidden]
        // public IActionsBuilder AddInterceptor(ActionInterceptorFactory interceptorFactory)
        // {
        //     Register.AddInterceptor(interceptorFactory);
        //
        //     return this;
        // }
        //
        // [DebuggerHidden]
        // public IActionsBuilder AddExceptionHandler<TExceptionHandler>()
        //     where TExceptionHandler : class, IActionExceptionHandler
        // {
        //     Register.AddExceptionHandler<TExceptionHandler>();
        //
        //     return this;
        // }
        //
        // [DebuggerHidden]
        // public IActionsBuilder AddExceptionHandler(ActionExceptionHandlerFactory exceptionHandlerFactory)
        // {
        //     Register.AddExceptionHandler(exceptionHandlerFactory);
        //
        //     return this;
        // }
        //
        // [DebuggerHidden]
        // public IActionsBuilder AddObserver<TObserver>()
        //     where TObserver : class, IActionObserver
        // {
        //     Register.AddObserver<TObserver>();
        //
        //     return this;
        // }
        //
        // [DebuggerHidden]
        // public IActionsBuilder AddObserver(ActionObserverFactory observerFactory)
        // {
        //     Register.AddObserver(observerFactory);
        //
        //     return this;
        // }
        // #endregion
    }
}
