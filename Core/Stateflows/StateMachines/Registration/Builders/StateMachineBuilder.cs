﻿using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.Common.Extensions;
using Stateflows.Common.Registration;
using Stateflows.Common.Registration.Builders;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class StateMachineBuilderBuilder :
        IInitializedStateMachineBuilder,
        IFinalizedStateMachineBuilder,
        IStateMachineBuilder,
        IFinalizedOverridenStateMachineBuilder,
        IOverridenStateMachineBuilder,
        IInternal,
        IBehaviorBuilder
    {
        public Graph Result { get; }

        public IServiceCollection Services { get; }

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Result.Name);

        int IBehaviorBuilder.BehaviorVersion => Result.Version;

        public StateMachineBuilderBuilder(string name, int version, StateflowsBuilder stateflowsBuilder, IServiceCollection services)
        {
            Services = services;
            Result = new Graph(name, version, stateflowsBuilder);
        }

        public IInitializedStateMachineBuilder AddInitializer(Type initializerType, string initializerName, StateMachinePredicateAsync initializerAction)
        {
            if (!Result.Initializers.TryGetValue(initializerName, out var initializer))
            {
                initializer = new Logic<StateMachinePredicateAsync>(Constants.Initialize);

                Result.Initializers.Add(initializerName, initializer);
                Result.InitializerTypes.Add(initializerType);
            }

            initializer.Actions.Add(initializerAction);

            return this;
        }

        public IInitializedStateMachineBuilder AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
        {
            Result.DefaultInitializer = new Logic<StateMachinePredicateAsync>(Constants.Initialize);

            Result.DefaultInitializer.Actions.Add(c =>
            {
                var context = new StateMachineInitializationContext(c);
                return actionAsync(context);
            });

            return this;
        }

        IFinalizedOverridenStateMachineBuilder IStateMachineEvents<IFinalizedOverridenStateMachineBuilder>.
            AddInitializer<TInitializationEvent>(
                Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            => AddInitializer(actionAsync) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineEvents<IFinalizedOverridenStateMachineBuilder>.AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineEvents<IFinalizedOverridenStateMachineBuilder>.AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddDefaultInitializer(actionAsync) as IFinalizedOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineEvents<IOverridenStateMachineBuilder>.AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            => AddInitializer(actionAsync) as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineEvents<IOverridenStateMachineBuilder>.AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync) as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineEvents<IOverridenStateMachineBuilder>.AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddDefaultInitializer(actionAsync) as IOverridenStateMachineBuilder;

        public IInitializedStateMachineBuilder AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));
            
            var initializerName = Event<TInitializationEvent>.Name;

            return AddInitializer(typeof(TInitializationEvent), initializerName, async c =>
            {
                var result = false;
                var context = new StateMachineInitializationContext<TInitializationEvent>(c, c.EventHolder as EventHolder<TInitializationEvent>);

                result = await actionAsync(context);

                return result;
            });
        }

        public IInitializedStateMachineBuilder AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            Result.Finalize.Actions.Add(async c =>
            {
                var context = new StateMachineActionContext(c);

                await actionAsync(context);
            });

            return this;
        }

        private IInitializedStateMachineBuilder AddVertex(string stateName, VertexType type, Action<Vertex> vertexBuildAction = null)
        {
            stateName.ThrowIfNullOrEmpty(nameof(stateName));

            if (Result.Vertices.ContainsKey(stateName))
                throw new StateDefinitionException(stateName, $"State '{stateName}' is already registered", Result.Class);

            var vertex = new Vertex()
            {
                Name = stateName,
                Type = type,
                Graph = Result,
            };

            vertexBuildAction?.Invoke(vertex);

            Result.Vertices.Add(vertex.Name, vertex);
            Result.AllVertices.Add(vertex.Identifier, vertex);

            return this;
        }

        [DebuggerHidden]
        public IInitializedStateMachineBuilder AddState(string stateName, StateBuildAction stateBuildAction = null)
            => AddVertex(stateName, VertexType.State, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));

        IFinalizedOverridenStateMachineBuilder IStateMachine<IFinalizedOverridenStateMachineBuilder>.AddCompositeState(
            string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction) as
                IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachine<IFinalizedOverridenStateMachineBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachine<IFinalizedOverridenStateMachineBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachine<IFinalizedOverridenStateMachineBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachine<IOverridenStateMachineBuilder>.AddCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction) as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachine<IOverridenStateMachineBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction) as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachine<IOverridenStateMachineBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
             => AddChoice(choiceName, choiceBuildAction) as IOverridenStateMachineBuilder;

        [DebuggerHidden]
        public IFinalizedStateMachineBuilder AddFinalState(string finalStateName = FinalState.Name)
            => AddVertex(finalStateName, VertexType.FinalState) as IFinalizedStateMachineBuilder;

        [DebuggerHidden]
        public IInitializedStateMachineBuilder AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddVertex(junctionName, VertexType.Junction, vertex => junctionBuildAction?.Invoke(new StateBuilder(vertex, Services)));

        [DebuggerHidden]
        public IInitializedStateMachineBuilder AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddVertex(choiceName, VertexType.Choice, vertex => choiceBuildAction?.Invoke(new StateBuilder(vertex, Services)));

        #region AddCompositeState

        IOverridenStateMachineBuilder IStateMachine<IOverridenStateMachineBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as IOverridenStateMachineBuilder;

        public IInitializedStateMachineBuilder AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddVertex(compositeStateName, VertexType.CompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex, Services)));

        public IInitializedStateMachineBuilder AddInitialState(string stateName, StateBuildAction stateBuildAction = null)
        {
            Result.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialState, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));
        }

        public IInitializedStateMachineBuilder AddInitialCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
        {
            Result.InitialVertexName = compositeStateName;
            return AddVertex(compositeStateName, VertexType.InitialCompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex, Services)));
        }
        #endregion

        #region Observability

        IFinalizedOverridenStateMachineBuilder IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>.
            AddObserver<TObserver>()
            => AddObserver<TObserver>() as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>.AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(exceptionHandlerFactory) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IFinalizedOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineUtils<IOverridenStateMachineBuilder>.AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(exceptionHandlerFactory) as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineUtils<IOverridenStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IOverridenStateMachineBuilder;

        public IInitializedStateMachineBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler
        {
            Services.AddServiceType<TExceptionHandler>();
            AddExceptionHandler(serviceProvider => serviceProvider.GetRequiredService<TExceptionHandler>());

            return this;
        }

        IFinalizedOverridenStateMachineBuilder IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>.AddObserver(
            StateMachineObserverFactory observerFactory)
            => AddObserver(observerFactory) as IFinalizedOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineUtils<IOverridenStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IOverridenStateMachineBuilder;

        public IInitializedStateMachineBuilder AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
        {
            Result.ExceptionHandlerFactories.Add(exceptionHandlerFactory);

            return this;
        }

        IOverridenStateMachineBuilder IStateMachineUtils<IOverridenStateMachineBuilder>.AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => AddInterceptor(interceptorFactory) as IOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>.AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => AddInterceptor(interceptorFactory) as IFinalizedOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineUtils<IOverridenStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineUtils<IOverridenStateMachineBuilder>.AddObserver(StateMachineObserverFactory observerFactory)
            => AddObserver(observerFactory) as IOverridenStateMachineBuilder;

        public IInitializedStateMachineBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor
        {
            Services.AddServiceType<TInterceptor>();
            AddInterceptor(serviceProvider => serviceProvider.GetRequiredService<TInterceptor>());

            return this;
        }

        public IInitializedStateMachineBuilder AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
        {
            Result.InterceptorFactories.Add(interceptorFactory);

            return this;
        }

        public IInitializedStateMachineBuilder AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver
        {
            Services.AddServiceType<TObserver>();
            AddObserver(serviceProvider => serviceProvider.GetRequiredService<TObserver>());

            return this;
        }

        public IInitializedStateMachineBuilder AddObserver(StateMachineObserverFactory observerFactory)
        {
            Result.ObserverFactories.Add(observerFactory);

            return this;
        }

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => AddInterceptor(interceptorFactory) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddObserver(StateMachineObserverFactory observerFactory)
            => AddObserver(observerFactory) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(exceptionHandlerFactory) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineEvents<IStateMachineBuilder>.AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddDefaultInitializer(actionAsync) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineEvents<IStateMachineBuilder>.AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            => AddInitializer<TInitializationEvent>(actionAsync) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineEvents<IStateMachineBuilder>.AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync) as IStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => AddInterceptor(interceptorFactory) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddObserver(StateMachineObserverFactory observerFactory)
            => AddObserver(observerFactory) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(exceptionHandlerFactory) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineEvents<IFinalizedStateMachineBuilder>.AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddDefaultInitializer(actionAsync) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineEvents<IFinalizedStateMachineBuilder>.AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            => AddInitializer(actionAsync) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineEvents<IFinalizedStateMachineBuilder>.AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync) as IFinalizedStateMachineBuilder;
        #endregion

        public IOverridenStateMachineBuilder UseStateMachine<TStateMachine>(OverridenStateMachineBuildAction buildAction)
            where TStateMachine : class, IStateMachine
        {
            Result.BaseStateMachineName = StateMachine<TStateMachine>.Name;
            var sm = FormatterServices.GetUninitializedObject(typeof(TStateMachine)) as IStateMachine;
            sm.Build(this);
            
            foreach (var vertex in Result.AllVertices.Values)
            {
                vertex.OriginStateMachineName ??= Result.BaseStateMachineName;
            }
            
            foreach (var edge in Result.AllEdges)
            {
                edge.OriginStateMachineName ??= Result.BaseStateMachineName;
            }
            
            buildAction?.Invoke(this);

            return this;
        }

        IFinalizedOverridenStateMachineBuilder IStateMachineFinal<IFinalizedOverridenStateMachineBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName) as IFinalizedOverridenStateMachineBuilder;

        public IOverridenStateMachineBuilder UseState(string stateName, OverridenStateBuildAction stateBuildAction)
        {
            if (
                !Result.Vertices.TryGetValue(stateName, out var vertex) ||
                (
                    vertex.Type != VertexType.State && 
                    vertex.Type != VertexType.InitialState
                ) || 
                vertex.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"State '{stateName}' not found in overriden state machine '{Result.BaseStateMachineName}'", Result.Class);
            }
            
            stateBuildAction?.Invoke(new StateBuilder(vertex, Services));

            return this;
        }

        public IOverridenStateMachineBuilder UseCompositeState(string compositeStateName,
            OverridenCompositeStateBuildAction compositeStateBuildAction)
        {
            if (
                !Result.Vertices.TryGetValue(compositeStateName, out var vertex) ||
                (
                    vertex.Type != VertexType.CompositeState &&
                    vertex.Type != VertexType.InitialCompositeState
                ) ||
                vertex.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"Composite state '{compositeStateName}' not found in overriden state machine '{Result.BaseStateMachineName}'", Result.Class);
            }
            
            compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex, Services));

            return this;
        }

        public IOverridenStateMachineBuilder UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
        {
            if (!Result.Vertices.TryGetValue(junctionName, out var vertex) || vertex.Type != VertexType.Junction || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Junction '{junctionName}' not found in overriden state machine '{Result.BaseStateMachineName}'", Result.Class);
            }
            
            junctionBuildAction?.Invoke(new StateBuilder(vertex, Services));

            return this;
        }

        public IOverridenStateMachineBuilder UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
        {
            if (!Result.Vertices.TryGetValue(choiceName, out var vertex) || vertex.Type != VertexType.Choice || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Choice '{choiceName}' not found in overriden state machine '{Result.BaseStateMachineName}'", Result.Class);
            }
            
            choiceBuildAction?.Invoke(new StateBuilder(vertex, Services));

            return this;
        }
    }
}

