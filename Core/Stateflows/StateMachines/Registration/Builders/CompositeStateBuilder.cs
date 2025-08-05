using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Registration;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class CompositeStateBuilder : 
        ICompositeStateBuilder,
        IOverridenCompositeStateBuilder,
        IOverridenRegionalizedCompositeStateBuilder,
        IFinalizedCompositeStateBuilder,
        IFinalizedOverridenCompositeStateBuilder,
        IInitializedCompositeStateBuilder,
        IVertexBuilder,
        IGraphBuilder,
        IBehaviorBuilder
    {
        public Region Region { get; }

        public Vertex Vertex => Region.ParentVertex;
        
        public Graph Graph => Vertex.Graph;

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Region.Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Region.Graph.Version;

        public CompositeStateBuilder(Region region)
        {
            Region = region;
            Builder = new StateBuilder(Region.ParentVertex);
        }

        private StateBuilder Builder { get; set; }

        [DebuggerHidden]
        private IInitializedCompositeStateBuilder AddVertex(string stateName, VertexType type, Action<Vertex> vertexBuildAction = null)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                throw new StateDefinitionException(stateName, $"State name cannot be empty", Region.Graph.Class);
            }

            if (Region.Vertices.ContainsKey(stateName))
            {
                throw new StateDefinitionException(stateName, $"State '{stateName}' is already registered", Region.Graph.Class);
            }
            
            var vertex = new Vertex()
            {
                Name = stateName,
                Type = type,
                ParentRegion = Region,
                Graph = Region.Graph,
            };

            vertexBuildAction?.Invoke(vertex);

            Region.Vertices.Add(vertex.Name, vertex);
            Region.Graph.AllVertices.Add(vertex.Identifier, vertex);
            
            Graph.VisitingTasks.Add(visitor => visitor.VertexAddedAsync(Graph.Name, Graph.Version, vertex.Name, vertex.Type));

            return this;
        }

        #region Events
        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
        {
            Builder.AddOnEntry(actionsAsync);
            return this;
        }

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddOnInitialize(Func<IStateActionContext, Task> actionAsync)
        {
            Builder.AddOnInitialize(actionAsync);
            return this;
        }

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder ICompositeStateEvents<IFinalizedOverridenCompositeStateBuilder>.AddOnFinalize(Func<IStateActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder ICompositeStateEvents<IFinalizedOverridenCompositeStateBuilder>.AddOnInitialize(Func<IStateActionContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IOverridenCompositeStateBuilder ICompositeStateEvents<IOverridenCompositeStateBuilder>.AddOnFinalize(Func<IStateActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IOverridenCompositeStateBuilder ICompositeStateEvents<IOverridenCompositeStateBuilder>.AddOnInitialize(Func<IStateActionContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddOnFinalize(Func<IStateActionContext, Task> actionAsync)
        {
            Builder.AddOnFinalize(actionAsync);
            return this;
        }
        
        [DebuggerHidden]

        public IInitializedCompositeStateBuilder AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
        {
            Builder.AddOnExit(actionsAsync);
            return this;
        }
        #endregion

        #region Utils
        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddDeferredEvent<TEvent>()
        {
            Builder.AddDeferredEvent<TEvent>();
            return this;
        }
        #endregion

        #region Transitions
        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction = null)

        {
            Builder.AddTransition<TEvent>(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitions<IFinalizedOverridenCompositeStateBuilder>.
            AddDefaultTransition(string targetStateName,
                DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitions<IFinalizedOverridenCompositeStateBuilder>.AddInternalTransition<TEvent>(
            InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitions<IFinalizedOverridenCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitions<IFinalizedOverridenCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitions<IFinalizedOverridenCompositeStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitions<IFinalizedOverridenCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateTransitions<IOverridenCompositeStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateTransitions<IOverridenCompositeStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateTransitions<IOverridenCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateTransitions<IOverridenCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateTransitions<IOverridenCompositeStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
        {
            Builder.AddElseTransition<TEvent>(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateTransitions<IOverridenCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction = null)
            => AddTransition<Completion>(targetStateName, builder => transitionBuildAction?.Invoke(builder as IDefaultTransitionBuilder));

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            => AddElseTransition<Completion>(targetStateName, builder => transitionBuildAction?.Invoke(builder as IElseDefaultTransitionBuilder));

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)

            => AddTransition<TEvent>(Constants.DefaultTransitionTarget, builder => transitionBuildAction?.Invoke(builder as IInternalTransitionBuilder<TEvent>));

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)

            => AddElseTransition<TEvent>(Constants.DefaultTransitionTarget, builder => transitionBuildAction?.Invoke(builder as IElseInternalTransitionBuilder<TEvent>));
        #endregion

        #region AddState
        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddState(string stateName, StateBuildAction stateBuildAction = null)
            => AddVertex(stateName, VertexType.State, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex)));

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateMachine<IOverridenCompositeStateBuilder>.AddCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction) as IOverridenCompositeStateBuilder;
        [DebuggerHidden]

        IOverridenCompositeStateBuilder IStateMachine<IOverridenCompositeStateBuilder>.AddOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateMachine<IOverridenCompositeStateBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateMachine<IOverridenCompositeStateBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction) as IOverridenCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateMachine<IOverridenRegionalizedCompositeStateBuilder>.AddFork(
            string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateMachine<IOverridenRegionalizedCompositeStateBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenCompositeStateBuilder IStateMachine<IOverridenCompositeStateBuilder>.AddFork(string forkName,
            ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction) as IOverridenCompositeStateBuilder;

        IOverridenCompositeStateBuilder IStateMachine<IOverridenCompositeStateBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction) as IOverridenCompositeStateBuilder;

        public IInitializedCompositeStateBuilder AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddVertex(forkName, VertexType.Fork, vertex => forkBuildAction?.Invoke(new StateBuilder(vertex)));

        public IInitializedCompositeStateBuilder AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddVertex(joinName, VertexType.Join, vertex => joinBuildAction?.Invoke(new StateBuilder(vertex)));
        #endregion

        [DebuggerHidden]
        public IFinalizedCompositeStateBuilder AddFinalState(string finalStateName = FinalState.Name)
            => AddVertex(finalStateName, VertexType.FinalState) as IFinalizedCompositeStateBuilder;

        #region AddCompositeState
        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateMachine<IOverridenCompositeStateBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddVertex(compositeStateName, VertexType.CompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion)));

        public IInitializedCompositeStateBuilder AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddVertex(orthogonalStateName, VertexType.OrthogonalState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex)));

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddInitialState(string stateName, StateBuildAction stateBuildAction = null)
        {
            Region.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialState, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex)));
        }

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddInitialCompositeState(string stateName, CompositeStateBuildAction compositeStateBuildAction)
        {
            Region.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialCompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion)));
        }

        public IInitializedCompositeStateBuilder AddInitialOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            Region.InitialVertexName = orthogonalStateName;
            return AddVertex(orthogonalStateName, VertexType.InitialCompositeState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex)));
        }
        #endregion

        [DebuggerHidden]
        ICompositeStateBuilder IStateEntry<ICompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync) as ICompositeStateBuilder;

        [DebuggerHidden]
        ICompositeStateBuilder IStateExit<ICompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync) as ICompositeStateBuilder;

        [DebuggerHidden]
        ICompositeStateBuilder ICompositeStateEvents<ICompositeStateBuilder>.AddOnInitialize(Func<IStateActionContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as ICompositeStateBuilder;

        [DebuggerHidden]
        ICompositeStateBuilder ICompositeStateEvents<ICompositeStateBuilder>.AddOnFinalize(Func<IStateActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as ICompositeStateBuilder;

        [DebuggerHidden]
        IInitializedCompositeStateBuilder IStateMachineInitial<IInitializedCompositeStateBuilder>.AddInitialState(string stateName, StateBuildAction stateBuildAction)
            => AddInitialState(stateName, stateBuildAction);

        [DebuggerHidden]
        IInitializedCompositeStateBuilder IStateMachineInitial<IInitializedCompositeStateBuilder>.AddInitialCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddInitialCompositeState(compositeStateName, compositeStateBuildAction);

        [DebuggerHidden]
        ICompositeStateBuilder IStateTransitions<ICompositeStateBuilder>.AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetStateName, transitionBuildAction) as ICompositeStateBuilder;

        [DebuggerHidden]
        ICompositeStateBuilder IStateTransitions<ICompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition<TEvent>(targetStateName, transitionBuildAction) as ICompositeStateBuilder;

        [DebuggerHidden]
        ICompositeStateBuilder IStateTransitions<ICompositeStateBuilder>.AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as ICompositeStateBuilder;

        [DebuggerHidden]
        ICompositeStateBuilder IStateTransitions<ICompositeStateBuilder>.AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction) as ICompositeStateBuilder;

        [DebuggerHidden]
        ICompositeStateBuilder IStateTransitions<ICompositeStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(builder => transitionBuildAction?.Invoke(builder)) as ICompositeStateBuilder;

        [DebuggerHidden]
        ICompositeStateBuilder IStateTransitions<ICompositeStateBuilder>.AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition<TEvent>(builder => transitionBuildAction?.Invoke(builder)) as ICompositeStateBuilder;

        [DebuggerHidden]
        ICompositeStateBuilder IStateUtils<ICompositeStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ICompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateEntry<IFinalizedCompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync) as IFinalizedCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateExit<IFinalizedCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync) as IFinalizedCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateUtils<IFinalizedCompositeStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IFinalizedCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateTransitions<IFinalizedCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetStateName, transitionBuildAction) as IFinalizedCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateTransitions<IFinalizedCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition<TEvent>(targetStateName, transitionBuildAction) as IFinalizedCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateTransitions<IFinalizedCompositeStateBuilder>.AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IFinalizedCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateTransitions<IFinalizedCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction) as IFinalizedCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateTransitions<IFinalizedCompositeStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(builder => transitionBuildAction?.Invoke(builder)) as IFinalizedCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateTransitions<IFinalizedCompositeStateBuilder>.AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition<TEvent>(builder => transitionBuildAction?.Invoke(builder)) as IFinalizedCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder ICompositeStateEvents<IFinalizedCompositeStateBuilder>.AddOnInitialize(Func<IStateActionContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IFinalizedCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder ICompositeStateEvents<IFinalizedCompositeStateBuilder>.AddOnFinalize(Func<IStateActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IFinalizedCompositeStateBuilder;

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddVertex(junctionName, VertexType.Junction, vertex => junctionBuildAction?.Invoke(new StateBuilder(vertex)));

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddVertex(choiceName, VertexType.Choice, vertex => choiceBuildAction?.Invoke(new StateBuilder(vertex)));

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateEntry<IOverridenCompositeStateBuilder>.AddOnEntry(
            params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateExit<IOverridenCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateUtils<IOverridenCompositeStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseDefaultTransition(string targetStateName,
            OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseInternalTransition<TEvent>(
            OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseElseDefaultTransition(string targetStateName,
            OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseElseInternalTransition<TEvent>(
            OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseTransition<TEvent>(string targetStateName,
            OverridenTransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        public IOverridenCompositeStateBuilder UseTransition<TEvent>(string targetStateName,
            OverridenTransitionBuildAction<TEvent> transitionBuildAction)
        {
            Builder.UseTransition(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        public IOverridenCompositeStateBuilder UseDefaultTransition(string targetStateName,
            OverridenDefaultTransitionBuildAction transitionBuildAction)
        {
            Builder.UseDefaultTransition(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        public IOverridenCompositeStateBuilder UseInternalTransition<TEvent>(OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
        {
            Builder.UseInternalTransition(transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        public IOverridenCompositeStateBuilder UseElseTransition<TEvent>(string targetStateName,
            OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
        {
            Builder.UseElseTransition(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        public IOverridenCompositeStateBuilder UseElseDefaultTransition(string targetStateName,
            OverridenElseDefaultTransitionBuildAction transitionBuildAction)
        {
            Builder.UseElseDefaultTransition(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        public IOverridenCompositeStateBuilder UseElseInternalTransition<TEvent>(
            OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
        {
            Builder.UseElseInternalTransition(transitionBuildAction);
            return this;
        }

        public IOverridenCompositeStateBuilder UseState(string stateName, OverridenStateBuildAction stateBuildAction)
        {
            if (
                !Region.Vertices.TryGetValue(stateName, out var vertex) ||
                (
                    vertex.Type != VertexType.State && 
                    vertex.Type != VertexType.InitialState
                ) || 
                vertex.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"State '{stateName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            stateBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        public IOverridenCompositeStateBuilder UseCompositeState(string compositeStateName,
            OverridenCompositeStateBuildAction compositeStateBuildAction)
        {
            if (
                !Region.Vertices.TryGetValue(compositeStateName, out var vertex) ||
                (
                    vertex.Type != VertexType.CompositeState &&
                    vertex.Type != VertexType.InitialCompositeState
                ) ||
                vertex.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"Composite state '{compositeStateName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }

            compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion));

            return this;
        }

        public IOverridenCompositeStateBuilder UseOrthogonalState(string orthogonalStateName,
            OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            if (
                !Region.Vertices.TryGetValue(orthogonalStateName, out var vertex) ||
                (
                    vertex.Type != VertexType.CompositeState &&
                    vertex.Type != VertexType.InitialCompositeState
                ) ||
                vertex.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"Orthogonal state '{orthogonalStateName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }

            orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex));

            return this;
        }

        public IOverridenCompositeStateBuilder UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
        {
            if (!Region.Vertices.TryGetValue(junctionName, out var vertex) || vertex.Type != VertexType.Junction || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Junction '{junctionName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            junctionBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        public IOverridenCompositeStateBuilder UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
        {
            if (!Region.Vertices.TryGetValue(choiceName, out var vertex) || vertex.Type != VertexType.Choice || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Choice '{choiceName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            choiceBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        IFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        IFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        public IOverridenCompositeStateBuilder UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
        {
            if (!Region.Vertices.TryGetValue(forkName, out var vertex) || vertex.Type != VertexType.Choice || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Fork '{forkName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            forkBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        public IOverridenCompositeStateBuilder UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
        {
            if (!Region.Vertices.TryGetValue(joinName, out var vertex) || vertex.Type != VertexType.Choice || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Join '{joinName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            joinBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        public IOverridenRegionalizedCompositeStateBuilder MakeOrthogonal(OrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            Region.ParentVertex.Type = Region.ParentVertex.Type == VertexType.InitialCompositeState
                ? VertexType.InitialOrthogonalState
                : VertexType.OrthogonalState;

            orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(Region.ParentVertex));

            return this;
        }

        IFinalizedOverridenCompositeStateBuilder IStateMachineFinal<IFinalizedOverridenCompositeStateBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName) as IFinalizedOverridenCompositeStateBuilder;

        IFinalizedOverridenCompositeStateBuilder IStateEntry<IFinalizedOverridenCompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync) as IFinalizedOverridenCompositeStateBuilder;

        IFinalizedOverridenCompositeStateBuilder IStateExit<IFinalizedOverridenCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync) as IFinalizedOverridenCompositeStateBuilder;

        IFinalizedOverridenCompositeStateBuilder IStateUtils<IFinalizedOverridenCompositeStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IFinalizedOverridenCompositeStateBuilder;

        IFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        IFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>.UseCompositeState(string compositeStateName, OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        IFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>.UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        IFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>.UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        IFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>.UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateOrthogonalization<IFinalizedOverridenRegionalizedCompositeStateBuilder>.MakeOrthogonal(OrthogonalStateBuildAction orthogonalStateBuildAction)
            => MakeOrthogonal(orthogonalStateBuildAction) as IFinalizedOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateEntry<IOverridenRegionalizedCompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateExit<IOverridenRegionalizedCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateUtils<IOverridenRegionalizedCompositeStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder ICompositeStateEvents<IOverridenRegionalizedCompositeStateBuilder>.AddOnInitialize(Func<IStateActionContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder ICompositeStateEvents<IOverridenRegionalizedCompositeStateBuilder>.AddOnFinalize(Func<IStateActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateTransitions<IOverridenRegionalizedCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetStateName, transitionBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateTransitions<IOverridenRegionalizedCompositeStateBuilder>.AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateTransitions<IOverridenRegionalizedCompositeStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateTransitions<IOverridenRegionalizedCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition<TEvent>(targetStateName, transitionBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateTransitions<IOverridenRegionalizedCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateTransitions<IOverridenRegionalizedCompositeStateBuilder>.AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition<TEvent>(transitionBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseTransition<TEvent>(string targetStateName, OverridenTransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseDefaultTransition(string targetStateName, OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseInternalTransition<TEvent>(OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseElseTransition<TEvent>(string targetStateName, OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseElseDefaultTransition(string targetStateName, OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseElseInternalTransition<TEvent>(OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateMachine<IOverridenRegionalizedCompositeStateBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateMachine<IOverridenRegionalizedCompositeStateBuilder>.AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateMachine<IOverridenRegionalizedCompositeStateBuilder>.AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateMachine<IOverridenRegionalizedCompositeStateBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateMachine<IOverridenRegionalizedCompositeStateBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseCompositeState(string compositeStateName, OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction) as IOverridenRegionalizedCompositeStateBuilder;

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineFinal<IFinalizedOverridenRegionalizedCompositeStateBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName) as IFinalizedOverridenRegionalizedCompositeStateBuilder;

        public string Name => Vertex.Name;
        public VertexType Type => Vertex.Type;
    }
}
