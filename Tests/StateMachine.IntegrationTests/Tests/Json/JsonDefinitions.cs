using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OneOf;
using Stateflows.Common;
using Stateflows.Common.Json;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Validation;
using Stateflows.StateMachines.Json;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;

namespace StateMachine.IntegrationTests.Tests.Json;

[TestClass]
[TestCategory("JsonDefinitions")]
public class JsonDefinitions
{
    internal const string Minimal = """
        {
          "formatVersion": "1.0",
          "name": "json-machine",
          "initial": "start",
          "vertices": [
            { "kind": "state", "name": "start" },
            { "kind": "final", "name": "done" }
          ]
        }
        """;

    internal static JsonOptions Options() => new JsonOptions()
        .AddType<Ping>("ping").AddType<Pong>("pong")
        .AddType<CapableState>("state").AddType<MachineDefinition>("machine")
        .AddType<EntryOnly>("entry").AddType<MultiConfiguration>("multi")
        .AddType<string>("string").AddType<int>("int").AddType<int[]>("ints")
        .AddType<Display>("display").AddType<TransitionDefinition>("transition")
        .AddType<Guard>("guard").AddType<OneOf<Ping, Pong>>("union")
        .AddType<UnionGuard>("unionGuard").AddType<Completion>("completion");

    internal static (StateflowsBuilder Services, StateMachinesRegister Register, StateMachinesBuilder Builder) Host()
    {
        var services = new StateflowsBuilder(new ServiceCollection());
        var register = (StateMachinesRegister)services.EnsureStateMachinesServices();
        return (services, register, new StateMachinesBuilder(register));
    }

    internal static JObject Model() => JObject.Parse(Minimal);
    internal static JObject State(JObject model) => (JObject)model["vertices"]![0]!;

    [TestMethod]
    public void MinimalModelRegistersAndResolvesInitial()
    {
        var (_, register, builder) = Host();
        var definition = JsonStateMachineDefinition.Parse(Minimal, Options());
        Assert.IsFalse(definition.Register(builder).HasErrors);
        var graph = register.StateMachines["json-machine.1"];
        Assert.AreEqual(VertexType.InitialState, graph.InitialVertex.Type);
        Assert.AreEqual("start", graph.InitialVertex.Name);
        Assert.AreEqual(2, graph.AllVertices.Count);
    }

