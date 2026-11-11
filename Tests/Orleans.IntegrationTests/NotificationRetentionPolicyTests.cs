using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;

namespace Orleans.IntegrationTests;

public sealed partial class BehaviorNotificationTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    [Timeout(30_000)]
    public async Task TransientOutboxAcceptancePreservesHistoryAndDeduplicatesAfterStorageFailure(bool commitBeforeFailure)
    {
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), new NotificationDeliveryCallFilter(),
            Channel.CreateUnbounded<int>().Writer, storage);
        await host.StartAsync();
        try
        {
            var publisher = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "transient-outbox-copy");
            var subscriptions = GetSubscriptions(host.Services, publisher);
            var retained = CreateNotification(new CustomNotification { Text = "retained" }, publisher, true, 0);
            await subscriptions.PublishAsync([retained]);
            var transient = CreateNotification(new CustomNotification { Text = "transient" }, publisher, timeToLive: 0);
            var source = GetBehaviorGrainKey(publisher);
            storage.FailNextNotificationsWrite(commitBeforeFailure);
            await Assert.ThrowsExactlyAsync<OrleansException>(
                () => subscriptions.PublishOutboxAsync(source, 1, [transient]));

            await host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0)
                .ForceActivationCollection(TimeSpan.Zero);
            await subscriptions.PublishOutboxAsync(source, 1, [transient]);
            var writes = storage.NotificationsWrites;
            await subscriptions.PublishOutboxAsync(source, 1, [transient]);

            var persisted = storage.GetNotificationsState(publisher);
            Assert.AreEqual(1L, persisted.PublicationWatermarks[source]);
            Assert.AreEqual(retained.Id, persisted.History.Single().Notification.Id);
            Assert.AreEqual(writes, storage.NotificationsWrites,
                "Replaying an accepted transient outbox publication must not write state again.");
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    [Timeout(30_000)]
    public async Task ActivationPrunesLegacyTransientHistoryAndSupersededRetainedRetries()
    {
        var calls = new NotificationDeliveryCallFilter();
        calls.ReturnStatuses[Event<CustomNotification>.Name] = EventStatus.Failed;
        calls.ReturnStatuses[Event<AnotherNotification>.Name] = EventStatus.Failed;
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), calls, Channel.CreateUnbounded<int>().Writer, storage);
        await host.StartAsync();
        try
        {
            var publisher = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "legacy-policy");
            var subscriber = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "legacy-policy-subscriber");
            var subscriptions = GetSubscriptions(host.Services, publisher);
            var names = new[] { Event<CustomNotification>.Name, Event<AnotherNotification>.Name };
            await subscriptions.AddSubscriptionAsync(subscriber, names);
            var old = CreateNotification(new CustomNotification { Text = "old-retained" }, publisher);
            var latest = CreateNotification(new CustomNotification { Text = "latest" }, publisher, true, 0);
            var transient = CreateNotification(new AnotherNotification { Count = 1 }, publisher);
            await subscriptions.PublishAsync([old, latest, transient]).WaitAsync(DeliveryTimeout);
            await calls.ReadAttemptAsync(names[0], failed: false);
            var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
            await management.ForceActivationCollection(TimeSpan.Zero);
            storage.UpdateNotificationsState(publisher, state =>
            {
                var previous = state.History.Single(n => n.Notification.Id == old.Id);
                previous.Notification = previous.Notification with { Retained = true, TimeToLive = 0 };
                var zeroTtl = state.History.Single(n => n.Notification.Id == transient.Id);
                zeroTtl.Notification = zeroTtl.Notification with { TimeToLive = 0 };
            });
            var history = await subscriptions.GetNotificationsAsync(DateTime.MinValue, names);
            Assert.AreEqual(latest.Id, history.Single().Id);
            Assert.AreEqual(latest.Id,
                (await subscriptions.GetPendingDeliveriesAsync(GetBehaviorGrainKey(subscriber), 10)).Single().Notification.Id);
            Assert.AreEqual(1, storage.GetNotificationsState(publisher).History.Count);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    [Timeout(30_000)]
    public async Task ZeroTtlNonretainedNotificationsAreDeliveredOnceWithoutHistoryOrRetry()
    {
        var calls = new NotificationDeliveryCallFilter();
        calls.ReturnStatuses[Event<CustomNotification>.Name] = EventStatus.Failed;
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), calls, Channel.CreateUnbounded<int>().Writer, storage);
        await host.StartAsync();
        try
        {
            var publisher = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "transient-policy");
            var subscriber = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "transient-policy-subscriber");
            var subscriptions = GetSubscriptions(host.Services, publisher);
            var names = new[] { Event<CustomNotification>.Name };
            await subscriptions.AddSubscriptionAsync(subscriber, names);
            var writes = storage.NotificationsWrites;
            var first = CreateNotification(new CustomNotification { Text = "first" }, publisher, timeToLive: 0);
            await subscriptions.PublishAsync([first]).WaitAsync(DeliveryTimeout);
            Assert.AreEqual(first.Id, (await calls.ReadAttemptAsync(names[0], failed: false)).NotificationId);
            Assert.AreEqual(0, (await subscriptions.GetNotificationsAsync(DateTime.MinValue, names)).Length);
            Assert.AreEqual(0, (await subscriptions.GetPendingDeliveriesAsync(GetBehaviorGrainKey(subscriber), 10)).Length);

            await ReceiveDeliveryReminderAsync(subscriptions);
            var second = CreateNotification(new CustomNotification { Text = "second" }, publisher, timeToLive: 0);
            await subscriptions.PublishAsync([second]).WaitAsync(DeliveryTimeout);
            Assert.AreEqual(second.Id, (await calls.ReadAttemptAsync(names[0], failed: false)).NotificationId,
                "A reminder or subsequent publication must not redeliver a failed transient notification.");
            Assert.AreEqual(0, (await subscriptions.GetNotificationsAsync(null, names)).Length);
            Assert.AreEqual(writes, storage.NotificationsWrites,
                "Transient publication must not persist notification history or delivery state.");
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(300)]
    [Timeout(30_000)]
    public async Task RetainedPublicationSupersedesHistoryAndPendingRetries(int ttl)
    {
        var calls = new NotificationDeliveryCallFilter();
        calls.ReturnStatuses[Event<CustomNotification>.Name] = EventStatus.Failed;
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), calls, Channel.CreateUnbounded<int>().Writer, storage);
        await host.StartAsync();
        try
        {
            var publisher = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", $"retained-policy-{ttl}");
            var subscriber = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", $"retained-policy-subscriber-{ttl}");
            var key = GetBehaviorGrainKey(subscriber);
            var subscriptions = GetSubscriptions(host.Services, publisher);
            var names = new[] { Event<CustomNotification>.Name };
            await subscriptions.AddSubscriptionAsync(subscriber, names);
            var first = CreateNotification(new CustomNotification { Text = "first" }, publisher, true, ttl);
            await subscriptions.PublishAsync([first]).WaitAsync(DeliveryTimeout);
            await calls.ReadAttemptAsync(names[0], failed: false);
            var previousDelivery = (await subscriptions.GetPendingDeliveriesAsync(key, 10)).Single();

            var latest = CreateNotification(new CustomNotification { Text = "latest" }, publisher, true, ttl);
            await subscriptions.PublishAsync([latest]).WaitAsync(DeliveryTimeout);
            var pending = await subscriptions.GetPendingDeliveriesAsync(key, 10);
            Assert.AreEqual(latest.Id, pending.Single().Notification.Id);
            Assert.IsFalse(await subscriptions.IsDeliveryPendingAsync(key, previousDelivery.DeliveryId));
            Assert.AreEqual(latest.Id, (await subscriptions.GetNotificationsAsync(null, names)).Single().Id);
            Assert.AreEqual(latest.Id, (await subscriptions.GetNotificationsAsync(DateTime.MinValue, names)).Single().Id);
            Assert.AreEqual(1, storage.GetNotificationsState(publisher).History.Count,
                "Superseded retained payloads must be physically removed, not merely hidden from queries.");

            await subscriptions.ConfirmDeliveriesAsync(key, [pending.Single().DeliveryId]);
            Assert.AreEqual(latest.Id, (await subscriptions.GetNotificationsAsync(null, names)).Single().Id,
                "Acknowledgment must not remove the retained requestable value.");

            var expired = CreateNotification(new CustomNotification { Text = "expired" }, publisher, true, 1,
                DateTime.UtcNow.AddSeconds(-2));
            await subscriptions.PublishAsync([expired]).WaitAsync(DeliveryTimeout);
            Assert.AreEqual(0, (await subscriptions.GetNotificationsAsync(DateTime.MinValue, names)).Length);
            Assert.AreEqual(0, (await subscriptions.GetPendingDeliveriesAsync(key, 10)).Length);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    [Timeout(30_000)]
    public async Task PositiveTtlNonretainedNotificationsKeepHistoryAndRetryUntilExpiration()
    {
        var calls = new NotificationDeliveryCallFilter();
        calls.ReturnStatuses[Event<CustomNotification>.Name] = EventStatus.Failed;
        using var host = CreateHost(GetFreeTcpPorts(2), calls);
        await host.StartAsync();
        try
        {
            var publisher = CreatePublisherId("history-policy");
            var subscriber = CreateSubscriberId("history-policy-subscriber");
            var subscriptions = GetSubscriptions(host.Services, publisher);
            var names = new[] { Event<CustomNotification>.Name };
            var key = GetBehaviorGrainKey(subscriber);
            await subscriptions.AddSubscriptionAsync(subscriber, names);
            var first = CreateNotification(new CustomNotification { Text = "first" }, publisher, timeToLive: 3);
            var second = CreateNotification(new CustomNotification { Text = "second" }, publisher, timeToLive: 3);
            await subscriptions.PublishAsync([first, second]).WaitAsync(DeliveryTimeout);
            await calls.ReadAttemptAsync(names[0], failed: false);
            Assert.AreEqual(0, (await subscriptions.GetNotificationsAsync(null, names)).Length);
            CollectionAssert.AreEqual(new[] { first.Id, second.Id },
                (await subscriptions.GetNotificationsAsync(DateTime.MinValue, names)).Select(n => n.Id).ToArray());
            Assert.AreEqual(2, (await subscriptions.GetPendingDeliveriesAsync(key, 10)).Length);
            await ReceiveDeliveryReminderAsync(subscriptions);
            Assert.AreEqual(first.Id, (await calls.ReadAttemptAsync(names[0], failed: false)).NotificationId);

            await WaitForExpirationAsync(second.SentAt, second.TimeToLive);
            await ReceiveDeliveryReminderAsync(subscriptions);
            Assert.AreEqual(0, (await subscriptions.GetNotificationsAsync(DateTime.MinValue, names)).Length);
            Assert.AreEqual(0, (await subscriptions.GetPendingDeliveriesAsync(key, 10)).Length);
        }
        finally
        {
            await host.StopAsync();
        }
    }
}
