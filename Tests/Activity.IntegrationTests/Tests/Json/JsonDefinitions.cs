using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Stateflows.Activities.Json;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Json;
using Stateflows.Common.Registration.Builders;

namespace Activity.IntegrationTests.Tests.Json;

[TestClass]
[TestCategory("JsonDefinitions")]
public class JsonDefinitions
{
    internal const string Minimal = """
        {
          "formatVersion": "1.0",
          "name": "json-activity",
          "nodes": [
            { "kind": "initial", "flows": [{ "kind": "control", "target": "action" }] },
            { "kind": "action", "name": "action", "implementation": { "clrType": "action" },
              "flows": [{ "kind": "control", "target": "done" }] },
            { "kind": "final", "name": "done" }
          ]
        }
        """;

    internal static JsonOptions Options() => new JsonOptions()
        .AddType<NoOp>("action")
        .AddType<Definition>("definition")
        .AddType<Configured>("configured")
        .AddType<DualConfiguration>("dual")
        .AddType<AbstractAction>("abstract")
        .AddType<NoPublicConstructor>("private")
        .AddType<int>("int")
        .AddType<string>("string");

    internal static (ActivitiesRegister Register, ActivitiesBuilder Builder) Host()
    {
        var services = new StateflowsBuilder(new ServiceCollection());
        var register = (ActivitiesRegister)services.EnsureActivitiesServices();
        return (register, new ActivitiesBuilder(register));
    }

    internal static JObject Model() => JObject.Parse(Minimal);
    internal static JObject Action(JObject model) => (JObject)model["nodes"]![1]!;

    [TestMethod]
    public void MinimalModelRegistersWithResolvedSpecialNodeAlias()
    {
        var (register, builder) = Host();
        var definition = JsonActivityDefinition.Parse(Minimal, Options());
        Assert.IsFalse(definition.Register(builder).HasErrors);
        var graph = register.Activities["json-activity.1"];
        Assert.AreEqual(3, graph.AllNodes.Count);
        Assert.AreEqual(2, graph.AllEdges.Count);
        Assert.AreEqual(NodeType.Final, graph.AllEdgesList.Single(edge => edge.SourceName == "action").Target.Type);
        Assert.AreSame(graph, register.Activities["json-activity.current"]);
    }

