using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OneOf;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.StateMachines.Json;
using Stateflows.StateMachines.Registration.Interfaces;
using StateMachine.IntegrationTests.Utils;
using static StateMachine.IntegrationTests.Tests.Json.JsonDefinitions;

namespace StateMachine.IntegrationTests.Tests.Json;

[TestClass]
[TestCategory("JsonDefinitions")]
public class JsonExecutionTests : StateflowsTestClass
{
    private readonly Trace trace = new();

    public sealed class Trace
    {
        public ConcurrentQueue<string> Entries { get; } = new();
        public TaskCompletionSource Done { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Hosted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public sealed class TraceHandler(Trace trace, IStateContext state) : IStateEntry, IStateExit, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task OnEntryAsync()
        {
            trace.Entries.Enqueue($"{state.Name}:{Configuration}");
            if (Configuration == "done") trace.Done.TrySetResult();
            return Task.CompletedTask;
        }
        public Task OnExitAsync()
        {
            trace.Entries.Enqueue($"{state.Name}:{Configuration}");
            return Task.CompletedTask;
        }
    }

    public sealed class Predicate : IStateMachineGuard<object>, IConfigurable<bool>
    {
        public bool Configuration { private get; set; }
        public Task<bool> GuardAsync(object @event) => Task.FromResult(Configuration);
    }

    public sealed class Effect(Trace trace, ITransitionContext context) :
        ITransitionEffect<Ping>, ITransitionEffect<Pong>, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task EffectAsync(Ping @event) => Execute();
        public Task EffectAsync(Pong @event) => Execute();
        private Task Execute()
        {
            var target = context.Target ?? throw new InvalidOperationException("This test effect requires a targeted transition.");
            trace.Entries.Enqueue($"{context.Source.Name}->{target.Name}:{Configuration}");
            return Task.CompletedTask;
        }
    }

