using Stateflows.Activities.Json;
using Stateflows.Common;
using Stateflows.Common.Json;
using StateMachine.IntegrationTests.Utils;
using Newtonsoft.Json.Linq;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class JsonActivity : StateflowsTestClass
{
    private const string EventModel = """
        {
          "formatVersion": "1.0",
          "name": "json-event-activity",
          "nodes": [
            {
              "kind": "acceptEvent",
              "name": "receive",
              "eventType": "event.ping",
              "implementation": { "clrType": "node.accept" },
              "flows": [{ "kind": "control", "target": "complete" }]
            },
            { "kind": "final", "name": "complete" }
          ]
        }
        """;

    private const string DecisionModel = """
        {
          "formatVersion": "1.0",
          "name": "json-decision-activity",
          "nodes": [
            { "kind": "initial", "flows": [{ "kind": "control", "target": "produce" }] },
            {
              "kind": "action",
              "name": "produce",
              "implementation": { "clrType": "node.produce" },
              "flows": [{ "kind": "object", "tokenType": "token.int", "target": "route" }]
            },
            {
              "kind": "decision",
              "name": "route",
              "mode": "object",
              "tokenType": "token.int",
              "flows": [
                {
                  "kind": "object",
                  "target": "accepted",
                  "tokenType": "token.int",
                  "pipeline": [{ "kind": "guard", "handler": { "clrType": "flow.guard" } }]
                },
                { "kind": "elseObject", "target": "rejected", "tokenType": "token.int" }
              ]
            },
            {
              "kind": "action",
              "name": "accepted",
              "implementation": { "clrType": "node.decisionResult" },
              "flows": [{ "kind": "control", "target": "complete" }]
            },
            {
              "kind": "action",
              "name": "rejected",
              "implementation": { "clrType": "node.decisionResult" },
              "flows": [{ "kind": "control", "target": "complete" }]
            },
            { "kind": "final", "name": "complete" }
          ]
        }
        """;

    private const string ControlDecisionModel = """
        {
          "formatVersion": "1.0",
          "name": "json-control-decision-activity",
          "nodes": [
            { "kind": "initial", "flows": [{ "kind": "control", "target": "route" }] },
            {
              "kind": "decision",
              "name": "route",
              "mode": "control",
              "flows": [
                {
                  "kind": "control",
                  "target": "accepted",
                  "pipeline": [{ "kind": "guard", "handler": { "clrType": "flow.controlGuard" } }]
                },
                { "kind": "elseControl", "target": "rejected" }
              ]
            },
            {
              "kind": "action",
              "name": "accepted",
              "implementation": { "clrType": "node.controlAccepted" },
              "flows": [{ "kind": "control", "target": "complete" }]
            },
            {
              "kind": "action",
              "name": "rejected",
              "implementation": { "clrType": "node.controlRejected" },
              "flows": [{ "kind": "control", "target": "complete" }]
            },
            { "kind": "final", "name": "complete" }
          ]
        }
        """;

    private const string Model = """
        {
          "$schema": "urn:stateflows:activity:1.0",
          "formatVersion": "1.0",
          "name": "json-activity",
          "definitions": [{ "clrType": "activity.definition" }],
          "nodes": [
            { "kind": "initial", "flows": [{ "kind": "control", "target": "fromDefinition" }] },
            {
              "kind": "action",
              "name": "produce",
              "implementation": { "clrType": "node.produce" },
              "flows": [
                {
                  "kind": "object",
                  "tokenType": "token.int",
                  "target": "consume",
                  "pipeline": [
                    { "kind": "guard", "handler": { "clrType": "flow.guard" } },
                    {
                      "kind": "transform",
                      "outputTokenType": "token.string",
                      "handler": {
                        "clrType": "flow.transform",
                        "configuration": { "prefix": "v=" }
                      }
                    }
                  ]
                }
              ]
            },
            {
              "kind": "action",
              "name": "consume",
              "implementation": { "clrType": "node.consume" },
              "flows": [{ "kind": "control", "target": "done" }]
            },
            { "kind": "final", "name": "done" }
          ]
        }
        """;

    private static JsonOptions Options() => new JsonOptions()
        .AddType<Producer>("node.produce")
        .AddType<Consumer>("node.consume")
        .AddType<int>("token.int")
        .AddType<string>("token.string")
        .AddType<AcceptFour>("flow.guard")
        .AddType<ToText>("flow.transform")
        .AddType<TransformConfiguration>("config.transform")
        .AddType<ActivityDefinition>("activity.definition")
        .AddType<Ping>("event.ping")
        .AddType<PingHandler>("node.accept")
        .AddType<DecisionResult>("node.decisionResult")
        .AddType<RejectControlGuard>("flow.controlGuard")
        .AddType<AcceptedControlResult>("node.controlAccepted")
        .AddType<RejectedControlResult>("node.controlRejected");

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        Consumer.Received = null;
        ActivityDefinition.Builds = 0;
        PingHandler.Received = null;
        builder.AddActivities(activities => activities
            .AddActivityJson(Model, Options())
            .AddActivityJson(EventModel, Options())
            .AddActivityJson(DecisionModel, Options())
            .AddActivityJson(ControlDecisionModel, Options()));
    }

    [TestMethod]
    public void SchemaRejectsUnsupportedFieldsAndInvalidWeights()
    {
        var unknown = JObject.Parse(Model);
        unknown["unexpected"] = true;
        var schemaError = Assert.Throws<JsonActivityException>(() =>
            JsonActivityDefinition.Parse(unknown.ToString(), Options()));
        Assert.IsTrue(schemaError.ValidationReport.Diagnostics.Any(diagnostic => diagnostic.RuleId == "JSON-002"));

        var negativeWeight = JObject.Parse(Model);
        negativeWeight["nodes"]![1]!["flows"]![0]!["weight"] = -1;
        var weightError = Assert.Throws<JsonActivityException>(() =>
            JsonActivityDefinition.Parse(negativeWeight.ToString(), Options()));
        Assert.IsTrue(weightError.ValidationReport.Diagnostics.Any(diagnostic => diagnostic.RuleId == "JSON-002"));
    }

    [TestMethod]
    public async Task ActivityIsBuiltThroughBuildersAndExecutesOrderedFlowPipeline()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("json-activity", "instance"), out var activity));
        var result = await activity.SendAsync(new Initialize());
        Assert.AreEqual(EventStatus.Initialized, result.Status);
        Assert.AreEqual(1, ActivityDefinition.Builds);
        Assert.AreEqual("v=4", Consumer.Received);
        Assert.AreEqual(BehaviorStatus.Finalized, (await activity.GetStatusAsync()).Response.BehaviorStatus);
    }

    [TestMethod]
    public async Task AcceptEventNodeUsesItsExplicitEventAndHandlerBindings()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("json-event-activity", "event-instance"), out var activity));
        Assert.AreEqual(EventStatus.Initialized, (await activity.SendAsync(new Initialize())).Status);
        await activity.SendAsync(new Ping { Value = "accepted" });
        Assert.AreEqual("accepted", PingHandler.Received);
        Assert.AreEqual(BehaviorStatus.Finalized, (await activity.GetStatusAsync()).Response.BehaviorStatus);
    }

    [TestMethod]
    public async Task ObjectDecisionRoutesTokensThroughGuardAndElseFlow()
    {
        DecisionResult.Accepted = 0;
        DecisionResult.Rejected = 0;
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("json-decision-activity", "decision-instance"), out var activity));
        Assert.AreEqual(EventStatus.Initialized, (await activity.SendAsync(new Initialize())).Status);
        Assert.AreEqual(1, DecisionResult.Accepted);
        Assert.AreEqual(0, DecisionResult.Rejected);
        Assert.AreEqual(BehaviorStatus.Finalized, (await activity.GetStatusAsync()).Response.BehaviorStatus);
    }

    [TestMethod]
    public async Task ControlDecisionUsesItsGuardAndElseFlow()
    {
        AcceptedControlResult.Executions = 0;
        RejectedControlResult.Executions = 0;
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("json-control-decision-activity", "control-instance"), out var activity));
        Assert.AreEqual(EventStatus.Initialized, (await activity.SendAsync(new Initialize())).Status);
        Assert.AreEqual(0, AcceptedControlResult.Executions);
        Assert.AreEqual(1, RejectedControlResult.Executions);
        Assert.AreEqual(BehaviorStatus.Finalized, (await activity.GetStatusAsync()).Response.BehaviorStatus);
    }

    public sealed class Producer(IOutputTokens<int> output) : IActionNode
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            output.Add(4);
            return Task.CompletedTask;
        }
    }

    public sealed class Consumer(IInputTokens<string> input) : IActionNode
    {
        public static string? Received { get; set; }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Received = input.Single();
            return Task.CompletedTask;
        }
    }

    public sealed class AcceptFour : IFlowGuard<int>
    {
        public Task<bool> GuardAsync(int token) => Task.FromResult(token == 4);
    }

    public sealed class TransformConfiguration
    {
        public string Prefix { get; set; } = "";
    }

    public sealed class ToText : IFlowTransformation<int, string>, IConfigurable<TransformConfiguration>
    {
        public TransformConfiguration Configuration { private get; set; } = new();

        public Task<string> TransformAsync(int token) => Task.FromResult(Configuration.Prefix + token);
    }

    public sealed class ActivityDefinition : IActivity
    {
        public static int Builds { get; set; }

        public static void Build(IActivityBuilder builder)
        {
            Builds++;
            builder.AddAction("fromDefinition", _ => Task.CompletedTask,
                action => action.AddControlFlow("produce"));
        }
    }

    public sealed class Ping
    {
        public string Value { get; set; } = "";
    }

    public sealed class PingHandler : IAcceptEventActionNode<Ping>
    {
        public static string? Received { get; set; }

        public Task ExecuteAsync(Ping @event, CancellationToken cancellationToken)
        {
            Received = @event.Value;
            return Task.CompletedTask;
        }
    }

    public sealed class DecisionResult(IInputTokens<int> input) : IActionNode
    {
        public static int Accepted { get; set; }
        public static int Rejected { get; set; }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (input.Single() == 4)
                Accepted++;
            else
                Rejected++;
            return Task.CompletedTask;
        }
    }

    public sealed class RejectControlGuard : IControlFlowGuard
    {
        public Task<bool> GuardAsync() => Task.FromResult(false);
    }

    public sealed class AcceptedControlResult : IActionNode
    {
        public static int Executions { get; set; }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Executions++;
            return Task.CompletedTask;
        }
    }

    public sealed class RejectedControlResult : IActionNode
    {
        public static int Executions { get; set; }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Executions++;
            return Task.CompletedTask;
        }
    }
}
