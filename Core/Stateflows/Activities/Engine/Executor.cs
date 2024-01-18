using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Activities.Models;
using Stateflows.Activities.Streams;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;

namespace Stateflows.Activities.Engine
{
    internal class Executor : IDisposable
    {
        public Graph Graph { get; }

        public ActivitiesRegister Register { get; }

        public NodeScope NodeScope { get; }


        private readonly ILogger<Executor> Logger;

        public Executor(ActivitiesRegister register, Graph graph, IServiceProvider serviceProvider)
        {
            Register = register;
            NodeScope = new NodeScope(serviceProvider, Guid.NewGuid());
            Graph = graph;

            Logger = serviceProvider.GetService<ILogger<Executor>>();
        }

        public RootContext Context { get; private set; }

        private Inspector inspector;
        public Inspector Inspector
            => inspector ??= new Inspector(this, Logger);

        private EventWaitHandle FinalizationEvent { get; } = new EventWaitHandle(false, EventResetMode.AutoReset);

        private bool Finalized { get; set; } = false;

        public IEnumerable<Node> GetActiveNodes()
            => Context.ActiveNodes.Keys
                .Select(nodeId => Graph.AllNodes[nodeId])
                .OrderByDescending(node => node.Level)
                .ToArray();

        public async Task<bool> HydrateAsync(RootContext context)
        {
            Context = context;
            Context.Executor = this;

            await Inspector.AfterHydrateAsync(new ActivityActionContext(Context, NodeScope.CreateChildScope()));

            return true;
        }

        public async Task<RootContext> DehydrateAsync()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' already dehydrated?");

            await Inspector.BeforeDehydrateAsync(new ActivityActionContext(Context, NodeScope.CreateChildScope()));

            NodeScope.Dispose();

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

        public async Task<bool> InitializeAsync(InitializationRequest @event)
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is activity '{Graph.Name}' hydrated?");

            if (!Initialized && await DoInitializeActivityAsync(@event))
            {
                Context.Initialized = true;

                Context.ClearEvent();

                await ExecuteGraphAsync();

                return true;
            }

            Debug.WriteLine($"{Context.Id.Instance} initialized already");

            return false;
        }

        private IEnumerable<Token> OutputTokens { get; set; } = null;

        public async Task<IEnumerable<Token>> GetResultAsync()
        {
            await FinalizationEvent.WaitOneAsync();

            return OutputTokens;
        }

        public async Task<EventStatus> ProcessAsync<TEvent>(TEvent @event)
            where TEvent : Event
        {
            var result = EventStatus.Rejected;

            if (Initialized)
            {
                result = await DoProcessAsync(@event);
            }

            // disposal

            return result;
        }

        private async Task<EventStatus> DoProcessAsync<TEvent>(TEvent @event)
            where TEvent : Event
        {
            Debug.Assert(Context != null, $"Context is not available. Is activity '{Graph.Name}' hydrated?");

            var activeNodes = GetActiveNodes();
            var activeNode = activeNodes.FirstOrDefault(node =>
                node.EventType == @event.GetType() &&
                (
                    !(@event is TimeEvent) ||
                    (
                        Context.NodeTimeEvents.TryGetValue(node.Identifier, out var timeEventId) &&
                        @event.Id == timeEventId
                    )
                )
            );

            if (activeNode == null)
            {
                return EventStatus.NotConsumed;
            }

            while (activeNode != null)
            {
                Context.NodesToExecute.Add(activeNode);

                activeNode = activeNode.Parent;
            }

            return await ExecuteGraphAsync();
        }

        private async Task<EventStatus> ExecuteGraphAsync()
        {
            (var output, var finalized) = await DoExecuteStructuredNodeAsync(Graph, NodeScope);

            if (finalized)
            {
                await DoFinalizeAsync(output);
            }

            return EventStatus.Consumed;
        }

        public async Task DoFinalizeAsync(IEnumerable<Token> outputTokens)
        {
            if (Finalized) return;

            Finalized = true;

            if (Graph.OutputNode != null)
            {
                OutputTokens = outputTokens;
            }

            var context = new ActivityActionContext(Context, NodeScope);

            await Inspector.BeforeActivityFinalizationAsync(context);

            await Graph.Finalize.WhenAll(context);

            await Inspector.AfterActivityFinalizationAsync(context);

            FinalizationEvent.Set();
        }

        public async Task<(IEnumerable<Token> Output, bool Finalized)> DoExecuteStructuredNodeAsync(Node node, NodeScope nodeScope, IEnumerable<Token> input = null)
        {
            if (Context.NodeThreads.TryGetValue(node.Identifier, out var storedThreadId))
            {
                nodeScope.ThreadId = storedThreadId;
            }
            else
            {
                Context.NodeThreads.Add(node.Identifier, nodeScope.ThreadId);
            }

            var finalized = await DoExecuteNodeStructureAsync(
                node,
                nodeScope,
                input
            );

            var result = (
                node.OutputNode != null
                    ? Context.GetOutputTokens(node.OutputNode.Identifier, nodeScope.ThreadId)
                    : new List<Token>(),
                finalized
            );

            if (finalized)
            {
                Context.NodeThreads.Remove(node.Identifier);
            }

            return result;
        }

