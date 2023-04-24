using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;
using System.Runtime.Serialization;

namespace Stateflows.StateMachines.Registration
{
    internal class StateMachinesRegister
    {
        private IServiceCollection Services { get; }

        public List<ExceptionHandlerFactory> GlobalExceptionHandlerFactories { get; set; } = new List<ExceptionHandlerFactory>();

        public List<InterceptorFactory> GlobalInterceptorFactories { get; set; } = new List<InterceptorFactory>();

        public List<ObserverFactory> GlobalObserverFactories { get; set; } = new List<ObserverFactory>();

        public StateMachinesRegister(IServiceCollection services)
        {
            Services = services;
        }

        public Dictionary<string, Graph> StateMachines { get; set; } = new Dictionary<string, Graph>();

        [DebuggerHidden]
        public void AddStateMachine(string stateMachineName, StateMachineBuilderAction buildAction)
        {
            if (StateMachines.ContainsKey(stateMachineName))
            {
                throw new Exception($"State machine '{stateMachineName}' is already registered");
            }

            var builder = new StateMachineBuilder(stateMachineName, Services);
            buildAction(builder);
            builder.Result.Build();

            StateMachines.Add(builder.Result.Name, builder.Result);
        }

        [DebuggerHidden]
        public void AddStateMachine(string stateMachineName, Type stateMachineType)
        {
            if (StateMachines.ContainsKey(stateMachineName))
            {
                throw new Exception($"State machine '{stateMachineName}' is already registered");
            }

            Services.RegisterStateMachine(stateMachineType);

            var sm = FormatterServices.GetUninitializedObject(stateMachineType) as StateMachine;

            var builder = new TypedStateMachineBuilder(stateMachineName, stateMachineType, Services);
            sm.Build(builder);
            builder.Result.Build();

            StateMachines.Add(builder.Result.Name, builder.Result);
        }

        [DebuggerHidden]
        public void AddStateMachine<TStateMachine>(string stateMachineName)
            where TStateMachine : StateMachine
            => AddStateMachine(stateMachineName, typeof(TStateMachine));

        public void AddGlobalInterceptor(InterceptorFactory interceptorFactory)
            => GlobalInterceptorFactories.Add(interceptorFactory);

        public void AddGlobalInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor
        {
            Services.RegisterInterceptor<TInterceptor>();
            AddGlobalInterceptor(serviceProvider => serviceProvider.GetRequiredService<TInterceptor>());
        }

        public void AddGlobalExceptionHandler(ExceptionHandlerFactory exceptionHandlerFactory)
            => GlobalExceptionHandlerFactories.Add(exceptionHandlerFactory);

        public void AddGlobalExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler
        {
            Services.RegisterExceptionHandler<TExceptionHandler>();
            AddGlobalExceptionHandler(serviceProvider => serviceProvider.GetRequiredService<TExceptionHandler>());
        }

        public void AddGlobalObserver(ObserverFactory observerFactory)
            => GlobalObserverFactories.Add(observerFactory);

        public void AddGlobalObserver<TObserver>()
            where TObserver : class, IStateMachineObserver
        {
            Services.RegisterObserver<TObserver>();
            AddGlobalObserver(serviceProvider => serviceProvider.GetRequiredService<TObserver>());
        }
    }
}
