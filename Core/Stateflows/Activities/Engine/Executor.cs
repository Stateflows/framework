using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Context;
using Stateflows.Utils;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Exceptions;
using Stateflows.Activities.Enums;
using Stateflows.Activities.Models;
using Stateflows.Activities.Streams;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Classes;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Common.Engine;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Models;
using Edge = Stateflows.Activities.Models.Edge;
using Graph = Stateflows.Activities.Models.Graph;
using Stream = Stateflows.Activities.Streams.Stream;

namespace Stateflows.Activities.Engine
{    
    internal partial class Executor : IAsyncDisposable, IStateflowsExecutor
    {
        private static readonly object lockHandle = new object();
        private static readonly object nodesLockHandle = new object();
        private readonly Dictionary<Node, object> nodeLockHandles = new Dictionary<Node, object>();
        private readonly Dictionary<(Node Node, Guid ThreadId), SemaphoreSlim> nodeExecutionGates = [];
        private readonly AsyncLocal<NodeScope> invocationScope = new();
        private readonly AsyncLocal<ActivityContinuationFrame> currentFrame = new();
        private readonly AsyncLocal<ResumePlan> resumePlan = new();
        private readonly AsyncLocal<HandoffCoordinator> handoffCoordinator = new();
        private readonly AsyncLocal<bool> resumingCrossing = new();
        private readonly object crossingScopesLock = new();
        private readonly Dictionary<(Node Node, NodeScope Parent), NodeScope> crossingScopes = [];
        private readonly Dictionary<Node, Edge[]> ownedCrossingEdges;

        private static NodeScope FindOwningScope(NodeScope scope, Node node)
        {
            for (; scope != null; scope = scope.BaseNodeScope)
                if (scope.Node == node)
                    return scope;
            throw new InvalidOperationException($"Execution scope for '{node.Name}' is unavailable.");
        }

        private static bool HasSingleAnchoredInvocation(Node node)
        {
            for (; node.Parent != null; node = node.Parent)
                if (node.Type != NodeType.StructuredActivity || node.Options.HasFlag(NodeOptions.Reentrant))
                    return false;
            return true;
        }

        private NodeScope ResolveCrossingScope(Edge edge, NodeScope sourceScope, out Node resumeNode)
        {
            resumeNode = null;
            var scope = FindOwningScope(sourceScope, edge.Owner);
            if (scope.CrossingClosed || scope.CancellationToken.IsCancellationRequested)
                return null;
            var path = new Stack<Node>();
            for (var node = edge.Target.Parent; node != edge.Owner; node = node.Parent)
                path.Push(node);
            while (path.TryPop(out var node))
            {
                if (!crossingScopes.TryGetValue((node, scope), out var activation))
                {
                    // Anchored active structures survive transactional executor hydration. Resume their existing
                    // execution path, just as an accepted event does; a pending offer never starts a new structure.
                    if (node.Anchored && HasSingleAnchoredInvocation(node) && Context.NodeThreads.ContainsKey(node.Identifier))
                    {
                        resumeNode = node;
                        return scope;
                    }
                    return null;
                }
                if (activation.CrossingClosed || activation.CancellationToken.IsCancellationRequested)
                    return null;
                if (!activation.AcceptingCrossings)
                {
                    resumeNode = node;
                    return scope;
                }
                scope = activation;
            }
            return scope.CancellationToken.IsCancellationRequested ? null : scope;
        }

        private Task HandleCrossingSuccessorAsync(Edge edge, NodeScope sourceScope)
        {
            NodeScope scope;
            Node resumeNode;
            TaskCompletionSource completion;
            lock (crossingScopesLock)
            {
                scope = ResolveCrossingScope(edge, sourceScope, out resumeNode);
                if (scope == null)
                    return Task.CompletedTask;
                completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
                scope.CrossingWork.RemoveAll(task => task.IsCompletedSuccessfully);
                scope.CrossingWork.Add(completion.Task);
            }

            // The destination drains this work; an escaped continuation must not hold its source open.
            _ = ExecuteCrossingSuccessorAsync(edge, scope, resumeNode, completion);
            return Task.CompletedTask;
        }

        private async Task ExecuteCrossingSuccessorAsync(Edge edge, NodeScope scope, Node resumeNode, TaskCompletionSource completion)
        {
            var previousFrame = currentFrame.Value;
            var previousScope = invocationScope.Value;
            currentFrame.Value = scope.ContinuationFrame;
            invocationScope.Value = scope;
            try
            {
                if (resumeNode == null)
                    await ExecuteSuccessorsAsync(new[] { edge }.GroupBy(candidate => candidate.Target).ToArray(), scope);
                else
                {
                    var previousResume = resumingCrossing.Value;
                    resumingCrossing.Value = true;
                    try
                    {
                        for (var node = edge.Target.Parent; node != scope.Node; node = node.Parent)
                            Context.NodesToExecute.Add(node);
                        await DoHandleNodeAsync(resumeNode, null, scope);
                    }
                    finally
                    {
                        resumingCrossing.Value = previousResume;
                    }
                }
                completion.TrySetResult();
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }
            finally
            {
                currentFrame.Value = previousFrame;
                invocationScope.Value = previousScope;
            }
        }

        /// <summary>
        /// Coordinates the mixed handoff of a regular transaction whose firing inside a structural continuation
        /// (structured, iterative or parallel scope) routes into streamed (mixed) work. The transactional part of the
        /// call unwinds like a checkpoint pause, recording the suspended frames, and the continuation is resumed by
        /// the runtime host as streamed work only after the transaction has unwound.
        /// </summary>
        private sealed class HandoffCoordinator
        {
            public bool Requested { get; private set; }
            public TaskCompletionSource Unwound { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

            public bool TryRequest(Func<Func<Task>, bool> host, Func<Task> continuation)
            {
                lock (this)
                {
                    if (Requested)
                        return true;
                    if (host == null || !host(continuation))
                        return false;
                    Requested = true;
                    return true;
                }
            }
        }
        private readonly int maxConcurrentNodeInvocations;
        private readonly IStateflowsEventFilter eventFilter;
        private readonly IStateflowsValueStorage valueStorage;
        private readonly ILogger<Executor> logger;
        
        public bool StateHasChanged { get; set; }

        /// <summary>
        /// Set when the executor belongs to a long-lived runtime-owned Activity session. Session executors keep all
        /// network mutation on the caller's (runtime) scheduler instead of offloading parallel partitions to the thread pool.
        /// </summary>
        internal bool SessionOwned { get; set; }

        /// <summary>Runtime host for streamed work handed off by a regular transaction (mixed acceptance); returns false when unavailable.</summary>
        internal Func<Func<Task>, bool> StreamedWorkHost { get; set; }

        private sealed class ResumePlan(IEnumerable<ReadyActivityFiring> firings, IEnumerable<ActivityContinuationFrame> frames)
        {
            private readonly List<ReadyActivityFiring> firings = firings.ToList();
            private readonly List<ActivityContinuationFrame> frames = frames.ToList();

            private static bool Matches(Guid? ownerFrameId, Guid threadId, Guid? frameId, Guid ownerThreadId)
                => ownerFrameId == frameId && (frameId == null || ownerThreadId == threadId);

            public (ReadyActivityFiring[] Firings, ActivityContinuationFrame[] Frames) Take(Guid? frameId, Guid threadId)
            {
                lock (this)
                {
                    var ready = firings.Where(f => Matches(f.FrameId, threadId, frameId, f.ThreadId)).ToArray();
                    var children = frames.Where(f => Matches(f.ParentFrameId, threadId, frameId, f.OuterThreadId)).ToArray();
                    firings.RemoveAll(ready.Contains);
                    frames.RemoveAll(children.Contains);
                    return (ready, children);
                }
            }

            public void RestoreLeftovers(RootContext context)
            {
                lock (this)
                {
                    context.RestoreCheckpointContinuation(firings, frames);
                    firings.Clear();
                    frames.Clear();
                }
            }
        }
        
        private object GetLock(Node node)
        {
            object result = null;
            lock (nodesLockHandle)
            {
                if (!nodeLockHandles.TryGetValue(node, out result))
                {
                    result = new object();
                    nodeLockHandles[node] = result;
                }
            }

            return result;
        }

        public readonly Graph Graph;
        public readonly ActivitiesRegister Register;
        private readonly NodeScope rootNodeScope;
        public NodeScope NodeScope => invocationScope.Value ?? rootNodeScope;
        private readonly PendingTasks pendingTasks = PendingTasksScope.RequireCurrent();
        
        public void AddPendingTask(Task task)
        {
            (PendingTasksScope.Current ?? pendingTasks).Add(task);
        }
        
        private static readonly NodeType[] CancellableTypes =
        [
            NodeType.Action,
            NodeType.Decision,
            NodeType.Fork,
            NodeType.Join,
            NodeType.Merge,
            NodeType.SendEventAction,
            NodeType.PublishEventAction,
            NodeType.AcceptEventAction,
            NodeType.TimeEventAction,
            NodeType.StructuredActivity,
            NodeType.ParallelActivity,
            NodeType.IterativeActivity,
            NodeType.ExceptionHandler,
            NodeType.DataStore
        ];

        public readonly NodeType[] StructuralTypes =
        [
            NodeType.StructuredActivity,
            NodeType.ParallelActivity,
            NodeType.IterativeActivity
        ];

