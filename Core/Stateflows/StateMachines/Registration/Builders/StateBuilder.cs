using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Registration;
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
        IBehaviorStateBuilder, 
        ITypedStateBuilder,
        IBehaviorTypedStateBuilder,
        IInternal,
        IBehaviorBuilder,
        IEmbeddedBehaviorBuilder
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
                    if (e is StateflowsException)
                    {
                        throw;
                    }
                    else
                    {
                        await c.Executor.Inspector.OnStateInitializeExceptionAsync(context, e);
                    }
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
                    if (e is StateflowsException)
                    {
                        throw;
                    }
                    else
                    {
                        await c.Executor.Inspector.OnStateFinalizeExceptionAsync(context, e);
                    }
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
                        if (e is StateflowsException)
                        {
                            throw;
                        }
                        else
                        {
                            await c.Executor.Inspector.OnStateEntryExceptionAsync(context, e);
                        }
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
                        if (e is StateflowsException)
                        {
                            throw;
                        }
                        else
                        {
                            await c.Executor.Inspector.OnStateExitExceptionAsync(context, e);
                        }
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

            if (typeof(TEvent) == typeof(FinalizationRequest))
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
            => AddTransitionInternal<TEvent>(targetVertexName, true, builder => transitionBuildAction?.Invoke(builder as IElseTransitionBuilder<TEvent>));

        [DebuggerHidden]
        public IStateBuilder AddDefaultTransition(string targetVertexName, DefaultTransitionBuildAction transitionBuildAction = null)
            => AddTransition<CompletionEvent>(targetVertexName, builder => transitionBuildAction?.Invoke(builder as IDefaultTransitionBuilder));

        [DebuggerHidden]
        public IStateBuilder AddElseDefaultTransition(string targetVertexName, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            => AddElseTransition<CompletionEvent>(targetVertexName, builder => transitionBuildAction?.Invoke(builder as IElseDefaultTransitionBuilder));

        [DebuggerHidden]
        public IStateBuilder AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            where TEvent : Event, new()
            => AddTransition<TEvent>(Constants.DefaultTransitionTarget, builder => transitionBuildAction?.Invoke(builder as IInternalTransitionBuilder<TEvent>));

        [DebuggerHidden]
        public IStateBuilder AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            where TEvent : Event, new()
            => AddElseTransition<TEvent>(Constants.DefaultTransitionTarget, builder => transitionBuildAction?.Invoke(builder as IElseInternalTransitionBuilder<TEvent>));

        [DebuggerHidden]
        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ITypedStateBuilder;

        [DebuggerHidden]
        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddElseTransition<TEvent>(string targetVertexName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition<TEvent>(targetVertexName, transitionBuildAction) as ITypedStateBuilder;

        [DebuggerHidden]
        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddDefaultTransition(string targetVertexName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ITypedStateBuilder;

        [DebuggerHidden]
        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddElseDefaultTransition(string targetVertexName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetVertexName, transitionBuildAction) as ITypedStateBuilder;

        [DebuggerHidden]
        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(builder => transitionBuildAction?.Invoke(builder)) as ITypedStateBuilder;

        [DebuggerHidden]
        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition<TEvent>(builder => transitionBuildAction?.Invoke(builder)) as ITypedStateBuilder;

        [DebuggerHidden]
        ITypedStateBuilder IStateUtils<ITypedStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ITypedStateBuilder;
        #endregion

        #region Submachine
        [DebuggerHidden]
        public IBehaviorStateBuilder AddSubmachine(string submachineName, EmbeddedBehaviorBuildAction buildAction, StateActionInitializationBuilder initializationBuilder = null)
        {
            Vertex.BehaviorType = BehaviorType.StateMachine;
            Vertex.BehaviorName = submachineName;
            Vertex.BehaviorInitializationBuilder = initializationBuilder;

            buildAction?.Invoke(this);

            return this;
        }

        [DebuggerHidden]
        IBehaviorTypedStateBuilder IStateSubmachine<IBehaviorTypedStateBuilder>.AddSubmachine(string submachineName, EmbeddedBehaviorBuildAction buildAction, StateActionInitializationBuilder initializationBuilder)
            => AddSubmachine(submachineName, buildAction, initializationBuilder) as IBehaviorTypedStateBuilder;
        #endregion

        #region DoActivity
        [DebuggerHidden]
        public IBehaviorStateBuilder AddDoActivity(string doActivityName, EmbeddedBehaviorBuildAction buildAction = null, StateActionInitializationBuilder initializationBuilder = null)
        {
            Vertex.BehaviorType = BehaviorType.Activity;
            Vertex.BehaviorName = doActivityName;
            Vertex.BehaviorInitializationBuilder = initializationBuilder;

            buildAction?.Invoke(this);

            return this;
        }

        [DebuggerHidden]
        IBehaviorTypedStateBuilder IStateDoActivity<IBehaviorTypedStateBuilder>.AddDoActivity(string doActivityName, EmbeddedBehaviorBuildAction buildAction, StateActionInitializationBuilder initializationBuilder)
            => AddDoActivity(doActivityName, buildAction, initializationBuilder) as IBehaviorTypedStateBuilder;
        #endregion

        [DebuggerHidden]
        IBehaviorStateBuilder IStateEntry<IBehaviorStateBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateExit<IBehaviorStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnExit(actionAsync) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateUtils<IBehaviorStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddElseTransition<TEvent>(string targetVertexName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition<TEvent>(targetVertexName, transitionBuildAction) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddDefaultTransition(string targetVertexName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddElseDefaultTransition(string targetVertexName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetVertexName, transitionBuildAction) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(builder => transitionBuildAction?.Invoke(builder)) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition<TEvent>(builder => transitionBuildAction?.Invoke(builder)) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorTypedStateBuilder IStateUtils<IBehaviorTypedStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IBehaviorTypedStateBuilder;

        [DebuggerHidden]
        IBehaviorTypedStateBuilder IStateTransitions<IBehaviorTypedStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as IBehaviorTypedStateBuilder;

        [DebuggerHidden]
        IBehaviorTypedStateBuilder IStateTransitions<IBehaviorTypedStateBuilder>.AddElseTransition<TEvent>(string targetVertexName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition<TEvent>(targetVertexName, transitionBuildAction) as IBehaviorTypedStateBuilder;

        [DebuggerHidden]
        IBehaviorTypedStateBuilder IStateTransitions<IBehaviorTypedStateBuilder>.AddDefaultTransition(string targetVertexName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as IBehaviorTypedStateBuilder;

        [DebuggerHidden]
        IBehaviorTypedStateBuilder IStateTransitions<IBehaviorTypedStateBuilder>.AddElseDefaultTransition(string targetVertexName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetVertexName, transitionBuildAction) as IBehaviorTypedStateBuilder;

        [DebuggerHidden]
        IBehaviorTypedStateBuilder IStateTransitions<IBehaviorTypedStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as IBehaviorTypedStateBuilder;

        [DebuggerHidden]
        IBehaviorTypedStateBuilder IStateTransitions<IBehaviorTypedStateBuilder>.AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition<TEvent>(transitionBuildAction) as IBehaviorTypedStateBuilder;

        public IEmbeddedBehaviorBuilder AddForwardedEvent<TEvent>(ForwardedEventBuildAction<TEvent> buildAction = null)
            where TEvent : Event, new()
            => AddInternalTransition<TEvent>(b =>
            {
                b.AddEffect(c =>
                {
                    if (c.TryLocateBehavior(Vertex.GetBehaviorId(c.StateMachine.Id), out var behavior))
                    {
                        _ = behavior.SendAsync(c.Event);
                    }

                    return Task.CompletedTask;
                });

                buildAction?.Invoke(b as IForwardedEventBuilder<TEvent>);
            }) as IEmbeddedBehaviorBuilder;

        public IEmbeddedBehaviorBuilder AddSubscription<TNotification>()
            where TNotification : Notification, new()
        {
            Vertex.BehaviorSubscriptions.Add(typeof(TNotification));
            
            return this;
        }
    }
}

