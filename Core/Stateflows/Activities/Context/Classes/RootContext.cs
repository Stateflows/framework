using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Context;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Streams;
using Stateflows.Activities.Registration;
using Stream = Stateflows.Activities.Streams.Stream;

namespace Stateflows.Activities.Context.Classes
{
    public class RootContext
    {
        public ActivityId Id { get; }

        internal StateflowsContext Context { get; set; }

        internal Executor Executor { get; set; }

        [JsonIgnore]
        private readonly AsyncLocal<bool> isStreamingExecution = new();
        [JsonIgnore]
        private readonly AsyncLocal<InvocationProvenance?> invocationProvenance = new();
        [JsonIgnore]
        private int outstandingTransactions;
        private int streamingBusyPeriod;
        [JsonIgnore]
        private int pauseAfterFiring;
        [JsonIgnore]
        private Guid rootThreadId;

        private const string ReadyFiringsKey = "Activity.ReadyFirings";
        private const string ContinuationFramesKey = "Activity.ContinuationFrames";
        private const string InterleavedIdleFramesKey = "Activity.InterleavedIdleFrames";

        internal ActivityContinuationFrame GetInterleavedIdleFrame(string node, Guid threadId)
        {
            lock (Context.Values)
                return Context.Values.TryGetValue(InterleavedIdleFramesKey, out var value) &&
                    value is List<ActivityContinuationFrame> frames
                        ? frames.FirstOrDefault(frame => frame.NodeIdentifier == node &&
                            frame.OuterThreadId == threadId)
                        : null;
        }

        internal void SetInterleavedIdleFrame(ActivityContinuationFrame frame, bool idle)
        {
            lock (Context.Values)
            {
                if (!Context.Values.TryGetValue(InterleavedIdleFramesKey, out var value) ||
                    value is not List<ActivityContinuationFrame> frames)
                    Context.Values[InterleavedIdleFramesKey] = frames = [];
                frames.RemoveAll(candidate => candidate.Id == frame.Id);
                if (idle)
                    frames.Add(frame);
            }
        }

        internal bool PauseAfterFiring
        {
            get => System.Threading.Volatile.Read(ref pauseAfterFiring) != 0;
            set => Interlocked.Exchange(ref pauseAfterFiring, value ? 1 : 0);
        }

        internal Guid RootThreadId
        {
            get
            {
                lock (Context.Values)
                    return rootThreadId == Guid.Empty ? rootThreadId = Guid.NewGuid() : rootThreadId;
            }
        }

        internal bool HasReadyFirings
        {
            get
            {
                lock (Context.Values)
                    return (Context.Values.TryGetValue(ReadyFiringsKey, out var value) &&
                        value is List<ReadyActivityFiring> { Count: > 0 }) ||
                        (Context.Values.TryGetValue(ContinuationFramesKey, out var frames) &&
                        frames is List<ActivityContinuationFrame> { Count: > 0 });
            }
        }

        internal bool HasReadyFiringsInFrame(Guid frameId, Guid threadId)
        {
            lock (Context.Values)
                return Context.Values.TryGetValue(ReadyFiringsKey, out var value) &&
                    value is List<ReadyActivityFiring> ready &&
                    ready.Any(firing => firing.FrameId == frameId && firing.ThreadId == threadId);
        }

        internal (ReadyActivityFiring[] Firings, ActivityContinuationFrame[] Frames) TakeCheckpointContinuation()
        {
            lock (Context.Values)
            {
                var firings = Context.Values.TryGetValue(ReadyFiringsKey, out var value) &&
                    value is List<ReadyActivityFiring> ready
                        ? ready.ToArray()
                        : [];
                var frames = Context.Values.TryGetValue(ContinuationFramesKey, out var framesValue) &&
                    framesValue is List<ActivityContinuationFrame> suspended
                        ? suspended.ToArray()
                        : [];
                Context.Values.Remove(ReadyFiringsKey);
                Context.Values.Remove(ContinuationFramesKey);
                return (firings, frames);
            }
        }

