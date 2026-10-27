using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Json;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Json;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests.Json;

[TestClass]
[TestCategory("JsonDefinitions")]
public class JsonExecutionTests : StateflowsTestClass
{
    private readonly Trace trace = new();

    public sealed class Trace
    {
        public ConcurrentQueue<string> Entries { get; } = new();
        public ConcurrentQueue<int> Tokens { get; } = new();
        public TaskCompletionSource<int> Received { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private static JObject Binding(string alias, object? configuration = null)
    {
        var binding = new JObject { ["clrType"] = alias };
        if (configuration != null) binding["configuration"] = JToken.FromObject(configuration);
        return binding;
    }

    private static JObject Flow(string kind, string target, string? token = null)
    {
        var flow = new JObject { ["kind"] = kind, ["target"] = target };
        if (token != null) flow["tokenType"] = token;
        return flow;
    }

    private static JObject Node(string name, string alias, object? configuration = null, params JObject[] flows)
        => new()
        {
            ["kind"] = "action", ["name"] = name,
            ["implementation"] = Binding(alias, configuration), ["flows"] = new JArray(flows)
        };

    private static JObject Initial(string target)
        => new() { ["kind"] = "initial", ["flows"] = new JArray(Flow("control", target)) };

    private static JObject Document(string name, params JObject[] nodes)
        => new() { ["formatVersion"] = "1.0", ["name"] = name, ["nodes"] = new JArray(nodes) };

    private static JObject Done() => new() { ["kind"] = "final", ["name"] = "done" };

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        ServiceCollection.AddSingleton(trace);
        var options = new JsonOptions()
            .AddType<Note>("note").AddType<Produce>("produce").AddType<Consume>("consume")
            .AddType<IntGuard>("intGuard").AddType<TextGuard>("textGuard")
            .AddType<ToText>("toText").AddType<ToNumber>("toNumber")
            .AddType<Lifecycle>("lifecycle").AddType<ScopeDefinition>("scopeDefinition")
            .AddType<InitializeEvent>("initializeEvent").AddType<EventInitializer>("initializer")
            .AddType<DefaultInitializer>("defaultInitializer").AddType<Finalizer>("finalizer")
            .AddType<Observer>("observer").AddType<Interceptor>("interceptor")
            .AddType<Receive>("receive").AddType<Message>("message")
            .AddType<Send>("send").AddType<Publish>("publish").AddType<Forward>("forward")
            .AddType<Throw>("throw").AddType<Handle>("handle").AddType<InvalidOperationException>("exception")
            .AddType<DualConfiguration>("dual")
            .AddType<int>("int").AddType<string>("string");

        builder.AddActivities(activities =>
        {
            Register(Document("configured", Initial("first"),
                Node("first", "note", "one", Flow("control", "second")),
                Node("second", "note", "two", Flow("control", "done")), Done()));

            var pipeline = Flow("object", "consume", "int");
            pipeline["pipeline"] = JArray.Parse("""
                [
                  {"kind":"guard","handler":{"clrType":"intGuard","configuration":true}},
                  {"kind":"transform","outputTokenType":"string","handler":{"clrType":"toText","configuration":"value="}},
                  {"kind":"guard","handler":{"clrType":"textGuard","configuration":true}},
                  {"kind":"transform","outputTokenType":"int","handler":{"clrType":"toNumber"}}
                ]
                """);
            Register(Document("pipeline", Initial("produce"),
                Node("produce", "produce", null, pipeline),
                Node("consume", "consume", null, Flow("control", "done")), Done()));

            var stopped = (JObject)pipeline.DeepClone();
            stopped["pipeline"]![0]!["handler"]!["configuration"] = false;
            Register(Document("stopped", Initial("produce"),
                Node("produce", "produce", null, stopped),
                Node("consume", "consume", null, Flow("control", "done")), Done()));

            foreach (var kind in new[] { "structured", "parallel", "iterative" })
            {
                var scope = new JObject
                {
                    ["kind"] = kind, ["name"] = "scope",
                    ["flows"] = new JArray(Flow("control", "done")),
                    ["nodes"] = new JArray(
                        new JObject { ["kind"] = "input", ["flows"] = new JArray(Flow("object", "consume", "int")) },
                        Node("consume", "consume"))
                };
                if (kind != "structured")
                {
                    scope["iterationTokenType"] = "int";
                    scope["chunkSize"] = 2;
                }
                Register(Document(kind, Initial("produce"),
                    Node("produce", "produce", null, Flow("object", "scope", "int")), scope, Done()));
            }

            Register(Document("scope-lifecycle", Initial("scope"),
                new JObject
                {
                    ["kind"] = "structured", ["name"] = "scope",
                    ["onInitialize"] = Binding("lifecycle", "start"),
                    ["onFinalize"] = Binding("lifecycle", "stop"),
                    ["definitions"] = new JArray(Binding("scopeDefinition")),
                    ["flows"] = new JArray(Flow("control", "done")),
                    ["nodes"] = new JArray(Node("authored", "note", "authored"))
                }, Done()));

            var lifecycle = Document("lifecycle", Initial("action"), Node("action", "note", "body", Flow("control", "done")), Done());
            lifecycle["initializers"] = new JArray(new JObject { ["eventType"] = "initializeEvent", ["handler"] = Binding("initializer", "event") });
            lifecycle["defaultInitializers"] = new JArray(Binding("defaultInitializer", "default"));
            lifecycle["finalizers"] = new JArray(Binding("finalizer", "end"));
            lifecycle["observers"] = new JArray(Binding("observer", "observed"));
            lifecycle["interceptors"] = new JArray(Binding("interceptor", "intercepted"));
            Register(lifecycle);

            Register(Document("receive",
                new JObject
                {
                    ["kind"] = "acceptEvent", ["name"] = "receive", ["eventType"] = "message",
                    ["implementation"] = Binding("receive", "received"),
                    ["flows"] = new JArray(Flow("object", "consume", "int"))
                },
                Node("consume", "consume", null, Flow("control", "done")), Done()));

            foreach (var kind in new[] { "sendEvent", "publishEvent" })
                Register(Document(kind, Initial("event"),
                    new JObject
                    {
                        ["kind"] = kind, ["name"] = "event", ["eventType"] = "message",
                        ["implementation"] = Binding(kind == "sendEvent" ? "send" : "publish", 23),
                        ["flows"] = new JArray(Flow("control", "done"))
                    }, Done()));

            Register(Document("fork-join", Initial("fork"),
                new JObject
                {
                    ["kind"] = "fork", ["name"] = "fork",
                    ["flows"] = new JArray(Flow("control", "left"), Flow("control", "right"))
                },
                Node("left", "note", "left", Flow("control", "join")),
                Node("right", "note", "right", Flow("control", "join")),
                new JObject { ["kind"] = "join", ["name"] = "join", ["flows"] = new JArray(Flow("control", "done")) },
                Done()));

            foreach (var kind in new[] { "merge", "dataStore" })
                Register(Document(kind, Initial("produce"),
                    Node("produce", "produce", null, Flow("object", "relay", "int")),
                    new JObject { ["kind"] = kind, ["name"] = "relay", ["flows"] = new JArray(Flow("object", "consume", "int")) },
                    Node("consume", "consume", null, Flow("control", "done")), Done()));

            Register(Document("scope-output", Initial("produce"),
                Node("produce", "produce", null, Flow("object", "scope", "int")),
                new JObject
                {
                    ["kind"] = "structured", ["name"] = "scope",
                    ["flows"] = new JArray(Flow("object", "consume", "int")),
                    ["nodes"] = new JArray(
                        new JObject { ["kind"] = "input", ["flows"] = new JArray(Flow("object", "forward", "int")) },
                        Node("forward", "forward", null, Flow("object", "result", "int")),
                        new JObject { ["kind"] = "output", ["name"] = "result" })
                },
                Node("consume", "consume", null, Flow("control", "done")), Done()));

            var throwing = Node("throw", "throw", null, Flow("object", "consume", "int"));
            throwing["exceptionHandlers"] = new JArray(new JObject
            {
                ["exceptionType"] = "exception", ["handler"] = Binding("handle", "handled")
            });
            Register(Document("exception", Initial("throw"), throwing,
                Node("consume", "consume", null, Flow("control", "done")), Done()));

            var dual = Node("dual", "dual", 7, Flow("control", "done"));
            dual["implementation"]!["configurationType"] = "int";
            Register(Document("dual", Initial("dual"), dual, Done()));

            foreach (var guard in new[] { true, false })
            {
                var ordinary = Flow("object", "consume", "int");
                ordinary["pipeline"] = new JArray(new JObject { ["kind"] = "guard", ["handler"] = Binding("intGuard", guard) });
                var fallback = Flow("elseObject", "consumeElse", "int");
                fallback["pipeline"] = JArray.Parse("""
                    [{"kind":"transform","outputTokenType":"string","handler":{"clrType":"toText","configuration":"else="}},
                     {"kind":"transform","outputTokenType":"int","handler":{"clrType":"toNumber"}}]
                    """);
                Register(Document("decision-" + guard, Initial("produce"),
                    Node("produce", "produce", null, Flow("object", "decision", "int")),
                    new JObject
                    {
                        ["kind"] = "decision", ["mode"] = "object", ["name"] = "decision", ["tokenType"] = "int",
                        ["flows"] = new JArray(ordinary, fallback)
                    },
                    Node("consume", "consume", null, Flow("control", "done")),
                    Node("consumeElse", "consume", null, Flow("control", "done")), Done()));
            }

            void Register(JObject model) => activities.AddActivityJson(model.ToString(), options);
        });
    }

