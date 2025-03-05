using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Classes;
using Stateflows.Common.Registration.Builders;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Registration
{
    internal class StateMachinesRegister : IStateMachinesRegister
    {
        private readonly StateflowsBuilder stateflowsBuilder;

        private IServiceCollection Services { get; }

        public List<StateMachineExceptionHandlerFactoryAsync> GlobalExceptionHandlerFactories { get; set; } = new List<StateMachineExceptionHandlerFactoryAsync>();

        public List<StateMachineInterceptorFactoryAsync> GlobalInterceptorFactories { get; set; } = new List<StateMachineInterceptorFactoryAsync>();

        public List<StateMachineObserverFactoryAsync> GlobalObserverFactories { get; set; } = new List<StateMachineObserverFactoryAsync>();
        
        public StateMachinesRegister(StateflowsBuilder stateflowsBuilder, IServiceCollection services)
        {
            this.stateflowsBuilder = stateflowsBuilder;
            Services = services;
        }

        public readonly Dictionary<string, Graph> StateMachines = new Dictionary<string, Graph>();

        public readonly Dictionary<string, int> CurrentVersions = new Dictionary<string, int>();

        private readonly MethodInfo StateMachineTypeAddedAsyncMethod =
            typeof(IStateMachineVisitor).GetMethod(nameof(IStateMachineVisitor.StateMachineTypeAddedAsync));

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

            var builder = new StateMachineBuilder(stateMachineName, version, stateflowsBuilder, Services);
            buildAction(builder);
            builder.Graph.Build();
            
            builder.Graph.VisitingTasks.Add(v => v.StateMachineAddedAsync(stateMachineName, version));

            StateMachines.Add(key, builder.Graph);

            if (IsNewestVersion(stateMachineName, version))
            {
                StateMachines[currentKey] = builder.Graph;
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

            var sm = StateflowsActivator.CreateUninitializedInstance(stateMachineType) as IStateMachine;

            var builder = new StateMachineBuilder(stateMachineName, version, stateflowsBuilder, Services)
                {
                    Graph =
                    {
                        StateMachineType = stateMachineType
                    }
                };
            sm.Build(builder);
            builder.Graph.Build();

            var method = StateMachineTypeAddedAsyncMethod.MakeGenericMethod(stateMachineType);

            builder.Graph.VisitingTasks.AddRange(new Func<IStateMachineVisitor, Task>[] {
                v => v.StateMachineAddedAsync(stateMachineName, version),
                v => (Task)method.Invoke(v, new object[] { stateMachineName, version })
            });
            
            StateMachines.Add(key, builder.Graph);

            if (IsNewestVersion(stateMachineName, version))
            {
                StateMachines[currentKey] = builder.Graph;
            }
        }

        [DebuggerHidden]
        public void AddStateMachine<TStateMachine>(string stateMachineName = null, int version = 1)
            where TStateMachine : class, IStateMachine
            => AddStateMachine(stateMachineName ?? StateMachine<TStateMachine>.Name, version, typeof(TStateMachine));

        [DebuggerHidden]
        public void AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => GlobalInterceptorFactories.Add(serviceProvider => Task.FromResult(interceptorFactory(serviceProvider)));
        
        [DebuggerHidden]
        public void AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
            => GlobalInterceptorFactories.Add(interceptorFactoryAsync);

        [DebuggerHidden]
        public void AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor
            =>  GlobalInterceptorFactories.Add(async serviceProvider => await StateflowsActivator.CreateInstanceAsync<TInterceptor>(serviceProvider, "interceptor"));

        [DebuggerHidden]
        public void AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
            => GlobalExceptionHandlerFactories.Add(serviceProvider => Task.FromResult(exceptionHandlerFactory(serviceProvider)));
        
        [DebuggerHidden]
        public void AddExceptionHandler(StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
            => GlobalExceptionHandlerFactories.Add(exceptionHandlerFactoryAsync);

        [DebuggerHidden]
        public void AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler
            =>  GlobalExceptionHandlerFactories.Add(async serviceProvider => await StateflowsActivator.CreateInstanceAsync<TExceptionHandler>(serviceProvider, "exception handler"));

        [DebuggerHidden]
        public void AddObserver(StateMachineObserverFactory observerFactory)
            => GlobalObserverFactories.Add(serviceProvider => Task.FromResult(observerFactory(serviceProvider)));
        
        [DebuggerHidden]
        public void AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync)
            => GlobalObserverFactories.Add(observerFactoryAsync);

        [DebuggerHidden]
        public void AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver
            =>  GlobalObserverFactories.Add(async serviceProvider => await StateflowsActivator.CreateInstanceAsync<TObserver>(serviceProvider, "observer"));

        public async Task VisitStateMachinesAsync(IStateMachineVisitor visitor)
        {
            var tasks = StateMachines
                .Where((item, index) => !item.Key.EndsWith(".current"))
                .Select(item => item.Value)
                .SelectMany(graph => graph.VisitingTasks);
            
            foreach (var task in tasks)
            {
                await task(visitor);
            }
        }
    }
}
