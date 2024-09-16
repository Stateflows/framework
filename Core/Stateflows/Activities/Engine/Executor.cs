using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Utils;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Activities.Models;
using Stateflows.Activities.Streams;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;
using Stateflows.Common.Exceptions;
using Stateflows.Activities.Events;

namespace Stateflows.Activities.Engine
{
    internal class Executor : IDisposable
    {
        private static readonly object lockHandle = new object();
        private static readonly object nodesLockHandle = new object();
        private readonly Dictionary<Node, object> nodeLockHandles = new Dictionary<Node, object>();

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
            NodeType.TimeEventAction,
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

        public static readonly NodeType[] SystemTypes = new NodeType[]
        {
            NodeType.Initial,
            NodeType.Input,
            NodeType.Output
        };

        public static readonly NodeType[] interactiveNodeTypes = new NodeType[]
        {
            NodeType.AcceptEventAction,
            NodeType.TimeEventAction,
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
            Debug.Assert(Context != null, $"Context is unavailable. Is activity '{Graph.Name}' already dehydrated?");

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

        public async Task<EventStatus> ProcessAsync(EventHolder eventHolder, IEnumerable<TokenHolder> input)
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is activity '{Graph.Name}' hydrated?");

            if (Initialized)
            {
                //var context = new ActivityInitializationContext(Context, NodeScope, eventHolder, null);
                //var result = await DoInitializeActivityAsync(context);

                //if (
                //    result == InitializationStatus.InitializedImplicitly ||
                //    result == InitializationStatus.InitializedExplicitly
                //)
                //{
                //    Context.Initialized = true;

                //    input = input?.Concat(context.InputTokens).ToArray() ?? context.InputTokens.ToArray();

                    await ExecuteGraphAsync(input);

                    //if (result == InitializationStatus.InitializedExplicitly)
                    //{
                    //    return EventStatus.Initialized;
                    //}
                    //else
                    //{
                    //    if (eventHolder is EventHolder<Initialize>)
                    //    {
                    //        return EventStatus.Initialized;
                    //    }
                    //    else
                    //    {
                    //        return EventStatus.Consumed;
                    //    }
                    //}
                //}
                //else
                //{
                //    return result == InitializationStatus.NoSuitableInitializer
                //        ? EventStatus.Rejected
                //        : EventStatus.NotInitialized;
                //}
            }

            return EventStatus.NotInitialized;
        }

        public async Task<EventStatus> InitializeAsync<TEvent>(EventHolder<TEvent> eventHolder, IEnumerable<TokenHolder> input = null)
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is activity '{Graph.Name}' hydrated?");

            if (!Initialized)
            {
                var context = new ActivityInitializationContext<TEvent>(Context, NodeScope, eventHolder, null);
                var result = await DoInitializeActivityAsync(context);

                if (
                    result == InitializationStatus.InitializedImplicitly ||
                    result == InitializationStatus.InitializedExplicitly
                )
                {
                    Context.Initialized = true;

                    input = input?.Concat(context.InputTokens).ToArray() ?? context.InputTokens.ToArray();

                    await ExecuteGraphAsync(input);

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

        public async Task<bool> CancelAsync()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is activity '{Graph.Name}' hydrated?");

            if (Initialized)
            {
                Context.ActiveNodes.Clear();
                Context.NodeTimeEvents.Clear();
                Context.NodeThreads.Clear();
                Context.OutputTokens.Clear();
                Context.GlobalValues.Clear();
                Context.Streams.Clear();
                Context.Context.PendingTimeEvents.Clear();
                Context.Context.PendingStartupEvents.Clear();
                Context.Context.TriggerTime = null;

                await DoFinalizeAsync();

                return true;
            }

            return false;
        }

        public void Reset(ResetMode resetMode)
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is activity '{Graph.Name}' hydrated?");

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

        private IEnumerable<object> OutputTokens { get; set; } = null;

        public async Task<IEnumerable<object>> GetResultAsync()
        {
            await FinalizationEvent.WaitOneAsync();

            return OutputTokens;
        }

        public async Task<EventStatus> ProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            var result = EventStatus.Rejected;

            if (Initialized)
            {
                result = await DoProcessAsync(eventHolder);
            }

            // disposal

            return result;
        }

