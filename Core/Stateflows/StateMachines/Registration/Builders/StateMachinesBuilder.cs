using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Attributes;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class StateMachinesBuilder : IStateMachinesBuilder
    {
        private readonly StateMachinesRegister Register;

        public StateMachinesBuilder(StateMachinesRegister register)
        {
            Register = register;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddFromAssembly(Assembly assembly)
        {
            assembly.GetAttributedTypes<StateMachineBehaviorAttribute>().ToList().ForEach(@type =>
            {
                if (
                    typeof(StateMachine).IsAssignableFrom(@type) &&
                    @type.GetCustomAttributes(typeof(StateMachineBehaviorAttribute)).FirstOrDefault() is StateMachineBehaviorAttribute attribute)
                {
                    Register.AddStateMachine(attribute.Name ?? @type.FullName, attribute.Version, @type);
                }
            });

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                AddFromAssembly(assembly);
            }

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddFromLoadedAssemblies()
            => AddFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        [DebuggerHidden]
        public IStateMachinesBuilder AddStateMachine(string stateMachineName, StateMachineBuilderAction buildAction)
        {
            Register.AddStateMachine(stateMachineName, 1, buildAction);

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddStateMachine(string stateMachineName, int version, StateMachineBuilderAction buildAction)
        {
            Register.AddStateMachine(stateMachineName, version, buildAction);

            return this;
        }

        //[DebuggerHidden]
        public IStateMachinesBuilder AddStateMachine<TStateMachine>(string stateMachineName = null, int version = 1)
            where TStateMachine : StateMachine
        {
            Register.AddStateMachine<TStateMachine>(stateMachineName ?? StateMachineInfo<TStateMachine>.Name, version);

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddStateMachine<TStateMachine>(int version)
            where TStateMachine : StateMachine
            => AddStateMachine<TStateMachine>(null, version);

        #region Observability
        [DebuggerHidden]
        public IStateMachinesBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor
        {
            Register.AddGlobalInterceptor<TInterceptor>();

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
        {
            Register.AddGlobalInterceptor(interceptorFactory);

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler
        {
            Register.AddGlobalExceptionHandler<TExceptionHandler>();

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
        {
            Register.AddGlobalExceptionHandler(exceptionHandlerFactory);

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver
        {
            Register.AddGlobalObserver<TObserver>();

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddObserver(StateMachineObserverFactory observerFactory)
        {
            Register.AddGlobalObserver(observerFactory);

            return this;
        }
        #endregion
    }
}

