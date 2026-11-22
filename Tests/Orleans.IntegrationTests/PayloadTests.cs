using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Orleans.Runtime;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Payloads;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

public sealed partial class BehaviorNotificationTests
{
    public class PayloadClrResponse
    {
        public int Value { get; set; }
    }

    public sealed class DerivedPayloadClrResponse : PayloadClrResponse;

    private static IHost CreatePayloadHost(int[] ports)
        => Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddStateflows(b => b
                .AddPayloads(b => b
                    .AddPayload("structural-response", b => b.AddField("value", FieldType.Int32))
                    .AddRequest("structural-query", "structural-response", b => b
                        .AddField("value", FieldType.Int32, b => b.AddValidation(new RangeAttribute(1, 10))))
                    .AddRequest<PayloadClrResponse>("structural-clr-query", b => b.AddField("value", FieldType.Int32)))
                .AddStateMachines(b => b.AddStateMachine("structural-orleans", b => b
                    .AddInitialState("ready", b => b
                        .AddInternalTransition("structural-query", b => b.AddEffect(c =>
                            c.Event.Respond(c.Behavior.ServiceProvider.GetRequiredService<IPayloadFactory>()
                                .Create("structural-response", b => b.Set("value", c.Event.Get<int>("value"))))))
                        .AddInternalTransition("structural-clr-query", b => b.AddEffect(c =>
                            c.Event.Respond(new DerivedPayloadClrResponse { Value = c.Event.Get<int>("value") }))))))))
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
            .UseOrleans(silo => silo
                .UseLocalhostClustering(ports[0], ports[1], null,
                    $"structural-service-{Guid.NewGuid():N}", $"structural-cluster-{Guid.NewGuid():N}")
                .AddMemoryGrainStorageAsDefault()
                .UseInMemoryReminderService()
                .AddStateflowsRuntime())
            .Build();

    [TestMethod]
    public async Task PayloadsValidateAndReturnBothResponseKindsAcrossOrleansReactivation()
    {
        using var host = CreatePayloadHost(GetFreeTcpPorts(2));
        await host.StartAsync();
        try
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var factory = services.GetRequiredService<IPayloadFactory>();
            Assert.IsTrue(services.GetRequiredService<IBehaviorLocator>().TryLocateBehavior(
                new BehaviorId(BehaviorType.StateMachine, "structural-orleans", "one"), out var behavior));
            using (behavior)
            {
                var invalid = await behavior.RequestAsync(factory.Create("structural-query", b => b.Set("value", 0)));
                Assert.AreEqual(EventStatus.Invalid, invalid.Status);
                Assert.IsFalse(invalid.Validation.IsValid);
                var request = factory.Create("structural-query", b => b.Set("value", 7));
                OrleansEventHolder wire = request.ToTypedEventHolder();
                var restored = (EventHolder)wire;
                Assert.AreEqual("structural-query", restored.Name);
                Assert.AreEqual(7, ((Payload)restored.BoxedPayload).Get<int>("value"));

                var tokenTransfer = new TokensInput()
                    .Add<Payload>(request)
                    .Add(42);
                OrleansEventHolder wireTransfer = tokenTransfer.ToTypedEventHolder();
                var restoredTransfer = (TokensInput)((EventHolder)wireTransfer).BoxedPayload;
                Assert.AreEqual("structural-query", ((Payload)restoredTransfer.Tokens[0].BoxedPayload).Name);
                Assert.AreEqual(42, restoredTransfer.Tokens[1].BoxedPayload);

                var response = await behavior.RequestAsync(request);
                Assert.AreEqual(EventStatus.Consumed, response.Status);
                Assert.AreEqual("structural-response", response.Response.Name);
                Assert.AreEqual(7, response.Response.Get<int>("value"));

                await services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0)
                    .ForceActivationCollection(TimeSpan.Zero);
                var clr = await behavior.RequestAsync<PayloadClrResponse>(
                    factory.Create("structural-clr-query", b => b.Set("value", 9)));
                Assert.AreEqual(EventStatus.Consumed, clr.Status);
                Assert.AreEqual(9, clr.Response.Value);
                Assert.IsInstanceOfType<DerivedPayloadClrResponse>(clr.Response);
            }
        }
        finally { await host.StopAsync(); }
    }
}
