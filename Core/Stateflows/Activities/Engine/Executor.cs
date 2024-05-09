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
using Stateflows.Utils;

namespace Stateflows.Activities.Engine
{
    internal class Executor : IDisposable
    {
        public readonly Graph Graph;
        public readonly ActivitiesRegister Register;
        public readonly NodeScope NodeScope;
        private readonly ILogger<Executor> Logger;

        public readonly NodeType[] CancellableTypes = new NodeType[]
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

        public readonly NodeType[] StructuralTypes = new NodeType[]
        {
            NodeType.StructuredActivity,
            NodeType.ParallelActivity,
            NodeType.IterativeActivity
        };

        public Executor(ActivitiesRegister register, Graph graph, IServiceProvider serviceProvider)
        {
            Register = register;
            NodeScope = new NodeScope(serviceProvider, graph, Guid.NewGuid());
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

        public IEnumerable<Type> GetExpectedEvents()
            => GetActiveNodes()
                .Select(node => node.EventType)
                .Distinct()
                .ToArray();

        public Task HydrateAsync(RootContext context)
        {
            Context = context;
            Context.Executor = this;

            return Inspector.AfterHydrateAsync(new ActivityActionContext(Context, NodeScope.CreateChildScope()));
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

                await ExecuteGraphAsync();

                return true;
            }

            Logger.LogInformation("Behavior \"{id}\" is already initialized.", $"{Context.Id.BehaviorId.Type}:{Context.Id.Name}:{Context.Id.Instance}");

            return false;
        }

        public async Task<bool> CancelAsync()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' hydrated?");

            if (Initialized)
            {
                Context.ActiveNodes.Clear();
                Context.NodeTimeEvents.Clear();
                Context.NodeThreads.Clear();
                Context.OutputTokens.Clear();
                Context.GlobalValues.Clear();
                Context.Streams.Clear();
                Context.Context.PendingTimeEvents.Clear();
                Context.Context.TriggerTime = null;

                await DoFinalizeAsync();

                return true;
            }

            return false;
        }

        public void Reset(bool keepVersion)
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' hydrated?");

