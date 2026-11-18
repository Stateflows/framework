using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Common.Registration.Builders;
using Stateflows.Payloads;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class PayloadTokenInjection : StateflowsTestClass
{
    private static readonly List<string> Captured = [];
    private IPayloadFactory payloadFactory = null!;

    public sealed class NamedTokenAction(
        [TokenName("first", "second")] IInputTokens input,
        [TokenName("first", "second")] IInputToken required,
        [TokenName("other")] IOptionalInputToken optional,
        [TokenName("first", "second")] IInputOutputTokens inputOutput) : IActionNode
    {
        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
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
                .AddInputPin(["first", "second"], pin => pin.AddFlow<NamedTokenAction>())
                .AddAction<NamedTokenAction>(action => action.AddFlow(["first", "second"], "output"))
                .AddOutputPin(["first", "second"], "output", pin => pin.SetTimeToLive(300).SetRetain(true))));

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

        var output = await activity.GetOutputAsync(["first", "second"]);
        CollectionAssert.AreEquivalent(new[] { "first", "second" }, output.Select(token => token.Name).ToArray());
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
