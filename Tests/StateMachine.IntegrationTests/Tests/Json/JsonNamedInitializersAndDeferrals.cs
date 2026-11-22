using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Stateflows.Activities;
using Stateflows.Activities.Json;
using Stateflows.Common;
using Stateflows.Common.Json;
using Stateflows.Common.Registration.Builders;
using Stateflows.Payloads;
using Stateflows.Payloads.Json;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Json;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests.Json;

[TestClass]
[TestCategory("JsonNamedInitializersAndDeferrals")]
public class JsonNamedInitializersAndDeferrals : StateflowsTestClass
{
    private readonly Trace trace = new();
    public sealed class Trace { public ConcurrentQueue<string> Entries { get; } = new(); }

    public sealed class NamedInitializer(Trace trace) : IInitializer<Payload>, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task<bool> OnInitializeAsync(Payload @event)
        {
            trace.Entries.Enqueue($"{Configuration}:{@event.Name}:{@event.Get<int>("quantity")}");
            return Task.FromResult(@event.Get<int>("quantity") > 0);
        }
    }

    public sealed class NamedDeferralGuard : IDeferralGuard<Payload>
    {
        public Task<bool> GuardAsync(Payload @event) => Task.FromResult(@event.Get<int>("quantity") > 0);
    }

    public sealed class Receive(Trace trace) : IAcceptEventActionNode<Payload>
    {
        public Task ExecuteAsync(Payload @event, CancellationToken cancellationToken)
        {
            trace.Entries.Enqueue($"accepted:{@event.Name}");
            return Task.CompletedTask;
        }
    }

    private static JsonOptions Options() => new JsonOptions()
        .AddType<NamedInitializer>("initializer").AddType<NamedDeferralGuard>("deferralGuard").AddType<Receive>("receive");

    private static JObject InitializedMachine() => JObject.Parse("""
        {
          "formatVersion":"1.0","name":"json-named-initializer","initial":"waiting",
          "initializers":[{"events":[{"eventName":"orderSubmitted"},{"eventName":"orderCancelled"}],
                           "handlers":[{"clrType":"initializer","configuration":"machine"}]}],
          "vertices":[{"kind":"state","name":"waiting"}]
        }
        """);

    private static JObject DeferringMachine() => JObject.Parse("""
        {
          "formatVersion":"1.0","name":"json-named-deferral","initial":"waiting",
          "vertices":[
            {"kind":"state","name":"waiting",
             "deferrals":[{"events":[{"eventName":"orderCancelled"},{"eventName":"other"}],"guard":{"clrType":"deferralGuard"}}],
             "transitions":[{"kind":"event","triggers":[{"eventName":"orderSubmitted"}],"target":"submitted"}]},
            {"kind":"state","name":"submitted",
             "transitions":[{"kind":"event","triggers":[{"eventName":"orderCancelled"}],"target":"cancelled"}]},
            {"kind":"state","name":"cancelled"}
          ]
        }
        """);

    private static JObject Activity() => JObject.Parse("""
        {
          "formatVersion":"1.0","name":"json-named-list-activity",
          "initializers":[{"eventNames":["orderSubmitted","orderCancelled"],
                           "handler":{"clrType":"initializer","configuration":"activity"}}],
          "nodes":[
            {"kind":"acceptEvent","name":"receive","eventNames":["other","orderCancelled"],
             "implementation":{"clrType":"receive"}}
          ]
        }
        """);

    private static void Events(IStateflowsBuilder builder) => builder.AddPayloads(events =>
    {
        foreach (var name in new[] { "orderSubmitted", "orderCancelled", "other" })
            JsonPayloadDefinition.Parse($$"""
                {"formatVersion":"1.0","name":"{{name}}","fields":[{"name":"quantity","type":"Int32"}]}
                """, new()).Register(events);
    });

    [TestInitialize] public override void Initialize() => base.Initialize();
    [TestCleanup] public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        ServiceCollection.AddSingleton(trace);
        Events(builder);
        builder.AddStateMachines(machines =>
        {
            JsonStateMachineDefinition.Parse(InitializedMachine().ToString(), Options()).Register(machines);
            JsonStateMachineDefinition.Parse(DeferringMachine().ToString(), Options()).Register(machines);
        });
        builder.AddActivities(activities => JsonActivityDefinition.Parse(Activity().ToString(), Options()).Register(activities));
    }

    private IPayloadFactory Factory => ServiceProvider.GetRequiredService<IPayloadFactory>();

    [TestMethod]
    [DataRow("orderSubmitted")]
    [DataRow("orderCancelled")]
    public async Task NamedInitializerAcceptsEveryDeclaredEvent(string name)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("json-named-initializer", name), out var machine));
        Assert.AreEqual(EventStatus.NotInitialized,
            (await machine.SendAsync(Factory.CreateFromJson(name, """{"quantity":0}"""))).Status);
        Assert.AreEqual(EventStatus.Initialized,
            (await machine.SendAsync(Factory.CreateFromJson(name, """{"quantity":3}"""))).Status);
        Assert.AreEqual("waiting", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
        CollectionAssert.AreEqual(new[] { $"machine:{name}:0", $"machine:{name}:3" }, trace.Entries.ToArray());
    }

    [TestMethod]
    public async Task NamedInitializerIgnoresUndeclaredEvents()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("json-named-initializer", "other"), out var machine));
        Assert.AreNotEqual(EventStatus.Initialized,
            (await machine.SendAsync(Factory.CreateFromJson("other", """{"quantity":3}"""))).Status);
        Assert.AreEqual(0, trace.Entries.Count);
    }

    [TestMethod]
    public async Task NamedDeferralHoldsGuardedEventsUntilTheyCanBeConsumed()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("json-named-deferral", "one"), out var machine));
        await machine.SendAsync(new Initialize());
        Assert.AreEqual(EventStatus.NotConsumed,
            (await machine.SendAsync(Factory.CreateFromJson("orderCancelled", """{"quantity":0}"""))).Status);
        Assert.AreEqual(EventStatus.Deferred,
            (await machine.SendAsync(Factory.CreateFromJson("orderCancelled", """{"quantity":1}"""))).Status);
        Assert.AreEqual(EventStatus.Consumed,
            (await machine.SendAsync(Factory.CreateFromJson("orderSubmitted", """{"quantity":1}"""))).Status);
        Assert.AreEqual("cancelled", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
    }

    [TestMethod]
    public async Task ActivityEventNamesDriveInitializersAndAcceptNodes()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("json-named-list-activity", "one"), out var activity));
        Assert.AreEqual(EventStatus.Initialized,
            (await activity.SendAsync(Factory.CreateFromJson("orderSubmitted", """{"quantity":2}"""))).Status);
        Assert.AreEqual(EventStatus.Consumed,
            (await activity.SendAsync(Factory.CreateFromJson("other", "{}"))).Status);
        Assert.AreEqual(EventStatus.Consumed,
            (await activity.SendAsync(Factory.CreateFromJson("orderCancelled", "{}"))).Status);
        CollectionAssert.AreEqual(new[] { "activity:orderSubmitted:2", "accepted:other", "accepted:orderCancelled" },
            trace.Entries.ToArray());
    }

    [TestMethod]
    [DataRow("initializer")]
    [DataRow("deferral")]
    public void LegacyTriggersKeyIsRejectedForInitializersAndDeferrals(string place)
    {
        var document = place == "initializer" ? InitializedMachine() : DeferringMachine();
        var owner = place == "initializer"
            ? (JObject)document["initializers"]![0]!
            : (JObject)document["vertices"]![0]!["deferrals"]![0]!;
        owner["triggers"] = owner["events"];
        owner.Remove("events");
        var exception = Assert.Throws<JsonStateMachineException>(() =>
            JsonStateMachineDefinition.Parse(document.ToString(), Options()));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.RuleId == "JSON-002"));
    }

    [TestMethod]
    public void NamedDeferralOfAlreadyDeferredEventIsReported()
    {
        var builder = new StateflowsBuilder(new ServiceCollection());
        Events(builder);
        var document = DeferringMachine();
        var deferrals = (JArray)document["vertices"]![0]!["deferrals"]!;
        deferrals.Add(JObject.Parse("""{"events":[{"eventName":"other"}]}"""));
        var exception = Assert.Throws<JsonStateMachineException>(() => builder.AddStateMachines(machines =>
            JsonStateMachineDefinition.Parse(document.ToString(), Options()).Register(machines)));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.RuleId == "SM-035"));
    }

    [TestMethod]
    [DataRow("""["orderSubmitted","orderSubmitted"]""")]
    [DataRow("""[]""")]
    [DataRow("""[""]""")]
    public void InvalidActivityEventNamesFailSchema(string names)
    {
        var document = Activity();
        document["nodes"]![0]!["eventNames"] = JArray.Parse(names);
        Assert.IsTrue(Assert.Throws<JsonActivityException>(() => JsonActivityDefinition.Parse(document.ToString(), Options()))
            .ValidationReport.Diagnostics.Any(d => d.RuleId == "JSON-002"));
        document = Activity();
        document["initializers"]![0]!["eventNames"] = JArray.Parse(names);
        Assert.IsTrue(Assert.Throws<JsonActivityException>(() => JsonActivityDefinition.Parse(document.ToString(), Options()))
            .ValidationReport.Diagnostics.Any(d => d.RuleId == "JSON-002"));
    }

    [TestMethod]
    public void UnknownActivityEventNamesFailBinding()
    {
        var builder = new StateflowsBuilder(new ServiceCollection());
        Events(builder);
        var document = Activity();
        document["nodes"]![0]!["eventNames"] = JArray.Parse("""["other","missing"]""");
        document["initializers"]![0]!["eventNames"] = JArray.Parse("""["missing"]""");
        var exception = Assert.Throws<JsonActivityException>(() => builder.AddActivities(activities =>
            JsonActivityDefinition.Parse(document.ToString(), Options()).Register(activities)));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d =>
            d.RuleId == "JSON-003" && d.ElementPath.EndsWith("/nodes/0/eventNames/1")));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d =>
            d.RuleId == "JSON-003" && d.ElementPath.EndsWith("/initializers/0/eventNames/0")));
    }
}
