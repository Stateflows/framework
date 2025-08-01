﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Context;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Streams;
using Stateflows.Activities.Registration;

namespace Stateflows.Activities.Context.Classes
{
    public class RootContext
    {
        public ActivityId Id { get; }

        internal StateflowsContext Context { get; set; }

        internal Executor Executor { get; set; }

        public RootContext(StateflowsContext context)
        {
            Context = context;
            Id = new ActivityId(Context.Id);
        }

        public Dictionary<string, string> GlobalValues => Context.GlobalValues;

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

        private List<TokenHolder> activityOutputTokens = null;
        public List<TokenHolder> ActivityOutputTokens
        {
            get
            {
                lock (Context.Values)
                {
                    if (activityOutputTokens == null)
                    {
                        if (!Context.Values.TryGetValue(Constants.ActivityOutputTokens, out var activityOutputTokensObj))
                        {
                            activityOutputTokens = new List<TokenHolder>();
                            Context.Values[Constants.ActivityOutputTokens] = activityOutputTokens;
                        }
                        else
                        {
                            activityOutputTokens = activityOutputTokensObj as List<TokenHolder>;
                        }
                    }
                }

                return activityOutputTokens;
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
            => activatedNodes ??= new List<string>();

        private List<string> pendingNodes = null;
        public List<string> PendingNodes
            => pendingNodes ??= new List<string>();

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
            => activatedFlows ??= new List<string>();

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
        internal LockedList<Node> NodesToExecute { get; set; } = new LockedList<Node>();

        [JsonIgnore]
        private Dictionary<Guid, List<Node>> TerminatedNodes { get; set; } = new Dictionary<Guid, List<Node>>();

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

        private readonly Stack<EventHolder> EventsStack = new Stack<EventHolder>();

        public void SetEvent(EventHolder eventHolder)
        {
            EventsStack.Push(eventHolder);
        }

        public void ClearEvent()
        {
            EventsStack.Pop();
        }

        public EventHolder EventHolder => EventsStack.Any()
            ? EventsStack.Peek()
            : null;

        public EventHolder ExecutionTriggerHolder => EventsStack.Any()
            ? EventsStack.Last()
            : null;

        public readonly List<Exception> Exceptions = new List<Exception>();

        internal Node NodeOfOrigin { get; set; }

        internal bool IsNodeCompleted(Node node, NodeScope nodeScope)
            => nodeScope is { IsTerminated: true } ||
                nodeScope is { ChildScope: { IsTerminated: true } } ||
                (
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
                    
                    // input node on topmost level
                    (
                        (
                            node is Graph &&
                            node.InputNode == null
                        ) ||
                        !(node is Graph)
                    )
                );

        public async Task Send<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)

        {
            var locator = Executor.NodeScope.ServiceProvider.GetService<IBehaviorLocator>();
            if (locator != null && locator.TryLocateBehavior(Id.BehaviorId, out var behavior))
            {
                await behavior.SendAsync(@event, headers);
            }
        }
    }
}
