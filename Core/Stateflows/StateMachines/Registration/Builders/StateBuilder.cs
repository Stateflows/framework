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
        void IPseudostateTransitionsEffects<IOverridenJoinBuilder>.AddTransition(string targetStateName,
            DefaultTransitionEffectBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, b => transitionBuildAction?.Invoke(b as IDefaultTransitionEffectBuilder));
        
        [DebuggerHidden]
        void IPseudostateTransitionsEffects<IJoinBuilder>.AddTransition(string targetStateName, DefaultTransitionEffectBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, b => transitionBuildAction?.Invoke(b as IDefaultTransitionEffectBuilder));

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
                        throw new StateDefinitionException(c.Source.Name, $"DoActivity '{Vertex.BehaviorName}' not found", c.StateMachine.Id.StateMachineClass);
                    }
                });

                buildAction?.Invoke(b as IForwardedEventBuilder<TEvent>);
            }) as IEmbeddedBehaviorBuilder;

        [DebuggerHidden]
        public IEmbeddedBehaviorBuilder AddSubscription<TNotificationEvent>()
        {
            Vertex.BehaviorSubscriptions.Add(typeof(TNotificationEvent));
            
            return this;
        }

        [DebuggerHidden]
        IOverridenStateBuilder IStateEntry<IOverridenStateBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as IOverridenStateBuilder;

        [DebuggerHidden]
        IOverridenStateBuilder IStateExit<IOverridenStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnExit(actionAsync) as IOverridenStateBuilder;

        [DebuggerHidden]
        IOverridenStateBuilder IStateUtils<IOverridenStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IOverridenStateBuilder;

        [DebuggerHidden]
        void IPseudostateTransitionsEffectsOverrides<IOverridenJoinBuilder>.UseTransition(string targetStateName, DefaultTransitionEffectBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, b => transitionBuildAction?.Invoke(b as IDefaultTransitionEffectBuilder));

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedStateBuilder>.
            UseDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedStateBuilder>.UseInternalTransition<TEvent>(
            InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedStateBuilder>.UseElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedStateBuilder>.UseElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IBehaviorOverridenRegionalizedStateBuilder>.UseTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedStateBuilder>.UseDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedStateBuilder>.UseInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedStateBuilder>.UseElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedStateBuilder>.UseElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateTransitionsOverrides<IOverridenRegionalizedStateBuilder>.UseTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseInternalTransition(transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseElseInternalTransition<TEvent>(
            ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => UseElseInternalTransition(transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenStateBuilder IStateTransitionsOverrides<IBehaviorOverridenStateBuilder>.UseTransition<TEvent>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction)
            => UseTransition(targetStateName, transitionBuildAction) as IBehaviorOverridenStateBuilder;

        [DebuggerHidden]
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

        [DebuggerHidden]
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

        [DebuggerHidden]
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

        [DebuggerHidden]
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

        [DebuggerHidden]
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

        [DebuggerHidden]
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

        [DebuggerHidden]
        public IOverridenRegionalizedStateBuilder MakeComposite(CompositeStateBuildAction compositeStateBuildAction)
        {
            Vertex.Type = Vertex.Type == VertexType.InitialState
                ? VertexType.InitialCompositeState
                : VertexType.CompositeState;

            compositeStateBuildAction?.Invoke(new CompositeStateBuilder(Vertex.DefaultRegion, Services));

            return this;
        }

        [DebuggerHidden]
        public IOverridenRegionalizedStateBuilder MakeOrthogonal(OrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            Vertex.Type = Vertex.Type == VertexType.InitialState
                ? VertexType.InitialOrthogonalState
                : VertexType.OrthogonalState;

            orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(Vertex, Services));

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
        void IPseudostateElseTransitionsOverrides<IOverridenChoiceBuilder>.UseElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => UseElseDefaultTransition(targetStateName, transitionBuildAction);

        [DebuggerHidden]
        IOverridenChoiceBuilder IPseudostateTransitionsOverrides<IOverridenChoiceBuilder>.UseTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => UseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenChoiceBuilder;

        [DebuggerHidden]
        void IPseudostateElseTransitionsOverrides<IOverridenJunctionBuilder>.UseElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
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
        IBehaviorOverridenRegionalizedStateBuilder IStateComposition<IBehaviorOverridenRegionalizedStateBuilder>.
            MakeComposite(CompositeStateBuildAction compositeStateBuildAction)
            => MakeComposite(compositeStateBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateOrthogonalization<IBehaviorOverridenRegionalizedStateBuilder>.
            MakeOrthogonal(OrthogonalStateBuildAction orthogonalStateBuildAction)
            => MakeOrthogonal(orthogonalStateBuildAction) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateEntry<IOverridenRegionalizedStateBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateExit<IOverridenRegionalizedStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnExit(actionAsync) as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IOverridenRegionalizedStateBuilder IStateUtils<IOverridenRegionalizedStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateSubmachine<IBehaviorOverridenRegionalizedStateBuilder>.AddSubmachine(string submachineName, EmbeddedBehaviorBuildAction buildAction,
            StateActionInitializationBuilderAsync initializationBuilder)
            => AddSubmachine(submachineName, buildAction, initializationBuilder) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateDoActivity<IBehaviorOverridenRegionalizedStateBuilder>.AddDoActivity(string doActivityName,
            EmbeddedBehaviorBuildAction buildAction, StateActionInitializationBuilderAsync initializationBuilder)
            => AddDoActivity(doActivityName, buildAction, initializationBuilder) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateEntry<IBehaviorOverridenRegionalizedStateBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateExit<IBehaviorOverridenRegionalizedStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnExit(actionAsync) as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IBehaviorOverridenRegionalizedStateBuilder IStateUtils<IBehaviorOverridenRegionalizedStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IBehaviorOverridenRegionalizedStateBuilder;

        [DebuggerHidden]
        IForkBuilder IForkTransitions<IForkBuilder>.AddTransition(string targetStateName, DefaultTransitionEffectBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, b => transitionBuildAction?.Invoke(b as IDefaultTransitionEffectBuilder)) as IForkBuilder; 

        [DebuggerHidden]
        IOverridenForkBuilder IForkTransitions<IOverridenForkBuilder>.AddTransition(string targetStateName, DefaultTransitionEffectBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, b => transitionBuildAction?.Invoke(b as IDefaultTransitionEffectBuilder)) as IOverridenForkBuilder; 
    }
}
