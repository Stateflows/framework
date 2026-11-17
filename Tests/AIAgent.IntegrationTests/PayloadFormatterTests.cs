using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Payloads;
using Stateflows.MAF;
using Stateflows.MAF.AIAgents;

namespace AIAgent.IntegrationTests;

[TestClass]
[DoNotParallelize]
public sealed class PayloadFormatterTests : StateflowsTestClass
{
    [TestInitialize]
    public override void Initialize()
    {
        SharedTestAIAgent.Instance.Reset();
        base.Initialize();
    }

    [TestCleanup]
    public override void Cleanup()
        => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder
            .AddPayloads(e => e
                .AddPayload("order", b => b.AddField("id", FieldType.Int32))
                .AddPayload("cancel", b => b.AddField("reason", FieldType.String))
            )
            .AddAIAgents(b => b.AddAIAgent<TestBehaviorAgent>("payloadAgent", 1, a => a
                .AddConsumedEvent("order", (_, @event) =>
                    Task.FromResult(new ChatMessage(ChatRole.User, $"order {@event.Get<int>("id")}")))
                .AddConsumedEvent("cancel", (_, @event) =>
                    Task.FromResult(new ChatMessage(ChatRole.User, $"cancel {@event.Get<string>("reason")}")))
            ));

    [TestMethod]
    public async Task NamedFormatter_FormatsOnlyMatchingPayload()
    {
        var factory = ServiceProvider.GetRequiredService<IPayloadFactory>();
        var locator = ServiceProvider.GetRequiredService<IAIAgentLocator>();
        Assert.IsTrue(locator.TryLocateAIAgent(new AIAgentId("payloadAgent", "x"), out var agent));

        await agent.SendAsync(factory.Create("order", b => b.Set("id", 7)));

        Assert.AreEqual("order 7", SharedTestAIAgent.Instance.LastRunMessages.Single().Text);
    }
}
