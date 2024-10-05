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
        IFinalizedCompositeStateBuilder,
        IInitializedCompositeStateBuilder,
        IJunctionBuilder,
        IChoiceBuilder,
        IInternal,
        IBehaviorBuilder
    {
        public Vertex Vertex { get; }

        public IServiceCollection Services { get; }

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Vertex.Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Vertex.Graph.Version;

        public CompositeStateBuilder(Vertex vertex, IServiceCollection services)
        {
            Vertex = vertex;
            Services = services;
            Builder = new StateBuilder(Vertex, Services);
        }

        private StateBuilder Builder { get; set; }

        [DebuggerHidden]
        private IInitializedCompositeStateBuilder AddVertex(string stateName, VertexType type, Action<Vertex> vertexBuildAction = null)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                throw new StateDefinitionException(stateName, $"State name cannot be empty", Vertex.Graph.Class);
            }

            if (Vertex.Vertices.ContainsKey(stateName))
            {
                throw new StateDefinitionException(stateName, $"State '{stateName}' is already registered", Vertex.Graph.Class);
            }

            var vertex = new Vertex()
            {
                Name = stateName,
                Type = type,
                Parent = Vertex,
                Graph = Vertex.Graph,
            };

            vertexBuildAction?.Invoke(vertex);

            Vertex.Vertices.Add(vertex.Name, vertex);
            Vertex.Graph.AllVertices.Add(vertex.Identifier, vertex);

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
        public IInitializedCompositeStateBuilder AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)

        {
            Builder.AddElseTransition<TEvent>(targetStateName, transitionBuildAction);
            return this;
        }

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction = null)
            => AddTransition<CompletionEvent>(targetStateName, builder => transitionBuildAction?.Invoke(builder as IDefaultTransitionBuilder));

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            => AddElseTransition<CompletionEvent>(targetStateName, builder => transitionBuildAction?.Invoke(builder as IElseDefaultTransitionBuilder));

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
        public IInitializedCompositeStateBuilder AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddVertex(compositeStateName, VertexType.CompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex, Services)));

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddInitialState(string stateName, StateBuildAction stateBuildAction = null)
        {
            Vertex.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialState, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));
        }

        [DebuggerHidden]
        public IInitializedCompositeStateBuilder AddInitialCompositeState(string stateName, CompositeStateBuildAction compositeStateBuildAction)
        {
            Vertex.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialCompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex, Services)));
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
        void IPseudostateTransitions<IJunctionBuilder>.AddElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IChoiceBuilder IPseudostateTransitions<IChoiceBuilder>.AddTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IChoiceBuilder;

        [DebuggerHidden]
        void IPseudostateTransitions<IChoiceBuilder>.AddElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);
    }
}
