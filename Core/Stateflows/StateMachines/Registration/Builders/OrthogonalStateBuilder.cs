﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Registration;
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
    internal class OrthogonalStateBuilder :
        IOrthogonalStateBuilder,
        IOverridenOrthogonalStateBuilder,
        IInternal,
        IVertexBuilder,
        IGraphBuilder,
        IBehaviorBuilder
    {   
        public Vertex Vertex { get; }

        public Graph Graph => Vertex.Graph;

        public IServiceCollection Services { get; }

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Vertex.Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Vertex.Graph.Version;

        public OrthogonalStateBuilder(Vertex vertex, IServiceCollection services)
        {
            Vertex = vertex;
            Services = services;
        }

        #region Events
        [DebuggerHidden]
        public IOrthogonalStateBuilder AddOnInitialize(Func<IStateActionContext, Task> actionAsync)
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
                        var inspector = await c.Executor.GetInspectorAsync();
                        
                        Trace.WriteLine($"⦗→s⦘ State Machine '{context.Context.Id.Name}:{context.Context.Id.Instance}': exception thrown '{e.Message}'");
                        if (!await inspector.OnStateInitializeExceptionAsync(context, e))
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
        public IOrthogonalStateBuilder AddOnFinalize(Func<IStateActionContext, Task> actionAsync)
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
                        var inspector = await c.Executor.GetInspectorAsync();

                        Trace.WriteLine($"⦗→s⦘ State Machine '{context.Context.Id.Name}:{context.Context.Id.Instance}': exception thrown '{e.Message}'");
                        if (!await inspector.OnStateFinalizeExceptionAsync(context, e))
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
        public IOrthogonalStateBuilder AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
        {
            foreach (var actionAsync in actionsAsync)
            {
                actionAsync.ThrowIfNull(nameof(actionAsync));

                var actionHandler = actionAsync.AddStateMachineInvocationContext(Vertex.Graph);

                Vertex.Entry.Actions.Add(async c =>
                    {
                        var context = new StateActionContext(c, Vertex, Constants.Entry);
                        try
                        {
                            await actionHandler(context);
                        }
                        catch (Exception e)
                        {
                            if (e is StateflowsDefinitionException)
                            {
                                throw;
                            }
                            else
                            {
                                var inspector = await c.Executor.GetInspectorAsync();

                                Trace.WriteLine($"⦗→s⦘ State Machine '{context.Context.Id.Name}:{context.Context.Id.Instance}': exception thrown '{e.Message}'");
                                if (!await inspector.OnStateEntryExceptionAsync(context, e))
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
            }

            return this;
        }

        [DebuggerHidden]
        public IOrthogonalStateBuilder AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
        {
            foreach (var actionAsync in actionsAsync)
            {
                actionAsync.ThrowIfNull(nameof(actionAsync));

                var actionHandler = actionAsync.AddStateMachineInvocationContext(Vertex.Graph);

                Vertex.Exit.Actions.Add(async c =>
                    {
                        var context = new StateActionContext(c, Vertex, Constants.Exit);
                        try
                        {
                            await actionHandler(context);
                        }
                        catch (Exception e)
                        {
                            if (e is StateflowsDefinitionException)
                            {
                                throw;
                            }
                            else
                            {
                                var inspector = await c.Executor.GetInspectorAsync();

                                Trace.WriteLine($"⦗→s⦘ State Machine '{context.Context.Id.Name}:{context.Context.Id.Instance}': exception thrown '{e.Message}'");
                                if (!await inspector.OnStateExitExceptionAsync(context, e))
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
            }

            return this;
        }
        #endregion

        #region Utils
        [DebuggerHidden]
        public IOrthogonalStateBuilder AddDeferredEvent<TEvent>()
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

            transitionBuildAction?.Invoke(new TransitionBuilder<TEvent>(edge, Services));

            return this;
        }

        [DebuggerHidden]
        public IOrthogonalStateBuilder AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction = null)
            => AddTransitionInternal(targetStateName, false, transitionBuildAction);

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
            => AddTransitionInternal<TEvent>(targetStateName, true, builder => transitionBuildAction?.Invoke(builder as IElseTransitionBuilder<TEvent>));

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
            => AddTransition<TEvent>(Constants.DefaultTransitionTarget, builder => transitionBuildAction?.Invoke(builder as IInternalTransitionBuilder<TEvent>));

        [DebuggerHidden]
        public IOrthogonalStateBuilder AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            => AddElseTransition<TEvent>(Constants.DefaultTransitionTarget, builder => transitionBuildAction?.Invoke(builder as IElseInternalTransitionBuilder<TEvent>));
        #endregion

        [DebuggerHidden]
        public IOverridenOrthogonalStateBuilder UseTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction)
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
        public IOverridenOrthogonalStateBuilder UseDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction)
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
        public IOverridenOrthogonalStateBuilder UseInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
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
        public IOverridenOrthogonalStateBuilder UseElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction)
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
        public IOverridenOrthogonalStateBuilder UseElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction)
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
        public IOverridenOrthogonalStateBuilder UseElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
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
        public IOrthogonalStateBuilder AddRegion(RegionBuildAction buildAction)
        {
            var region = new Region()
            {
                ParentVertex = Vertex,
                Graph = Vertex.Graph,
                OriginStateMachineName = Vertex.OriginStateMachineName
            };

            Vertex.Regions.Add(region);

            buildAction?.Invoke(new RegionBuilder(region, Services));

            return this;
        }

        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder IStateEntry<IOverridenOrthogonalStateBuilder>.AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnEntry(actionsAsync) as IOverridenOrthogonalStateBuilder;

        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder IStateExit<IOverridenOrthogonalStateBuilder>.AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
            => AddOnExit(actionsAsync) as IOverridenOrthogonalStateBuilder;

        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder ICompositeStateEvents<IOverridenOrthogonalStateBuilder>.AddOnInitialize(Func<IStateActionContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IOverridenOrthogonalStateBuilder;
        
        [DebuggerHidden]
        IOverridenOrthogonalStateBuilder ICompositeStateEvents<IOverridenOrthogonalStateBuilder>.AddOnFinalize(Func<IStateActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IOverridenOrthogonalStateBuilder;

        IOverridenOrthogonalStateBuilder IStateUtils<IOverridenOrthogonalStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as IOverridenOrthogonalStateBuilder;

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

            buildAction?.Invoke(new RegionBuilder(Vertex.Regions[index], Services));
            return this;
        }

        public string Name => Vertex.Name;
        public VertexType Type => Vertex.Type;
    }
}
