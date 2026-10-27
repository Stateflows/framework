using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Registration;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using ActionBuildAction = Stateflows.Actions.Registration.Interfaces.ActionBuildAction;
using ActionDelegateAsync = Stateflows.Actions.Registration.ActionDelegateAsync;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class OrthogonalStateBuilder :
        IOrthogonalStateBuilder,
        IOverridenOrthogonalStateBuilder,
        IBehaviorOrthogonalStateBuilder,
        IBehaviorOverridenOrthogonalStateBuilder,
        IStateBuilderInfo,
        IGraphBuilder,
        IVertexBuilder,
        IBehaviorBuilder
    {   
        public Vertex Vertex { get; }

        public Graph Graph => Vertex.Graph;

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Vertex.Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Vertex.Graph.Version;

        public OrthogonalStateBuilder(Vertex vertex)
        {
            Vertex = vertex;
        }

        IBehaviorOrthogonalStateBuilder IStateDoActivity<IBehaviorOrthogonalStateBuilder>.AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction)
        {
            new StateBuilder(Vertex).AddDoActivity<TActivity>(buildAction);
            return this;
        }

        IBehaviorOrthogonalStateBuilder IStateDoActivity<IBehaviorOrthogonalStateBuilder>.AddDoActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            new StateBuilder(Vertex).AddDoActivity(activityBuildAction);
            return this;
        }

        IBehaviorOrthogonalStateBuilder IStateDoAction<IBehaviorOrthogonalStateBuilder>.AddDoAction<TAction>(ActionBuildAction<TAction> buildAction)
        {
            new StateBuilder(Vertex).AddDoAction<TAction>(buildAction);
            return this;
        }

        IBehaviorOrthogonalStateBuilder IStateDoAction<IBehaviorOrthogonalStateBuilder>.AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction)
        {
            new StateBuilder(Vertex).AddDoAction(actionDelegate, buildAction);
            return this;
        }

        IBehaviorOverridenOrthogonalStateBuilder IStateDoActivity<IBehaviorOverridenOrthogonalStateBuilder>.AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction)
        {
            new StateBuilder(Vertex).AddDoActivity<TActivity>(buildAction);
            return this;
        }

        IBehaviorOverridenOrthogonalStateBuilder IStateDoActivity<IBehaviorOverridenOrthogonalStateBuilder>.AddDoActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            new StateBuilder(Vertex).AddDoActivity(activityBuildAction);
            return this;
        }

        IBehaviorOverridenOrthogonalStateBuilder IStateDoAction<IBehaviorOverridenOrthogonalStateBuilder>.AddDoAction<TAction>(ActionBuildAction<TAction> buildAction)
        {
            new StateBuilder(Vertex).AddDoAction<TAction>(buildAction);
            return this;
        }

        IBehaviorOverridenOrthogonalStateBuilder IStateDoAction<IBehaviorOverridenOrthogonalStateBuilder>.AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction)
        {
            new StateBuilder(Vertex).AddDoAction(actionDelegate, buildAction);
            return this;
        }

        IBehaviorOrthogonalStateBuilder IStateEntry<IBehaviorOrthogonalStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => (IBehaviorOrthogonalStateBuilder)AddOnEntry(actionsAsync);

        IBehaviorOrthogonalStateBuilder IStateExit<IBehaviorOrthogonalStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => (IBehaviorOrthogonalStateBuilder)AddOnExit(actionsAsync);

        IBehaviorOrthogonalStateBuilder ICompositeStateInitialization<IBehaviorOrthogonalStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => (IBehaviorOrthogonalStateBuilder)AddOnInitialize(actionsAsync);

        IBehaviorOrthogonalStateBuilder ICompositeStateFinalization<IBehaviorOrthogonalStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => (IBehaviorOrthogonalStateBuilder)AddOnFinalize(actionsAsync);

        IBehaviorOrthogonalStateBuilder IStateUtils<IBehaviorOrthogonalStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => (IBehaviorOrthogonalStateBuilder)AddDeferredEvent(buildAction);

        IBehaviorOrthogonalStateBuilder IRegions<IBehaviorOrthogonalStateBuilder>.AddRegion(RegionBuildAction buildAction)
            => (IBehaviorOrthogonalStateBuilder)AddRegion(buildAction);

        IBehaviorOrthogonalStateBuilder IStateTransitions<IBehaviorOrthogonalStateBuilder>.AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction)
            => (IBehaviorOrthogonalStateBuilder)AddTransition(targetStateName, transitionBuildAction);

        IBehaviorOrthogonalStateBuilder IStateTransitions<IBehaviorOrthogonalStateBuilder>.AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => (IBehaviorOrthogonalStateBuilder)AddDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorOrthogonalStateBuilder IStateTransitions<IBehaviorOrthogonalStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => (IBehaviorOrthogonalStateBuilder)AddInternalTransition(transitionBuildAction);

        IBehaviorOrthogonalStateBuilder IStateTransitions<IBehaviorOrthogonalStateBuilder>.AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => (IBehaviorOrthogonalStateBuilder)AddElseTransition(targetStateName, transitionBuildAction);

        IBehaviorOrthogonalStateBuilder IStateTransitions<IBehaviorOrthogonalStateBuilder>.AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => (IBehaviorOrthogonalStateBuilder)AddElseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorOrthogonalStateBuilder IStateTransitions<IBehaviorOrthogonalStateBuilder>.AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => (IBehaviorOrthogonalStateBuilder)AddElseInternalTransition(transitionBuildAction);

        IBehaviorOverridenOrthogonalStateBuilder IStateEntry<IBehaviorOverridenOrthogonalStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => (IBehaviorOverridenOrthogonalStateBuilder)AddOnEntry(actionsAsync);

        IBehaviorOverridenOrthogonalStateBuilder IStateExit<IBehaviorOverridenOrthogonalStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => (IBehaviorOverridenOrthogonalStateBuilder)AddOnExit(actionsAsync);

        IBehaviorOverridenOrthogonalStateBuilder ICompositeStateInitialization<IBehaviorOverridenOrthogonalStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => (IBehaviorOverridenOrthogonalStateBuilder)AddOnInitialize(actionsAsync);

        IBehaviorOverridenOrthogonalStateBuilder ICompositeStateFinalization<IBehaviorOverridenOrthogonalStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => (IBehaviorOverridenOrthogonalStateBuilder)AddOnFinalize(actionsAsync);

        IBehaviorOverridenOrthogonalStateBuilder IStateUtils<IBehaviorOverridenOrthogonalStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)AddDeferredEvent(buildAction);

        IBehaviorOverridenOrthogonalStateBuilder IStateUtilsOverrides<IBehaviorOverridenOrthogonalStateBuilder>.UseDeferredEvent<TEvent>(OverridenDeferralBuildAction<TEvent> buildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)UseDeferredEvent(buildAction);

        IBehaviorOverridenOrthogonalStateBuilder IRegions<IBehaviorOverridenOrthogonalStateBuilder>.AddRegion(RegionBuildAction buildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)AddRegion(buildAction);

        IBehaviorOverridenOrthogonalStateBuilder IRegionsOverrides<IBehaviorOverridenOrthogonalStateBuilder>.UseRegion(int index, OverridenRegionBuildAction buildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)((IRegionsOverrides<IOverridenOrthogonalStateBuilder>)this).UseRegion(index, buildAction);

        IBehaviorOverridenOrthogonalStateBuilder IStateTransitions<IBehaviorOverridenOrthogonalStateBuilder>.AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)AddTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenOrthogonalStateBuilder IStateTransitions<IBehaviorOverridenOrthogonalStateBuilder>.AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)AddDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenOrthogonalStateBuilder IStateTransitions<IBehaviorOverridenOrthogonalStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)AddInternalTransition(transitionBuildAction);

        IBehaviorOverridenOrthogonalStateBuilder IStateTransitions<IBehaviorOverridenOrthogonalStateBuilder>.AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)AddElseTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenOrthogonalStateBuilder IStateTransitions<IBehaviorOverridenOrthogonalStateBuilder>.AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)AddElseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenOrthogonalStateBuilder IStateTransitions<IBehaviorOverridenOrthogonalStateBuilder>.AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)AddElseInternalTransition(transitionBuildAction);

        IBehaviorOverridenOrthogonalStateBuilder IStateTransitionsOverrides<IBehaviorOverridenOrthogonalStateBuilder>.UseTransition<TEvent>(string targetStateName, OverridenTransitionBuildAction<TEvent> transitionBuildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)UseTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenOrthogonalStateBuilder IStateTransitionsOverrides<IBehaviorOverridenOrthogonalStateBuilder>.UseDefaultTransition(string targetStateName, OverridenDefaultTransitionBuildAction transitionBuildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)UseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenOrthogonalStateBuilder IStateTransitionsOverrides<IBehaviorOverridenOrthogonalStateBuilder>.UseInternalTransition<TEvent>(OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)UseInternalTransition(transitionBuildAction);

        IBehaviorOverridenOrthogonalStateBuilder IStateTransitionsOverrides<IBehaviorOverridenOrthogonalStateBuilder>.UseElseTransition<TEvent>(string targetStateName, OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)UseElseTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenOrthogonalStateBuilder IStateTransitionsOverrides<IBehaviorOverridenOrthogonalStateBuilder>.UseElseDefaultTransition(string targetStateName, OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)UseElseDefaultTransition(targetStateName, transitionBuildAction);

        IBehaviorOverridenOrthogonalStateBuilder IStateTransitionsOverrides<IBehaviorOverridenOrthogonalStateBuilder>.UseElseInternalTransition<TEvent>(OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => (IBehaviorOverridenOrthogonalStateBuilder)UseElseInternalTransition(transitionBuildAction);

        #region Events
        [DebuggerHidden]
        public IOrthogonalStateBuilder AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
        {
            foreach (var actionAsync in actionsAsync)
            {
                actionAsync.ThrowIfNull(nameof(actionAsync));

                Vertex.Initialize.Actions.Add(async c =>
                    {
                        var context = new StateActionContext(c, Vertex, Constants.Entry);
                        await actionAsync(context);
                    }
                );
            }

            return this;
        }

        [DebuggerHidden]
        public IOrthogonalStateBuilder AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
        {
            foreach (var actionAsync in actionsAsync)
            {
                actionAsync.ThrowIfNull(nameof(actionAsync));

                Vertex.Finalize.Actions.Add(async c =>
                    {
                        var context = new StateActionContext(c, Vertex, Constants.Entry);
                        await actionAsync(context);
                    }
                );
            }

            return this;
        }

        [DebuggerHidden]
        public IOrthogonalStateBuilder AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
        {
            foreach (var actionAsync in actionsAsync)
            {
                actionAsync.ThrowIfNull(nameof(actionAsync));

                Vertex.Entry.Actions.Add(async c =>
                    {
                        var context = new StateActionContext(c, Vertex, Constants.Entry);
                        await actionAsync(context);
                    }
                );
            }

            return this;
        }

        [DebuggerHidden]
        public IOrthogonalStateBuilder AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
        {
            foreach (var actionAsync in actionsAsync)
            {
                actionAsync.ThrowIfNull(nameof(actionAsync));

                Vertex.Exit.Actions.Add(async c =>
                    {
                        var context = new StateActionContext(c, Vertex, Constants.Exit);
                        await actionAsync(context);
                    }
                );
            }

            return this;
        }
        #endregion

        #region Utils
        [DebuggerHidden]
        public IOrthogonalStateBuilder AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
        {
            if (typeof(TEvent) == typeof(Completion))
                throw new DeferralDefinitionException(typeof(TEvent).GetEventName(), "Completion event cannot be deferred.", Vertex.Graph.Class);

            if (typeof(TEvent) == typeof(Finalize))
                throw new DeferralDefinitionException(typeof(TEvent).GetEventName(), "Exit event cannot be deferred.", Vertex.Graph.Class);

            if (typeof(TEvent).IsSubclassOf(typeof(TimeEvent)))
                throw new DeferralDefinitionException(typeof(TEvent).GetEventName(), "Time events cannot be deferred.", Vertex.Graph.Class);

            var builder = new EventDeferralBuilder<TEvent>(Vertex);
            buildAction?.Invoke(builder);

            var eventName = typeof(TEvent).GetEventName();
            if (!Vertex.Deferrals.TryAdd(eventName, builder.Logic))
                throw new DeferralDefinitionException(eventName, $"Event '{eventName}' is already deferred by '{Vertex.Name}'.", Vertex.Graph.Class);
            Vertex.DeferralTypes.Add(eventName, typeof(TEvent));

            return this;
        }
        #endregion

        #region Transitions
        [DebuggerHidden]
        private IOrthogonalStateBuilder AddTransitionInternal<TEvent>(string targetStateName, bool isElse, TransitionBuildAction<TEvent> transitionBuildAction = null)
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

            transitionBuildAction?.Invoke(new TransitionBuilder<TEvent>(edge));

            return this;
        }

        [DebuggerHidden]
        public IOrthogonalStateBuilder AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction = null)
        {
            targetStateName.ThrowIfNullOrEmpty(nameof(targetStateName));
            return AddTransitionInternal(targetStateName, false, transitionBuildAction);
        }

        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder IStateTransitions<IOverridenOrthogonalStateBuilder>.AddDefaultTransition(string targetStateName,
            DefaultTransitionBuildAction transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IOverridenOrthogonalStateBuilder;

        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder IStateTransitions<IOverridenOrthogonalStateBuilder>.AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddInternalTransition(transitionBuildAction) as IOverridenOrthogonalStateBuilder;

        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder IStateTransitions<IOverridenOrthogonalStateBuilder>.AddElseTransition<TEvent>(string targetStateName,
            ElseTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition(targetStateName, transitionBuildAction) as IOverridenOrthogonalStateBuilder;

        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder IStateTransitions<IOverridenOrthogonalStateBuilder>.AddElseDefaultTransition(string targetStateName,
            ElseDefaultTransitionBuildAction transitionBuildAction)
            => AddElseDefaultTransition(targetStateName, transitionBuildAction) as IOverridenOrthogonalStateBuilder;

        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder IStateTransitions<IOverridenOrthogonalStateBuilder>.AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseInternalTransition(transitionBuildAction) as IOverridenOrthogonalStateBuilder;

        [DebuggerHidden]
        public IOrthogonalStateBuilder AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
        {
            targetStateName.ThrowIfNullOrEmpty(nameof(targetStateName));
            return AddTransitionInternal<TEvent>(targetStateName, true, builder => transitionBuildAction?.Invoke(builder as IElseTransitionBuilder<TEvent>));
        }

        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder IStateTransitions<IOverridenOrthogonalStateBuilder>.AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransition(targetStateName, transitionBuildAction) as IOverridenOrthogonalStateBuilder;

        [DebuggerHidden]
        public IOrthogonalStateBuilder AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction = null)
            => AddTransition<Completion>(targetStateName, builder => transitionBuildAction?.Invoke(builder as IDefaultTransitionBuilder));

        [DebuggerHidden]
        public IOrthogonalStateBuilder AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            => AddElseTransition<Completion>(targetStateName, builder => transitionBuildAction?.Invoke(builder as IElseDefaultTransitionBuilder));

        [DebuggerHidden]
        public IOrthogonalStateBuilder AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransitionInternal<TEvent>(Constants.DefaultTransitionTarget, false, builder => transitionBuildAction?.Invoke(builder as IInternalTransitionBuilder<TEvent>));

        [DebuggerHidden]
        public IOrthogonalStateBuilder AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddTransitionInternal<TEvent>(Constants.DefaultTransitionTarget, true, builder => transitionBuildAction?.Invoke(builder as IElseInternalTransitionBuilder<TEvent>));
        #endregion

        [DebuggerHidden]
        public IOverridenOrthogonalStateBuilder UseTransition<TEvent>(string targetStateName, OverridenTransitionBuildAction<TEvent> transitionBuildAction)
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
        public IOverridenOrthogonalStateBuilder UseDefaultTransition(string targetStateName, OverridenDefaultTransitionBuildAction transitionBuildAction)
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
        public IOverridenOrthogonalStateBuilder UseInternalTransition<TEvent>(OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction)
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
        public IOverridenOrthogonalStateBuilder UseElseTransition<TEvent>(string targetStateName, OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
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
        public IOverridenOrthogonalStateBuilder UseElseDefaultTransition(string targetStateName, OverridenElseDefaultTransitionBuildAction transitionBuildAction)
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
        public IOverridenOrthogonalStateBuilder UseElseInternalTransition<TEvent>(OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
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
        public IOrthogonalStateBuilder AddRegion(RegionBuildAction buildAction)
        {
            var region = new Region()
            {
                ParentVertex = Vertex,
                Graph = Vertex.Graph,
                OriginStateMachineName = Vertex.OriginStateMachineName
            };

            Vertex.Regions.Add(region);

            buildAction?.Invoke(new RegionBuilder(region));

            return this;
        }

        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder IStateEntry<IOverridenOrthogonalStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync) as IOverridenOrthogonalStateBuilder;

        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder IStateExit<IOverridenOrthogonalStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync) as IOverridenOrthogonalStateBuilder;

        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder ICompositeStateInitialization<IOverridenOrthogonalStateBuilder>.AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnInitialize(actionsAsync) as IOverridenOrthogonalStateBuilder;
        
        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder ICompositeStateFinalization<IOverridenOrthogonalStateBuilder>.AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnFinalize(actionsAsync) as IOverridenOrthogonalStateBuilder;

        IOverridenOrthogonalStateBuilder IStateUtils<IOverridenOrthogonalStateBuilder>.AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction)
            => AddDeferredEvent<TEvent>(buildAction) as IOverridenOrthogonalStateBuilder;

        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder IRegions<IOverridenOrthogonalStateBuilder>.AddRegion(RegionBuildAction buildAction)
            => AddRegion(buildAction) as IOverridenOrthogonalStateBuilder;

        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder IRegionsOverrides<IOverridenOrthogonalStateBuilder>.UseRegion(int index, OverridenRegionBuildAction buildAction)
        {
            if (index < 0 || index >= Vertex.Regions.Count)
            {
                throw new StateMachineOverrideException($"Region at index {index} not found in overriden state '{Vertex.Name}'", Vertex.Graph.Class);
            }

            buildAction?.Invoke(new RegionBuilder(Vertex.Regions[index]));
            return this;
        }

        public string Name => Vertex.Name;
        public VertexType Type => Vertex.Type;
        public IOverridenOrthogonalStateBuilder UseDeferredEvent<TEvent>(OverridenDeferralBuildAction<TEvent> buildAction = null)
        {
            if (typeof(TEvent) == typeof(Completion))
                throw new DeferralDefinitionException(typeof(TEvent).GetEventName(), "Completion event cannot be deferred.", Vertex.Graph.Class);

            if (typeof(TEvent) == typeof(Finalize))
                throw new DeferralDefinitionException(typeof(TEvent).GetEventName(), "Exit event cannot be deferred.", Vertex.Graph.Class);

            if (typeof(TEvent).IsSubclassOf(typeof(TimeEvent)))
                throw new DeferralDefinitionException(typeof(TEvent).GetEventName(), "Time events cannot be deferred.", Vertex.Graph.Class);

            if (
                !Vertex.Deferrals.TryGetValue(typeof(TEvent).GetEventName(), out var deferralLogic) ||
                deferralLogic?.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"Deferral of event '{Event<TEvent>.Name}' not found in overriden state '{Vertex.Name}'", Vertex.Graph.Class);
            }
            
            var builder = new EventDeferralBuilder<TEvent>(Vertex, deferralLogic);
            
            buildAction?.Invoke(builder);

            return this;
        }
    }
}
