using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Payloads;

namespace Action.IntegrationTests.Tests;

[TestClass]
public sealed class PayloadTokenInjection : StateflowsTestClass
{
    private IPayloadFactory payloadFactory = null!;

    public sealed class NamedTokenAction(
        [TokenName("first")] IInputTokens input,
        [TokenName("first")] IOutputTokens output) : IAction
    {
        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            output.AddRange(input);
            return Task.CompletedTask;
        }
    }

    [TestInitialize]
    public override void Initialize()
    {
        base.Initialize();
        payloadFactory = ServiceProvider.GetRequiredService<IPayloadFactory>();
    }

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder
            .AddPayloads(b => b
                .AddPayload("first", _ => { })
                .AddPayload("other", _ => { }))
            .AddActions(b => b
                .AddAction<NamedTokenAction>("named-token-injection", 1, action => action
                    .AddConsumedPayloadToken("first")
                    .AddProducedPayloadToken(["first"])));

    [TestMethod]
    public async Task TokenNameFiltersInputAndOutputCollections()
    {
        Assert.IsTrue(ActionLocator.TryLocateAction(
            new ActionId("named-token-injection", Guid.NewGuid().ToString()), out var action));

        var result = await action.SendInputAsync(input => input
            .Add(payloadFactory.Create("first", _ => { }))
            .Add(payloadFactory.Create("other", _ => { })));

        Assert.AreEqual(EventStatus.Consumed, result.Status);
        Assert.IsNotNull(result.Response);
        var output = result.Response.GetPayloadTokens("first").ToArray();
        Assert.HasCount(1, output);
        Assert.IsEmpty(result.Response.GetPayloadTokens("other"));
    }
}
