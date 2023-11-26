using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
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
        ITypedInitializedCompositeStateBuilder,
        ITypedFinalizedCompositeStateBuilder,
        ITypedCompositeStateBuilder,
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
        public IInitializedCompositeStateBuilder AddOnEntry(Func<IStateActionContext, Task> actionAsync)
        {
            Builder.AddOnEntry(actionAsync);
            return this;
        }

        public IInitializedCompositeStateBuilder AddOnInitialize(Func<IStateActionContext, Task> actionAsync)
        {
            Builder.AddOnInitialize(actionAsync);
            return this;
        }

        public IInitializedCompositeStateBuilder AddOnFinalize(Func<IStateActionContext, Task> actionAsync)
        {
            Builder.AddOnFinalize(actionAsync);
            return this;
        }

        public IInitializedCompositeStateBuilder AddOnExit(Func<IStateActionContext, Task> actionAsync)
        {
            Builder.AddOnExit(actionAsync);
            return this;
        }
        #endregion

        #region Utils
        public IInitializedCompositeStateBuilder AddDeferredEvent<TEvent>() where TEvent : Event, new()
        {
            Builder.AddDeferredEvent<TEvent>();
            return this;
        }
        #endregion

        #region Transitions
        public IInitializedCompositeStateBuilder AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
        {
            Builder.AddTransition<TEvent>(targetVertexName, transitionBuildAction);
            return this;
        }

        public IInitializedCompositeStateBuilder AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction = null)
            => AddTransition<Completion>(targetVertexName, transitionBuildAction);

        public IInitializedCompositeStateBuilder AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            => AddTransition<TEvent>(Constants.DefaultTransitionTarget, transitionBuildAction);

        ITypedInitializedCompositeStateBuilder IStateTransitions<ITypedInitializedCompositeStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ITypedInitializedCompositeStateBuilder;

        ITypedInitializedCompositeStateBuilder IStateTransitions<ITypedInitializedCompositeStateBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ITypedInitializedCompositeStateBuilder;

        ITypedInitializedCompositeStateBuilder IStateTransitions<ITypedInitializedCompositeStateBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ITypedInitializedCompositeStateBuilder;
        #endregion

        #region AddState
        public IInitializedCompositeStateBuilder AddState(string stateName, StateBuilderAction stateBuildAction = null)
            => AddVertex(stateName, VertexType.State, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));

        ITypedInitializedCompositeStateBuilder IStateMachine<ITypedInitializedCompositeStateBuilder>.AddState(string stateName, StateBuilderAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as ITypedInitializedCompositeStateBuilder;

        ITypedInitializedCompositeStateBuilder IStateMachineInitial<ITypedInitializedCompositeStateBuilder>.AddInitialState(string stateName, StateBuilderAction stateBuildAction)
            => AddInitialState(stateName, stateBuildAction) as ITypedInitializedCompositeStateBuilder;
        #endregion

        #region AddFinalState
        public IFinalizedCompositeStateBuilder AddFinalState(string stateName = FinalState.Name)
            => AddVertex(stateName, VertexType.FinalState) as IFinalizedCompositeStateBuilder;

        ITypedFinalizedCompositeStateBuilder IStateMachineFinal<ITypedFinalizedCompositeStateBuilder>.AddFinalState(string stateName)
            => AddVertex(stateName, VertexType.FinalState) as ITypedFinalizedCompositeStateBuilder;
        #endregion

        #region AddCompositeState
        public IInitializedCompositeStateBuilder AddCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddVertex(stateName, VertexType.CompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex, Services)));

        public IInitializedCompositeStateBuilder AddInitialState(string stateName, StateBuilderAction stateBuildAction = null)
        {
            Vertex.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialState, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));
        }

        public IInitializedCompositeStateBuilder AddInitialCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
        {
            Vertex.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialCompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex, Services)));
        }

        ITypedInitializedCompositeStateBuilder IStateMachine<ITypedInitializedCompositeStateBuilder>.AddCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddCompositeState(stateName, compositeStateBuildAction) as ITypedInitializedCompositeStateBuilder;

        ITypedInitializedCompositeStateBuilder IStateMachineInitial<ITypedInitializedCompositeStateBuilder>.AddInitialCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddInitialCompositeState(stateName, compositeStateBuildAction) as ITypedInitializedCompositeStateBuilder;
        #endregion

        ICompositeStateBuilder IStateEntry<ICompositeStateBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as ICompositeStateBuilder;

        ICompositeStateBuilder IStateExit<ICompositeStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnExit(actionAsync) as ICompositeStateBuilder;

        ICompositeStateBuilder ICompositeStateEvents<ICompositeStateBuilder>.AddOnInitialize(Func<IStateActionContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as ICompositeStateBuilder;

        ICompositeStateBuilder ICompositeStateEvents<ICompositeStateBuilder>.AddOnFinalize(Func<IStateActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as ICompositeStateBuilder;

        IInitializedCompositeStateBuilder IStateMachineInitial<IInitializedCompositeStateBuilder>.AddInitialState(string stateName, StateBuilderAction stateBuildAction)
            => AddInitialState(stateName, stateBuildAction);

        IInitializedCompositeStateBuilder IStateMachineInitial<IInitializedCompositeStateBuilder>.AddInitialCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddInitialCompositeState(stateName, compositeStateBuildAction);

        ICompositeStateBuilder IStateTransitions<ICompositeStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ICompositeStateBuilder;

        ICompositeStateBuilder IStateTransitions<ICompositeStateBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ICompositeStateBuilder;

        ICompositeStateBuilder IStateTransitions<ICompositeStateBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ICompositeStateBuilder;

        ICompositeStateBuilder IStateUtils<ICompositeStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ICompositeStateBuilder;

        ITypedInitializedCompositeStateBuilder IStateUtils<ITypedInitializedCompositeStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ITypedInitializedCompositeStateBuilder;

        ITypedCompositeStateBuilder IStateUtils<ITypedCompositeStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ITypedCompositeStateBuilder;

        ITypedCompositeStateBuilder IStateTransitions<ITypedCompositeStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ITypedCompositeStateBuilder;

        ITypedCompositeStateBuilder IStateTransitions<ITypedCompositeStateBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ITypedCompositeStateBuilder;

        ITypedCompositeStateBuilder IStateTransitions<ITypedCompositeStateBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ITypedCompositeStateBuilder;

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
