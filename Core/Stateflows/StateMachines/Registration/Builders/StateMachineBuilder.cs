using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Models;
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
    internal class StateMachineBuilder :
        IInitializedStateMachineBuilder,
        IFinalizedStateMachineBuilder,
        IStateMachineBuilder,
        IFinalizedOverridenStateMachineBuilder,
        IOverridenStateMachineBuilder,
        IInternal,
        IBehaviorBuilder,
        IGraphBuilder
    {
        public Graph Graph { get; }
        
        public IServiceCollection Services { get; }

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Graph.Version;

        public StateMachineBuilder(string name, int version, StateflowsBuilder stateflowsBuilder, IServiceCollection services)
        {
            Services = services;
            Graph = new Graph(name, version, stateflowsBuilder);
        }

        public IInitializedStateMachineBuilder AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
        {
            Graph.DefaultInitializer = new Logic<StateMachinePredicateAsync>(Constants.Initialize);

            Graph.DefaultInitializer.Actions.Add(c =>
            {
                var context = new StateMachineInitializationContext(c);
                return actionAsync(context);
            });
            
            Graph.VisitingTasks.Add(v => v.DefaultInitializerAddedAsync(Graph.Name, Graph.Version));

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
            
            if (!Graph.Initializers.TryGetValue(initializerName, out var initializer))
            {
                initializer = new Logic<StateMachinePredicateAsync>(Constants.Initialize);

                Graph.Initializers.Add(initializerName, initializer);
                Graph.InitializerTypes.Add(typeof(TInitializationEvent));
            }

            initializer.Actions.Add(async c =>
            {
                var result = false;
                var context = new StateMachineInitializationContext<TInitializationEvent>(c, c.EventHolder as EventHolder<TInitializationEvent>);

                result = await actionAsync(context);

                return result;
            });
            
            Graph.VisitingTasks.Add(v => v.InitializerAddedAsync<TInitializationEvent>(Graph.Name, Graph.Version));

            return this;
        }

        public IInitializedStateMachineBuilder AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            Graph.Finalize.Actions.Add(async c =>
            {
                var context = new StateMachineActionContext(c);

                await actionAsync(context);
            });
            
            Graph.VisitingTasks.Add(v => v.FinalizerAddedAsync(Graph.Name, Graph.Version));

            return this;
        }

        private IInitializedStateMachineBuilder AddVertex(string stateName, VertexType type, Action<Vertex> vertexBuildAction = null)
        {
            stateName.ThrowIfNullOrEmpty(nameof(stateName));

            if (Graph.Vertices.ContainsKey(stateName))
                throw new StateDefinitionException(stateName, $"State '{stateName}' is already registered", Graph.Class);

            var vertex = new Vertex()
            {
                Name = stateName,
                Type = type,
                Graph = Graph,
            };

            vertexBuildAction?.Invoke(vertex);

            Graph.Vertices.Add(vertex.Name, vertex);
            Graph.AllVertices.Add(vertex.Identifier, vertex);
            
            Graph.VisitingTasks.Add(visitor => visitor.VertexAddedAsync(Graph.Name, Graph.Version, vertex.Name, vertex.Type));

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

        IFinalizedOverridenStateMachineBuilder IStateMachine<IFinalizedOverridenStateMachineBuilder>.AddOrthogonalState(
            string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as
                IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachine<IFinalizedOverridenStateMachineBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachine<IFinalizedOverridenStateMachineBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachine<IOverridenStateMachineBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction) as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachine<IOverridenStateMachineBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction) as IOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachine<IFinalizedOverridenStateMachineBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachine<IFinalizedOverridenStateMachineBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction) as IFinalizedOverridenStateMachineBuilder;

        public IInitializedStateMachineBuilder AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddVertex(forkName, VertexType.Fork, vertex => forkBuildAction?.Invoke(new StateBuilder(vertex, Services)));

        public IInitializedStateMachineBuilder AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddVertex(joinName, VertexType.Join, vertex => joinBuildAction?.Invoke(new StateBuilder(vertex, Services)));

        IFinalizedOverridenStateMachineBuilder IStateMachine<IFinalizedOverridenStateMachineBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachine<IOverridenStateMachineBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachine<IOverridenStateMachineBuilder>.AddCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction) as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachine<IOverridenStateMachineBuilder>.AddOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IOverridenStateMachineBuilder;

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

        public IInitializedStateMachineBuilder AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddVertex(compositeStateName, VertexType.CompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion, Services)));

        public IInitializedStateMachineBuilder AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddVertex(orthogonalStateName, VertexType.OrthogonalState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex, Services)));

        public IInitializedStateMachineBuilder AddInitialState(string stateName, StateBuildAction stateBuildAction = null)
        {
            Graph.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialState, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));
        }

        public IInitializedStateMachineBuilder AddInitialCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
        {
            Graph.InitialVertexName = compositeStateName;
            return AddVertex(compositeStateName, VertexType.InitialCompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion, Services)));
        }

        public IInitializedStateMachineBuilder AddInitialOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            Graph.InitialVertexName = orthogonalStateName;
            return AddVertex(orthogonalStateName, VertexType.InitialOrthogonalState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex, Services)));
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
            Services.AddScoped<TExceptionHandler>();
            
            AddExceptionHandler(async serviceProvider => await StateflowsActivator.CreateInstanceAsync<TExceptionHandler>(serviceProvider, "exception handler"));

            return this;
        }

        IFinalizedOverridenStateMachineBuilder IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>.AddObserver(
            StateMachineObserverFactory observerFactory)
            => AddObserver(observerFactory) as IFinalizedOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineUtils<IOverridenStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IOverridenStateMachineBuilder;

        public IInitializedStateMachineBuilder AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(serviceProvider => Task.FromResult(exceptionHandlerFactory(serviceProvider)));

        public IInitializedStateMachineBuilder AddExceptionHandler(StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
        {
            Graph.ExceptionHandlerFactories.Add(exceptionHandlerFactoryAsync);

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
            Services.AddScoped<TInterceptor>();

            AddInterceptor(async serviceProvider => await StateflowsActivator.CreateInstanceAsync<TInterceptor>(serviceProvider, "interceptor"));

            return this;
        }

        public IInitializedStateMachineBuilder AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => AddInterceptor(serviceProvider => Task.FromResult(interceptorFactory(serviceProvider)));
        
        public IInitializedStateMachineBuilder AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
        {
            Graph.InterceptorFactories.Add(interceptorFactoryAsync);

            return this;
        }

        public IInitializedStateMachineBuilder AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver
        {
            Services.AddScoped<TObserver>();
            
            AddObserver(async serviceProvider => await StateflowsActivator.CreateInstanceAsync<TObserver>(serviceProvider, "observer"));

            return this;
        }

        public IInitializedStateMachineBuilder AddObserver(StateMachineObserverFactory observerFactory)
            => AddObserver(serviceProvider => Task.FromResult(observerFactory(serviceProvider)));
        
        public IInitializedStateMachineBuilder AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync)
        {
            Graph.ObserverFactories.Add(observerFactoryAsync);

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
            => AddInitializer(actionAsync) as IStateMachineBuilder;

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
            Graph.BaseStateMachineName = StateMachine<TStateMachine>.Name;
            var sm = StateflowsActivator.CreateUninitializedInstance(typeof(TStateMachine)) as IStateMachine;
            sm.Build(this);
            
            foreach (var vertex in Graph.AllVertices.Values)
            {
                vertex.OriginStateMachineName ??= Graph.BaseStateMachineName;
            }
            
            foreach (var edge in Graph.AllEdges)
            {
                edge.OriginStateMachineName ??= Graph.BaseStateMachineName;
            }
            
            buildAction?.Invoke(this);

            return this;
        }

        IFinalizedOverridenStateMachineBuilder IStateMachineFinal<IFinalizedOverridenStateMachineBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName) as IFinalizedOverridenStateMachineBuilder;

        public IOverridenStateMachineBuilder UseState(string stateName, OverridenStateBuildAction stateBuildAction)
        {
            if (
                !Graph.Vertices.TryGetValue(stateName, out var vertex) ||
                (
                    vertex.Type != VertexType.State && 
                    vertex.Type != VertexType.InitialState
                ) || 
                vertex.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"State '{stateName}' not found in overriden state machine '{Graph.BaseStateMachineName}'", Graph.Class);
            }
            
            stateBuildAction?.Invoke(new StateBuilder(vertex, Services));

            return this;
        }

        IFinalizedOverridenStateMachineBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineBuilder>.
            UseCompositeState(string compositeStateName,
                OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction) as
                IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineBuilder>.UseOrthogonalState(string orthogonalStateName,
            OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineBuilder>.
            UseJunction(string junctionName,
                OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineBuilder>.UseChoice(
            string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineBuilder>.UseFork(
            string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineBuilder>.UseJoin(
            string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineBuilder>.UseState(
            string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction) as IFinalizedOverridenStateMachineBuilder;

        public IOverridenStateMachineBuilder UseCompositeState(string compositeStateName,
            OverridenCompositeStateBuildAction compositeStateBuildAction)
        {
            if (
                !Graph.Vertices.TryGetValue(compositeStateName, out var vertex) ||
                (
                    vertex.Type != VertexType.CompositeState &&
                    vertex.Type != VertexType.InitialCompositeState
                ) ||
                vertex.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"Composite state '{compositeStateName}' not found in overriden state machine '{Graph.BaseStateMachineName}'", Graph.Class);
            }

            compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion, Services));

            return this;
        }

        public IOverridenStateMachineBuilder UseOrthogonalState(string orthogonalStateName,
            OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            if (
                !Graph.Vertices.TryGetValue(orthogonalStateName, out var vertex) ||
                (
                    vertex.Type != VertexType.OrthogonalState &&
                    vertex.Type != VertexType.InitialOrthogonalState
                ) ||
                vertex.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"Orthogonal state '{orthogonalStateName}' not found in overriden state machine '{Graph.BaseStateMachineName}'", Graph.Class);
            }

            orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex, Services));

            return this;
        }

        public IOverridenStateMachineBuilder UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
        {
            if (!Graph.Vertices.TryGetValue(junctionName, out var vertex) || vertex.Type != VertexType.Junction || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Junction '{junctionName}' not found in overriden state machine '{Graph.BaseStateMachineName}'", Graph.Class);
            }
            
            junctionBuildAction?.Invoke(new StateBuilder(vertex, Services));

            return this;
        }

        public IOverridenStateMachineBuilder UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
        {
            if (!Graph.Vertices.TryGetValue(choiceName, out var vertex) || vertex.Type != VertexType.Choice || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Choice '{choiceName}' not found in overriden state machine '{Graph.BaseStateMachineName}'", Graph.Class);
            }
            
            choiceBuildAction?.Invoke(new StateBuilder(vertex, Services));

            return this;
        }

        public IOverridenStateMachineBuilder UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
        {
            if (!Graph.Vertices.TryGetValue(forkName, out var vertex) || vertex.Type != VertexType.Fork || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Fork '{forkName}' not found in overriden state machine '{Graph.BaseStateMachineName}'", Graph.Class);
            }
            
            forkBuildAction?.Invoke(new StateBuilder(vertex, Services));

            return this;
        }

        public IOverridenStateMachineBuilder UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
        {
            if (!Graph.Vertices.TryGetValue(joinName, out var vertex) || vertex.Type != VertexType.Join || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Join '{joinName}' not found in overriden state machine '{Graph.BaseStateMachineName}'", Graph.Class);
            }
            
            joinBuildAction?.Invoke(new StateBuilder(vertex, Services));

            return this;
        }
    }
}

