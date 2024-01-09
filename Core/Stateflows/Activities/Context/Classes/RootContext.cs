using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Streams;
using Stateflows.Activities.Registration;
using Stateflows.Common.Classes;

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

        internal IEnumerable<Stream> GetStreams(Node node, Guid threadId)
        {
            lock (Streams)
            {
                return node.IncomingEdges
                    .Select(edge => GetStream(edge.Identifier, threadId))
                    .Where(stream => stream.IsActivated)
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

        private Dictionary<Guid, Dictionary<string, List<Token>>> outputTokens = null;
        public Dictionary<Guid, Dictionary<string, List<Token>>> OutputTokens
        {
            get
            {
                lock (Context.Values)
                {
                    if (outputTokens == null)
                    {
                        if (!Context.Values.TryGetValue(Constants.OutputTokens, out var outputTokensObj))
                        {
                            outputTokens = new Dictionary<Guid, Dictionary<string, List<Token>>>();
                            Context.Values[Constants.OutputTokens] = outputTokens;
                        }
                        else
                        {
                            outputTokens = outputTokensObj as Dictionary<Guid, Dictionary<string, List<Token>>>;
                        }
                    }
                }

                return outputTokens;
            }
        }

        public List<Token> GetOutputTokens(string nodeName, Guid threadId)
        {
            List<Token> tokens;

            lock (OutputTokens)
            {
                if (!OutputTokens.TryGetValue(threadId, out var nodes))
                {
                    nodes = new Dictionary<string, List<Token>>();
                    OutputTokens[threadId] = nodes;
                }

                if (!nodes.TryGetValue(nodeName, out tokens))
                {
                    tokens = new List<Token>();
                    nodes[nodeName] = tokens;
                }
            }

            return tokens;
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

        public bool ForceConsumed
        {
            get => Context.Values.TryGetValue(Constants.ForceConsumed, out var consumed) && (bool)consumed;
            set => Context.Values[Constants.ForceConsumed] = value;
        }

        private readonly Stack<Event> EventsStack = new Stack<Event>();

        public void SetEvent(Event @event)
        {
            EventsStack.Push(@event);
        }

        public void ClearEvent()
        {
            EventsStack.Pop();
        }

        public Event Event => EventsStack.Any()
            ? EventsStack.Peek()
            : null;

        internal Exception Exception { get; set; }

        internal Node NodeOfOrigin { get; set; }

        internal void ClearTemporaryInternalValues()
        {
            Context.Values.Remove(Constants.Node);
            Context.Values.Remove(Constants.Event);
            Context.Values.Remove(Constants.SourceNode);
            Context.Values.Remove(Constants.TargetNode);
            Context.Values.Remove(Constants.ForceConsumed);
        }

        public async Task Send<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            var locator = Executor.NodeScope.ServiceProvider.GetService<IBehaviorLocator>();
            if (locator != null && locator.TryLocateBehavior(Id.BehaviorId, out var behavior))
            {
                await behavior.SendAsync(@event);
            }
        }
    }
}
