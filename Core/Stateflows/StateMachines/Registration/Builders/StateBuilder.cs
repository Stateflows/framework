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
using Stateflows.Common.Registration;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal partial class StateBuilder : 
        IStateBuilder,
        ISubmachineStateBuilder, 
        ITypedStateBuilder,
        ISubmachineTypedStateBuilder,
        IInternal,
        IBehaviorBuilder
    {
        public Vertex Vertex { get; }

        public IServiceCollection Services { get; }

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(nameof(StateMachine), Vertex.Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Vertex.Graph.Version;

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
            if (typeof(TEvent) == typeof(CompletionEvent))
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
        private IStateBuilder AddTransitionInternal<TEvent>(string targetVertexName, bool isElse, TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
        {
            var targetEdgeType = targetVertexName == Constants.DefaultTransitionTarget
                ? TriggerType.InternalTransition
                : TriggerType.Transition;

            var edge = new Edge()
            {
                Trigger = EventInfo<TEvent>.Name,
                TriggerType = typeof(TEvent),
                IsElse = isElse,
                Graph = Vertex.Graph,
                SourceName = Vertex.Name,
                Source = Vertex,
                TargetName = targetVertexName,
                Type = EventInfo<TEvent>.Name == Constants.CompletionEvent
                    ? TriggerType.DefaultTransition
                    : targetEdgeType
            };

            if (Vertex.Edges.ContainsKey(edge.Name))
                if (targetVertexName == Constants.DefaultTransitionTarget)
                    throw new TransitionDefinitionException($"Internal transition in '{edge.SourceName}' triggered by '{edge.Trigger}' is already registered", Vertex.Graph.Class);
                else
                    if (edge.Trigger == Constants.CompletionEvent)
                        throw new TransitionDefinitionException($"Default transition from '{edge.SourceName}' to '{edge.TargetName}' is already registered", Vertex.Graph.Class);
                    else
                        throw new TransitionDefinitionException($"Transition from '{edge.SourceName}' to '{edge.TargetName}' triggered by '{edge.Trigger}' is already registered", Vertex.Graph.Class);

            Vertex.Edges.Add(edge.Name, edge);
            Vertex.Graph.AllEdges.Add(edge);

            transitionBuildAction?.Invoke(new TransitionBuilder<TEvent>(edge));

            return this;
        }

        [DebuggerHidden]
        public IStateBuilder AddTransition<TEvent>(string targetVertexName, TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            => AddTransitionInternal<TEvent>(targetVertexName, false, transitionBuildAction);

        [DebuggerHidden]
        public IStateBuilder AddElseTransition<TEvent>(string targetVertexName, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            => AddTransitionInternal<TEvent>(targetVertexName, true, builder => transitionBuildAction?.Invoke(builder));

        [DebuggerHidden]
        public IStateBuilder AddDefaultTransition(string targetVertexName, TransitionBuildAction<CompletionEvent> transitionBuildAction = null)
            => AddTransition<CompletionEvent>(targetVertexName, transitionBuildAction);

        [DebuggerHidden]
        public IStateBuilder AddElseDefaultTransition(string targetVertexName, ElseTransitionBuildAction<CompletionEvent> transitionBuildAction = null)
            => AddElseTransition<CompletionEvent>(targetVertexName, transitionBuildAction);

        [DebuggerHidden]
        public IStateBuilder AddInternalTransition<TEvent>(TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            => AddTransition<TEvent>(Constants.DefaultTransitionTarget, transitionBuildAction);

        [DebuggerHidden]
        public IStateBuilder AddElseInternalTransition<TEvent>(ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            => AddElseTransition<TEvent>(Constants.DefaultTransitionTarget, transitionBuildAction);

        [DebuggerHidden]
        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ITypedStateBuilder;

        [DebuggerHidden]
        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddElseTransition<TEvent>(string targetVertexName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition<TEvent>(targetVertexName, transitionBuildAction) as ITypedStateBuilder;

        [DebuggerHidden]
        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuildAction<CompletionEvent> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ITypedStateBuilder;

        [DebuggerHidden]
        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddElseDefaultTransition(string targetVertexName, ElseTransitionBuildAction<CompletionEvent> transitionBuildAction)
            => AddElseDefaultTransition(targetVertexName, transitionBuildAction) as ITypedStateBuilder;

        [DebuggerHidden]
        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddInternalTransition<TEvent>(TransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ITypedStateBuilder;

        [DebuggerHidden]
        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddElseInternalTransition<TEvent>(ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition<TEvent>(transitionBuildAction) as ITypedStateBuilder;

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
        ISubmachineStateBuilder IStateTransitions<ISubmachineStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ISubmachineStateBuilder;

        [DebuggerHidden]
        ISubmachineStateBuilder IStateTransitions<ISubmachineStateBuilder>.AddElseTransition<TEvent>(string targetVertexName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition<TEvent>(targetVertexName, transitionBuildAction) as ISubmachineStateBuilder;

        [DebuggerHidden]
        ISubmachineStateBuilder IStateTransitions<ISubmachineStateBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuildAction<CompletionEvent> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ISubmachineStateBuilder;

        [DebuggerHidden]
        ISubmachineStateBuilder IStateTransitions<ISubmachineStateBuilder>.AddElseDefaultTransition(string targetVertexName, ElseTransitionBuildAction<CompletionEvent> transitionBuildAction)
            => AddElseDefaultTransition(targetVertexName, transitionBuildAction) as ISubmachineStateBuilder;

        [DebuggerHidden]
        ISubmachineStateBuilder IStateTransitions<ISubmachineStateBuilder>.AddInternalTransition<TEvent>(TransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ISubmachineStateBuilder;

        [DebuggerHidden]
        ISubmachineStateBuilder IStateTransitions<ISubmachineStateBuilder>.AddElseInternalTransition<TEvent>(ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition<TEvent>(transitionBuildAction) as ISubmachineStateBuilder;

        [DebuggerHidden]
        ISubmachineTypedStateBuilder IStateUtils<ISubmachineTypedStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ISubmachineTypedStateBuilder;

        [DebuggerHidden]
        ISubmachineTypedStateBuilder IStateTransitions<ISubmachineTypedStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ISubmachineTypedStateBuilder;

        [DebuggerHidden]
        ISubmachineTypedStateBuilder IStateTransitions<ISubmachineTypedStateBuilder>.AddElseTransition<TEvent>(string targetVertexName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition<TEvent>(targetVertexName, transitionBuildAction) as ISubmachineTypedStateBuilder;

        [DebuggerHidden]
        ISubmachineTypedStateBuilder IStateTransitions<ISubmachineTypedStateBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuildAction<CompletionEvent> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ISubmachineTypedStateBuilder;

        [DebuggerHidden]
        ISubmachineTypedStateBuilder IStateTransitions<ISubmachineTypedStateBuilder>.AddElseDefaultTransition(string targetVertexName, ElseTransitionBuildAction<CompletionEvent> transitionBuildAction)
            => AddElseDefaultTransition(targetVertexName, transitionBuildAction) as ISubmachineTypedStateBuilder;

        [DebuggerHidden]
        ISubmachineTypedStateBuilder IStateTransitions<ISubmachineTypedStateBuilder>.AddInternalTransition<TEvent>(TransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ISubmachineTypedStateBuilder;

        [DebuggerHidden]
        ISubmachineTypedStateBuilder IStateTransitions<ISubmachineTypedStateBuilder>.AddElseInternalTransition<TEvent>(ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition<TEvent>(transitionBuildAction) as ISubmachineTypedStateBuilder;
        #endregion
    }
}