        private async Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            Debug.Assert(Context != null, $"Context is not available. Is activity '{Graph.Name}' hydrated?");

            var activeNodes = GetActiveNodes();
            var activeNode = activeNodes.FirstOrDefault(node =>
                node.EventType == eventHolder.PayloadType &&
                (
                    !(eventHolder.Payload is TimeEvent timeEvent) ||
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

            var input = eventHolder.Payload is TokensTransferEvent tokensTransferEvent
                ? tokensTransferEvent.Tokens
                : null;

            (var result, var output) = await ExecuteGraphAsync(input);

            if (eventHolder.Payload is IRequest<TokensOutput> inputTokens)
            {
                inputTokens.Respond(new TokensOutput() { Tokens = output.ToList() });
            }

            return result;
        }

        private async Task<(EventStatus, IEnumerable<TokenHolder>)> ExecuteGraphAsync(IEnumerable<TokenHolder> input = null)
        {
            (var output, var finalized) = await DoExecuteStructuredNodeAsync(Graph, NodeScope, input);

            if (finalized)
            {
                await DoFinalizeAsync(output);
            }

            return (EventStatus.Consumed, output);
        }

        public async Task DoFinalizeAsync(IEnumerable<object> outputTokens = null)
        {
            if (Finalized) return;

            Context.Finalized = true;

            if (Graph.OutputNode != null && outputTokens != null)
            {
                OutputTokens = outputTokens;
            }

            var context = new ActivityActionContext(Context, NodeScope);

            await Inspector.BeforeActivityFinalizationAsync(context);

            await Graph.Finalize.WhenAll(context);
            try
            {
                await Graph.Finalize.WhenAll(context);
            }
            catch (Exception e)
            {
                if (!await Inspector.OnActivityFinalizationExceptionAsync(context, e))
                {
                    throw;
                }
                else
                {
                    throw new ExecutionException(e);
                }
            }

            await Inspector.AfterActivityFinalizationAsync(context);

            FinalizationEvent.Set();
        }

        public async Task<(IEnumerable<TokenHolder> Output, bool Finalized)> DoExecuteStructuredNodeAsync(Node node, NodeScope nodeScope, IEnumerable<TokenHolder> input = null)
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
                : new List<TokenHolder>();

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

        public async Task<IEnumerable<TokenHolder>> DoExecuteParallelNodeAsync<TToken>(Node node, NodeScope nodeScope, IEnumerable<TokenHolder> input = null)
        {
            var typedInput = input.OfType<TokenHolder<TToken>>().ToArray();

            var restOfInput = input.Except(typedInput).ToArray();

            var inputPartitions = typedInput.Partition(node.ChunkSize).ToArray();

            var outputTokens = new List<TokenHolder>();

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
                : new List<TokenHolder>();
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

        public async Task<IEnumerable<TokenHolder>> DoExecuteIterativeNodeAsync<TToken>(ActionContext context)
        {
            var typedInput = context.InputTokens.OfType<TokenHolder<TToken>>().ToArray();

            var restOfInput = context.InputTokens.Except(typedInput).ToArray();

            var inputPartitions = typedInput.Partition(context.Node.ChunkSize).ToArray();

            var threadId = Guid.NewGuid();

            var outputTokens = new List<TokenHolder>();

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
                : new List<TokenHolder>();
        }

        private async Task<bool> DoExecuteStructureAsync(Node node, NodeScope nodeScope, IEnumerable<TokenHolder> input = null, IEnumerable<TokenHolder> selectionTokens = null)
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
                nodes.Select(n => DoHandleNodeAsync(n, null, nodeScope, n == node.InputNode ? input : null, selectionTokens)).ToArray()
            );

            var result = Context.IsNodeCompleted(node, nodeScope);

            return result;
        }

