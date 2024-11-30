using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Registration;
using Stateflows.StateMachines.Events;
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
        IFinalizedCompositeStateBuilder,
        IFinalizedOverridenCompositeStateBuilder,
        IInitializedCompositeStateBuilder,
        IJunctionBuilder,
        IOverridenJunctionBuilder,
        IChoiceBuilder,
        IOverridenChoiceBuilder,
        IInternal,
        IBehaviorBuilder
    {
        public Region Region { get; }

        public IServiceCollection Services { get; }

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Region.Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Region.Graph.Version;

        public CompositeStateBuilder(Region region, IServiceCollection services)
        {
            Region = region;
            Services = services;
            Builder = new StateBuilder(Region.ParentVertex, Services);
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

            return this;
        }

        #region Events
        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddOnEntry(Func<IStateActionContext, Task> actionAsync)
        {
            Builder.AddOnEntry(actionAsync);
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

        public IInitializedCompositeStateBuilder AddOnExit(Func<IStateActionContext, Task> actionAsync)
        {
            Builder.AddOnExit(actionAsync);
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
            => AddVertex(stateName, VertexType.State, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateMachine<IOverridenCompositeStateBuilder>.AddCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateMachine<IOverridenCompositeStateBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateMachine<IOverridenCompositeStateBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction) as IOverridenCompositeStateBuilder;

        #endregion

        #region AddFinalState
        [DebuggerHidden]
        public IFinalizedCompositeStateBuilder AddFinalState(string finalStateName = FinalState.Name)
            => AddVertex(finalStateName, VertexType.FinalState) as IFinalizedCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateMachineFinal<IFinalizedCompositeStateBuilder>.AddFinalState(string finalStateName)
            => AddVertex(finalStateName, VertexType.FinalState) as IFinalizedCompositeStateBuilder;
        #endregion

        #region AddCompositeState
        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateMachine<IOverridenCompositeStateBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddVertex(compositeStateName, VertexType.CompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion, Services)));

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddInitialState(string stateName, StateBuildAction stateBuildAction = null)
        {
            Region.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialState, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));
        }

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddInitialCompositeState(string stateName, CompositeStateBuildAction compositeStateBuildAction)
        {
            Region.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialCompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion, Services)));
        }
        #endregion

        [DebuggerHidden]
        ICompositeStateBuilder IStateEntry<ICompositeStateBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as ICompositeStateBuilder;

        [DebuggerHidden]
        ICompositeStateBuilder IStateExit<ICompositeStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnExit(actionAsync) as ICompositeStateBuilder;

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
        IFinalizedCompositeStateBuilder IStateEntry<IFinalizedCompositeStateBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as IFinalizedCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedCompositeStateBuilder IStateExit<IFinalizedCompositeStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as IFinalizedCompositeStateBuilder;

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
            => AddVertex(junctionName, VertexType.Junction, vertex => junctionBuildAction?.Invoke(new StateBuilder(vertex, Services)));

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddVertex(choiceName, VertexType.Choice, vertex => choiceBuildAction?.Invoke(new StateBuilder(vertex, Services)));

        [DebuggerHidden]
        IJunctionBuilder IPseudostateTransitions<IJunctionBuilder>.AddTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IJunctionBuilder;

        [DebuggerHidden]
        void IPseudostateTransitions<IOverridenChoiceBuilder>.AddElseTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IOverridenChoiceBuilder IPseudostateTransitions<IOverridenChoiceBuilder>.AddTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IOverridenChoiceBuilder;

        [DebuggerHidden]
        void IPseudostateTransitions<IOverridenJunctionBuilder>.AddElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);
        
        [DebuggerHidden]
        IOverridenJunctionBuilder IPseudostateTransitions<IOverridenJunctionBuilder>.AddTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IOverridenJunctionBuilder;

        [DebuggerHidden]
        void IPseudostateTransitions<IJunctionBuilder>.AddElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IChoiceBuilder IPseudostateTransitions<IChoiceBuilder>.AddTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IChoiceBuilder;

        [DebuggerHidden]
        void IPseudostateTransitions<IChoiceBuilder>.AddElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateEntry<IOverridenCompositeStateBuilder>.AddOnEntry(
            Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateExit<IOverridenCompositeStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnExit(actionAsync) as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IOverridenCompositeStateBuilder IStateUtils<IOverridenCompositeStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseInternalTransition<TEvent>(
            InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        IFinalizedOverridenCompositeStateBuilder IStateTransitionsOverrides<IFinalizedOverridenCompositeStateBuilder>.UseTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction) as IFinalizedOverridenCompositeStateBuilder;

        [DebuggerHidden]
        public IOverridenCompositeStateBuilder UseTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
        {
            Builder.UseTransition(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        public IOverridenCompositeStateBuilder UseDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
        {
            Builder.UseDefaultTransition(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        public IOverridenCompositeStateBuilder UseInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
        {
            Builder.UseInternalTransition(transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        public IOverridenCompositeStateBuilder UseElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
        {
            Builder.UseElseTransition(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        public IOverridenCompositeStateBuilder UseElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
        {
            Builder.UseElseDefaultTransition(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        public IOverridenCompositeStateBuilder UseElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
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
            
            stateBuildAction?.Invoke(new StateBuilder(vertex, Services));

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
            
            compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion, Services));

            return this;
        }

        public IOverridenCompositeStateBuilder UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
        {
            if (!Region.Vertices.TryGetValue(junctionName, out var vertex) || vertex.Type != VertexType.Junction || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Junction '{junctionName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            junctionBuildAction?.Invoke(new StateBuilder(vertex, Services));

            return this;
        }

        public IOverridenCompositeStateBuilder UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
        {
            if (!Region.Vertices.TryGetValue(choiceName, out var vertex) || vertex.Type != VertexType.Choice || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Choice '{choiceName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            choiceBuildAction?.Invoke(new StateBuilder(vertex, Services));

            return this;
        }

        IFinalizedOverridenCompositeStateBuilder IStateMachineFinal<IFinalizedOverridenCompositeStateBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName) as IFinalizedOverridenCompositeStateBuilder;

        IFinalizedOverridenCompositeStateBuilder IStateEntry<IFinalizedOverridenCompositeStateBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as IFinalizedOverridenCompositeStateBuilder;

        IFinalizedOverridenCompositeStateBuilder IStateExit<IFinalizedOverridenCompositeStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnExit(actionAsync) as IFinalizedOverridenCompositeStateBuilder;

        IFinalizedOverridenCompositeStateBuilder IStateUtils<IFinalizedOverridenCompositeStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IFinalizedOverridenCompositeStateBuilder;

        IOverridenJunctionBuilder IPseudostateTransitionsOverrides<IOverridenJunctionBuilder>.UseTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenJunctionBuilder;

        void IPseudostateTransitionsOverrides<IOverridenChoiceBuilder>.UseElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);

        IOverridenChoiceBuilder IPseudostateTransitionsOverrides<IOverridenChoiceBuilder>.UseTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenChoiceBuilder;

        void IPseudostateTransitionsOverrides<IOverridenJunctionBuilder>.UseElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);
    }
}
