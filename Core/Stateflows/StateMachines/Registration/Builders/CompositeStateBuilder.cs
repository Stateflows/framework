using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using Stateflows.StateMachines.Exceptions;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class CompositeStateBuilder : 
        ICompositeStateInitialBuilder,
        IFinalizedCompositeStateBuilder,
        ICompositeStateBuilder,
        ITypedCompositeStateBuilder,
        ITypedFinalizedCompositeStateBuilder,
        ITypedCompositeStateInitialBuilder,
        IInternal
    {
        public Vertex Vertex { get; }

        public IServiceCollection Services { get; }

        public CompositeStateBuilder(Vertex vertex, IServiceCollection services)
        {
            Vertex = vertex;
            Services = services;
            Builder = new StateBuilder(Vertex, Services);
        }

        private StateBuilder Builder { get; set; }

        private ICompositeStateBuilder AddVertex(string stateName, VertexType type, Action<Vertex> vertexBuildAction = null)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                throw new StateDefinitionException(stateName, $"State name cannot be empty");
            }

            if (Vertex.Vertices.ContainsKey(stateName))
            {
                throw new StateDefinitionException(stateName, $"State '{stateName}' is already registered");
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
        public ICompositeStateBuilder AddOnEntry(Func<IStateActionContext, Task> actionAsync)
        {
            Builder.AddOnEntry(actionAsync);
            return this;
        }

        public ICompositeStateBuilder AddOnInitialize(Func<IStateActionContext, Task> actionAsync)
        {
            Builder.AddOnInitialize(actionAsync);
            return this;
        }

        public ICompositeStateBuilder AddOnFinalize(Func<IStateActionContext, Task> actionAsync)
        {
            Builder.AddOnFinalize(actionAsync);
            return this;
        }

        public ICompositeStateBuilder AddOnExit(Func<IStateActionContext, Task> actionAsync)
        {
            Builder.AddOnExit(actionAsync);
            return this;
        }
        #endregion

        #region Utils
        public ICompositeStateBuilder AddDeferredEvent<TEvent>() where TEvent : Event, new()
        {
            Builder.AddDeferredEvent<TEvent>();
            return this;
        }
        #endregion

        #region Transitions
        public ICompositeStateBuilder AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
        {
            Builder.AddTransition<TEvent>(targetVertexName, transitionBuildAction);
            return this;
        }

        public ICompositeStateBuilder AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction = null)
            => AddTransition<Completion>(targetVertexName, transitionBuildAction);

        public ICompositeStateBuilder AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            => AddTransition<TEvent>(Constants.DefaultTransitionTarget, transitionBuildAction);

        ITypedCompositeStateBuilder IStateTransitions<ITypedCompositeStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ITypedCompositeStateBuilder;

        ITypedCompositeStateBuilder IStateTransitions<ITypedCompositeStateBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ITypedCompositeStateBuilder;

        ITypedCompositeStateBuilder IStateTransitions<ITypedCompositeStateBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ITypedCompositeStateBuilder;
        #endregion

        #region AddState
        public ICompositeStateBuilder AddState(string stateName, StateBuilderAction stateBuildAction = null)
            => AddVertex(stateName, VertexType.State, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));

        ITypedCompositeStateBuilder IStateMachine<ITypedCompositeStateBuilder>.AddState(string stateName, StateBuilderAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as ITypedCompositeStateBuilder;

        ITypedCompositeStateBuilder IStateMachineInitial<ITypedCompositeStateBuilder>.AddInitialState(string stateName, StateBuilderAction stateBuildAction)
            => AddInitialState(stateName, stateBuildAction) as ITypedCompositeStateBuilder;
        #endregion

        #region AddFinalState
        public IFinalizedCompositeStateBuilder AddFinalState(string stateName = FinalState.Name)
            => AddVertex(stateName, VertexType.FinalState) as IFinalizedCompositeStateBuilder;

        ITypedFinalizedCompositeStateBuilder IStateMachineFinal<ITypedFinalizedCompositeStateBuilder>.AddFinalState(string stateName)
            => AddVertex(stateName, VertexType.FinalState) as ITypedFinalizedCompositeStateBuilder;
        #endregion

        #region AddCompositeState
        public ICompositeStateBuilder AddCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddVertex(stateName, VertexType.CompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex, Services)));

        public ICompositeStateBuilder AddInitialState(string stateName, StateBuilderAction stateBuildAction = null)
        {
            Vertex.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialState, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));
        }

        public ICompositeStateBuilder AddInitialCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
        {
            Vertex.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialCompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex, Services)));
        }

        ITypedCompositeStateBuilder IStateMachine<ITypedCompositeStateBuilder>.AddCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddCompositeState(stateName, compositeStateBuildAction) as ITypedCompositeStateBuilder;

        ITypedCompositeStateBuilder IStateMachineInitial<ITypedCompositeStateBuilder>.AddInitialCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddInitialCompositeState(stateName, compositeStateBuildAction) as ITypedCompositeStateBuilder;
        #endregion

        ICompositeStateInitialBuilder IStateEntry<ICompositeStateInitialBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as ICompositeStateInitialBuilder;

        ICompositeStateInitialBuilder IStateExit<ICompositeStateInitialBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnExit(actionAsync) as ICompositeStateInitialBuilder;

        ICompositeStateInitialBuilder ICompositeStateEvents<ICompositeStateInitialBuilder>.AddOnInitialize(Func<IStateActionContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as ICompositeStateInitialBuilder;

        ICompositeStateInitialBuilder ICompositeStateEvents<ICompositeStateInitialBuilder>.AddOnFinalize(Func<IStateActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as ICompositeStateInitialBuilder;

        ICompositeStateBuilder IStateMachineInitial<ICompositeStateBuilder>.AddInitialState(string stateName, StateBuilderAction stateBuildAction)
            => AddInitialState(stateName, stateBuildAction);

        ICompositeStateBuilder IStateMachineInitial<ICompositeStateBuilder>.AddInitialCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddInitialCompositeState(stateName, compositeStateBuildAction);

        ICompositeStateInitialBuilder IStateTransitions<ICompositeStateInitialBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ICompositeStateInitialBuilder;

        ICompositeStateInitialBuilder IStateTransitions<ICompositeStateInitialBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ICompositeStateInitialBuilder;

        ICompositeStateInitialBuilder IStateTransitions<ICompositeStateInitialBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ICompositeStateInitialBuilder;

        ICompositeStateInitialBuilder IStateUtils<ICompositeStateInitialBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ICompositeStateInitialBuilder;

        ITypedCompositeStateBuilder IStateUtils<ITypedCompositeStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ITypedCompositeStateBuilder;

        ITypedCompositeStateInitialBuilder IStateUtils<ITypedCompositeStateInitialBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ITypedCompositeStateInitialBuilder;

        ITypedCompositeStateInitialBuilder IStateTransitions<ITypedCompositeStateInitialBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ITypedCompositeStateInitialBuilder;

        ITypedCompositeStateInitialBuilder IStateTransitions<ITypedCompositeStateInitialBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ITypedCompositeStateInitialBuilder;

        ITypedCompositeStateInitialBuilder IStateTransitions<ITypedCompositeStateInitialBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ITypedCompositeStateInitialBuilder;

        IFinalizedCompositeStateBuilder IStateEntry<IFinalizedCompositeStateBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as IFinalizedCompositeStateBuilder;

        IFinalizedCompositeStateBuilder IStateExit<IFinalizedCompositeStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as IFinalizedCompositeStateBuilder;

        IFinalizedCompositeStateBuilder IStateUtils<IFinalizedCompositeStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IFinalizedCompositeStateBuilder;

        IFinalizedCompositeStateBuilder IStateTransitions<IFinalizedCompositeStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as IFinalizedCompositeStateBuilder;

        IFinalizedCompositeStateBuilder IStateTransitions<IFinalizedCompositeStateBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as IFinalizedCompositeStateBuilder;

        IFinalizedCompositeStateBuilder IStateTransitions<IFinalizedCompositeStateBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as IFinalizedCompositeStateBuilder;

        ITypedFinalizedCompositeStateBuilder IStateUtils<ITypedFinalizedCompositeStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ITypedFinalizedCompositeStateBuilder;

        ITypedFinalizedCompositeStateBuilder IStateTransitions<ITypedFinalizedCompositeStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ITypedFinalizedCompositeStateBuilder;

        ITypedFinalizedCompositeStateBuilder IStateTransitions<ITypedFinalizedCompositeStateBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ITypedFinalizedCompositeStateBuilder;

        ITypedFinalizedCompositeStateBuilder IStateTransitions<ITypedFinalizedCompositeStateBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ITypedFinalizedCompositeStateBuilder;

        IFinalizedCompositeStateBuilder ICompositeStateEvents<IFinalizedCompositeStateBuilder>.AddOnInitialize(Func<IStateActionContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IFinalizedCompositeStateBuilder;

        IFinalizedCompositeStateBuilder ICompositeStateEvents<IFinalizedCompositeStateBuilder>.AddOnFinalize(Func<IStateActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IFinalizedCompositeStateBuilder;
    }
}