        public async Task<IEnumerable<Token>> DoExecuteParallelNodeAsync<TToken>(ActionContext context)
            where TToken : Token, new()
        {
            var restOfInput = context.InputTokens.Where(t => !(t is TToken)).ToArray();

            var parallelInput = context.InputTokens.OfType<TToken>().ToArray();

            var threadIds = parallelInput.ToDictionary<Token, Token, Guid>(t => t, t => Guid.NewGuid());

            await Task.WhenAll(parallelInput
                .Select(async token =>
                {
                    var threadId = threadIds[token];

                    await DoExecuteNodeStructureAsync(
                        context.Node,
                        context.NodeScope,
                        restOfInput.Concat(new Token[] { token }).ToArray(),
                        token
                    );

                    lock (Context.Streams)
                    {
                        Context.Streams.Remove(threadId);
                    }
                })
            );

            return context.Node.OutputNode != null
                ? threadIds.Values.SelectMany(threadId => Context.GetOutputTokens(context.Node.OutputNode.Identifier, threadId))
                : new List<Token>();
        }

        public async Task DoInitializeNodeAsync(Node node, ActionContext context)
        {
            await Inspector.BeforeNodeInitializationAsync(context);

            await node.Initialize.WhenAll(context);

            await Inspector.AfterNodeInitializationAsync(context);
        }

        public async Task DoFinalizeNodeAsync(Node node, ActionContext context)
        {
            await Inspector.BeforeNodeFinalizationAsync(context);

            await node.Finalize.WhenAll(context);

            await Inspector.AfterNodeFinalizationAsync(context);
        }

        public async Task<IEnumerable<Token>> DoExecuteIterativeNodeAsync<TToken>(ActionContext context)
            where TToken : Token, new()
        {
            var restOfInput = context.InputTokens.Where(t => !(t is TToken)).ToArray();

            var iterativeInput = context.InputTokens.OfType<TToken>().ToArray();

            var threadId = Guid.NewGuid();

            foreach (var token in iterativeInput)
            {
                await DoExecuteNodeStructureAsync(
                    context.Node,
                    context.NodeScope,
                    restOfInput.Concat(new Token[] { token }),
                    token
                );

                lock (Context.Streams)
                {
                    Context.Streams.Remove(threadId);
                }
            }

            return context.Node.OutputNode != null
                ? Context.GetOutputTokens(context.Node.OutputNode.Identifier, threadId)
                : new List<Token>();
        }

        private async Task<bool> DoExecuteNodeStructureAsync(Node node, NodeScope nodeScope, IEnumerable<Token> input = null, Token selectionToken = null)
        {
            var startingNode = Context.NodesToExecute.Any()
                ? node.Nodes.Values.FirstOrDefault(n => Context.NodesToExecute.Contains(n))
                : null;

            var nodes = new List<Node>();
            if (startingNode != null)
            {
                nodes.Add(startingNode);
            }
            else
            {
                nodes.AddRange(node.InitialNodes);

                if (node.InputNode != null)
                {
                    nodes.Add(node.InputNode);
                }
            }

            await Task.WhenAll(
                nodes.Select(n => DoHandleNodeAsync(n, nodeScope, n == node.InputNode ? input : null, selectionToken))
            );

            var result =
                nodeScope.IsTerminated ||
                (
                    !Context.GetStreams(nodeScope.ThreadId).Values.Any(s => s.Tokens.Any()) &&
                    !node.Nodes.Values.Any(node => node.Type == NodeType.AcceptEventAction && !node.IncomingEdges.Any())
                );

            return result;
        }

        public async Task<bool> DoInitializeActivityAsync(InitializationRequest @event)
        {
            var result = false;

            if (
                Graph.Initializers.TryGetValue(@event.Name, out var initializer) ||
                (
                    @event.Name == EventInfo<InitializationRequest>.Name &&
                    !Graph.Initializers.Any()
                )
            )
            {
                var context = new ActivityInitializationContext(Context, NodeScope, @event);
                await Inspector.BeforeActivityInitializationAsync(context);

                try
                {
                    result = (initializer == null) || await initializer.WhenAll(context);
                }
                catch (Exception e)
                {
                    await Inspector.OnActivityInitializationExceptionAsync(context, @event, e);

                    result = false;
                }

                await Inspector.AfterActivityInitializationAsync(context);
            }

            return result;
        }