    [TestMethod]
    [DataRow("""{"formatVersion":"1.0","name":"a","name":"b"}""")]
    [DataRow("""{"formatVersion":"1.0","name":"a","vertices":[{"kind":"state","name":"x","metadata":{"x":1,"x":2}}]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"a",}""")]
    [DataRow("""{"formatVersion":"1.0","name":"a"} {}""")]
    public void StrictParsingRejectsDuplicatePropertiesAndInvalidJson(string json)
    {
        var exception = Assert.Throws<JsonStateMachineException>(() => JsonStateMachineDefinition.Parse(json, Options()));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.RuleId == "JSON-001"));
    }

    [TestMethod]
    [DataRow("unknown")]
    [DataRow("formatVersion")]
    [DataRow("nullHandlers")]
    [DataRow("stateRegions")]
    [DataRow("finalHandlers")]
    [DataRow("internalTarget")]
    [DataRow("emptyTriggers")]
    [DataRow("defaultTriggers")]
    [DataRow("elseGuard")]
    [DataRow("mixedHandler")]
    [DataRow("regionBehavior")]
    [DataRow("identityConfig")]
    public void SchemaRejectsInvalidShapes(string mutation)
    {
        var model = Model();
        var state = State(model);
        var transition = JObject.Parse("""{"kind":"event","triggers":[{"clrType":"ping"}],"target":"done"}""");
        switch (mutation)
        {
            case "unknown": model["typo"] = true; break;
            case "formatVersion": model["formatVersion"] = "2.0"; break;
            case "nullHandlers": state["onEntry"] = JValue.CreateNull(); break;
            case "stateRegions": state["regions"] = new JArray(); break;
            case "finalHandlers": model["vertices"]![1]!["onEntry"] = new JArray(); break;
            case "internalTarget": transition["kind"] = "internal"; break;
            case "emptyTriggers": transition["triggers"] = new JArray(); break;
            case "defaultTriggers": transition["kind"] = "default"; break;
            case "elseGuard": transition["else"] = true; transition["guard"] = JObject.Parse("""{"clrType":"guard"}"""); break;
            case "mixedHandler": state["onEntry"] = JArray.Parse("""[{"clrType":"entry","behaviorClass":{"type":"Action","name":"a"}}]"""); break;
            case "regionBehavior":
                state["kind"] = "composite";
                state["region"] = JObject.Parse("""{"do":{"behaviorClass":{"type":"Action","name":"a"}}}""");
                break;
            case "identityConfig": state["name"] = JObject.Parse("""{"clrType":"state","configuration":{}}"""); break;
        }
        state["transitions"] = new JArray(transition);
        var exception = Assert.Throws<JsonStateMachineException>(() => JsonStateMachineDefinition.Parse(model.ToString(), Options()));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.RuleId == "JSON-002"));
    }

    [TestMethod]
    public void AllClrBindingsAreValidatedBeforeAnyDefinitionRuns()
    {
        var (_, register, builder) = Host();
        var model = Model();
        model["definitions"] = JArray.Parse("""[{"clrType":"machine"}]""");
        State(model)["onEntry"] = JArray.Parse("""[{"clrType":"missing"},{"clrType":"ping"}]""");
        var before = MachineDefinition.Builds;
        var exception = Assert.Throws<JsonStateMachineException>(() => builder.AddStateMachineJson(model.ToString(), Options()));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Count(d => d.RuleId == "JSON-003") >= 2);
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.ElementPath == "/vertices/0/onEntry/0/clrType"));
        Assert.AreEqual(before, MachineDefinition.Builds);
        Assert.IsEmpty(register.StateMachines);
    }

    [TestMethod]
    public void IdentityDefinitionAndHandlerSelectionsAreIndependent()
    {
        var (_, register, builder) = Host();
        var model = Model();
        State(model)["name"] = JObject.Parse("""{"clrType":"state"}""");
        model["initial"] = JObject.Parse("""{"clrType":"state"}""");
        var before = CapableState.Builds;
        builder.AddStateMachineJson(model.ToString(), Options());
        var vertex = register.StateMachines["json-machine.1"].InitialVertex;
        Assert.AreEqual(State<CapableState>.Name, vertex.Name);
        Assert.IsFalse(vertex.Entry.Any);
        Assert.IsFalse(vertex.Exit.Any);
        Assert.AreEqual(before, CapableState.Builds);
    }

    [TestMethod]
    public void SelectingDefinitionDoesNotSelectItsHandlers()
    {
        var (_, register, builder) = Host();
        var model = Model();
        State(model)["definitions"] = JArray.Parse("""[{"clrType":"state"}]""");
        var before = CapableState.Builds;
        builder.AddStateMachineJson(model.ToString(), Options());
        Assert.AreEqual(before + 1, CapableState.Builds);
        Assert.IsFalse(register.StateMachines["json-machine.1"].InitialVertex.Entry.Any);
    }

    [TestMethod]
    public void SelectingEntryDoesNotSelectExitOrDefinition()
    {
        var (_, register, builder) = Host();
        var model = Model();
        State(model)["onEntry"] = JArray.Parse("""[{"clrType":"state"}]""");
        var before = CapableState.Builds;
        builder.AddStateMachineJson(model.ToString(), Options());
        var vertex = register.StateMachines["json-machine.1"].InitialVertex;
        Assert.IsTrue(vertex.Entry.Any);
        Assert.IsFalse(vertex.Exit.Any);
        Assert.AreEqual(before, CapableState.Builds);
    }

    [TestMethod]
    [DataRow("name")]
    [DataRow("definition")]
    [DataRow("staticConfiguration")]
    [DataRow("badConfiguration")]
    [DataRow("ambiguousConfiguration")]
    [DataRow("wrongConfigurationType")]
    [DataRow("missingConfigContract")]
    [DataRow("typedConfigurationAlias")]
    public void BindingRejectsInvalidCapabilitiesAndConfiguration(string mutation)
    {
        var (_, register, builder) = Host();
        var model = Model();
        var state = State(model);
        switch (mutation)
        {
            case "name": state["name"] = JObject.Parse("""{"clrType":"ping"}"""); break;
            case "definition": state["definitions"] = JArray.Parse("""[{"clrType":"entry"}]"""); break;
            case "staticConfiguration": state["definitions"] = JArray.Parse("""[{"clrType":"state","configuration":{}}]"""); break;
            case "badConfiguration": state["onEntry"] = JArray.Parse("""[{"clrType":"entry","configuration":{"bad":true}}]"""); break;
            case "ambiguousConfiguration": state["onEntry"] = JArray.Parse("""[{"clrType":"multi","configuration":1}]"""); break;
            case "wrongConfigurationType": state["onEntry"] = JArray.Parse("""[{"clrType":"entry","configurationType":"int","configuration":1}]"""); break;
            case "missingConfigContract": state["onEntry"] = JArray.Parse("""[{"clrType":"state","configuration":{}}]"""); break;
            case "typedConfigurationAlias": state["onEntry"] = JArray.Parse("""[{"clrType":"entry","configuration":{"$type":"notRegistered","$value":"a"}}]"""); break;
        }
        Assert.Throws<JsonStateMachineException>(() => builder.AddStateMachineJson(model.ToString(), Options()));
        Assert.IsEmpty(register.StateMachines);
    }

    [TestMethod]
    public void ExplicitConfigurationContractIsAccepted()
    {
        var (_, register, builder) = Host();
        var model = Model();
        State(model)["onEntry"] = JArray.Parse("""[{"clrType":"multi","configurationType":"int","configuration":4}]""");
        builder.AddStateMachineJson(model.ToString(), Options());
        Assert.IsTrue(register.StateMachines["json-machine.1"].InitialVertex.Entry.Any);
    }

    [TestMethod]
    public void MetadataDeserializesObjectsScalarsCollectionsAndNestedDiscriminators()
    {
        var (_, register, builder) = Host();
        var model = Model();
        State(model)["metadata"] = JObject.Parse("""
            {
              "text":"hello",
              "number":{"$type":"int","$value":42},
              "array":{"$type":"ints","$value":[1,2]},
              "display":{"$type":"display","Label":"Start","Child":{"$type":"display","Label":"Nested"}},
              "plain":{"nested":{"$type":"display","Label":"Nested"}},
              "nullable":null
            }
            """);
        builder.AddStateMachineJson(model.ToString(), Options());
        var values = register.StateMachines["json-machine.1"].InitialVertex.Metadata;
        Assert.AreEqual("hello", values["text"]);
        Assert.AreEqual(42, values["number"]);
        CollectionAssert.AreEqual(new[] { 1, 2 }, (int[])values["array"]);
        Assert.AreEqual("Nested", ((Display)((Display)values["display"]).Child!).Label);
        Assert.IsNull(values["nullable"]);
    }

    [TestMethod]
    [DataRow("""{"$type":"missing"}""")]
    [DataRow("""{"$type":"System.IO.FileInfo, System.Private.CoreLib","OriginalPath":"secret"}""")]
    [DataRow("""{"$type":"display","MissingMember":true}""")]
    [DataRow("""{"$ref":"other"}""")]
    public void InvalidTypedMetadataIsRejectedBeforeDefinitionsRun(string value)
    {
        var (_, _, builder) = Host();
        var model = Model();
        model["definitions"] = JArray.Parse("""[{"clrType":"machine"}]""");
        State(model)["metadata"] = new JObject { ["value"] = JToken.Parse(value) };
        var before = MachineDefinition.Builds;
        var exception = Assert.Throws<JsonStateMachineException>(() => builder.AddStateMachineJson(model.ToString(), Options()));
        Assert.AreEqual(before, MachineDefinition.Builds);
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.ElementPath == "/vertices/0/metadata/value"));
    }

    [TestMethod]
    public void MultipleTriggersRemainOneTransitionAndOneDeferralLogic()
    {
        var (_, register, builder) = Host();
        var model = Model();
        State(model)["transitions"] = JArray.Parse("""
            [{"kind":"event","triggers":[{"clrType":"ping"},{"clrType":"pong"}],"target":"done",
              "guard":{"clrType":"guard"},"metadata":{"x":{"$type":"int","$value":3}}}]
            """);
        State(model)["deferrals"] = JArray.Parse("""
            [{"triggers":[{"clrType":"ping"},{"clrType":"pong"}],"guard":{"clrType":"guard"}}]
            """);
        builder.AddStateMachineJson(model.ToString(), Options());
        var graph = register.StateMachines["json-machine.1"];
        Assert.HasCount(1, graph.AllEdges);
        CollectionAssert.AreEquivalent(new[] { typeof(Ping), typeof(Pong) }, graph.AllEdges[0].ActualTriggerTypes.ToArray());
        Assert.AreEqual(3, graph.AllEdges[0].Metadata["x"]);
        Assert.HasCount(2, graph.InitialVertex.Deferrals);
        Assert.AreSame(graph.InitialVertex.Deferrals[Event<Ping>.Name], graph.InitialVertex.Deferrals[Event<Pong>.Name]);
    }

    [TestMethod]
    public void ExistingUnionTypeMappingIsSupported()
    {
        var (services, register, builder) = Host();
        services.AddOneOf();
        var model = Model();
        State(model)["transitions"] = JArray.Parse("""
            [{"kind":"event","triggers":[{"clrType":"union"}],"target":"done","guard":{"clrType":"unionGuard"}}]
            """);
        builder.AddStateMachineJson(model.ToString(), Options());
        CollectionAssert.AreEquivalent(new[] { typeof(Ping), typeof(Pong) },
            register.StateMachines["json-machine.1"].AllEdges[0].ActualTriggerTypes.ToArray());
    }

    [TestMethod]
    [DataRow("duplicateTrigger")]
    [DataRow("wrongGuard")]
    [DataRow("overlap")]
    public void TriggerSetBindingRejectsDuplicatesAndIncompatibleHandlers(string mutation)
    {
        var (services, register, builder) = Host();
        services.AddOneOf();
        var model = Model();
        State(model)["transitions"] = JArray.Parse("""
            [{"kind":"event","triggers":[{"clrType":"ping"},{"clrType":"pong"}],"target":"done"}]
            """);
        var transition = State(model)["transitions"]![0]!;
        if (mutation == "duplicateTrigger") transition["triggers"]![1]!["clrType"] = "ping";
        if (mutation == "overlap") transition["triggers"]![1]!["clrType"] = "union";
        if (mutation == "wrongGuard") transition["guard"] = JObject.Parse("""{"clrType":"entry"}""");
        Assert.Throws<JsonStateMachineException>(() => builder.AddStateMachineJson(model.ToString(), Options()));
        Assert.IsEmpty(register.StateMachines);
    }

    [TestMethod]
    public void InstanceTransitionDefinitionExecutesOnlyRequestedDefinitionWithConfiguration()
    {
        var (_, register, builder) = Host();
        var model = Model();
        State(model)["transitions"] = JArray.Parse("""
            [{"kind":"event","triggers":[{"clrType":"ping"},{"clrType":"pong"}],"target":"done",
              "definitions":[{"clrType":"transition","configuration":"marker"}]}]
            """);
        builder.AddStateMachineJson(model.ToString(), Options());
        var edge = register.StateMachines["json-machine.1"].AllEdges.Single();
        Assert.AreEqual("marker", edge.Metadata["definition"]);
        Assert.IsFalse(edge.Guards.Any);
        Assert.IsFalse(edge.Effects.Any);
    }

    [TestMethod]
    [DataRow("vertices")]
    [DataRow("initial")]
    [DataRow("metadata")]
    public void DefinitionCompositionDoesNotSilentlyReplaceContributions(string collision)
    {
        var (_, register, builder) = Host();
        var model = Model();
        model["definitions"] = JArray.Parse("""[{"clrType":"machine"}]""");
        if (collision == "vertices") State(model)["name"] = "defined";
        if (collision == "initial") model["initial"] = "start";
        if (collision == "metadata")
        {
            model.Remove("definitions");
            State(model)["definitions"] = JArray.Parse("""[{"clrType":"metadataDefinition"}]""");
            State(model)["metadata"] = JObject.Parse("""{"key":2}""");
        }
        Assert.Throws<JsonStateMachineException>(() => builder.AddStateMachineJson(model.ToString(),
            Options().AddType<MetadataDefinition>("metadataDefinition")));
        Assert.IsEmpty(register.StateMachines);
    }

    [TestMethod]
    public void GraphErrorsAccumulateAtAuthoredJsonPathsAndPreventPublication()
    {
        var (_, register, builder) = Host();
        var model = Model();
        State(model)["transitions"] = JArray.Parse("""
            [
              {"kind":"event","triggers":[{"clrType":"ping"}],"target":"missing-one"},
              {"kind":"event","triggers":[{"clrType":"pong"}],"target":"missing-two"}
            ]
            """);
        var exception = Assert.Throws<JsonStateMachineException>(() => builder.AddStateMachineJson(model.ToString(), Options()));
        Assert.AreEqual(2, exception.ValidationReport.Diagnostics.Count(d => d.RuleId == "SM-017"));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.ElementPath == "/vertices/0/transitions/1"));
        Assert.IsEmpty(register.StateMachines);
    }

    [TestMethod]
    public void MultipleFinalStatesAndEmptyCompositeWarningsArePreserved()
    {
        var (_, register, builder) = Host();
        var model = Model();
        ((JArray)model["vertices"]!).Add(JObject.Parse("""{"kind":"final","name":"other","metadata":{"x":1}}"""));
        ((JArray)model["vertices"]!).Add(JObject.Parse("""{"kind":"composite","name":"empty"}"""));
        var definition = JsonStateMachineDefinition.Parse(model.ToString(), Options());
        var report = definition.Register(builder);
        Assert.IsTrue(report.Diagnostics.Any(d => d.Severity == ModelDiagnosticSeverity.Warning && d.ElementPath == "/vertices/3"));
        Assert.HasCount(4, register.StateMachines["json-machine.1"].AllVertices);
    }

    [TestMethod]
    public void MissingBehaviorReferencesAreReportedWithTheirJsonPaths()
    {
        var (_, register, builder) = Host();
        var model = Model();
        State(model)["onEntry"] = JArray.Parse("""[{"behaviorClass":{"type":"Action","name":"missing"}}]""");
        var exception = Assert.Throws<JsonStateMachineException>(() => builder.AddStateMachineJson(model.ToString(), Options()));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.RuleId == "SM-041" &&
            d.ElementPath == "/vertices/0/onEntry/0/behaviorClass"));
        Assert.IsEmpty(register.StateMachines);
    }

    [TestMethod]
    public void DefinitionsCanSupplyInitialSelectionAndJsonAddsWithoutOverriding()
    {
        var (_, register, builder) = Host();
        var model = Model();
        model.Remove("initial");
        model["definitions"] = JArray.Parse("""[{"clrType":"machine"}]""");
        builder.AddStateMachineJson(model.ToString(), Options());
        Assert.AreEqual("defined", register.StateMachines["json-machine.1"].InitialVertex.Name);
        Assert.HasCount(3, register.StateMachines["json-machine.1"].AllVertices);
    }

    [TestMethod]
    public void RegionalModelsSelectHistoriesAndAllowMetadataOnEveryPseudostate()
    {
        var (_, register, builder) = Host();
        var model = JObject.Parse("""
            {
              "formatVersion":"1.0","name":"json-machine","initial":"outer",
              "vertices":[
                {"kind":"composite","name":"outer","region":{
                  "initial":"child","history":"history-two",
                  "vertices":[
                    {"kind":"state","name":"child","transitions":[{"kind":"event","triggers":[{"clrType":"ping"}],"target":"choice"}]},
                    {"kind":"choice","name":"choice","metadata":{"tag":"choice"},"transitions":[
                      {"kind":"default","target":"junction","guard":{"clrType":"guard"}},
                      {"kind":"default","target":"child-final","else":true}
                    ]},
                    {"kind":"junction","name":"junction","metadata":{"tag":"junction"},"transitions":[{"kind":"default","target":"child-final"}]},
                    {"kind":"history","name":"history-one","metadata":{"tag":"history"},"transitions":[{"kind":"default","target":"child"}]},
                    {"kind":"history","name":"history-two","metadata":{"tag":"history"}},
                    {"kind":"final","name":"child-final","metadata":{"tag":"final"}}
                  ]
                }}
              ]
            }
            """);
        builder.AddStateMachineJson(model.ToString(), Options());
        var graph = register.StateMachines["json-machine.1"];
        var region = graph.InitialVertex.Regions.Single();
        Assert.AreEqual("history-two", region.History.Name);
        Assert.AreEqual("child", region.InitialVertex.Name);
        foreach (var name in new[] { "choice", "junction", "history-one", "history-two", "child-final" })
            Assert.IsTrue(graph.AllVertices[name].Metadata.ContainsKey("tag"));
    }

    [TestMethod]
    public void ForkJoinAndOrthogonalRegionsUseSingleLogicalEdges()
    {
        var (_, register, builder) = Host();
        const string json = """
            {
              "formatVersion":"1.0","name":"json-machine","initial":"start",
              "vertices":[
                {"kind":"state","name":"start","transitions":[{"kind":"event","triggers":[{"clrType":"ping"}],"target":"fork"}]},
                {"kind":"fork","name":"fork","metadata":{"tag":"fork"},"transitions":[
                  {"kind":"default","target":"left"},{"kind":"default","target":"right"}
                ]},
                {"kind":"orthogonal","name":"parallel","regions":[
                  {"initial":"left","vertices":[{"kind":"state","name":"left","transitions":[{"kind":"default","target":"join"}]}]},
                  {"initial":"right","vertices":[{"kind":"state","name":"right","transitions":[{"kind":"default","target":"join"}]}]}
                ]},
                {"kind":"join","name":"join","metadata":{"tag":"join"},"transitions":[{"kind":"default","target":"done"}]},
                {"kind":"final","name":"done"}
              ]
            }
            """;
        builder.AddStateMachineJson(json, Options());
        var graph = register.StateMachines["json-machine.1"];
        Assert.HasCount(6, graph.AllEdges);
        Assert.HasCount(2, graph.AllVertices["parallel"].Regions);
        Assert.AreEqual("fork", graph.AllVertices["fork"].Metadata["tag"]);
        Assert.AreEqual("join", graph.AllVertices["join"].Metadata["tag"]);
    }

    [TestMethod]
    public void CompositeAndOrthogonalDefinitionsExpandOnlyTheirSelectedContracts()
    {
        var (_, register, builder) = Host();
        const string json = """
            {
              "formatVersion":"1.0","name":"json-machine","initial":"composite",
              "vertices":[
                {"kind":"composite","name":"composite","definitions":[{"clrType":"regional"}],
                 "region":{"vertices":[{"kind":"final","name":"added-final"}]}},
                {"kind":"orthogonal","name":"orthogonal","definitions":[{"clrType":"regional"}],
                 "regions":[{"initial":"added","vertices":[{"kind":"state","name":"added"}]}]}
              ]
            }
            """;
        builder.AddStateMachineJson(json, Options().AddType<RegionalDefinition>("regional"));
        var graph = register.StateMachines["json-machine.1"];
        Assert.HasCount(2, graph.AllVertices["composite"].Regions[0].Vertices);
        Assert.HasCount(2, graph.AllVertices["orthogonal"].Regions);
        Assert.IsFalse(graph.AllVertices["composite"].Entry.Any);
        Assert.IsFalse(graph.AllVertices["orthogonal"].Entry.Any);
    }

    [TestMethod]
    public void FailedNewVersionDoesNotReplaceCurrentVersionOrLeakOwnership()
    {
        var (services, register, builder) = Host();
        builder.AddStateMachineJson(Minimal, Options());
        var current = register.StateMachines["json-machine.current"];
        var model = Model();
        model["version"] = 2;
        model["initial"] = "missing";
        var owner = new BehaviorClass(BehaviorType.StateMachine, "owner");
        var parent = new BehaviorClass(BehaviorType.StateMachine, "parent");
        register.OwnerClass = owner;
        register.ParentClass = parent;
        Assert.Throws<JsonStateMachineException>(() => builder.AddStateMachineJson(model.ToString(), Options()));
        Assert.AreEqual(owner, register.OwnerClass);
        Assert.AreEqual(parent, register.ParentClass);
        Assert.AreSame(current, register.StateMachines["json-machine.current"]);
        Assert.IsFalse(register.StateMachines.ContainsKey("json-machine.2"));
    }

    [TestMethod]
    public void SubmachineVisitorsRetainJsonTriggerTypes()
    {
        var (_, register, builder) = Host();
        var model = Model();
        State(model)["transitions"] = JArray.Parse("""
            [{"kind":"event","triggers":[{"clrType":"ping"},{"clrType":"pong"}],"target":"done"}]
            """);
        builder.AddStateMachineJson(model.ToString(), Options());
        model["name"] = "host";
        State(model).Remove("transitions");
        State(model)["do"] = JObject.Parse("""{"behaviorClass":{"type":"StateMachine","name":"json-machine"}}""");
        builder.AddStateMachineJson(model.ToString(), Options());
        CollectionAssert.AreEquivalent(new[] { typeof(Ping), typeof(Pong) },
            register.StateMachines["host.1"].InitialVertex.BehaviorEventTypes.ToArray());
    }

    [TestMethod]
    public void ConfigurationObjectsAreValidatedAndNativeDateStringsStayStrings()
    {
        var (_, register, builder) = Host();
        var model = Model();
        State(model)["onEntry"] = JArray.Parse("""
            [{"clrType":"objectConfiguration","configuration":{"Label":"configured","Child":{"$type":"display","Label":"nested"}}}]
            """);
        State(model)["metadata"] = JObject.Parse("""{"date":"2026-10-21T00:00:00Z"}""");
        builder.AddStateMachineJson(model.ToString(), Options().AddType<ObjectConfiguration>("objectConfiguration"));
        Assert.IsInstanceOfType<string>(register.StateMachines["json-machine.1"].InitialVertex.Metadata["date"]);
    }

    [TestMethod]
    public void MissingHandlerBehaviorsAccumulateWithGraphErrors()
    {
        var (_, register, builder) = Host();
        var model = Model();
        State(model)["onExit"] = JArray.Parse("""[{"behaviorClass":{"type":"Activity","name":"missing"}}]""");
        State(model)["transitions"] = JArray.Parse("""
            [{"kind":"event","triggers":[{"clrType":"ping"}],"target":"missing"}]
            """);
        var exception = Assert.Throws<JsonStateMachineException>(() => builder.AddStateMachineJson(model.ToString(), Options()));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.RuleId == "SM-041"));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.RuleId == "SM-017"));
        Assert.IsEmpty(register.StateMachines);
    }

    [TestMethod]
    public void ForbiddenDeferralsRetainTheirDeclarationPath()
    {
        var (_, register, builder) = Host();
        var model = Model();
        State(model)["deferrals"] = JArray.Parse("""[{"triggers":[{"clrType":"completion"}]}]""");
        var exception = Assert.Throws<JsonStateMachineException>(() => builder.AddStateMachineJson(model.ToString(), Options()));
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d =>
            d.RuleId == "SM-036" && d.ElementPath == "/vertices/0/deferrals/0"));
        Assert.IsEmpty(register.StateMachines);
    }

    [TestMethod]
    [DataRow("composite")]
    [DataRow("orthogonal")]
    public void RegionalStatesRejectSubmachinesBeforeDefinitionExecution(string kind)
    {
        var (_, register, builder) = Host();
        var model = Model();
        State(model)["kind"] = kind;
        State(model)["do"] = JObject.Parse("""{"behaviorClass":{"type":"StateMachine","name":"missing"}}""");
        model["definitions"] = JArray.Parse("""[{"clrType":"machine"}]""");
        var before = MachineDefinition.Builds;
        var exception = Assert.Throws<JsonStateMachineException>(() => builder.AddStateMachineJson(model.ToString(), Options()));
        Assert.AreEqual(before, MachineDefinition.Builds);
        Assert.IsTrue(exception.ValidationReport.Diagnostics.Any(d => d.RuleId == "JSON-003"));
        Assert.IsEmpty(register.StateMachines);
    }

    [TestMethod]
    public void MixedTriggerSetsPreserveExceptionPolymorphismWithoutWideningOtherTriggers()
    {
        var (_, register, builder) = Host();
        var model = Model();
        State(model)["transitions"] = JArray.Parse("""
            [{"kind":"event","triggers":[{"clrType":"exception"},{"clrType":"baseEvent"}],"target":"done"}]
            """);
        builder.AddStateMachineJson(model.ToString(), Options().AddType<Exception>("exception").AddType<BaseEvent>("baseEvent"));
        var edge = register.StateMachines["json-machine.1"].AllEdges.Single();
        Assert.IsTrue(new EventHolder<InvalidOperationException> { Payload = new() }.Triggers(edge));
        Assert.IsTrue(new EventHolder<BaseEvent> { Payload = new() }.Triggers(edge));
        Assert.IsFalse(new EventHolder<DerivedEvent> { Payload = new() }.Triggers(edge));
    }

    public sealed class Ping;
    public sealed class Pong;
    public sealed class Display
    {
        public string Label { get; set; } = "";
        public object? Child { get; set; }
    }
    public sealed class CapableState : IStateDefinition, IStateEntry, IStateExit
    {
        public static int Builds;
        public static void Build(IStateBuilder builder) => Interlocked.Increment(ref Builds);
        public Task OnEntryAsync() => Task.CompletedTask;
        public Task OnExitAsync() => Task.CompletedTask;
    }
    public sealed class MachineDefinition : IStateMachine
    {
        public static int Builds;
        public static void Build(IStateMachineBuilder builder)
        {
            Interlocked.Increment(ref Builds);
            builder.AddInitialState("defined");
        }
    }
    public sealed class EntryOnly : IStateEntry, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public Task OnEntryAsync() => Task.CompletedTask;
    }
    public sealed class MultiConfiguration : IStateEntry, IConfigurable<string>, IConfigurable<int>
    {
        string IConfigurable<string>.Configuration { set { } }
        int IConfigurable<int>.Configuration { set { } }
        public Task OnEntryAsync() => Task.CompletedTask;
    }
    public sealed class Guard : IStateMachineGuard<object>
    {
        public Task<bool> GuardAsync(object @event) => Task.FromResult(true);
    }
    public sealed class UnionGuard : ITransitionGuard<OneOf<Ping, Pong>>
    {
        public Task<bool> GuardAsync(OneOf<Ping, Pong> @event) => Task.FromResult(true);
    }
    public sealed class TransitionDefinition : ITransitionDefinition<object>, ITransitionGuard<object>,
        ITransitionEffect<object>, IConfigurable<string>
    {
        public string Configuration { private get; set; } = "";
        public void Build(ITransitionBuilder<object> builder) => builder.AddMetadata("definition", Configuration);
        public Task<bool> GuardAsync(object @event) => Task.FromResult(false);
        public Task EffectAsync(object @event) => Task.CompletedTask;
    }
    public sealed class MetadataDefinition : IStateDefinition
    {
        public static void Build(IStateBuilder builder)
            => ((Stateflows.Common.Interfaces.IMetadataBuilder)builder).Metadata.Add("key", 1);
    }
    public sealed class RegionalDefinition : ICompositeStateDefinition, IOrthogonalStateDefinition, IStateEntry
    {
        static void ICompositeStateDefinition.Build(ICompositeStateBuilder builder)
            => builder.AddInitialState("defined-child");
        static void IOrthogonalStateDefinition.Build(IOrthogonalStateBuilder builder)
            => builder.AddRegion(region => region.AddInitialState("defined-region-child"));
        public Task OnEntryAsync() => throw new InvalidOperationException("Entry was not selected.");
    }
    public sealed class ObjectConfiguration : IStateEntry, IConfigurable<Display>
    {
        public Display Configuration { private get; set; } = new();
        public Task OnEntryAsync() => Task.CompletedTask;
    }
    public class BaseEvent;
    public sealed class DerivedEvent : BaseEvent;
}
