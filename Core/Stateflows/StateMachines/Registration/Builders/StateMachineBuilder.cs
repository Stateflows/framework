using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Models;
using Stateflows.Common.Registration;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Utilities;
using Stateflows.Entities;
using Stateflows.Entities.Registration.Interfaces;
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
    internal class StateMachineBuilder :
        IInitializedStateMachineBuilder,
        IFinalizedStateMachineBuilder,
        IStateMachineBuilder,
        IFinalizedOverridenStateMachineBuilder,
        IOverridenStateMachineBuilder,
        IBehaviorBuilder,
        IInitializedStateMachineWithEntityBuilder,
        IFinalizedStateMachineWithEntityBuilder,
        IStateMachineWithEntityBuilder,
        IOverridenStateMachineWithEntityBuilder,
        IFinalizedOverridenStateMachineWithEntityBuilder,
        IGraphBuilder
    {
        public Graph Graph { get; }

        BehaviorClass IBehaviorBuilder.BehaviorClass => new(Constants.StateMachine, Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Graph.Version;

        public StateMachineBuilder(string name, int version, StateflowsBuilder stateflowsBuilder, BehaviorClass? ownerClass, BehaviorClass? parentClass)
        {
            Graph = new Graph(name, version, stateflowsBuilder, ownerClass, parentClass);
        }

        public IInitializedStateMachineBuilder AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
        {
            Graph.DefaultInitializer ??= new Logic<StateMachinePredicateAsync>(Constants.Initialize);

            Graph.DefaultInitializer.Actions.Add(c =>
            {
                var context = new StateMachineInitializationContext(c);
                return actionAsync(context);
            });
            
            Graph.VisitingTasks.Add(v => v.DefaultInitializerAddedAsync(Graph.Name, Graph.Version));

            return this;
        }

        IFinalizedOverridenStateMachineWithEntityBuilder
            IStateMachineEvents<IFinalizedOverridenStateMachineWithEntityBuilder>.AddInitializer<TInitializationEvent>(
                Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            => AddInitializer(actionAsync) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineEvents<IFinalizedOverridenStateMachineWithEntityBuilder>.AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineEvents<IFinalizedOverridenStateMachineWithEntityBuilder>.AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddDefaultInitializer(actionAsync) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineEvents<IOverridenStateMachineWithEntityBuilder>.AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            => AddInitializer(actionAsync) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineEvents<IOverridenStateMachineWithEntityBuilder>.AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineEvents<IOverridenStateMachineWithEntityBuilder>.AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddDefaultInitializer(actionAsync) as IOverridenStateMachineWithEntityBuilder;

        IStateMachineWithEntityBuilder IStateMachineEvents<IStateMachineWithEntityBuilder>.AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            => AddInitializer(actionAsync) as IStateMachineWithEntityBuilder;

        IStateMachineWithEntityBuilder IStateMachineEvents<IStateMachineWithEntityBuilder>.AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync) as IStateMachineWithEntityBuilder;

        IStateMachineWithEntityBuilder IStateMachineEvents<IStateMachineWithEntityBuilder>.AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddDefaultInitializer(actionAsync) as IStateMachineWithEntityBuilder;

        IFinalizedStateMachineWithEntityBuilder IStateMachineEvents<IFinalizedStateMachineWithEntityBuilder>.AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            => AddInitializer(actionAsync) as IFinalizedStateMachineWithEntityBuilder;

        IFinalizedStateMachineWithEntityBuilder IStateMachineEvents<IFinalizedStateMachineWithEntityBuilder>.AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync) as IFinalizedStateMachineWithEntityBuilder;

        IFinalizedStateMachineWithEntityBuilder IStateMachineEvents<IFinalizedStateMachineWithEntityBuilder>.AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddDefaultInitializer(actionAsync) as IFinalizedStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineEvents<IInitializedStateMachineWithEntityBuilder>.AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            => AddInitializer(actionAsync) as IInitializedStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineEvents<IInitializedStateMachineWithEntityBuilder>.AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync) as IInitializedStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineEvents<IInitializedStateMachineWithEntityBuilder>.AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddDefaultInitializer(actionAsync) as IInitializedStateMachineWithEntityBuilder;

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
            => AddVertex(stateName, VertexType.State, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex)));

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineElements<IFinalizedOverridenStateMachineWithEntityBuilder>.AddCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction) as IFinalizedOverridenStateMachineWithEntityBuilder; 

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineElements<IFinalizedOverridenStateMachineWithEntityBuilder>.AddOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineElements<IFinalizedOverridenStateMachineWithEntityBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineElements<IOverridenStateMachineWithEntityBuilder>.AddCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineElements<IOverridenStateMachineWithEntityBuilder>.AddOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineElements<IOverridenStateMachineWithEntityBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as IOverridenStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineElements<IInitializedStateMachineWithEntityBuilder>.AddCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction) as IInitializedStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineElements<IInitializedStateMachineWithEntityBuilder>.AddOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IInitializedStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineElements<IInitializedStateMachineWithEntityBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as IInitializedStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineElements<IFinalizedOverridenStateMachineBuilder>.AddCompositeState(
            string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction) as
                IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineElements<IFinalizedOverridenStateMachineBuilder>.AddOrthogonalState(
            string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as
                IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineElements<IFinalizedOverridenStateMachineBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineElements<IFinalizedOverridenStateMachineWithEntityBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineElements<IFinalizedOverridenStateMachineWithEntityBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineElements<IFinalizedOverridenStateMachineWithEntityBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineElements<IFinalizedOverridenStateMachineWithEntityBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineElements<IOverridenStateMachineWithEntityBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineElements<IOverridenStateMachineWithEntityBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineElements<IOverridenStateMachineWithEntityBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineElements<IOverridenStateMachineWithEntityBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction) as IOverridenStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineElements<IInitializedStateMachineWithEntityBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction) as IInitializedStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineElements<IInitializedStateMachineWithEntityBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction) as IInitializedStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineElements<IInitializedStateMachineWithEntityBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction) as IInitializedStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineElements<IInitializedStateMachineWithEntityBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction) as IInitializedStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineElements<IFinalizedOverridenStateMachineBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineElements<IOverridenStateMachineBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction) as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineElements<IOverridenStateMachineBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction) as IOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineElements<IFinalizedOverridenStateMachineBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineElements<IFinalizedOverridenStateMachineBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction) as IFinalizedOverridenStateMachineBuilder;

        public IInitializedStateMachineBuilder AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddVertex(forkName, VertexType.Fork, vertex => forkBuildAction?.Invoke(new StateBuilder(vertex)));

        public IInitializedStateMachineBuilder AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddVertex(joinName, VertexType.Join, vertex => joinBuildAction?.Invoke(new StateBuilder(vertex)));

        IFinalizedOverridenStateMachineBuilder IStateMachineElements<IFinalizedOverridenStateMachineBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as IFinalizedOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineElements<IOverridenStateMachineBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineElements<IOverridenStateMachineBuilder>.AddCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction) as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineElements<IOverridenStateMachineBuilder>.AddOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineElements<IOverridenStateMachineBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction) as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineElements<IOverridenStateMachineBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
             => AddChoice(choiceName, choiceBuildAction) as IOverridenStateMachineBuilder;

        [DebuggerHidden]
        public IFinalizedStateMachineBuilder AddFinalState(string finalStateName = null)
            => AddVertex(finalStateName ?? State<FinalState>.Name, VertexType.FinalState) as IFinalizedStateMachineBuilder;

        [DebuggerHidden]
        public IInitializedStateMachineBuilder AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddVertex(junctionName, VertexType.Junction, vertex => junctionBuildAction?.Invoke(new StateBuilder(vertex)));

        [DebuggerHidden]
        public IInitializedStateMachineBuilder AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddVertex(choiceName, VertexType.Choice, vertex => choiceBuildAction?.Invoke(new StateBuilder(vertex)));

        #region AddCompositeState

        public IInitializedStateMachineBuilder AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddVertex(compositeStateName, VertexType.CompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion)));

        public IInitializedStateMachineBuilder AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddVertex(orthogonalStateName, VertexType.OrthogonalState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex)));

        public IInitializedStateMachineBuilder AddInitialState(string stateName, StateBuildAction stateBuildAction = null)
        {
            stateName ??= InitialState.Name;
            Graph.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialState, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex)));
        }

        public IInitializedStateMachineBuilder AddInitialCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
        {
            Graph.InitialVertexName = compositeStateName;
            return AddVertex(compositeStateName, VertexType.InitialCompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion)));
        }

        public IInitializedStateMachineBuilder AddInitialOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            Graph.InitialVertexName = orthogonalStateName;
            return AddVertex(orthogonalStateName, VertexType.InitialOrthogonalState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex)));
        }
        #endregion

        #region Observability

        IFinalizedOverridenStateMachineBuilder IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>.
            AddObserver<TObserver>()
            => AddObserver<TObserver>() as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineUtils<IFinalizedOverridenStateMachineWithEntityBuilder>.AddExceptionHandler(
            StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
            => AddExceptionHandler(exceptionHandlerFactoryAsync) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineUtils<IFinalizedOverridenStateMachineWithEntityBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IFinalizedOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineUtils<IOverridenStateMachineWithEntityBuilder>.AddExceptionHandler(
            StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
            => AddExceptionHandler(exceptionHandlerFactoryAsync) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineUtils<IOverridenStateMachineWithEntityBuilder>.
            AddObserver<TObserver>()
            => AddObserver<TObserver>() as IOverridenStateMachineWithEntityBuilder;

        IStateMachineWithEntityBuilder IStateMachineUtils<IStateMachineWithEntityBuilder>.AddExceptionHandler(
            StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
            => AddExceptionHandler(exceptionHandlerFactoryAsync) as IStateMachineWithEntityBuilder;

        IStateMachineWithEntityBuilder IStateMachineUtils<IStateMachineWithEntityBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IStateMachineWithEntityBuilder;

        IFinalizedStateMachineWithEntityBuilder IStateMachineUtils<IFinalizedStateMachineWithEntityBuilder>.AddExceptionHandler(
            StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
            => AddExceptionHandler(exceptionHandlerFactoryAsync) as IFinalizedStateMachineWithEntityBuilder;

        IFinalizedStateMachineWithEntityBuilder IStateMachineUtils<IFinalizedStateMachineWithEntityBuilder>.
            AddObserver<TObserver>()
            => AddObserver<TObserver>() as IFinalizedStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineUtils<IInitializedStateMachineWithEntityBuilder>.AddExceptionHandler(
            StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
            => AddExceptionHandler(exceptionHandlerFactoryAsync) as IInitializedStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineUtils<IInitializedStateMachineWithEntityBuilder>.
            AddObserver<TObserver>()
            => AddObserver<TObserver>() as IInitializedStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>.AddExceptionHandler(StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
            => AddExceptionHandler(exceptionHandlerFactoryAsync) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineUtils<IFinalizedOverridenStateMachineWithEntityBuilder>.SetResourceName(string resourceName)
            => SetResourceName(resourceName) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder
            IStateMachineUtils<IFinalizedOverridenStateMachineWithEntityBuilder>.
            AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IFinalizedOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineUtils<IOverridenStateMachineWithEntityBuilder>.SetResourceName(string resourceName)
            => SetResourceName(resourceName) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineUtils<IOverridenStateMachineWithEntityBuilder>.
            AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IOverridenStateMachineWithEntityBuilder;

        IStateMachineWithEntityBuilder IStateMachineUtils<IStateMachineWithEntityBuilder>.SetResourceName(string resourceName)
            => SetResourceName(resourceName) as IStateMachineWithEntityBuilder;

        IStateMachineWithEntityBuilder IStateMachineUtils<IStateMachineWithEntityBuilder>.AddExceptionHandler<
            TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IStateMachineWithEntityBuilder;

        IFinalizedStateMachineWithEntityBuilder IStateMachineUtils<IFinalizedStateMachineWithEntityBuilder>.SetResourceName(string resourceName)
            => SetResourceName(resourceName) as IFinalizedStateMachineWithEntityBuilder;

        IFinalizedStateMachineWithEntityBuilder IStateMachineUtils<IFinalizedStateMachineWithEntityBuilder>.
            AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IFinalizedStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineUtils<IInitializedStateMachineWithEntityBuilder>.SetResourceName(string resourceName)
            => SetResourceName(resourceName) as IInitializedStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineUtils<IInitializedStateMachineWithEntityBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IInitializedStateMachineWithEntityBuilder;

        IOverridenStateMachineBuilder IStateMachineUtils<IOverridenStateMachineBuilder>.SetResourceName(string resourceName)
            => SetResourceName(resourceName) as IOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>.SetResourceName(string resourceName)
            => SetResourceName(resourceName) as IFinalizedOverridenStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.SetResourceName(string resourceName)
            => SetResourceName(resourceName) as IFinalizedStateMachineBuilder;

        IInitializedStateMachineBuilder IStateMachineUtils<IInitializedStateMachineBuilder>.SetResourceName(string resourceName)
            => SetResourceName(resourceName) as IInitializedStateMachineBuilder;

        public IStateMachineBuilder SetResourceName(string resourceName)
        {
            Graph.ResourceName = resourceName;

            return this;
        }

        IOverridenStateMachineBuilder IStateMachineUtils<IOverridenStateMachineBuilder>.AddExceptionHandler(StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
            => AddExceptionHandler(exceptionHandlerFactoryAsync) as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineUtils<IOverridenStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IOverridenStateMachineBuilder;

        public IInitializedStateMachineBuilder AddExceptionHandler<TExceptionHandler>()
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
                StateMachinesContextHolder.ParentBehaviorContext.Value = context.TryGetParentBehaviorContext(out var parentBehaviorContext)
                    ? parentBehaviorContext
                    : null;
                StateMachinesContextHolder.OwnerBehaviorContext.Value = context.TryGetOwnerBehaviorContext(out var ownerBehaviorContext)
                    ? ownerBehaviorContext
                    : null;
                StateMachinesContextHolder.StateMachineContext.Value = ((BaseContext)context).StateMachine;
                StateMachinesContextHolder.ExecutionContext.Value = context;
                
                return await StateflowsActivator.CreateModelElementInstanceAsync<TExceptionHandler>(serviceProvider, "exception handler");
            });

            return this;
        }

        IFinalizedStateMachineWithEntityBuilder IStateMachineUtils<IFinalizedStateMachineWithEntityBuilder>.AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
            => AddInterceptor(interceptorFactoryAsync) as IFinalizedStateMachineWithEntityBuilder;

        IStateMachineWithEntityBuilder IStateMachineUtils<IStateMachineWithEntityBuilder>.AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
            => AddInterceptor(interceptorFactoryAsync) as IStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineUtils<IOverridenStateMachineWithEntityBuilder>.AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
            => AddInterceptor(interceptorFactoryAsync) as IOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineUtils<IFinalizedOverridenStateMachineWithEntityBuilder>.AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
            => AddInterceptor(interceptorFactoryAsync) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineUtils<IInitializedStateMachineWithEntityBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IInitializedStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineUtils<IFinalizedOverridenStateMachineWithEntityBuilder>.AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync)
            => AddObserver(observerFactoryAsync) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineUtils<IFinalizedOverridenStateMachineWithEntityBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IFinalizedOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineUtils<IOverridenStateMachineWithEntityBuilder>.AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync)
            => AddObserver(observerFactoryAsync) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineUtils<IOverridenStateMachineWithEntityBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IOverridenStateMachineWithEntityBuilder;

        IStateMachineWithEntityBuilder IStateMachineUtils<IStateMachineWithEntityBuilder>.AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync)
            => AddObserver(observerFactoryAsync) as IStateMachineWithEntityBuilder;

        IStateMachineWithEntityBuilder IStateMachineUtils<IStateMachineWithEntityBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IStateMachineWithEntityBuilder;

        IFinalizedStateMachineWithEntityBuilder IStateMachineUtils<IFinalizedStateMachineWithEntityBuilder>.AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync)
            => AddObserver(observerFactoryAsync) as IFinalizedStateMachineWithEntityBuilder;

        IFinalizedStateMachineWithEntityBuilder IStateMachineUtils<IFinalizedStateMachineWithEntityBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IFinalizedStateMachineWithEntityBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineUtils<IInitializedStateMachineWithEntityBuilder>.AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync)
            => AddObserver(observerFactoryAsync) as IInitializedStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>.AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync)
            => AddObserver(observerFactoryAsync) as IFinalizedOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineUtils<IOverridenStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IOverridenStateMachineBuilder;

        public IInitializedStateMachineBuilder AddExceptionHandler(StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
        {
            Graph.ExceptionHandlerFactories.Add(exceptionHandlerFactoryAsync);

            return this;
        }
        
        IOverridenStateMachineBuilder IStateMachineUtils<IOverridenStateMachineBuilder>.AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
            => AddInterceptor(interceptorFactoryAsync) as IOverridenStateMachineBuilder;

        IInitializedStateMachineWithEntityBuilder IStateMachineUtils<IInitializedStateMachineWithEntityBuilder>.AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
            => AddInterceptor(interceptorFactoryAsync) as IInitializedStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IFinalizedOverridenStateMachineBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineUtils<IFinalizedOverridenStateMachineBuilder>.AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
            => AddInterceptor(interceptorFactoryAsync) as IFinalizedOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineUtils<IOverridenStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IOverridenStateMachineBuilder;

        IOverridenStateMachineBuilder IStateMachineUtils<IOverridenStateMachineBuilder>.AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync)
            => AddObserver(observerFactoryAsync) as IOverridenStateMachineBuilder;

        public IInitializedStateMachineBuilder AddInterceptor<TInterceptor>()
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
                StateMachinesContextHolder.ParentBehaviorContext.Value = context.TryGetParentBehaviorContext(out var parentBehaviorContext)
                    ? parentBehaviorContext
                    : null;
                StateMachinesContextHolder.OwnerBehaviorContext.Value = context.TryGetOwnerBehaviorContext(out var ownerBehaviorContext)
                    ? ownerBehaviorContext
                    : null;
                StateMachinesContextHolder.StateMachineContext.Value = ((BaseContext)context).StateMachine;
                StateMachinesContextHolder.ExecutionContext.Value = context;

                return await StateflowsActivator.CreateModelElementInstanceAsync<TInterceptor>(serviceProvider, "interceptor");
            });

            return this;
        }
        
        public IInitializedStateMachineBuilder AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
        {
            Graph.InterceptorFactories.Add(interceptorFactoryAsync);

            return this;
        }

        public IInitializedStateMachineBuilder AddObserver<TObserver>()
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
                StateMachinesContextHolder.ParentBehaviorContext.Value = context.TryGetParentBehaviorContext(out var parentBehaviorContext)
                    ? parentBehaviorContext
                    : null;
                StateMachinesContextHolder.OwnerBehaviorContext.Value = context.TryGetOwnerBehaviorContext(out var ownerBehaviorContext)
                    ? ownerBehaviorContext
                    : null;
                StateMachinesContextHolder.StateMachineContext.Value = ((BaseContext)context).StateMachine;
                StateMachinesContextHolder.ExecutionContext.Value = context;

                return await StateflowsActivator.CreateModelElementInstanceAsync<TObserver>(serviceProvider, "observer");
            });

            return this;
        }
        
        public IInitializedStateMachineBuilder AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync)
        {
            Graph.ObserverFactories.Add(observerFactoryAsync);

            return this;
        }

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
            => AddInterceptor(interceptorFactoryAsync) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync)
            => AddObserver(observerFactoryAsync) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddExceptionHandler(StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
            => AddExceptionHandler(exceptionHandlerFactoryAsync) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineEvents<IStateMachineBuilder>.AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddDefaultInitializer(actionAsync) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineEvents<IStateMachineBuilder>.AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            => AddInitializer(actionAsync) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineEvents<IStateMachineBuilder>.AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync) as IStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync)
            => AddInterceptor(interceptorFactoryAsync) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync)
            => AddObserver(observerFactoryAsync) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddExceptionHandler(StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
            => AddExceptionHandler(exceptionHandlerFactoryAsync) as IFinalizedStateMachineBuilder;

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
            TStateMachine.Build(this);
            
            foreach (var vertex in Graph.AllVertices.Values)
            {
                vertex.OriginStateMachineName ??= Graph.BaseStateMachineName;

                foreach (var deferral in vertex.Deferrals.Values)
                {
                    deferral.OriginStateMachineName ??= Graph.BaseStateMachineName;
                }
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
            
            stateBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineWithEntityBuilder>.UseCompositeState(string compositeStateName, OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineWithEntityBuilder>.UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineWithEntityBuilder>.UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineWithEntityBuilder>.UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineWithEntityBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineWithEntityBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineWithEntityBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction) as IFinalizedOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineOverrides<IOverridenStateMachineWithEntityBuilder>.UseCompositeState(string compositeStateName, OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineOverrides<IOverridenStateMachineWithEntityBuilder>.UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineOverrides<IOverridenStateMachineWithEntityBuilder>.UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineOverrides<IOverridenStateMachineWithEntityBuilder>.UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineOverrides<IOverridenStateMachineWithEntityBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineOverrides<IOverridenStateMachineWithEntityBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction) as IOverridenStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineOverrides<IOverridenStateMachineWithEntityBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction) as IOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineBuilder IStateMachineOverrides<IFinalizedOverridenStateMachineBuilder>.
            UseCompositeState(string compositeStateName, OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction) as IFinalizedOverridenStateMachineBuilder;

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

            compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion));

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

            orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex));

            return this;
        }

        public IOverridenStateMachineBuilder UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
        {
            if (!Graph.Vertices.TryGetValue(junctionName, out var vertex) || vertex.Type != VertexType.Junction || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Junction '{junctionName}' not found in overriden state machine '{Graph.BaseStateMachineName}'", Graph.Class);
            }
            
            junctionBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        public IOverridenStateMachineBuilder UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
        {
            if (!Graph.Vertices.TryGetValue(choiceName, out var vertex) || vertex.Type != VertexType.Choice || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Choice '{choiceName}' not found in overriden state machine '{Graph.BaseStateMachineName}'", Graph.Class);
            }
            
            choiceBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        public IOverridenStateMachineBuilder UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
        {
            if (!Graph.Vertices.TryGetValue(forkName, out var vertex) || vertex.Type != VertexType.Fork || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Fork '{forkName}' not found in overriden state machine '{Graph.BaseStateMachineName}'", Graph.Class);
            }
            
            forkBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        public IOverridenStateMachineBuilder UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
        {
            if (!Graph.Vertices.TryGetValue(joinName, out var vertex) || vertex.Type != VertexType.Join || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Join '{joinName}' not found in overriden state machine '{Graph.BaseStateMachineName}'", Graph.Class);
            }
            
            joinBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }
        
        [DebuggerHidden]
        private string GetEntityName()
            => $"{Graph.Name}.entity";

        private StateMachineBuilder AddEntity<TTemplate>(EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
        {
            var entityName = GetEntityName();
            Graph.StateflowsBuilder.AddEntities(b => b.AddEntity<TTemplate>(entityName, buildAction), Graph.Class, Graph.Class);
            
            Graph.EntityName = entityName;
            
            return this;
        }

        private StateMachineBuilder AddEntity<TTemplate, TEntity>(EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
        {
            var entityName = GetEntityName();
            Graph.StateflowsBuilder.AddEntities(b => b.AddEntity<TTemplate, TEntity>(entityName, 1, buildAction), Graph.Class, Graph.Class);
            
            Graph.EntityName = entityName;

            return this;
        }

        IInitializedStateMachineWithEntityBuilder IStateMachineEntity<IInitializedStateMachineWithEntityBuilder>.AddEntity<TTemplate>(EntityBuildAction<TTemplate>? buildAction)
            => AddEntity<TTemplate>(buildAction);

        IOverridenStateMachineWithEntityBuilder IStateMachineEntity<IOverridenStateMachineWithEntityBuilder>.AddEntity<TTemplate, TEntity>(EntityBuildAction<TTemplate>? buildAction)
            => AddEntity<TTemplate, TEntity>(buildAction);

        IOverridenStateMachineWithEntityBuilder IStateMachineEntity<IOverridenStateMachineWithEntityBuilder>.AddEntity<TTemplate>(EntityBuildAction<TTemplate>? buildAction)
            => AddEntity<TTemplate>(buildAction);

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineEntity<IFinalizedOverridenStateMachineWithEntityBuilder>.AddEntity<TTemplate, TEntity>(EntityBuildAction<TTemplate>? buildAction)
            => AddEntity<TTemplate, TEntity>(buildAction);

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineEntity<IFinalizedOverridenStateMachineWithEntityBuilder>.AddEntity<TTemplate>(EntityBuildAction<TTemplate>? buildAction)
            => AddEntity<TTemplate>(buildAction);

        IStateMachineWithEntityBuilder IStateMachineEntity<IStateMachineWithEntityBuilder>.AddEntity<TTemplate, TEntity>(EntityBuildAction<TTemplate>? buildAction)
            => AddEntity<TTemplate, TEntity>(buildAction);

        IStateMachineWithEntityBuilder IStateMachineEntity<IStateMachineWithEntityBuilder>.AddEntity<TTemplate>(EntityBuildAction<TTemplate>? buildAction)
            => AddEntity<TTemplate>(buildAction);

        IFinalizedStateMachineWithEntityBuilder IStateMachineEntity<IFinalizedStateMachineWithEntityBuilder>.AddEntity<TTemplate, TEntity>(EntityBuildAction<TTemplate>? buildAction)
            => AddEntity<TTemplate, TEntity>(buildAction);

        IFinalizedStateMachineWithEntityBuilder IStateMachineEntity<IFinalizedStateMachineWithEntityBuilder>.AddEntity<TTemplate>(EntityBuildAction<TTemplate>? buildAction)
            => AddEntity<TTemplate>(buildAction);

        IInitializedStateMachineWithEntityBuilder IStateMachineEntity<IInitializedStateMachineWithEntityBuilder>.AddEntity<TTemplate, TEntity>(EntityBuildAction<TTemplate>? buildAction)
            => AddEntity<TTemplate, TEntity>(buildAction) as IInitializedStateMachineWithEntityBuilder;

        IFinalizedStateMachineWithEntityBuilder IStateMachineFinal<IFinalizedStateMachineWithEntityBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName) as IFinalizedStateMachineWithEntityBuilder;

        IOverridenStateMachineWithEntityBuilder IStateMachineOverride<IOverridenStateMachineWithEntityBuilder>.UseStateMachine<TStateMachine>(OverridenStateMachineBuildAction buildAction)
            => UseStateMachine<TStateMachine>(buildAction) as IOverridenStateMachineWithEntityBuilder;

        IFinalizedOverridenStateMachineWithEntityBuilder IStateMachineFinal<IFinalizedOverridenStateMachineWithEntityBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName) as IFinalizedOverridenStateMachineWithEntityBuilder;
    }
}

