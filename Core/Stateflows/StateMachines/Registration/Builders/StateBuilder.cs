﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Registration;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class StateBuilder : 
        IStateBuilder,
        IOverridenStateBuilder,
        IOverridenRegionalizedStateBuilder,
        IJunctionBuilder,
        IOverridenJunctionBuilder,
        IChoiceBuilder,
        IOverridenChoiceBuilder,
        IForkBuilder,
        IOverridenForkBuilder,
        IJoinBuilder,
        IOverridenJoinBuilder,
        IBehaviorStateBuilder,
        IBehaviorOverridenStateBuilder,
        IBehaviorOverridenRegionalizedStateBuilder,
        IVertexBuilder,
        IBehaviorBuilder,
        IEmbeddedBehaviorBuilder,
        IGraphBuilder
    {
        public Vertex Vertex { get; }

        public Graph Graph => Vertex.Graph;

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Vertex.Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Vertex.Graph.Version;

        public StateBuilder(Vertex vertex)
        {
            Vertex = vertex;
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
                await actionAsync(context);
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
                await actionAsync(context);
            }
            );

            return this;
        }

        [DebuggerHidden]
        public IStateBuilder AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
        {
            foreach (var actionAsync in actionsAsync)
            {
                actionAsync.ThrowIfNull(nameof(actionAsync));

                var actionHandler = actionAsync.AddStateMachineInvocationContext(Vertex.Graph);

                Vertex.Entry.Actions.Add(async c =>
                    {
                        var context = new StateActionContext(c, Vertex, Constants.Entry);
                        await actionHandler(context);
                    }
                );
            }

            return this;
        }

        [DebuggerHidden]
        public IStateBuilder AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
        {
            foreach (var actionAsync in actionsAsync)
            {
                actionAsync.ThrowIfNull(nameof(actionAsync));

                var actionHandler = actionAsync.AddStateMachineInvocationContext(Vertex.Graph);

                Vertex.Exit.Actions.Add(async c =>
                    {
                        var context = new StateActionContext(c, Vertex, Constants.Exit);
                        await actionHandler(context);
                    }
                );
            }

            return this;
        }
        #endregion

        #region Utils
        [DebuggerHidden]
        public IStateBuilder AddDeferredEvent<TEvent>()
        {
            if (typeof(TEvent) == typeof(Completion))
                throw new DeferralDefinitionException(typeof(TEvent).GetEventName(), "Completion event cannot be deferred.", Vertex.Graph.Class);

            if (typeof(TEvent) == typeof(Finalize))
                throw new DeferralDefinitionException(typeof(TEvent).GetEventName(), "Exit event cannot be deferred.", Vertex.Graph.Class);

            if (typeof(TEvent).IsSubclassOf(typeof(TimeEvent)))
                throw new DeferralDefinitionException(typeof(TEvent).GetEventName(), "Time events cannot be deferred.", Vertex.Graph.Class);

            Vertex.DeferredEvents.Add(typeof(TEvent).GetEventName());

            return this;
        }
        #endregion

        #region Transitions
        [DebuggerHidden]
        private IStateBuilder AddTransitionInternal<TEvent>(string targetStateName, bool isElse, TransitionBuildAction<TEvent> transitionBuildAction = null)
        {
            var targetEdgeType = targetStateName == Constants.DefaultTransitionTarget
                ? EdgeType.InternalTransition
                : EdgeType.Transition;

            var triggerType = typeof(TEvent);

            var trigger = Event.GetName(triggerType);

            var triggerDescriptor = isElse
                ? $"{trigger}|else"
                : trigger;

            var edge = new Edge()
            {
                Trigger = trigger,
                TriggerType = typeof(TEvent),
                IsElse = isElse,
                Graph = Vertex.Graph,
                SourceName = Vertex.Name,
                Source = Vertex,
                TargetName = targetStateName,
                Type = typeof(TEvent).GetEventName() == Constants.Completion
                    ? EdgeType.DefaultTransition
                    : targetEdgeType,
                Name = $"{Vertex.Name}-{triggerDescriptor}->{targetStateName}",
            };

            if (typeof(Exception).IsAssignableFrom(edge.TriggerType))
            {
                edge.PolymorphicTriggers = true;
            }

            if (!Vertex.Edges.TryAdd(edge.Name, edge))
                if (targetStateName == Constants.DefaultTransitionTarget)
                    throw new TransitionDefinitionException($"Internal transition in '{edge.SourceName}' triggered by '{edge.Trigger}' is already registered", Vertex.Graph.Class);
                else
                    if (edge.Trigger == Constants.Completion)
                        throw new TransitionDefinitionException($"Default transition from '{edge.SourceName}' to '{edge.TargetName}' is already registered", Vertex.Graph.Class);
                    else
                        throw new TransitionDefinitionException($"Transition from '{edge.SourceName}' to '{edge.TargetName}' triggered by '{edge.Trigger}' is already registered", Vertex.Graph.Class);

            Vertex.Graph.AllEdges.Add(edge);

            transitionBuildAction?.Invoke(new TransitionBuilder<TEvent>(edge));
            
            Vertex.Graph.VisitingTasks.Add(visitor => visitor.TransitionAddedAsync<TEvent>(Vertex.Graph.Name, Vertex.Graph.Version, edge.SourceName, targetStateName == Constants.DefaultTransitionTarget ? edge.TargetName : null));

            return this;
        }

        [DebuggerHidden]
        public IStateBuilder AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction = null)
            => AddTransitionInternal(targetStateName, false, transitionBuildAction);
        
        [DebuggerHidden]
        IJoinBuilder IPseudostateTransitions<IJoinBuilder>.AddTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IJoinBuilder;

        [DebuggerHidden]
        void IPseudostateElseTransitions<IJoinBuilder>.AddElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IOverridenJoinBuilder IPseudostateTransitions<IOverridenJoinBuilder>.AddTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IOverridenJoinBuilder;

        [DebuggerHidden]
        void IPseudostateElseTransitions<IOverridenJoinBuilder>.AddElseTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitions<IBehaviorOverridenRegionalizedStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitions<IBehaviorOverridenRegionalizedStateBuilder>.AddInternalTransition<TEvent>(
            InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitions<IBehaviorOverridenRegionalizedStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitions<IBehaviorOverridenRegionalizedStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitions<IBehaviorOverridenRegionalizedStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitions<IBehaviorOverridenRegionalizedStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitions<IOverridenRegionalizedStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitions<IOverridenRegionalizedStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitions<IOverridenRegionalizedStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitions<IOverridenRegionalizedStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitions<IOverridenRegionalizedStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitions<IOverridenRegionalizedStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitions<IBehaviorOverridenStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitions<IBehaviorOverridenStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitions<IBehaviorOverridenStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitions<IBehaviorOverridenStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;
        
        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitions<IBehaviorOverridenStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitions<IBehaviorOverridenStateBuilder>.AddTransition<TEvent>(
            string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IOverridenStateBuilder IStateTransitions<IOverridenStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IOverridenStateBuilder;

        [DebuggerHidden]
        IOverridenStateBuilder IStateTransitions<IOverridenStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction) as IOverridenStateBuilder;

        [DebuggerHidden]
        IOverridenStateBuilder IStateTransitions<IOverridenStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction) as IOverridenStateBuilder;

        [DebuggerHidden]
        IOverridenStateBuilder IStateTransitions<IOverridenStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenStateBuilder;

        [DebuggerHidden]
        IOverridenStateBuilder IStateTransitions<IOverridenStateBuilder>.AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction) as IOverridenStateBuilder;

        [DebuggerHidden]
        public IStateBuilder AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            => AddTransitionInternal<TEvent>(targetStateName, true, builder => transitionBuildAction?.Invoke(builder as IElseTransitionBuilder<TEvent>));

        [DebuggerHidden]
        IOverridenStateBuilder IStateTransitions<IOverridenStateBuilder>.AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction) as IOverridenStateBuilder;

        [DebuggerHidden]
        public IStateBuilder AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction = null)
            => AddTransition<Completion>(targetStateName, builder => transitionBuildAction?.Invoke(builder as IDefaultTransitionBuilder));

        [DebuggerHidden]
        public IStateBuilder AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            => AddElseTransition<Completion>(targetStateName, builder => transitionBuildAction?.Invoke(builder as IElseDefaultTransitionBuilder));

        [DebuggerHidden]
        public IStateBuilder AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(Constants.DefaultTransitionTarget, builder => transitionBuildAction?.Invoke(builder as IInternalTransitionBuilder<TEvent>));

        [DebuggerHidden]
        public IStateBuilder AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition<TEvent>(Constants.DefaultTransitionTarget, builder => transitionBuildAction?.Invoke(builder as IElseInternalTransitionBuilder<TEvent>));
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
        #endregion

        [DebuggerHidden]
        IBehaviorStateBuilder IStateEntry<IBehaviorStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateExit<IBehaviorStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateUtils<IBehaviorStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetStateName, transitionBuildAction) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition<TEvent>(targetStateName, transitionBuildAction) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(builder => transitionBuildAction?.Invoke(builder)) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition<TEvent>(builder => transitionBuildAction?.Invoke(builder)) as IBehaviorStateBuilder;

        [DebuggerHidden]
        IJunctionBuilder IPseudostateTransitions<IJunctionBuilder>.AddTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IJunctionBuilder;

        void IPseudostateElseTransitions<IOverridenChoiceBuilder>.AddElseTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IOverridenChoiceBuilder IPseudostateTransitions<IOverridenChoiceBuilder>.AddTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IOverridenChoiceBuilder;

        [DebuggerHidden]
        void IPseudostateElseTransitions<IOverridenJunctionBuilder>.AddElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IOverridenJunctionBuilder IPseudostateTransitions<IOverridenJunctionBuilder>.AddTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IOverridenJunctionBuilder;

        [DebuggerHidden]
        void IPseudostateElseTransitions<IJunctionBuilder>.AddElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IChoiceBuilder IPseudostateTransitions<IChoiceBuilder>.AddTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IChoiceBuilder;

        [DebuggerHidden]
        void IPseudostateElseTransitions<IChoiceBuilder>.AddElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        public IEmbeddedBehaviorBuilder AddForwardedEvent<TEvent>(ForwardedEventBuildAction<TEvent> buildAction = null)
            => AddInternalTransition<TEvent>(b =>
            {
                b.AddEffect(async c =>
                {
                    var stateValues = ((IRootContext)c).Context.GetStateValues(Vertex.Name);
                    var behaviorId = stateValues.BehaviorId ?? Vertex.GetBehaviorId(c.Behavior.Id);
                    if (c.TryLocateBehavior(behaviorId, out var behavior))
                    {
                        var result = await behavior.SendAsync(c.Event);

                        c.Behavior.GetExecutor().OverrideEventStatus(
                            result.Status == EventStatus.Consumed
                                ? EventStatus.Forwarded
                                : result.Status
                        );
                    }
                    else
                    {
                        throw new StateDefinitionException(c.Source.Name, $"DoActivity '{Vertex.BehaviorName}' not found", c.Behavior.Id.BehaviorClass);
                    }
                });

                buildAction?.Invoke(b as IForwardedEventBuilder<TEvent>);
            }) as IEmbeddedBehaviorBuilder;

        [DebuggerHidden]
        public IEmbeddedBehaviorBuilder AddSubscription<TNotification>()
        {
            Vertex.BehaviorSubscriptions.Add(typeof(TNotification));
            
            return this;
        }

        public IEmbeddedBehaviorBuilder AddRelay<TNotification>()
        {
            Vertex.BehaviorRelays.Add(typeof(TNotification));
            
            return this;
        }

        [DebuggerHidden]
        IOverridenStateBuilder IStateEntry<IOverridenStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync) as IOverridenStateBuilder;

        [DebuggerHidden]
        IOverridenStateBuilder IStateExit<IOverridenStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync) as IOverridenStateBuilder;

        [DebuggerHidden]
        IOverridenStateBuilder IStateUtils<IOverridenStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedStateBuilder>.
            UseDefaultTransition(string targetStateName, OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedStateBuilder>.UseInternalTransition<TEvent>(
            OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedStateBuilder>.UseElseDefaultTransition(string targetStateName,
            OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedStateBuilder>.UseElseInternalTransition<TEvent>(
            OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedStateBuilder>.UseTransition<TEvent>(string targetStateName,
            OverridenTransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedStateBuilder>.UseDefaultTransition(string targetStateName,
            OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedStateBuilder>.UseInternalTransition<TEvent>(OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedStateBuilder>.UseElseDefaultTransition(string targetStateName,
            OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedStateBuilder>.UseElseInternalTransition<TEvent>(
            OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedStateBuilder>.UseTransition<TEvent>(string targetStateName,
            OverridenTransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseDefaultTransition(string targetStateName,
            OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseInternalTransition<TEvent>(OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseElseDefaultTransition(string targetStateName,
            OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseElseInternalTransition<TEvent>(
            OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseTransition<TEvent>(string targetStateName,
            OverridenTransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        public IOverridenStateBuilder UseTransition<TEvent>(string targetStateName, OverridenTransitionBuildAction<TEvent> transitionBuildAction)
        {
            var edge = Vertex.Edges.Values.FirstOrDefault(edge =>
                edge.TriggerType == typeof(TEvent) &&
                edge.TargetName == targetStateName &&
                edge.Type == EdgeType.Transition &&
                !edge.IsElse
            );
            
            if (edge?.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Transition triggered by '{Event<TEvent>.Name}' and targeting '{targetStateName}' not found in overriden state '{Vertex.Name}'", Vertex.Graph.Class);
            }
            
            transitionBuildAction?.Invoke(new TransitionBuilder<TEvent>(edge));

            return this;
        }

        [DebuggerHidden]
        public IOverridenStateBuilder UseDefaultTransition(string targetStateName, OverridenDefaultTransitionBuildAction transitionBuildAction)
        {
            var edge = Vertex.Edges.Values.FirstOrDefault(edge =>
                edge.TargetName == targetStateName &&
                edge.Type == EdgeType.DefaultTransition &&
                !edge.IsElse
            );
            
            if (edge?.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Default transition targeting '{targetStateName}' not found in overriden state '{Vertex.Name}'", Vertex.Graph.Class);
            }
            
            transitionBuildAction?.Invoke(new TransitionBuilder<Completion>(edge));

            return this;
        }

        [DebuggerHidden]
        public IOverridenStateBuilder UseInternalTransition<TEvent>(OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
        {
            var edge = Vertex.Edges.Values.FirstOrDefault(edge =>
                edge.TriggerType == typeof(TEvent) &&
                edge.Type == EdgeType.InternalTransition &&
                !edge.IsElse
            );
            
            if (edge?.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Internal transition triggered by '{Event<TEvent>.Name}' not found in overriden state '{Vertex.Name}'", Vertex.Graph.Class);
            }
            
            transitionBuildAction?.Invoke(new TransitionBuilder<TEvent>(edge));

            return this;
        }

        [DebuggerHidden]
        public IOverridenStateBuilder UseElseTransition<TEvent>(string targetStateName, OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
        {
            var edge = Vertex.Edges.Values.FirstOrDefault(edge =>
                edge.TriggerType == typeof(TEvent) &&
                edge.TargetName == targetStateName &&
                edge.Type == EdgeType.Transition &&
                edge.IsElse
            );
            
            if (edge?.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Transition triggered by '{Event<TEvent>.Name}' and targeting '{targetStateName}' not found in overriden state '{Vertex.Name}'", Vertex.Graph.Class);
            }
            
            transitionBuildAction?.Invoke(new TransitionBuilder<TEvent>(edge));

            return this;
        }

        [DebuggerHidden]
        public IOverridenStateBuilder UseElseDefaultTransition(string targetStateName, OverridenElseDefaultTransitionBuildAction transitionBuildAction)
        {
            var edge = Vertex.Edges.Values.FirstOrDefault(edge =>
                edge.TargetName == targetStateName &&
                edge.Type == EdgeType.DefaultTransition &&
                edge.IsElse
            );
            
            if (edge?.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Default transition targeting '{targetStateName}' not found in overriden state '{Vertex.Name}'", Vertex.Graph.Class);
            }
            
            transitionBuildAction?.Invoke(new TransitionBuilder<Completion>(edge));

            return this;
        }

        [DebuggerHidden]
        public IOverridenStateBuilder UseElseInternalTransition<TEvent>(OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
        {
            var edge = Vertex.Edges.Values.FirstOrDefault(edge =>
                edge.TriggerType == typeof(TEvent) &&
                edge.Type == EdgeType.InternalTransition &&
                edge.IsElse
            );
            
            if (edge?.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Internal transition triggered by '{Event<TEvent>.Name}' not found in overriden state '{Vertex.Name}'", Vertex.Graph.Class);
            }
            
            transitionBuildAction?.Invoke(new TransitionBuilder<TEvent>(edge));

            return this;
        }

        [DebuggerHidden]
        public IOverridenRegionalizedStateBuilder MakeComposite(CompositeStateBuildAction compositeStateBuildAction)
        {
            Vertex.Type = Vertex.Type == VertexType.InitialState
                ? VertexType.InitialCompositeState
                : VertexType.CompositeState;

            compositeStateBuildAction?.Invoke(new CompositeStateBuilder(Vertex.DefaultRegion));

            return this;
        }

        [DebuggerHidden]
        public IOverridenRegionalizedStateBuilder MakeOrthogonal(OrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            Vertex.Type = Vertex.Type == VertexType.InitialState
                ? VertexType.InitialOrthogonalState
                : VertexType.OrthogonalState;

            orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(Vertex));

            return this;
        }

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateSubmachine<IBehaviorOverridenStateBuilder>.AddSubmachine(string submachineName, EmbeddedBehaviorBuildAction buildAction,
            StateActionInitializationBuilder initializationBuilder)
            => AddSubmachine(submachineName, buildAction, initializationBuilder) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateDoActivity<IBehaviorOverridenStateBuilder>.AddDoActivity(string doActivityName, EmbeddedBehaviorBuildAction buildAction,
            StateActionInitializationBuilder initializationBuilder)
            => AddDoActivity(doActivityName, buildAction, initializationBuilder) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IOverridenJunctionBuilder IPseudostateTransitionsOverrides<IOverridenJunctionBuilder>.UseTransition(string targetStateName, OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenJunctionBuilder;

        [DebuggerHidden]
        void IPseudostateElseTransitionsOverrides<IOverridenChoiceBuilder>.UseElseTransition(string targetStateName, OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IOverridenChoiceBuilder IPseudostateTransitionsOverrides<IOverridenChoiceBuilder>.UseTransition(string targetStateName, OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenChoiceBuilder;

        [DebuggerHidden]
        void IPseudostateElseTransitionsOverrides<IOverridenJunctionBuilder>.UseElseTransition(string targetStateName, OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateEntry<IBehaviorOverridenStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateExit<IBehaviorOverridenStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateUtils<IBehaviorOverridenStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateComposition<IBehaviorOverridenRegionalizedStateBuilder>.
            MakeComposite(CompositeStateBuildAction compositeStateBuildAction)
            => MakeComposite(compositeStateBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateOrthogonalization<IBehaviorOverridenRegionalizedStateBuilder>.
            MakeOrthogonal(OrthogonalStateBuildAction orthogonalStateBuildAction)
            => MakeOrthogonal(orthogonalStateBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateEntry<IOverridenRegionalizedStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateExit<IOverridenRegionalizedStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateUtils<IOverridenRegionalizedStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateSubmachine<IBehaviorOverridenRegionalizedStateBuilder>.AddSubmachine(string submachineName, EmbeddedBehaviorBuildAction buildAction,
            StateActionInitializationBuilder initializationBuilder)
            => AddSubmachine(submachineName, buildAction, initializationBuilder) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateDoActivity<IBehaviorOverridenRegionalizedStateBuilder>.AddDoActivity(string doActivityName,
            EmbeddedBehaviorBuildAction buildAction, StateActionInitializationBuilder initializationBuilder)
            => AddDoActivity(doActivityName, buildAction, initializationBuilder) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateEntry<IBehaviorOverridenRegionalizedStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateExit<IBehaviorOverridenRegionalizedStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateUtils<IBehaviorOverridenRegionalizedStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IForkBuilder IForkTransitions<IForkBuilder>.AddTransition(string targetStateName, DefaultTransitionEffectBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, b => transitionBuildAction?.Invoke(b as IDefaultTransitionEffectBuilder)) as IForkBuilder; 

        [DebuggerHidden]
        IOverridenForkBuilder IForkTransitions<IOverridenForkBuilder>.AddTransition(string targetStateName, DefaultTransitionEffectBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, b => transitionBuildAction?.Invoke(b as IDefaultTransitionEffectBuilder)) as IOverridenForkBuilder;

        public string Name => Vertex.Name;
        public VertexType Type => Vertex.Type;

        IOverridenJoinBuilder IPseudostateTransitionsOverrides<IOverridenJoinBuilder>.UseTransition(
            string targetStateName,
            OverridenDefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenJoinBuilder;

        void IPseudostateElseTransitionsOverrides<IOverridenJoinBuilder>.UseElseTransition(string targetStateName,
            OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);
    }
}
