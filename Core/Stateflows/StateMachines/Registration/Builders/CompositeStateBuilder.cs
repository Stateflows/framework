using System;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using Stateflows.Common.Utilities;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal partial class CompositeStateBuilder : 
        ICompositeStateInitialBuilder, 
        ICompositeStateBuilder, 
        ICompositeStateBuilderInternal,
        ITypedCompositeStateBuilder,
        ITypedCompositeStateInitialBuilder
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

        #region Events
        public ICompositeStateBuilder AddOnEntry(StateActionDelegateAsync actionAsync)
        {
            Builder.AddOnEntry(actionAsync);
            return this;
        }
        public ICompositeStateBuilder AddOnInitialize(StateActionDelegateAsync actionAsync)
        {
            Builder.AddOnInitialize(actionAsync);
            return this;
        }

        public ICompositeStateBuilder AddOnExit(StateActionDelegateAsync actionAsync)
        {
            Builder.AddOnExit(actionAsync);
            return this;
        }
        #endregion

        #region Transitions
        public ICompositeStateBuilder AddTransition<TEvent>(string targetStateName, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
        {
            Builder.AddTransition<TEvent>(targetStateName, transitionBuildAction);
            return this;
        }

        public ICompositeStateBuilder AddDefaultTransition(string targetStateName, TransitionBuilderAction<Completion> buildAction = null)
            => AddTransition<Completion>(targetStateName, buildAction);

        public ICompositeStateBuilder AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> buildAction = null)
            where TEvent : Event, new()
            => AddTransition<TEvent>(Constants.DefaultTransitionTarget, buildAction);

        ITypedCompositeStateBuilder IStateTransitionsBuilderBase<ITypedCompositeStateBuilder>.AddTransition<TEvent>(string targetStateName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetStateName, transitionBuildAction) as ITypedCompositeStateBuilder;

        ITypedCompositeStateBuilder IStateTransitionsBuilderBase<ITypedCompositeStateBuilder>.AddDefaultTransition(string targetStateName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as ITypedCompositeStateBuilder;

        ITypedCompositeStateBuilder IStateTransitionsBuilderBase<ITypedCompositeStateBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ITypedCompositeStateBuilder;
        #endregion

        #region AddState
        public ICompositeStateBuilder AddState(string stateName, StateBuilderAction stateBuildAction = null)
        {
            if (Vertex.Graph.AllVertices.ContainsKey(stateName))
            {
                throw new Exception($"State '{stateName}' is already registered");
            }

            var vertex = new Vertex()
            {
                Name = stateName,
                Parent = Vertex,
                Graph = Vertex.Graph,
            };

            stateBuildAction?.Invoke(new StateBuilder(vertex, Services));

            Vertex.Vertices.Add(vertex.Name, vertex);
            Vertex.Graph.AllVertices.Add(vertex.Name, vertex);

            return this;
        }

        ITypedCompositeStateBuilder IStateMachineBuilderBase<ITypedCompositeStateBuilder>.AddState(string stateName, StateBuilderAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as ITypedCompositeStateBuilder;

        ITypedCompositeStateBuilder IStateMachineInitialBuilderBase<ITypedCompositeStateBuilder>.AddInitialState(string stateName, StateBuilderAction stateBuildAction)
            => AddInitialState(stateName, stateBuildAction) as ITypedCompositeStateBuilder;
        #endregion

        #region AddCompositeState
        public ICompositeStateBuilder AddCompositeState(string stateName, CompositeStateBuilderAction stateBuildAction)
        {
            var vertex = new Vertex()
            {
                Name = stateName,
                Parent = Vertex,
                Graph = Vertex.Graph,
            };

            stateBuildAction?.Invoke(new CompositeStateBuilder(vertex, Services));

            Vertex.Vertices.Add(vertex.Name, vertex);
            Vertex.Graph.AllVertices.Add(vertex.Name, vertex);

            return this;
        }

        public ICompositeStateBuilder AddInitialState(string stateName, StateBuilderAction stateBuildAction = null)
        {
            Vertex.InitialVertexName = stateName;
            return AddState(stateName, stateBuildAction);
        }

        public ICompositeStateBuilder AddInitialCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
        {
            Vertex.InitialVertexName = stateName;
            return AddCompositeState(stateName, compositeStateBuildAction);
        }

        ITypedCompositeStateBuilder IStateMachineBuilderBase<ITypedCompositeStateBuilder>.AddCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddCompositeState(stateName, compositeStateBuildAction) as ITypedCompositeStateBuilder;

        ITypedCompositeStateBuilder IStateMachineInitialBuilderBase<ITypedCompositeStateBuilder>.AddInitialCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddInitialCompositeState(stateName, compositeStateBuildAction) as ITypedCompositeStateBuilder;
        #endregion

        ICompositeStateInitialBuilder IStateEventsBuilderBase<ICompositeStateInitialBuilder>.AddOnEntry(StateActionDelegateAsync actionAsync)
            => AddOnEntry(actionAsync) as ICompositeStateInitialBuilder;

        ICompositeStateInitialBuilder IStateEventsBuilderBase<ICompositeStateInitialBuilder>.AddOnExit(StateActionDelegateAsync actionAsync)
            => AddOnExit(actionAsync) as ICompositeStateInitialBuilder;

        ICompositeStateInitialBuilder ICompositeStateEventsBuilderBase<ICompositeStateInitialBuilder>.AddOnInitialize(StateActionDelegateAsync actionAsync)
            => AddOnInitialize(actionAsync) as ICompositeStateInitialBuilder;

        ICompositeStateBuilder IStateMachineInitialBuilderBase<ICompositeStateBuilder>.AddInitialState(string stateName, StateBuilderAction stateBuildAction)
            => AddInitialState(stateName, stateBuildAction);

        ICompositeStateBuilder IStateMachineInitialBuilderBase<ICompositeStateBuilder>.AddInitialCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddInitialCompositeState(stateName, compositeStateBuildAction);

        ITypedCompositeStateBuilder ICompositeStateEventsBuilderBase<ITypedCompositeStateBuilder>.AddOnInitialize(StateActionDelegateAsync actionAsync)
            => AddOnInitialize(actionAsync) as ITypedCompositeStateBuilder;

        ICompositeStateInitialBuilder IStateTransitionsBuilderBase<ICompositeStateInitialBuilder>.AddTransition<TEvent>(string targetStateName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetStateName, transitionBuildAction) as ICompositeStateInitialBuilder;

        ICompositeStateInitialBuilder IStateTransitionsBuilderBase<ICompositeStateInitialBuilder>.AddDefaultTransition(string targetStateName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as ICompositeStateInitialBuilder;

        ICompositeStateInitialBuilder IStateTransitionsBuilderBase<ICompositeStateInitialBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ICompositeStateInitialBuilder;
    }
}
