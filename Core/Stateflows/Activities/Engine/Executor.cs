using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
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
using Stateflows.Activities.Inspection.Classes;
using Stateflows.StateMachines.Extensions;

namespace Stateflows.Activities.Engine
{
    internal class Executor : IDisposable, IStateflowsExecutor
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

        private static readonly NodeType[] CancellableTypes = new NodeType[]
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

        private static readonly NodeType[] SystemTypes = new NodeType[]
        {
            NodeType.Initial,
            NodeType.Input,
            NodeType.Output
        };

        private static readonly NodeType[] InteractiveNodeTypes = new NodeType[]
        {
            NodeType.AcceptEventAction,
            NodeType.TimeEventAction,
        };

        public Executor(ActivitiesRegister register, Graph graph, IServiceProvider serviceProvider)
        {
            Register = register;
            NodeScope = new NodeScope(serviceProvider, graph, null, Guid.NewGuid());
            Graph = graph;

            var logger = serviceProvider.GetService<ILogger<Executor>>();
            Inspector = new Inspector(this, logger);
            ActivitiesContextHolder.Inspection.Value = new ActivityInspection(this, Inspector);
        }

        public RootContext Context { get; private set; }

        public readonly Inspector Inspector;
        
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

        public Type[] GetExpectedEvents()
            => GetActiveNodes()
                .SelectMany(node => node.ActualEventTypes)
                .Distinct()
                .ToArray();

        public IEnumerable<string> GetExpectedEventNames()
            => GetExpectedEvents()
                .Where(type => !typeof(SystemEvent).IsAssignableFrom(type))
                .Where(type => !typeof(Exception).IsAssignableFrom(type))
                .Select(type => type.GetEventName())
                .ToArray();

        public async Task HydrateAsync(RootContext context)
        {
            Context = context;
            Context.Executor = this;

            await Inspector.BuildAsync();
            
            Inspector.AfterHydrate(new ActivityActionContext(Context, NodeScope.CreateChildScope()));
        }

        public Task<RootContext> DehydrateAsync()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is activity '{Graph.Name}' already dehydrated?");

            Inspector.BeforeDehydrate(new ActivityActionContext(Context, NodeScope.CreateChildScope()));

            NodeScope.Dispose();

            var result = Context;
            result.Executor = null;
            Context = null;

            return Task.FromResult(result);
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

        public async Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            Debug.Assert(Context != null, $"Context is not available. Is activity '{Graph.Name}' hydrated?");

            Node activeNode = null;

            if (eventHolder.Payload is TokensInputEvent)
            {
                activeNode = Graph.InputNode;
            }

