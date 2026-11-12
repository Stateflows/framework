using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Activities.Models;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Entities.Enums;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class PublishEventActions : StateflowsTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder.AddActivities(activities => activities
            .AddActivity("publish-many", activity => activity
                .AddInitial(initial => initial.AddControlFlow<PublishMany>())
                .AddPublishEventAction<Notice, PublishMany>())
            .AddActivity("publish-overridden", activity => activity
                .AddInitial(initial => initial.AddControlFlow<PublishMany>())
                .AddPublishEventAction<Notice, PublishMany>(node => node
                    .SetTimeToLive(0).SetRetain(false).SetImmediatePublish(false)))
            .AddActivity("publish-related", activity => activity
                .AddInitial(initial => initial.AddControlFlow<PublishMany>())
                .AddPublishEventAction<Notice, PublishMany>(node => node
                    .SetPublishScope(PublishScope.Self | PublishScope.Parent | PublishScope.Owner)
                    .SetImmediatePublish(false)))
            .AddActivity("publish-none", activity => activity
                .AddInitial(initial => initial.AddControlFlow<PublishNone>())
                .AddPublishEventAction<Notice, PublishNone>())
            .AddActivity("output-retained", activity => activity
                .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>())
                .AddOutputPin<int>(pin => pin.SetRetain(true).SetTimeToLive(0)))
            .AddActivity("output-default", activity => activity
                .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>())
                .AddOutputPin<int>())
            .AddActivity("output-independent", activity => activity
                .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>())
                .AddInputPin<string>(pin => pin.AddFlow("convert"))
                .AddAction("convert", context =>
                {
                    context.Output(int.Parse(context.GetTokensOfType<string>().Single()));
                    return Task.CompletedTask;
                }, node => node.AddFlow<int, OutputPin<int>>())
                .AddOutputPin<int>(pin => pin.SetTimeToLive(300))));

    private IActivityBehavior Locate(string name, string instance = "a")
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId(name, instance), out var activity));
        return activity;
    }

    [TestMethod]
    public async Task TypedNodePublishesEveryGeneratedEventUsingStaticSettings()
    {
        using var activity = Locate("publish-many");
        var notices = new List<EventHolder>();
        await using var watcher = await activity.WatchAsync([Event<Notice>.Name], notices.Add);
        await activity.SendAsync(new Initialize());
        await watcher.WaitForDeliveryAsync();
        CollectionAssert.AreEquivalent(new[] { 1, 2 }, notices.Cast<EventHolder<Notice>>().Select(n => n.Payload.Value).ToArray());
        Assert.IsTrue(notices.All(n => n.Retained && n.TimeToLive == 37 &&
            n.Headers.Values.Any(header => header is ImmediatePublish)));
        Assert.AreEqual(1, (await activity.GetOutputAsync<Notice>()).Count(),
            "Immediate publications can be dispatched concurrently, but retain only one notification.");
    }

    [TestMethod]
    public async Task BuilderOverridesTypedNodePublicationDefaults()
    {
        using var activity = Locate("publish-overridden");
        var notices = new List<EventHolder>();
        await using var watcher = await activity.WatchAsync([Event<Notice>.Name], notices.Add);
        await activity.SendAsync(new Initialize());
        await watcher.WaitForDeliveryAsync();
        Assert.AreEqual(2, notices.Count);
        Assert.IsTrue(notices.All(n => !n.Retained && n.TimeToLive == 0 &&
            !n.Headers.Values.Any(header => header is ImmediatePublish)));
        Assert.AreEqual(0, (await activity.GetOutputAsync<Notice>()).Count());
    }

    [TestMethod]
    public async Task OutputRetentionIsOptInAndSurvivesFinalization()
    {
        using var activity = Locate("output-retained");
        await activity.SendAsync(new TokensInput().Add(1).Add(2));
        await activity.FinalizeAsync();
        CollectionAssert.AreEqual(new[] { 2 }, (await activity.GetOutputAsync<int>()).ToArray());
        CollectionAssert.AreEqual(new[] { 2 }, (await activity.GetOutputAsync<int>(DateTime.MinValue)).ToArray());
    }

    [TestMethod]
    public async Task DefaultOutputIsRawAndDoesNotCreateActivityOutputStorage()
    {
        using var activity = Locate("output-default");
        var received = new List<int>();
        await using var watcher = await activity.WatchOutputAsync<int>(received.Add);
        await activity.SendAsync(new TokensInput().Add(1).Add(2));
        await watcher.WaitForDeliveryAsync();
        CollectionAssert.AreEqual(new[] { 1, 2 }, received);
        Assert.AreEqual(0, (await activity.GetOutputAsync<int>()).Count());
        Assert.AreEqual(0, (await activity.GetOutputAsync<int>(DateTime.MinValue)).Count());
        Assert.AreEqual(0, (await activity.GetNotificationsAsync<TokensOutput<int>>(DateTime.MinValue)).Count());
    }

    [TestMethod]
    public async Task OutputPinDoesNotJoinIndependentIncomingFlows()
    {
        using var activity = Locate("output-independent");
        await activity.SendAsync(new TokensInput().Add(1));
        await activity.SendAsync(new TokensInput().Add("2"));
        CollectionAssert.AreEqual(new[] { 1, 2 }, (await activity.GetOutputAsync<int>(DateTime.MinValue)).ToArray());
    }

    [TestMethod]
    public async Task PublicationScopeRoutesToSelfParentAndOwner()
    {
        using var activity = Locate("publish-related");
        using var parent = Locate("publish-related", "parent");
        using var owner = Locate("publish-related", "owner");
        var result = await activity.SendAsync(new Initialize(), new Dictionary<string, EventHeader>
        {
            [nameof(BehaviorEmbedding)] = new BehaviorEmbedding { ParentId = parent.Id, OwnerId = owner.Id }
        });
        Assert.AreEqual(EventStatus.Initialized, result.Status);
        foreach (var publisher in new[] { activity, parent, owner })
            Assert.AreEqual(1, (await publisher.GetOutputAsync<Notice>(DateTime.MinValue)).Count());
    }

    [TestMethod]
    public async Task StaticNoneScopeDisablesPublication()
    {
        using var activity = Locate("publish-none");
        Assert.AreEqual(EventStatus.Initialized, (await activity.SendAsync(new Initialize())).Status);
        Assert.AreEqual(0, (await activity.GetOutputAsync<Notice>(DateTime.MinValue)).Count());
    }

    [TestMethod]
    public async Task NotificationQueriesExpireRetainedValuesWithoutRevivingOlderOnes()
    {
        ServiceProvider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
            ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
        var storage = ServiceProvider.GetRequiredService<IStateflowsNotificationsStorage>();
        BehaviorId id = new ActivityId("output-retained", "expiry");
        var older = new Notice(1).ToEventHolder(id);
        older.SentAt = DateTime.Now.AddMinutes(-2);
        older.Retained = true;
        var expired = new Notice(2).ToEventHolder(id);
        expired.SentAt = DateTime.Now.AddMinutes(-1);
        expired.Retained = true;
        expired.TimeToLive = 1;
        var recent = new Notice(3).ToEventHolder(id);
        recent.SentAt = DateTime.Now;
        recent.TimeToLive = 300;
        await storage.SaveNotificationsAsync(id, [older, expired, recent]);
        var names = new[] { Event<Notice>.Name };
        Assert.AreEqual(0, (await storage.GetNotificationsAsync(id, names, DateTime.Now.AddDays(1))).Count());
        CollectionAssert.AreEqual(new[] { 3 },
            (await storage.GetNotificationsAsync(id, names, DateTime.MinValue))
            .Cast<EventHolder<Notice>>().Select(n => n.Payload.Value).ToArray());
    }

    [TestMethod]
    [DataRow(false, 0)]
    [DataRow(false, 300)]
    [DataRow(true, 0)]
    [DataRow(true, 300)]
    public async Task NotificationStorageAppliesTtlAndLatestValuePolicy(bool retain, int ttl)
    {
        ServiceProvider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
            ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
        var storage = ServiceProvider.GetRequiredService<IStateflowsNotificationsStorage>();
        BehaviorId id = new ActivityId("output-retained", $"policy-{retain}-{ttl}");
        var names = new[] { Event<Notice>.Name };
        foreach (var value in new[] { 1, 2 })
        {
            var notification = new Notice(value).ToEventHolder(id);
            notification.SentAt = DateTime.Now;
            notification.Retained = retain;
            notification.TimeToLive = ttl;
            await storage.SaveNotificationsAsync(id, [notification]);
        }
        var history = (await storage.GetNotificationsAsync(id, names, DateTime.MinValue))
            .Cast<EventHolder<Notice>>().Select(n => n.Payload.Value).ToArray();
        CollectionAssert.AreEqual(retain ? new[] { 2 } : ttl > 0 ? new[] { 1, 2 } : Array.Empty<int>(), history);
        var latest = (await storage.GetNotificationsAsync(id, names, DateTime.Now.AddDays(1)))
            .Cast<EventHolder<Notice>>().Select(n => n.Payload.Value).ToArray();
        CollectionAssert.AreEqual(retain ? new[] { 2 } : Array.Empty<int>(), latest);
    }

    [TestMethod]
    public async Task RequestsWithoutTimestampNeverReturnNonretainedHistory()
    {
        ServiceProvider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
            ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
        var storage = ServiceProvider.GetRequiredService<IStateflowsNotificationsStorage>();
        using var activity = Locate("output-default", "future-history");
        var notification = new Notice(1).ToEventHolder(activity.Id);
        notification.SentAt = DateTime.Now.AddSeconds(10);
        notification.TimeToLive = 300;
        await storage.SaveNotificationsAsync(activity.Id, [notification]);
        Assert.AreEqual(0, (await activity.GetOutputAsync<Notice>()).Count());
        Assert.AreEqual(0, (await activity.GetOutputAsync([Event<Notice>.Name])).Count());
        CollectionAssert.AreEqual(new[] { 1 },
            (await activity.GetOutputAsync<Notice>(DateTime.MinValue)).Select(n => n.Value).ToArray());
    }

    [TestMethod]
    public void PublicationScopeCanBeOverriddenAndNestedOutputPinsRemainBoundaries()
    {
        Graph? graph = null;
        new ServiceCollection().AddStateflows(sf => sf.AddActivities(activities => activities.AddActivity("model", activity =>
        {
            graph = ((ActivityBuilder)activity).Graph;
            activity.AddInitial(initial => initial.AddControlFlow("publish"))
                .AddPublishEventAction<Notice, PublishMany>("publish", node =>
                    node.SetPublishScope(PublishScope.Parent | PublishScope.Owner))
                .AddOutputPin<int>("root-result", pin => pin.SetPublishScope(PublishScope.None))
                .AddInputPin<int>(pin => pin.AddFlow("scope"))
                .AddStructuredActivity("scope", scope => scope
                    .AddInputPin<int>(pin => pin.AddFlow("scope-result"))
                    .AddOutputPin<int>("scope-result"));
        })));
        var publish = graph!.Nodes.Values.Single(node => node.Name == "publish");
        Assert.AreEqual(PublishScope.Parent | PublishScope.Owner, publish.PublishScope);
        var output = graph.OutputPins.Single();
        Assert.AreEqual(NodeType.PublishEventAction, output.Type);
        Assert.AreEqual(PublishScope.None, output.PublishScope);
        Assert.AreEqual(NodeOptions.None, output.Options);
        Assert.IsFalse(output.Retain);
        Assert.IsFalse(output.ImmediatePublish);
        Assert.AreEqual(0, output.TimeToLive);
        Assert.AreEqual(NodeType.OutputPin, graph.Nodes.Values.Single(node => node.Name == "scope").OutputPins.Single().Type);
    }

    public sealed record Notice(int Value);

    public sealed class PublishMany : IPublishEventActionNode<Notice>
    {
        public static PublishScope PublishScope => PublishScope.Self;
        public static int TimeToLive => 37;
        public static bool Retain => true;
        public static bool ImmediatePublish => true;
        public Task<IEnumerable<Notice>> GenerateEventsAsync()
            => Task.FromResult<IEnumerable<Notice>>([new(1), new(2)]);
    }

    public sealed class PublishNone : IPublishEventActionNode<Notice>
    {
        public static PublishScope PublishScope => PublishScope.None;
        public Task<IEnumerable<Notice>> GenerateEventsAsync()
            => Task.FromResult<IEnumerable<Notice>>([new(1)]);
    }
}
