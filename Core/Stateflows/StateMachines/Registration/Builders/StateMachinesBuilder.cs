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
        private readonly IStateMachinesRegister Register;

        public StateMachinesBuilder(IStateMachinesRegister register)
        {
            Register = register;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddFromAssembly(Assembly assembly)
        {
            assembly.GetAttributedTypes<StateMachineBehaviorAttribute>().ToList().ForEach(@type =>
            {
                if (
                    typeof(IStateMachine).IsAssignableFrom(@type) &&
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
        public IStateMachinesBuilder AddStateMachine(string stateMachineName, StateMachineBuildAction buildAction)
        {
            Register.AddStateMachine(stateMachineName, 1, buildAction);

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddStateMachine(string stateMachineName, int version, StateMachineBuildAction buildAction)
        {
            Register.AddStateMachine(stateMachineName, version, buildAction);

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddStateMachine<TStateMachine>(string stateMachineName = null, int version = 1)
            where TStateMachine : class, IStateMachine
        {
            Register.AddStateMachine<TStateMachine>(stateMachineName ?? StateMachine<TStateMachine>.Name, version);

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddStateMachine<TStateMachine>(int version)
            where TStateMachine : class, IStateMachine
            => AddStateMachine<TStateMachine>(null, version);

        #region Observability
        [DebuggerHidden]
        public IStateMachinesBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor
        {
            Register.AddInterceptor<TInterceptor>();

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
        {
            Register.AddInterceptor(interceptorFactory);

            return this;
        }
        
        [DebuggerHidden]
        public IStateMachinesBuilder AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
        {
            Register.AddInterceptor(interceptorFactoryAsync);

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler
        {
            Register.AddExceptionHandler<TExceptionHandler>();

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
        {
            Register.AddExceptionHandler(exceptionHandlerFactory);

            return this;
        }
        
        [DebuggerHidden]
        public IStateMachinesBuilder AddExceptionHandler(StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
        {
            Register.AddExceptionHandler(exceptionHandlerFactoryAsync);

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver
        {
            Register.AddObserver<TObserver>();

            return this;
        }

        [DebuggerHidden]
        public IStateMachinesBuilder AddObserver(StateMachineObserverFactory observerFactory)
        {
            Register.AddObserver(observerFactory);

            return this;
        }
        
        [DebuggerHidden]
        public IStateMachinesBuilder AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync)
        {
            Register.AddObserver(observerFactoryAsync);

            return this;
        }

   //      [DebuggerHidden]
   //      public IStateMachinesBuilder AddVisitor<TVisitor>()
   //          where TVisitor : class, IStateMachineVisitor
   //      {
   //          Register.AddVisitor<TVisitor>();
   //
   //          return this;
   //      }
   //
   //      [DebuggerHidden]
   //      public IStateMachinesBuilder AddVisitor(StateMachineVisitorFactory visitorFactory)
   // {
   //          Register.AddVisitor(visitorFactory);
   //
   //          return this;
   //      }
   //      
   //      [DebuggerHidden]
   //      public IStateMachinesBuilder AddVisitor(StateMachineVisitorFactoryAsync visitorFactoryAsync)
   //      {
   //          Register.AddVisitor(visitorFactoryAsync);
   //
   //          return this;
   //      }
        #endregion
    }
}