        public async Task<InitializationStatus> DoInitializeActivityAsync<TInitializationEvent>(ActivityInitializationContext<TInitializationEvent> context)
        {
            InitializationStatus result;

            var initializer = Graph.Initializers.ContainsKey(context.InitializationEvent.GetType().GetEventName())
                ? Graph.Initializers[context.InitializationEvent.GetType().GetEventName()]
                : Graph.DefaultInitializer;

            await Inspector.BeforeActivityInitializationAsync(context);

            if (initializer != null)
            {
                try
                {
                    result = await initializer.WhenAll(context)
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
                        if (!await Inspector.OnActivityInitializationExceptionAsync(context, context.InitializationEventHolder, e))
                        {
                            throw;
                        }
                        else
                        {
                            throw new ExecutionException(e);
                        }
                    }

                    result = InitializationStatus.NotInitialized;
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

            await Inspector.AfterActivityInitializationAsync(context, Initialized);

            return result;
        }

        public async Task<bool> HandleExceptionAsync(Node node, Exception exception, BaseContext context)
        {
            Node handler = null;
            var currentNode = node;
            while (currentNode != null)
            {
                var handlers = currentNode.ExceptionHandlers.Where(n => n.ExceptionType.IsAssignableFrom(exception.GetType()));
                handler = handlers.Any()
                    ? handlers.FirstOrDefault(n => n.ExceptionType == exception.GetType()) ?? handlers.First()
                    : null;

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
                var exceptionName = exception.GetType().Name;
                Trace.WriteLine($"⦗→s⦘ Activity '{node.Graph.Name}:{context.Activity.Id.Instance}': handling '{exceptionName}'");

                var exceptionContext = new ActionContext(
                    context.Context,
                    currentScope,
                    handler,
                    new TokenHolder[]
                    {
                        exception.ToExceptionHolder(exception.GetType()),
                        new NodeReferenceToken() { Node = node }.ToTokenHolder(),
                    }
                );

                await handler.Action.WhenAll(exceptionContext);

                DoHandleOutput(exceptionContext);

                ReportExceptionHandled(node, exceptionName, exceptionContext.OutputTokens.Where(t => t is TokenHolder<ControlToken>).ToArray(), Context);

                return true;
            }

            return false;
        }

        public async Task DoHandleNodeAsync(Node node, Edge upstreamEdge, NodeScope nodeScope, IEnumerable<TokenHolder> input = null, IEnumerable<TokenHolder> selectionTokens = null)
        {
            if (CancellableTypes.Contains(node.Type) && nodeScope.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            var activated = false;

            IEnumerable<TokenHolder> inputTokens = Array.Empty<TokenHolder>();

            lock (GetLock(node))
            {
                var streams = !Context.NodesToExecute.Contains(node)
                    ? Context.GetActivatedStreams(node, nodeScope.ThreadId)
                    : Array.Empty<Stream>();

                activated =
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
                    );

                inputTokens = input ?? streams.SelectMany(stream => stream.Tokens).Distinct().ToArray();

                if (streams.Any())
                {
                    if (activated)
                    {
                        foreach (var stream in streams)
                        {
                            if (!stream.IsPersistent)
                            {
                                Context.ClearStream(stream.EdgeIdentifier, nodeScope.ThreadId);
                            }
                        }
                    }
                    else
                    {
                        ReportNodeAttemptedExecution(node, upstreamEdge, streams, Context);
                    }
                }
            }

            nodeScope = nodeScope.CreateChildScope(node);

            var actionContext = new ActionContext(Context, nodeScope, node, inputTokens, selectionTokens);

            await Inspector.BeforeNodeActivateAsync(actionContext, activated);

            if (activated)
            {
                if (
                    interactiveNodeTypes.Contains(node.Type) &&
                    (
                        Context.EventHolder == null ||
                        Context.EventHolder.GetType() != node.EventType
                    )
                )
                {
                    Inspector.AcceptEventsPlugin.RegisterAcceptEventNode(node, nodeScope.ThreadId);

                    if (Debugger.IsAttached)
                    {
                        Trace.WriteLine($"⦗→s⦘ Activity '{node.Graph.Name}:{Context.Id.Instance}': AcceptEvent node '{node.Name}' registered and waiting for event");
                    }

                    return;
                }

                ReportNodeExecuting(node, upstreamEdge, inputTokens, Context);

                await node.Action.WhenAll(actionContext);

                if (interactiveNodeTypes.Contains(node.Type))
                {
                    var @event = Context.EventHolder;

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
                    if (
                        StructuralTypes.Contains(node.Type) &&
                        Context.IsNodeCompleted(node, nodeScope) &&
                        !actionContext.OutputTokens.Any()
                    )
                    {
                        actionContext.OutputTokens.Add(new ControlToken().ToTokenHolder());
                    }

                    TokenHolder[] outputTokens;
                    lock (actionContext.OutputTokens)
                    {
                        outputTokens = actionContext.OutputTokens.ToArray();
                    }

                    ReportNodeExecuted(node, outputTokens.Where(t => t is TokenHolder<ControlToken>).ToArray(), Context);

                    if (outputTokens.Any(t => t is TokenHolder<ControlToken>))
                    {
                        var tokenNames = outputTokens.Select(token => token.Name).Distinct().ToArray();

                        var nodes = (
                            await Task.WhenAll(
                                node.Edges
                                    .Where(edge => tokenNames.Contains(edge.TokenType.GetTokenName()) || edge.Weight == 0)
                                    .OrderBy(edge => edge.IsElse)
                                    .Select(async edge => (
                                        Edge: edge,
                                        Activated: await DoHandleEdgeAsync(edge, actionContext))
                                    )
                            )
                        )
                            .Where(x => x.Activated)
                            .Select(x => x.Edge)
                            .GroupBy(edge => edge.Target)
                            .ToArray();

                        await Task.WhenAll(nodes.Select(n => DoHandleNodeAsync(n.Key, n.First(), nodeScope)).ToArray());
                    }
                }
            }

            await Inspector.AfterNodeActivateAsync(null);
        }

        private static void ReportNodeExecuting(Node node, Edge upstreamEdge, IEnumerable<TokenHolder> inputTokens, RootContext context)
        {
            if (Debugger.IsAttached && !SystemTypes.Contains(node.Type))
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
            if (Debugger.IsAttached && !SystemTypes.Contains(node.Type))
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
            if (Debugger.IsAttached)
            {
                lock (lockHandle)
                {
                    Trace.WriteLine($"⦗→s⦘ Activity '{node.Graph.Name}:{context.Id.Instance}': '{exceptionName}' handled{(!outputTokens.Any() ? ", no output" : "")}");
                    ReportTokens(outputTokens, false);
                }
            }
        }

        private static void ReportNodeAttemptedExecution(Node node, Edge upstreamEdge, IEnumerable<Stream> streams, RootContext context)
        {
            if (Debugger.IsAttached && !SystemTypes.Contains(node.Type))
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
                Trace.WriteLine($"    {(input ? "Input" : "Output")} (with count):");
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

        private void DoHandleOutput(ActionContext context)
            => context.Context.GetOutputTokens(context.Node.Identifier, context.NodeScope.ThreadId).AddRange(context.OutputTokens);

        private async Task<bool> DoHandleEdgeAsync(Edge edge, ActionContext context)
        {
            var edgeTokenName = edge.TokenType.GetTokenName();

            IEnumerable<TokenHolder> originalTokens = context.OutputTokens.Where(t => t.Name == edgeTokenName).ToArray();

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

                if (edgeTokenName != typeof(ControlToken).GetTokenName())
                {
                    processedTokens.Add(new ControlToken().ToTokenHolder());
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

        public IActivity GetActivity(Type activityType)
        {
            var activity = NodeScope.ServiceProvider.GetService(activityType) as IActivity;

            return activity;
        }
    }
}