    [TestMethod]
    [DataRow("""{"formatVersion":"1.0","name":"a","name":"b","nodes":[]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"a","nodes":[{"kind":"action","name":"a","implementation":{"clrType":"action","clrType":"action"}}]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"a","nodes":[],}""")]
    [DataRow("""{"formatVersion":"1.0","name":"a","nodes":[]} {}""")]
    [DataRow("""{"formatVersion":"1.0",/*comment*/"name":"a","nodes":[]}""")]
    public void StrictParsingRejectsInvalidJson(string json)
    {
        var exception = Assert.Throws<JsonActivityException>(() => JsonActivityDefinition.Parse(json, Options()));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.RuleId == "JSON-001"));
    }

    [TestMethod]
    [DataRow("unknown")]
    [DataRow("version")]
    [DataRow("formatVersion")]
    [DataRow("missingImplementation")]
    [DataRow("unknownNode")]
    [DataRow("finalFlows")]
    [DataRow("initialOptions")]
    [DataRow("negativeWeight")]
    [DataRow("zeroChunk")]
    [DataRow("elseGuard")]
    [DataRow("controlTransformation")]
    [DataRow("missingOutputType")]
    [DataRow("configurationTypeWithoutValue")]
    [DataRow("unknownBindingProperty")]
    [DataRow("nullNodes")]
    public void SchemaRejectsInvalidShapes(string mutation)
    {
        var model = Model();
        var action = Action(model);
        switch (mutation)
        {
            case "unknown": model["metadata"] = new JObject(); break;
            case "version": model["version"] = 0; break;
            case "formatVersion": model["formatVersion"] = "2.0"; break;
            case "missingImplementation": action.Remove("implementation"); break;
            case "unknownNode": action["kind"] = "unknown"; break;
            case "finalFlows": model["nodes"]![2]!["flows"] = new JArray(); break;
            case "initialOptions": model["nodes"]![0]!["options"] = new JArray(); break;
            case "negativeWeight": action["flows"]![0]!["weight"] = -1; break;
            case "zeroChunk":
                model["nodes"] = JArray.Parse("""[{"kind":"parallel","name":"p","iterationTokenType":"int","chunkSize":0,"nodes":[]}]""");
                break;
            case "elseGuard":
                action["flows"] = JArray.Parse("""[{"kind":"elseObject","tokenType":"int","target":"done","pipeline":[{"kind":"guard","handler":{"clrType":"action"}}]}]""");
                break;
            case "controlTransformation":
                action["flows"]![0]!["pipeline"] = JArray.Parse("""[{"kind":"transform","outputTokenType":"int","handler":{"clrType":"action"}}]""");
                break;
            case "missingOutputType":
                action["flows"] = JArray.Parse("""[{"kind":"object","tokenType":"int","target":"done","pipeline":[{"kind":"transform","handler":{"clrType":"action"}}]}]""");
                break;
            case "configurationTypeWithoutValue": action["implementation"]!["configurationType"] = "string"; break;
            case "unknownBindingProperty": action["implementation"]!["assembly"] = "untrusted"; break;
            case "nullNodes": model["nodes"] = JValue.CreateNull(); break;
        }
        var exception = Assert.Throws<JsonActivityException>(() => JsonActivityDefinition.Parse(model.ToString(), Options()));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.RuleId == "JSON-002"));
    }

    [TestMethod]
    public void AllBindingsAreValidatedBeforeDefinitionsRun()
    {
        var (register, builder) = Host();
        var model = Model();
        model["definitions"] = JArray.Parse("""[{"clrType":"definition"}]""");
        Action(model)["implementation"] = JObject.Parse("""{"clrType":"missing"}""");
        Action(model)["flows"] = JArray.Parse("""[{"kind":"object","tokenType":"missingToken","target":"done"}]""");
        var before = Definition.Builds;
        var exception = Assert.Throws<JsonActivityException>(() => builder.AddActivityJson(model.ToString(), Options()));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Count(d => d.RuleId == "JSON-003") >= 2);
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.ElementPath == "/nodes/1/implementation/clrType"));
        Assert.AreEqual(before, Definition.Builds);
        Assert.IsEmpty(register.Activities);
        Assert.IsEmpty(register.CurrentVersions);
    }

    [TestMethod]
    [DataRow("unknownAlias", "JSON-003")]
    [DataRow("wrongRole", "JSON-003")]
    [DataRow("privateConstructor", "JSON-003")]
    [DataRow("abstract", "JSON-003")]
    [DataRow("staticConfiguration", "JSON-003")]
    [DataRow("wrongDefinition", "JSON-003")]
    [DataRow("ambiguousConfiguration", "JSON-003")]
    [DataRow("wrongConfigurationType", "JSON-003")]
    [DataRow("missingConfigurationContract", "JSON-003")]
    [DataRow("badConfiguration", "JSON-004")]
    [DataRow("unknownTypedConfiguration", "JSON-004")]
    public void BindingRejectsInvalidCapabilitiesAndConfiguration(string mutation, string rule)
    {
        var (register, builder) = Host();
        var model = Model();
        var implementation = (JObject)Action(model)["implementation"]!;
        switch (mutation)
        {
            case "unknownAlias": implementation["clrType"] = "missing"; break;
            case "wrongRole": implementation["clrType"] = "string"; break;
            case "privateConstructor": implementation["clrType"] = "private"; break;
            case "abstract": implementation["clrType"] = "abstract"; break;
            case "wrongDefinition": model["definitions"] = JArray.Parse("""[{"clrType":"action"}]"""); break;
            case "staticConfiguration": model["definitions"] = JArray.Parse("""[{"clrType":"definition","configuration":{}}]"""); break;
            case "ambiguousConfiguration":
                implementation["clrType"] = "dual";
                implementation["configuration"] = 1;
                break;
            case "wrongConfigurationType":
                implementation["clrType"] = "configured";
                implementation["configurationType"] = "int";
                implementation["configuration"] = 1;
                break;
            case "missingConfigurationContract": implementation["configuration"] = "text"; break;
            case "badConfiguration":
                implementation["clrType"] = "configured";
                implementation["configuration"] = new JObject { ["unknown"] = true };
                break;
            case "unknownTypedConfiguration":
                implementation["clrType"] = "configured";
                implementation["configuration"] = JObject.Parse("""{"$type":"missing","$value":"text"}""");
                break;
        }
        var exception = Assert.Throws<JsonActivityException>(() => builder.AddActivityJson(model.ToString(), Options()));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.RuleId == rule));
        Assert.IsEmpty(register.Activities);
    }

    [TestMethod]
    public void SelectingActionDoesNotInvokeItsStaticDefinition()
    {
        var (_, builder) = Host();
        var model = Model();
        Action(model)["implementation"]!["clrType"] = "definition";
        var before = Definition.Builds;
        builder.AddActivityJson(model.ToString(), Options());
        Assert.AreEqual(before, Definition.Builds);
    }

    [TestMethod]
    public void DefinitionsContributeBeforeJsonWithoutReplacingNodes()
    {
        var (register, builder) = Host();
        var model = Model();
        model["definitions"] = JArray.Parse("""[{"clrType":"definition"}]""");
        model["nodes"]![0]!["flows"]![0]!["target"] = "fromDefinition";
        var before = Definition.Builds;
        builder.AddActivityJson(model.ToString(), Options());
        Assert.AreEqual(before + 1, Definition.Builds);
        Assert.AreEqual(4, register.Activities["json-activity.1"].AllNodes.Count);
    }

    [TestMethod]
    public void DefinitionCollisionsPreventPublication()
    {
        var (register, builder) = Host();
        var model = Model();
        model["definitions"] = JArray.Parse("""[{"clrType":"definition"}]""");
        Action(model)["name"] = "fromDefinition";
        var exception = Assert.Throws<JsonActivityException>(() => builder.AddActivityJson(model.ToString(), Options()));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.RuleId == "JSON-005"));
        Assert.IsEmpty(register.Activities);
    }

    [TestMethod]
    [DataRow("duplicate")]
    [DataRow("duplicateSpecial")]
    [DataRow("wrongFlowKind")]
    [DataRow("elseOnAction")]
    [DataRow("decisionTokenMismatch")]
    [DataRow("nonreactiveEvent")]
    [DataRow("nonException")]
    public void BindingRejectsUnsupportedNodeAndFlowCombinations(string mutation)
    {
        var (register, builder) = Host();
        var model = Model();
        var action = Action(model);
        switch (mutation)
        {
            case "duplicate": ((JArray)model["nodes"]!).Add(action.DeepClone()); break;
            case "duplicateSpecial": ((JArray)model["nodes"]!).Add(model["nodes"]![0]!.DeepClone()); break;
            case "wrongFlowKind": model["nodes"]![0]!["flows"]![0]!["kind"] = "object"; model["nodes"]![0]!["flows"]![0]!["tokenType"] = "int"; break;
            case "elseOnAction": action["flows"]![0]!["kind"] = "elseControl"; break;
            case "decisionTokenMismatch":
                model["nodes"] = JArray.Parse("""[{"kind":"decision","name":"d","mode":"object","tokenType":"int","flows":[{"kind":"object","tokenType":"string","target":"done"}]}]""");
                break;
            case "nonreactiveEvent":
                model["nodes"] = JArray.Parse("""[{"kind":"parallel","name":"p","iterationTokenType":"int","nodes":[{"kind":"acceptEvent","name":"accept","eventType":"int","implementation":{"clrType":"action"}}]}]""");
                break;
            case "nonException":
                action["exceptionHandlers"] = JArray.Parse("""[{"exceptionType":"int","handler":{"clrType":"action"}}]""");
                break;
        }
        Assert.Throws<JsonActivityException>(() => builder.AddActivityJson(model.ToString(), Options()));
        Assert.IsEmpty(register.Activities);
    }

    [TestMethod]
    public void AliasRegistryIsSnapshottedAtParseTime()
    {
        var (register, builder) = Host();
        var options = new JsonOptions();
        var definition = JsonActivityDefinition.Parse(Minimal, options);
        options.AddType<NoOp>("action");
        Assert.Throws<JsonActivityException>(() => definition.Register(builder));
        Assert.IsEmpty(register.Activities);
        builder.AddActivityJson(Minimal, options);
        Assert.IsTrue(register.Activities.ContainsKey("json-activity.1"));
    }

    [TestMethod]
    public void FailedNewVersionDoesNotReplacePublishedGraph()
    {
        var (register, builder) = Host();
        builder.AddActivityJson(Minimal, Options());
        var original = register.Activities["json-activity.current"];
        var model = Model();
        model["version"] = 2;
        Action(model)["flows"]![0]!["target"] = "missing";
        Assert.Throws<JsonActivityException>(() => builder.AddActivityJson(model.ToString(), Options()));
        Assert.AreSame(original, register.Activities["json-activity.current"]);
        Assert.AreEqual(1, register.CurrentVersions["json-activity"]);
        Assert.IsFalse(register.Activities.ContainsKey("json-activity.2"));
        Action(model)["flows"]![0]!["target"] = "done";
        builder.AddActivityJson(model.ToString(), Options());
        Assert.AreEqual(2, register.CurrentVersions["json-activity"]);
        Assert.AreNotSame(original, register.Activities["json-activity.current"]);
    }

    [TestMethod]
    public void DuplicateVersionIsRejectedWithoutChangingCurrent()
    {
        var (register, builder) = Host();
        builder.AddActivityJson(Minimal, Options());
        var original = register.Activities["json-activity.current"];
        Assert.Throws<JsonActivityException>(() => builder.AddActivityJson(Minimal, Options()));
        Assert.AreSame(original, register.Activities["json-activity.current"]);
    }

    [TestMethod]
    public async Task JsonRegistrationUsesBuilderVisitorNotifications()
    {
        var (register, builder) = Host();
        var model = Model();
        model["defaultInitializers"] = JArray.Parse("""[{"clrType":"defaultInitializer"}]""");
        model["finalizers"] = JArray.Parse("""[{"clrType":"finalizer"}]""");
        builder.AddActivityJson(model.ToString(), Options()
            .AddType<DefaultInitializer>("defaultInitializer").AddType<Finalizer>("finalizer"));
        var visitor = new Visitor();
        await register.VisitActivityAsync("json-activity", 1, visitor);
        CollectionAssert.AreEqual(new[] { NodeType.Initial, NodeType.Action, NodeType.Final }, visitor.Nodes.ToArray());
        Assert.AreEqual(2, visitor.ControlFlows);
        CollectionAssert.AreEqual(new[] { typeof(NoOp) }, visitor.NodeTypes.ToArray());
        Assert.AreEqual(1, visitor.DefaultInitializers);
        Assert.AreEqual(1, visitor.Finalizers);
    }

    [TestMethod]
    public void UnknownTargetPreventsPublicationAndReportsCompositionFailure()
    {
        var (register, builder) = Host();
        var model = Model();
        Action(model)["flows"]![0]!["target"] = "missing";
        var exception = Assert.Throws<JsonActivityException>(() => builder.AddActivityJson(model.ToString(), Options()));
        Assert.IsTrue(exception.ValidationReport.HasErrors);
        StringAssert.Contains(exception.Message, "missing");
        Assert.IsEmpty(register.Activities);
    }

    [TestMethod]
    public void InvalidFlowStageCapabilityIsRejectedBeforeConstruction()
    {
        var (register, builder) = Host();
        var model = Model();
        Action(model)["flows"] = JArray.Parse("""
            [{"kind":"object","tokenType":"int","target":"done",
              "pipeline":[{"kind":"guard","handler":{"clrType":"action"}},
                          {"kind":"transform","outputTokenType":"string","handler":{"clrType":"configured"}}]}]
            """);
        var exception = Assert.Throws<JsonActivityException>(() => builder.AddActivityJson(model.ToString(), Options()));
        Assert.AreEqual(2, exception.ValidationReport.Diagnostics.Count(d => d.RuleId == "JSON-003"));
        Assert.IsEmpty(register.Activities);
    }

    [TestMethod]
    public async Task AcceptEventVisitorRetainsAuthoredEventType()
    {
        var (register, builder) = Host();
        builder.AddActivityJson("""
            {"formatVersion":"1.0","name":"event-visitor","nodes":[
              {"kind":"acceptEvent","name":"accept","eventType":"int","implementation":{"clrType":"accept"},
               "flows":[{"kind":"control","target":"done"}]},
              {"kind":"final","name":"done"}]}
            """, Options().AddType<Accept>("accept"));
        var visitor = new Visitor();
        await register.VisitActivityAsync("event-visitor", 1, visitor);
        CollectionAssert.AreEqual(new[] { typeof(int) }, visitor.EventTypes.ToArray());
        CollectionAssert.AreEqual(new[] { NodeType.AcceptEventAction, NodeType.Final }, visitor.Nodes.ToArray());
    }

    [TestMethod]
    public void TimeEventRegistersItsEventTypeAndConfiguredImplementation()
    {
        var (register, builder) = Host();
        builder.AddActivityJson("""
            {"formatVersion":"1.0","name":"time-event","nodes":[
              {"kind":"timeEvent","name":"timer","eventType":"timer","implementation":{"clrType":"timeAction","configuration":"tick"},
               "flows":[{"kind":"control","target":"done"}]},
              {"kind":"final","name":"done"}]}
            """, Options().AddType<Timer>("timer").AddType<TimeAction>("timeAction"));
        var node = register.Activities["time-event.1"].AllNodes.Values.Single(n => n.Type == NodeType.TimeEventAction);
        Assert.AreEqual(typeof(Timer), node.EventType);
    }

    [TestMethod]
    public void TimeEventRejectsNonTimeEventTypeBeforePublication()
    {
        var (register, builder) = Host();
        var exception = Assert.Throws<JsonActivityException>(() => builder.AddActivityJson("""
            {"formatVersion":"1.0","name":"time-event","nodes":[
              {"kind":"timeEvent","name":"timer","eventType":"int","implementation":{"clrType":"timeAction"}}]}
            """, Options().AddType<TimeAction>("timeAction")));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.ElementPath == "/nodes/0/eventType" && d.RuleId == "JSON-003"));
        Assert.IsEmpty(register.Activities);
    }

    public sealed class Visitor : ActivityVisitor
    {
        public List<NodeType> Nodes { get; } = [];
        public List<Type> NodeTypes { get; } = [];
        public List<Type> EventTypes { get; } = [];
        public int ControlFlows { get; private set; }
        public int DefaultInitializers { get; private set; }
        public int Finalizers { get; private set; }

        public override Task NodeAddedAsync(string activityName, int activityVersion, string nodeName, NodeType nodeType, string? parentNodeName = null)
        {
            Nodes.Add(nodeType);
            return Task.CompletedTask;
        }

        public override Task NodeTypeAddedAsync<TNode>(string activityName, int activityVersion, string nodeName)
        {
            NodeTypes.Add(typeof(TNode));
            return Task.CompletedTask;
        }

        public override Task AcceptEventNodeAddedAsync<TEvent>(string activityName, int activityVersion, string nodeName, string? parentNodeName = null)
        {
            EventTypes.Add(typeof(TEvent));
            return Task.CompletedTask;
        }

        public override Task ControlFlowAddedAsync(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false)
        {
            ControlFlows++;
            return Task.CompletedTask;
        }

        public override Task DefaultInitializerAddedAsync(string activityName, int activityVersion)
        {
            DefaultInitializers++;
            return Task.CompletedTask;
        }

        public override Task FinalizerAddedAsync(string activityName, int activityVersion)
        {
            Finalizers++;
            return Task.CompletedTask;
        }
    }

    public sealed class DefaultInitializer : IDefaultInitializer
    {
        public Task<bool> OnInitializeAsync() => Task.FromResult(true);
    }

    public sealed class Finalizer : IFinalizer
    {
        public Task OnFinalizeAsync() => Task.CompletedTask;
    }

    public sealed class Accept : IAcceptEventActionNode<int>
    {
        public Task ExecuteAsync(int @event, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public sealed class Timer : TimeEvent
    {
        protected override DateTime GetTriggerTime(DateTime startedAt) => startedAt.AddDays(1);
    }

    public sealed class TimeAction : ITimeEventActionNode, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public sealed class NoOp : IActionNode
    {
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public abstract class AbstractAction : IActionNode
    {
        public abstract Task ExecuteAsync(CancellationToken cancellationToken);
    }

    public sealed class NoPublicConstructor : IActionNode
    {
        private NoPublicConstructor() { }
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public sealed class Configured : IActionNode, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public sealed class DualConfiguration : IActionNode, IConfigurable<string>, IConfigurable<int>
    {
        string IConfigurable<string>.Configuration { set { } }
        int IConfigurable<int>.Configuration { set { } }
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public sealed class Definition : IActivity, IActionNode
    {
        public static int Builds { get; private set; }
        public static void Build(IActivityBuilder builder)
        {
            Builds++;
            builder.AddAction<NoOp>("fromDefinition", action => action.AddControlFlow("action"));
        }
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