    private async Task Run(string name, object? initialization = null)
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId(name, "instance"), out var activity));
        var result = initialization is InitializeEvent @event
            ? await activity.SendAsync(@event)
            : await activity.SendAsync(new Initialize());
        Assert.AreEqual(EventStatus.Initialized, result.Status);
        Assert.AreEqual(BehaviorStatus.Finalized, (await activity.GetStatusAsync()).Response.BehaviorStatus);
    }

    [TestMethod]
    public async Task SameHandlerHasIndependentConfigurationAtEachNode()
    {
        await Run("configured");
        CollectionAssert.AreEqual(new[] { "note:one", "note:two" }, trace.Entries.ToArray());
    }

    [TestMethod]
    public async Task OrderedPipelineChangesTokenTypesAndRunsSubsequentGuards()
    {
        await Run("pipeline");
        CollectionAssert.AreEqual(new[] { "int-guard:True", "text:value=1", "text-guard:value=1:True", "number:value=1",
            "int-guard:True", "text:value=2", "text-guard:value=2:True", "number:value=2",
            "int-guard:True", "text:value=3", "text-guard:value=3:True", "number:value=3", "consume" }, trace.Entries.ToArray());
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, trace.Tokens.ToArray());
    }

    [TestMethod]
    public async Task RejectedTokensDoNotExecuteLaterStagesOrDestination()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("stopped", "instance"), out var activity));
        Assert.AreEqual(EventStatus.Initialized, (await activity.SendAsync(new Initialize())).Status);
        CollectionAssert.AreEqual(new[] { "int-guard:False", "int-guard:False", "int-guard:False" }, trace.Entries.ToArray());
        Assert.IsEmpty(trace.Tokens);
    }

    [TestMethod]
    [DataRow("structured", 1)]
    [DataRow("parallel", 2)]
    [DataRow("iterative", 2)]
    public async Task NestedScopesExecuteInputFlowsAndHonorChunkSize(string name, int executions)
    {
        await Run(name);
        CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, trace.Tokens.ToArray());
        Assert.AreEqual(executions, trace.Entries.Count(entry => entry == "consume"));
    }

    [TestMethod]
    public async Task StructuredDefinitionsComposeWithJsonAndLifecycleBindings()
    {
        await Run("scope-lifecycle");
        CollectionAssert.AreEqual(new[] { "initialize:start", "note:built", "note:authored", "finalize:stop" },
            trace.Entries.ToArray());
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task InitializersFinalizersAndConfiguredInfrastructureExecute(bool explicitEvent)
    {
        await Run("lifecycle", explicitEvent ? new InitializeEvent { Value = 42 } : null);
        var entries = trace.Entries.ToArray();
        Assert.IsTrue(entries.Contains(explicitEvent ? "initialize:event:42" : "initialize:default"));
        Assert.IsFalse(entries.Contains(explicitEvent ? "initialize:default" : "initialize:event:42"));
        Assert.IsTrue(entries.Contains("note:body"));
        Assert.IsTrue(entries.Contains("finalize:end"));
        Assert.IsTrue(entries.Contains("observer:observed"));
        Assert.IsTrue(entries.Contains("interceptor:intercepted"));
        Assert.IsTrue(Array.IndexOf(entries, "note:body") < Array.IndexOf(entries, "finalize:end"));
    }

    [TestMethod]
    public async Task AcceptedEventCanProduceExplicitOutputTokens()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("receive", "instance"), out var activity));
        Assert.AreEqual(EventStatus.Initialized, (await activity.SendAsync(new Initialize())).Status);
        await activity.SendAsync(new Message { Value = 17 });
        CollectionAssert.AreEqual(new[] { 17 }, trace.Tokens.ToArray());
        Assert.IsTrue(trace.Entries.Contains("received:17"));
        Assert.AreEqual(BehaviorStatus.Finalized, (await activity.GetStatusAsync()).Response.BehaviorStatus);
    }

    [TestMethod]
    public async Task ConfiguredSendEventDeliversToSelectedBehavior()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("receive", "target"), out var target));
        Assert.AreEqual(EventStatus.Initialized, (await target.SendAsync(new Initialize())).Status);
        await Run("sendEvent");
        Assert.AreEqual(23, await trace.Received.Task.WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.IsTrue(trace.Entries.Contains("send:23"));
        Assert.IsTrue(trace.Entries.Contains("send:target"));
    }

    [TestMethod]
    public async Task ConfiguredPublishEventGeneratesNotification()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("publishEvent", "instance"), out var activity));
        var notification = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var watcher = await activity.WatchAsync<Message>(message => notification.TrySetResult(message.Value));
        Assert.AreEqual(EventStatus.Initialized, (await activity.SendAsync(new Initialize())).Status);
        Assert.AreEqual(23, await notification.Task.WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.AreEqual(BehaviorStatus.Finalized, (await activity.GetStatusAsync()).Response.BehaviorStatus);
        Assert.IsTrue(trace.Entries.Contains("publish:23"));
    }

    [TestMethod]
    public async Task ForkAndJoinExecuteBothBranches()
    {
        await Run("fork-join");
        CollectionAssert.AreEquivalent(new[] { "note:left", "note:right" }, trace.Entries.ToArray());
    }

    [TestMethod]
    [DataRow("merge")]
    [DataRow("dataStore")]
    [DataRow("scope-output")]
    public async Task RoutingAndStructuredOutputPreserveTokens(string name)
    {
        await Run(name);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, trace.Tokens.ToArray());
        Assert.AreEqual(1, trace.Entries.Count(entry => entry == "consume"));
    }

    [TestMethod]
    public async Task ConfiguredNodeExceptionHandlerReceivesExceptionAndProducesTokens()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("exception", "instance"), out var activity));
        await activity.SendAsync(new Initialize());
        Assert.IsTrue(trace.Entries.Contains("handled:failure"));
        CollectionAssert.AreEqual(new[] { 31 }, trace.Tokens.ToArray());
        Assert.AreEqual(BehaviorStatus.Finalized, (await activity.GetStatusAsync()).Response.BehaviorStatus);
    }

    [TestMethod]
    public async Task ExplicitConfigurationTypeSelectsOnlyMatchingSetter()
    {
        await Run("dual");
        CollectionAssert.AreEqual(new[] { "int:7", "dual:execute" }, trace.Entries.ToArray());
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task ObjectDecisionChoosesGuardedOrTransformedElseFlow(bool guard)
    {
        await Run("decision-" + guard);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, trace.Tokens.ToArray());
        Assert.AreEqual(guard ? 0 : 3, trace.Entries.Count(entry => entry.StartsWith("text:else=", StringComparison.Ordinal)));
        Assert.AreEqual(1, trace.Entries.Count(entry => entry == "consume"));
    }

    public sealed class Note(Trace trace) : IActionNode, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            trace.Entries.Enqueue("note:" + Configuration);
            return Task.CompletedTask;
        }
    }

    public sealed class Produce(IOutputTokens<int> output) : IActionNode
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            output.AddRange(new[] { 1, 2, 3 });
            return Task.CompletedTask;
        }
    }

    public sealed class Consume(Trace trace, IInputTokens<int> input) : IActionNode
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            trace.Entries.Enqueue("consume");
            foreach (var value in input) trace.Tokens.Enqueue(value);
            return Task.CompletedTask;
        }
    }

    public sealed class IntGuard(Trace trace) : IFlowGuard<int>, IConfigurable<bool>
    {
        public bool Configuration { private get; set; }
        public Task<bool> GuardAsync(int token)
        {
            trace.Entries.Enqueue("int-guard:" + Configuration);
            return Task.FromResult(Configuration);
        }
    }

    public sealed class TextGuard(Trace trace) : IFlowGuard<string>, IConfigurable<bool>
    {
        public bool Configuration { private get; set; }
        public Task<bool> GuardAsync(string token)
        {
            trace.Entries.Enqueue($"text-guard:{token}:{Configuration}");
            return Task.FromResult(Configuration);
        }
    }

    public sealed class ToText(Trace trace) : IFlowTransformation<int, string>, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task<string> TransformAsync(int token)
        {
            var text = Configuration + token;
            trace.Entries.Enqueue("text:" + text);
            return Task.FromResult(text);
        }
    }

    public sealed class ToNumber(Trace trace) : IFlowTransformation<string, int>
    {
        public Task<int> TransformAsync(string token)
        {
            trace.Entries.Enqueue("number:" + token);
            return Task.FromResult(int.Parse(token[(token.IndexOf('=') + 1)..]));
        }
    }

    public sealed class Lifecycle(Trace trace) : IStructuredActivityNodeInitialization, IStructuredActivityNodeFinalization, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task OnInitializeAsync() { trace.Entries.Enqueue("initialize:" + Configuration); return Task.CompletedTask; }
        public Task OnFinalizeAsync() { trace.Entries.Enqueue("finalize:" + Configuration); return Task.CompletedTask; }
    }

    public sealed class ScopeDefinition : IStructuredActivityNodeDefinition
    {
        public static void Build(IStructuredActivityBuilder builder)
            => builder.AddInitial(initial => initial.AddControlFlow("definition"))
                .AddAction<Note>("definition", action => action.Configure(note => note.Configuration = "built")
                    .AddControlFlow("authored"));
    }

    public sealed class InitializeEvent
    {
        public int Value { get; set; }
    }

    public sealed class EventInitializer(Trace trace) : IInitializer<InitializeEvent>, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task<bool> OnInitializeAsync(InitializeEvent @event)
        {
            trace.Entries.Enqueue($"initialize:{Configuration}:{@event.Value}");
            return Task.FromResult(true);
        }
    }

    public sealed class DefaultInitializer(Trace trace) : IDefaultInitializer, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task<bool> OnInitializeAsync() { trace.Entries.Enqueue("initialize:" + Configuration); return Task.FromResult(true); }
    }

    public sealed class Finalizer(Trace trace) : IFinalizer, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task OnFinalizeAsync() { trace.Entries.Enqueue("finalize:" + Configuration); return Task.CompletedTask; }
    }

    public sealed class Observer(Trace trace) : ActivityObserver, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public override async Task OnActivityInitializeAsync(IActivityInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
        {
            await next();
            trace.Entries.Enqueue("observer:" + Configuration);
        }
    }

    public sealed class Interceptor(Trace trace) : ActivityInterceptor, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public override async Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
        {
            if (!Before(context)) return EventStatus.Rejected;

            return await next();

            bool Before(IEventContext<TEvent> context)
            {
                trace.Entries.Enqueue("interceptor:" + Configuration);
                return true;
            }
        }
    }

    public sealed class Message
    {
        public int Value { get; set; }
    }

    public sealed class Receive(Trace trace, IOutputTokens<int> output) : IAcceptEventActionNode<Message>, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task ExecuteAsync(Message @event, CancellationToken cancellationToken)
        {
            trace.Entries.Enqueue($"{Configuration}:{@event.Value}");
            output.Add(@event.Value);
            trace.Received.TrySetResult(@event.Value);
            return Task.CompletedTask;
        }
    }

    public sealed class Send(Trace trace) : ISendEventActionNode<Message>, IConfigurable<int>
    {
        public int Configuration { private get; set; }
        public Task<Message> GenerateEventAsync()
        {
            trace.Entries.Enqueue("send:" + Configuration);
            return Task.FromResult(new Message { Value = Configuration });
        }
        public Task<BehaviorId> SelectTargetAsync()
        {
            trace.Entries.Enqueue("send:target");
            return Task.FromResult<BehaviorId>(new ActivityId("receive", "target"));
        }
    }

    public sealed class Publish(Trace trace) : IPublishEventActionNode<Message>, IConfigurable<int>
    {
        public int Configuration { private get; set; }
        public Task<Message> GenerateEventAsync()
        {
            trace.Entries.Enqueue("publish:" + Configuration);
            return Task.FromResult(new Message { Value = Configuration });
        }
    }

    public sealed class Forward(IInputTokens<int> input, IOutputTokens<int> output) : IActionNode
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            output.AddRange(input);
            return Task.CompletedTask;
        }
    }

    public sealed class Throw(IOutputTokens<int> output) : IActionNode
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _ = output;
            throw new InvalidOperationException("failure");
        }
    }

    public sealed class Handle(Trace trace, IOutputTokens<int> output) : IExceptionHandlerNode<InvalidOperationException>, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task HandleAsync(InvalidOperationException exception, CancellationToken cancellationToken)
        {
            trace.Entries.Enqueue(Configuration + ":" + exception.Message);
            output.Add(31);
            return Task.CompletedTask;
        }
    }

    public sealed class DualConfiguration(Trace trace) : IActionNode, IConfigurable<int>, IConfigurable<string>
    {
        int IConfigurable<int>.Configuration { set => trace.Entries.Enqueue("int:" + value); }
        string IConfigurable<string>.Configuration { set => trace.Entries.Enqueue("string:" + value); }
        public Task ExecuteAsync(CancellationToken cancellationToken) { trace.Entries.Enqueue("dual:execute"); return Task.CompletedTask; }
    }
}
