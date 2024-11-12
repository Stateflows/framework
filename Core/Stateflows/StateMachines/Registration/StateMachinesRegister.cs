using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.Common.Registration.Builders;

namespace Stateflows.StateMachines.Registration
{
    internal class StateMachinesRegister
    {
        private readonly StateflowsBuilder stateflowsBuilder;

        private IServiceCollection Services { get; }

        public List<StateMachineExceptionHandlerFactory> GlobalExceptionHandlerFactories { get; set; } = new List<StateMachineExceptionHandlerFactory>();

        public List<StateMachineInterceptorFactory> GlobalInterceptorFactories { get; set; } = new List<StateMachineInterceptorFactory>();

        public List<StateMachineObserverFactory> GlobalObserverFactories { get; set; } = new List<StateMachineObserverFactory>();

        public StateMachinesRegister(StateflowsBuilder stateflowsBuilder, IServiceCollection services)
        {
            this.stateflowsBuilder = stateflowsBuilder;
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

            var builder = new StateMachineBuilderBuilder(stateMachineName, version, stateflowsBuilder, Services);
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

            Services.AddServiceType(stateMachineType);

            var sm = FormatterServices.GetUninitializedObject(stateMachineType) as IStateMachine;

            var builder = new StateMachineBuilderBuilder(stateMachineName, version, stateflowsBuilder, Services);
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
            where TStateMachine : IStateMachine
            => AddStateMachine(stateMachineName, version, typeof(TStateMachine));

        [DebuggerHidden]
        public void AddGlobalInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => GlobalInterceptorFactories.Add(interceptorFactory);

        [DebuggerHidden]
        public void AddGlobalInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor
        {
            Services.AddServiceType<TInterceptor>();
            AddGlobalInterceptor(serviceProvider => serviceProvider.GetRequiredService<TInterceptor>());
        }

        [DebuggerHidden]
        public void AddGlobalExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
            => GlobalExceptionHandlerFactories.Add(exceptionHandlerFactory);

        [DebuggerHidden]
        public void AddGlobalExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler
        {
            Services.AddServiceType<TExceptionHandler>();
            AddGlobalExceptionHandler(serviceProvider => serviceProvider.GetRequiredService<TExceptionHandler>());
        }

        [DebuggerHidden]
        public void AddGlobalObserver(StateMachineObserverFactory observerFactory)
            => GlobalObserverFactories.Add(observerFactory);

        [DebuggerHidden]
        public void AddGlobalObserver<TObserver>()
            where TObserver : class, IStateMachineObserver
        {
            Services.AddServiceType<TObserver>();
            AddGlobalObserver(serviceProvider => serviceProvider.GetRequiredService<TObserver>());
        }

        [DebuggerHidden]
        public void Validate(IEnumerable<BehaviorClass> behaviorClasses)
        {
            foreach (var stateMachine in StateMachines.Values)
            {
                stateMachine.Validate(behaviorClasses);
            }
        }
    }
}
