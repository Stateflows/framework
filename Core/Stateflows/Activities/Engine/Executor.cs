using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
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

        private CancellationTokenSource cancellationTokenSource;
        private CancellationTokenSource CancellationTokenSource => cancellationTokenSource ??= new CancellationTokenSource();

        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        public Executor(ActivitiesRegister register, Graph graph, IServiceProvider serviceProvider)
        {
            Register = register;
            NodeScope = new NodeScope(serviceProvider);
            Graph = graph;
        }

        public RootContext Context { get; private set; }

        private Inspector observer;

        public Inspector Observer => observer ??= new Inspector(this);

        private HashSet<Task> Tasks { get; set; } = new HashSet<Task>();

        private EventWaitHandle FinalizationEvent { get; } = new EventWaitHandle(false, EventResetMode.AutoReset);

        private bool Finalized { get; set; } = false;

        private void RegisterTask(Task task, string threadId)
        {
            lock (Tasks)
            {
                Tasks.Add(task);
            }

            _ = Task.Run(async () =>
            {
                await Task.WhenAll(task);

                lock (Tasks)
                {
                    Tasks.Remove(task);

                    if (!Tasks.Any())
                    {
                        DoFinalize(threadId);
                    }
                }
            });
        }

        public async Task<bool> HydrateAsync(RootContext context)
        {
            Context = context;
            Context.Executor = this;

            await Observer.AfterHydrateAsync(new ActivityActionContext(Context, NodeScope.CreateChildScope()));

            return true;
        }

        public async Task<RootContext> DehydrateAsync()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' already dehydrated?");

            await Observer.BeforeDehydrateAsync(new ActivityActionContext(Context, NodeScope.CreateChildScope()));

            var result = Context;
            result.Executor = null;
            Context = null;

            return result;
        }

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

                _ = DoInitializeStructuredNodeAsync(Graph);

                Debug.WriteLine($"{Context.Id.Instance} initialization");

                return true;
            }

            Debug.WriteLine($"{Context.Id.Instance} initialized already");

            return false;
        }

        public bool Cancel()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is activity '{Graph.Name}' hydrated?");

            if (Initialized)
            {
                CancellationTokenSource.Cancel();

                Debug.WriteLine("Cancelled");

                return true;
            }

            return false;
        }

        public async Task ExitAsync()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is activity '{Graph.Name}' hydrated?");

            if (Initialized)
            {
                //var currentStack = await GetNodesStackAsync(false);

                //foreach (var vertex in currentStack)
                //{
                //    await DoExitAsync(vertex);
                //}

                //Context.NodesStack.Clear();
            }
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

            NodeScope.Dispose();

            //ServiceProvider = null;

            return result;
        }

        private async Task<EventStatus> DoProcessAsync<TEvent>(TEvent @event)
            where TEvent : Event
        {
            Debug.Assert(Context != null, $"Context is not available. Is activity '{Graph.Name}' hydrated?");

            //var currentStack = await GetVerticesStackAsync(true);
            //currentStack.Reverse();

            //if (!await TryDeferEventAsync(@event))
            //{
            //    foreach (var vertex in currentStack)
            //    {
            //        foreach (var edge in vertex.Edges)
            //        {
            //            if (edge.Trigger == @event.Name)
            //            {
            //                if (await DoGuardAsync(edge, @event))
            //                {
            //                    await DoConsumeAsync<TEvent>(vertex, edge);

            //                    return true;
            //                }
            //            }
            //        }
            //    }
            //}

            //await DispatchNextDeferredEvent();

            return EventStatus.NotConsumed;
        }

        public void DoFinalize(string threadId)
        {
            lock (Context)
            {
                if (Finalized) return;

                Finalized = true;

                if (Graph.OutputNode != null)
                {
                    OutputTokens = Context.GetOutputTokens(Graph.OutputNode.Identifier, threadId);
                }

                FinalizationEvent.Set();
            }
        }

        public async Task<IEnumerable<Token>> DoInitializeStructuredNodeAsync(Node node, IEnumerable<Token> input = null)
        {
            var threadId = Guid.NewGuid().ToString();

            await DoExecuteNodeStructureAsync(
                node,
                threadId,
                input
            );

            return node.OutputNode != null
                ? Context.GetOutputTokens(node.OutputNode.Identifier, threadId)
                : new List<Token>();
        }

        public async Task<IEnumerable<Token>> DoInitializeParallelNodeAsync<TToken>(Node node, IEnumerable<Token> input = null)
            where TToken : Token, new()
        {
            var restOfInput = input.Where(t => !(t is TToken)).ToArray();

            var parallelInput = input.OfType<TToken>().ToArray();

            var threadIds = parallelInput.ToDictionary<Token, Token, string>(t => t, t => Guid.NewGuid().ToString());

            await Task.WhenAll(parallelInput
                .Select(async token =>
                {
                    var threadId = threadIds[token];

                    await DoExecuteNodeStructureAsync(
                        node,
                        threadId,
                        restOfInput.Concat(new Token[] { token }).ToArray()
                    );

                    //lock (Context.Streams)
                    //{
                    //    Context.Streams.Remove(threadId);
                    //}
                })
            );

            return node.OutputNode != null
                ? threadIds.Values.SelectMany(threadId => Context.GetOutputTokens(node.OutputNode.Identifier, threadId))
                : new List<Token>();
        }

        public async Task<IEnumerable<Token>> DoInitializeIterativeNodeAsync<TToken>(Node node, IEnumerable<Token> input = null)
            where TToken : Token, new()
        {
            var restOfInput = input.Where(t => !(t is TToken)).ToArray();

            var iterativeInput = input.OfType<TToken>().ToArray();

            var threadId = Guid.NewGuid().ToString();

            foreach (var token in iterativeInput)
            {
                await DoExecuteNodeStructureAsync(
                    node,
                    threadId,
                    restOfInput.Concat(new Token[] { token })
                );

                lock (Context.Streams)
                {
                    Context.Streams.Remove(threadId);
                }
            }

            return node.OutputNode != null
                ? Context.GetOutputTokens(node.OutputNode.Identifier, threadId)
                : new List<Token>();
        }

        private Task DoExecuteNodeStructureAsync(Node node, string threadId, IEnumerable<Token> input = null)
        {
            var nodes = new List<Node>();
            nodes.AddRange(node.InitialNodes);
            if (node.InputNode != null)
            {
                nodes.Add(node.InputNode);
            }

            return Task.WhenAll(
                nodes.Select(n => DoHandleNodeAsync(n, threadId, n == node.InputNode ? input : null))
            );

            //RegisterTask(
            //    Task.Run(async () =>
            //    {
            //        await Task.WhenAll(Graph.InitialNodes
            //            .Select(node => node.Action.WhenAll(Context))
            //        );

            //        await Task.WhenAll(Graph.InitialNodes
            //            .SelectMany(node => node.Edges.Values)
            //            .Where(edge => Context.Streams[edge.Identifier].IsActivated)
            //            .Select(edge => edge.Target)
            //            .Select(node => node.Action.WhenAll(Context))
            //        );
            //    })
            //);

            //await Graph.Initialize.WhenAll(Context);

            //await Observer.AfterStateMachineInitializeAsync(context);
        }

        public async Task<bool> DoInitializeActivityAsync(InitializationRequest @event)
        {
            if (
                Graph.Initializers.TryGetValue(@event.EventName, out var initializer) ||
                (
                    @event.EventName == EventInfo<InitializationRequest>.Name &&
                    !Graph.Initializers.Any()
                )
            )
            {
                Context.Event = @event;
                var context = new ActivityInitializationContext(Context, NodeScope, @event);
                //await Observer.BeforeStateMachineInitializeAsync(context);

                if (initializer != null)
                {
                    try
                    {
                        await initializer.WhenAll(context);
                    }
                    catch (Exception)
                    {
                        return false;
                        //throw;
                    }
                }

                //await Observer.AfterStateMachineInitializeAsync(context);

                return true;
            }

            return false;
        }

        public Task DoHandleNodeAsync(Node node, string threadId, IEnumerable<Token> input = null)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    Stream[] streams;

                    lock (node)
                    {
                        streams = node.IncomingEdges
                            .Select(edge => Context.GetStream(edge.Identifier, threadId))
                            .Where(stream => stream.IsActivated)
                            .ToArray();
                    }

                    if ( // implicit join node - has incoming streams on all edges
                            node.IncomingEdges.Count != streams.Length &&
                            node.Options.HasFlag(NodeOptions.ImplicitJoin)
                        )
                    {
                        lock (node.Graph)
                        {
                            Debug.WriteLine("");
                            Debug.WriteLine($"    >>> Skipping node {node.Name.Split('.').Last()}: only {streams.Length} of {node.IncomingEdges.Count} edges are activated, threadId: {threadId}. Missing edge sources:");
                            foreach (var edge in node.IncomingEdges)
                            {
                                if (!streams.Any(s => s.EdgeIdentifier == edge.Identifier))
                                {
                                    Debug.WriteLine($"      - {TokenInfo.GetName(edge.TokenType).Split('.').Last()} from {edge.SourceName.Split('.').Last()}");
                                }
                            }
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
                            streams.Length > 0 &&
                            !node.Options.HasFlag(NodeOptions.ImplicitJoin)
                        ) ||
                        ( // implicit join node - has incoming streams on all edges
                            node.IncomingEdges.Count == streams.Length &&
                            node.Options.HasFlag(NodeOptions.ImplicitJoin)
                        )
                    )
                    {
                        var inputTokens = input == null
                            ? streams.SelectMany(stream => stream.Tokens).ToArray()
                            : input;

                        lock (node.Graph)
                        {
                            Debug.WriteLine("");
                            Debug.WriteLine($">>> Executing node {node.Name.Split('.').Last()}, threadId: {threadId}");
                        }

                        var context = new ActionContext(Context, NodeScope.CreateChildScope(), node, inputTokens);

                        await node.Action.WhenAll(context);

                        if (node.Type == NodeType.Output)
                        {
                            DoHandleOutput(context, threadId);
                        }
                        else
                        {
                            if (context.OutputTokens.Any(t => t is ControlToken))
                            {
                                try
                                {
                                    var tokenNames = context.OutputTokens.Select(token => token.Name).Distinct();
                                    var edges = node.Edges.Where(edge => tokenNames.Contains(edge.TokenType.GetTokenName()) || edge.Weight == 0);

                                    foreach (var edge in edges)
                                    {
                                        if (CancellationToken.IsCancellationRequested) return;

                                        await DoHandleEdgeAsync(context, edge, threadId);
                                    }

                                    var nodes = edges.Select(edge => edge.Target).Distinct();

                                    _ = Task.WhenAll(nodes.Select(n => DoHandleNodeAsync(n, threadId)));
                                }
                                catch (Exception e)
                                {
                                    throw;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
            });

            RegisterTask(task, threadId);

            return task;
        }

        private void DoHandleOutput(ActionContext context, string threadId)
            => context.Context.GetOutputTokens(context.Node.Identifier, threadId).AddRange(context.OutputTokens);

        private async Task DoHandleEdgeAsync(ActionContext context, Edge edge, string threadId)
        {
            var edgeTokenName = edge.TokenType.GetTokenName();

            IEnumerable<Token> processedTokens = context.OutputTokens.Where(t => /*t != null && */t.Name == edgeTokenName).ToArray();

            foreach (var logic in edge.Pipeline.Actions)
            {
                if (CancellationToken.IsCancellationRequested) return;

                foreach (var action in logic.Actions)
                {
                    if (CancellationToken.IsCancellationRequested) return;

                    processedTokens = await action.Invoke(new PipelineContext(Context, context.NodeScope, edge, processedTokens));
                }
            }

            if (processedTokens.Count() >= edge.Weight)
            {
                var stream = Context.GetStream(edge.Identifier, threadId);

                if (edgeTokenName != TokenInfo<ControlToken>.TokenName)
                {
                    processedTokens = processedTokens.Concat(new[] { new ControlToken() });
                }

                lock (edge.Source.Graph)
                {
                    Debug.WriteLine("");
                    Debug.WriteLine($">>> Edge {edge.Source.Name.Split('.').Last()}-->{edge.Target.Name.Split('.').Last()}, threadId: {threadId}, tokens count: {processedTokens.Count()}");
                }

                lock (edge.Source)
                {
                    stream.Consume(processedTokens);

                    if (!stream.IsActivated)
                    {
                        Debug.WriteLine($"  -----------> NOT ACTIVATED threadId: {threadId}");
                    }
                }
            }
            else
            {
                //lock (edge.Source.Graph)
                //{
                //    Debug.WriteLine("");
                //    Debug.WriteLine($">>> Edge from node {edge.Source.Name.Split('.').Last()} processing, tokens count smaller than edge weight");
                //}
            }
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

        //public TActivity GetActivity<TActivity>(RootContext context)
        //    where TActivity : Activity
        //{
        //    if (!Activities.TryGetValue(typeof(TActivity), out var activity))
        //    {
        //        activity = ServiceProvider.GetService<TActivity>();
        //        activity.Context = new ActivityActionContext(context);

        //        Activities.Add(typeof(TActivity), activity);
        //    }

        //    return activity as TActivity;
        ////}

        //private readonly IDictionary<Type, Action> Actions = new Dictionary<Type, Action>();

        //public TAction GetAction<TAction>(IActionContext context)
        //    where TAction : Action
        //{
        //    lock (Actions)
        //    {
        //        if (!Actions.TryGetValue(typeof(TAction), out var action))
        //        {
        //            action = ServiceProvider.GetService<TAction>();

        //            Actions.Add(typeof(TAction), action);
        //        }

        //        action.Context = context;

        //        return action as TAction;
        //    }
        //}

        //private readonly IDictionary<Type, StructuredActivity> StructuredActivities = new Dictionary<Type, StructuredActivity>();

        ////public StructuredActivity GetStructuredActivity(Type structuredActivityType, IActionContext context)
        ////{
        ////    if (!StructuredActivities.TryGetValue(structuredActivityType, out var structuredActivity))
        ////    {
        ////        structuredActivity = ServiceProvider.GetService(structuredActivityType) as StructuredActivity;

        ////        StructuredActivities.Add(typeof(StructuredActivity), structuredActivity);
        ////    }

        ////    structuredActivity.Context = context;

        ////    return structuredActivity;
        ////}

        //public TStructuredActivity GetStructuredActivity<TStructuredActivity>(IActionContext context)
        //    where TStructuredActivity : StructuredActivity
        //{
        //    if (!StructuredActivities.TryGetValue(typeof(TStructuredActivity), out var structuredActivity))
        //    {
        //        structuredActivity = ServiceProvider.GetService<TStructuredActivity>();

        //        StructuredActivities.Add(typeof(TStructuredActivity), structuredActivity);
        //    }

        //    structuredActivity.Context = context;

        //    return structuredActivity as TStructuredActivity;
        //}

        //private readonly IDictionary<Type, ActivityNode> ExceptionHandlers = new Dictionary<Type, ActivityNode>();

        //public TExceptionHandler GetExceptionHandler<TException, TExceptionHandler>(IExceptionHandlerContext<TException> context)
        //    where TException : Exception
        //    where TExceptionHandler : ExceptionHandler<TException>
        //{
        //    if (!ExceptionHandlers.TryGetValue(typeof(TExceptionHandler), out var exceptionHandlerNode))
        //    {
        //        var exceptionHandler = ServiceProvider.GetService<TExceptionHandler>();

        //        exceptionHandler.Context = context;

        //        ExceptionHandlers.Add(typeof(TExceptionHandler), exceptionHandler);

        //        return exceptionHandler;
        //    }
        //    else
        //    {
        //        if (exceptionHandlerNode is TExceptionHandler exceptionHandler)
        //        {
        //            exceptionHandler.Context = context;

        //            return exceptionHandler;
        //        }
        //    }

        //    return null;
        //}

        //private readonly IDictionary<Type, object> Flows = new Dictionary<Type, object>();

        //public TControlFlow GetControlFlow<TControlFlow>(IFlowContext context)
        //    where TControlFlow : ControlFlow
        //{
        //    if (!Flows.TryGetValue(typeof(TControlFlow), out var flowObj))
        //    {
        //        var flow = ServiceProvider.GetService<TControlFlow>();

        //        flow.Context = context;

        //        Flows.Add(typeof(TControlFlow), flow);

        //        return flow;
        //    }
        //    else
        //    {
        //        (flowObj as TControlFlow).Context = context;

        //        return flowObj as TControlFlow;
        //    }
        //}

        //public TFlow GetObjectFlow<TFlow, TToken>(IFlowContext<TToken> context)
        //    where TFlow : ObjectFlow<TToken>
        //    where TToken : Token, new();
        //{
        //    if (!Flows.TryGetValue(typeof(TFlow), out var flowObj))
        //    {
        //        var flow = ServiceProvider.GetService<TFlow>();

        //        flow.Context = context;

        //        Flows.Add(typeof(TFlow), flow);

        //        return flow;
        //    }
        //    else
        //    {
        //        (flowObj as TFlow).Context = context;

        //        return flowObj as TFlow;
        //    }
        //}

        //public TFlow GetObjectTransformationFlow<TFlow, TToken, TTransformedToken>(IFlowContext<TToken> context)
        //    where TFlow : ObjectTransformationFlow<TToken, TTransformedToken>
        //    where TToken : Token, new();
        //    where TTransformedToken : Token
        //{
        //    if (!Flows.TryGetValue(typeof(TFlow), out var flowObj))
        //    {
        //        var flow = ServiceProvider.GetService<TFlow>();

        //        flow.Context = context;

        //        Flows.Add(typeof(TFlow), flow);

        //        return flow;
        //    }
        //    else
        //    {
        //        (flowObj as TFlow).Context = context;

        //        return flowObj as TFlow;
        //    }
        //}
    }
}
