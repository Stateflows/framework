using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Registration;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using ActionBuildAction = Stateflows.Actions.Registration.Interfaces.ActionBuildAction;
using ActionDelegateAsync = Stateflows.Actions.Registration.ActionDelegateAsync;
using ForkBuildAction = Stateflows.StateMachines.Registration.Interfaces.ForkBuildAction;
using JoinBuildAction = Stateflows.StateMachines.Registration.Interfaces.JoinBuildAction;
using OverridenForkBuildAction = Stateflows.StateMachines.Registration.Interfaces.OverridenForkBuildAction;
using OverridenJoinBuildAction = Stateflows.StateMachines.Registration.Interfaces.OverridenJoinBuildAction;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class CompositeStateBuilder : 
        ICompositeStateBuilder,
        IOverridenCompositeStateBuilder,
        IOverridenRegionalizedCompositeStateBuilder,
        IFinalizedCompositeStateBuilder,
        IFinalizedOverridenCompositeStateBuilder,
        IFinalizedOverridenRegionalizedCompositeStateBuilder,
        IInitializedCompositeStateBuilder,
        IBehaviorCompositeStateBuilder,
        IBehaviorInitializedCompositeStateBuilder,
        IBehaviorFinalizedCompositeStateBuilder,
        IBehaviorOverridenCompositeStateBuilder,
        IBehaviorOverridenRegionalizedCompositeStateBuilder,
        IBehaviorFinalizedOverridenCompositeStateBuilder,
        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder,
        IStateBuilderInfo,
        IGraphBuilder,
        IVertexBuilder,
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
        private CompositeStateBuilder AddVertex(string stateName, VertexType type, Action<Vertex> vertexBuildAction = null)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                throw new StateDefinitionException(stateName, $"State name cannot be empty", Region.Graph.Class);
            }
            
            if (Graph.AllVertices.ContainsKey(stateName))
            {
                throw new StateDefinitionException(stateName, $"Vertex '{stateName}' is already registered", Graph.Class);
            }
            
            var vertex = new Vertex()
            {
                Name = stateName,
                Type = type,
                ParentRegion = Region,
                Graph = Graph,
            };

            vertexBuildAction?.Invoke(vertex);

            if (Graph.AllVertices.ContainsKey(vertex.Name))
                throw new StateDefinitionException(vertex.Name, $"Vertex '{vertex.Name}' is already registered", Graph.Class);
            Region.Vertices.Add(vertex.Name, vertex);
            Region.Graph.AllVertices.Add(vertex.Identifier, vertex);

            Graph.VisitingTasks.Add(visitor => visitor.VertexAddedAsync(Graph.Name, Graph.Version, vertex.Name, vertex.Type));

            return this;
        }

        #region Events
        [DebuggerHidden]
        private CompositeStateBuilder AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
        {
            Builder.AddOnEntry(actionsAsync);
            return this;
        }

        [DebuggerHidden]
        private CompositeStateBuilder AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
        {
            Builder.AddOnInitialize(actionsAsync);
            return this;
        }

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder ICompositeStateFinalization<IFinalizedOverridenCompositeStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnFinalize(actionsAsync);

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder ICompositeStateInitialization<IFinalizedOverridenCompositeStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnInitialize(actionsAsync);

        [DebuggerHidden]
        IOverridenCompositeStateBuilder ICompositeStateFinalization<IOverridenCompositeStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnFinalize(actionsAsync);

        [DebuggerHidden]
        IOverridenCompositeStateBuilder ICompositeStateInitialization<IOverridenCompositeStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnInitialize(actionsAsync);

        [DebuggerHidden]
        private CompositeStateBuilder AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
        {
            Builder.AddOnFinalize(actionsAsync);
            return this;
        }
        
        [DebuggerHidden]

        private CompositeStateBuilder AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
        {
            Builder.AddOnExit(actionsAsync);
            return this;
        }
        #endregion

        #region Utils
        [DebuggerHidden]
        private CompositeStateBuilder AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
        {
            Builder.AddDeferredEvent<TEvent>(buildAction);
            return this;
        }
        #endregion

        #region Transitions
        [DebuggerHidden]
        private CompositeStateBuilder AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction = null)

        {
            Builder.AddTransition<TEvent>(targetStateName, transitionBuildAction);
            return this;
        }

        IInitializedCompositeStateBuilder IStateTransitions<IInitializedCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction);

        IInitializedCompositeStateBuilder IStateTransitions<IInitializedCompositeStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitions<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitions<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateTransitions<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateTransitions<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateTransitions<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateTransitions<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateTransitions<IBehaviorOverridenCompositeStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateTransitions<IBehaviorOverridenCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedCompositeStateBuilder IStateTransitions<IBehaviorFinalizedCompositeStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedCompositeStateBuilder IStateTransitions<IBehaviorFinalizedCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction);

        IBehaviorInitializedCompositeStateBuilder IStateTransitions<IBehaviorInitializedCompositeStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorInitializedCompositeStateBuilder IStateTransitions<IBehaviorInitializedCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction);

        IBehaviorCompositeStateBuilder IStateTransitions<IBehaviorCompositeStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorCompositeStateBuilder IStateTransitions<IBehaviorCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder
            IStateTransitions<IFinalizedOverridenRegionalizedCompositeStateBuilder>.AddDefaultTransition(
                string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as
                IFinalizedOverridenRegionalizedCompositeStateBuilder;

        IFinalizedOverridenRegionalizedCompositeStateBuilder
            IStateTransitions<IFinalizedOverridenRegionalizedCompositeStateBuilder>.AddTransition<TEvent>(
                string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetStateName, transitionBuildAction) as
                IFinalizedOverridenRegionalizedCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitions<IFinalizedOverridenCompositeStateBuilder>.
            AddDefaultTransition(string targetStateName,
                DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitions<IFinalizedOverridenCompositeStateBuilder>.AddInternalTransition<TEvent>(
            InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction);

        IInitializedCompositeStateBuilder IStateTransitions<IInitializedCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction);

        IInitializedCompositeStateBuilder IStateTransitions<IInitializedCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        IInitializedCompositeStateBuilder IStateTransitions<IInitializedCompositeStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction);

        IInitializedCompositeStateBuilder IStateTransitions<IInitializedCompositeStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitions<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitions<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitions<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitions<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddInternalTransition<TEvent>(
            InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateTransitions<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateTransitions<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateTransitions<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateTransitions<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddInternalTransition<TEvent>(
            InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateTransitions<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateTransitions<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateTransitions<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateTransitions<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddInternalTransition<TEvent>(
            InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateTransitions<IBehaviorOverridenCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateTransitions<IBehaviorOverridenCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateTransitions<IBehaviorOverridenCompositeStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateTransitions<IBehaviorOverridenCompositeStateBuilder>.AddInternalTransition<TEvent>(
            InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction);

        IBehaviorFinalizedCompositeStateBuilder IStateTransitions<IBehaviorFinalizedCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedCompositeStateBuilder IStateTransitions<IBehaviorFinalizedCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedCompositeStateBuilder IStateTransitions<IBehaviorFinalizedCompositeStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction);

        IBehaviorFinalizedCompositeStateBuilder IStateTransitions<IBehaviorFinalizedCompositeStateBuilder>.AddInternalTransition<TEvent>(
            InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction);

        IBehaviorInitializedCompositeStateBuilder IStateTransitions<IBehaviorInitializedCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction);

        IBehaviorInitializedCompositeStateBuilder IStateTransitions<IBehaviorInitializedCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorInitializedCompositeStateBuilder IStateTransitions<IBehaviorInitializedCompositeStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction);

        IBehaviorInitializedCompositeStateBuilder IStateTransitions<IBehaviorInitializedCompositeStateBuilder>.AddInternalTransition<TEvent>(
            InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction);

        IBehaviorCompositeStateBuilder IStateTransitions<IBehaviorCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction);

        IBehaviorCompositeStateBuilder IStateTransitions<IBehaviorCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorCompositeStateBuilder IStateTransitions<IBehaviorCompositeStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction);

        IBehaviorCompositeStateBuilder IStateTransitions<IBehaviorCompositeStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitions<IFinalizedOverridenRegionalizedCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitions<IFinalizedOverridenRegionalizedCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitions<IFinalizedOverridenRegionalizedCompositeStateBuilder>.AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitions<IFinalizedOverridenRegionalizedCompositeStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction);

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitions<IFinalizedOverridenCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitions<IFinalizedOverridenCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitions<IFinalizedOverridenCompositeStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction);

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitions<IFinalizedOverridenCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateTransitions<IOverridenCompositeStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateTransitions<IOverridenCompositeStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction);

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateTransitions<IOverridenCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateTransitions<IOverridenCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateTransitions<IOverridenCompositeStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction);

        [DebuggerHidden]
        private CompositeStateBuilder AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
        {
            Builder.AddElseTransition<TEvent>(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateTransitions<IOverridenCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        private CompositeStateBuilder AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction = null)
            => AddTransition<Completion>(targetStateName, builder => transitionBuildAction?.Invoke(builder as IDefaultTransitionBuilder));

        [DebuggerHidden]
        private CompositeStateBuilder AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            => AddElseTransition<Completion>(targetStateName, builder => transitionBuildAction?.Invoke(builder as IElseDefaultTransitionBuilder));

        [DebuggerHidden]
        private CompositeStateBuilder AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
        {
            Builder.AddInternalTransition(transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        private CompositeStateBuilder AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
        {
            Builder.AddElseInternalTransition(transitionBuildAction);
            return this;
        }
        #endregion

        #region AddState
        [DebuggerHidden]
        private CompositeStateBuilder AddState(string stateName, StateBuildAction stateBuildAction = null)
            => AddVertex(stateName, VertexType.State, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex)));

        IInitializedCompositeStateBuilder IStateMachineElements<IInitializedCompositeStateBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction);

        IInitializedCompositeStateBuilder IStateMachineElements<IInitializedCompositeStateBuilder>.AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction);

        IInitializedCompositeStateBuilder IStateMachineElements<IInitializedCompositeStateBuilder>.AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateMachineElements<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateMachineElements<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateMachineElements<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateMachineElements<IBehaviorOverridenCompositeStateBuilder>.AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateMachineElements<IBehaviorOverridenCompositeStateBuilder>.AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateMachineElements<IBehaviorOverridenCompositeStateBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction);

        IBehaviorInitializedCompositeStateBuilder IStateMachineElements<IBehaviorInitializedCompositeStateBuilder>.AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction);

        IBehaviorInitializedCompositeStateBuilder IStateMachineElements<IBehaviorInitializedCompositeStateBuilder>.AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IBehaviorInitializedCompositeStateBuilder IStateMachineElements<IBehaviorInitializedCompositeStateBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction);

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateMachineElements<IOverridenCompositeStateBuilder>.AddCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction);
        [DebuggerHidden]

        IOverridenCompositeStateBuilder IStateMachineElements<IOverridenCompositeStateBuilder>.AddOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateMachineElements<IOverridenCompositeStateBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction);

        IInitializedCompositeStateBuilder IStateMachineElements<IInitializedCompositeStateBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction);

        IInitializedCompositeStateBuilder IStateMachineElements<IInitializedCompositeStateBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction);

        IInitializedCompositeStateBuilder IStateMachineElements<IInitializedCompositeStateBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction);

        IInitializedCompositeStateBuilder IStateMachineElements<IInitializedCompositeStateBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateMachineElements<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateMachineElements<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateMachineElements<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateMachineElements<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddJunction(string junctionName,
            JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateMachineElements<IBehaviorOverridenCompositeStateBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateMachineElements<IBehaviorOverridenCompositeStateBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateMachineElements<IBehaviorOverridenCompositeStateBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateMachineElements<IBehaviorOverridenCompositeStateBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction);

        IBehaviorInitializedCompositeStateBuilder IStateMachineElements<IBehaviorInitializedCompositeStateBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction);

        IBehaviorInitializedCompositeStateBuilder IStateMachineElements<IBehaviorInitializedCompositeStateBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction);

        IBehaviorInitializedCompositeStateBuilder IStateMachineElements<IBehaviorInitializedCompositeStateBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction);

        IBehaviorInitializedCompositeStateBuilder IStateMachineElements<IBehaviorInitializedCompositeStateBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction);

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateMachineElements<IOverridenCompositeStateBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateMachineElements<IOverridenRegionalizedCompositeStateBuilder>.AddFork(
            string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateMachineElements<IOverridenRegionalizedCompositeStateBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction);

        IOverridenCompositeStateBuilder IStateMachineElements<IOverridenCompositeStateBuilder>.AddFork(string forkName,
            ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction);

        IOverridenCompositeStateBuilder IStateMachineElements<IOverridenCompositeStateBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction);

        private CompositeStateBuilder AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddVertex(forkName, VertexType.Fork, vertex => forkBuildAction?.Invoke(new StateBuilder(vertex)));

        private CompositeStateBuilder AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddVertex(joinName, VertexType.Join, vertex => joinBuildAction?.Invoke(new StateBuilder(vertex)));
        #endregion

        [DebuggerHidden]
        private CompositeStateBuilder AddFinalState(string finalStateName = null)
            => AddVertex(finalStateName ?? State<FinalState>.Name, VertexType.FinalState);

        #region AddCompositeState
        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateMachineElements<IOverridenCompositeStateBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction);

        [DebuggerHidden]
        private CompositeStateBuilder AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddVertex(compositeStateName, VertexType.CompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion)));

        private CompositeStateBuilder AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddVertex(orthogonalStateName, VertexType.OrthogonalState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex)));

        [DebuggerHidden]
        private CompositeStateBuilder AddInitialState(string stateName, StateBuildAction stateBuildAction = null)
        {
            stateName ??= $"{Region.ParentVertex.Name}:{InitialState.Name}";
            Region.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialState, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex)));
        }

        IBehaviorInitializedCompositeStateBuilder IStateMachineInitial<IBehaviorInitializedCompositeStateBuilder>.AddInitialState(string stateName, StateBuildAction stateBuildAction)
            => AddInitialState(stateName, stateBuildAction);

        [DebuggerHidden]
        private CompositeStateBuilder AddInitialCompositeState(string stateName, CompositeStateBuildAction compositeStateBuildAction)
        {
            Region.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialCompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion)));
        }

        IBehaviorInitializedCompositeStateBuilder IStateMachineInitial<IBehaviorInitializedCompositeStateBuilder>.AddInitialOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddInitialOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IBehaviorInitializedCompositeStateBuilder IStateMachineInitial<IBehaviorInitializedCompositeStateBuilder>.AddInitialCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddInitialCompositeState(compositeStateName, compositeStateBuildAction);

        IInitializedCompositeStateBuilder IStateMachineInitial<IInitializedCompositeStateBuilder>.AddInitialOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddInitialOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        private CompositeStateBuilder AddInitialOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            Region.InitialVertexName = orthogonalStateName;
            return AddVertex(orthogonalStateName, VertexType.InitialCompositeState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex)));
        }
        #endregion

        [DebuggerHidden]
        ICompositeStateBuilder IStateEntry<ICompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync);

        [DebuggerHidden]
        ICompositeStateBuilder IStateExit<ICompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync);

        [DebuggerHidden]
        ICompositeStateBuilder ICompositeStateInitialization<ICompositeStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnInitialize(actionsAsync);

        [DebuggerHidden]
        ICompositeStateBuilder ICompositeStateFinalization<ICompositeStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnFinalize(actionsAsync);

        [DebuggerHidden]
        IInitializedCompositeStateBuilder IStateMachineInitial<IInitializedCompositeStateBuilder>.AddInitialState(string stateName, StateBuildAction stateBuildAction)
            => AddInitialState(stateName, stateBuildAction);

        [DebuggerHidden]
        IInitializedCompositeStateBuilder IStateMachineInitial<IInitializedCompositeStateBuilder>.AddInitialCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddInitialCompositeState(compositeStateName, compositeStateBuildAction);

        [DebuggerHidden]
        ICompositeStateBuilder IStateTransitions<ICompositeStateBuilder>.AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        ICompositeStateBuilder IStateTransitions<ICompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition<TEvent>(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        ICompositeStateBuilder IStateTransitions<ICompositeStateBuilder>.AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        ICompositeStateBuilder IStateTransitions<ICompositeStateBuilder>.AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        ICompositeStateBuilder IStateTransitions<ICompositeStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(builder => transitionBuildAction?.Invoke(builder));

        [DebuggerHidden]
        ICompositeStateBuilder IStateTransitions<ICompositeStateBuilder>.AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition<TEvent>(builder => transitionBuildAction?.Invoke(builder));

        [DebuggerHidden]
        ICompositeStateBuilder IStateUtils<ICompositeStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => AddDeferredEvent<TEvent>(buildAction);

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateEntry<IFinalizedCompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync);

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateExit<IFinalizedCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync);

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateUtils<IFinalizedCompositeStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => AddDeferredEvent<TEvent>(buildAction);

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateTransitions<IFinalizedCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateTransitions<IFinalizedCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition<TEvent>(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateTransitions<IFinalizedCompositeStateBuilder>.AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateTransitions<IFinalizedCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateTransitions<IFinalizedCompositeStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(builder => transitionBuildAction?.Invoke(builder));

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateTransitions<IFinalizedCompositeStateBuilder>.AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition<TEvent>(builder => transitionBuildAction?.Invoke(builder));

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder ICompositeStateInitialization<IFinalizedCompositeStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnInitialize(actionsAsync);

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder ICompositeStateFinalization<IFinalizedCompositeStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnFinalize(actionsAsync);

        [DebuggerHidden]
        private CompositeStateBuilder AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddVertex(junctionName, VertexType.Junction, vertex => junctionBuildAction?.Invoke(new StateBuilder(vertex)));

        [DebuggerHidden]
        private CompositeStateBuilder AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddVertex(choiceName, VertexType.Choice, vertex => choiceBuildAction?.Invoke(new StateBuilder(vertex)));

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateEntry<IOverridenCompositeStateBuilder>.AddOnEntry(
            params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync);

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateExit<IOverridenCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync);

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateUtils<IOverridenCompositeStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => AddDeferredEvent<TEvent>(buildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>.UseTransition<TEvent>(string targetStateName, OverridenTransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction);

        IOverridenCompositeStateBuilder IStateTransitionsOverrides<IOverridenCompositeStateBuilder>.UseDefaultTransition(string targetStateName,
            OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction);

        IOverridenCompositeStateBuilder IStateTransitionsOverrides<IOverridenCompositeStateBuilder>.UseTransition<TEvent>(string targetStateName,
            OverridenTransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.UseDefaultTransition(string targetStateName,
            OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.UseTransition<TEvent>(string targetStateName,
            OverridenTransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>.UseDefaultTransition(string targetStateName,
            OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>.UseTransition<TEvent>(string targetStateName,
            OverridenTransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>.UseDefaultTransition(string targetStateName,
            OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>.UseTransition<TEvent>(string targetStateName,
            OverridenTransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateTransitionsOverrides<IBehaviorOverridenCompositeStateBuilder>.UseDefaultTransition(string targetStateName,
            OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateTransitionsOverrides<IBehaviorOverridenCompositeStateBuilder>.UseTransition<TEvent>(string targetStateName,
            OverridenTransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>.UseDefaultTransition(string targetStateName, OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseDefaultTransition(string targetStateName,
            OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseInternalTransition<TEvent>(
            OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction);

        IOverridenCompositeStateBuilder IStateTransitionsOverrides<IOverridenCompositeStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction);

        IOverridenCompositeStateBuilder IStateTransitionsOverrides<IOverridenCompositeStateBuilder>.UseElseDefaultTransition(string targetStateName,
            OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);

        IOverridenCompositeStateBuilder IStateTransitionsOverrides<IOverridenCompositeStateBuilder>.UseElseInternalTransition<TEvent>(
            OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction);

        IOverridenCompositeStateBuilder IStateTransitionsOverrides<IOverridenCompositeStateBuilder>.UseInternalTransition<TEvent>(
            OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.UseElseDefaultTransition(string targetStateName,
            OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.UseElseInternalTransition<TEvent>(
            OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.UseInternalTransition<TEvent>(
            OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>.UseElseDefaultTransition(string targetStateName,
            OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>.UseElseInternalTransition<TEvent>(
            OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>.UseInternalTransition<TEvent>(
            OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>.UseElseDefaultTransition(string targetStateName,
            OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>.UseElseInternalTransition<TEvent>(
            OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>.UseInternalTransition<TEvent>(
            OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateTransitionsOverrides<IBehaviorOverridenCompositeStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateTransitionsOverrides<IBehaviorOverridenCompositeStateBuilder>.UseElseDefaultTransition(string targetStateName,
            OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateTransitionsOverrides<IBehaviorOverridenCompositeStateBuilder>.UseElseInternalTransition<TEvent>(
            OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateTransitionsOverrides<IBehaviorOverridenCompositeStateBuilder>.UseInternalTransition<TEvent>(
            OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>.UseElseTransition<TEvent>(string targetStateName, OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>.UseElseDefaultTransition(string targetStateName, OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>.UseElseInternalTransition<TEvent>(OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>.UseInternalTransition<TEvent>(OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction);

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseElseDefaultTransition(string targetStateName,
            OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseElseInternalTransition<TEvent>(
            OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction);

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseTransition<TEvent>(string targetStateName,
            OverridenTransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        private CompositeStateBuilder UseTransition<TEvent>(string targetStateName,
            OverridenTransitionBuildAction<TEvent> transitionBuildAction)
        {
            Builder.UseTransition(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        private CompositeStateBuilder UseDefaultTransition(string targetStateName,
            OverridenDefaultTransitionBuildAction transitionBuildAction)
        {
            Builder.UseDefaultTransition(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        private CompositeStateBuilder UseInternalTransition<TEvent>(OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
        {
            Builder.UseInternalTransition(transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        private CompositeStateBuilder UseElseTransition<TEvent>(string targetStateName,
            OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
        {
            Builder.UseElseTransition(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        private CompositeStateBuilder UseElseDefaultTransition(string targetStateName,
            OverridenElseDefaultTransitionBuildAction transitionBuildAction)
        {
            Builder.UseElseDefaultTransition(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        private CompositeStateBuilder UseElseInternalTransition<TEvent>(
            OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
        {
            Builder.UseElseInternalTransition(transitionBuildAction);
            return this;
        }

        private CompositeStateBuilder UseState(string stateName, OverridenStateBuildAction stateBuildAction)
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

        IOverridenCompositeStateBuilder IStateMachineOverrides<IOverridenCompositeStateBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction);

        IOverridenCompositeStateBuilder IStateMachineOverrides<IOverridenCompositeStateBuilder>.UseCompositeState(string compositeStateName,
            OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction);

        IOverridenCompositeStateBuilder IStateMachineOverrides<IOverridenCompositeStateBuilder>.UseOrthogonalState(string orthogonalStateName,
            OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IOverridenCompositeStateBuilder IStateMachineOverrides<IOverridenCompositeStateBuilder>.UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction);

        IOverridenCompositeStateBuilder IStateMachineOverrides<IOverridenCompositeStateBuilder>.UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction);

        IOverridenCompositeStateBuilder IStateMachineOverrides<IOverridenCompositeStateBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction);

        IOverridenCompositeStateBuilder IStateMachineOverrides<IOverridenCompositeStateBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.UseCompositeState(string compositeStateName,
            OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.UseOrthogonalState(string orthogonalStateName,
            OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.UseJunction(string junctionName,
            OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.UseChoice(string choiceName,
            OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.UseFork(string forkName,
            OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.UseJoin(string joinName,
            OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.UseState(string stateName,
            OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>.UseCompositeState(string compositeStateName,
            OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>.UseOrthogonalState(string orthogonalStateName,
            OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>.UseJunction(string junctionName,
            OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>.UseChoice(string choiceName,
            OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>.UseCompositeState(string compositeStateName,
            OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>.UseOrthogonalState(string orthogonalStateName,
            OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>.UseJunction(string junctionName,
            OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>.UseChoice(string choiceName,
            OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>.UseState(string stateName,
            OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateMachineOverrides<IBehaviorOverridenCompositeStateBuilder>.UseCompositeState(string compositeStateName,
            OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateMachineOverrides<IBehaviorOverridenCompositeStateBuilder>.UseOrthogonalState(string orthogonalStateName,
            OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateMachineOverrides<IBehaviorOverridenCompositeStateBuilder>.UseJunction(string junctionName,
            OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateMachineOverrides<IBehaviorOverridenCompositeStateBuilder>.UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateMachineOverrides<IBehaviorOverridenCompositeStateBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateMachineOverrides<IBehaviorOverridenCompositeStateBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction);

        IBehaviorOverridenCompositeStateBuilder IStateMachineOverrides<IBehaviorOverridenCompositeStateBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>.UseCompositeState(string compositeStateName, OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>.UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>.UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>.UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction);

        private CompositeStateBuilder UseCompositeState(string compositeStateName, OverridenCompositeStateBuildAction compositeStateBuildAction)
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

        private CompositeStateBuilder UseOrthogonalState(string orthogonalStateName,
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

        private CompositeStateBuilder UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
        {
            if (!Region.Vertices.TryGetValue(junctionName, out var vertex) || vertex.Type != VertexType.Junction || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Junction '{junctionName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            junctionBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        private CompositeStateBuilder UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
        {
            if (!Region.Vertices.TryGetValue(choiceName, out var vertex) || vertex.Type != VertexType.Choice || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Choice '{choiceName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            choiceBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        IFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction);

        IFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction);

        private CompositeStateBuilder UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
        {
            if (!Region.Vertices.TryGetValue(forkName, out var vertex) || vertex.Type != VertexType.Choice || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Fork '{forkName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            forkBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        private CompositeStateBuilder UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
        {
            if (!Region.Vertices.TryGetValue(joinName, out var vertex) || vertex.Type != VertexType.Choice || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Join '{joinName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            joinBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        private CompositeStateBuilder MakeOrthogonal(OrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            Region.ParentVertex.Type = Region.ParentVertex.Type == VertexType.InitialCompositeState
                ? VertexType.InitialOrthogonalState
                : VertexType.OrthogonalState;

            orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(Region.ParentVertex));

            return this;
        }

        IFinalizedOverridenCompositeStateBuilder IStateMachineFinal<IFinalizedOverridenCompositeStateBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName);

        IFinalizedOverridenCompositeStateBuilder IStateEntry<IFinalizedOverridenCompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync);

        IFinalizedOverridenCompositeStateBuilder IStateExit<IFinalizedOverridenCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync);

        IFinalizedOverridenCompositeStateBuilder IStateUtils<IFinalizedOverridenCompositeStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => AddDeferredEvent<TEvent>(buildAction);

        IFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction);

        IFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>.UseCompositeState(string compositeStateName, OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction);

        IFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>.UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>.UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction);

        IFinalizedOverridenCompositeStateBuilder IStateMachineOverrides<IFinalizedOverridenCompositeStateBuilder>.UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateOrthogonalization<IFinalizedOverridenRegionalizedCompositeStateBuilder>.MakeOrthogonal(OrthogonalStateBuildAction orthogonalStateBuildAction)
            => MakeOrthogonal(orthogonalStateBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateEntry<IOverridenRegionalizedCompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync);

        IOverridenRegionalizedCompositeStateBuilder IStateExit<IOverridenRegionalizedCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync);

        IOverridenRegionalizedCompositeStateBuilder IStateUtils<IOverridenRegionalizedCompositeStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => AddDeferredEvent<TEvent>(buildAction);

        IOverridenRegionalizedCompositeStateBuilder ICompositeStateInitialization<IOverridenRegionalizedCompositeStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnInitialize(actionsAsync);

        IOverridenRegionalizedCompositeStateBuilder ICompositeStateFinalization<IOverridenRegionalizedCompositeStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnFinalize(actionsAsync);

        IOverridenRegionalizedCompositeStateBuilder IStateTransitions<IOverridenRegionalizedCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetStateName, transitionBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateTransitions<IOverridenRegionalizedCompositeStateBuilder>.AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateTransitions<IOverridenRegionalizedCompositeStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateTransitions<IOverridenRegionalizedCompositeStateBuilder>.AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition<TEvent>(targetStateName, transitionBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateTransitions<IOverridenRegionalizedCompositeStateBuilder>.AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateTransitions<IOverridenRegionalizedCompositeStateBuilder>.AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition<TEvent>(transitionBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseTransition<TEvent>(string targetStateName, OverridenTransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseDefaultTransition(string targetStateName, OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseInternalTransition<TEvent>(OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseElseTransition<TEvent>(string targetStateName, OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseElseDefaultTransition(string targetStateName, OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseElseInternalTransition<TEvent>(OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateMachineElements<IOverridenRegionalizedCompositeStateBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateMachineElements<IOverridenRegionalizedCompositeStateBuilder>.AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateMachineElements<IOverridenRegionalizedCompositeStateBuilder>.AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateMachineElements<IOverridenRegionalizedCompositeStateBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateMachineElements<IOverridenRegionalizedCompositeStateBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseCompositeState(string compositeStateName, OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateMachineOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineFinal<IFinalizedOverridenRegionalizedCompositeStateBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName);

        public string Name => Vertex.Name;
        public VertexType Type => Vertex.Type;

        private CompositeStateBuilder AddHistory(string historyEntrypointName, HistoryBuildAction buildAction)
        {
            historyEntrypointName ??= State<History>.Name;
            AddVertex(historyEntrypointName, VertexType.History);
            var historyVertex = Vertex.DefaultRegion.Vertices[historyEntrypointName];
            historyVertex.HistoricalRegion = Vertex.DefaultRegion;
            Vertex.DefaultRegion.History ??= historyVertex;
            
            buildAction?.Invoke(new StateBuilder(historyVertex));

            return this;
        }

        IOverridenCompositeStateBuilder IStateHistory<IOverridenCompositeStateBuilder>.AddHistory(string historyEntrypointName, HistoryBuildAction buildAction)
            => AddHistory(historyEntrypointName, buildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateHistory<IOverridenRegionalizedCompositeStateBuilder>.AddHistory(string historyEntrypointName, HistoryBuildAction buildAction)
            => AddHistory(historyEntrypointName, buildAction);

        IInitializedCompositeStateBuilder IStateHistory<IInitializedCompositeStateBuilder>.AddHistory(string historyEntrypointName, HistoryBuildAction buildAction)
            => AddHistory(historyEntrypointName, buildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateEntry<IFinalizedOverridenRegionalizedCompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateExit<IFinalizedOverridenRegionalizedCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateUtils<IFinalizedOverridenRegionalizedCompositeStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => AddDeferredEvent<TEvent>(buildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder ICompositeStateInitialization<IFinalizedOverridenRegionalizedCompositeStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnInitialize(actionsAsync);

        IFinalizedOverridenRegionalizedCompositeStateBuilder ICompositeStateFinalization<IFinalizedOverridenRegionalizedCompositeStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnFinalize(actionsAsync);

        private CompositeStateBuilder UseDeferredEvent<TEvent>(OverridenDeferralBuildAction<TEvent> buildAction = null)
        {
            if (typeof(TEvent) == typeof(Completion))
                throw new DeferralDefinitionException(typeof(TEvent).GetEventName(), "Completion event cannot be deferred.", Vertex.Graph.Class);

            if (typeof(TEvent) == typeof(Finalize))
                throw new DeferralDefinitionException(typeof(TEvent).GetEventName(), "Exit event cannot be deferred.", Vertex.Graph.Class);

            if (typeof(TEvent).IsSubclassOf(typeof(TimeEvent)))
                throw new DeferralDefinitionException(typeof(TEvent).GetEventName(), "Time events cannot be deferred.", Vertex.Graph.Class);

            if (
                !Vertex.Deferrals.TryGetValue(typeof(TEvent).GetEventName(), out var deferralLogic) ||
                deferralLogic?.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"Deferral of event '{Event<TEvent>.Name}' not found in overriden state '{Vertex.Name}'", Vertex.Graph.Class);
            }
            
            var builder = new DeferralBuilder<TEvent>(Vertex, deferralLogic);
            
            buildAction?.Invoke(builder);

            return this;
        }

        IOverridenRegionalizedCompositeStateBuilder IStateUtilsOverrides<IOverridenRegionalizedCompositeStateBuilder>.UseDeferredEvent<TEvent>(OverridenDeferralBuildAction<TEvent> buildAction)
            => UseDeferredEvent(buildAction);

        IFinalizedOverridenCompositeStateBuilder IStateUtilsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseDeferredEvent<TEvent>(OverridenDeferralBuildAction<TEvent> buildAction)
            => UseDeferredEvent(buildAction);

        IFinalizedOverridenRegionalizedCompositeStateBuilder IStateUtilsOverrides<IFinalizedOverridenRegionalizedCompositeStateBuilder>.UseDeferredEvent<TEvent>(OverridenDeferralBuildAction<TEvent> buildAction)
            => UseDeferredEvent(buildAction);

        IBehaviorCompositeStateBuilder IStateDoActivity<IBehaviorCompositeStateBuilder>.AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction)
        {
            Builder.AddDoActivity<TActivity>(buildAction);
            return this;
        }

        IBehaviorInitializedCompositeStateBuilder IStateDoActivity<IBehaviorInitializedCompositeStateBuilder>.AddDoActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            Builder.AddDoActivity(activityBuildAction);
            return this;
        }

        IBehaviorInitializedCompositeStateBuilder IStateDoActivity<IBehaviorInitializedCompositeStateBuilder>.AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction)
        {
            Builder.AddDoActivity<TActivity>(buildAction);
            return this;
        }

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateDoActivity<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddDoActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            Builder.AddDoActivity(activityBuildAction);
            return this;
        }

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateDoActivity<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction)
        {
            Builder.AddDoActivity<TActivity>(buildAction);
            return this;
        }

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateDoActivity<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddDoActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            Builder.AddDoActivity(activityBuildAction);
            return this;
        }

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateDoActivity<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction)
        {
            Builder.AddDoActivity<TActivity>(buildAction);
            return this;
        }

        IBehaviorFinalizedCompositeStateBuilder IStateDoActivity<IBehaviorFinalizedCompositeStateBuilder>.AddDoActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            Builder.AddDoActivity(activityBuildAction);
            return this;
        }

        IBehaviorFinalizedCompositeStateBuilder IStateDoActivity<IBehaviorFinalizedCompositeStateBuilder>.AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction)
        {
            Builder.AddDoActivity<TActivity>(buildAction);
            return this;
        }

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateDoActivity<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddDoActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            Builder.AddDoActivity(activityBuildAction);
            return this;
        }

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateDoActivity<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction)
        {
            Builder.AddDoActivity<TActivity>(buildAction);
            return this;
        }

        IBehaviorOverridenCompositeStateBuilder IStateDoActivity<IBehaviorOverridenCompositeStateBuilder>.AddDoActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            Builder.AddDoActivity(activityBuildAction);
            return this;
        }

        IBehaviorOverridenCompositeStateBuilder IStateDoActivity<IBehaviorOverridenCompositeStateBuilder>.AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction)
        {
            Builder.AddDoActivity<TActivity>(buildAction);
            return this;
        }

        IBehaviorCompositeStateBuilder IStateDoActivity<IBehaviorCompositeStateBuilder>.AddDoActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            Builder.AddDoActivity(activityBuildAction);
            return this;
        }

        IBehaviorCompositeStateBuilder IStateDoAction<IBehaviorCompositeStateBuilder>.AddDoAction<TAction>(ActionBuildAction<TAction> buildAction)
        {
            Builder.AddDoAction<TAction>(buildAction);
            return this;
        }

        IBehaviorInitializedCompositeStateBuilder IStateDoAction<IBehaviorInitializedCompositeStateBuilder>.AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction)
        {
            Builder.AddDoAction(actionDelegate, buildAction);
            return this;
        }

        IBehaviorInitializedCompositeStateBuilder IStateDoAction<IBehaviorInitializedCompositeStateBuilder>.AddDoAction<TAction>(ActionBuildAction<TAction> buildAction)
        {
            Builder.AddDoAction<TAction>(buildAction);
            return this;
        }

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateDoAction<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction)
        {
            Builder.AddDoAction(actionDelegate, buildAction);
            return this;
        }

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateDoAction<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddDoAction<TAction>(ActionBuildAction<TAction> buildAction)
        {
            Builder.AddDoAction<TAction>(buildAction);
            return this;
        }

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateDoAction<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction)
        {
            Builder.AddDoAction(actionDelegate, buildAction);
            return this;
        }

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateDoAction<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddDoAction<TAction>(ActionBuildAction<TAction> buildAction)
        {
            Builder.AddDoAction<TAction>(buildAction);
            return this;
        }

        IBehaviorFinalizedCompositeStateBuilder IStateDoAction<IBehaviorFinalizedCompositeStateBuilder>.AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction)
        {
            Builder.AddDoAction(actionDelegate, buildAction);
            return this;
        }

        IBehaviorFinalizedCompositeStateBuilder IStateDoAction<IBehaviorFinalizedCompositeStateBuilder>.AddDoAction<TAction>(ActionBuildAction<TAction> buildAction)
        {
            Builder.AddDoAction<TAction>(buildAction);
            return this;
        }

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateDoAction<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction)
        {
            Builder.AddDoAction(actionDelegate, buildAction);
            return this;
        }

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateDoAction<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddDoAction<TAction>(ActionBuildAction<TAction> buildAction)
        {
            Builder.AddDoAction<TAction>(buildAction);
            return this;
        }

        IBehaviorOverridenCompositeStateBuilder IStateDoAction<IBehaviorOverridenCompositeStateBuilder>.AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction)
        {
            Builder.AddDoAction(actionDelegate, buildAction);
            return this;
        }

        IBehaviorOverridenCompositeStateBuilder IStateDoAction<IBehaviorOverridenCompositeStateBuilder>.AddDoAction<TAction>(ActionBuildAction<TAction> buildAction)
        {
            Builder.AddDoAction<TAction>(buildAction);
            return this;
        }

        IBehaviorCompositeStateBuilder IStateDoAction<IBehaviorCompositeStateBuilder>.AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction)
        {
            Builder.AddDoAction(actionDelegate, buildAction);
            return this;
        }

        IBehaviorCompositeStateBuilder IStateEntry<IBehaviorCompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync);

        IBehaviorCompositeStateBuilder IStateExit<IBehaviorCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync);

        IBehaviorCompositeStateBuilder IStateUtils<IBehaviorCompositeStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => AddDeferredEvent(buildAction);

        IBehaviorCompositeStateBuilder ICompositeStateInitialization<IBehaviorCompositeStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnInitialize(actionsAsync);

        IBehaviorCompositeStateBuilder ICompositeStateFinalization<IBehaviorCompositeStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnFinalize(actionsAsync);

        IBehaviorCompositeStateBuilder IStateHistory<IBehaviorCompositeStateBuilder>.AddHistory(string historyEntrypointName, HistoryBuildAction buildAction)
            => AddHistory(historyEntrypointName, buildAction);

        IBehaviorInitializedCompositeStateBuilder IStateEntry<IBehaviorInitializedCompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync);

        IBehaviorInitializedCompositeStateBuilder IStateExit<IBehaviorInitializedCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync);

        IBehaviorInitializedCompositeStateBuilder IStateUtils<IBehaviorInitializedCompositeStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => AddDeferredEvent(buildAction);

        IBehaviorInitializedCompositeStateBuilder ICompositeStateInitialization<IBehaviorInitializedCompositeStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnInitialize(actionsAsync);

        IBehaviorInitializedCompositeStateBuilder ICompositeStateFinalization<IBehaviorInitializedCompositeStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnFinalize(actionsAsync);

        IBehaviorInitializedCompositeStateBuilder IStateHistory<IBehaviorInitializedCompositeStateBuilder>.AddHistory(string historyEntrypointName,
            HistoryBuildAction buildAction)
            => AddHistory(historyEntrypointName, buildAction);

        IBehaviorFinalizedCompositeStateBuilder IStateMachineFinal<IBehaviorFinalizedCompositeStateBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName);

        IBehaviorFinalizedCompositeStateBuilder IStateEntry<IBehaviorFinalizedCompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync);

        IBehaviorFinalizedCompositeStateBuilder IStateExit<IBehaviorFinalizedCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync);

        IBehaviorFinalizedCompositeStateBuilder IStateUtils<IBehaviorFinalizedCompositeStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => AddDeferredEvent(buildAction);

        IBehaviorFinalizedCompositeStateBuilder ICompositeStateInitialization<IBehaviorFinalizedCompositeStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnInitialize(actionsAsync);

        IBehaviorFinalizedCompositeStateBuilder ICompositeStateFinalization<IBehaviorFinalizedCompositeStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnFinalize(actionsAsync);

        IBehaviorOverridenCompositeStateBuilder IStateEntry<IBehaviorOverridenCompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync);

        IBehaviorOverridenCompositeStateBuilder IStateExit<IBehaviorOverridenCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync);

        IBehaviorOverridenCompositeStateBuilder IStateUtils<IBehaviorOverridenCompositeStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => AddDeferredEvent(buildAction);

        IBehaviorOverridenCompositeStateBuilder IStateUtilsOverrides<IBehaviorOverridenCompositeStateBuilder>.UseDeferredEvent<TEvent>(OverridenDeferralBuildAction<TEvent> buildAction)
            => UseDeferredEvent(buildAction);

        IBehaviorOverridenCompositeStateBuilder ICompositeStateInitialization<IBehaviorOverridenCompositeStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnInitialize(actionsAsync);

        IBehaviorOverridenCompositeStateBuilder ICompositeStateFinalization<IBehaviorOverridenCompositeStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnFinalize(actionsAsync);

        IBehaviorOverridenCompositeStateBuilder IStateHistory<IBehaviorOverridenCompositeStateBuilder>.AddHistory(string historyEntrypointName, HistoryBuildAction buildAction)
            => AddHistory(historyEntrypointName, buildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateOrthogonalization<IBehaviorOverridenRegionalizedCompositeStateBuilder>.MakeOrthogonal(
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => MakeOrthogonal(orthogonalStateBuildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateMachineFinal<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateEntry<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateExit<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateUtils<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => AddDeferredEvent(buildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateUtilsOverrides<IBehaviorOverridenRegionalizedCompositeStateBuilder>.UseDeferredEvent<TEvent>(
            OverridenDeferralBuildAction<TEvent> buildAction)
            => UseDeferredEvent(buildAction);

        IBehaviorOverridenRegionalizedCompositeStateBuilder ICompositeStateInitialization<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnInitialize(actionsAsync);

        IBehaviorOverridenRegionalizedCompositeStateBuilder ICompositeStateFinalization<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnFinalize(actionsAsync);

        IBehaviorOverridenRegionalizedCompositeStateBuilder IStateHistory<IBehaviorOverridenRegionalizedCompositeStateBuilder>.AddHistory(string historyEntrypointName,
            HistoryBuildAction buildAction)
            => AddHistory(historyEntrypointName, buildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateMachineFinal<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateEntry<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateExit<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateUtils<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => AddDeferredEvent(buildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder IStateUtilsOverrides<IBehaviorFinalizedOverridenCompositeStateBuilder>.UseDeferredEvent<TEvent>(
            OverridenDeferralBuildAction<TEvent> buildAction)
            => UseDeferredEvent(buildAction);

        IBehaviorFinalizedOverridenCompositeStateBuilder ICompositeStateInitialization<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnInitialize(actionsAsync);

        IBehaviorFinalizedOverridenCompositeStateBuilder ICompositeStateFinalization<IBehaviorFinalizedOverridenCompositeStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnFinalize(actionsAsync);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateOrthogonalization<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.MakeOrthogonal(
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => MakeOrthogonal(orthogonalStateBuildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateEntry<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateExit<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateUtils<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddDeferredEvent<TEvent>(
            DeferralBuildAction<TEvent> buildAction)
            => AddDeferredEvent(buildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder IStateUtilsOverrides<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.UseDeferredEvent<TEvent>(
            OverridenDeferralBuildAction<TEvent> buildAction)
            => UseDeferredEvent(buildAction);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder ICompositeStateInitialization<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnInitialize(actionsAsync);

        IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder ICompositeStateFinalization<IBehaviorFinalizedOverridenRegionalizedCompositeStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnFinalize(actionsAsync);

        ICompositeStateBuilder IStateHistory<ICompositeStateBuilder>.AddHistory(string historyEntrypointName, HistoryBuildAction buildAction)
            => AddHistory(historyEntrypointName, buildAction);

        IOverridenRegionalizedCompositeStateBuilder IStateOrthogonalization<IOverridenRegionalizedCompositeStateBuilder>.MakeOrthogonal(OrthogonalStateBuildAction orthogonalStateBuildAction)
            => MakeOrthogonal(orthogonalStateBuildAction);

        IOverridenCompositeStateBuilder IStateUtilsOverrides<IOverridenCompositeStateBuilder>.UseDeferredEvent<TEvent>(OverridenDeferralBuildAction<TEvent> buildAction)
            => UseDeferredEvent(buildAction);

        IInitializedCompositeStateBuilder IStateEntry<IInitializedCompositeStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync);

        IInitializedCompositeStateBuilder IStateExit<IInitializedCompositeStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync);

        IInitializedCompositeStateBuilder IStateUtils<IInitializedCompositeStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => AddDeferredEvent(buildAction);

        IInitializedCompositeStateBuilder ICompositeStateInitialization<IInitializedCompositeStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnInitialize(actionsAsync);

        IInitializedCompositeStateBuilder ICompositeStateFinalization<IInitializedCompositeStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnFinalize(actionsAsync);

        IFinalizedCompositeStateBuilder IStateMachineFinal<IFinalizedCompositeStateBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName);
    }
}
