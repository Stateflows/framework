using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Exceptions;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Context;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal sealed class Executor : IDisposable, IStateflowsExecutor
    {
        public readonly Graph Graph;

        public bool StateHasChanged;

        public StateMachinesRegister Register { get; set; }

        public IServiceProvider ServiceProvider => ScopesStack.Peek().ServiceProvider;

        private readonly Stack<IServiceScope> ScopesStack = new Stack<IServiceScope>();

        private EventStatus EventStatus;
        private bool IsEventStatusOverriden;

        public void OverrideEventStatus(EventStatus eventStatus)
        {
            EventStatus = eventStatus;
            IsEventStatusOverriden = true;
        }

        public Executor(StateMachinesRegister register, Graph graph, IServiceProvider serviceProvider, StateflowsContext stateflowsContext, EventHolder @event)
        {
            Register = register;
            ScopesStack.Push(serviceProvider.CreateScope());
            Graph = graph;
            Context = new RootContext(stateflowsContext, this, @event);
            var logger = ServiceProvider.GetService<ILogger<Executor>>();
            Inspector = new Inspector(this, logger);
        }

        public void Dispose()
        {
            while (ScopesStack.Any())
            {
                EndScope();
            }
        }

        public void BeginScope()
        {
            ScopesStack.Push(ServiceProvider.CreateScope());
        }

        public void EndScope()
        {
            var scope = ScopesStack.Pop();
            scope.Dispose();
        }

        public readonly RootContext Context;

        public readonly Inspector Inspector;

        public IEnumerable<string> GetDeferredEvents()
            => VerticesTree.AllNodes_FromTheTop.SelectMany(vertex => vertex.Value.DeferredEvents).ToArray();

        public Tree<Vertex> VerticesTree { get; private set; } = null;

        public void RebuildVerticesTree()
        {
            VerticesTree = Context.StatesTree.Translate(
                vertexName => Graph.AllVertices[vertexName],
                vertexName => Graph.AllVertices.ContainsKey(vertexName)
            );
        }

        public IReadOnlyTree<string> GetStateTree()
            => VerticesTree.Translate(vertex => vertex.Name);

        public Task HydrateAsync()
        {
            RebuildVerticesTree();

            return Inspector.AfterHydrateAsync(new StateMachineActionContext(Context));
        }

        public Task DehydrateAsync()
            => Inspector.BeforeDehydrateAsync(new StateMachineActionContext(Context));

        public bool Initialized
            => VerticesTree.HasValue;

        public bool Finalized
            => VerticesTree.Value?.Type == VertexType.FinalState;

        public BehaviorStatus BehaviorStatus =>
            (Initialized, Finalized) switch
            {
                (false, false) => BehaviorStatus.NotInitialized,
                (true, false) => BehaviorStatus.Initialized,
                (true, true) => BehaviorStatus.Finalized,
                _ => BehaviorStatus.NotInitialized
            };

        public async Task<EventStatus> InitializeAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' hydrated?");

            if (!Initialized)
            {
                var result = await DoInitializeStateMachineAsync(eventHolder);

                if (
                    result == InitializationStatus.InitializedImplicitly ||
                    result == InitializationStatus.InitializedExplicitly
                )
                {
                    await DoInitializeCascadeAsync(Graph.InitialVertex);

                    await DoCompletionAsync();

                    if (result == InitializationStatus.InitializedExplicitly)
                    {
                        return EventStatus.Initialized;
                    }
                    else
                    {
                        if (eventHolder.Payload is Initialize)
                        {
                            return EventStatus.Initialized;
                        }
                        else
                        {
                            return EventStatus.Consumed;
                        }
                    }
                }
                else
                {
                    return result == InitializationStatus.NoSuitableInitializer
                        ? EventStatus.Rejected
                        : EventStatus.NotInitialized;
                }
            }

            return EventStatus.NotInitialized;
        }

        public async Task<bool> ExitAsync()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' hydrated?");

            if (Initialized)
            {
                foreach (var vertex in VerticesTree.AllNodes_ChildrenFirst.Select(node => node.Value))
                {
                    await DoExitAsync(vertex);
                }

                await DoFinalizeStateMachineAsync();

                Context.StatesTree.Root = null;
                StateHasChanged = true;

                return true;
            }

            return false;
        }

        public void Reset(ResetMode resetMode)
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' hydrated?");

            if (Initialized)
            {
                Context.Context.Values.Clear();

                if (resetMode != ResetMode.KeepVersionAndSubscriptions) // KeepSubscriptions || Full
                {
                    Context.Context.Version = 0;

                    if (resetMode != ResetMode.KeepSubscriptions) // Full
                    {
                        Context.Context.Deleted = true;
                    }
                }
            }
        }

        private async Task DoInitializeCascadeAsync(Vertex vertex, bool omitRoot = false)
        {
            if (!omitRoot)
            {
                await DoEntryAsync(vertex);
            }

            Context.StatesTree.AddTo(vertex.Identifier, vertex?.ParentRegion?.ParentVertex?.Identifier);

            StateHasChanged = true;

            if (vertex.Regions.Any() && !omitRoot)
            {
                await DoInitializeStateAsync(vertex);
            }

            await Task.WhenAll(vertex.Regions
                .Where(region => region.InitialVertex != null)
                .Select(region => DoInitializeCascadeAsync(region.InitialVertex))
            );

            Context.StatesTree.Sort((v1, v2) =>
            {
                var region1 = Graph.AllVertices[v1]?.ParentRegion;
                var index1 = region1?.ParentVertex.Regions.IndexOf(region1) ?? -1;
                var region2 = Graph.AllVertices[v2]?.ParentRegion;
                var index2 = region2?.ParentVertex.Regions.IndexOf(region2) ?? -1;

                if (index1 < index2)
                {
                    return -1;
                }
                else
                {
                    if (index1 > index2)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            });
        }

        public IEnumerable<string> GetExpectedEventNames()
            => GetExpectedEvents()
                .Where(type => !type.IsSubclassOf(typeof(TimeEvent)))
                .Where(type => type != typeof(Startup))
                .Where(type => type != typeof(Completion))
                .Select(type => type.GetEventName())
                .ToArray();

        public IEnumerable<Type> GetExpectedEvents()
        {
            var currentStack = VerticesTree.AllNodes_FromTheTop.Select(node => node.Value).ToArray() ?? new Vertex[0];

            return currentStack.Any()
                ? currentStack
                    .SelectMany(vertex => vertex.Regions
                        .SelectMany(region => region.Edges.Values)
                    )
                    .SelectMany(edge => edge.ActualTriggerTypes)
                    .Distinct()
                    .ToArray()
                : Graph.InitializerTypes
                    .ToArray();
        }

        public async Task<EventStatus> ProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            StateHasChanged = false;

            var result = EventStatus.Rejected;

            if (Initialized)
            {
                result = await DoProcessAsync(eventHolder);
            }

            if (IsEventStatusOverriden)
            {
                result = EventStatus;
            }

            return result;
        }

        private bool TryDeferEvent<TEvent>(EventHolder<TEvent> eventHolder)
        {
            var deferredEvents = GetDeferredEvents();
            if (deferredEvents.Any() && deferredEvents.Contains(eventHolder.Name))
            {
                Context.DeferredEvents.Add(eventHolder);
                return true;
            }
            return false;
        }

        private async Task DispatchNextDeferredEvent()
        {
            var deferredEvents = GetDeferredEvents();
            foreach (var eventHolder in Context.DeferredEvents)
            {
                if (!deferredEvents.Any() || !deferredEvents.Contains(eventHolder.Name))
                {
                    Context.DeferredEvents.Remove(eventHolder);

                    Context.SetEvent(eventHolder);

                    RebuildVerticesTree();

                    await eventHolder.DoProcessAsync(this);

                    Context.ClearEvent();

                    break;
                }
            }
        }

        Task<EventStatus> IStateflowsExecutor.DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
            => DoProcessAsync(eventHolder);

        private async Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            Debug.Assert(Context != null, $"Context is not available. Is state machine '{Graph.Name}' hydrated?");

            var currentStack = VerticesTree.AllNodes_FromTheTop.Select(node => node.Value).ToArray();

            var deferred = true;

            var result = EventStatus.NotConsumed;

            if (!TryDeferEvent(eventHolder))
            {
                deferred = false;

                var activatedEdges = new List<Edge>();

                Edge lastActivatedEdge = null;

                foreach (var vertex in currentStack)
                {
                    var edges = vertex.OrderedEdges
                        .SelectMany(edge => edge.GetActualEdges())
                        .ToArray();

                    foreach (var edge in edges)
                    {
                        if (
                            eventHolder.Triggers(edge) && 
                            await DoGuardAsync<TEvent>(edge) &&
                            (
                                lastActivatedEdge == null ||
                                edge.Source.IsOrthogonalTo(lastActivatedEdge.Source)
                            )
                        )
                        {
                            activatedEdges.Add(edge);

                            lastActivatedEdge = edge;
                        }
                    }
                }

                foreach (var edge in activatedEdges)
                {
                    Context.AddExecutionStep(
                        edge.SourceName,
                        edge.TargetName == string.Empty
                            ? null
                            : edge.TargetName,
                        eventHolder.Payload
                    );

                    await DoConsumeAsync<TEvent>(edge);

                    result = EventStatus.Consumed;
                }

                if (result == EventStatus.Consumed)
                {
                    await DoCompletionAsync();
                }
            }

            if (!deferred)
            {
                RebuildVerticesTree();

                await DispatchNextDeferredEvent();
            }

            return deferred
                ? EventStatus.Deferred
                : result;
        }

        [DebuggerStepThrough]
        private async Task<bool> DoGuardAsync<TEvent>(Edge edge)
        {
            BeginScope();

            var context = new GuardContext<TEvent>(Context, edge);

            await Inspector.BeforeTransitionGuardAsync(context);

            var result = await edge.Guards.WhenAll(Context);

            await Inspector.AfterGuardAsync(context, result);


            return result;
        }

        private async Task DoEffectAsync<TEvent>(Edge edge)

        {
            BeginScope();

            var context = new TransitionContext<TEvent>(Context, edge);

            await Inspector.BeforeEffectAsync(context);

            await edge.Effects.WhenAll(Context);

            await Inspector.AfterEffectAsync(context);

            EndScope();
        }

        public async Task<InitializationStatus> DoInitializeStateMachineAsync(EventHolder @event)
        {
            BeginScope();

            InitializationStatus result;

            var initializer = Graph.Initializers.ContainsKey(@event.Name)
                ? Graph.Initializers[@event.Name]
                : Graph.DefaultInitializer;

            var context = new StateMachineInitializationContext(Context);
            await Inspector.BeforeStateMachineInitializeAsync(context);

            if (initializer != null)
            {
                try
                {
                    result = await initializer.WhenAll(Context)
                        ? initializer == Graph.DefaultInitializer
                            ? InitializationStatus.InitializedImplicitly
                            : InitializationStatus.InitializedExplicitly
                        : InitializationStatus.NotInitialized;
                }
                catch (Exception e)
                {
                    if (!await Inspector.OnStateMachineInitializationExceptionAsync(context, e))
                    {
                        throw;
                    }
                    else
                    {
                        throw new BehaviorExecutionException(e);
                    }
                }
            }
            else
            {
                result = Graph.Initializers.Any()
                    ? InitializationStatus.NoSuitableInitializer
                    : InitializationStatus.InitializedImplicitly;
            }

            await Inspector.AfterStateMachineInitializeAsync(
                context,
                result == InitializationStatus.InitializedImplicitly ||
                result == InitializationStatus.InitializedExplicitly
            );

            EndScope();

            return result;
        }

        public async Task DoFinalizeStateMachineAsync()
        {
            BeginScope();

            var context = new StateMachineActionContext(Context);
            await Inspector.BeforeStateMachineFinalizeAsync(context);

            try
            {
                await Graph.Finalize.WhenAll(Context);
            }
            catch (Exception e)
            {
                if (!await Inspector.OnStateMachineFinalizationExceptionAsync(context, e))
                {
                    throw;
                }
                else
                {
                    throw new BehaviorExecutionException(e);
                }
            }

            await Inspector.AfterStateMachineFinalizeAsync(context);

            EndScope();
        }

        public async Task DoInitializeStateAsync(Vertex vertex)
        {
            BeginScope();

            var context = new StateActionContext(Context, vertex, Constants.Initialize);
            await Inspector.BeforeStateInitializeAsync(context);

            await vertex.Initialize.WhenAll(Context);

            await Inspector.AfterStateInitializeAsync(context);

            EndScope();
        }

        public async Task DoFinalizeStateAsync(Vertex vertex)
        {
            BeginScope();

            var context = new StateActionContext(Context, vertex, Constants.Finalize);
            await Inspector.BeforeStateFinalizeAsync(context);

            await vertex.Finalize.WhenAll(Context);

            await Inspector.AfterStateFinalizeAsync(context);

            EndScope();
        }

        public async Task DoEntryAsync(Vertex vertex)
        {
            BeginScope();

            var context = new StateActionContext(Context, vertex, Constants.Entry);
            await Inspector.BeforeStateEntryAsync(context);

            await vertex.Entry.WhenAll(Context);

            await Inspector.AfterStateEntryAsync(context);

            EndScope();
        }

        private async Task DoExitAsync(Vertex vertex)
        {
            BeginScope();

            var context = new StateActionContext(Context, vertex, Constants.Exit);
            await Inspector.BeforeStateExitAsync(context);

            await vertex.Exit.WhenAll(Context);

            await Inspector.AfterStateExitAsync(context);

            EndScope();
        }

        private async Task DoConsumeAsync<TEvent>(Edge edge)
        {
            var exitingVertices = new List<Vertex>();
            var enteringVertices = new List<Vertex>();

            var vertex = edge.Source?.ParentRegion?.ParentVertex;
            while (vertex != null)
            {
                exitingVertices.Insert(0, vertex);
                vertex = vertex.ParentRegion?.ParentVertex;
            }

            if (VerticesTree.TryFind(edge.Source, out var node))
            {
                exitingVertices.AddRange(node.AllNodes_FromTheBottom.Select(node => node.Value));
            }

            vertex = edge.Target;
            while (vertex != null)
            {
                enteringVertices.Insert(0, vertex);
                vertex = vertex.ParentRegion?.ParentVertex;
            }

            if (enteringVertices.Any() && exitingVertices.Any())
            {
                while (
                    enteringVertices.Any() &&
                    exitingVertices.Any() &&
                    enteringVertices[0] == exitingVertices[0] &&
                    enteringVertices[0] != edge.Target
                )
                {
                    enteringVertices.RemoveAt(0);
                    exitingVertices.RemoveAt(0);
                }

                exitingVertices.Reverse();
                foreach (var exitingVertex in exitingVertices)
                {
                    await DoExitAsync(exitingVertex);

                    Context.StatesTree.Remove(exitingVertex.Identifier);

                    StateHasChanged = true;
                }
            }

            await DoEffectAsync<TEvent>(edge);

            List<Region> regions = new List<Region>();

            for (var i = 0; i < enteringVertices.Count; i++)
            {
                var enteringVertex = enteringVertices[i];

                await DoEntryAsync(enteringVertex);

                if (enteringVertex.Regions.Any())
                {
                    await DoInitializeStateAsync(enteringVertex);
                }

                if (enteringVertex.Regions.Count > 1 && i < enteringVertices.Count - 1)
                {
                    var enteredRegion = enteringVertices[i + 1].ParentRegion;

                    bool initializing = true;
                    foreach (var region in enteringVertex.Regions)
                    {
                        if (region == enteredRegion)
                        {
                            initializing = false;
                            continue;
                        }

                        if (initializing)
                        {
                            await DoInitializeCascadeAsync(region.InitialVertex);
                        }
                        else
                        {
                            regions.Add(region);
                        }
                    }
                }

                if (enteringVertex != enteringVertices.Last())
                {
                    Context.StatesTree.AddTo(enteringVertex.Identifier, enteringVertex?.ParentRegion?.ParentVertex?.Identifier);

                    StateHasChanged = true;
                }
            }

            if (edge.Target != null && enteringVertices.Any())
            {
                var topVertex = enteringVertices.Last();

                await DoInitializeCascadeAsync(topVertex, true);

                if (topVertex.Type == VertexType.FinalState)
                {
                    if (topVertex.ParentRegion is null)
                    {
                        await DoFinalizeStateMachineAsync();
                    }
                    else
                    {
                        await DoFinalizeStateAsync(topVertex.ParentRegion.ParentVertex);
                    }
                }
            }

            foreach (var region in regions)
            {
                await DoInitializeCascadeAsync(region.InitialVertex);
            }
        }

        private async Task<bool> DoCompletionAsync()
        {
            var completionEventHolder = (new Completion()).ToEventHolder();
            Context.SetEvent(completionEventHolder);

            RebuildVerticesTree();

            var result = await DoProcessAsync(completionEventHolder);

            Context.ClearEvent();

            return result == EventStatus.Consumed;
        }

        public IStateMachine GetStateMachine(Type stateMachineType)
        {
            var stateMachine = ServiceProvider.GetService(stateMachineType) as IStateMachine;

            return stateMachine;
        }

        public TDefaultInitializer GetDefaultInitializer<TDefaultInitializer>(IStateMachineInitializationContext context)
            where TDefaultInitializer : class, IDefaultInitializer
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = null;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var initializer = ServiceProvider.GetService<TDefaultInitializer>();

            return initializer;
        }

        public TInitializer GetInitializer<TInitializer, TInitializationEvent>(IStateMachineInitializationContext<TInitializationEvent> context)
            where TInitializer : class, IInitializer<TInitializationEvent>
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = null;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var initializer = ServiceProvider.GetService<TInitializer>();

            return initializer;
        }

        public TFinalizer GetFinalizer<TFinalizer>(IStateMachineActionContext context)
            where TFinalizer : class, IFinalizer
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = null;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var initializer = ServiceProvider.GetService<TFinalizer>();

            return initializer;
        }

        public TState GetState<TState>(IStateActionContext context)
            where TState : class, IState
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = context.CurrentState.Values;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            StateMachinesContextHolder.StateContext.Value = context.CurrentState;
            StateMachinesContextHolder.TransitionContext.Value = null;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var state = ServiceProvider.GetService<TState>();

            return state;
        }

        public TTransition GetTransition<TTransition, TEvent>(ITransitionContext<TEvent> context)
            where TTransition : class, ITransition<TEvent>

        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = context.SourceState.Values;
            ContextValues.TargetStateValuesHolder.Value = context.TargetState?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var transition = ServiceProvider.GetService<TTransition>();

            return transition;
        }

        public TTransitionGuard GetTransitionGuard<TTransitionGuard, TEvent>(ITransitionContext<TEvent> context)
            where TTransitionGuard : class, ITransitionGuard<TEvent>

        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = context.SourceState.Values;
            ContextValues.TargetStateValuesHolder.Value = context.TargetState?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var transition = ServiceProvider.GetService<TTransitionGuard>();

            return transition;
        }

        public TTransitionEffect GetTransitionEffect<TTransitionEffect, TEvent>(ITransitionContext<TEvent> context)
            where TTransitionEffect : class, ITransitionEffect<TEvent>

        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = context.SourceState.Values;
            ContextValues.TargetStateValuesHolder.Value = context.TargetState?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var transition = ServiceProvider.GetService<TTransitionEffect>();

            return transition;
        }

        public TDefaultTransition GetDefaultTransition<TDefaultTransition>(ITransitionContext<Completion> context)
            where TDefaultTransition : class, IDefaultTransition
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = context.SourceState.Values;
            ContextValues.TargetStateValuesHolder.Value = context.TargetState?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var transition = ServiceProvider.GetService<TDefaultTransition>();

            return transition;
        }

        public TDefaultTransitionGuard GetDefaultTransitionGuard<TDefaultTransitionGuard>(ITransitionContext<Completion> context)
            where TDefaultTransitionGuard : class, IDefaultTransitionGuard
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = context.SourceState.Values;
            ContextValues.TargetStateValuesHolder.Value = context.TargetState?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var transition = ServiceProvider.GetService<TDefaultTransitionGuard>();

            return transition;
        }

        public TDefaultTransitionEffect GetDefaultTransitionEffect<TDefaultTransitionEffect>(ITransitionContext<Completion> context)
            where TDefaultTransitionEffect : class, IDefaultTransitionEffect
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = context.SourceState.Values;
            ContextValues.TargetStateValuesHolder.Value = context.TargetState?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var transition = ServiceProvider.GetService<TDefaultTransitionEffect>();

            return transition;
        }
    }
}
