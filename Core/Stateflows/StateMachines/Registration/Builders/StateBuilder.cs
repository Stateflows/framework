using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Registration;
using Stateflows.StateMachines.Events;
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
    internal partial class StateBuilder : 
        IStateBuilder,
        IOverridenStateBuilder,
        IOverridenCompositedStateBuilder,
        IJunctionBuilder,
        IOverridenJunctionBuilder,
        IChoiceBuilder,
        IOverridenChoiceBuilder,
        IBehaviorStateBuilder,
        IBehaviorOverridenStateBuilder,
        IBehaviorOverridenCompositedStateBuilder,
        IInternal,
        IBehaviorBuilder,
        IEmbeddedBehaviorBuilder
    {
        public Vertex Vertex { get; }

        public IServiceCollection Services { get; }

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Vertex.Graph.Name);

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
                    if (e is StateflowsDefinitionException)
                    {
                        throw;
                    }
                    else
                    {
                        if (!await c.Executor.Inspector.OnStateInitializeExceptionAsync(context, e))
                        {
                            throw;
                        }
                        else
                        {
                            throw new BehaviorExecutionException(e);
                        }
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
                    if (e is StateflowsDefinitionException)
                    {
                        throw;
                    }
                    else
                    {
                        if (!await c.Executor.Inspector.OnStateFinalizeExceptionAsync(context, e))
                        {
                            throw;
                        }
                        else
                        {
                            throw new BehaviorExecutionException(e);
                        }
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
                        if (e is StateflowsDefinitionException)
                        {
                            throw;
                        }
                        else
                        {
                            if (!await c.Executor.Inspector.OnStateEntryExceptionAsync(context, e))
                            {
                                throw;
                            }
                            else
                            {
                                throw new BehaviorExecutionException(e);
                            }
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
                        if (e is StateflowsDefinitionException)
                        {
                            throw;
                        }
                        else
                        {
                            if (!await c.Executor.Inspector.OnStateExitExceptionAsync(context, e))
                            {
                                throw;
                            }
                            else
                            {
                                throw new BehaviorExecutionException(e);
                            }
                        }
                    }
                }
            );

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

            if (Vertex.Edges.ContainsKey(edge.Name))
                if (targetStateName == Constants.DefaultTransitionTarget)
                    throw new TransitionDefinitionException($"Internal transition in '{edge.SourceName}' triggered by '{edge.Trigger}' is already registered", Vertex.Graph.Class);
                else
                    if (edge.Trigger == Constants.Completion)
                        throw new TransitionDefinitionException($"Default transition from '{edge.SourceName}' to '{edge.TargetName}' is already registered", Vertex.Graph.Class);
                    else
                        throw new TransitionDefinitionException($"Transition from '{edge.SourceName}' to '{edge.TargetName}' triggered by '{edge.Trigger}' is already registered", Vertex.Graph.Class);

            Vertex.Edges.Add(edge.Name, edge);
            Vertex.Graph.AllEdges.Add(edge);

            transitionBuildAction?.Invoke(new TransitionBuilder<TEvent>(edge, Services));

            return this;
        }

        [DebuggerHidden]
        public IStateBuilder AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction = null)
            => AddTransitionInternal(targetStateName, false, transitionBuildAction);

        [DebuggerHidden]
        IBehaviorOverridenCompositedStateBuilder IStateTransitions<IBehaviorOverridenCompositedStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenCompositedStateBuilder IStateTransitions<IBehaviorOverridenCompositedStateBuilder>.AddInternalTransition<TEvent>(
            InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction) as IBehaviorOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenCompositedStateBuilder IStateTransitions<IBehaviorOverridenCompositedStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenCompositedStateBuilder IStateTransitions<IBehaviorOverridenCompositedStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenCompositedStateBuilder IStateTransitions<IBehaviorOverridenCompositedStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction) as IBehaviorOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenCompositedStateBuilder IStateTransitions<IBehaviorOverridenCompositedStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IOverridenCompositedStateBuilder IStateTransitions<IOverridenCompositedStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IOverridenCompositedStateBuilder IStateTransitions<IOverridenCompositedStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction) as IOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IOverridenCompositedStateBuilder IStateTransitions<IOverridenCompositedStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction) as IOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IOverridenCompositedStateBuilder IStateTransitions<IOverridenCompositedStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IOverridenCompositedStateBuilder IStateTransitions<IOverridenCompositedStateBuilder>.AddElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction) as IOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IOverridenCompositedStateBuilder IStateTransitions<IOverridenCompositedStateBuilder>.AddTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction) as IOverridenCompositedStateBuilder;

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
        public IBehaviorStateBuilder AddSubmachine(string submachineName, EmbeddedBehaviorBuildAction buildAction, StateActionInitializationBuilderAsync initializationBuilder = null)
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
        public IBehaviorStateBuilder AddDoActivity(string doActivityName, EmbeddedBehaviorBuildAction buildAction = null, StateActionInitializationBuilderAsync initializationBuilder = null)
        {
            Vertex.BehaviorType = BehaviorType.Activity;
            Vertex.BehaviorName = doActivityName;
            Vertex.BehaviorInitializationBuilder = initializationBuilder;

            buildAction?.Invoke(this);

            return this;
        }
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

        void IPseudostateTransitions<IOverridenChoiceBuilder>.AddElseTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

        IOverridenChoiceBuilder IPseudostateTransitions<IOverridenChoiceBuilder>.AddTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IOverridenChoiceBuilder;

        void IPseudostateTransitions<IOverridenJunctionBuilder>.AddElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction);

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
        public IEmbeddedBehaviorBuilder AddForwardedEvent<TEvent>(ForwardedEventBuildAction<TEvent> buildAction = null)
            => AddInternalTransition<TEvent>(b =>
            {
                b.AddEffect(async c =>
                {
                    if (c.TryLocateBehavior(Vertex.GetBehaviorId(c.StateMachine.Id), out var behavior))
                    {
                        var result = await behavior.SendAsync(c.Event);

                        c.StateMachine.GetExecutor().OverrideEventStatus(
                            result.Status == EventStatus.Consumed
                                ? EventStatus.Forwarded
                                : result.Status
                        );
                    }
                    else
                    {
                        throw new StateDefinitionException(c.SourceState.Name, $"DoActivity '{Vertex.BehaviorName}' not found", c.StateMachine.Id.StateMachineClass);
                    }
                });

                buildAction?.Invoke(b as IForwardedEventBuilder<TEvent>);
            }) as IEmbeddedBehaviorBuilder;

        public IEmbeddedBehaviorBuilder AddSubscription<TNotificationEvent>()
        {
            Vertex.BehaviorSubscriptions.Add(typeof(TNotificationEvent));
            
            return this;
        }

        IOverridenStateBuilder IStateEntry<IOverridenStateBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as IOverridenStateBuilder;

        IOverridenStateBuilder IStateExit<IOverridenStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnExit(actionAsync) as IOverridenStateBuilder;

        IOverridenStateBuilder IStateUtils<IOverridenStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IOverridenStateBuilder;

        IBehaviorOverridenCompositedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenCompositedStateBuilder>.
            UseDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenCompositedStateBuilder;

        IBehaviorOverridenCompositedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenCompositedStateBuilder>.UseInternalTransition<TEvent>(
            InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction) as IBehaviorOverridenCompositedStateBuilder;

        IBehaviorOverridenCompositedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenCompositedStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenCompositedStateBuilder;

        IBehaviorOverridenCompositedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenCompositedStateBuilder>.UseElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenCompositedStateBuilder;

        IBehaviorOverridenCompositedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenCompositedStateBuilder>.UseElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction) as IBehaviorOverridenCompositedStateBuilder;

        IBehaviorOverridenCompositedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenCompositedStateBuilder>.UseTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenCompositedStateBuilder;

        IOverridenCompositedStateBuilder IStateTransitionsOverrides<IOverridenCompositedStateBuilder>.UseDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenCompositedStateBuilder;

        IOverridenCompositedStateBuilder IStateTransitionsOverrides<IOverridenCompositedStateBuilder>.UseInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction) as IOverridenCompositedStateBuilder;

        IOverridenCompositedStateBuilder IStateTransitionsOverrides<IOverridenCompositedStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction) as IOverridenCompositedStateBuilder;

        IOverridenCompositedStateBuilder IStateTransitionsOverrides<IOverridenCompositedStateBuilder>.UseElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenCompositedStateBuilder;

        IOverridenCompositedStateBuilder IStateTransitionsOverrides<IOverridenCompositedStateBuilder>.UseElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction) as IOverridenCompositedStateBuilder;

        IOverridenCompositedStateBuilder IStateTransitionsOverrides<IOverridenCompositedStateBuilder>.UseTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction) as IOverridenCompositedStateBuilder;

        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;

        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction) as IBehaviorOverridenStateBuilder;

        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;

        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;

        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction) as IBehaviorOverridenStateBuilder;

        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;

        public IOverridenStateBuilder UseTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction)
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
            
            transitionBuildAction?.Invoke(new TransitionBuilder<TEvent>(edge, Services));

            return this;
        }

        public IOverridenStateBuilder UseDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
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
            
            transitionBuildAction?.Invoke(new TransitionBuilder<Completion>(edge, Services));

            return this;
        }

        public IOverridenStateBuilder UseInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
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
            
            transitionBuildAction?.Invoke(new TransitionBuilder<TEvent>(edge, Services));

            return this;
        }

        public IOverridenStateBuilder UseElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
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
            
            transitionBuildAction?.Invoke(new TransitionBuilder<TEvent>(edge, Services));

            return this;
        }

        public IOverridenStateBuilder UseElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
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
            
            transitionBuildAction?.Invoke(new TransitionBuilder<Completion>(edge, Services));

            return this;
        }

        public IOverridenStateBuilder UseElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
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
            
            transitionBuildAction?.Invoke(new TransitionBuilder<TEvent>(edge, Services));

            return this;
        }

        public IOverridenCompositedStateBuilder MakeComposite(CompositeStateBuildAction compositeStateBuildAction)
        {
            Vertex.Type = VertexType.CompositeState;
            
            compositeStateBuildAction?.Invoke(new CompositeStateBuilder(Vertex, Services));

            return this;
        }

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateSubmachine<IBehaviorOverridenStateBuilder>.AddSubmachine(string submachineName, EmbeddedBehaviorBuildAction buildAction,
            StateActionInitializationBuilderAsync initializationBuilder)
            => AddSubmachine(submachineName, buildAction, initializationBuilder) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateDoActivity<IBehaviorOverridenStateBuilder>.AddDoActivity(string doActivityName, EmbeddedBehaviorBuildAction buildAction,
            StateActionInitializationBuilderAsync initializationBuilder)
            => AddDoActivity(doActivityName, buildAction, initializationBuilder) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IOverridenJunctionBuilder IPseudostateTransitionsOverrides<IOverridenJunctionBuilder>.UseTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenJunctionBuilder;

        [DebuggerHidden]
        void IPseudostateTransitionsOverrides<IOverridenChoiceBuilder>.UseElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IOverridenChoiceBuilder IPseudostateTransitionsOverrides<IOverridenChoiceBuilder>.UseTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenChoiceBuilder;

        [DebuggerHidden]
        void IPseudostateTransitionsOverrides<IOverridenJunctionBuilder>.UseElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateEntry<IBehaviorOverridenStateBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateExit<IBehaviorOverridenStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnExit(actionAsync) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateUtils<IBehaviorOverridenStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenCompositedStateBuilder IStateComposition<IBehaviorOverridenCompositedStateBuilder>.
            MakeComposite(CompositeStateBuildAction compositeStateBuildAction)
            => MakeComposite(compositeStateBuildAction) as IBehaviorOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IOverridenCompositedStateBuilder IStateEntry<IOverridenCompositedStateBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as IOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IOverridenCompositedStateBuilder IStateExit<IOverridenCompositedStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnExit(actionAsync) as IOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IOverridenCompositedStateBuilder IStateUtils<IOverridenCompositedStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenCompositedStateBuilder IStateSubmachine<IBehaviorOverridenCompositedStateBuilder>.AddSubmachine(string submachineName, EmbeddedBehaviorBuildAction buildAction,
            StateActionInitializationBuilderAsync initializationBuilder)
            => AddSubmachine(submachineName, buildAction, initializationBuilder) as IBehaviorOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenCompositedStateBuilder IStateDoActivity<IBehaviorOverridenCompositedStateBuilder>.AddDoActivity(string doActivityName,
            EmbeddedBehaviorBuildAction buildAction, StateActionInitializationBuilderAsync initializationBuilder)
            => AddDoActivity(doActivityName, buildAction, initializationBuilder) as IBehaviorOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenCompositedStateBuilder IStateEntry<IBehaviorOverridenCompositedStateBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as IBehaviorOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenCompositedStateBuilder IStateExit<IBehaviorOverridenCompositedStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnExit(actionAsync) as IBehaviorOverridenCompositedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenCompositedStateBuilder IStateUtils<IBehaviorOverridenCompositedStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IBehaviorOverridenCompositedStateBuilder;
    }
}