            if (Initialized)
            {
                Context.Context.Values.Clear();
                if (!keepVersion)
                {
                    Context.Context.Version = 0;
                }
            }
        }

        private IEnumerable<object> OutputTokens { get; set; } = null;

        public async Task<IEnumerable<object>> GetResultAsync()
        {
            await FinalizationEvent.WaitOneAsync();

            return OutputTokens;
        }

        public async Task<EventStatus> ProcessAsync<TEvent>(TEvent @event)
            //where TEvent : Event, new()
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
            //where TEvent : Event, new()
        {
            Debug.Assert(Context != null, $"Context is not available. Is activity '{Graph.Name}' hydrated?");

            var activeNodes = GetActiveNodes();
            var activeNode = activeNodes.FirstOrDefault(node =>
                node.EventType == @event.GetType() &&
                (
                    !(@event is TimeEvent timeEvent) ||
                    (
                        Context.NodeTimeEvents.TryGetValue(node.Identifier, out var timeEventId) &&
                        timeEvent.Id == timeEventId
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

        //public async Task DoFinalizeAsync(IEnumerable<Token> outputTokens = null)
        public async Task DoFinalizeAsync(IEnumerable<object> outputTokens = null)
        {
            if (Finalized) return;

            Finalized = true;

            if (Graph.OutputNode != null && outputTokens != null)
            {
                OutputTokens = outputTokens;
            }

            var context = new ActivityActionContext(Context, NodeScope);

            await Inspector.BeforeActivityFinalizationAsync(context);

            await Graph.Finalize.WhenAll(context);

            await Inspector.AfterActivityFinalizationAsync(context);

            FinalizationEvent.Set();
        }

        //public async Task<(IEnumerable<Token> Output, bool Finalized)> DoExecuteStructuredNodeAsync(Node node, NodeScope nodeScope, IEnumerable<Token> input = null)
        public async Task<(IEnumerable<Token> Output, bool Finalized)> DoExecuteStructuredNodeAsync(Node node, NodeScope nodeScope, IEnumerable<Token> input = null)
        {
            if (node.Anchored)
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
                input
            );

            var output = node.OutputNode != null
                ? Context.GetOutputTokens(node.OutputNode.Identifier, nodeScope.ThreadId)
                : new List<Token>();

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
                Context.NodeThreads.Remove(node.Identifier);
            }

            return result;
        }

        public async Task<IEnumerable<Token>> DoExecuteParallelNodeAsync<TToken>(Node node, NodeScope nodeScope, IEnumerable<Token> input = null)
        {
            var typedInput = input.OfType<Token<TToken>>().ToArray();

            var restOfInput = input.Except(typedInput).ToArray();

            var inputPartitions = typedInput.Partition(node.ChunkSize).ToArray();

            var outputTokens = new List<Token>();

            await Task.WhenAll(inputPartitions
                .Select(inputPartition =>
                    Task.Run(async () =>
                    {
                        var threadId = Guid.NewGuid();

                        await DoExecuteStructureAsync(
                            node,
                            nodeScope.CreateChildScope(node, threadId),
                            restOfInput.Concat(inputPartition).ToArray(),
                            inputPartition
                        );

                        lock (Context.Streams)
                        {
                            if (
                                node.OutputNode != null &&
                                Context.OutputTokens.TryGetValue(threadId, out var nodesOutputTokens) &&
                                nodesOutputTokens.TryGetValue(node.OutputNode.Identifier, out var tokens)
                            )
                            {
                                outputTokens.AddRange(Context.GetOutputTokens(node.OutputNode.Identifier, threadId));
                            }

                            Context.Streams.Remove(threadId);
                            Context.OutputTokens.Remove(threadId);
                        }
                    })
                ).ToArray()
            );

            return node.OutputNode != null
                ? outputTokens
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
        {
            var typedInput = context.InputTokens.OfType<Token<TToken>>().ToArray();

            var restOfInput = context.InputTokens.Except(typedInput).ToArray();

            var inputPartitions = typedInput.Partition(context.Node.ChunkSize).ToArray();

            var threadId = Guid.NewGuid();

            var outputTokens = new List<Token>();

            foreach (var inputPartition in inputPartitions)
            {
                await DoExecuteStructureAsync(
                    context.Node,
                    context.NodeScope.CreateChildScope(context.Node, threadId),
                    restOfInput.Concat(inputPartition),
                    inputPartition
                );

                if (
                    context.Node.OutputNode != null &&
                    Context.OutputTokens.TryGetValue(threadId, out var nodesOutputTokens) &&
                    nodesOutputTokens.TryGetValue(context.Node.OutputNode.Identifier, out var iterationOutputTokens)
                )
                {
                    outputTokens.AddRange(Context.GetOutputTokens(context.Node.OutputNode.Identifier, threadId));
                }

                lock (Context.Streams)
                {
                    Context.Streams.Remove(threadId);
                    Context.OutputTokens.Remove(threadId);
                }
            }

            return context.Node.OutputNode != null
                ? outputTokens
                : new List<Token>();
        }

        private async Task<bool> DoExecuteStructureAsync(Node node, NodeScope nodeScope, IEnumerable<Token> input = null, IEnumerable<Token> selectionTokens = null)
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
                nodes.Select(n => DoHandleNodeAsync(n, nodeScope, n == node.InputNode ? input : null, selectionTokens)).ToArray()
            );

            var result = Context.IsNodeCompleted(node, nodeScope);

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

        public async Task<IEnumerable<object>> HandleExceptionAsync(Node node, Exception exception, BaseContext context)
        {
            Node handler = null;
            var currentNode = node;
            while (currentNode != null)
            {
                handler = currentNode.ExceptionHandlers.FirstOrDefault(n => exception.GetType().IsAssignableFrom(n.ExceptionType));

                if (handler != null)
                {
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

            if (handler != null)
            {
                var exceptionContext = new ActionContext(
                    context.Context,
                    currentScope,
                    handler,
                    new Token[] { new ExceptionToken<Exception>() { Exception = exception } }
                );

                await handler.Action.WhenAll(exceptionContext);

                DoHandleOutput(exceptionContext);

                return exceptionContext.OutputTokens;
            }

            return new Token[0];
        }

        public async Task DoHandleNodeAsync(Node node, NodeScope nodeScope, IEnumerable<Token> input = null, IEnumerable<Token> selectionTokens = null)
        {
            if (CancellableTypes.Contains(node.Type) && nodeScope.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            IEnumerable<Stream> streams = Array.Empty<Stream>();

            if (!Context.NodesToExecute.Contains(node))
            {
                lock (node)
                {
                    streams = Context.GetStreams(node, nodeScope.ThreadId);
                }
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
                ) ||
                (
                    Context.NodesToExecute.Contains(node)
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

                nodeScope = nodeScope.CreateChildScope(node);
                lock (node.Graph)
                {
                    Debug.WriteLine($">>> Executing node {node.Name}, threadId: {nodeScope.ThreadId}");
                }

                var actionContext = new ActionContext(Context, nodeScope, node, inputTokens, selectionTokens);

                await node.Action.WhenAll(actionContext);

                if (node.Type == NodeType.AcceptEventAction)
                {
                    var @event = Context.Event;

                    Context.ClearEvent();

                    if (@event is TimeEvent)
                    {
                        Inspector.AcceptEventsPlugin.UnregisterAcceptEventNode(node);

                        if (@event is RecurringEvent && !node.IncomingEdges.Any() && Context.NodesToExecute.Contains(node))
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

                if (node.Type == NodeType.Output || node.Type == NodeType.ExceptionHandler)
                {
                    DoHandleOutput(actionContext);
                }
                else
                {
                    List<Token> outputTokens;
                    lock (actionContext.OutputTokens)
                    {
                        outputTokens = actionContext.OutputTokens.ToList();
                    }

                    if (
                        StructuralTypes.Contains(node.Type) &&
                        (
                            Context.IsNodeCompleted(node, nodeScope) ||
                            outputTokens.Any()
                        )
                    )
                    {
                        outputTokens.Add(new Control().ToToken());
                    }

                    if (outputTokens.Any(t => t is Token<Control>))
                    {
                        var tokenNames = outputTokens.Select(token => token.Name).Distinct().ToArray();
                        var edges = node.Edges
                            .Where(edge => tokenNames.Contains(edge.TokenType.GetTokenName()) || edge.Weight == 0)
                            .OrderBy(edge => edge.IsElse)
                            .ToArray();

                        foreach (var edge in edges)
                        {
                            _ = await DoHandleEdgeAsync(edge, actionContext);
                        }

                        var nodes = edges.Select(edge => edge.Target).Distinct().ToArray();

                        await Task.WhenAll(nodes.Select(n => DoHandleNodeAsync(n, nodeScope)).ToArray());
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

                if (edgeTokenName != typeof(Control).GetTokenName())
                {
                    processedTokens.Add(new Control().ToToken());
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