        public async Task DoHandleNodeAsync(Node node, NodeScope nodeScope, IEnumerable<Token> input = null, Token selectionToken = null)
        {
            var types = new NodeType[]
            {
                NodeType.Action,
                NodeType.Decision,
                NodeType.Fork,
                NodeType.Join,
                NodeType.Merge,
                NodeType.SendEventAction,
                NodeType.AcceptEventAction,
                NodeType.StructuredActivity,
                NodeType.ParallelActivity,
                NodeType.IterativeActivity,
                NodeType.ExceptionHandler,
                NodeType.DataStore
            };

            if (types.Contains(node.Type) && nodeScope.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            IEnumerable<Stream> streams;

            lock (node)
            {
                streams = Context.GetStreams(node, nodeScope.ThreadId);
            }

            if (
                ( // initial node case
                    node.Type == NodeType.Initial
                ) ||
                ( // input node case - has to have input
                    node.Type == NodeType.Input &&
                    (input?.Any() ?? false)
                ) ||
                ( // no implicit join node - has any incoming streams
                    streams.Any() &&
                    !node.Options.HasFlag(NodeOptions.ImplicitJoin)
                ) ||
                ( // implicit join node - has incoming streams on all edges
                    node.IncomingEdges.Count == streams.Count() &&
                    node.Options.HasFlag(NodeOptions.ImplicitJoin)
                )
            )
            {
                if (
                    node.Type == NodeType.AcceptEventAction &&
                    (
                        Context.Event == null ||
                        Context.Event.GetType() != node.EventType
                    )
                )
                {
                    Inspector.AcceptEventsPlugin.RegisterAcceptEventNode(node, nodeScope.ThreadId);

                    return;
                }

                var inputTokens = input ?? streams.SelectMany(stream => stream.Tokens).ToArray();

                lock (node.Graph)
                {
                    Debug.WriteLine($">>> Executing node {node.Name.Split('.').Last()}, threadId: {nodeScope.ThreadId}");
                }

                var actionContext = new ActionContext(Context, nodeScope, node, inputTokens, selectionToken);

                await node.Action.WhenAll(actionContext);

                if (node.Type == NodeType.AcceptEventAction)
                {
                    Context.ClearEvent();

                    if (node.IncomingEdges.Any())
                    {
                        Inspector.AcceptEventsPlugin.UnregisterAcceptEventNode(node);
                    }
                }

                if (node.Type == NodeType.Output)
                {
                    DoHandleOutput(actionContext);
                }
                else
                {
                    if (actionContext.OutputTokens.Any(t => t is ControlToken))
                    {
                        var tokenNames = actionContext.OutputTokens.Select(token => token.Name).Distinct().ToArray();
                        var edges = node.Edges
                            .Where(edge => tokenNames.Contains(edge.TokenType.GetTokenName()) || edge.Weight == 0)
                            .OrderBy(edge => edge.IsElse)
                            .ToArray();

                        foreach (var edge in edges)
                        {
                            _ = await DoHandleEdgeAsync(edge, actionContext);
                        }

                        var nodes = edges.Select(edge => edge.Target).Distinct();

                        await Task.WhenAll(nodes.Select(n => DoHandleNodeAsync(n, nodeScope)));
                    }
                }

                lock (node)
                {
                    foreach (var stream in streams)
                    {
                        if (!stream.IsPersistent)
                        {
                            Context.ClearStream(stream.EdgeIdentifier, nodeScope.ThreadId);
                        }
                    }
                }
            }
        }

        private void DoHandleOutput(ActionContext context)
            => context.Context.GetOutputTokens(context.Node.Identifier, context.NodeScope.ThreadId).AddRange(context.OutputTokens);

        private async Task<bool> DoHandleEdgeAsync(Edge edge, ActionContext context)
        {
            var edgeTokenName = edge.TokenType.GetTokenName();

            IEnumerable<Token> originalTokens = context.OutputTokens.Where(t => t.Name == edgeTokenName).ToArray();

            if (!originalTokens.Any())
            {
                return false;
            }

            var consumedTokens = new List<Token>();
            var processedTokens = new List<Token>();

            foreach (var token in originalTokens)
            {
                var processedToken = token;
                foreach (var logic in edge.TokenPipeline.Actions)
                {
                    foreach (var action in logic.Actions)
                    {
                        var pipelineContext = new TokenPipelineContext(context, edge, processedToken);
                        processedToken = await action.Invoke(pipelineContext);

                    }
                }

                if (processedToken != null)
                {
                    consumedTokens.Add(token);

                    processedTokens.Add(processedToken);
                }
            }

            if (processedTokens.Count >= edge.Weight)
            {
                var stream = Context.GetStream(edge.Identifier, context.NodeScope.ThreadId);

                if (edgeTokenName != TokenInfo<ControlToken>.TokenName)
                {
                    processedTokens.Add(new ControlToken());
                }

                if (!edge.Source.Options.HasFlag(NodeOptions.ImplicitFork) && consumedTokens.Any())
                {
                    foreach (var token in consumedTokens)
                    {
                        context.OutputTokens.Remove(token);
                    }
                }

                lock (edge.Target)
                {
                    stream.Consume(processedTokens, edge.Source.Type == NodeType.DataStore);

                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            NodeScope.Dispose();
        }

        private readonly IDictionary<Type, Activity> Activities = new Dictionary<Type, Activity>();

        public Activity GetActivity(Type activityType, RootContext context)
        {
            if (!Activities.TryGetValue(activityType, out var activity))
            {
                activity = NodeScope.ServiceProvider.GetService(activityType) as Activity;
                activity.Context = new ActivityActionContext(context, NodeScope);

                Activities.Add(activityType, activity);
            }

            return activity;
        }
    }
}
