using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Events;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal partial class StateBuilder : 
        IStateBuilder,
        ISubmachineStateBuilder, 
        ITypedStateBuilder,
        ISubmachineTypedStateBuilder,
        IInternal
    {
        public Vertex Vertex { get; }

        public IServiceCollection Services { get; }

        public StateBuilder(Vertex vertex, IServiceCollection services)
        {
            Vertex = vertex;
            Services = services;
        }

        #region Events
        [DebuggerHidden]
        public IStateBuilder AddOnInitialize(Func<IStateActionContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            actionAsync = actionAsync.AddStateMachineInvocationContext(Vertex.Graph);

            Vertex.Initialize.Actions.Add(async c =>
            {
                var context = new StateActionContext(c, Vertex, Constants.Entry);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    await c.Executor.Inspector.OnStateInitializeExceptionAsync(context, e);
                }
            }
            );

            return this;
        }

        [DebuggerHidden]
        public IStateBuilder AddOnFinalize(Func<IStateActionContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            actionAsync = actionAsync.AddStateMachineInvocationContext(Vertex.Graph);

            Vertex.Finalize.Actions.Add(async c =>
            {
                var context = new StateActionContext(c, Vertex, Constants.Entry);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    await c.Executor.Inspector.OnStateFinalizeExceptionAsync(context, e);
                }
            }
            );

            return this;
        }

        [DebuggerHidden]
        public IStateBuilder AddOnEntry(Func<IStateActionContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            actionAsync = actionAsync.AddStateMachineInvocationContext(Vertex.Graph);

            Vertex.Entry.Actions.Add(async c =>
                {
                    var context = new StateActionContext(c, Vertex, Constants.Entry);
                    try
                    {
                        await actionAsync(context);
                    }
                    catch (Exception e)
                    {
                        await c.Executor.Inspector.OnStateEntryExceptionAsync(context, e);
                    }
                }
            );

            return this;
        }

        [DebuggerHidden]
        public IStateBuilder AddOnExit(Func<IStateActionContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            actionAsync = actionAsync.AddStateMachineInvocationContext(Vertex.Graph);

            Vertex.Exit.Actions.Add(async c =>
                {
                    var context = new StateActionContext(c, Vertex, Constants.Exit);
                    try
                    {
                        await actionAsync(context);
                    }
                    catch (Exception e)
                    {
                        await c.Executor.Inspector.OnStateExitExceptionAsync(context, e);
                    }
                }
            );

            return this;
        }
        #endregion

        #region Utils
        [DebuggerHidden]
        public IStateBuilder AddDeferredEvent<TEvent>() where TEvent : Event, new()
        {
            if (typeof(TEvent) == typeof(Completion))
                throw new DeferralDefinitionException(EventInfo<TEvent>.Name, "Completion event cannot be deferred.", Vertex.Graph.Class);

            if (typeof(TEvent) == typeof(Exit))
                throw new DeferralDefinitionException(EventInfo<TEvent>.Name, "Exit event cannot be deferred.", Vertex.Graph.Class);

            if (typeof(TEvent).IsSubclassOf(typeof(TimeEvent)))
                throw new DeferralDefinitionException(EventInfo<TEvent>.Name, "Time events cannot be deferred.", Vertex.Graph.Class);

            Vertex.DeferredEvents.Add(EventInfo<TEvent>.Name);

            return this;
        }
        #endregion

        #region Transitions
        [DebuggerHidden]
        public IStateBuilder AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
        {
            var edge = new Edge()
            {
                Trigger = EventInfo<TEvent>.Name,
                TriggerType = typeof(TEvent),
                Graph = Vertex.Graph,
                SourceName = Vertex.Name,
                Source = Vertex,
                TargetName = targetVertexName,
            };

            if (Vertex.Edges.ContainsKey(edge.Name))
                if (targetVertexName == Constants.DefaultTransitionTarget)
                    throw new TransitionDefinitionException($"Internal transition in '{edge.SourceName}' triggered by '{edge.Trigger}' is already registered", Vertex.Graph.Class);
                else
                    if (edge.Trigger == Constants.CompletionEvent)
                        throw new TransitionDefinitionException($"Default transition from '{edge.SourceName}' to '{edge.TargetName}' is already registered", Vertex.Graph.Class);
                    else
                        throw new TransitionDefinitionException($"Default transition from '{edge.SourceName}' to '{edge.TargetName}' triggered by '{edge.Trigger}' is already registered", Vertex.Graph.Class);

            Vertex.Edges.Add(edge.Name, edge);
            Vertex.Graph.AllEdges.Add(edge.Identifier, edge);

            transitionBuildAction?.Invoke(new TransitionBuilder<TEvent>(edge));

            return this;
        }

        [DebuggerHidden]
        public IStateBuilder AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction = null)
            => AddTransition<Completion>(targetVertexName, transitionBuildAction);

        [DebuggerHidden]
        public IStateBuilder AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            => AddTransition<TEvent>(Constants.DefaultTransitionTarget, transitionBuildAction);

        [DebuggerHidden]
        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ITypedStateBuilder;

        [DebuggerHidden]
        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ITypedStateBuilder;

        [DebuggerHidden]
        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ITypedStateBuilder;

        [DebuggerHidden]
        ITypedStateBuilder IStateUtils<ITypedStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ITypedStateBuilder;
        #endregion

        #region Submachine
        [DebuggerHidden]
        public ISubmachineStateBuilder AddSubmachine(string submachineName, StateActionInitializationBuilder initializationBuilder = null)
        {
            Vertex.SubmachineName = submachineName;
            Vertex.SubmachineInitializationBuilder = initializationBuilder;

            return this;
        }

        [DebuggerHidden]
        ISubmachineTypedStateBuilder IStateSubmachine<ISubmachineTypedStateBuilder>.AddSubmachine(string submachineName, StateActionInitializationBuilder initializationBuilder)
            => AddSubmachine(submachineName, initializationBuilder) as ISubmachineTypedStateBuilder;

        [DebuggerHidden]
        ISubmachineStateBuilder IStateEntry<ISubmachineStateBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as ISubmachineStateBuilder;

        [DebuggerHidden]
        ISubmachineStateBuilder IStateExit<ISubmachineStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnExit(actionAsync) as ISubmachineStateBuilder;

        [DebuggerHidden]
        ISubmachineStateBuilder IStateUtils<ISubmachineStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ISubmachineStateBuilder;

        [DebuggerHidden]
        ISubmachineStateBuilder IStateTransitions<ISubmachineStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ISubmachineStateBuilder;

        [DebuggerHidden]
        ISubmachineStateBuilder IStateTransitions<ISubmachineStateBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ISubmachineStateBuilder;

        [DebuggerHidden]
        ISubmachineStateBuilder IStateTransitions<ISubmachineStateBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ISubmachineStateBuilder;

        [DebuggerHidden]
        ISubmachineTypedStateBuilder IStateUtils<ISubmachineTypedStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ISubmachineTypedStateBuilder;

        [DebuggerHidden]
        ISubmachineTypedStateBuilder IStateTransitions<ISubmachineTypedStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ISubmachineTypedStateBuilder;

        [DebuggerHidden]
        ISubmachineTypedStateBuilder IStateTransitions<ISubmachineTypedStateBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ISubmachineTypedStateBuilder;

        [DebuggerHidden]
        ISubmachineTypedStateBuilder IStateTransitions<ISubmachineTypedStateBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ISubmachineTypedStateBuilder;
        #endregion
    }
}