        internal void RestoreCheckpointContinuation(IEnumerable<ReadyActivityFiring> ready, IEnumerable<ActivityContinuationFrame> frames)
        {
            AddReadyFirings(ready);
            foreach (var frame in frames)
                AddContinuationFrame(frame);
        }

        internal void AddReadyFirings(IEnumerable<ReadyActivityFiring> ready)
        {
            lock (Context.Values)
            {
                if (!Context.Values.TryGetValue(ReadyFiringsKey, out var value) ||
                    value is not List<ReadyActivityFiring> existing)
                    Context.Values[ReadyFiringsKey] = existing = [];
                existing.AddRange(ready);
            }
        }

        internal void AddContinuationFrame(ActivityContinuationFrame frame)
        {
            lock (Context.Values)
            {
                if (!Context.Values.TryGetValue(ContinuationFramesKey, out var value) ||
                    value is not List<ActivityContinuationFrame> existing)
                    Context.Values[ContinuationFramesKey] = existing = [];
                if (!existing.Any(candidate => candidate.Id == frame.Id))
                    existing.Add(frame);
            }
        }

        [JsonIgnore]
        internal bool IsStreamingExecution
        {
            get => isStreamingExecution.Value || IsCurrentInvocationStreamed;
            set => isStreamingExecution.Value = value;
        }

