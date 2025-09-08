using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Registration.Builders;
using Stateflows.StateMachines.Context;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration
{
    internal class StateMachinesRegister : IStateMachinesRegister//, IBehaviorRegister
    {
        private readonly StateflowsBuilder stateflowsBuilder;

        public List<StateMachineExceptionHandlerFactoryAsync> GlobalExceptionHandlerFactories { get; set; } = new List<StateMachineExceptionHandlerFactoryAsync>();

        public List<StateMachineInterceptorFactoryAsync> GlobalInterceptorFactories { get; set; } = new List<StateMachineInterceptorFactoryAsync>();

        public List<StateMachineObserverFactoryAsync> GlobalObserverFactories { get; set; } = new List<StateMachineObserverFactoryAsync>();
        
        public StateMachinesRegister(StateflowsBuilder stateflowsBuilder)
        {
            this.stateflowsBuilder = stateflowsBuilder;
            // stateflowsBuilder.Registers.Add(this);
        }

        public readonly Dictionary<string, Graph> StateMachines = new Dictionary<string, Graph>();

        public readonly Dictionary<string, int> CurrentVersions = new Dictionary<string, int>();

        private readonly MethodInfo StateMachineTypeAddedAsyncMethod =
            typeof(IStateMachineVisitor).GetMethod(nameof(IStateMachineVisitor.StateMachineTypeAddedAsync));

        // private readonly List<Action<IServiceProvider>> StateMachineRegistrationActions =
        //     new List<Action<IServiceProvider>>();
        //
        // private bool Built = false;
        // public void Build(IServiceProvider serviceProvider)
        // {
        //     if (Built) return;
        //     
        //     foreach (var action in StateMachineRegistrationActions)
        //     {
        //         action.Invoke(serviceProvider);
        //     }
        //     
        //     Built = true;
        // }

        private static void RegisterStateMachine(Type stateMachineType, StateMachineBuilder stateMachineBuilder)
        {
            // Try to invoke a static RegisterEndpoints(EndpointsBuilder) on the concrete type
            var staticRegister = stateMachineType.GetMethod(
                "Build",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: [ typeof(StateMachineBuilder) ],
                modifiers: null
            );

            // static method found -> invoke without creating an instance
            staticRegister.Invoke(null, [ stateMachineBuilder ]);
        }

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
            // Action<IServiceProvider> action = _ => {
                var key = $"{stateMachineName}.{version}";
                var currentKey = $"{stateMachineName}.current";

                if (StateMachines.ContainsKey(key))
                {
                    throw new StateMachineDefinitionException($"State machine '{stateMachineName}' with version '{version}' is already registered", new StateMachineClass(stateMachineName));
                }

                var builder = new StateMachineBuilder(stateMachineName, version, stateflowsBuilder);
                buildAction(builder);
                builder.Graph.Build();
                
                builder.Graph.VisitingTasks.Add(v => v.StateMachineAddedAsync(stateMachineName, version));

                StateMachines.Add(key, builder.Graph);

                if (IsNewestVersion(stateMachineName, version))
                {
                    StateMachines[currentKey] = builder.Graph;
                }
            // };
            //
            // if (Built)
            // {
            //     action(ServiceProvider);
            // }
            // else
            // {
            //     StateMachineRegistrationActions.Add(action);
            // }
        }

        [DebuggerHidden]
        public void AddStateMachine(string stateMachineName, int version, Type stateMachineType)
        {
            // Action<IServiceProvider> action = serviceProvider => {
                var key = $"{stateMachineName}.{version}";
                var currentKey = $"{stateMachineName}.current";

                if (StateMachines.ContainsKey(key))
                {
                    throw new StateMachineDefinitionException($"State machine '{stateMachineName}' with version '{version}' is already registered", new StateMachineClass(stateMachineName));
                }

                // var sm = StateflowsActivator.CreateModelElementInstanceAsync(serviceProvider, stateMachineType).GetAwaiter().GetResult() as IStateMachine;
                // var sm = StateflowsActivator.CreateUninitializedInstance(stateMachineType) as IStateMachine;

                var stateMachineBuilder = new StateMachineBuilder(stateMachineName, version, stateflowsBuilder)
                    {
                        Graph =
                        {
                            StateMachineType = stateMachineType
                        }
                    };
                RegisterStateMachine(stateMachineType, stateMachineBuilder);
                // sm.Build(builder);
                stateMachineBuilder.Graph.Build();

                var method = StateMachineTypeAddedAsyncMethod.MakeGenericMethod(stateMachineType);

                stateMachineBuilder.Graph.VisitingTasks.AddRange(new Func<IStateMachineVisitor, Task>[] {
                    v => v.StateMachineAddedAsync(stateMachineName, version),
                    v => (Task)method.Invoke(v, new object[] { stateMachineName, version })
                });
                
                StateMachines.Add(key, stateMachineBuilder.Graph);

                if (IsNewestVersion(stateMachineName, version))
                {
                    StateMachines[currentKey] = stateMachineBuilder.Graph;
                }
            // };
            //
            // if (Built)
            // {
            //     action(ServiceProvider);
            // }
            // else
            // {
            //     StateMachineRegistrationActions.Add(action);
            // }
        }

        [DebuggerHidden]
        public void AddStateMachine<TStateMachine>(string stateMachineName = null, int version = 1)
            where TStateMachine : class, IStateMachine
            => AddStateMachine(stateMachineName ?? StateMachine<TStateMachine>.Name, version, typeof(TStateMachine));

        [DebuggerHidden]
        public void AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => GlobalInterceptorFactories.Add((serviceProvider, context) => Task.FromResult(interceptorFactory(serviceProvider, context)));
        
        [DebuggerHidden]
        public void AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
            => GlobalInterceptorFactories.Add(interceptorFactoryAsync);

        [DebuggerHidden]
        public void AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor
            =>  GlobalInterceptorFactories.Add(
                async (serviceProvider, context) => {
                    ContextValues.GlobalValuesHolder.Value = context.Behavior.Values;
                    ContextValues.StateValuesHolder.Value = null;
                    ContextValues.ParentStateValuesHolder.Value = null;
                    ContextValues.SourceStateValuesHolder.Value = null;
                    ContextValues.TargetStateValuesHolder.Value = null;
    
                    StateMachinesContextHolder.StateContext.Value = null;
                    StateMachinesContextHolder.TransitionContext.Value = null;
                    StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
                    StateMachinesContextHolder.BehaviorContext.Value = context.Behavior;
                    StateMachinesContextHolder.ExecutionContext.Value = context;
                    
                    return await StateflowsActivator.CreateModelElementInstanceAsync<TInterceptor>(serviceProvider, "interceptor");
                }
            );

        [DebuggerHidden]
        public void AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
            => GlobalExceptionHandlerFactories.Add((serviceProvider, context) => Task.FromResult(exceptionHandlerFactory(serviceProvider, context)));
        
        [DebuggerHidden]
        public void AddExceptionHandler(StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
            => GlobalExceptionHandlerFactories.Add(exceptionHandlerFactoryAsync);

        [DebuggerHidden]
        public void AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler
            =>  GlobalExceptionHandlerFactories.Add(
                async (serviceProvider, context) => {
                    ContextValues.GlobalValuesHolder.Value = context.Behavior.Values;
                    ContextValues.StateValuesHolder.Value = null;
                    ContextValues.ParentStateValuesHolder.Value = null;
                    ContextValues.SourceStateValuesHolder.Value = null;
                    ContextValues.TargetStateValuesHolder.Value = null;
    
                    StateMachinesContextHolder.StateContext.Value = null;
                    StateMachinesContextHolder.TransitionContext.Value = null;
                    StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
                    StateMachinesContextHolder.BehaviorContext.Value = context.Behavior;
                    StateMachinesContextHolder.ExecutionContext.Value = context;
                    
                    return await StateflowsActivator.CreateModelElementInstanceAsync<TExceptionHandler>(serviceProvider, "exception handler");
                }
            );

        [DebuggerHidden]
        public void AddObserver(StateMachineObserverFactory observerFactory)
            => GlobalObserverFactories.Add((serviceProvider, context) => Task.FromResult(observerFactory(serviceProvider, context)));
        
        [DebuggerHidden]
        public void AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync)
            => GlobalObserverFactories.Add(observerFactoryAsync);

        [DebuggerHidden]
        public void AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver
            =>  GlobalObserverFactories.Add(
                async (serviceProvider, context) => {
                    ContextValues.GlobalValuesHolder.Value = context.Behavior.Values;
                    ContextValues.StateValuesHolder.Value = null;
                    ContextValues.ParentStateValuesHolder.Value = null;
                    ContextValues.SourceStateValuesHolder.Value = null;
                    ContextValues.TargetStateValuesHolder.Value = null;
    
                    StateMachinesContextHolder.StateContext.Value = null;
                    StateMachinesContextHolder.TransitionContext.Value = null;
                    StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
                    StateMachinesContextHolder.BehaviorContext.Value = context.Behavior;
                    StateMachinesContextHolder.ExecutionContext.Value = context;
                    
                    return await StateflowsActivator.CreateModelElementInstanceAsync<TObserver>(serviceProvider, "observer");
                }
            );

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
