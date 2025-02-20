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

        public readonly StateMachinesRegister Register;

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
        }

        // public Task BuildAsync()
        //     => Inspector.BuildAsync();

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

        private Inspector inspector;

        public async Task<Inspector> GetInspectorAsync()
        {
            if (inspector == null)
            {
                var logger = ServiceProvider.GetService<ILogger<Executor>>();
                inspector = new Inspector(this, logger);

                await inspector.BuildAsync();
            }
            
            return inspector;
        }

        private string[] GetDeferredEvents()
            => VerticesTree.GetAllNodes_FromTheTop().SelectMany(vertex => vertex.Value.DeferredEvents).ToArray();

        public Tree<Vertex> VerticesTree { get; private set; } = null;

        private void RebuildVerticesTree()
        {
            VerticesTree = Context.StatesTree.Translate(
                vertexName => Graph.AllVertices[vertexName],
                vertexName => Graph.AllVertices.ContainsKey(vertexName)
            );
        }

        public IReadOnlyTree<string> GetStateTree()
            => VerticesTree.Translate(vertex => vertex.Name);

        public async Task HydrateAsync()
        {
            RebuildVerticesTree();

            await (await GetInspectorAsync()).AfterHydrateAsync(new StateMachineActionContext(Context));
        }

        public async Task DehydrateAsync()
            => await (await GetInspectorAsync()).BeforeDehydrateAsync(new StateMachineActionContext(Context));

        public bool Initialized
            => VerticesTree.HasValue;

        private bool Finalized
            => VerticesTree.Value?.Type == VertexType.FinalState;

        public BehaviorStatus BehaviorStatus =>
            (Initialized, Finalized) switch
            {
                (false, false) => BehaviorStatus.NotInitialized,
                (true, false) => BehaviorStatus.Initialized,
                (true, true) => BehaviorStatus.Finalized,
                _ => BehaviorStatus.NotInitialized
            };

        [DebuggerHidden]
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

        [DebuggerHidden]
        public async Task<bool> ExitAsync()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' hydrated?");

            if (!Initialized) return false;
            
            foreach (var vertex in VerticesTree.GetAllNodes_ChildrenFirst().Select(node => node.Value))
            {
                await DoExitAsync(vertex);
            }

            await DoFinalizeStateMachineAsync();

            Context.StatesTree.Root = null;
            StateHasChanged = true;

            return true;

        }

        [DebuggerHidden]
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

        [DebuggerHidden]
        private async Task DoInitializeCascadeAsync(Vertex vertex, bool omitRoot = false)
        {
            if (!Context.StatesTree.Contains(vertex.Identifier))
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
                    return index1 > index2 ? 1 : 0;
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

        private IEnumerable<Type> GetExpectedEvents()
        {
            var currentStack = VerticesTree.GetAllNodes_FromTheTop().Select(node => node.Value).ToArray() ?? new Vertex[0];

            return currentStack.Any()
                ? currentStack
                    .SelectMany(vertex => vertex.Edges.Values)
                    .SelectMany(edge => edge.ActualTriggerTypes)
                    .Distinct()
                    .ToArray()
                : Graph.InitializerTypes
                    .ToArray();
        }

        [DebuggerHidden]
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

        [DebuggerHidden]
        private bool TryDeferEvent<TEvent>(EventHolder<TEvent> eventHolder)
        {
            var deferredEvents = GetDeferredEvents();
            if (!deferredEvents.Any() || !deferredEvents.Contains(eventHolder.Name))
            {
                return false;
            }
            
            Context.DeferredEvents.Add(eventHolder);
            
            return true;
        }

        [DebuggerHidden]
        private async Task DispatchNextDeferredEvent()
        {
            var deferredEvents = GetDeferredEvents();
            foreach (var eventHolder in Context.DeferredEvents.Where(eventHolder => !deferredEvents.Any() || !deferredEvents.Contains(eventHolder.Name)))
            {
                Context.DeferredEvents.Remove(eventHolder);

                Context.SetEvent(eventHolder);

                RebuildVerticesTree();

                await eventHolder.DoProcessAsync(this);

                Context.ClearEvent();

                break;
            }
        }

        [DebuggerHidden]
        Task<EventStatus> IStateflowsExecutor.DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
            => DoProcessAsync(eventHolder);

        [DebuggerHidden]
        private async Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            Debug.Assert(Context != null, $"Context is not available. Is state machine '{Graph.Name}' hydrated?");

            var currentStack = VerticesTree.GetAllNodes_FromTheTop().Select(node => node.Value).ToArray();

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

                    if (vertex.Type == VertexType.Fork)
                    {
                        activatedEdges.AddRange(edges);
                    }
                    else
                    {
                        foreach (var edge in edges)
                        {
                            if (!eventHolder.Triggers(edge) ||
                                (
                                    lastActivatedEdge != null &&
                                    !edge.Source.IsOrthogonalTo(lastActivatedEdge.Source)
                                ) ||
                                !await DoGuardAsync<TEvent>(edge)
                            )
                            {
                                continue;
                            }
                            activatedEdges.Add(edge);

                            lastActivatedEdge = edge;
                        }
                        
                    }
                }

                // var forks = new Dictionary<Vertex, List<Edge>>();
                var joins = new Dictionary<Vertex, List<Edge>>();
                foreach (var edge in activatedEdges)
                {
                    if (
                        !Context.StatesTree.Contains(edge.Source.Identifier) &&
                        edge.Source.Type != VertexType.Fork
                    )
                    {
                        continue;
                    }
                    
                    if (edge.Target?.Type == VertexType.Join)
                    {
                        if (!joins.TryGetValue(edge.Target, out var edges))
                        {
                            edges = new List<Edge>();
                            joins[edge.Target] = edges;
                        }
                        
                        edges.Add(edge);
                    
                        if (edges.Count == edge.Target.IncomingEdges.Count())
                        {
                            // complete join
                    
                            await DoJoinAsync(edges, edge.Target);
                            
                            result = EventStatus.Consumed;
                        }
                    }
                    else
                    {
                        Context.AddExecutionStep(
                            edge.SourceName,
                            edge.TargetName == string.Empty
                                ? null
                                : edge.TargetName,
                            eventHolder.Payload
                        );

                        await DoConsumeAsync<TEvent>(
                            edge,
                            true
                        );
                        
                        result = EventStatus.Consumed;
                    }
                }

                if (result == EventStatus.Consumed)
                {
                    await DoCompletionAsync();
                }
            }

            if (deferred)
            {
                return EventStatus.Deferred;
            }
            
            RebuildVerticesTree();

            await DispatchNextDeferredEvent();

            return result;
        }

        [DebuggerHidden]
        private async Task<bool> DoGuardAsync<TEvent>(Edge edge)
        {
            BeginScope();

            var context = new GuardContext<TEvent>(Context, edge);

            var inspector = await GetInspectorAsync();

            await inspector.BeforeTransitionGuardAsync(context);

            var result = await edge.Guards.WhenAll(Context);

            await inspector.AfterGuardAsync(context, result);


            return result;
        }

        [DebuggerHidden]
        private async Task DoEffectAsync<TEvent>(Edge edge)
        {
            BeginScope();

            var context = new TransitionContext<TEvent>(Context, edge);

            var inspector = await GetInspectorAsync();
            
            await inspector.BeforeEffectAsync(context);

            await edge.Effects.WhenAll(Context);

            await inspector.AfterEffectAsync(context);

            EndScope();
        }

        [DebuggerHidden]
        private async Task<InitializationStatus> DoInitializeStateMachineAsync(EventHolder @event)
        {
            BeginScope();

            InitializationStatus result;

            var initializer = Graph.Initializers.TryGetValue(@event.Name, out var graphInitializer)
                ? graphInitializer
                : Graph.DefaultInitializer;
            
            var inspector = await GetInspectorAsync();

            var context = new StateMachineInitializationContext(Context);
            await inspector.BeforeStateMachineInitializeAsync(context);

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
                    if (!await inspector.OnStateMachineInitializationExceptionAsync(context, e))
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

            await inspector.AfterStateMachineInitializeAsync(
                context,
                result == InitializationStatus.InitializedImplicitly ||
                result == InitializationStatus.InitializedExplicitly
            );

            EndScope();

            return result;
        }

        [DebuggerHidden]
        private async Task DoFinalizeStateMachineAsync()
        {
            BeginScope();
            
            var inspector = await GetInspectorAsync();

            var context = new StateMachineActionContext(Context);
            await inspector.BeforeStateMachineFinalizeAsync(context);

            try
            {
                await Graph.Finalize.WhenAll(Context);
            }
            catch (Exception e)
            {
                if (!await inspector.OnStateMachineFinalizationExceptionAsync(context, e))
                {
                    throw;
                }
                else
                {
                    throw new BehaviorExecutionException(e);
                }
            }

            await inspector.AfterStateMachineFinalizeAsync(context);

            EndScope();
        }

        [DebuggerHidden]
        private async Task DoInitializeStateAsync(Vertex vertex)
        {
            BeginScope();
            
            var inspector = await GetInspectorAsync();

            var context = new StateActionContext(Context, vertex, Constants.Initialize);
            await inspector.BeforeStateInitializeAsync(context);

            await vertex.Initialize.WhenAll(Context);

            await inspector.AfterStateInitializeAsync(context);

            EndScope();
        }

        [DebuggerHidden]
        private async Task DoFinalizeStateAsync(Vertex vertex)
        {
            BeginScope();
            
            var inspector = await GetInspectorAsync();

            var context = new StateActionContext(Context, vertex, Constants.Finalize);
            await inspector.BeforeStateFinalizeAsync(context);

            await vertex.Finalize.WhenAll(Context);

            await inspector.AfterStateFinalizeAsync(context);

            EndScope();
        }
        
        [DebuggerHidden]
        private async Task DoEntryAsync(Vertex vertex)
        {
            BeginScope();
            
            var inspector = await GetInspectorAsync();

            var context = new StateActionContext(Context, vertex, Constants.Entry);
            await inspector.BeforeStateEntryAsync(context);

            await vertex.Entry.WhenAll(Context);

            await inspector.AfterStateEntryAsync(context);

            EndScope();
        }

        [DebuggerHidden]
        private async Task DoExitAsync(Vertex vertex)
        {
            BeginScope();
            
            var inspector = await GetInspectorAsync();

            var context = new StateActionContext(Context, vertex, Constants.Exit);
            await inspector.BeforeStateExitAsync(context);

            await vertex.Exit.WhenAll(Context);

            await inspector.AfterStateExitAsync(context);

            EndScope();
        }

        [DebuggerHidden]
        private async Task DoConsumeAsync<TEvent>(Edge edge, bool exitSource)
        {
            var exitingVertices = new List<Vertex>();
            var enteringVertices = new List<Vertex>();

            if (exitSource)
            {
                var exitingVertex = edge.Source?.ParentRegion?.ParentVertex;
                while (exitingVertex != null)
                {
                    exitingVertices.Insert(0, exitingVertex);
                    exitingVertex = exitingVertex.ParentRegion?.ParentVertex;
                }

                if (VerticesTree.TryFind(edge.Source, out var node))
                {
                    exitingVertices.AddRange(node.GetAllNodes_FromTheBottom().Select(n => n.Value));
                }
            }

            var vertex = edge.Target;
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

                // exit non-exited regions of exited orthogonal states
                exitingVertices = exitingVertices.SelectMany(exitingVertex => exitingVertex.GetBranch().Reverse()).Distinct().ToList();

                foreach (var exitingVertex in exitingVertices.Where(exitingVertex => Context.StatesTree.TryFind(exitingVertex.Identifier, out var _)))
                {
                    await DoExitAsync(exitingVertex);

                    Context.StatesTree.Remove(exitingVertex.Identifier);

                    StateHasChanged = true;
                }
            }
            
            RebuildVerticesTree();
            
            await DoEffectAsync<TEvent>(edge);

            var regions = new List<Region>();

            for (var i = 0; i < enteringVertices.Count; i++)
            {
                var enteringVertex = enteringVertices[i];

                if (!Context.StatesTree.Contains(enteringVertex.Identifier))
                {
                    await DoEntryAsync(enteringVertex);

                    if (enteringVertex.Regions.Any())
                    {
                        await DoInitializeStateAsync(enteringVertex);
                    }
                }

                if (enteringVertex.Regions.Count > 1 && i < enteringVertices.Count - 1)
                {
                    var enteredRegion = enteringVertices[i + 1].ParentRegion;

                    var initializing = true;
                    foreach (var region in enteringVertex.Regions)
                    {
                        if (region == enteredRegion)
                        {
                            initializing = false;
                            continue;
                        }

                        if (initializing)
                        {
                            if (region.InitialVertex != null)
                            {
                                await DoInitializeCascadeAsync(region.InitialVertex);
                            }
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
            
            await Task.WhenAll(regions
                .Where(region => region.InitialVertex != null)
                .Select(region => DoInitializeCascadeAsync(region.InitialVertex))
            );
        }
        
        [DebuggerHidden]
        private async Task DoJoinAsync(IEnumerable<Edge> edges, Vertex join)
        {
            var enteringVertices = new List<Vertex>();
            while (join != null)
            {
                enteringVertices.Insert(0, join);
                join = join.ParentRegion?.ParentVertex;
            }
            
            foreach (var edge in edges)
            {
                var exitingVertices = new List<Vertex>();

                var vertex = edge.Source?.ParentRegion?.ParentVertex;
                while (vertex != null)
                {
                    exitingVertices.Insert(0, vertex);
                    vertex = vertex.ParentRegion?.ParentVertex;
                }

                if (VerticesTree.TryFind(edge.Source, out var node))
                {
                    exitingVertices.AddRange(node.GetAllNodes_FromTheBottom().Select(n => n.Value));
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
                        if (Context.StatesTree.TryFind(exitingVertex.Identifier, out var treeNode) && treeNode.Nodes.Any())
                        {
                            continue;
                        }
                        
                        await DoExitAsync(exitingVertex);

                        Context.StatesTree.Remove(exitingVertex.Identifier);

                        StateHasChanged = true;
                    }
                }

                await DoEffectAsync<Completion>(edge);
            }

            var regions = new List<Region>();

            for (var i = 0; i < enteringVertices.Count; i++)
            {
                var enteringVertex = enteringVertices[i];

                if (!Context.StatesTree.Contains(enteringVertex.Identifier))
                {
                    await DoEntryAsync(enteringVertex);

                    if (enteringVertex.Regions.Any())
                    {
                        await DoInitializeStateAsync(enteringVertex);
                    }
                }

                if (enteringVertex.Regions.Count > 1 && i < enteringVertices.Count - 1)
                {
                    var enteredRegion = enteringVertices[i + 1].ParentRegion;

                    var initializing = true;
                    foreach (var region in enteringVertex.Regions)
                    {
                        if (region == enteredRegion)
                        {
                            initializing = false;
                            continue;
                        }

                        if (initializing)
                        {
                            if (region.InitialVertex != null)
                            {
                                await DoInitializeCascadeAsync(region.InitialVertex);
                            }
                        }
                        else
                        {
                            regions.Add(region);
                        }
                    }
                }

                if (!enteringVertex.Regions.Any() || enteringVertex != enteringVertices.Last())
                {
                    Context.StatesTree.AddTo(enteringVertex.Identifier, enteringVertex?.ParentRegion?.ParentVertex?.Identifier);

                    StateHasChanged = true;
                }
            }
            
            await Task.WhenAll(regions
                .Where(region => region.InitialVertex != null)
                .Select(region => DoInitializeCascadeAsync(region.InitialVertex))
            );
        }
        
        [DebuggerHidden]
        private async Task<bool> DoCompletionAsync()
        {
            var completionEventHolder = (new Completion()).ToEventHolder();
            Context.SetEvent(completionEventHolder);

            RebuildVerticesTree();

            var result = await DoProcessAsync(completionEventHolder);

            Context.ClearEvent();

            return result == EventStatus.Consumed;
        }

        public async Task<IStateMachine> GetStateMachineAsync(Type stateMachineType)
        {
            return await StateflowsActivator.CreateInstanceAsync(ServiceProvider, stateMachineType, "state machine") as IStateMachine;
        }

        public Task<TDefaultInitializer> GetDefaultInitializerAsync<TDefaultInitializer>(IStateMachineInitializationContext context)
            where TDefaultInitializer : class, IDefaultInitializer
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = null;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            return StateflowsActivator.CreateInstanceAsync<TDefaultInitializer>(ServiceProvider, "default initializer");
        }

        public Task<TInitializer> GetInitializerAsync<TInitializer, TInitializationEvent>(IStateMachineInitializationContext<TInitializationEvent> context)
            where TInitializer : class, IInitializer<TInitializationEvent>
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = null;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            return StateflowsActivator.CreateInstanceAsync<TInitializer>(ServiceProvider, "initializer");
        }

        public Task<TFinalizer> GetFinalizerAsync<TFinalizer>(IStateMachineActionContext context)
            where TFinalizer : class, IFinalizer
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = null;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            return StateflowsActivator.CreateInstanceAsync<TFinalizer>(ServiceProvider, "finalizer");
        }

        public Task<TState> GetStateAsync<TState>(IStateActionContext context)
            where TState : class, IState
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = context.CurrentState.Values;
            ContextValues.ParentStateValuesHolder.Value = context.CurrentState.TryGetParent(out var parent) ? parent.Values : null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            StateMachinesContextHolder.StateContext.Value = context.CurrentState;
            StateMachinesContextHolder.TransitionContext.Value = null;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            return StateflowsActivator.CreateInstanceAsync<TState>(ServiceProvider, "state");
        }

        public Task<TTransition> GetTransitionAsync<TTransition, TEvent>(ITransitionContext<TEvent> context)
            where TTransition : class, ITransition<TEvent>
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = context.Source.TryGetParent(out var parent) ? parent.Values : null;
            ContextValues.SourceStateValuesHolder.Value = context.Source.Values;
            ContextValues.TargetStateValuesHolder.Value = context.Target?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            return StateflowsActivator.CreateInstanceAsync<TTransition>(ServiceProvider, "transition");
        }

        public Task<TTransitionGuard> GetTransitionGuardAsync<TTransitionGuard, TEvent>(ITransitionContext<TEvent> context)
            where TTransitionGuard : class, ITransitionGuard<TEvent>

        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = context.Source.TryGetParent(out var parent) ? parent.Values : null;
            ContextValues.SourceStateValuesHolder.Value = context.Source.Values;
            ContextValues.TargetStateValuesHolder.Value = context.Target?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            return StateflowsActivator.CreateInstanceAsync<TTransitionGuard>(ServiceProvider, "transition guard");
        }

        public Task<TTransitionEffect> GetTransitionEffectAsync<TTransitionEffect, TEvent>(ITransitionContext<TEvent> context)
            where TTransitionEffect : class, ITransitionEffect<TEvent>

        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = context.Source.TryGetParent(out var parent) ? parent.Values : null;
            ContextValues.SourceStateValuesHolder.Value = context.Source.Values;
            ContextValues.TargetStateValuesHolder.Value = context.Target?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            return StateflowsActivator.CreateInstanceAsync<TTransitionEffect>(ServiceProvider, "transition effect");
        }

        public Task<TDefaultTransition> GetDefaultTransitionAsync<TDefaultTransition>(ITransitionContext<Completion> context)
            where TDefaultTransition : class, IDefaultTransition
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = context.Source.TryGetParent(out var parent) ? parent.Values : null;
            ContextValues.SourceStateValuesHolder.Value = context.Source.Values;
            ContextValues.TargetStateValuesHolder.Value = context.Target?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            return StateflowsActivator.CreateInstanceAsync<TDefaultTransition>(ServiceProvider, "default transition");
        }

        public Task<TDefaultTransitionGuard> GetDefaultTransitionGuardAsync<TDefaultTransitionGuard>(ITransitionContext<Completion> context)
            where TDefaultTransitionGuard : class, IDefaultTransitionGuard
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = context.Source.TryGetParent(out var parent) ? parent.Values : null;
            ContextValues.SourceStateValuesHolder.Value = context.Source.Values;
            ContextValues.TargetStateValuesHolder.Value = context.Target?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            return StateflowsActivator.CreateInstanceAsync<TDefaultTransitionGuard>(ServiceProvider, "default transition guard");
        }

        public Task<TDefaultTransitionEffect> GetDefaultTransitionEffectAsync<TDefaultTransitionEffect>(ITransitionContext<Completion> context)
            where TDefaultTransitionEffect : class, IDefaultTransitionEffect
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = context.Source.TryGetParent(out var parent) ? parent.Values : null;
            ContextValues.SourceStateValuesHolder.Value = context.Source.Values;
            ContextValues.TargetStateValuesHolder.Value = context.Target?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            return StateflowsActivator.CreateInstanceAsync<TDefaultTransitionEffect>(ServiceProvider, "default transition effect");
        }
    }
}