        private static readonly NodeType[] SystemTypes =
        [
            NodeType.Initial,
            NodeType.InputPin,
            NodeType.OutputPin
        ];

        private static readonly NodeType[] InteractiveNodeTypes =
        [
            NodeType.AcceptEventAction,
            NodeType.TimeEventAction,
        ];

        public Executor(ActivitiesRegister register, Graph graph, IServiceProvider serviceProvider, int maxConcurrentNodeInvocations = 16)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(maxConcurrentNodeInvocations, 1);
            this.maxConcurrentNodeInvocations = maxConcurrentNodeInvocations;
            Register = register;
            rootNodeScope = new NodeScope(serviceProvider, graph, null, Guid.NewGuid());
            Graph = graph;
            ownedCrossingEdges = graph.AllEdges.Values.Where(edge => edge.IsCrossScope)
                .GroupBy(edge => edge.Owner).ToDictionary(group => group.Key, group => group.ToArray());

            logger = serviceProvider.GetRequiredService<ILogger<Executor>>();
            Inspector = new Inspector(this, logger);
            inspection = new ActivityInspection(this, Inspector);

            eventFilter = serviceProvider.GetRequiredService<IStateflowsEventFilter>();
            valueStorage = serviceProvider.GetRequiredService<IStateflowsValueStorage>();
        }

        public RootContext Context { get; private set; }

        public readonly Inspector Inspector;

        private readonly IActivityInspection inspection;

        internal IDisposable BindInspection() => new InspectionScope(inspection);

        private sealed class InspectionScope : IDisposable
        {
            private readonly IActivityInspection previous = ActivitiesContextHolder.Inspection.Value;

            public InspectionScope(IActivityInspection inspection)
                => ActivitiesContextHolder.Inspection.Value = inspection;

            public void Dispose()
                => ActivitiesContextHolder.Inspection.Value = previous;
        }
        
        private EventWaitHandle FinalizationEvent { get; } = new(false, EventResetMode.AutoReset);

        private bool Finalized
        {
            get
            {
                Debug.Assert(Context != null, $"Context is unavailable. Is activity '{Graph.Name}' hydrated?");

                return Context.Finalized;
            }
        }

        public IEnumerable<Node> GetActiveNodes()
            => Context.ActiveNodes.Keys
                .Select(nodeId => Graph.AllNodes[nodeId])
                .OrderByDescending(node => node.Level)
                .ToArray();
        
        public Tree<Node> NodesTree { get; private set; } = null;

        private void RebuildNodesTree()
        {
            NodesTree = new Tree<Node>();
            
            if (Context.ActiveNodes.Any())
            {
                NodesTree.Root = new TreeNode<Node>(Graph);
            }

            foreach (var node in Context.ActiveNodes.Keys.Select(nodeId => Graph.AllNodes[nodeId]))
            {
                AddActiveBranch(node);
            }
        }

        private void AddActiveBranch(Node node)
        {
            if (node.Parent != null && !NodesTree.Contains(node.Parent))
            {
                AddActiveBranch(node.Parent);
            }
            
            NodesTree.AddTo(node, node.Parent);
        }

        public IReadOnlyTree<string> GetNodesTree()
            => NodesTree.Translate(node => node.OwnName);

        public IEnumerable<Type> GetExpectedEvents()
            => BehaviorStatus switch
                {
                    BehaviorStatus.Initialized => GetActiveNodes()
                        .SelectMany(node => node.ActualEventTypes)
                        .Distinct()
                        .ToArray(),
                    
                    BehaviorStatus.NotInitialized => Graph.InitializerTypes.Any()
                        ? Graph.InitializerTypes.ToArray()
                        : [ typeof(Initialize) ],
                    
                    _ => []
                };

        public IEnumerable<string> GetExpectedEventNames()
            => GetExpectedEvents()
                .Where(type => !typeof(SystemEvent).IsAssignableFrom(type))
                .Where(type => !typeof(Exception).IsAssignableFrom(type))
                .Where(type => !typeof(StructuralEvent).IsAssignableFrom(type))
                .Select(type => type.GetEventName())
                .Concat(BehaviorStatus == BehaviorStatus.Initialized
                    ? GetActiveNodes().Where(node => node.StructuralEventName != null).Select(node => node.StructuralEventName)
                    : [])
                .Distinct()
                .ToArray();

        public async Task HydrateAsync(RootContext context)
        {
            lock (crossingScopesLock)
                crossingScopes.Clear();
            Context = context;
            Context.Executor = this;
            rootNodeScope.ThreadId = context.RootThreadId;

            RebuildNodesTree();

            await Inspector.BuildAsync();
        }

        internal Task RequestCheckpointPauseAsync()
        {
            if (Context != null)
                Context.PauseAfterFiring = true;
            return Task.CompletedTask;
        }

        internal async Task ResumeReadyFiringsAsync(bool clearPause = true)
        {
            if (Context == null)
                return;

            var (ready, frames) = Context.TakeCheckpointContinuation();
            if (ready.Length == 0 && frames.Length == 0)
                return;

            if (clearPause)
                Context.PauseAfterFiring = false;
            using var eventExecution = Context.BeginEventExecution();
            var plan = new ResumePlan(ready, frames);
            resumePlan.Value = plan;
            currentFrame.Value = null;
            try
            {
                await ExecuteGraphAsync(resume: true);
            }
            finally
            {
                plan.RestoreLeftovers(Context);
                resumePlan.Value = null;
            }
        }

        public async Task<RootContext> DehydrateAsync()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is activity '{Graph.Name}' already dehydrated?");

            await rootNodeScope.DisposeAsync();

            var result = Context;
            result.Executor = null;
            Context = null;

            return result;
        }

        public BehaviorStatus BehaviorStatus =>
            (Initialized, Finalized) switch
            {
                (false, false) => BehaviorStatus.NotInitialized,
                (true, false) => BehaviorStatus.Initialized,
                (true, true) => BehaviorStatus.Finalized,
                _ => BehaviorStatus.NotInitialized
            };

        public bool Initialized
        {
            get
            {
                Debug.Assert(Context != null, $"Context is unavailable. Is activity '{Graph.Name}' hydrated?");

                return Context.Initialized;
            }
        }

        public async Task<EventStatus> InitializeAsync<TEvent>(
            EventHolder<TEvent> eventHolder,
            IEnumerable<TokenHolder> input = null,
            Func<Task> initialized = null)
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is activity '{Graph.Name}' hydrated?");

            if (!Initialized)
            {
                var context = new ActivityInitializationContext<TEvent>(Context, NodeScope, eventHolder, null);
                var result = await DoInitializeActivityAsync(context);

                if (result is InitializationStatus.InitializedImplicitly or InitializationStatus.InitializedExplicitly)
                {
                    Context.Initialized = true;
                    Context.Context.Status = BehaviorStatus.Initialized;
                    if (initialized != null)
                    {
                        await initialized();
                    }

                    input = input?.Concat(context.InputTokens).ToArray() ?? context.InputTokens.ToArray();

                    if (result == InitializationStatus.InitializedImplicitly)
                    {
                        Context.SetEvent(new Initialize().ToEventHolder());
                    }

                    var (_, outputTokens) = await ExecuteGraphAsync(input);

                    if (result == InitializationStatus.InitializedImplicitly)
                    {
                        Context.ClearEvent();
                    }

                    RebuildNodesTree();

                    return result == InitializationStatus.InitializedExplicitly
                        ? EventStatus.Initialized
                            : eventHolder.Payload is Initialize
                                ? EventStatus.Initialized
                                : EventStatus.Consumed;
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

        public async Task<bool> CancelAsync()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is activity '{Graph.Name}' hydrated?");

            if (Initialized)
            {
                lock (crossingScopesLock)
                    crossingScopes.Clear();
                Context.ActiveNodes.Clear();
                Context.FlowTokensCount.Clear();
                Context.NodeTimeEvents.Clear();
                Context.NodeThreads.Clear();
                Context.OutputTokens.Clear();
                Context.Streams.Clear();
                Context.Context.PendingTimeEvents.Clear();
                Context.Context.PendingStartupEvents.Clear();
                Context.Context.TriggerTime = null;

                await DoFinalizeAsync();

                RebuildNodesTree();

                return true;
            }

            return false;
        }

        public void Reset(ResetMode resetMode)
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is activity '{Graph.Name}' hydrated?");

            if (Initialized)
            {
                lock (crossingScopesLock)
                    crossingScopes.Clear();
                Context.Context.Values.Clear();
                lock (Context.Context.GlobalValues)
                    Context.Context.GlobalValues.Clear();
                Context.Context.Status = BehaviorStatus.Unknown;
                Context.Context.LastExecutedAt = DateTime.MinValue;
                Context.Context.TriggerTime = null;
                Context.Context.TriggerOnStartup = false;
                Context.Context.PendingTimeEvents = [];
                Context.Context.PendingStartupEvents = [];

                if (resetMode != ResetMode.KeepVersionAndSubscriptions) // KeepSubscriptions || Full
                {
                    Context.Context.Version = 0;
                    Context.Context.Subscriptions = [];
                    Context.Context.Subscribers = [];

                    if (resetMode != ResetMode.KeepSubscriptions) // Full
                    {
                        Context.Context.Deleted = true;
                    }
                }
                
                StateHasChanged = true;
            }
        }

