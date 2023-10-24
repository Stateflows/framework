using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Attributes;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.EventHandlers;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateMachinesDependencyInjection
    {
        [DebuggerHidden]
        public static IStateflowsBuilder AddStateMachines(this IStateflowsBuilder stateflowsBuilder, Assembly assembly)
        {
            assembly.GetAttributedTypes<StateMachineAttribute>().ToList().ForEach(@type =>
            {
                if (typeof(StateMachine).IsAssignableFrom(@type))
                {
                    var attribute = @type.GetCustomAttributes(typeof(StateMachineAttribute)).FirstOrDefault() as StateMachineAttribute;
                    stateflowsBuilder.EnsureStateMachinesServices().AddStateMachine(attribute?.Name ?? @type.FullName, @type);
                }
            });

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddStateMachines(this IStateflowsBuilder stateflowsBuilder, IEnumerable<Assembly> assemblies = null)
        {
            if (assemblies == null)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            foreach (var assembly in assemblies)
            {
                stateflowsBuilder.AddStateMachines(assembly);
            }

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddStateMachine(this IStateflowsBuilder stateflowsBuilder, string stateMachineName, StateMachineBuilderAction buildAction)
        {
            stateflowsBuilder.EnsureStateMachinesServices().AddStateMachine(stateMachineName, buildAction);

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddStateMachine<TStateMachine>(this IStateflowsBuilder stateflowsBuilder, string stateMachineName = null)
            where TStateMachine : StateMachine
        {
            stateflowsBuilder.EnsureStateMachinesServices().AddStateMachine<TStateMachine>(stateMachineName ?? StateMachineInfo<TStateMachine>.Name);

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddStateMachinesInterceptor<TInterceptor>(this IStateflowsBuilder stateflowsBuilder)
            where TInterceptor : class, IStateMachineInterceptor
        {
            stateflowsBuilder.EnsureStateMachinesServices().AddGlobalInterceptor<TInterceptor>();

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddStateMachinesInterceptor(this IStateflowsBuilder stateflowsBuilder, StateMachineInterceptorFactory interceptorFactory)
        {
            stateflowsBuilder.EnsureStateMachinesServices().AddGlobalInterceptor(interceptorFactory);

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddStateMachinesExceptionHandler<TExceptionHandler>(this IStateflowsBuilder stateflowsBuilder)
            where TExceptionHandler : class, IStateMachineExceptionHandler
        {
            stateflowsBuilder.EnsureStateMachinesServices().AddGlobalExceptionHandler<TExceptionHandler>();

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddStateMachinesExceptionHandler(this IStateflowsBuilder stateflowsBuilder, StateMachineExceptionHandlerFactory exceptionHandlerFactory)
        {
            stateflowsBuilder.EnsureStateMachinesServices().AddGlobalExceptionHandler(exceptionHandlerFactory);

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddStateMachinesObserver<TObserver>(this IStateflowsBuilder stateflowsBuilder)
            where TObserver : class, IStateMachineObserver
        {
            stateflowsBuilder.EnsureStateMachinesServices().AddGlobalObserver<TObserver>();

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddStateMachinesObserver(this IStateflowsBuilder stateflowsBuilder, StateMachineObserverFactory observerFactory)
        {
            stateflowsBuilder.EnsureStateMachinesServices().AddGlobalObserver(observerFactory);

            return stateflowsBuilder;
        }

        private readonly static Dictionary<IStateflowsBuilder, StateMachinesRegister> Registers = new Dictionary<IStateflowsBuilder, StateMachinesRegister>();

        private static StateMachinesRegister EnsureStateMachinesServices(this IStateflowsBuilder stateflowsBuilder)
        {
            if (!Registers.TryGetValue(stateflowsBuilder, out var register))
            {
                register = new StateMachinesRegister(stateflowsBuilder.ServiceCollection);
                Registers.Add(stateflowsBuilder, register);

                stateflowsBuilder
                    .EnsureStateflowServices()
                    .ServiceCollection
                    .AddScoped<IStateMachinePlugin, TimeEvents>()
                    .AddScoped<IStateMachinePlugin, Submachines>()
                    .AddSingleton(register)
                    .AddSingleton<IEventProcessor, Processor>()
                    .AddTransient<IBehaviorProvider, Provider>()
                    .AddSingleton<IStateMachineEventHandler, InitializationHandler>()
                    .AddSingleton<IStateMachineEventHandler, BehaviorStatusHandler>()
                    .AddSingleton<IStateMachineEventHandler, CurrentStateHandler>()
                    .AddSingleton<IStateMachineEventHandler, ExitHandler>();
            }

            return register;
        }
    }
}