            if (activeNode == null)
            {
                var activeNodes = GetActiveNodes();
                activeNode = activeNodes.FirstOrDefault(node =>
                    eventHolder.IsAcceptedBy(node, Context.NodeTimeEvents)
                    // node.ActualEventTypes.Contains(eventHolder.PayloadType) &&
                    // (
                    //     !(eventHolder.Payload is TimeEvent timeEvent) ||
                    //     (
                    //         Context.NodeTimeEvents.TryGetValue(node.Identifier, out var timeEventId) &&
                    //         timeEvent.Id == timeEventId
                    //     )
                    // )
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

            (var result, var output) = await ExecuteGraphAsync(input);

            var tokensOutput = new TokensOutput() { Tokens = output.ToList() };
            if (eventHolder.Payload is IRequest<TokensOutput> inputTokens)
            {
                inputTokens.Respond(tokensOutput);
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

            output = output.Where(h => h.PayloadType != typeof(ControlToken)).ToList();
            
            Context.ActivityOutputTokens.AddRange(output);

            if (output.Any())
            {
                var tokensOutput = new TokensOutput() { Tokens = output.ToList() };
                var tokensOutputHolder = tokensOutput.ToEventHolder(Context.Id);

                var tokenTypes = output.Select(t => t.PayloadType).Distinct();
                var tokensOutputType = typeof(TokensOutput<>);
                var tokensOutputs = tokenTypes
                    .Select(type =>
                    {
                        var tokensOutputGenericType = tokensOutputType.MakeGenericType(type);
                        var tokensOutput = (TokensTransferEvent)Activator.CreateInstance(tokensOutputGenericType);
                        tokensOutput.Tokens = output.Where(t => t.PayloadType == type).ToList();

                        return tokensOutput.ToTypedEventHolder(Context.Id);
                    })
                    .ToList();

                tokensOutputs.Add(tokensOutputHolder);

                var notificationsHub = NodeScope.ServiceProvider.GetRequiredService<INotificationsHub>();

                await notificationsHub.PublishRangeAsync(tokensOutputs);
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

            Inspector.BeforeActivityFinalization(context);

            await Graph.Finalize.WhenAll(context);
            try
            {
                await Graph.Finalize.WhenAll(context);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"⦗→s⦘ Activity '{Context.Id.Name}:{Context.Id.Instance}': exception thrown '{e.Message}'");
                if (!Inspector.OnActivityFinalizationException(context, e))
                {
                    throw;
                }
                else
                {
                    throw new BehaviorExecutionException(e);
                }
            }

            Inspector.AfterActivityFinalization(context);

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

        public async Task<IEnumerable<TokenHolder>> DoExecuteParallelNodeAsync<TToken>(Node node, Edge edge, NodeScope nodeScope, IEnumerable<TokenHolder> input = null)
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
                            nodeScope.CreateChildScope(node, edge, threadId),
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
            Inspector.BeforeNodeInitialization(context);

            await node.Initialize.WhenAll(context);

            Inspector.AfterNodeInitialization(context);
        }

        public async Task DoFinalizeNodeAsync(Node node, ActionContext context)
        {
            Inspector.BeforeNodeFinalization(context);

            await node.Finalize.WhenAll(context);

            Inspector.AfterNodeFinalization(context);
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
                    context.NodeScope.CreateChildScope(context.Node, context.Edge, threadId),
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
                nodes.Select(n => DoHandleNodeAsync(n, nodeScope.Edge, nodeScope, n == node.InputNode ? input : null, selectionTokens)).ToArray()
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

            Inspector.BeforeActivityInitialization(context);

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
                        Trace.WriteLine($"⦗→s⦘ Activity '{Context.Id.Name}:{Context.Id.Instance}': exception thrown '{e.Message}'");
                        if (!Inspector.OnActivityInitializationException(context, context.InitializationEventHolder, e))
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

            Inspector.AfterActivityInitialization(context, Initialized);

            return result;
        }

        public async Task<ExceptionHandlingResult> HandleExceptionAsync(Node node, Exception exception, ActionContext context)
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

                if (node.ExceptionHandlers.Contains(handler))
                {
                    context.OutputTokens.AddRange(exceptionContext.OutputTokens);
                }
                else
                {
                    DoHandleOutput(exceptionContext);
                }

                ReportExceptionHandled(node, exceptionName, exceptionContext.OutputTokens.Where(t => t is TokenHolder<ControlToken>).ToArray(), Context);

                return node.ExceptionHandlers.Contains(handler)
                    ? ExceptionHandlingResult.HandledDirectly
                    : ExceptionHandlingResult.HandledIndirectly;
            }

            return ExceptionHandlingResult.NotHandled;
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
                    ? Context.GetNodeStreams(node, nodeScope.ThreadId, node.Type != NodeType.Output)
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

            nodeScope = nodeScope.CreateChildScope(node, upstreamEdge);

            var actionContext = new ActionContext(Context, nodeScope, node, inputTokens, selectionTokens);

            Inspector.BeforeNodeActivate(actionContext, activated);

            if (activated)
            {
                if (
                    InteractiveNodeTypes.Contains(node.Type) &&
                    (
                        Context.EventHolder == null ||
                        !Context.EventHolder.IsAcceptedBy(node)
                        // !node.ActualEventTypes.Contains(Context.EventHolder.PayloadType)
                    )
                )
                {
                    Inspector.AcceptEventsPlugin.RegisterAcceptEventNode(node, nodeScope.ThreadId);

                    if (Debugger.IsAttached)
                    {
                        Trace.WriteLine($"⦗→s⦘ Activity '{node.Graph.Name}:{Context.Id.Instance}': AcceptEvent node '{node.Name}' registered and waiting for event");
                    }
                    
                    Inspector.AfterNodeActivate(actionContext);

                    return;
                }

                ReportNodeExecuting(node, upstreamEdge, inputTokens, Context);

                await node.Action.WhenAll(actionContext);

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

                    var tokenNames = outputTokens.Select(token => token.Name).Distinct().ToArray();

                    var nodes = (
                        await Task.WhenAll(
                            node.Edges
                                .Where(edge => edge.Target.Type == NodeType.Output || outputTokens.Any(t => t is TokenHolder<ControlToken>))
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
                        .Reverse()
                        .GroupBy(edge => edge.Target)
                        .ToArray();

                    await Task.WhenAll(nodes.Select(n => DoHandleNodeAsync(n.Key, n.First(), nodeScope)).ToArray());
                }
            }

            Inspector.AfterNodeActivate(actionContext);
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

        internal void DoHandleOutput(ActionContext context)
            => context.Context.GetOutputTokens(context.Node.Identifier, context.NodeScope.ThreadId).AddRange(context.OutputTokens);

        private async Task<bool> DoHandleEdgeAsync(Edge edge, ActionContext context)
        {
            var flowContext = new FlowContext(context.Context, context.NodeScope, edge);
            
            var edgeTokenName = edge.TokenType.GetTokenName();

            IEnumerable<TokenHolder> originalTokens = context.OutputTokens.Where(t => t.Name == edgeTokenName).ToArray();

            flowContext.TokenCount = originalTokens.Count();
            
            Inspector.BeforeFlowActivate(flowContext);

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
                    }
                }

                if (processedToken != null)
                {
                    consumedTokens.Add(token);

                    processedTokens.Add(processedToken);
                }
            }

            flowContext.TargetTokenCount = processedTokens.Count;
            
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
                }
                
                Inspector.AfterFlowActivate(flowContext, true);
                
                return true;
            }
            
            flowContext.TargetTokenCount = processedTokens.Count;
            
            Inspector.AfterFlowActivate(flowContext, false);

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

        public async Task<IActivity> GetActivity(Type activityType)
            => await StateflowsActivator.CreateInstanceAsync(NodeScope.ServiceProvider, activityType, "activity") as IActivity;
    }
}
