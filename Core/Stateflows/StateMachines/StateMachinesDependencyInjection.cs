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
                if (typeof(StateMachine).IsAssignableFrom(@type) && @type.GetConstructor(Type.EmptyTypes) != null)
                {
                    var sm = Activator.CreateInstance(@type) as StateMachine;
                    var attribute = @type.GetCustomAttributes(typeof(StateMachineAttribute)).FirstOrDefault() as StateMachineAttribute;
                    stateflowsBuilder.EnsureStateMachinesServices().AddStateMachine(attribute?.Name ?? @type.Name, sm.Build);
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
            where TStateMachine : StateMachine, new()
        {
            stateflowsBuilder.EnsureStateMachinesServices().AddStateMachine<TStateMachine>(stateMachineName ?? StateMachineInfo<TStateMachine>.Name);

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddGlobalInterceptor<TInterceptor>(this IStateflowsBuilder stateflowsBuilder)
            where TInterceptor : class, IStateMachineInterceptor
        {
            stateflowsBuilder.EnsureStateMachinesServices().AddGlobalInterceptor<TInterceptor>();

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddGlobalInterceptor(this IStateflowsBuilder stateflowsBuilder, InterceptorFactory interceptorFactory)
        {
            stateflowsBuilder.EnsureStateMachinesServices().AddGlobalInterceptor(interceptorFactory);

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddGlobalExceptionHandler<TExceptionHandler>(this IStateflowsBuilder stateflowsBuilder)
            where TExceptionHandler : class, IStateMachineExceptionHandler
        {
            stateflowsBuilder.EnsureStateMachinesServices().AddGlobalExceptionHandler<TExceptionHandler>();

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddGlobalExceptionHandler(this IStateflowsBuilder stateflowsBuilder, ExceptionHandlerFactory exceptionHandlerFactory)
        {
            stateflowsBuilder.EnsureStateMachinesServices().AddGlobalExceptionHandler(exceptionHandlerFactory);

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddGlobalObserver<TObserver>(this IStateflowsBuilder stateflowsBuilder)
            where TObserver : class, IStateMachineObserver
        {
            stateflowsBuilder.EnsureStateMachinesServices().AddGlobalObserver<TObserver>();

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddGlobalObserver(this IStateflowsBuilder stateflowsBuilder, ObserverFactory observerFactory)
        {
            stateflowsBuilder.EnsureStateMachinesServices().AddGlobalObserver(observerFactory);

            return stateflowsBuilder;
        }

        private static Dictionary<IStateflowsBuilder, StateMachinesRegister> Registers = new Dictionary<IStateflowsBuilder, StateMachinesRegister>();

        private static StateMachinesRegister EnsureStateMachinesServices(this IStateflowsBuilder stateflowsBuilder)
        {
            if (!stateflowsBuilder.Services.Any(x => x.ServiceType == typeof(StateflowsEngine)))
            {
                var register = new StateMachinesRegister(stateflowsBuilder.Services);
                Registers.Add(stateflowsBuilder, register);

                stateflowsBuilder
                    .EnsureStateflowServices()
                    .AddGlobalObserver(p => p.GetRequiredService<Observer>())
                    .AddGlobalInterceptor(p => p.GetRequiredService<Observer>())
                    .Services
                    .AddScoped<Observer>()
                    .AddSingleton(register)
                    .AddSingleton<IEventProcessor, Processor>()
                    .AddSingleton<IBehaviorProvider, Provider>()
                    .AddSingleton<IStateMachineEventHandler, InitializationHandler>()
                    .AddSingleton<IStateMachineEventHandler, InitializedHandler>()
                    .AddSingleton<IStateMachineEventHandler, CurrentStateHandler>()
                    .AddSingleton<IStateMachineEventHandler, ExpectedEventsHandler>();
            }

            if (!Registers.TryGetValue(stateflowsBuilder, out var result))
            {
                Debug.Assert(result != null, "No StateMachineRegister instance available. Are services registered properly?");
            }

            return result;
        }
    }
}
