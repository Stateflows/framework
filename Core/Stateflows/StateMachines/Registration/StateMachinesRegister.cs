using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Registration
{
    internal class StateMachinesRegister
    {
        private IServiceCollection Services { get; }

        public List<StateMachineExceptionHandlerFactory> GlobalExceptionHandlerFactories { get; set; } = new List<StateMachineExceptionHandlerFactory>();

        public List<StateMachineInterceptorFactory> GlobalInterceptorFactories { get; set; } = new List<StateMachineInterceptorFactory>();

        public List<StateMachineObserverFactory> GlobalObserverFactories { get; set; } = new List<StateMachineObserverFactory>();

        public StateMachinesRegister(IServiceCollection services)
        {
            Services = services;
        }

        public readonly Dictionary<string, Graph> StateMachines = new Dictionary<string, Graph>();

        public readonly Dictionary<string, int> CurrentVersions = new Dictionary<string, int>();

        private bool IsNewestVersion(string stateMachineName, int version)
        {
            var result = false;

            if (CurrentVersions.TryGetValue(stateMachineName, out var currentVersion))
            {
                if (currentVersion < version)
                {
                    result = true;
                    CurrentVersions[stateMachineName] = version;
                }
            }
            else
            {
                result = true;
                CurrentVersions[stateMachineName] = version;
            }

            return result;
        }

        [DebuggerHidden]
        public void AddStateMachine(string stateMachineName, int version, StateMachineBuildAction buildAction)
        {
            var key = $"{stateMachineName}.{version}";
            var currentKey = $"{stateMachineName}.current";

            if (StateMachines.ContainsKey(key))
            {
                throw new StateMachineDefinitionException($"State machine '{stateMachineName}' with version '{version}' is already registered", new StateMachineClass(stateMachineName));
            }

            var builder = new StateMachineBuilder(stateMachineName, version, Services);
            buildAction(builder);
            builder.Result.Build();

            StateMachines.Add(key, builder.Result);

            if (IsNewestVersion(stateMachineName, version))
            {
                StateMachines[currentKey] = builder.Result;
            }
        }

        [DebuggerHidden]
        public void AddStateMachine(string stateMachineName, int version, Type stateMachineType)
        {
            var key = $"{stateMachineName}.{version}";
            var currentKey = $"{stateMachineName}.current";

            if (StateMachines.ContainsKey(key))
            {
                throw new StateMachineDefinitionException($"State machine '{stateMachineName}' with version '{version}' is already registered", new StateMachineClass(stateMachineName));
            }

            Services.RegisterStateMachine(stateMachineType);

            var sm = FormatterServices.GetUninitializedObject(stateMachineType) as StateMachine;

            var builder = new StateMachineBuilder(stateMachineName, version, Services);
            //builder.AddStateMachineEvents(stateMachineType);
            builder.Result.StateMachineType = stateMachineType;
            sm.Build(builder);
            builder.Result.Build();

            StateMachines.Add(key, builder.Result);

            if (IsNewestVersion(stateMachineName, version))
            {
                StateMachines[currentKey] = builder.Result;
            }
        }

        [DebuggerHidden]
        public void AddStateMachine<TStateMachine>(string stateMachineName, int version = 1)
            where TStateMachine : StateMachine
            => AddStateMachine(stateMachineName, version, typeof(TStateMachine));

        [DebuggerHidden]
        public void AddGlobalInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => GlobalInterceptorFactories.Add(interceptorFactory);

        [DebuggerHidden]
        public void AddGlobalInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor
        {
            Services.RegisterInterceptor<TInterceptor>();
            AddGlobalInterceptor(serviceProvider => serviceProvider.GetRequiredService<TInterceptor>());
        }

        [DebuggerHidden]
        public void AddGlobalExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
            => GlobalExceptionHandlerFactories.Add(exceptionHandlerFactory);

        [DebuggerHidden]
        public void AddGlobalExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler
        {
            Services.RegisterExceptionHandler<TExceptionHandler>();
            AddGlobalExceptionHandler(serviceProvider => serviceProvider.GetRequiredService<TExceptionHandler>());
        }

        [DebuggerHidden]
        public void AddGlobalObserver(StateMachineObserverFactory observerFactory)
            => GlobalObserverFactories.Add(observerFactory);

        [DebuggerHidden]
        public void AddGlobalObserver<TObserver>()
            where TObserver : class, IStateMachineObserver
        {
            Services.RegisterObserver<TObserver>();
            AddGlobalObserver(serviceProvider => serviceProvider.GetRequiredService<TObserver>());
        }
    }
}
