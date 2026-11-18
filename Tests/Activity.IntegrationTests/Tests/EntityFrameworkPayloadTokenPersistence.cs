using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stateflows;
using Stateflows.Activities.Context.Classes;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Enums;
using Stateflows.Common.Interfaces;
using Stateflows.Payloads;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class EntityFrameworkPayloadTokenPersistence
{
    [TestMethod]
    public async Task PersistentPayloadFlowQueueRestoresFromEntityFrameworkStorage()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextFactory<PayloadPersistenceDbContext>(options =>
            options.UseInMemoryDatabase($"payload-persistence-{Guid.NewGuid():N}"));
        services.AddStateflows(stateflows => stateflows
            .AddPayloads(payloads => payloads.AddPayload("queued-payload", payload =>
                payload.AddField("value", FieldType.Int32)))
            .AddEntityFrameworkCoreStorage<PayloadPersistenceDbContext>(StorageKind.Context));

        using var provider = services.BuildServiceProvider();
        var payloadFactory = provider.GetRequiredService<IPayloadFactory>();
        var token = new TokenHolder<Payload>
        {
            Payload = payloadFactory.Create("queued-payload", payload => payload.Set("value", 42)),
            IsStreamed = true
        };
        var id = new BehaviorId(BehaviorType.Activity, "payload-persistence", Guid.NewGuid().ToString("N"));
        var context = new StateflowsContext(id);
        var threadId = Guid.NewGuid();
        var root = new RootContext(context);
        var flow = root.GetStream("payload-flow", threadId);
        flow.Consume([token], isPersistent: true);

        var storage = provider.GetRequiredService<IStateflowsStorage>();
        await storage.DehydrateAsync(context);
        var restored = await storage.HydrateAsync(id);

        var restoredFlow = new RootContext(restored).GetStream("payload-flow", threadId);
        Assert.IsTrue(restoredFlow.IsPersistent);
        var restoredToken = restoredFlow.Tokens.Single();
        Assert.IsTrue(restoredToken.IsStreamed);
        Assert.AreEqual("queued-payload", restoredToken.Name);
        Assert.IsInstanceOfType<Payload>(restoredToken.BoxedPayload);
        var restoredPayload = (Payload)restoredToken.BoxedPayload;
        Assert.AreEqual(42, restoredPayload.Get<int>("value"));
    }

    private sealed class PayloadPersistenceDbContext(DbContextOptions<PayloadPersistenceDbContext> options)
        : DbContext(options), IStateflowsDbContext_v1
    {
        public DbSet<Context_v1> Contexts_v1 { get; set; } = null!;
        public DbSet<Notification_v1> Notifications_v1 { get; set; } = null!;
        public DbSet<Value_v1> Values_v1 { get; set; } = null!;
    }
}