        private IEnumerable<object> OutputTokens { get; set; } = null;

        public async Task<IEnumerable<object>> GetResultAsync()
        {
            await FinalizationEvent.WaitOneAsync();

            return OutputTokens;
        }

        public async Task<EventStatus> ProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            StateHasChanged = false;
            
            var result = EventStatus.Rejected;

            if (Initialized)
            {
                result = await DoProcessAsync(eventHolder);
            }

            // disposal

            return result;
        }

        public async Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            Debug.Assert(Context != null, $"Context is not available. Is activity '{Graph.Name}' hydrated?");

            if (eventHolder.Payload is TokensInputEvent inputBatch && !Graph.AcceptsInput(inputBatch.Tokens))
                return EventStatus.Rejected;

            Node activeNode = null;

            if (eventHolder.Payload is TokensInputEvent)
            {
                foreach (var inputPin in Graph.InputPins)
                    Context.NodesToExecute.Add(inputPin);
                activeNode = Graph.InputPins.FirstOrDefault();
            }

            if (activeNode == null)
            {
                var activeNodes = GetActiveNodes();
                activeNode = activeNodes.FirstOrDefault(node =>
                    eventHolder.IsAcceptedBy(node, Context.NodeTimeEvents)
                );
            }

            if (activeNode == null)
            {
                return EventStatus.NotConsumed;
            }

            while (activeNode != null)
            {
                Context.NodesToExecute.Add(activeNode);

                activeNode = activeNode.Parent;
            }

            var input = eventHolder.Payload is TokensInputEvent tokensTransferEvent
                ? tokensTransferEvent.Tokens
                : null;

            var (result, output) = await ExecuteGraphAsync(input);
            HandleGuardRequest(output, eventHolder);
            
            if (eventHolder.Payload is IRequest<TokensOutput> inputTokens)
            {
                var tokensOutput = new TokensOutput() { Tokens = output.ToList() };
                inputTokens.Respond(tokensOutput);
            }

            return result;
        }

        private async Task<(EventStatus, IEnumerable<TokenHolder>)> ExecuteGraphAsync(IEnumerable<TokenHolder> input = null, bool resume = false)
        {
            HandoffCoordinator coordinator = null;
            if (!resume && StreamedWorkHost != null && Context.IsCurrentCallStreamed == false)
            {
                coordinator = new HandoffCoordinator();
                handoffCoordinator.Value = coordinator;
            }

            (IEnumerable<TokenHolder> Output, bool Finalized) structure;
            try
            {
                structure = await DoExecuteStructuredNodeAsync(Graph, NodeScope, input, resume);
            }
            finally
            {
                if (coordinator != null)
                {
                    handoffCoordinator.Value = null;
                    coordinator.Unwound.TrySetResult();
                }
            }

            var (output, finalized) = structure;

            if (finalized)
            {
                await DoFinalizeAsync(output);
            }

            output = output.Where(h => h.PayloadType != typeof(ControlToken)).ToList();
            
            return (EventStatus.Consumed, output);
        }
        
        private void HandleGuardRequest<TEvent>(IEnumerable<TokenHolder> outputTokens, EventHolder<TEvent> eventHolder)
        {
            var guardRequest = eventHolder.Headers.Values.OfType<TransitionGuardRequest>().FirstOrDefault();
            if (guardRequest != null)
            {
                var output = outputTokens.OfType<TokenHolder<bool>>().FirstOrDefault()?.Payload ?? false;

                if (output)
                {
                    var headers = eventHolder.Headers
                        .Where(h => h.Value is not TransitionGuardRequest)
                        .ToDictionary();
                    
                    headers[nameof(TransitionGuardResponse)] = new TransitionGuardResponse
                    {
                        GuardIdentifier = guardRequest.GuardIdentifier
                    };

                    _ = Context.SendAsync(eventHolder.Payload, headers);
                }
                        
                var behaviorId = Context.Context.ContextOwnerId.Value;
                if (behaviorId.Type == BehaviorType.StateMachine)
                {
                    if (output)
                    {
                        switch (guardRequest.EdgeType)
                        {
                            case EdgeType.Transition:
                                Trace.WriteLine(
                                    $"⦗→s⦘ State Machine '{behaviorId.Name}:{behaviorId.Instance}': delegated guard passed transition from '{guardRequest.SourceName}' to '{guardRequest.TargetName}' triggered by event '{eventHolder.Name}', retransmitting event");
                                break;

                            case EdgeType.DefaultTransition:
                                Trace.WriteLine(
                                    $"⦗→s⦘ State Machine '{behaviorId.Name}:{behaviorId.Instance}': delegated guard passed default transition from '{guardRequest.SourceName}' to '{guardRequest.TargetName}', retransmitting event");
                                break;

                            case EdgeType.InternalTransition:
                                Trace.WriteLine(
                                    $"⦗→s⦘ State Machine '{behaviorId.Name}:{behaviorId.Instance}': delegated guard passed internal transition in '{guardRequest.SourceName}' triggered by event '{eventHolder.Name}', retransmitting event");
                                break;
                        }
                    }
                    else
                    {
                        switch (guardRequest.EdgeType)
                        {
                            case EdgeType.Transition:
                                Trace.WriteLine(
                                    $"⦗→s⦘ State Machine '{behaviorId.Name}:{behaviorId.Instance}': delegated guard stopped event '{eventHolder.Name}' from triggering transition from '{guardRequest.SourceName}' to '{guardRequest.TargetName}'");
                                break;

                            case EdgeType.DefaultTransition:
                                Trace.WriteLine(
                                    $"⦗→s⦘ State Machine '{behaviorId.Name}:{behaviorId.Instance}': delegated guard stopped default transition from '{guardRequest.SourceName}' to '{guardRequest.TargetName}'");
                                break;

                            case EdgeType.InternalTransition:
                                Trace.WriteLine(
                                    $"⦗→s⦘ State Machine '{behaviorId.Name}:{behaviorId.Instance}': delegated guard stopped event '{eventHolder.Name}' from triggering internal transition in '{guardRequest.SourceName}'");
                                break;
                        }
                    }
                }
            }
        }


        public async Task DoFinalizeAsync(IEnumerable<object> outputTokens = null)
        {
            if (Finalized) return;

            Context.Finalized = true;

            if (Graph.OutputPins.Any() && outputTokens != null)
            {
                OutputTokens = outputTokens;
            }

            var context = new ActivityActionContext(Context, NodeScope);

            try
            {
                await Inspector.OnActivityFinalizeAsync(context, () => Graph.Finalize.IterateOverAsync(context));
            }
            catch (Exception e)
            {
                Trace.WriteLine($"⦗→s⦘ Activity '{Context.Id.Name}:{Context.Id.Instance}': exception '{e.GetType().FullName}' thrown with message '{e.Message}'");
                await Inspector.OnActivityFinalizationExceptionAsync(context, e);
                throw;
            }


            RebuildNodesTree();

            FinalizationEvent.Set();
        }

        public async Task<(IEnumerable<TokenHolder> Output, bool Finalized)> DoExecuteStructuredNodeAsync(Node node, NodeScope nodeScope, IEnumerable<TokenHolder> input = null, bool resume = false)
        {
            if (node.Anchored && HasSingleAnchoredInvocation(node))
            {
                if (Context.NodeThreads.TryGetValue(node.Identifier, out var storedThreadId))
                {
                    nodeScope.ThreadId = storedThreadId;
                }
                else
                {
                    Context.NodeThreads.Add(node.Identifier, nodeScope.ThreadId);
                }
            }

            var finalized = await DoExecuteStructureAsync(
                node,
                nodeScope,
                input,
                resume: resume
            );

            var output = node.OutputPins
                .SelectMany(pin => Context.TakeOutputTokens(pin.Identifier, nodeScope.ThreadId)).ToList();

            if (node.ExceptionHandlers.Any())
            {
                output.AddRange(node.ExceptionHandlers.SelectMany(exceptionHandler => Context.GetOutputTokens(exceptionHandler.Identifier, nodeScope.ThreadId)).ToList());
            }

            var result = (
                output,
                finalized
            );

            if (finalized)
            {
                lock (crossingScopesLock)
                {
                    Context.NodeThreads.Remove(node.Identifier);
                    if (ownedCrossingEdges.Count > 0 && node.Parent != null)
                    {
                        var key = (node, FindOwningScope(nodeScope.BaseNodeScope, node.Parent));
                        if (crossingScopes.TryGetValue(key, out var active) && active == nodeScope)
                            crossingScopes.Remove(key);
                    }
                }
            }

            return result;
        }

        private ActivityContinuationFrame CreateFrame(ActionContext context, ActivityContinuationKind kind)
            => new()
            {
                ParentFrameId = currentFrame.Value?.Id,
                NodeIdentifier = context.Node.Identifier,
                UpstreamEdgeIdentifier = context.Edge?.Identifier,
                OuterThreadId = context.NodeScope.ThreadId,
                IsRootThread = currentFrame.Value == null,
                Kind = kind,
                IsStreamed = context.IsStreamed,
                InputTokens = context.InputTokens.ToList()
            };

        private ActivityContinuationFrame CreatePartitionedFrame<TToken>(ActionContext context, ActivityContinuationKind kind, bool sharedThread)
        {
            var frame = CreateFrame(context, kind);
            var typedInput = context.InputTokens.OfTokenType<TToken>().ToArray();
            frame.RestOfInput = context.InputTokens.Except(typedInput).ToList();
            var threadId = Guid.NewGuid();
            frame.Partitions = typedInput.Partition(context.Node.ChunkSize)
                .Select(partition => new ActivityContinuationPartition
                {
                    ThreadId = sharedThread ? threadId : Guid.NewGuid(),
                    Selection = partition.ToList()
                })
                .ToList();
            return frame;
        }

        private static NodeScope CreatePartitionScope(ActionContext context, ActivityContinuationPartition partition)
        {
            var scope = context.NodeScope.CreateChildScope(context.Node, context.Edge, partition.ThreadId);
            scope.SelectionTokens = partition.Selection;
            return scope;
        }
        private List<TokenHolder> CollectPartitionOutput(Node node, Guid threadId)
        {
            lock (Context.Streams)
            {
                var output = node.OutputPins
                    .SelectMany(pin => Context.TakeOutputTokens(pin.Identifier, threadId)).ToList();

                Context.Streams.Remove(threadId);
                Context.OutputTokens.Remove(threadId);
                return output;
            }
        }

        /// <summary>
        /// Executes a structured activity node invocation, or continues one suspended by a checkpoint barrier.
        /// </summary>
        internal async Task ExecuteStructuredActivityAsync(ActionContext context)
        {
            if (context.Node.Options.HasFlag(NodeOptions.Interleave))
            {
                await ExecuteInterleavedStructuredAsync(context);
                return;
            }
            var node = context.Node;
            var resuming = context.ResumeFrame != null;
            var frame = context.ResumeFrame ?? CreateFrame(context, ActivityContinuationKind.Structured);

            if (!resuming && !Context.NodesToExecute.Contains(node))
            {
                await DoInitializeNodeAsync(node, context);
            }

            currentFrame.Value = frame;
            (IEnumerable<TokenHolder> Output, bool Finalized) result;
            try
            {
                result = await DoExecuteStructuredNodeAsync(node, context.NodeScope, resuming ? null : context.InputTokens, resuming);
            }
            catch (ActivityCheckpointPauseException)
            {
                Context.AddContinuationFrame(frame);
                throw;
            }

            context.OutputTokens.AddRange(result.Output);

            if (result.Finalized)
            {
                await DoFinalizeNodeAsync(node, context);
            }
        }

        /// <summary>
        /// Executes an iterative activity node invocation partition by partition, or continues one suspended by a
        /// checkpoint barrier with its remaining partitions and already accumulated output.
        /// </summary>
        internal async Task ExecuteIterativeActivityAsync<TToken>(ActionContext context)
        {
            if (context.Node.Options.HasFlag(NodeOptions.Interleave))
            {
                await ExecuteInterleavedPartitionsAsync<TToken>(context, parallel: false);
                return;
            }
            var node = context.Node;
            var frame = context.ResumeFrame;
            if (frame == null)
            {
                frame = CreatePartitionedFrame<TToken>(context, ActivityContinuationKind.Iterative, sharedThread: true);
                await DoInitializeNodeAsync(node, context);
            }

            currentFrame.Value = frame;
            try
            {
                foreach (var partition in frame.Partitions)
                {
                    if (frame.Completed)
                    {
                        break;
                    }

                    if (partition.Completed)
                    {
                        continue;
                    }

                    var resuming = partition.Started;
                    partition.Started = true;

                    var result = await DoExecuteStructureAsync(
                        node,
                        CreatePartitionScope(context, partition),
                        resuming ? null : frame.RestOfInput.Concat(partition.Selection).ToArray(),
                        resuming ? null : partition.Selection,
                        resuming
                    );

                    frame.AccumulatedOutput.AddRange(CollectPartitionOutput(node, partition.ThreadId));
                    partition.Completed = true;

                    if (result && node.FinalNode != null)
                    {
                        frame.Completed = true;
                    }
                }
            }
            catch (ActivityCheckpointPauseException)
            {
                Context.AddContinuationFrame(frame);
                throw;
            }

            if (node.OutputPins.Any())
            {
                context.OutputTokens.AddRange(frame.AccumulatedOutput);
            }

            await DoFinalizeNodeAsync(node, context);
        }

        /// <summary>
        /// Executes a parallel activity node invocation, or continues one suspended by a checkpoint barrier. Completed
        /// partitions keep their accumulated output and are not executed again.
        /// </summary>
        internal async Task ExecuteParallelActivityAsync<TToken>(ActionContext context)
        {
            if (context.Node.Options.HasFlag(NodeOptions.Interleave))
            {
                await ExecuteInterleavedPartitionsAsync<TToken>(context, parallel: true);
                return;
            }
            var node = context.Node;
            var frame = context.ResumeFrame;
            if (frame == null)
            {
                frame = CreatePartitionedFrame<TToken>(context, ActivityContinuationKind.Parallel, sharedThread: false);
                await DoInitializeNodeAsync(node, context);
            }

            currentFrame.Value = frame;

            using var partitionSlots = node.MaxConcurrency > 0 ? new SemaphoreSlim(node.MaxConcurrency) : null;
            async Task RunPartitionAsync(ActivityContinuationPartition partition)
            {
                if (partitionSlots != null)
                {
                    try { await partitionSlots.WaitAsync(context.NodeScope.CancellationToken); }
                    catch (OperationCanceledException) when (context.NodeScope.CancellationToken.IsCancellationRequested) { return; }
                }
                try
                {
                    if (context.NodeScope.CancellationToken.IsCancellationRequested)
                        return;
                    if (Context.PauseAfterFiring)
                        throw new ActivityCheckpointPauseException();
                    if (SessionOwned)
                        await ExecutePartitionAsync(partition);
                    else
                        await Task.Run(() => ExecutePartitionAsync(partition));
                }
                finally { partitionSlots?.Release(); }
            }

            async Task ExecutePartitionAsync(ActivityContinuationPartition partition)
            {
                var resuming = partition.Started;
                partition.Started = true;

                await DoExecuteStructureAsync(
                    node,
                    CreatePartitionScope(context, partition),
                    resuming ? null : frame.RestOfInput.Concat(partition.Selection).ToArray(),
                    resuming ? null : partition.Selection,
                    resuming
                );

                var output = CollectPartitionOutput(node, partition.ThreadId);
                lock (frame)
                {
                    frame.AccumulatedOutput.AddRange(output);
                    partition.Completed = true;
                }
            }

            try
            {
                await Task.WhenAll(frame.Partitions
                    .Where(partition => !partition.Completed)
                    .Select(RunPartitionAsync)
                    .ToArray());
            }
            catch (ActivityCheckpointPauseException)
            {
                Context.AddContinuationFrame(frame);
                throw;
            }

            if (node.OutputPins.Any())
            {
                context.OutputTokens.AddRange(frame.AccumulatedOutput);
            }

            await DoFinalizeNodeAsync(node, context);
        }

        public async Task DoInitializeNodeAsync(Node node, ActionContext context)
        {
            await Inspector.OnNodeInitializeAsync(context, () => node.Initialize.IterateOverAsync(context));
        }

        public async Task DoFinalizeNodeAsync(Node node, ActionContext context)
        {
            await Inspector.OnNodeFinalizeAsync(context, () => node.Finalize.IterateOverAsync(context));
        }

        private async Task<bool> DoExecuteStructureAsync(Node node, NodeScope nodeScope, IEnumerable<TokenHolder> input = null, IEnumerable<TokenHolder> selectionTokens = null, bool resume = false)
        {
            if (input != null && !node.AcceptsInput(input.Where(token => token.PayloadType != typeof(ControlToken))))
                throw new InvalidOperationException($"Activity scope '{node.Name}' received tokens unsupported by its input pins.");

            if (ownedCrossingEdges.Count > 0 || node.Options.HasFlag(NodeOptions.Interleave))
            {
                var parentScope = node.Parent == null
                    ? null
                    : FindOwningScope(nodeScope.BaseNodeScope, node.Parent);
                nodeScope.ContinuationFrame = currentFrame.Value;
                lock (crossingScopesLock)
                {
                    nodeScope.CrossingClosed = false;
                    nodeScope.AcceptingCrossings = true;
                    if (node.Type == NodeType.StructuredActivity && !node.Options.HasFlag(NodeOptions.Reentrant))
                        crossingScopes[(node, parentScope)] = nodeScope;
                }
            }

            if (resume && resumePlan.Value is { } plan)
            {
                var (firings, frames) = plan.Take(currentFrame.Value?.Id, nodeScope.ThreadId);
                if (Context.PauseAfterFiring)
                {
                    // A new barrier arrived before the recovered work started; keep it ready for the next recovery.
                    Context.RestoreCheckpointContinuation(firings, frames);
                    throw new ActivityCheckpointPauseException();
                }

                var tasks = firings
                    .GroupBy(firing => firing.NodeIdentifier)
                    .Select(group =>
                    {
                        var target = GetCheckpointNode(group.Key);
                        var edge = group
                            .Select(firing => target.IncomingEdges.FirstOrDefault(e => e.Identifier == firing.UpstreamEdgeIdentifier))
                            .FirstOrDefault(e => e != null);
                        return DoHandleNodeAsync(target, edge, nodeScope);
                    })
                    .Concat(frames.Select(frame =>
                    {
                        var target = GetCheckpointNode(frame.NodeIdentifier);
                        var edge = target.IncomingEdges.FirstOrDefault(e => e.Identifier == frame.UpstreamEdgeIdentifier);
                        return DoHandleNodeAsync(target, edge, nodeScope, resumeFrame: frame);
                    }))
                    .Concat(HandlePendingCrossings(node, nodeScope,
                        firings.Select(firing => firing.NodeIdentifier).Concat(frames.Select(frame => frame.NodeIdentifier)).ToHashSet()))
                    .ToArray();

                var resumedResult = await CompleteStructureWorkAsync(nodeScope, Task.WhenAll(tasks));
                RebuildNodesTree();
                return resumedResult;
            }

            var nodes = node.Nodes.Values.Where(n => Context.NodesToExecute.Contains(n)).ToList();
            if (nodes.Count == 0 && !resume && !(resumingCrossing.Value && Context.NodesToExecute.Contains(node)))
            {
                nodes.AddRange(node.InitialNodes);
                nodes.AddRange(node.InputPins);
            }

            var pendingCrossings = HandlePendingCrossings(node, nodeScope, nodes.Select(child => child.Identifier).ToHashSet());

            var localWork = Task.WhenAll(
                nodes.Where(n => n.Type != NodeType.InputPin ||
                    input?.Any(token => n.PinTokenType.IsAssignableFrom(token.ActualPayloadType)) == true)
                .Select(n => DoHandleNodeAsync(n, nodeScope.Edge, nodeScope,
                    n.Type == NodeType.InputPin
                        ? input?.Where(token => n.PinTokenType.IsAssignableFrom(token.ActualPayloadType)).ToArray()
                        : null, selectionTokens)).ToArray()
            );
            var result = await CompleteStructureWorkAsync(nodeScope, Task.WhenAll(pendingCrossings.Append(localWork)));

            RebuildNodesTree();

            return result;
        }

        private Task[] HandlePendingCrossings(Node node, NodeScope scope, HashSet<string> excluded)
            => ownedCrossingEdges.Count == 0 ? [] : node.Nodes.Values
                .Where(child => !excluded.Contains(child.Identifier))
                .SelectMany(child => child.IncomingEdges)
                .Where(edge => edge.IsCrossScope &&
                    Context.GetStream(edge.Identifier, RootContext.GetStreamThreadId(edge, scope)).IsActivated)
                .GroupBy(edge => edge.Target)
                .Select(group => HandleCrossingSuccessorAsync(group.First(), scope))
                .ToArray();

        private async Task<bool> CompleteStructureWorkAsync(NodeScope scope, Task localWork)
        {
            if (scope.Node.Options.HasFlag(NodeOptions.Interleave) &&
                scope.Node.Type == NodeType.StructuredActivity)
            {
                await localWork;
                return Context.IsNodeCompleted(scope.Node, scope);
            }
            if (ownedCrossingEdges.Count == 0)
            {
                await localWork;
                return Context.IsNodeCompleted(scope.Node, scope);
            }

            Exception failure = null;
            try
            {
                await localWork;
            }
            catch (Exception exception)
            {
                failure = exception;
                lock (crossingScopesLock)
                {
                    scope.CrossingClosed = true;
                    scope.AcceptingCrossings = false;
                }
            }

            var completed = false;
            try
            {
                completed = await CompleteCrossingScopeAsync(scope, failure == null);
            }
            catch (Exception exception)
            {
                if (failure == null || failure is ActivityCheckpointPauseException)
                    failure = exception;
            }
            if (failure != null)
                ExceptionDispatchInfo.Capture(failure).Throw();
            return completed;
        }

        private async Task<bool> CompleteCrossingScopeAsync(NodeScope scope, bool canComplete)
        {
            if (ownedCrossingEdges.Count == 0)
                return Context.IsNodeCompleted(scope.Node, scope);
            Exception failure = null;
            while (true)
            {
                Task[] work;
                lock (crossingScopesLock)
                {
                    work = scope.CrossingWork.Where(task => !task.IsCompletedSuccessfully).ToArray();
                    scope.CrossingWork.Clear();
                    if (work.Length == 0)
                    {
                        if (failure != null)
                            ExceptionDispatchInfo.Capture(failure).Throw();
                        if (!canComplete)
                            return false;
                        if (scope.ContinuationFrame is { } frame &&
                            Context.HasReadyFiringsInFrame(frame.Id, scope.ThreadId))
                        {
                            scope.CrossingClosed = true;
                            scope.AcceptingCrossings = false;
                            throw new ActivityCheckpointPauseException();
                        }
                        var result = Context.IsNodeCompleted(scope.Node, scope);
                        scope.CrossingClosed |= result;
                        scope.AcceptingCrossings = false;
                        if (result && scope.Node.Type == NodeType.StructuredActivity &&
                            ownedCrossingEdges.TryGetValue(scope.Node, out var edges))
                            foreach (var edge in edges)
                                Context.ClearStream(edge.Identifier, RootContext.GetStreamThreadId(edge, scope));
                        if (result && !scope.Node.Anchored && scope.Node.Parent != null)
                        {
                            var key = (scope.Node, FindOwningScope(scope.BaseNodeScope, scope.Node.Parent));
                            if (crossingScopes.TryGetValue(key, out var active) && active == scope)
                                crossingScopes.Remove(key);
                        }
                        return result;
                    }
                }
                try
                {
                    await Task.WhenAll(work);
                }
                catch (Exception exception)
                {
                    if (failure == null || failure is ActivityCheckpointPauseException)
                        failure = exception;
                    lock (crossingScopesLock)
                    {
                        scope.CrossingClosed = true;
                        scope.AcceptingCrossings = false;
                    }
                }
            }
        }

        private Node GetCheckpointNode(string identifier)
            => Graph.AllNodes.TryGetValue(identifier, out var node)
                ? node
                : throw new InvalidOperationException($"Checkpoint references unknown Activity node '{identifier}'.");

        public async Task<InitializationStatus> DoInitializeActivityAsync<TInitializationEvent>(ActivityInitializationContext<TInitializationEvent> context)
        {
            InitializationStatus result = InitializationStatus.NotInitialized;

            var initializer = Graph.Initializers.ContainsKey(context.InitializationEvent.GetType().GetEventName())
                ? Graph.Initializers[context.InitializationEvent.GetType().GetEventName()]
                : Graph.DefaultInitializer;

            var implicitInitialization = (initializer == null || initializer == Graph.DefaultInitializer) && !(context.InitializationEvent is Initialize);

            await Inspector.OnActivityInitializeAsync(context, implicitInitialization, async () =>
            {
                if (initializer != null)
                {
                    try
                    {
                        result = await initializer.IterateOverAsync(context)
                            ? initializer == Graph.DefaultInitializer
                                ? InitializationStatus.InitializedImplicitly
                                : InitializationStatus.InitializedExplicitly
                            : InitializationStatus.NotInitialized;
                    }
                    catch (Exception e)
                    {
                        if (e is StateflowsDefinitionException)
                        {
                            throw;
                        }
                        else
                        {
                            Trace.WriteLine($"⦗→s⦘ Activity '{Context.Id.Name}:{Context.Id.Instance}': exception '{e.GetType().FullName}' thrown with message '{e.Message}'");
                            await Inspector.OnActivityInitializationExceptionAsync(context, context.InitializationEventHolder, e);
                            throw;
                        }
                    }
                }
                else
                {
                    if (Graph.Initializers.Any())
                    {
                        result = InitializationStatus.NotInitialized;
                    }
                    else
                    {
                        result = InitializationStatus.InitializedImplicitly;
                    }
                }

                return result == InitializationStatus.InitializedImplicitly ||
                    result == InitializationStatus.InitializedExplicitly;
            });

            RebuildNodesTree();

            return result;
        }

        public async Task<ExceptionHandlingResult> HandleExceptionAsync(Node node, Exception exception, ActionContext context)
        {
            Context.RecordException(exception);
            Node handler = null;
            Node handlerOwner = null;
            var currentNode = node;
            while (currentNode != null)
            {
                handler = currentNode.FindExceptionHandler(exception);

                if (handler != null)
                {
                    handlerOwner = currentNode;
                    break;
                }

                currentNode = currentNode.Parent;
            }

            var currentScope = context.NodeScope;
            while (currentNode != null)
            {
                if (currentScope.Node == currentNode)
                {
                    break;
                }

                currentScope = currentScope.BaseNodeScope;
            }
            
            var exceptionName = exception.GetType().FullName;

            if (handler != null)
            {
                var exceptionContext = new ActionContext(
                    context.Context,
                    currentScope,
                    handler,
                    new TokenHolder[]
                    {
                        exception.ToExceptionHolder(exception.GetType()),
                        new NodeReferenceToken() { Node = node }.ToTokenHolder(),
                    }
                )
                {
                    IsStreamed = context is ActionContext actionContext && actionContext.IsStreamed
                };

                await handler.Action.IterateOverAsync(exceptionContext);

                if (node.ExceptionHandlers.Contains(handler))
                {
                    context.OutputTokens.AddRange(exceptionContext.OutputTokens);
                }
                else
                {
                    DoHandleOutput(exceptionContext);
                }

                ReportExceptionHandled(handlerOwner, exceptionName, exceptionContext.OutputTokens.Where(t => !(t is TokenHolder<ControlToken>)).ToArray(), Context);

                return node.ExceptionHandlers.Contains(handler)
                    ? ExceptionHandlingResult.HandledDirectly
                    : ExceptionHandlingResult.HandledIndirectly;
            }
            
            Trace.WriteLine($"⦗→s⦘ Activity '{node.Graph.Name}:{context.Activity.Id.Instance}': exception '{exceptionName}' is not handled within Activity");

            return ExceptionHandlingResult.NotHandled;
        }

        private sealed class NodeExecutionLease(SemaphoreSlim gate) : IDisposable
        {
            private SemaphoreSlim gate = gate;

            public void Dispose()
                => Interlocked.Exchange(ref gate, null)?.Release();
        }

        public async Task DoHandleNodeAsync(Node node, Edge upstreamEdge, NodeScope nodeScope, IEnumerable<TokenHolder> input = null, IEnumerable<TokenHolder> selectionTokens = null, ActivityContinuationFrame resumeFrame = null)
        {
            SemaphoreSlim gate;
            lock (nodeExecutionGates)
            {
                var key = (node, nodeScope.ThreadId);
                if (!nodeExecutionGates.TryGetValue(key, out gate))
                {
                    gate = new SemaphoreSlim(node.Options.HasFlag(NodeOptions.Reentrant) ? maxConcurrentNodeInvocations : 1);
                    nodeExecutionGates.Add(key, gate);
                }
            }

            var cancellationToken = CancellableTypes.Contains(node.Type)
                ? nodeScope.CancellationToken
                : CancellationToken.None;
            try
            {
                await gate.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            using var lease = new NodeExecutionLease(gate);
            await DoHandleNodeCoreAsync(node, upstreamEdge, nodeScope, lease, input, selectionTokens, resumeFrame);
        }

        private async Task DoHandleNodeCoreAsync(Node node, Edge upstreamEdge, NodeScope nodeScope, NodeExecutionLease lease, IEnumerable<TokenHolder> input, IEnumerable<TokenHolder> selectionTokens, ActivityContinuationFrame resumeFrame)
        {
            if (await TryForceFinalizeAsync(nodeScope))
            {
                Trace.WriteLine($"⦗→s⦘ Activity '{node.Graph.Name}:{Context.Id.Instance}': forcefully finalized before executing node '{node.Name}'");

                return;
            }

            if (CancellableTypes.Contains(node.Type) && nodeScope.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            bool activated;

            IEnumerable<TokenHolder> inputTokens;
            IEnumerable<Stream> streams;

            lock (GetLock(node))
            {
                streams = Context.GetNodeStreams(node, nodeScope, node.Type != NodeType.OutputPin).ToArray();

                activated =
                    ( // initial node case
                        node.Type == NodeType.Initial
                    ) ||
                    ( // input pin case - has to have input
                        node.Type == NodeType.InputPin &&
                        (input?.Any() ?? false)
                    ) ||
                    ( // no implicit join node - has any incoming streams
                        streams.Any() &&
                        !node.Options.HasFlag(NodeOptions.ImplicitJoin)
                    ) ||
                    ( // implicit join node - has incoming streams on all edges
                        node.IncomingEdges.Count == streams.Count() &&
                        node.Options.HasFlag(NodeOptions.ImplicitJoin)
                    ) ||
                    (
                        Context.NodesToExecute.Contains(node)
                    ) ||
                    ( // structural invocation recovered from a checkpoint
                        resumeFrame != null
                    );

                inputTokens = resumeFrame != null ? resumeFrame.InputTokens.ToArray() : input ?? streams
                    .SelectMany(stream => stream.Tokens)
                    .DistinctBy(token => (token.Id, token.IsStreamed))
                    .ToArray();

                if (streams.Any() && resumeFrame == null)
                {
                    if (activated)
                    {
                        foreach (var stream in streams)
                        {
                            if (!(stream.IsPersistent || InteractiveNodeTypes.Contains(node.Type)))
                            {
                                Context.ClearStream(stream.EdgeIdentifier, RootContext.GetStreamThreadId(Graph.AllEdges[stream.EdgeIdentifier], nodeScope));
                                Context.ActivatedFlows.Add(stream.EdgeIdentifier);
                            }
                        }
                    }
                    else
                    {
                        ReportNodeAttemptedExecution(node, upstreamEdge, streams, Context);
                    }
                }
            }

            NodeScope activeScope = null;
            if ((ownedCrossingEdges.Count > 0 || node.Options.HasFlag(NodeOptions.Interleave)) && node.Type == NodeType.StructuredActivity &&
                !node.Options.HasFlag(NodeOptions.Reentrant))
            {
                var parentScope = FindOwningScope(nodeScope, node.Parent);
                lock (crossingScopesLock)
                    if (crossingScopes.TryGetValue((node, parentScope), out var active) && !active.CrossingClosed)
                        activeScope = active;
            }
            InterleavingInvocation interleaving = null;
            if (activated && node.Options.HasFlag(NodeOptions.Interleave))
            {
                interleaving = await AdmitInterleavingAsync(node, FindOwningScope(nodeScope, node.Parent),
                    activeScope, upstreamEdge, inputTokens.ToArray(), lease);
                if (interleaving == null)
                    return;
                nodeScope = interleaving.Scope;
            }
            else
                nodeScope = activeScope ?? nodeScope.CreateChildScope(node, upstreamEdge,
                    StructuralTypes.Contains(node.Type) && node.Options.HasFlag(NodeOptions.Reentrant) ? Guid.NewGuid() : null);
            invocationScope.Value = nodeScope;

            var actionContext = new ActionContext(Context, nodeScope, node, inputTokens, selectionTokens)
            {
                ResumeFrame = resumeFrame,
                // An accept-event/time-event firing is a source owned by its triggering event: its enabling
                // tokens do not color it, the provenance of the event does.
                IsStreamed = (resumeFrame?.IsStreamed ?? false) ||
                    Context.IsStreamingExecution ||
                    // A data store immediately converts transactional arrivals and retained outputs to streamed work
                    // during a streaming busy period.
                    (node.Type == NodeType.DataStore && Context.IsStreamingBusyPeriod) ||
                    (!InteractiveNodeTypes.Contains(node.Type) &&
                        (inputTokens.Any(token => token.IsStreamed) ||
                        streams.Any(stream => stream.IsStreamed)))
            };
            using var invocationProvenance = Context.BeginInvocationProvenance(actionContext.IsStreamed);

            var actuallyActivated = resumeFrame == null && !Context.ActiveNodes.Keys.Contains(node.Identifier);
            try
            {
                if (actuallyActivated)
                    await Inspector.OnNodeActivateAsync(actionContext, activated, ActivateAsync);
                else
                    await ActivateAsync();
            }
            finally
            {
                if (interleaving != null)
                {
                    FinishInterleaving(node, nodeScope);
                    if (interleaving.Finalized)
                        await nodeScope.DisposeAsync();
                }
            }

            async Task ActivateAsync()
            {
                RegisterNodePending(node);

                if (activated)
                {
                    RegisterNodeActivation(node);

                    if (InteractiveNodeTypes.Contains(node.Type))
                    {
                        if (
                            (
                                Context.EventHolder == null ||
                                !Context.EventHolder.IsAcceptedBy(node)
                            ) &&
                            actuallyActivated
                            // !node.ActualEventTypes.Contains(Context.EventHolder.PayloadType)
                        )
                        {
                            Inspector.AcceptEventsPlugin.RegisterAcceptEventNode(node, nodeScope.ThreadId);

                            Trace.WriteLine($"⦗→s⦘ Activity '{node.Graph.Name}:{Context.Id.Instance}': {(node.Type == NodeType.AcceptEventAction ? "AcceptEvent" : "TimeEvent")} node '{node.Name}' registered and waiting for event");


                            return;
                        }
                        else
                        {
                            foreach (var edge in node.IncomingEdges)
                            {
                                if (edge.Source.Type != NodeType.DataStore)
                                {
                                    Context.ClearStream(edge.Identifier, RootContext.GetStreamThreadId(edge, nodeScope));
                                }
                            }
                        }
                    }

                    ReportNodeExecuting(node, upstreamEdge, inputTokens, Context);

                    try
                    {
                        await node.Action.IterateOverAsync(actionContext);
                    }
                    catch (ActivityCheckpointPauseException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        await Inspector.OnNodeExecutionExceptionAsync(actionContext, e);
                        throw;
                    }

                    if (InteractiveNodeTypes.Contains(node.Type))
                    {
                        var @event = Context.EventHolder;

                        if (@event.BoxedPayload is TimeEvent)
                        {
                            Inspector.AcceptEventsPlugin.UnregisterAcceptEventNode(node);

                            if (@event.BoxedPayload is RecurringEvent && !node.IncomingEdges.Any() && Context.NodesToExecute.Contains(node))
                            {
                                Inspector.AcceptEventsPlugin.RegisterAcceptEventNode(node, nodeScope.ThreadId);
                            }
                        }
                        else
                        {
                            if (node.IncomingEdges.Any())
                            {
                                Inspector.AcceptEventsPlugin.UnregisterAcceptEventNode(node);
                            }
                        }
                    }

                    if (await TryForceFinalizeAsync(nodeScope))
                    {
                        Trace.WriteLine($"⦗→s⦘ Activity '{node.Graph.Name}:{Context.Id.Instance}': forcefully finalized after executing node '{node.Name}'");

                        return;
                    }

                    if (nodeScope.CancellationToken.IsCancellationRequested && CancellableTypes.Contains(node.Type) &&
                        !StructuralTypes.Contains(node.Type))
                        return;

                    if (node.Type is NodeType.OutputPin or NodeType.ExceptionHandler)
                    {
                        DoHandleOutput(actionContext);
                    }
                    else
                    {
                        if (
                            StructuralTypes.Contains(node.Type) &&
                            (interleaving?.Finalized ?? Context.IsNodeCompleted(node, nodeScope)) &&
                            !actionContext.OutputTokens.Any()
                        )
                        {
                            var controlToken = new ControlToken().ToTokenHolder();
                            controlToken.IsStreamed = actionContext.IsStreamed;
                            actionContext.OutputTokens.Add(controlToken);
                        }

                        TokenHolder[] outputTokens;
                        lock (actionContext.OutputTokens)
                        {
                            outputTokens = actionContext.OutputTokens.ToArray();
                        }

                        ReportNodeExecuted(node, outputTokens.Where(t => t is TokenHolder<ControlToken>).ToArray(), Context);

                        var nodes = (
                            await Task.WhenAll(
                                node.Edges
                                    .Where(edge => edge.Target.Type == NodeType.OutputPin || outputTokens.Any(t => t is TokenHolder<ControlToken>))
                                    .Where(edge => outputTokens.Any(t => edge.TokenType.IsAssignableFrom(t.ActualPayloadType)) || edge.Weight == 0)
                                    .OrderBy(edge => edge.IsElse)
                                    .Select(async edge =>
                                        (
                                            Edge: edge,
                                            Activated: await DoHandleEdgeAsync(edge, actionContext)
                                        )
                                    )
                            )
                        )
                            .Where(x => x.Activated)
                            .Select(x => x.Edge)
                            .Reverse()
                            .GroupBy(edge => edge.Target)
                            .ToArray();

                        // Successors can loop back to this node; only the firing and its routing own the gate.
                        if (interleaving != null)
                            FinishInterleaving(node, nodeScope);
                        lease.Dispose();
                        var crossingNodes = nodes.Where(group => group.Any(edge => edge.IsCrossScope)).ToArray();
                        var crossingWork = crossingNodes
                            .Select(group => HandleCrossingSuccessorAsync(group.First(), nodeScope)).ToArray();
                        nodes = nodes.Except(crossingNodes).ToArray();
                        await Task.WhenAll(crossingWork.Append(ExecuteSuccessorsAsync(nodes, nodeScope)));
                    }
                }

            }
        }

        private async Task ExecuteSuccessorsAsync(IGrouping<Node, Edge>[] nodes, NodeScope nodeScope)
        {
            if (Context.PauseAfterFiring && nodes.Length > 0)
            {
                Context.AddReadyFirings(nodes.SelectMany(group => group.Select(edge =>
                    new ReadyActivityFiring
                    {
                        NodeIdentifier = group.Key.Identifier,
                        UpstreamEdgeIdentifier = edge.Identifier,
                        FrameId = currentFrame.Value?.Id,
                        ThreadId = nodeScope.ThreadId,
                        IsRootThread = currentFrame.Value == null
                    })));
                throw new ActivityCheckpointPauseException();
            }

            if (currentFrame.Value != null && nodes.Length > 0 &&
                handoffCoordinator.Value is { } coordinator &&
                Context.IsCurrentCallStreamed == false)
            {
                var mixedGroups = nodes.Where(group => IsMixedTarget(group.Key, nodeScope)).ToArray();
                if (mixedGroups.Length > 0 &&
                    coordinator.TryRequest(StreamedWorkHost, () => ContinueHandedOffFramesAsync(coordinator)))
                {
                    Context.AddReadyFirings(mixedGroups.SelectMany(group => group.Select(edge =>
                        new ReadyActivityFiring
                        {
                            NodeIdentifier = group.Key.Identifier,
                            UpstreamEdgeIdentifier = edge.Identifier,
                            FrameId = currentFrame.Value?.Id,
                            ThreadId = nodeScope.ThreadId,
                            IsRootThread = false
                        })));
                    await Task.WhenAll(nodes.Where(group => !mixedGroups.Contains(group))
                        .Select(group => DoHandleNodeAsync(group.Key, group.First(), nodeScope)));
                    throw new ActivityCheckpointPauseException();
                }
            }

            var handedOff = TryHandOffMixedSuccessors(nodes, nodeScope);
            await Task.WhenAll(nodes.Where(group => !handedOff.Contains(group.Key))
                .Select(group => DoHandleNodeAsync(group.Key, group.First(), nodeScope)));
        }

        /// <summary>
        /// Mixed acceptance handoff: when a transactional root-level firing routes into a successor whose input
        /// already holds streamed tokens, that successor fires as streamed work. The regular transaction's
        /// responsibility ends at the handoff, so the firing is given to the runtime host instead of awaited.
        /// </summary>
        private HashSet<Node> TryHandOffMixedSuccessors(IGrouping<Node, Edge>[] nodes, NodeScope nodeScope)
        {
            var handedOff = new HashSet<Node>();
            var host = StreamedWorkHost;
            // Only a regular transaction hands off; streamed calls already own their streamed successors.
            if (host == null || currentFrame.Value != null || Context.IsCurrentCallStreamed != false)
                return handedOff;

            foreach (var group in nodes)
            {
                var target = group.Key;
                if (target.Type is NodeType.OutputPin or NodeType.Final)
                    continue;

                var edge = group.First();
                if (IsMixedTarget(target, nodeScope) && host(() => DoHandleNodeAsync(target, edge, nodeScope)))
                    handedOff.Add(target);
            }

            return handedOff;
        }

        private bool IsMixedTarget(Node target, NodeScope nodeScope)
        {
            if (target.Type is NodeType.OutputPin or NodeType.Final)
                return false;

            // A data store converts transactional arrivals to streamed work during a streaming busy period.
            if (target.Type == NodeType.DataStore && Context.IsStreamingBusyPeriod)
                return true;

            lock (GetLock(target))
            {
                return Context.GetNodeStreams(target, nodeScope, true)
                    .Any(stream => stream.IsStreamed || stream.Tokens.Any(token => token.IsStreamed));
            }
        }

        /// <summary>
        /// Streamed continuation of structural frames handed off by a regular transaction; starts only after the
        /// transaction has unwound (and recorded its frames). Under an active checkpoint barrier the frames stay
        /// recorded and are resumed by the checkpoint recovery instead.
        /// </summary>
        private async Task ContinueHandedOffFramesAsync(HandoffCoordinator coordinator)
        {
            await coordinator.Unwound.Task;
            if (Context == null || Context.PauseAfterFiring)
                return;

            var previousScope = invocationScope.Value;
            var previousFrame = currentFrame.Value;
            invocationScope.Value = rootNodeScope;
            currentFrame.Value = null;
            try
            {
                await ResumeReadyFiringsAsync(clearPause: false);
            }
            finally
            {
                invocationScope.Value = previousScope;
                currentFrame.Value = previousFrame;
            }
        }

        private async Task<bool> TryForceFinalizeAsync(NodeScope nodeScope)
        {
            if (await valueStorage.GetOrDefaultAsync(Context.Id, CommonValues.ForceFinalizeKey, false))
            {
                await valueStorage.RemoveAsync(Context.Id, CommonValues.ForceFinalizeKey);

                var scope = nodeScope;
                while (scope.BaseNodeScope != null)
                {
                    scope = scope.BaseNodeScope;
                }

                scope.Terminate();

                await CancelAsync();

                return true;
            }

            return false;
        }

        private static void ReportNodeExecuting(Node node, Edge upstreamEdge, IEnumerable<TokenHolder> inputTokens, RootContext context)
        {
            if (/*Debugger.IsAttached && */!SystemTypes.Contains(node.Type))
            {
                lock (lockHandle)
                {
                    Trace.WriteLine($"⦗→s⦘ Activity '{node.Graph.Name}:{context.Id.Instance}': executing '{node.Name}'{(!inputTokens.Any() ? ", no input" : "")}");
                    if (upstreamEdge != null)
                    {
                        Trace.WriteLine($"    Triggered by {((upstreamEdge.TokenType == typeof(ControlToken)) ? "control" : "'" + upstreamEdge.TokenType.GetTokenName() + "'")} flow from '{upstreamEdge.Source.Name}'");
                    }
                    ReportTokens(inputTokens);  
                }
            }
        }

        private static void ReportNodeExecuted(Node node, IEnumerable<TokenHolder> outputTokens, RootContext context)
        {
            if (/*Debugger.IsAttached && */!SystemTypes.Contains(node.Type))
            {
                lock (lockHandle)
                {
                    Trace.WriteLine($"⦗→s⦘ Activity '{node.Graph.Name}:{context.Id.Instance}': executed '{node.Name}'{(!outputTokens.Any() ? ", no output" : "")}");
                    ReportTokens(outputTokens, false);
                }
            }
        }

        private static void ReportExceptionHandled(Node node, string exceptionName, IEnumerable<TokenHolder> outputTokens, RootContext context)
        {
            // if (Debugger.IsAttached)
            {
                lock (lockHandle)
                {
                    Trace.WriteLine($"⦗→s⦘ Activity '{node.Graph.Name}:{context.Id.Instance}': exception '{exceptionName}' handled by exception handler of node '{node.Name}'{(!outputTokens.Any() ? ", no output" : "")}");
                    ReportTokens(outputTokens, false);
                }
            }
        }

        private void ReportNodeAttemptedExecution(Node node, Edge upstreamEdge, IEnumerable<Stream> streams, RootContext context)
        {
            if (/*Debugger.IsAttached && */!SystemTypes.Contains(node.Type))
            {
                lock (lockHandle)
                {
                    Trace.WriteLine($"⦗→s⦘ Activity '{node.Graph.Name}:{context.Id.Instance}': omitting '{node.Name}'");
                    if (upstreamEdge != null)
                    {
                        Trace.WriteLine($"    Triggered by {((upstreamEdge.TokenType == typeof(ControlToken)) ? "control" : "'" + upstreamEdge.TokenType.GetTokenName() + "'")} flow from '{upstreamEdge.Source.Name}'");
                    }
                    ReportTokens(streams.SelectMany(stream => stream.Tokens).Distinct().ToArray());
                    var activatedFlows = streams.Select(s => s.EdgeIdentifier).ToArray();
                    var missingFlows = node.IncomingEdges.Where(e => !activatedFlows.Contains(e.Identifier));
                    ReportMissingFlows(missingFlows);

                    var inputTokens = streams.SelectMany(stream => stream.Tokens).ToArray();
                    var missingRequiredTypes = node.InputTokenTypes
                        .Where(requiredType => !inputTokens.Any(token =>
                            requiredType.IsAssignableFrom(token.GetEffectivePayloadType())))
                        .Distinct()
                        .ToArray();
                    if (missingRequiredTypes.Length > 0)
                    {
                        var missingTypeNames = missingRequiredTypes
                            .Select(type => type.FullName ?? type.Name)
                            .ToArray();
                        logger.LogWarning(
                            "Activity {ActivityName}:{ActivityInstance} skipped node {NodeName}; required input token types were not delivered: {MissingTokenTypes}.",
                            node.Graph.Name,
                            context.Id.Instance,
                            node.Name,
                            string.Join(", ", missingTypeNames));
                        Trace.WriteLine(
                            $"WARNING: Activity '{node.Graph.Name}:{context.Id.Instance}' skipped node '{node.Name}' because required input token types were not delivered: {string.Join(", ", missingTypeNames)}.");
                    }
                }
            }
        }

        private static void ReportTokens(IEnumerable<TokenHolder> inputTokens, bool input = true)
        {
            var inputDigest = inputTokens
                .Where(t => t.Name != typeof(ControlToken).GetTokenName())
                .GroupBy(t => t.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .OrderBy(d => d.Name);

            if (inputDigest.Any())
            {
                Trace.WriteLine($"    {(input ? "Input" : "Output")} token types (with count):");
                foreach (var digest in inputDigest)
                {
                    Trace.WriteLine($"    - '{digest.Name}' ({digest.Count})");
                }
            }
        }

        private static void ReportMissingFlows(IEnumerable<Edge> flows)
        {
            if (flows.Any())
            {
                Trace.WriteLine($"    Not activated incoming flows:");
                foreach (var flow in flows)
                {
                    var tokenName = flow.TargetTokenType.GetTokenName();
                    if (tokenName == typeof(ControlToken).GetTokenName())
                    {
                        Trace.WriteLine($"    - control flow from '{flow.SourceName}'  ");
                    }
                    else
                    {
                        Trace.WriteLine($"    - '{tokenName}' from '{flow.SourceName}'  ");
                    }
                }
            }
        }

        internal void DoHandleOutput(ActionContext context)
        {
            var output = context.Context.GetOutputTokens(context.Node.Identifier, context.NodeScope.ThreadId);
            lock (output)
                output.AddRange(context.OutputTokens);
        }
        

        public void RegisterNodeActivation(Node node)
        {
            lock (Context.Context.Values)
            {
                StateHasChanged = true;
                
                if (!Context.ActivatedNodes.Contains(node.Identifier))
                {
                    Context.ActivatedNodes.Add(node.Identifier);
                }
                
                if (Context.PendingNodes.Contains(node.Identifier))
                {
                    Context.PendingNodes.Remove(node.Identifier);
                }
            }
        }
        public void RegisterNodePending(Node node)
        {
            lock (Context.Context.Values)
            {
                StateHasChanged = true;
                
                if (!Context.PendingNodes.Contains(node.Identifier))
                {
                    Context.PendingNodes.Add(node.Identifier);
                }
            }
        }
        
        private void RegisterFlowAttempt(Edge edge, int tokensCount)
        {
            lock (Context.Context.Values)
            {
                StateHasChanged = true;
                
                if (Context.FlowTokensCount.TryGetValue(edge.Identifier, out var previousTokensCount))
                {
                    if (
                        Context.FlowTokensCount[edge.Identifier] >= edge.Weight &&
                        Context.ActivatedNodes.Contains(edge.Target.Identifier)
                    )
                    {
                        Context.ActivatedNodes.Remove(edge.Target.Identifier);
                        Context.FlowTokensCount[edge.Identifier] = tokensCount;
                    }
                    else
                    {
                        Context.FlowTokensCount[edge.Identifier] = previousTokensCount + tokensCount;
                    }
                }
                else
                {
                    Context.FlowTokensCount[edge.Identifier] = tokensCount;
                }
            }
        }
        
        private async Task<bool> DoHandleEdgeAsync(Edge edge, ActionContext context)
        {
            var flowContext = new FlowContext(context.Context, context.NodeScope, edge);
            var stream = Context.GetStream(edge.Identifier, RootContext.GetStreamThreadId(edge, context.NodeScope));
            
            var edgeTokenName = edge.TokenType.GetTokenName();

            IEnumerable<TokenHolder> originalTokens = context.OutputTokens
            .Where(t => edge.TokenType.IsAssignableFrom(t.ActualPayloadType))
            .Select(token => token.CopyForExecution(context.IsStreamed || token.IsStreamed))
            .ToArray();

            flowContext.TokenCount = stream.Tokens.Count;
            flowContext.SourceTokenCount = originalTokens.Count();

            var afterContext = await Inspector.OnFlowActivateAsync(flowContext, ActivateFlowAsync);
            return afterContext.Activated;

            async Task<IActivityAfterFlowContext> ActivateFlowAsync()
            {
                var consumedTokens = new List<TokenHolder>();
                var processedTokens = new List<TokenHolder>();

                foreach (var token in originalTokens)
                {
                    var processedToken = token;
                    foreach (var logic in edge.TokenPipeline.Actions)
                    {
                        foreach (var action in logic.Actions)
                        {
                            var pipelineContext = new TokenPipelineContext(context, edge, processedToken);
                            processedToken = await action.Invoke(pipelineContext, Inspector);
                            if (processedToken != null)
                            {
                                processedToken.IsStreamed |= token.IsStreamed || context.IsStreamed;
                            }
                        }
                    }

                    if (processedToken != null)
                    {
                        consumedTokens.Add(token);

                        processedTokens.Add(processedToken);
                    }
                }

                flowContext.TargetTokenCount = processedTokens.Count;

                if (context.NodeScope.CancellationToken.IsCancellationRequested)
                {
                    flowContext.ActivationResult = false;
                    return flowContext;
                }

                RegisterFlowAttempt(
                    edge,
                    edgeTokenName != typeof(ControlToken).GetTokenName()
                        ? processedTokens.Count
                        : 1
                );

                lock (GetLock(edge.Target))
                {
                    // A concurrent destination firing may have consumed and detached the previous stream while
                    // this flow's asynchronous pipeline ran. Commit to the current destination-owned marking.
                    stream = Context.GetStream(edge.Identifier, RootContext.GetStreamThreadId(edge, context.NodeScope));
                    var flowActivated = processedTokens.Count + stream.Tokens.Count >= edge.Weight;
                    flowContext.ActivationResult = flowActivated;
                    if (flowActivated)
                    {
                        if (edgeTokenName != typeof(ControlToken).GetTokenName())
                        {
                            var controlToken = new ControlToken().ToTokenHolder();
                            controlToken.IsStreamed = context.IsStreamed || processedTokens.Any(token => token.IsStreamed);
                            processedTokens.Add(controlToken);
                        }

                        if (!edge.Source.Options.HasFlag(NodeOptions.ImplicitFork) && consumedTokens.Any())
                        {
                            foreach (var token in consumedTokens)
                                context.OutputTokens.Remove(token);
                        }
                    }

                    stream.Consume(processedTokens, edge.Source.Type == NodeType.DataStore);
                    flowContext.TokenCount = stream.Tokens.Count;

                    if (!flowActivated)
                        flowContext.TargetTokenCount = processedTokens.Count;
                }

                return flowContext;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            await rootNodeScope.DisposeAsync();
        }

        public async Task<object> GetActivityAsync(Type activityType)
            => await StateflowsActivator.CreateModelElementInstanceAsync(NodeScope.ServiceProvider, activityType, "activity");
    }
}
