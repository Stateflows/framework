using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Stateflows.Activities;
using Stateflows.Activities.Json;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Json;
using Stateflows.Common.Registration.Builders;
using Stateflows.Events;
using Stateflows.Events.Json;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Json;
using Stateflows.StateMachines.Registration.Interfaces;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests.Json;

[TestClass]
[TestCategory("JsonNamedEventBehaviors")]
public class JsonNamedEventBehaviors : StateflowsTestClass
{
    private readonly Trace trace = new();
    public sealed class Trace { public ConcurrentQueue<string> Entries { get; } = new(); }
    public sealed class Ping;

    public sealed class Guard : ITransitionGuard<Ping>, ITransitionGuard<StructuralEvent>
    {
        public Task<bool> GuardAsync(Ping @event) => Task.FromResult(true);
        public Task<bool> GuardAsync(StructuralEvent @event) => Task.FromResult(@event.Get<int>("quantity") > 0);
    }

    public sealed class Effect(Trace trace) : ITransitionEffect<Ping>, ITransitionEffect<StructuralEvent>
    {
        public Task EffectAsync(Ping @event) { trace.Entries.Enqueue("ping"); return Task.CompletedTask; }
        public Task EffectAsync(StructuralEvent @event) { trace.Entries.Enqueue(@event.Name); return Task.CompletedTask; }
    }

