using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Common.Utilities;
using Stateflows.Common.Registration.Builders;
using Stateflows.Payloads;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class PayloadTokenInjection : StateflowsTestClass
{
    private static readonly List<string> Captured = [];
    private static readonly List<(string[] PinNames, string[] FlowNames, string[] TargetNames)> Inspected = [];
    private IPayloadFactory payloadFactory = null!;

    private sealed class PayloadNamesVisitor : ActivityVisitor
    {
        public readonly List<(TokenNamesDeclarationKind Kind, string[] Names)> Declarations = [];

        public override Task TokenNamesDeclaredAsync(string elementId, string activityName, int activityVersion,
            TokenNamesDeclarationKind kind, IReadOnlyList<string> tokenNames)
        {
            if (activityName == "named-token-injection")
                Declarations.Add((kind, tokenNames.ToArray()));
            return Task.CompletedTask;
        }
    }

    public sealed class NamedTokenAction(
        [TokenName("first", "second")] IInputTokens input,
        [TokenName("first", "second")] IInputToken required,
        [TokenName("other")] IOptionalInputToken optional,
        [TokenName("first", "second")] IInputOutputTokens inputOutput,
        IActivityInspection inspection) : IActionNode
    {
        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var nodes = inspection.Nodes.ToArray();
            var pinNames = nodes.SelectMany(node => node.PinTokenNames).Distinct().ToArray();
            var flows = nodes.SelectMany(node => node.Flows).ToArray();
            lock (Inspected)
                Inspected.Add((pinNames, flows.SelectMany(flow => flow.TokenNames).Distinct().ToArray(),
                    flows.SelectMany(flow => flow.TargetTokenNames).Distinct().ToArray()));
            lock (Captured)
            {
                Captured.AddRange(input.Select(payload => payload.Name));
                Captured.Add(required.Token.Name);
                Captured.Add(optional.IsAvailable ? "optional-present" : "optional-empty");
            }

            inputOutput.PassAllOn();
            return Task.CompletedTask;
        }
    }

    [TestInitialize]
    public override void Initialize()
    {
        lock (Captured) Captured.Clear();
        lock (Inspected) Inspected.Clear();
        base.Initialize();
        payloadFactory = ServiceProvider.GetRequiredService<IPayloadFactory>();
    }

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder
            .AddPayloads(b => b
                .AddPayload("first", _ => { })
                .AddPayload("second", _ => { })
                .AddPayload("other", _ => { }))
            .AddActivities(b => b.AddActivity("named-token-injection", b => b
                .AddPayloadInputPin(["first", "second"], pin => pin.AddFlow<NamedTokenAction>())
                .AddAction<NamedTokenAction>(action => action.AddPayloadFlow(["first", "second"], "output"))
                .AddPayloadOutputPin(["first", "second"], "output", pin => pin.SetTimeToLive(300).SetRetain(true))));

    [TestMethod]
    public async Task TokenNameFiltersInputAndOutputCollections()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(
            new ActivityId("named-token-injection", Guid.NewGuid().ToString()), out var activity));

        var result = await activity.SendAsync(new TokensInput()
            .Add<Payload>(payloadFactory.Create("first", _ => { }))
            .Add<Payload>(payloadFactory.Create("second", _ => { })));

        Assert.AreEqual(EventStatus.Consumed, result.Status);
        lock (Captured)
            CollectionAssert.AreEqual(new[] { "first", "second", "first", "optional-empty" }, Captured.ToArray());
        lock (Inspected)
        {
            Assert.IsTrue(Inspected.Any(item => item.PinNames.Contains("first") && item.PinNames.Contains("second")));
            Assert.IsTrue(Inspected.Any(item => item.FlowNames.Contains("first") && item.FlowNames.Contains("second")));
            Assert.IsTrue(Inspected.Any(item => item.TargetNames.Contains("first") && item.TargetNames.Contains("second")));
        }

        var output = await activity.GetOutputAsync(["first", "second"]);
        CollectionAssert.AreEquivalent(new[] { "first", "second" }, output.Select(token => token.Name).ToArray());
    }

    [TestMethod]
    public async Task VisitorReportsPayloadPinAndFlowNames()
    {
        var visitor = new PayloadNamesVisitor();
        await ServiceProvider.GetRequiredService<IActivitiesRegister>().VisitActivitiesAsync(visitor);

        CollectionAssert.Contains(visitor.Declarations.Select(item => item.Kind).ToArray(), TokenNamesDeclarationKind.InputPin);
        CollectionAssert.Contains(visitor.Declarations.Select(item => item.Kind).ToArray(), TokenNamesDeclarationKind.OutputPin);
        CollectionAssert.Contains(visitor.Declarations.Select(item => item.Kind).ToArray(), TokenNamesDeclarationKind.FlowSource);
        Assert.IsTrue(visitor.Declarations.Any(item =>
            item.Kind == TokenNamesDeclarationKind.InputPin && item.Names.SequenceEqual(["first", "second"])));
        Assert.IsTrue(visitor.Declarations.Any(item =>
            item.Kind == TokenNamesDeclarationKind.FlowTarget && item.Names.SequenceEqual(["first", "second"])));
    }

    [TestMethod]
    public void MixedTokenTransferRoundTripsThroughPolymorphicJson()
    {
        var original = new TokensInput()
            .Add<Payload>(payloadFactory.Create("first", _ => { }))
            .Add(42);

        var json = StateflowsJsonConverter.SerializePolymorphicObject(original);
        var restored = StateflowsJsonConverter.DeserializeObject<TokensInput>(json)!;

        Assert.HasCount(2, restored.Tokens);
        var payloadToken = restored.Tokens.OfType<TokenHolder<Payload>>().Single();
        Assert.AreEqual("first", payloadToken.Payload.Name);
        Assert.AreEqual(42, restored.Tokens.OfType<TokenHolder<int>>().Single().Payload);
    }

    [TestMethod]
    public void PayloadClosedGenericCollectionDoesNotReplaceNamedInterface()
    {
        var builder = new ActivityBuilder("definition", 1, null!, new StateflowsBuilder(new ServiceCollection()), null, null);
        Assert.ThrowsExactly<Stateflows.Activities.Exceptions.NodeDefinitionException>(() =>
            ((IActivityBuilder)builder).AddAction<PayloadClosedGenericAction>());
    }

    public sealed class PayloadClosedGenericAction(IInputTokens<Payload> input) : IActionNode
    {
        public Task ExecuteAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
