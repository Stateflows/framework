using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Models;
using Stateflows.Common.Registration;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines.Context;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class StateMachineElementsBuilder :
        IInitializedStateMachineElementsBuilder,
        IFinalizedStateMachineBuilder,
        IStateMachineBuilder,
        IFinalizedOverridenStateMachineElementsBuilder,
        IOverridenStateMachineElementsBuilder,
        IBehaviorBuilder,
        IGraphBuilder
    {
        public Graph Graph { get; }

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Graph.Version;

        public StateMachineElementsBuilder(string name, int version, StateflowsBuilder stateflowsBuilder)
        {
            Graph = new Graph(name, version, stateflowsBuilder);
        }

        public IInitializedStateMachineElementsBuilder AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
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

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineEvents<IFinalizedOverridenStateMachineElementsBuilder>.
            AddInitializer<TInitializationEvent>(
                Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            => AddInitializer(actionAsync) as IFinalizedOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineEvents<IFinalizedOverridenStateMachineElementsBuilder>.AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync) as IFinalizedOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineEvents<IFinalizedOverridenStateMachineElementsBuilder>.AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddDefaultInitializer(actionAsync) as IFinalizedOverridenStateMachineElementsBuilder;

        IOverridenStateMachineElementsBuilder IStateMachineEvents<IOverridenStateMachineElementsBuilder>.AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            => AddInitializer(actionAsync) as IOverridenStateMachineElementsBuilder;

        IOverridenStateMachineElementsBuilder IStateMachineEvents<IOverridenStateMachineElementsBuilder>.AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync) as IOverridenStateMachineElementsBuilder;

        IOverridenStateMachineElementsBuilder IStateMachineEvents<IOverridenStateMachineElementsBuilder>.AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddDefaultInitializer(actionAsync) as IOverridenStateMachineElementsBuilder;

        public IInitializedStateMachineElementsBuilder AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
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

        public IInitializedStateMachineElementsBuilder AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
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

        private IInitializedStateMachineElementsBuilder AddVertex(string stateName, VertexType type, Action<Vertex> vertexBuildAction = null)
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
        public IInitializedStateMachineElementsBuilder AddState(string stateName, StateBuildAction stateBuildAction = null)
            => AddVertex(stateName, VertexType.State, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex)));

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineElements<IFinalizedOverridenStateMachineElementsBuilder>.AddCompositeState(
            string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction) as
                IFinalizedOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineElements<IFinalizedOverridenStateMachineElementsBuilder>.AddOrthogonalState(
            string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as
                IFinalizedOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineElements<IFinalizedOverridenStateMachineElementsBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction) as IFinalizedOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineElements<IFinalizedOverridenStateMachineElementsBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction) as IFinalizedOverridenStateMachineElementsBuilder;

        IOverridenStateMachineElementsBuilder IStateMachineElements<IOverridenStateMachineElementsBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction) as IOverridenStateMachineElementsBuilder;

        IOverridenStateMachineElementsBuilder IStateMachineElements<IOverridenStateMachineElementsBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction) as IOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineElements<IFinalizedOverridenStateMachineElementsBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction) as IFinalizedOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineElements<IFinalizedOverridenStateMachineElementsBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction) as IFinalizedOverridenStateMachineElementsBuilder;

        public IInitializedStateMachineElementsBuilder AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddVertex(forkName, VertexType.Fork, vertex => forkBuildAction?.Invoke(new StateBuilder(vertex)));

        public IInitializedStateMachineElementsBuilder AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddVertex(joinName, VertexType.Join, vertex => joinBuildAction?.Invoke(new StateBuilder(vertex)));

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineElements<IFinalizedOverridenStateMachineElementsBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as IFinalizedOverridenStateMachineElementsBuilder;

        IOverridenStateMachineElementsBuilder IStateMachineElements<IOverridenStateMachineElementsBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as IOverridenStateMachineElementsBuilder;

        IOverridenStateMachineElementsBuilder IStateMachineElements<IOverridenStateMachineElementsBuilder>.AddCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction) as IOverridenStateMachineElementsBuilder;

        IOverridenStateMachineElementsBuilder IStateMachineElements<IOverridenStateMachineElementsBuilder>.AddOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IOverridenStateMachineElementsBuilder;

        IOverridenStateMachineElementsBuilder IStateMachineElements<IOverridenStateMachineElementsBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction) as IOverridenStateMachineElementsBuilder;

        IOverridenStateMachineElementsBuilder IStateMachineElements<IOverridenStateMachineElementsBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
             => AddChoice(choiceName, choiceBuildAction) as IOverridenStateMachineElementsBuilder;

        [DebuggerHidden]
        public IFinalizedStateMachineBuilder AddFinalState(string finalStateName = null)
            => AddVertex(finalStateName ?? State<FinalState>.Name, VertexType.FinalState) as IFinalizedStateMachineBuilder;

        [DebuggerHidden]
        public IInitializedStateMachineElementsBuilder AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddVertex(junctionName, VertexType.Junction, vertex => junctionBuildAction?.Invoke(new StateBuilder(vertex)));

        [DebuggerHidden]
        public IInitializedStateMachineElementsBuilder AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddVertex(choiceName, VertexType.Choice, vertex => choiceBuildAction?.Invoke(new StateBuilder(vertex)));

        #region AddCompositeState

        public IInitializedStateMachineElementsBuilder AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddVertex(compositeStateName, VertexType.CompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion)));

        public IInitializedStateMachineElementsBuilder AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddVertex(orthogonalStateName, VertexType.OrthogonalState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex)));

        public IInitializedStateMachineElementsBuilder AddInitialState(string stateName, StateBuildAction stateBuildAction = null)
        {
            stateName ??= InitialState.Name;
            Graph.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialState, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex)));
        }

        public IInitializedStateMachineElementsBuilder AddInitialCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
        {
            Graph.InitialVertexName = compositeStateName;
            return AddVertex(compositeStateName, VertexType.InitialCompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion)));
        }

        public IInitializedStateMachineElementsBuilder AddInitialOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            Graph.InitialVertexName = orthogonalStateName;
            return AddVertex(orthogonalStateName, VertexType.InitialOrthogonalState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex)));
        }
        #endregion

        #region Observability

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineUtils<IFinalizedOverridenStateMachineElementsBuilder>.
            AddObserver<TObserver>()
            => AddObserver<TObserver>() as IFinalizedOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineUtils<IFinalizedOverridenStateMachineElementsBuilder>.AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(exceptionHandlerFactory) as IFinalizedOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineUtils<IFinalizedOverridenStateMachineElementsBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IFinalizedOverridenStateMachineElementsBuilder;

        IOverridenStateMachineElementsBuilder IStateMachineUtils<IOverridenStateMachineElementsBuilder>.AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(exceptionHandlerFactory) as IOverridenStateMachineElementsBuilder;

        IOverridenStateMachineElementsBuilder IStateMachineUtils<IOverridenStateMachineElementsBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IOverridenStateMachineElementsBuilder;

        public IInitializedStateMachineElementsBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler
        {
           
            AddExceptionHandler(async (serviceProvider, context) =>
            {
                ContextValues.GlobalValuesHolder.Value = context.Behavior.Values;
                ContextValues.StateValuesHolder.Value = null;
                ContextValues.ParentStateValuesHolder.Value = null;
                ContextValues.SourceStateValuesHolder.Value = null;
                ContextValues.TargetStateValuesHolder.Value = null;

                StateMachinesContextHolder.StateContext.Value = null;
                StateMachinesContextHolder.TransitionContext.Value = null;
                StateMachinesContextHolder.BehaviorContext.Value = context.Behavior;
                if (((IStateflowsContextProvider)context).Context.ContextOwnerId == null)
                {
                    StateMachinesContextHolder.StateMachineContext.Value = ((BaseContext)context).StateMachine;
                }
                StateMachinesContextHolder.ExecutionContext.Value = context;
                
                return await StateflowsActivator.CreateModelElementInstanceAsync<TExceptionHandler>(serviceProvider, "exception handler");
            });

            return this;
        }

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineUtils<IFinalizedOverridenStateMachineElementsBuilder>.AddObserver(
            StateMachineObserverFactory observerFactory)
            => AddObserver(observerFactory) as IFinalizedOverridenStateMachineElementsBuilder;

        IOverridenStateMachineElementsBuilder IStateMachineUtils<IOverridenStateMachineElementsBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IOverridenStateMachineElementsBuilder;

        public IInitializedStateMachineElementsBuilder AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler((serviceProvider, context) => Task.FromResult(exceptionHandlerFactory(serviceProvider, context)));

        public IInitializedStateMachineElementsBuilder AddExceptionHandler(StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
        {
            Graph.ExceptionHandlerFactories.Add(exceptionHandlerFactoryAsync);

            return this;
        }
        
        IOverridenStateMachineElementsBuilder IStateMachineUtils<IOverridenStateMachineElementsBuilder>.AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => AddInterceptor(interceptorFactory) as IOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineUtils<IFinalizedOverridenStateMachineElementsBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IFinalizedOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineUtils<IFinalizedOverridenStateMachineElementsBuilder>.AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => AddInterceptor(interceptorFactory) as IFinalizedOverridenStateMachineElementsBuilder;

        IOverridenStateMachineElementsBuilder IStateMachineUtils<IOverridenStateMachineElementsBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IOverridenStateMachineElementsBuilder;

        IOverridenStateMachineElementsBuilder IStateMachineUtils<IOverridenStateMachineElementsBuilder>.AddObserver(StateMachineObserverFactory observerFactory)
            => AddObserver(observerFactory) as IOverridenStateMachineElementsBuilder;

        public IInitializedStateMachineElementsBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor
        {
            AddInterceptor(async (serviceProvider, context) =>
            {
                ContextValues.GlobalValuesHolder.Value = context.Behavior.Values;
                ContextValues.StateValuesHolder.Value = null;
                ContextValues.ParentStateValuesHolder.Value = null;
                ContextValues.SourceStateValuesHolder.Value = null;
                ContextValues.TargetStateValuesHolder.Value = null;

                StateMachinesContextHolder.StateContext.Value = null;
                StateMachinesContextHolder.TransitionContext.Value = null;
                StateMachinesContextHolder.BehaviorContext.Value = context.Behavior;
                if (((IStateflowsContextProvider)context).Context.ContextOwnerId == null)
                {
                    StateMachinesContextHolder.StateMachineContext.Value = ((BaseContext)context).StateMachine;
                }
                StateMachinesContextHolder.ExecutionContext.Value = context;

                return await StateflowsActivator.CreateModelElementInstanceAsync<TInterceptor>(serviceProvider, "interceptor");
            });

            return this;
        }

        public IInitializedStateMachineElementsBuilder AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => AddInterceptor((serviceProvider, context) => Task.FromResult(interceptorFactory(serviceProvider, context)));
        
        public IInitializedStateMachineElementsBuilder AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
        {
            Graph.InterceptorFactories.Add(interceptorFactoryAsync);

            return this;
        }

        public IInitializedStateMachineElementsBuilder AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver
        {
            AddObserver(async (serviceProvider, context) =>
            {
                ContextValues.GlobalValuesHolder.Value = context.Behavior.Values;
                ContextValues.StateValuesHolder.Value = null;
                ContextValues.ParentStateValuesHolder.Value = null;
                ContextValues.SourceStateValuesHolder.Value = null;
                ContextValues.TargetStateValuesHolder.Value = null;

                StateMachinesContextHolder.StateContext.Value = null;
                StateMachinesContextHolder.TransitionContext.Value = null;
                StateMachinesContextHolder.BehaviorContext.Value = context.Behavior;
                if (((IStateflowsContextProvider)context).Context.ContextOwnerId == null)
                {
                    StateMachinesContextHolder.StateMachineContext.Value = ((BaseContext)context).StateMachine;
                }
                StateMachinesContextHolder.ExecutionContext.Value = context;

                return await StateflowsActivator.CreateModelElementInstanceAsync<TObserver>(serviceProvider, "observer");
            });

            return this;
        }

        public IInitializedStateMachineElementsBuilder AddObserver(StateMachineObserverFactory observerFactory)
            => AddObserver((serviceProvider, context) => Task.FromResult(observerFactory(serviceProvider, context)));
        
        public IInitializedStateMachineElementsBuilder AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync)
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

        public IOverridenStateMachineElementsBuilder UseStateMachine<TStateMachine>(OverridenStateMachineBuildAction buildAction)
            where TStateMachine : class, IStateMachine
        {
            Graph.BaseStateMachineName = StateMachine<TStateMachine>.Name;
            TStateMachine.Build(this);
            // var sm = StateflowsActivator.CreateUninitializedInstance(typeof(TStateMachine)) as IStateMachine;
            // sm.Build(this);
            
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

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineFinal<IFinalizedOverridenStateMachineElementsBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName) as IFinalizedOverridenStateMachineElementsBuilder;

        public IOverridenStateMachineElementsBuilder UseState(string stateName, OverridenStateBuildAction stateBuildAction)
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
            
            stateBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineElementsBuilder>.
            UseCompositeState(string compositeStateName,
                OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction) as
                IFinalizedOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineElementsBuilder>.UseOrthogonalState(string orthogonalStateName,
            OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IFinalizedOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineElementsBuilder>.
            UseJunction(string junctionName,
                OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction) as IFinalizedOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineElementsBuilder>.UseChoice(
            string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction) as IFinalizedOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineElementsBuilder>.UseFork(
            string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction) as IFinalizedOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineElementsBuilder>.UseJoin(
            string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction) as IFinalizedOverridenStateMachineElementsBuilder;

        IFinalizedOverridenStateMachineElementsBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineElementsBuilder>.UseState(
            string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction) as IFinalizedOverridenStateMachineElementsBuilder;

        public IOverridenStateMachineElementsBuilder UseCompositeState(string compositeStateName,
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

            compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion));

            return this;
        }

        public IOverridenStateMachineElementsBuilder UseOrthogonalState(string orthogonalStateName,
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

            orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex));

            return this;
        }

        public IOverridenStateMachineElementsBuilder UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
        {
            if (!Graph.Vertices.TryGetValue(junctionName, out var vertex) || vertex.Type != VertexType.Junction || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Junction '{junctionName}' not found in overriden state machine '{Graph.BaseStateMachineName}'", Graph.Class);
            }
            
            junctionBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        public IOverridenStateMachineElementsBuilder UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
        {
            if (!Graph.Vertices.TryGetValue(choiceName, out var vertex) || vertex.Type != VertexType.Choice || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Choice '{choiceName}' not found in overriden state machine '{Graph.BaseStateMachineName}'", Graph.Class);
            }
            
            choiceBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        public IOverridenStateMachineElementsBuilder UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
        {
            if (!Graph.Vertices.TryGetValue(forkName, out var vertex) || vertex.Type != VertexType.Fork || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Fork '{forkName}' not found in overriden state machine '{Graph.BaseStateMachineName}'", Graph.Class);
            }
            
            forkBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        public IOverridenStateMachineElementsBuilder UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
        {
            if (!Graph.Vertices.TryGetValue(joinName, out var vertex) || vertex.Type != VertexType.Join || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Join '{joinName}' not found in overriden state machine '{Graph.BaseStateMachineName}'", Graph.Class);
            }
            
            joinBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }
    }
}