    public sealed class Receive(Trace trace) : IAcceptEventActionNode<StructuralEvent>, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task ExecuteAsync(StructuralEvent @event, CancellationToken cancellationToken)
        {
            trace.Entries.Enqueue($"{Configuration}:{@event.Name}:{@event.Get<int>("quantity")}");
            return Task.CompletedTask;
        }
    }

    private sealed class MachineVisitor : StateMachineVisitor
    {
        public List<EventDefinition> Named { get; } = [];
        public List<Type> Clr { get; } = [];
        public override Task TransitionAddedAsync(EventDefinition definition, string stateMachineName,
            int stateMachineVersion, string sourceVertexName, string? targetVertexName = null, bool isElse = false)
        {
            Named.Add(definition);
            return Task.CompletedTask;
        }
        public override Task TransitionAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion,
            string sourceVertexName, string? targetVertexName = null, bool isElse = false)
        {
            Clr.Add(typeof(TEvent));
            return Task.CompletedTask;
        }
    }

    private sealed class AcceptVisitor : ActivityVisitor
    {
        public List<EventDefinition> Named { get; } = [];
        public override Task AcceptEventNodeAddedAsync(EventDefinition definition, string activityName,
            int activityVersion, string nodeName, string? parentNodeName = null)
        {
            Named.Add(definition);
            return Task.CompletedTask;
        }
    }

    private static JsonOptions Options() => new JsonOptions()
        .AddType<Ping>("ping").AddType<Guard>("guard").AddType<Effect>("effect").AddType<Receive>("receive");

    private static JObject Machine() => JObject.Parse("""
        {
          "formatVersion":"1.0","name":"json-named","initial":"waiting",
          "vertices":[
            {"kind":"state","name":"waiting","transitions":[
              {"kind":"event","triggers":[
                {"clrType":"ping"},{"eventName":"orderSubmitted"},{"eventName":"orderCancelled"}
              ],"target":"done","guard":{"clrType":"guard"},"effects":[{"clrType":"effect"}]}
            ]},
            {"kind":"state","name":"done"}
          ]
        }
        """);

    private static JObject Activity() => JObject.Parse("""
        {
          "formatVersion":"1.0","name":"json-named-activity",
          "nodes":[
            {"kind":"acceptEvent","name":"receive","eventName":"orderSubmitted",
             "implementation":{"clrType":"receive","configuration":"accepted"},
             "flows":[{"kind":"control","target":"done"}]},
            {"kind":"final","name":"done"}
          ]
        }
        """);

    private static void Events(IStateflowsBuilder builder) => builder.AddEvents(events =>
    {
        foreach (var name in new[] { "orderSubmitted", "orderCancelled", "other" })
            JsonEventDefinition.Parse($$"""
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
            JsonStateMachineDefinition.Parse(Machine().ToString(), Options()).Register(machines);
            var namedMachine = Machine();
            namedMachine["name"] = "json-only-named";
            ((JArray)namedMachine["vertices"]![0]!["transitions"]![0]!["triggers"]!).RemoveAt(0);
            JsonStateMachineDefinition.Parse(namedMachine.ToString(), Options()).Register(machines);
            var fallback = Machine();
            fallback["name"] = "json-named-else";
            var transitions = (JArray)fallback["vertices"]![0]!["transitions"]!;
            transitions[0]["triggers"] = JArray.Parse("""[{"eventName":"orderSubmitted"}]""");
            var otherwise = (JObject)transitions[0].DeepClone();
            otherwise["else"] = true;
            otherwise.Remove("guard");
            transitions.Add(otherwise);
            JsonStateMachineDefinition.Parse(fallback.ToString(), Options()).Register(machines);
            var internalMachine = Machine();
            internalMachine["name"] = "json-named-internal";
            var transition = internalMachine["vertices"]![0]!["transitions"]![0]!;
            transition["kind"] = "internal";
            ((JObject)transition).Remove("target");
            JsonStateMachineDefinition.Parse(internalMachine.ToString(), Options()).Register(machines);
        });
        builder.AddActivities(activities => JsonActivityDefinition.Parse(Activity().ToString(), Options()).Register(activities));
    }

    [TestMethod]
    [DataRow("ping")]
    [DataRow("orderSubmitted")]
    [DataRow("orderCancelled")]
    public async Task MixedTriggersRouteEveryDeclaredEventWithoutAcceptingOtherNames(string name)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("json-named", name), out var machine));
        await machine.SendAsync(new Initialize());
        var factory = ServiceProvider.GetRequiredService<IEventFactory>();
        var expected = (await machine.GetStatusAsync()).Response.ExpectedEvents.ToArray();
        CollectionAssert.Contains(expected, "orderSubmitted");
        CollectionAssert.Contains(expected, "orderCancelled");
        Assert.AreEqual(EventStatus.NotConsumed, (await machine.SendAsync(factory.CreateFromJson("other", "{}"))).Status);
        Assert.AreEqual(EventStatus.NotConsumed,
            (await machine.SendAsync(factory.CreateFromJson("orderSubmitted", """{"quantity":0}"""))).Status);
        var result = name == "ping"
            ? await machine.SendAsync(new Ping())
            : await machine.SendAsync(factory.CreateFromJson(name, """{"quantity":2}"""));
        Assert.AreEqual(EventStatus.Consumed, result.Status);
        Assert.AreEqual("done", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
        CollectionAssert.AreEqual(new[] { name }, trace.Entries.ToArray());
    }

    [TestMethod]
    [DataRow("orderSubmitted")]
    [DataRow("orderCancelled")]
    public async Task PureNamedTriggerListsRouteEveryName(string name)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("json-only-named", name), out var machine));
        await machine.SendAsync(new Initialize());
        var factory = ServiceProvider.GetRequiredService<IEventFactory>();
        Assert.AreEqual(EventStatus.Consumed,
            (await machine.SendAsync(factory.CreateFromJson(name, """{"quantity":1}"""))).Status);
        Assert.AreEqual("done", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
        CollectionAssert.AreEqual(new[] { name }, trace.Entries.ToArray());
    }

    [TestMethod]
    public async Task NamedElseTransitionsUseNameRoutingAfterGuardRejection()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("json-named-else", "one"), out var machine));
        await machine.SendAsync(new Initialize());
        var factory = ServiceProvider.GetRequiredService<IEventFactory>();
        Assert.AreEqual(EventStatus.NotConsumed, (await machine.SendAsync(factory.CreateFromJson("other", "{}"))).Status);
        Assert.AreEqual(EventStatus.Consumed,
            (await machine.SendAsync(factory.CreateFromJson("orderSubmitted", """{"quantity":0}"""))).Status);
        Assert.AreEqual("done", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
        CollectionAssert.AreEqual(new[] { "orderSubmitted" }, trace.Entries.ToArray());
    }

    [TestMethod]
    public async Task MixedInternalTransitionsPreserveStateAndUseTypedHandlers()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("json-named-internal", "one"), out var machine));
        await machine.SendAsync(new Initialize());
        var factory = ServiceProvider.GetRequiredService<IEventFactory>();
        foreach (var name in new[] { "orderSubmitted", "orderCancelled" })
            Assert.AreEqual(EventStatus.Consumed,
                (await machine.SendAsync(factory.CreateFromJson(name, """{"quantity":2}"""))).Status);
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Ping())).Status);
        Assert.AreEqual("waiting", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
        CollectionAssert.AreEqual(new[] { "orderSubmitted", "orderCancelled", "ping" }, trace.Entries.ToArray());
    }

    [TestMethod]
    public async Task NamedAcceptNodeExecutesConfiguredStructuralImplementation()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("json-named-activity", "one"), out var activity));
        await activity.SendAsync(new Initialize());
        var factory = ServiceProvider.GetRequiredService<IEventFactory>();
        Assert.AreEqual(EventStatus.NotConsumed,
            (await activity.SendAsync(factory.CreateFromJson("orderCancelled", """{"quantity":2}"""))).Status);
        Assert.AreEqual(EventStatus.Consumed,
            (await activity.SendAsync(factory.CreateFromJson("orderSubmitted", """{"quantity":7}"""))).Status);
        CollectionAssert.AreEqual(new[] { "accepted:orderSubmitted:7" }, trace.Entries.ToArray());
        Assert.AreEqual(BehaviorStatus.Finalized, (await activity.GetStatusAsync()).Response.BehaviorStatus);
    }

    [TestMethod]
    public async Task NamedDescriptorsReachBehaviorVisitorsWithoutWildcardSubstitution()
    {
        var machine = new MachineVisitor();
        await ServiceProvider.GetRequiredService<IStateMachinesRegister>().VisitStateMachineAsync("json-named", 1, machine);
        CollectionAssert.AreEquivalent(new[] { "orderSubmitted", "orderCancelled" }, machine.Named.Select(e => e.Name).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(Ping) }, machine.Clr.ToArray());
        Assert.IsTrue(machine.Named.All(e => e.Fields["quantity"].Type == FieldType.Int32));
        var activity = new AcceptVisitor();
        await ((Stateflows.Activities.Registration.ActivitiesRegister)ServiceProvider.GetRequiredService<IActivitiesRegister>())
            .VisitActivityAsync("json-named-activity", 1, activity);
        Assert.AreEqual("orderSubmitted", activity.Named.Single().Name);
        Assert.AreEqual(FieldType.Int32, activity.Named.Single().Fields["quantity"].Type);
    }

    [TestMethod]
    [DataRow("""{"eventName":"orderSubmitted","clrType":"ping"}""")]
    [DataRow("""{"eventName":""}""")]
    [DataRow("""{}""")]
    public void InvalidTransitionReferenceShapesFailSchema(string reference)
    {
        var document = Machine();
        document["vertices"]![0]!["transitions"]![0]!["triggers"] = new JArray(JObject.Parse(reference));
        var exception = Assert.Throws<JsonStateMachineException>(() =>
            JsonStateMachineDefinition.Parse(document.ToString(), Options()));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.RuleId == "JSON-002"));
    }

    [TestMethod]
    [DataRow("both")]
    [DataRow("neither")]
    [DataRow("empty")]
    public void InvalidAcceptReferenceShapesFailSchema(string mutation)
    {
        var document = Activity();
        var node = (JObject)document["nodes"]![0]!;
        if (mutation == "both") node["eventType"] = "ping";
        if (mutation == "neither") node.Remove("eventName");
        if (mutation == "empty") node["eventName"] = "";
        var exception = Assert.Throws<JsonActivityException>(() => JsonActivityDefinition.Parse(document.ToString(), Options()));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.RuleId == "JSON-002"));
    }

    [TestMethod]
    public void UnknownNamesFailBindingBeforeBehaviorRegistration()
    {
        var builder = new StateflowsBuilder(new ServiceCollection());
        var machine = Machine();
        machine["vertices"]![0]!["transitions"]![0]!["triggers"] = JArray.Parse("""[{"eventName":"missing"}]""");
        var machineException = Assert.Throws<JsonStateMachineException>(() => builder.AddStateMachines(machines =>
            JsonStateMachineDefinition.Parse(machine.ToString(), Options()).Register(machines)));
        Assert.IsTrue(machineException.ValidationReport.Diagnostics.Any(d =>
            d.RuleId == "JSON-003" && d.ElementPath.EndsWith("/triggers/0/eventName")));
        var activityException = Assert.Throws<JsonActivityException>(() => builder.AddActivities(activities =>
            JsonActivityDefinition.Parse(Activity().ToString(), Options()).Register(activities)));
        Assert.IsTrue(activityException.ValidationReport.Diagnostics.Any(d =>
            d.RuleId == "JSON-003" && d.ElementPath.EndsWith("/eventName")));
    }

    [TestMethod]
    public void DuplicateStructuralTriggersFailBinding()
    {
        var builder = new StateflowsBuilder(new ServiceCollection());
        Events(builder);
        var document = Machine();
        document["vertices"]![0]!["transitions"]![0]!["triggers"] =
            JArray.Parse("""[{"eventName":"orderSubmitted"},{"eventName":"orderSubmitted"}]""");
        var exception = Assert.Throws<JsonStateMachineException>(() => builder.AddStateMachines(machines =>
            JsonStateMachineDefinition.Parse(document.ToString(), Options()).Register(machines)));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.RuleId == "JSON-003"));
    }

    [TestMethod]
    public void MixedNamedElseTriggersFailBindingInsteadOfOmittingTriggers()
    {
        var builder = new StateflowsBuilder(new ServiceCollection());
        Events(builder);
        var document = Machine();
        var transition = (JObject)document["vertices"]![0]!["transitions"]![0]!;
        transition["else"] = true;
        transition.Remove("guard");
        var exception = Assert.Throws<JsonStateMachineException>(() => builder.AddStateMachines(machines =>
            JsonStateMachineDefinition.Parse(document.ToString(), Options()).Register(machines)));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d =>
            d.RuleId == "JSON-003" && d.Message.Contains("one else edge")));
    }

    [TestMethod]
    public void NamedAcceptRequiresStructuralHandlerContract()
    {
        var builder = new StateflowsBuilder(new ServiceCollection());
        Events(builder);
        var options = new JsonOptions().AddType<Effect>("receive");
        var exception = Assert.Throws<JsonActivityException>(() => builder.AddActivities(activities =>
            JsonActivityDefinition.Parse(Activity().ToString(), options).Register(activities)));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d =>
            d.RuleId == "JSON-003" && d.ElementPath.EndsWith("/implementation")));
    }
}
