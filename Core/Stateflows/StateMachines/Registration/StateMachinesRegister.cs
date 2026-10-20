using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common.Classes;
using Stateflows.Common.Initializer;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Builders;
using Stateflows.StateMachine.Registration.Builders;
using Stateflows.StateMachines.Context;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Registration
{
    internal class StateMachinesRegister(StateflowsBuilder stateflowsBuilder) : IStateMachinesRegister, IOwnedRegistration
    {
        public readonly List<StateMachineExceptionHandlerFactoryAsync> GlobalExceptionHandlerFactories = [];

        public readonly List<StateMachineInterceptorFactoryAsync> GlobalInterceptorFactories = [];

        public readonly List<StateMachineObserverFactoryAsync> GlobalObserverFactories = [];

        public readonly Dictionary<string, Graph> StateMachines = [];

        private readonly Dictionary<string, int> CurrentVersions = [];

        private readonly MethodInfo StateMachineTypeAddedAsyncMethod =
            typeof(IStateMachineVisitor).GetMethod(nameof(IStateMachineVisitor.StateMachineTypeAddedAsync));

        public BehaviorClass? OwnerClass { get; set; }
        public BehaviorClass? ParentClass { get; set; }

        private static void RegisterStateMachine(Type stateMachineType, StateMachineBuilder stateMachineBuilder)
        {
            if (stateMachineType == null || !stateMachineType.IsClass ||
                !typeof(IStateMachine).IsAssignableFrom(stateMachineType) || stateMachineType.ContainsGenericParameters)
                throw new StateMachineDefinitionException("State machine CLR type must be a closed class implementing IStateMachine.", stateMachineBuilder.Graph.Class);

            var staticBuildMethod = typeof(StateMachinesRegister).GetMethod(
                nameof(BuildStateMachine), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(stateMachineType);
            try
            {
                staticBuildMethod.Invoke(null, [stateMachineBuilder]);
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }
        }

        private static void BuildStateMachine<TStateMachine>(StateMachineBuilder builder)
            where TStateMachine : class, IStateMachine
            => TStateMachine.Build(builder);

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
            ValidateName(stateMachineName);
            var key = $"{stateMachineName}.{version}";
            var currentKey = $"{stateMachineName}.current";

            if (StateMachines.ContainsKey(key))
            {
                throw new StateMachineDefinitionException($"State machine '{stateMachineName}' with version '{version}' is already registered", new StateMachineClass(stateMachineName));
            }

            var builder = new StateMachineBuilder(stateMachineName, version, stateflowsBuilder, OwnerClass, ParentClass);

            // Assign to local variable to avoid value being overriden when invoking lambda function at a later stage
            var ownerClass = OwnerClass;
            var parentClass = ParentClass;
            
            builder.Graph.VisitingTasks.Add(v =>
            {
                var hasDefaultInstance = BehaviorClassesInitializations.Instance.DefaultInstanceInitializationTokens
                    .Any(token => token.BehaviorClass.Type == StateMachineClass.Type && token.BehaviorClass.Name == stateMachineName);

                return v.StateMachineAddingAsync(stateMachineName, version, hasDefaultInstance);
            });

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
        public void AddStateMachine(string stateMachineName, int version, Type stateMachineType, StateMachineUtilsBuildAction buildAction = null)
        {
            ValidateName(stateMachineName);
            var key = $"{stateMachineName}.{version}";
            var currentKey = $"{stateMachineName}.current";

            if (StateMachines.ContainsKey(key))
            {
                throw new StateMachineDefinitionException($"State machine '{stateMachineName}' with version '{version}' is already registered", new StateMachineClass(stateMachineName));
            }

            var builder = new StateMachineBuilder(stateMachineName, version, stateflowsBuilder, OwnerClass, ParentClass)
            {
                Graph =
                    {
                        StateMachineType = stateMachineType
                    }
            };
            
            // Assign to local variable to avoid value being overriden when invoking lambda function at a later stage
            var ownerClass = OwnerClass;
            var parentClass = ParentClass;
            
            builder.Graph.VisitingTasks.Add(v =>
            {
                var hasDefaultInstance = BehaviorClassesInitializations.Instance.DefaultInstanceInitializationTokens
                    .Any(token => token.BehaviorClass.Type == StateMachineClass.Type && token.BehaviorClass.Name == stateMachineName);

                return v.StateMachineAddingAsync(stateMachineName, version, hasDefaultInstance);
            });
            
            RegisterStateMachine(stateMachineType, builder);
            buildAction?.Invoke(new StateMachineUtilsBuilder(builder.Graph));
            builder.Graph.Build();

            var method = StateMachineTypeAddedAsyncMethod.MakeGenericMethod(stateMachineType);

            builder.Graph.VisitingTasks.AddRange([
                v => v.StateMachineAddedAsync(stateMachineName, version),
                v => (Task)method.Invoke(v, [ stateMachineName, version ])
            ]);

            StateMachines.Add(key, builder.Graph);

            if (IsNewestVersion(stateMachineName, version))
            {
                StateMachines[currentKey] = builder.Graph;
            }
        }

        [DebuggerHidden]
        public void AddStateMachine<TStateMachine>(string stateMachineName = null, int version = 1, StateMachineUtilsBuildAction buildAction = null)
            where TStateMachine : class, IStateMachine
            => AddStateMachine(stateMachineName ?? StateMachine<TStateMachine>.Name, version, typeof(TStateMachine), buildAction);

        private static void ValidateName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("State machine name must not be empty.", nameof(name));
        }

        [DebuggerHidden]
        public void AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => GlobalInterceptorFactories.Add((serviceProvider, context) => Task.FromResult(interceptorFactory(serviceProvider, context)));

        [DebuggerHidden]
        public void AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
            => GlobalInterceptorFactories.Add(interceptorFactoryAsync);

        [DebuggerHidden]
        public void AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor
            => GlobalInterceptorFactories.Add(
                async (serviceProvider, context) =>
                {
                    ContextValues.GlobalValuesHolder.Value = context.Behavior.Values;
                    ContextValues.StateValuesHolder.Value = null;
                    ContextValues.ParentStateValuesHolder.Value = null;
                    ContextValues.SourceStateValuesHolder.Value = null;
                    ContextValues.TargetStateValuesHolder.Value = null;

                    StateMachinesContextHolder.StateContext.Value = null;
                    StateMachinesContextHolder.TransitionContext.Value = null;
                    StateMachinesContextHolder.BehaviorContext.Value = context.Behavior;
                    StateMachinesContextHolder.ParentBehaviorContext.Value = context.TryGetParentBehaviorContext(out var parentBehaviorContext)
                        ? parentBehaviorContext
                        : null;
                    StateMachinesContextHolder.OwnerBehaviorContext.Value = context.TryGetOwnerBehaviorContext(out var ownerBehaviorContext)
                        ? ownerBehaviorContext
                        : null;
                    StateMachinesContextHolder.StateMachineContext.Value = ((BaseContext)context).StateMachine;
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
            => GlobalExceptionHandlerFactories.Add(
                async (serviceProvider, context) =>
                {
                    ContextValues.GlobalValuesHolder.Value = context.Behavior.Values;
                    ContextValues.StateValuesHolder.Value = null;
                    ContextValues.ParentStateValuesHolder.Value = null;
                    ContextValues.SourceStateValuesHolder.Value = null;
                    ContextValues.TargetStateValuesHolder.Value = null;

                    StateMachinesContextHolder.StateContext.Value = null;
                    StateMachinesContextHolder.TransitionContext.Value = null;
                    StateMachinesContextHolder.BehaviorContext.Value = context.Behavior;
                    StateMachinesContextHolder.ParentBehaviorContext.Value = context.TryGetParentBehaviorContext(out var parentBehaviorContext)
                        ? parentBehaviorContext
                        : null;
                    StateMachinesContextHolder.OwnerBehaviorContext.Value = context.TryGetOwnerBehaviorContext(out var ownerBehaviorContext)
                        ? ownerBehaviorContext
                        : null;
                    StateMachinesContextHolder.StateMachineContext.Value = ((BaseContext)context).StateMachine;
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
            => GlobalObserverFactories.Add(
                async (serviceProvider, context) =>
                {
                    ContextValues.GlobalValuesHolder.Value = context.Behavior.Values;
                    ContextValues.StateValuesHolder.Value = null;
                    ContextValues.ParentStateValuesHolder.Value = null;
                    ContextValues.SourceStateValuesHolder.Value = null;
                    ContextValues.TargetStateValuesHolder.Value = null;

                    StateMachinesContextHolder.StateContext.Value = null;
                    StateMachinesContextHolder.TransitionContext.Value = null;
                    StateMachinesContextHolder.BehaviorContext.Value = context.Behavior;
                    StateMachinesContextHolder.ParentBehaviorContext.Value = context.TryGetParentBehaviorContext(out var parentBehaviorContext)
                        ? parentBehaviorContext
                        : null;
                    StateMachinesContextHolder.OwnerBehaviorContext.Value = context.TryGetOwnerBehaviorContext(out var ownerBehaviorContext)
                        ? ownerBehaviorContext
                        : null;
                    StateMachinesContextHolder.StateMachineContext.Value = ((BaseContext)context).StateMachine;
                    StateMachinesContextHolder.ExecutionContext.Value = context;

                    return await StateflowsActivator.CreateModelElementInstanceAsync<TObserver>(serviceProvider, "observer");
                }
            );

        public async Task VisitStateMachinesAsync(IStateMachineVisitor visitor)
        {
            var tasks = StateMachines
                .Where((item, _) => !item.Key.EndsWith(".current") && item.Value.ParentClass is null)
                .Select(item => item.Value)
                .SelectMany(graph => graph.VisitingTasks);

            foreach (var task in tasks)
            {
                await task(visitor);
            }
        }

        public async Task VisitStateMachineAsync(string stateMachineName, int version, IStateMachineVisitor visitor)
        {
            var tasks = StateMachines
                .Where(item => item.Key == $"{stateMachineName}.{version}")
                .Select(item => item.Value)
                .SelectMany(graph => graph.VisitingTasks);

            foreach (var task in tasks)
            {
                await task(visitor);
            }
        }
    }
}