        [JsonIgnore]
        internal bool IsCurrentInvocationStreamed
        {
            get
            {
                for (var provenance = invocationProvenance.Value; provenance != null; provenance = provenance.Parent)
                    if (provenance.IsStreamed)
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Opens an invocation provenance frame. A tracked transactional frame represents a regular transaction
        /// admitted into a live session; while any such transaction is outstanding its idle tokens keep their
        /// transactional color, and the marking is recolored as streamed once the last transaction closes.
        /// </summary>
        internal InvocationProvenanceScope BeginInvocationProvenance(bool streamed, bool trackTransaction = false, bool callRoot = false)
        {
            var previous = invocationProvenance.Value;
            var current = new InvocationProvenance(streamed, previous, callRoot);
            invocationProvenance.Value = current;
            var tracked = trackTransaction && !streamed;
            if (tracked)
                Interlocked.Increment(ref outstandingTransactions);
            var modeScope = Context.PushExecutionMode(IsCurrentInvocationStreamed
                ? ExecutionMode.Streamed : ExecutionMode.Transactional);
            return new InvocationProvenanceScope(this, current, previous, tracked, modeScope);
        }

        /// <summary>
        /// Whether the runtime call (regular transaction, streamed admission or handed-off streamed work) that owns
        /// the current asynchronous flow is streamed; <c>null</c> outside of a session call.
        /// </summary>
        [JsonIgnore]
        internal bool? IsCurrentCallStreamed
        {
            get
            {
                for (var provenance = invocationProvenance.Value; provenance != null; provenance = provenance.Parent)
                    if (provenance.IsCallRoot)
                        return provenance.IsStreamed;
                return null;
            }
        }

        /// <summary>
        /// Records an execution exception for the Activity and for the runtime call owning the current flow, so a
        /// live session reports failures of a call without attributing failures of concurrent invocations to it.
        /// </summary>
        internal void RecordException(Exception exception)
        {
            Context.ExecutionExceptions.Enqueue(exception);
            for (var provenance = invocationProvenance.Value; provenance != null; provenance = provenance.Parent)
            {
                if (provenance.IsCallRoot)
                {
                    provenance.Exceptions.Enqueue(exception);
                    break;
                }
            }
        }

        [JsonIgnore]
        internal int OutstandingTransactions => System.Threading.Volatile.Read(ref outstandingTransactions);

        [JsonIgnore]
        internal bool IsStreamingBusyPeriod => System.Threading.Volatile.Read(ref streamingBusyPeriod) != 0;

        /// <summary>
        /// Marks the start of a streaming busy period. Work owned by an outstanding regular transaction keeps its
        /// transactional color; idle marking is recolored immediately only when no transaction is outstanding.
        /// </summary>
        internal void BeginStreamingBusyPeriod()
        {
            if (Interlocked.Exchange(ref streamingBusyPeriod, 1) == 0 && OutstandingTransactions == 0)
                RecolorMarkingAsStreamed();
        }

        /// <summary>Ends the streaming busy period; the persisted marking returns to ordinary transactional semantics.</summary>
        internal void EndStreamingBusyPeriod()
        {
            Interlocked.Exchange(ref streamingBusyPeriod, 0);
            RecolorMarking(false);
        }

        private void CloseTransaction()
        {
            if (Interlocked.Decrement(ref outstandingTransactions) == 0 && IsStreamingBusyPeriod)
                RecolorMarkingAsStreamed();
        }

        internal sealed class InvocationProvenance(bool streamed, InvocationProvenance? parent, bool callRoot)
        {
            public InvocationProvenance? Parent { get; } = parent;
            public bool IsStreamed { get; } = streamed;
            public bool IsCallRoot { get; } = callRoot;
            public System.Collections.Concurrent.ConcurrentQueue<Exception> Exceptions { get; } = new();
        }

        internal sealed class InvocationProvenanceScope(
            RootContext context,
            InvocationProvenance invocation,
            InvocationProvenance? previous,
            bool tracked, IDisposable modeScope) : IDisposable
        {
            private int disposed;

            /// <summary>Exceptions recorded by the call opened with this scope.</summary>
            public IEnumerable<Exception> Exceptions => invocation.Exceptions;

            public void Dispose()
            {
                if (Interlocked.Exchange(ref disposed, 1) != 0)
                    return;
                context.invocationProvenance.Value = previous;
                modeScope.Dispose();
                if (tracked)
                    context.CloseTransaction();
            }
        }

        internal void RecolorMarkingAsStreamed() => RecolorMarking(true);

        private void RecolorMarking(bool streamed)
        {
            lock (Streams)
            {
                foreach (var stream in Streams.Values.SelectMany(edges => edges.Values))
                {
                    stream.IsStreamed = streamed;
                    foreach (var token in stream.Tokens)
                        token.IsStreamed = streamed;
                }
            }

            foreach (var token in OutputTokens.Values.SelectMany(nodes => nodes.Values).SelectMany(tokens => tokens))
                token.IsStreamed = streamed;

        }

        public RootContext(StateflowsContext context)
        {
            Context = context;
            // Discard output history from snapshots created before output became notification-based.
            Context.Values.Remove("ActivityOutputTokens");
            Id = new ActivityId(Context.Id);
        }

        private Dictionary<Guid, Dictionary<string, Stream>> streams = null;
        public Dictionary<Guid, Dictionary<string, Stream>> Streams
        {
            get
            {
                lock (Context.Values)
                {
                    if (streams == null)
                    {
                        if (!Context.Values.TryGetValue(Constants.Streams, out var streamsObj))
                        {
                            streams = new Dictionary<Guid, Dictionary<string, Stream>>();
                            Context.Values[Constants.Streams] = streams;
                        }
                        else
                        {
                            streams = streamsObj as Dictionary<Guid, Dictionary<string, Stream>>;
                        }
                    }
                }

                return streams;
            }
        }

        public Stream GetStream(string edgeIdentifier, Guid threadId)
        {
            Stream stream;

            lock (Streams)
            {
                if (!Streams.TryGetValue(threadId, out var edges))
                {
                    edges = new Dictionary<string, Stream>();
                    Streams.Add(threadId, edges);
                }

                if (!edges.TryGetValue(edgeIdentifier, out stream))
                {
                    stream = new Stream() { EdgeIdentifier = edgeIdentifier };
                    edges.Add(edgeIdentifier, stream);
                }
            }

            return stream;
        }

        public Dictionary<string, Stream> GetStreams(Guid threadId)
        {
            lock (Streams)
            {
                if (!Streams.TryGetValue(threadId, out var edges))
                {
                    edges = new Dictionary<string, Stream>();
                    Streams.Add(threadId, edges);
                }

                return edges;
            }
        }

        internal IEnumerable<Stream> GetNodeStreams(Node node, Guid threadId, bool activatedOnly)
        {
            lock (Streams)
            {
                return node.IncomingEdges
                    .Select(edge => GetStream(edge.Identifier, threadId))
                    .Where(stream => stream.IsActivated || !activatedOnly)
                    .ToArray();
            }
        }

        internal static Guid GetStreamThreadId(Edge edge, NodeScope scope)
        {
            if (edge.IsCrossScope)
            {
                for (var owner = scope; owner != null; owner = owner.BaseNodeScope)
                    if (owner.Node == edge.Owner)
                        return owner.ThreadId;
                throw new InvalidOperationException($"Owning execution scope is unavailable for crossing flow '{edge.Identifier}'.");
            }
            return scope.ThreadId;
        }

        internal IEnumerable<Stream> GetNodeStreams(Node node, NodeScope scope, bool activatedOnly)
        {
            lock (Streams)
                return node.IncomingEdges
                    .Select(edge => GetStream(edge.Identifier, GetStreamThreadId(edge, scope)))
                    .Where(stream => stream.IsActivated || !activatedOnly)
                    .ToArray();
        }

        public void ClearStream(string edgeIdentifier, Guid threadId)
        {
            lock (Streams)
            {
                if (Streams.TryGetValue(threadId, out var edges))
                {
                    edges.Remove(edgeIdentifier);

                    if (!edges.Any())
                    {
                        Streams.Remove(threadId);
                    }
                }
            }
        }

        private Dictionary<Guid, Dictionary<string, List<TokenHolder>>> outputTokens = null;
        public Dictionary<Guid, Dictionary<string, List<TokenHolder>>> OutputTokens
        {
            get
            {
                lock (Context.Values)
                {
                    if (outputTokens == null)
                    {
                        if (!Context.Values.TryGetValue(Constants.OutputTokens, out var outputTokensObj))
                        {
                            outputTokens = new Dictionary<Guid, Dictionary<string, List<TokenHolder>>>();
                            Context.Values[Constants.OutputTokens] = outputTokens;
                        }
                        else
                        {
                            outputTokens = outputTokensObj as Dictionary<Guid, Dictionary<string, List<TokenHolder>>>;
                        }
                    }
                }

                return outputTokens;
            }
        }

        public List<TokenHolder> GetOutputTokens(string nodeName, Guid threadId)
        {
            List<TokenHolder> tokens;

            lock (OutputTokens)
            {
                if (!OutputTokens.TryGetValue(threadId, out var nodes))
                {
                    nodes = new Dictionary<string, List<TokenHolder>>();
                    OutputTokens[threadId] = nodes;
                }

                if (!nodes.TryGetValue(nodeName, out tokens))
                {
                    tokens = new List<TokenHolder>();
                    nodes[nodeName] = tokens;
                }
            }

            return tokens;
        }

        internal List<TokenHolder> TakeOutputTokens(string nodeName, Guid threadId)
        {
            var tokens = GetOutputTokens(nodeName, threadId);
            lock (tokens)
            {
                var result = tokens.ToList();
                tokens.Clear();
                return result;
            }
        }

        private Dictionary<string, Guid> activeNodes = null;
        public Dictionary<string, Guid> ActiveNodes
        {
            get
            {
                lock (Context.Values)
                {
                    if (activeNodes == null)
                    {
                        if (!Context.Values.TryGetValue(Constants.ActiveNodes, out var activeNodesObj))
                        {
                            activeNodes = new Dictionary<string, Guid>();
                            Context.Values[Constants.ActiveNodes] = activeNodes;
                        }
                        else
                        {
                            activeNodes = activeNodesObj as Dictionary<string, Guid>;
                        }
                    }
                }

                return activeNodes;
            }
        }

        private List<string> activatedNodes = null;

        public List<string> ActivatedNodes
            => activatedNodes ??= [];

        private List<string> pendingNodes = null;
        public List<string> PendingNodes
            => pendingNodes ??= [];

        private Dictionary<string, int> flowTokensCount = null;
        public Dictionary<string, int> FlowTokensCount
        {
            get
            {
                lock (Context.Values)
                {
                    if (flowTokensCount == null)
                    {
                        if (!Context.Values.TryGetValue(Constants.FlowTokensCount, out var flowTokensCountObj))
                        {
                            flowTokensCount = new Dictionary<string, int>();
                            Context.Values[Constants.FlowTokensCount] = flowTokensCount;
                        }
                        else
                        {
                            flowTokensCount = flowTokensCountObj as Dictionary<string, int>;
                        }
                    }
                }

                return flowTokensCount;
            }
        }

        private List<string> activatedFlows = null;

        public List<string> ActivatedFlows
            => activatedFlows ??= [];

        private Dictionary<string, Guid> nodeThreads = null;
        public Dictionary<string, Guid> NodeThreads
        {
            get
            {
                lock (Context.Values)
                {
                    if (nodeThreads == null)
                    {
                        if (!Context.Values.TryGetValue(Constants.NodeThreads, out var nodesThreadsObj))
                        {
                            nodeThreads = new Dictionary<string, Guid>();
                            Context.Values[Constants.NodeThreads] = nodeThreads;
                        }
                        else
                        {
                            nodeThreads = nodesThreadsObj as Dictionary<string, Guid>;
                        }
                    }
                }

                return nodeThreads;
            }
        }

        private Dictionary<string, Guid> nodeTimeEvents = null;
        public Dictionary<string, Guid> NodeTimeEvents
        {
            get
            {
                lock (Context.Values)
                {
                    if (nodeTimeEvents == null)
                    {
                        if (!Context.Values.TryGetValue(Constants.NodeTimeEvents, out var nodesTimeEventsObj))
                        {
                            nodeTimeEvents = new Dictionary<string, Guid>();
                            Context.Values[Constants.NodeTimeEvents] = nodeTimeEvents;
                        }
                        else
                        {
                            nodeTimeEvents = nodesTimeEventsObj as Dictionary<string, Guid>;
                        }
                    }
                }

                return nodeTimeEvents;
            }
        }

        private Dictionary<string, Guid> nodeStartupEvents = null;
        public Dictionary<string, Guid> NodeStartupEvents
        {
            get
            {
                lock (Context.Values)
                {
                    if (nodeStartupEvents == null)
                    {
                        if (!Context.Values.TryGetValue(Constants.NodeStartupEvents, out var nodesStartupEventsObj))
                        {
                            nodeStartupEvents = new Dictionary<string, Guid>();
                            Context.Values[Constants.NodeStartupEvents] = nodeStartupEvents;
                        }
                        else
                        {
                            nodeStartupEvents = nodesStartupEventsObj as Dictionary<string, Guid>;
                        }
                    }
                }

                return nodeStartupEvents;
            }
        }

        [JsonIgnore]
        private readonly AsyncLocal<LockedList<Node>?> nodesToExecute = new();

        [JsonIgnore]
        private readonly LockedList<Node> defaultNodesToExecute = [];

        [JsonIgnore]
        internal LockedList<Node> NodesToExecute => nodesToExecute.Value ?? defaultNodesToExecute;

        internal IDisposable BeginEventExecution()
        {
            var previous = nodesToExecute.Value;
            nodesToExecute.Value = [];
            return new ExecutionScope(() => nodesToExecute.Value = previous);
        }

        private sealed class ExecutionScope(System.Action dispose) : IDisposable
        {
            private System.Action? dispose = dispose;

            public void Dispose()
                => Interlocked.Exchange(ref dispose, null)?.Invoke();
        }

        [JsonIgnore]
        private Dictionary<Guid, List<Node>> TerminatedNodes { get; set; } = [];

        internal bool IsTerminated(Node node, Guid threadId)
        {
            lock (TerminatedNodes)
            {
                return TerminatedNodes.TryGetValue(threadId, out var nodes) && nodes.Contains(node);
            }
        }

        internal void MarkAsTerminated(Node node, Guid threadId)
        {
            lock (TerminatedNodes)
            {
                if (!TerminatedNodes.TryGetValue(threadId, out var nodes))
                {
                    nodes = new List<Node>();
                    TerminatedNodes.Add(threadId, nodes);
                }

                nodes.Add(node);
            }
        }

        internal void UnmarkAsTerminated(Node node, Guid threadId)
        {
            lock (TerminatedNodes)
            {
                if (!TerminatedNodes.TryGetValue(threadId, out var nodes))
                {
                    nodes = new List<Node>();
                    TerminatedNodes.Add(threadId, nodes);
                }

                nodes.Remove(node);
            }
        }

        public bool Initialized
        {
            get => Context.Values.TryGetValue(Constants.Initialized, out var consumed) && (bool)consumed;
            set => Context.Values[Constants.Initialized] = value;
        }

        public bool Finalized
        {
            get => Context.Values.TryGetValue(Constants.Finalized, out var consumed) && (bool)consumed;
            set => Context.Values[Constants.Finalized] = value;
        }

        public bool ForceConsumed
        {
            get => Context.Values.TryGetValue(Constants.ForceConsumed, out var consumed) && (bool)consumed;
            set => Context.Values[Constants.ForceConsumed] = value;
        }

        private sealed record EventFrame(EventHolder Holder, EventFrame Previous);

        private readonly AsyncLocal<EventFrame> currentEvent = new();

        public void SetEvent(EventHolder eventHolder)
        {
            ArgumentNullException.ThrowIfNull(eventHolder);
            currentEvent.Value = new EventFrame(eventHolder, currentEvent.Value);
        }

        public void ClearEvent()
        {
            var frame = currentEvent.Value
                ?? throw new InvalidOperationException("No Activity event scope is active.");
            currentEvent.Value = frame.Previous;
        }

        public EventHolder EventHolder => currentEvent.Value?.Holder;

        public EventHolder ExecutionTriggerHolder
        {
            get
            {
                var frame = currentEvent.Value;
                while (frame?.Previous != null)
                {
                    frame = frame.Previous;
                }

                return frame?.Holder;
            }
        }

        public readonly List<Exception> Exceptions = [];

        internal Node NodeOfOrigin { get; set; }

        internal bool IsNodeCompleted(Node node, NodeScope nodeScope)
            => nodeScope is { IsTerminated: true } ||
                (nodeScope?.ChildScopes.Any(child => child.IsTerminated) ?? false) ||
                (
                    !(
                        node.Type == NodeType.IterativeActivity &&
                        node.FinalNode != null
                    ) &&
                    
                    // event nodes without incoming flows ('ever-active')
                    !node.Nodes.Values.Any(childNode =>
                        (
                            childNode.Type == NodeType.AcceptEventAction ||
                            childNode.Type == NodeType.TimeEventAction
                        ) &&
                        !childNode.IncomingEdges.Any()
                    ) &&
                    
                    // active structured child nodes
                    !node.Nodes.Values.Any(childNode =>
                        (
                            childNode.Type == NodeType.StructuredActivity ||
                            childNode.Type == NodeType.ParallelActivity ||
                            childNode.Type == NodeType.IterativeActivity
                        ) &&
                        !IsNodeCompleted(childNode, null)
                    ) &&
                    
                    // active event accepting nodes
                    !node.Nodes.Values.Any(node => ActiveNodes.Keys.Contains(node.Identifier)) &&
                    
                    // input pins on topmost level
                    (
                        (
                            node is Graph &&
                            !node.InputPins.Any()
                        ) ||
                        !(node is Graph)
                    )
                );

        public async Task SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers = null)
        {
            var locator = Executor.NodeScope.ServiceProvider.GetService<IBehaviorLocator>();
            if (locator != null && locator.TryLocateBehavior(Context.ContextParentId ?? Id.BehaviorId, out var behavior))
            {
                await behavior.SendAsync(@event, headers);
            }
        }
    }
}