    public sealed class HostedAction(Trace trace) : IAction
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            trace.Hosted.TrySetResult();
            return Task.CompletedTask;
        }
    }

    public sealed class UnionPredicate : ITransitionGuard<OneOf<Ping, Pong>>, IDeferralGuard<OneOf<Ping, Pong>>
    {
        public Task<bool> GuardAsync(OneOf<Ping, Pong> @event) => Task.FromResult(true);
    }

    public sealed class Initializer(Trace trace) : IInitializer<object>, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task<bool> OnInitializeAsync(object @event)
        {
            trace.Entries.Enqueue("initialize:" + Configuration);
            return Task.FromResult(true);
        }
    }

    public sealed class DualConfiguration(Trace trace) : IStateEntry, IConfigurable<string>, IConfigurable<int>
    {
        string IConfigurable<string>.Configuration { set => trace.Entries.Enqueue("dual:string:" + value); }
        int IConfigurable<int>.Configuration { set => trace.Entries.Enqueue("dual:int:" + value); }
        public Task OnEntryAsync()
        {
            trace.Entries.Enqueue("dual:entry");
            return Task.CompletedTask;
        }
    }

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        ServiceCollection.AddSingleton(trace);
        builder.AddOneOf();
        builder.AddActions(actions => actions
            .AddAction<HostedAction>("hosted")
            .AddAction("trueGuard", async context => context.Output(true))
            .AddAction("entryAction", async context => trace.Entries.Enqueue("behavior-entry"))
            .AddAction("exitAction", async context => trace.Entries.Enqueue("behavior-exit"))
            .AddAction("effectAction", async context => trace.Entries.Enqueue("behavior-effect")));
        builder.AddActivities(activities => activities.AddActivity("entryActivity", activity => activity
            .AddInitial(initial => initial.AddControlFlow("note"))
            .AddAction("note", async context => trace.Entries.Enqueue("behavior-activity")))
            .AddActivity("exitActivity", activity => activity
                .AddInitial(initial => initial.AddControlFlow("note"))
                .AddAction("note", async context => trace.Entries.Enqueue("behavior-exit")))
            .AddActivity("effectActivity", activity => activity
                .AddInput(input => input.AddFlow<Ping>("note"))
                .AddAction("note", async context => trace.Entries.Enqueue("behavior-effect")))
            .AddActivity("trueGuardActivity", activity => activity
                .AddAcceptEventAction<Ping>(async context => context.Output(true),
                    accept => accept.AddFlow<bool, OutputNode>())
                .AddOutput()));
        var options = Options().AddType<TraceHandler>("trace").AddType<Predicate>("predicate")
            .AddType<Effect>("effect").AddType<UnionPredicate>("unionPredicate").AddType<Initializer>("initializer")
            .AddType<DualConfiguration>("dual");
        builder.AddStateMachines(machines =>
        {
            foreach (var name in new[] { "configured", "configured-multi", "union", "behavior", "behavior-activity", "hosted", "initializers", "deferral" })
            {
                var model = JObject.Parse("""
                    {
                      "formatVersion":"1.0","name":"placeholder","initial":"start",
                      "vertices":[
                        {"kind":"state","name":"start",
                         "onEntry":[{"clrType":"trace","configuration":"entry"}],
                         "onExit":[{"clrType":"trace","configuration":"exit"}],
                         "transitions":[
                           {"kind":"event","triggers":[{"clrType":"ping"},{"clrType":"pong"}],"target":"finish",
                            "guard":{"all":[
                              {"clrType":"predicate","configuration":true},
                              {"any":[{"clrType":"predicate","configuration":false},{"clrType":"predicate","configuration":true}]}
                            ]},
                            "effects":[{"clrType":"effect","configuration":"effect"}]}
                         ]},
                        {"kind":"state","name":"finish","onEntry":[{"clrType":"trace","configuration":"done"}]}
                      ]
                    }
                    """);
                model["name"] = name;
                var state = State(model);
                var transition = state["transitions"]![0]!;
                if (name == "configured-multi")
                    state["onEntry"] = JArray.Parse("""
                        [{"clrType":"dual","configurationType":"int","configuration":42},
                         {"clrType":"dual","configurationType":"string","configuration":"abc"}]
                        """);
                if (name == "union")
                {
                    transition["triggers"] = JArray.Parse("""[{"clrType":"union"}]""");
                    transition["guard"] = JObject.Parse("""{"clrType":"unionPredicate"}""");
                }
                if (name is "behavior" or "behavior-activity")
                {
                    state["onEntry"] = JArray.Parse("""
                        [{"behaviorClass":{"type":"Action","name":"entryAction"}},
                         {"behaviorClass":{"type":"Activity","name":"entryActivity"}}]
                        """);
                    state["onExit"] = JArray.Parse("""[{"behaviorClass":{"type":"Action","name":"exitAction"}}]""");
                    transition["guard"] = JObject.Parse("""{"behaviorClass":{"type":"Action","name":"trueGuard"}}""");
                    transition["effects"] = JArray.Parse("""[{"behaviorClass":{"type":"Action","name":"effectAction"}}]""");
                    if (name == "behavior-activity")
                    {
                        state["onExit"] = JArray.Parse("""[{"behaviorClass":{"type":"Activity","name":"exitActivity"}}]""");
                        transition["triggers"] = JArray.Parse("""[{"clrType":"ping"}]""");
                        transition["guard"] = JObject.Parse("""{"behaviorClass":{"type":"Activity","name":"trueGuardActivity"}}""");
                        transition["effects"] = JArray.Parse("""[{"behaviorClass":{"type":"Activity","name":"effectActivity"}}]""");
                    }
                }
                if (name == "hosted")
                    state["do"] = JObject.Parse("""{"behaviorClass":{"type":"Action","name":"hosted"}}""");
                if (name == "initializers")
                    model["initializers"] = JArray.Parse("""
                        [{"triggers":[{"clrType":"ping"},{"clrType":"pong"}],
                          "handlers":[{"clrType":"initializer","configuration":"configured"}]}]
                        """);
                if (name == "deferral")
                {
                    state["deferrals"] = JArray.Parse("""
                        [{"triggers":[{"clrType":"union"}],"guard":{"clrType":"unionPredicate"}}]
                        """);
                    state.Remove("transitions");
                }
                machines.AddStateMachineJson(model.ToString(), options);
            }
        });
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ConfiguredHandlersRunForEveryTriggerWithIndependentConfigurationAndContexts(bool pong)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("configured", "instance"), out var machine));
        var result = pong ? await machine.SendAsync(new Pong()) : await machine.SendAsync(new Ping());
        Assert.AreEqual(EventStatus.Consumed, result.Status);
        Assert.AreEqual("finish", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
        CollectionAssert.AreEqual(new[] { "start:entry", "start:exit", "start->finish:effect", "finish:done" }, trace.Entries.ToArray());
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task UnionHandlerReceivesConvertedPayload(bool pong)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("union", "instance"), out var machine));
        var result = pong ? await machine.SendAsync(new Pong()) : await machine.SendAsync(new Ping());
        Assert.AreEqual(EventStatus.Consumed, result.Status);
        Assert.AreEqual("finish", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
    }

    [TestMethod]
    [DataRow("behavior")]
    [DataRow("behavior-activity")]
    public async Task BehaviorClassesExecuteEntryExitGuardAndEffectAdapters(string name)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId(name, "instance"), out var machine));
        await machine.SendAsync(new Ping());
        await trace.Done.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual("finish", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
        var expected = new[] { "behavior-entry", "behavior-activity", "behavior-exit", "behavior-effect" };
        var timeout = DateTime.UtcNow.AddSeconds(10);
        while (expected.Except(trace.Entries).Any() && DateTime.UtcNow < timeout)
            await Task.Delay(20);
        foreach (var entry in expected) CollectionAssert.Contains(trace.Entries.ToArray(), entry);
    }

    [TestMethod]
    public async Task HostedBehaviorUsesRegisteredBehaviorClass()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("hosted", "instance"), out var machine));
        await machine.SendAsync(new Initialize());
        await trace.Hosted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual("start", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task MultipleInitializerTriggersBindTheCorrectClrContract(bool pong)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("initializers", "instance"), out var machine));
        if (pong) await machine.SendAsync(new Pong());
        else await machine.SendAsync(new Ping());
        CollectionAssert.Contains(trace.Entries.ToArray(), "initialize:configured");
        Assert.AreEqual("start", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task UnionDeferralGuardsReceiveConvertedPayload(bool pong)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("deferral", "instance"), out var machine));
        await machine.SendAsync(new Initialize());
        var result = pong ? await machine.SendAsync(new Pong()) : await machine.SendAsync(new Ping());
        Assert.AreEqual(EventStatus.Deferred, result.Status);
    }

    [TestMethod]
    public async Task ExplicitConfigurationTypesSelectTheMatchingInterfaceSetter()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("configured-multi", "instance"), out var machine));
        await machine.SendAsync(new Initialize());
        CollectionAssert.AreEqual(new[] { "dual:int:42", "dual:entry", "dual:string:abc", "dual:entry" }, trace.Entries.ToArray());
    }
}
