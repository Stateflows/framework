using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using Stateflows;
using Stateflows.Actions;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

public sealed partial class BehaviorNotificationTests
{
    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(5);
    private const string NotificationDeliveryReminderName = StateflowsSubscriptions.ReminderName;

    [TestMethod]
    public async Task PublishReturnsAndSubscriptionsRemainCallableWhileBehaviorDeliveryIsBlocked()
    {
        var calls = new NotificationDeliveryCallFilter
        {
            BlockNotificationName = Event<CustomNotification>.Name
        };
        using var host = CreateHost(GetFreeTcpPorts(2), calls);
        await host.StartAsync();
        try
        {
            var publisherId = CreatePublisherId("blocked-publisher");
            var blockedSubscriber = CreateSubscriberId("blocked-subscriber");
            var healthySubscriber = CreateSubscriberId("healthy-subscriber");
            var subscriptions = GetSubscriptions(host.Services, publisherId);
            await subscriptions.AddSubscriptionAsync(blockedSubscriber, [Event<CustomNotification>.Name]);
            await subscriptions.AddSubscriptionAsync(healthySubscriber, [Event<AnotherNotification>.Name]);

            var blocked = CreateNotification(new CustomNotification { Text = "blocked" }, publisherId);
            var healthy = CreateNotification(new AnotherNotification { Count = 17 }, publisherId);
            await subscriptions.PublishAsync([blocked]).WaitAsync(DeliveryTimeout);

            await calls.BlockEntered.Task.WaitAsync(DeliveryTimeout);
            var queued = CreateNotification(new CustomNotification { Text = "queued-behind-blocked" }, publisherId);
            await subscriptions.PublishAsync([queued, healthy]).WaitAsync(DeliveryTimeout);
            var healthyDelivery = await calls.ReadAttemptAsync(
                Event<AnotherNotification>.Name, failed: false);
            Assert.AreEqual(healthy.Id, healthyDelivery.NotificationId);
            await calls.ReadConfirmationAsync(GetBehaviorGrainKey(healthySubscriber));

            var history = await subscriptions.GetNotificationsAsync(
                DateTime.MinValue, [Event<CustomNotification>.Name, Event<AnotherNotification>.Name]);
            CollectionAssert.AreEquivalent(
                new[] { blocked.Id, healthy.Id, queued.Id },
                history.Select(notification => notification.Id).ToArray());

            var blockedPending = await subscriptions.GetPendingDeliveriesAsync(
                GetBehaviorGrainKey(blockedSubscriber), 10);
            Assert.AreEqual(2, blockedPending.Length);
            Assert.AreEqual(blocked.Id, blockedPending[0].Notification.Id);
            Assert.AreEqual(queued.Id, blockedPending[1].Notification.Id);
            Assert.IsTrue(calls.DispatchStarted.ContainsKey(blocked.Id));
            Assert.IsFalse(calls.DispatchStarted.ContainsKey(queued.Id),
                "N2 must remain queued while N1 is blocked, even after the other subscriber has confirmed delivery.");
            calls.ReleaseBlockedDelivery.TrySetResult();
            Assert.AreEqual(blocked.Id,
                (await calls.ReadAttemptAsync(Event<CustomNotification>.Name, failed: false)).NotificationId);
            Assert.AreEqual(queued.Id,
                (await calls.ReadAttemptAsync(Event<CustomNotification>.Name, failed: false)).NotificationId);
            await calls.ReadConfirmationAsync(GetBehaviorGrainKey(blockedSubscriber));
        }
        finally
        {
            calls.ReleaseBlockedDelivery.TrySetResult();
            await host.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(EventStatus.Failed)]
    [DataRow(EventStatus.Overloaded)]
    public async Task FailedDeliveryIsIsolatedAndRetriedBySubscriptionsReminder(EventStatus failure)
    {
        var calls = new NotificationDeliveryCallFilter();
        calls.ReturnStatusesOnce[Event<CustomNotification>.Name] = failure;
        using var host = CreateHost(GetFreeTcpPorts(2), calls);
        await host.StartAsync();
        try
        {
            var publisherId = CreatePublisherId("failure-publisher");
            var failedSubscriber = CreateSubscriberId("failed-subscriber");
            var healthySubscriber = CreateSubscriberId("independent-subscriber");
            var subscriptions = GetSubscriptions(host.Services, publisherId);
            await subscriptions.AddSubscriptionAsync(failedSubscriber, [Event<CustomNotification>.Name]);
            await subscriptions.AddSubscriptionAsync(healthySubscriber, [Event<AnotherNotification>.Name]);

            var failed = CreateNotification(new CustomNotification { Text = "retry-me" }, publisherId);
            var healthy = CreateNotification(new AnotherNotification { Count = 23 }, publisherId);
            await subscriptions.PublishAsync([failed, healthy]).WaitAsync(DeliveryTimeout);

            var firstFailure = await calls.ReadAttemptAsync(Event<CustomNotification>.Name, failed: true);
            Assert.AreEqual(failed.Id, firstFailure.NotificationId);
            var healthyDelivery = await calls.ReadAttemptAsync(Event<AnotherNotification>.Name, failed: false);
            Assert.AreEqual(healthy.Id, healthyDelivery.NotificationId);
            await calls.ReadConfirmationAsync(GetBehaviorGrainKey(healthySubscriber));

            var pending = await subscriptions.GetPendingDeliveriesAsync(GetBehaviorGrainKey(failedSubscriber), 10);
            Assert.AreEqual(1, pending.Length);
            Assert.AreEqual(failed.Id, pending[0].Notification.Id);
            Assert.AreEqual(0,
                (await subscriptions.GetPendingDeliveriesAsync(GetBehaviorGrainKey(healthySubscriber), 10)).Length,
                "A failed subscriber must not prevent an independent subscriber from being confirmed.");

            await ReceiveDeliveryReminderAsync(subscriptions);
            var retry = await calls.ReadAttemptAsync(Event<CustomNotification>.Name, failed: false);
            Assert.AreEqual(failed.Id, retry.NotificationId);
            await calls.ReadConfirmationAsync(GetBehaviorGrainKey(failedSubscriber));
            Assert.AreEqual(0,
                (await subscriptions.GetPendingDeliveriesAsync(GetBehaviorGrainKey(failedSubscriber), 10)).Length);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task NotConsumedAndInvalidDeliveriesAreConfirmed()
    {
        var calls = new NotificationDeliveryCallFilter();
        calls.ReturnStatuses[Event<CustomNotification>.Name] = EventStatus.NotConsumed;
        calls.ReturnStatuses[Event<AnotherNotification>.Name] = EventStatus.Invalid;
        using var host = CreateHost(GetFreeTcpPorts(2), calls);
        await host.StartAsync();
        try
        {
            var publisherId = CreatePublisherId("status-publisher");
            var notConsumedSubscriber = CreateSubscriberId("not-consumed");
            var invalidSubscriber = CreateSubscriberId("invalid");
            var subscriptions = GetSubscriptions(host.Services, publisherId);
            await subscriptions.AddSubscriptionAsync(notConsumedSubscriber, [Event<CustomNotification>.Name]);
            await subscriptions.AddSubscriptionAsync(invalidSubscriber, [Event<AnotherNotification>.Name]);

            var notConsumed = CreateNotification(new CustomNotification { Text = "not-consumed" }, publisherId);
            var invalid = CreateNotification(new AnotherNotification { Count = 31 }, publisherId);
            await subscriptions.PublishAsync([notConsumed, invalid]).WaitAsync(DeliveryTimeout);

            Assert.AreEqual(EventStatus.NotConsumed,
                (await calls.ReadAttemptAsync(Event<CustomNotification>.Name, failed: false)).Status);
            Assert.AreEqual(EventStatus.Invalid,
                (await calls.ReadAttemptAsync(Event<AnotherNotification>.Name, failed: false)).Status);
            await calls.ReadConfirmationAsync(GetBehaviorGrainKey(notConsumedSubscriber));
            await calls.ReadConfirmationAsync(GetBehaviorGrainKey(invalidSubscriber));

            Assert.AreEqual(0,
                (await subscriptions.GetPendingDeliveriesAsync(GetBehaviorGrainKey(notConsumedSubscriber), 10)).Length);
            Assert.AreEqual(0,
                (await subscriptions.GetPendingDeliveriesAsync(GetBehaviorGrainKey(invalidSubscriber), 10)).Length);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task HistoryPendingDeliveriesAndSubscriptionsSurviveSiloActivationDeactivation()
    {
        var calls = new NotificationDeliveryCallFilter();
        calls.FailBeforeInvokeOnce[Event<CustomNotification>.Name] = 0;
        using var host = CreateHost(GetFreeTcpPorts(2), calls);
        await host.StartAsync();
        try
        {
            var publisherId = CreatePublisherId("durable-publisher");
            var subscriberId = CreateSubscriberId("durable-subscriber");
            var subscriptions = GetSubscriptions(host.Services, publisherId);
            await subscriptions.AddSubscriptionAsync(subscriberId, [Event<CustomNotification>.Name]);

            var notification = CreateNotification(new CustomNotification { Text = "durable" }, publisherId);
            await subscriptions.PublishAsync([notification]).WaitAsync(DeliveryTimeout);
            await calls.ReadAttemptAsync(Event<CustomNotification>.Name, failed: true);
            calls.BlockNotificationName = Event<CustomNotification>.Name;

            var beforeDeactivation = await subscriptions.GetNotificationsAsync(
                DateTime.MinValue, [Event<CustomNotification>.Name]);
            Assert.AreEqual(notification.Id, beforeDeactivation.Single().Id);
            var pendingBeforeDeactivation = await subscriptions.GetPendingDeliveriesAsync(
                GetBehaviorGrainKey(subscriberId), 10);
            Assert.AreEqual(1, pendingBeforeDeactivation.Length);
            var oldActivation = calls.LastSubscriptionsActivation;
            Assert.IsNotNull(oldActivation);

            var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
            await management.ForceActivationCollection(TimeSpan.Zero);

            var afterDeactivation = await subscriptions.GetNotificationsAsync(
                DateTime.MinValue, [Event<CustomNotification>.Name]);
            await calls.BlockEntered.Task.WaitAsync(DeliveryTimeout);
            Assert.AreNotSame(oldActivation, calls.LastSubscriptionsActivation,
                "The durable read must occur in a different subscriptions-grain activation.");
            Assert.AreEqual(notification.Id, afterDeactivation.Single().Id);
            Assert.IsNotNull(((EventHolder)afterDeactivation[0]).SenderId);
            var pendingAfterDeactivation = await subscriptions.GetPendingDeliveriesAsync(
                GetBehaviorGrainKey(subscriberId), 10);
            Assert.AreEqual(1, pendingAfterDeactivation.Length);
            Assert.AreEqual(pendingBeforeDeactivation[0].DeliveryId, pendingAfterDeactivation[0].DeliveryId);

            await ReceiveDeliveryReminderAsync(subscriptions);
            calls.ReleaseBlockedDelivery.TrySetResult();
            var retried = await calls.ReadAttemptAsync(Event<CustomNotification>.Name, failed: false);
            Assert.AreEqual(notification.Id, retried.NotificationId);
            await calls.ReadConfirmationAsync(GetBehaviorGrainKey(subscriberId));

            var next = CreateNotification(new CustomNotification { Text = "subscription-restored" }, publisherId);
            await subscriptions.PublishAsync([next]).WaitAsync(DeliveryTimeout);
            Assert.AreEqual(next.Id,
                (await calls.ReadAttemptAsync(Event<CustomNotification>.Name, failed: false)).NotificationId,
                "The subscription itself must also have been restored from durable state.");
        }
        finally
        {
            calls.ReleaseBlockedDelivery.TrySetResult();
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task ExpiredHistoryAndRetainedNotificationsAreExcludedAndCleanedUp()
    {
        var calls = new NotificationDeliveryCallFilter();
        using var host = CreateHost(GetFreeTcpPorts(2), calls);
        await host.StartAsync();
        try
        {
            var publisherId = CreatePublisherId("ttl-publisher");
            var subscriberId = CreateSubscriberId("ttl-subscriber");
            var subscriptions = GetSubscriptions(host.Services, publisherId);
            await subscriptions.AddSubscriptionAsync(subscriberId, [Event<CustomNotification>.Name]);

            var expired = CreateNotification(new CustomNotification { Text = "expired" }, publisherId,
                timeToLive: 1, sentAt: DateTime.UtcNow.AddSeconds(-2));
            var oldRetained = CreateNotification(
                new RetainedCustomNotification { Value = 1 }, publisherId, retained: true);
            var expiredRetained = CreateNotification(
                new RetainedCustomNotification { Value = 2 }, publisherId, retained: true,
                timeToLive: 1, sentAt: DateTime.UtcNow.AddSeconds(-2));
            var expiring = CreateNotification(
                new AnotherNotification { Count = 41 }, publisherId,
                timeToLive: 2);
            await subscriptions.PublishAsync([expired, oldRetained, expiredRetained, expiring])
                .WaitAsync(DeliveryTimeout);

            var history = await subscriptions.GetNotificationsAsync(
                DateTime.MinValue, [Event<CustomNotification>.Name]);
            Assert.AreEqual(0, history.Length, "Expired notifications must not appear in history replay.");
            Assert.AreEqual(0,
                (await subscriptions.GetPendingDeliveriesAsync(GetBehaviorGrainKey(subscriberId), 10)).Length,
                "Expired notifications must not enter a subscriber's pending queue.");
            Assert.AreEqual(expiring.Id,
                (await subscriptions.GetNotificationsAsync(
                    DateTime.MinValue, [Event<AnotherNotification>.Name])).Single().Id);

            var retained = await subscriptions.GetNotificationsAsync(
                DateTime.UtcNow.AddDays(1), [Event<RetainedCustomNotification>.Name]);
            Assert.AreEqual(0, retained.Length,
                "An expired latest retained value must not cause an older retained value to be replayed.");

            var newestRetained = CreateNotification(
                new RetainedCustomNotification { Value = 3 }, publisherId, retained: true);
            await subscriptions.PublishAsync([newestRetained]).WaitAsync(DeliveryTimeout);
            retained = await subscriptions.GetNotificationsAsync(
                DateTime.UtcNow.AddDays(1), [Event<RetainedCustomNotification>.Name]);
            Assert.AreEqual(3, ((EventHolder<RetainedCustomNotification>)(EventHolder)retained.Single()).Payload.Value,
                "Replay must select the newest retained notification.");

            await WaitForExpirationAsync(expiring.SentAt, expiring.TimeToLive);
            await ReceiveDeliveryReminderAsync(subscriptions);
            Assert.IsFalse(calls.ObservedAttempts.ContainsKey(expired.Id),
                "An expired notification must never be dispatched to a behavior.");
            var cleaned = await subscriptions.GetNotificationsAsync(
                DateTime.UtcNow.AddDays(1), [Event<RetainedCustomNotification>.Name]);
            Assert.AreEqual(3, ((EventHolder<RetainedCustomNotification>)(EventHolder)cleaned.Single()).Payload.Value);
            Assert.AreEqual(0,
                (await subscriptions.GetNotificationsAsync(DateTime.MinValue, [Event<CustomNotification>.Name])).Length);
            Assert.AreEqual(0,
                (await subscriptions.GetNotificationsAsync(DateTime.MinValue, [Event<AnotherNotification>.Name])).Length,
                "The reminder must remove expired history from durable storage.");
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task WatchReplaysHistoryAndRecoversAfterSubscriptionsGrainReactivation()
    {
        var calls = new NotificationDeliveryCallFilter();
        using var host = CreateHost(GetFreeTcpPorts(2), calls);
        await host.StartAsync();
        try
        {
            var publisherId = CreatePublisherId("watch-history-publisher");
            var subscriptions = GetSubscriptions(host.Services, publisherId);
            var previous = CreateNotification(new CustomNotification { Text = "history" }, publisherId);
            await subscriptions.PublishAsync([previous]).WaitAsync(DeliveryTimeout);

            var replaySince = DateTime.UtcNow.AddMinutes(-5);
            var history = await subscriptions.GetNotificationsAsync(replaySince, [Event<CustomNotification>.Name]);
            Assert.AreEqual(previous.Id, history.Single().Id);
            Assert.IsNotNull(((EventHolder)history[0]).SenderId);

            var clusterClient = host.Services.GetRequiredService<IClusterClient>();
            await using var behavior = new GrainBehavior(
                "host", publisherId, clusterClient, new DeliveryLogger(), TimeSpan.FromMilliseconds(50));
            var notifications = Channel.CreateUnbounded<CustomNotification>();
            await using var watcher = await behavior.WatchAsync<CustomNotification>(
                notification =>
                {
                    notifications.Writer.TryWrite(notification);
                    return Task.CompletedTask;
                },
                replaySince);
            var replayed = await notifications.Reader.ReadAsync().AsTask().WaitAsync(DeliveryTimeout);
            Assert.AreEqual("history", replayed.Text);

            var oldActivation = await calls.ReadWatchActivationAsync();
            var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
            await management.ForceActivationCollection(TimeSpan.Zero);
            var renewedActivation = await calls.ReadDifferentWatchActivationAsync(oldActivation);
            Assert.AreNotSame(oldActivation, renewedActivation);

            var afterReactivation = CreateNotification(
                new CustomNotification { Text = "after-reactivation" }, publisherId);
            await subscriptions.PublishAsync([afterReactivation]).WaitAsync(DeliveryTimeout);
            var delivered = await ReadNotificationAsync(notifications.Reader, "after-reactivation");
            Assert.AreEqual("after-reactivation", delivered.Text);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task LostAcknowledgementRetryDeliversSameNotificationId()
    {
        var calls = new NotificationDeliveryCallFilter();
        calls.LoseResponseAfterInvokeOnce[Event<CustomNotification>.Name] = 0;
        using var host = CreateHost(GetFreeTcpPorts(2), calls);
        await host.StartAsync();
        try
        {
            var publisherId = CreatePublisherId("ack-lost-publisher");
            var subscriberId = CreateSubscriberId("ack-lost-subscriber");
            var subscriptions = GetSubscriptions(host.Services, publisherId);
            await subscriptions.AddSubscriptionAsync(subscriberId, [Event<CustomNotification>.Name]);

            var notification = CreateNotification(new CustomNotification { Text = "same-id" }, publisherId);
            await subscriptions.PublishAsync([notification]).WaitAsync(DeliveryTimeout);
            var lostAttempt = await calls.ReadAttemptAsync(Event<CustomNotification>.Name, failed: true);
            Assert.AreEqual(notification.Id, lostAttempt.NotificationId);
            Assert.AreEqual(1,
                (await subscriptions.GetPendingDeliveriesAsync(GetBehaviorGrainKey(subscriberId), 10)).Length);

            var retryAttempt = calls.ReadAttemptAsync(Event<CustomNotification>.Name, failed: false);
            for (var reminderAttempt = 0; reminderAttempt < 5 && !retryAttempt.IsCompleted; reminderAttempt++)
            {
                await ReceiveDeliveryReminderAsync(subscriptions);
                await Task.Yield();
            }

            var retried = await retryAttempt.WaitAsync(DeliveryTimeout);
            Assert.AreEqual(notification.Id, retried.NotificationId,
                "A retry after a lost acknowledgement must preserve the original notification identifier.");
            Assert.AreEqual(EventStatus.Duplicated, retried.Status,
                "A committed receipt must acknowledge retry without running the behavior again.");
            await calls.ReadConfirmationAsync(GetBehaviorGrainKey(subscriberId));
            Assert.AreEqual(0,
                (await subscriptions.GetPendingDeliveriesAsync(GetBehaviorGrainKey(subscriberId), 10)).Length);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    public sealed class SubscribeToOwnRetainedNotification { }

    [TestMethod]
    public async Task SelfSubscriptionRegistersBeforeSendReturnsAndDeliversRetainedIndependently()
    {
        var calls = new NotificationDeliveryCallFilter
        {
            BlockNotificationName = Event<RetainedCustomNotification>.Name
        };
        var received = Channel.CreateUnbounded<int>();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), calls, received.Writer);
        await host.StartAsync();
        try
        {
            using var scope = host.Services.CreateScope();
            var locator = scope.ServiceProvider.GetRequiredService<IStateMachineLocator>();
            var machineId = new StateMachineId("self-subscribing-machine", $"self-{Guid.NewGuid():N}");
            var machine = await LocateStateMachineAsync(locator, machineId, DeliveryTimeout);
            BehaviorId behaviorId = machineId;
            var subscriberKey = GetBehaviorGrainKey(behaviorId);
            var subscriptions = GetSubscriptions(host.Services, behaviorId);

            Assert.AreEqual(EventStatus.Consumed,
                (await machine.SendAsync(new PublishRetainedNotification { Value = 1 }).WaitAsync(DeliveryTimeout)).Status);
            var retained = (await subscriptions.GetNotificationsAsync(
                null, [Event<RetainedCustomNotification>.Name])).Single();

            Assert.AreEqual(EventStatus.Consumed,
                (await machine.SendAsync(new SubscribeToOwnRetainedNotification()).WaitAsync(DeliveryTimeout)).Status,
                "Self-subscription must complete without waiting for its own retained notification delivery.");

            // Delivery to the behavior is blocked by the filter, so the pending replay can only exist if
            // registration was persisted before the Subscribe turn returned.
            var pending = await subscriptions.GetPendingDeliveriesAsync(subscriberKey, 10);
            Assert.AreEqual(1, pending.Length);
            Assert.AreEqual(retained.Id, pending[0].Notification.Id);

            await calls.BlockEntered.Task.WaitAsync(DeliveryTimeout);
            calls.ReleaseBlockedDelivery.TrySetResult();
            Assert.AreEqual(retained.Id,
                (await calls.ReadAttemptAsync(Event<RetainedCustomNotification>.Name, failed: false)).NotificationId);
            Assert.AreEqual(1, await ReadValueAsync(received.Reader, 1));
            await calls.ReadConfirmationAsync(subscriberKey);

            Assert.AreEqual(EventStatus.Consumed,
                (await machine.SendAsync(new PublishRetainedNotification { Value = 2 }).WaitAsync(DeliveryTimeout)).Status);
            Assert.AreEqual(2, await ReadValueAsync(received.Reader, 2));
            await calls.ReadConfirmationAsync(subscriberKey);
            Assert.AreEqual(0, (await subscriptions.GetPendingDeliveriesAsync(subscriberKey, 10)).Length);
        }
        finally
        {
            calls.ReleaseBlockedDelivery.TrySetResult();
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task StatelessActionPublicationIsDurableWhenExecutionReturns()
    {
        var calls = new NotificationDeliveryCallFilter();
        var received = Channel.CreateUnbounded<int>();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), calls, received.Writer);
        await host.StartAsync();
        try
        {
            using var scope = host.Services.CreateScope();
            var locator = scope.ServiceProvider.GetRequiredService<IActionLocator>();
            var actionId = new ActionId("publishing-action", $"action-{Guid.NewGuid():N}");
            Assert.IsTrue(locator.TryLocateAction(actionId, out var action));

            var result = await action.ExecuteAsync().WaitAsync(DeliveryTimeout);
            Assert.AreNotEqual(EventStatus.Failed, result.Status);

            var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
            await management.ForceActivationCollection(TimeSpan.Zero);

            BehaviorId behaviorId = actionId;
            var history = await GetSubscriptions(host.Services, behaviorId)
                .GetNotificationsAsync(DateTime.MinValue, [Event<CustomNotification>.Name]);
            var published = history.Single();
            Assert.AreEqual("from-action",
                ((EventHolder<CustomNotification>)(EventHolder)published).Payload.Text);
            Assert.IsTrue(published.SenderId.HasValue);
            Assert.AreEqual(behaviorId, (BehaviorId)published.SenderId.Value);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private static IHost CreateDeliveryScenarioHost(
        int[] ports,
        IIncomingGrainCallFilter filter,
        ChannelWriter<int> retainedValues,
        FaultingNotificationsStorage? storage = null,
        IStateflowsValidator? validator = null)
        => Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddStateflows(stateflows => stateflows
                    .AddStateMachines(machines => machines.AddStateMachine(
                            "self-subscribing-machine",
                            machine => machine
                                .AddInitialState("idle", state => state
                                    .AddInternalTransition<SubscribeToOwnRetainedNotification>(t => t
                                        .AddEffect(c =>
                                            c.Behavior.SubscribeAsync<RetainedCustomNotification>(c.Behavior.Id)))
                                    .AddInternalTransition<PublishRetainedNotification>(t => t
                                        .AddEffect(c => c.Behavior.PublishRetained(
                                            new RetainedCustomNotification { Value = c.Event.Value })))
                                    .AddInternalTransition<RetainedCustomNotification>(t => t
                                        .AddEffect(c =>
                                        {
                                            if (c.Event.Value < 0)
                                                throw new InvalidOperationException("Injected execution failure.");
                                            retainedValues.TryWrite(c.Event.Value);
                                        }))
                                    .AddInternalTransition<RecordValue>(t => t
                                        .AddEffect(c => retainedValues.TryWrite(c.Event.Value)))))
                        .AddStateMachine("deferred-machine", machine => machine
                            .AddInitialState("idle", state => state
                                .AddDeferredEvent<RetainedCustomNotification>()
                                .AddTransition<MoveNextEvent>("running"))
                            .AddState("running", state => state
                                .AddInternalTransition<RetainedCustomNotification>(t => t
                                    .AddEffect(c => retainedValues.TryWrite(c.Event.Value))))))
                    .AddActions(actions => actions.AddAction("publishing-action", c =>
                        {
                            c.Behavior.Publish(new CustomNotification { Text = "from-action" });
                            return Task.CompletedTask;
                        }, b => b.SetIsStateless(true))
                        .AddAction("stateful-action", c =>
                        {
                            retainedValues.TryWrite(99);
                            return Task.CompletedTask;
                        })));
            })
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
            .UseOrleans(silo =>
            {
                silo
                    .UseLocalhostClustering(ports[0], ports[1], null,
                        $"stateflows-delivery-{Guid.NewGuid():N}", $"orleans-delivery-{Guid.NewGuid():N}")
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(filter);
                        if (validator != null) services.AddSingleton(validator);
                    });
                if (storage == null)
                {
                    silo.AddMemoryGrainStorageAsDefault();
                }
                else
                {
                    silo.ConfigureServices(services => services.AddKeyedSingleton<IGrainStorage>(
                        "Default",
                        (provider, _) => storage.Attach(provider.GetRequiredService<IGrainStorageSerializer>())));
                }

                silo
                .UseInMemoryReminderService()
                .AddStateflowsRuntime();
            })
            .Build();

    [TestMethod]
    [DataRow(false, DisplayName = "Rejected before commit")]
    [DataRow(true, DisplayName = "Committed then reported failure")]
    public async Task NotificationsStorageWriteFailureKeepsHistoryAndPendingAtomicAcrossReactivation(bool commitBeforeFailure)
    {
        var calls = new NotificationDeliveryCallFilter();
        // Keep deliveries pending so the durable pending set can be compared with history.
        calls.ReturnStatuses[Event<CustomNotification>.Name] = EventStatus.Failed;
        var storage = new FaultingNotificationsStorage();
        var received = Channel.CreateUnbounded<int>();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), calls, received.Writer, storage);
        await host.StartAsync();
        try
        {
            var publisherId = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", $"pub-{Guid.NewGuid():N}");
            var subscriberId = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", $"sub-{Guid.NewGuid():N}");
            var subscriberKey = GetBehaviorGrainKey(subscriberId);
            var names = new[] { Event<CustomNotification>.Name };
            var subscriptions = GetSubscriptions(host.Services, publisherId);
            await subscriptions.AddSubscriptionAsync(subscriberId, names);

            var committed = CreateNotification(new CustomNotification { Text = "committed" }, publisherId);
            await subscriptions.PublishAsync([committed]).WaitAsync(DeliveryTimeout);
            Assert.AreEqual(committed.Id,
                (await calls.ReadAttemptAsync(Event<CustomNotification>.Name, failed: false)).NotificationId);

            var uncertain = CreateNotification(new CustomNotification { Text = "uncertain" }, publisherId);
            storage.FailNextNotificationsWrite(commitBeforeFailure);
            Exception? publishFailure = null;
            try
            {
                await subscriptions.PublishAsync([uncertain]).WaitAsync(DeliveryTimeout);
            }
            catch (Exception exception) when (exception is not TimeoutException)
            {
                publishFailure = exception;
            }
            Assert.IsNotNull(publishFailure, "A failed notifications write must fail publication.");

            var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
            await management.ForceActivationCollection(TimeSpan.Zero);
            var readsBefore = storage.NotificationsReads;

            var historyIds = (await subscriptions.GetNotificationsAsync(DateTime.MinValue, names))
                .Select(notification => notification.Id).ToArray();
            var pendingIds = (await subscriptions.GetPendingDeliveriesAsync(subscriberKey, 10))
                .Select(delivery => delivery.Notification.Id).ToArray();
            Assert.IsTrue(storage.NotificationsReads > readsBefore,
                "History and pending deliveries must be read from durable storage after reactivation.");

            var expected = commitBeforeFailure
                ? new[] { committed.Id, uncertain.Id }
                : new[] { committed.Id };
            CollectionAssert.AreEqual(expected, historyIds);
            CollectionAssert.AreEqual(expected, pendingIds,
                "Pending deliveries must be committed atomically with notification history.");

            calls.ReturnStatuses.TryRemove(Event<CustomNotification>.Name, out _);
            for (var reminderAttempt = 0;
                 reminderAttempt < 5 && !expected.All(id => calls.ObservedAttempts.TryGetValue(id, out var a) && a.Status != EventStatus.Failed);
                 reminderAttempt++)
            {
                await ReceiveDeliveryReminderAsync(subscriptions);
                await calls.ReadAttemptAsync(Event<CustomNotification>.Name, failed: false);
            }

            foreach (var id in expected)
            {
                Assert.IsTrue(calls.ObservedAttempts.TryGetValue(id, out var attempt) && attempt.Status != EventStatus.Failed,
                    $"Recovered pending delivery '{id}' was not retried successfully.");
            }
            await calls.ReadConfirmationAsync(subscriberKey);
            for (var confirmationAttempt = 0;
                 confirmationAttempt < 5 && (await subscriptions.GetPendingDeliveriesAsync(subscriberKey, 10)).Length > 0;
                 confirmationAttempt++)
            {
                await calls.ReadConfirmationAsync(subscriberKey);
            }
            Assert.AreEqual(0, (await subscriptions.GetPendingDeliveriesAsync(subscriberKey, 10)).Length);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private sealed class FaultingNotificationsStorage : IGrainStorage
    {
        private readonly ConcurrentDictionary<(string StateName, GrainId GrainId), (BinaryData Data, string ETag)> states = new();
        private IGrainStorageSerializer serializer = null!;
        private int failureMode;
        private int contextFailureMode;
        private int contextWrites;
        private int notificationsReads;

        public int NotificationsReads => System.Threading.Volatile.Read(ref notificationsReads);
        public int ContextWrites => System.Threading.Volatile.Read(ref contextWrites);
        public ConcurrentQueue<BehaviorContextState> ContextCommits { get; } = new();

        public IGrainStorage Attach(IGrainStorageSerializer storageSerializer)
        {
            serializer = storageSerializer;
            return this;
        }

        public void FailNextNotificationsWrite(bool commitBeforeFailure)
            => System.Threading.Volatile.Write(ref failureMode, commitBeforeFailure ? 2 : 1);

        public void FailNextContextWrite(bool commitBeforeFailure)
            => System.Threading.Volatile.Write(ref contextFailureMode, commitBeforeFailure ? 2 : 1);

        public bool HasContext(BehaviorId id)
            => states.Keys.Any(p => p.StateName == "Stateflows.BehaviorContext" &&
                p.GrainId.Key.ToString() == GetBehaviorGrainKey(id));

        public Stateflows.Common.Context.StateflowsContext GetContext(BehaviorId id)
        {
            var stored = states.Single(p => p.Key.StateName == "Stateflows.BehaviorContext" &&
                p.Key.GrainId.Key.ToString() == GetBehaviorGrainKey(id)).Value;
            return StateflowsJsonConverter.DeserializeObject<Stateflows.Common.Context.StateflowsContext>(
                serializer.Deserialize<BehaviorContextState>(stored.Data)!.Payload)!;
        }

        public void UpdateContext(BehaviorId id, System.Action<Stateflows.Common.Context.StateflowsContext> update)
        {
            var key = states.Keys.Single(p => p.StateName == "Stateflows.BehaviorContext" &&
                p.GrainId.Key.ToString() == GetBehaviorGrainKey(id));
            var context = GetContext(id);
            update(context);
            states[key] = (serializer.Serialize(new BehaviorContextState
            {
                Payload = StateflowsJsonConverter.SerializePolymorphicObject(context)
            }), Guid.NewGuid().ToString("N"));
        }

        public Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            if (stateName == "Stateflows.Notifications")
            {
                Interlocked.Increment(ref notificationsReads);
            }

            if (states.TryGetValue((stateName, grainId), out var stored))
            {
                grainState.State = serializer.Deserialize<T>(stored.Data);
                grainState.ETag = stored.ETag;
                grainState.RecordExists = true;
            }
            else
            {
                grainState.State = Activator.CreateInstance<T>();
                grainState.ETag = null;
                grainState.RecordExists = false;
            }

            return Task.CompletedTask;
        }

        public Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            if (stateName == "Stateflows.BehaviorContext") Interlocked.Increment(ref contextWrites);
            var mode = stateName == "Stateflows.Notifications" ? Interlocked.Exchange(ref failureMode, 0)
                : stateName == "Stateflows.BehaviorContext" ? Interlocked.Exchange(ref contextFailureMode, 0) : 0;
            if (mode == 1)
            {
                throw new InvalidOperationException("Injected notifications storage write failure.");
            }

            var eTag = Guid.NewGuid().ToString("N");
            states[(stateName, grainId)] = (serializer.Serialize(grainState.State), eTag);
            if (grainState.State is BehaviorContextState committed)
                ContextCommits.Enqueue(committed);
            if (mode == 2)
            {
                throw new InvalidOperationException("Injected notifications storage acknowledgement failure.");
            }

            grainState.ETag = eTag;
            grainState.RecordExists = true;
            return Task.CompletedTask;
        }

        public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            states.TryRemove((stateName, grainId), out _);
            grainState.ETag = null;
            grainState.RecordExists = false;
            return Task.CompletedTask;
        }
    }
    private static async Task<int> ReadValueAsync(ChannelReader<int> values, int expected)
    {
        using var timeout = new CancellationTokenSource(DeliveryTimeout);
        while (await values.WaitToReadAsync(timeout.Token))
        {
            while (values.TryRead(out var value))
            {
                if (value == expected)
                {
                    return value;
                }
            }
        }

        throw new TimeoutException($"Retained value '{expected}' was not delivered.");
    }
    private static BehaviorId CreatePublisherId(string instance)
        => new(BehaviorType.StateMachine, "notifying-machine", instance);

    private static BehaviorId CreateSubscriberId(string instance)
        => new(BehaviorType.StateMachine, "notifying-machine", instance);

    private static OrleansEventHolder CreateNotification<TNotification>(
        TNotification notification,
        BehaviorId publisherId,
        bool retained = false,
        int timeToLive = 0,
        DateTime? sentAt = null)
    {
        EventHolder holder = notification.ToEventHolder(publisherId);
        holder.Id = Guid.NewGuid();
        holder.Retained = retained;
        holder.TimeToLive = timeToLive;
        holder.SentAt = sentAt ?? DateTime.UtcNow;
        return holder;
    }

    private static string GetBehaviorGrainKey(BehaviorId id)
        => StateflowsGrainKeys.ForBehavior("host", id);

    private static Task ReceiveDeliveryReminderAsync(ISubscriptionsGrain subscriptions)
        => subscriptions.ReceiveReminder(NotificationDeliveryReminderName, default);

    private static async Task WaitForExpirationAsync(DateTime sentAt, int timeToLiveSeconds)
    {
        var remaining = sentAt.ToUniversalTime().AddSeconds(timeToLiveSeconds) - DateTime.UtcNow;
        if (remaining > TimeSpan.Zero)
        {
            var expired = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var timer = new Timer(
                static state => ((TaskCompletionSource)state!).TrySetResult(),
                expired,
                remaining,
                Timeout.InfiniteTimeSpan);
            await expired.Task.WaitAsync(DeliveryTimeout);
        }
    }

    private static async Task<CustomNotification> ReadNotificationAsync(
        ChannelReader<CustomNotification> notifications,
        string expectedText)
    {
        using var timeout = new CancellationTokenSource(DeliveryTimeout);
        while (await notifications.WaitToReadAsync(timeout.Token))
        {
            while (notifications.TryRead(out var notification))
            {
                if (notification.Text == expectedText)
                {
                    return notification;
                }
            }
        }

        throw new TimeoutException($"Notification '{expectedText}' was not delivered.");
    }

    private sealed record DeliveryAttempt(string NotificationName, Guid NotificationId, EventStatus Status, bool Failed);

    private sealed record DeliveryConfirmation(string SubscriberGrainKey, Guid[] DeliveryIds);

    private sealed class NotificationDeliveryCallFilter : IIncomingGrainCallFilter
    {
        private int blocked;
        private object? lastSubscriptionsActivation;
        private readonly ConcurrentDictionary<(string NotificationName, bool Failed), ConcurrentQueue<DeliveryAttempt>> deferredAttempts = new();
        private readonly ConcurrentDictionary<string, byte> deferredConfirmations = new();

        public string? BlockNotificationName { get; set; }
        public TaskCompletionSource BlockEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseBlockedDelivery { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public ConcurrentDictionary<string, byte> FailBeforeInvokeOnce { get; } = new();
        public ConcurrentDictionary<string, byte> LoseResponseAfterInvokeOnce { get; } = new();
        public ConcurrentDictionary<string, EventStatus> ReturnStatuses { get; } = new();
        public ConcurrentDictionary<string, EventStatus> ReturnStatusesOnce { get; } = new();
        public ConcurrentDictionary<Guid, byte> DispatchStarted { get; } = new();
        public ConcurrentDictionary<Guid, DeliveryAttempt> ObservedAttempts { get; } = new();
        public Channel<DeliveryAttempt> Attempts { get; } = Channel.CreateUnbounded<DeliveryAttempt>();
        public Channel<DeliveryConfirmation> Confirmations { get; } = Channel.CreateUnbounded<DeliveryConfirmation>();
        public Channel<object> WatchActivations { get; } = Channel.CreateUnbounded<object>();
        public object? LastSubscriptionsActivation => System.Threading.Volatile.Read(ref lastSubscriptionsActivation);

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            if (context.Grain is IBehaviorGrain &&
                context.InterfaceMethod.Name == nameof(IBehaviorGrain.ProcessEventAsync))
            {
                var notification = (OrleansEventHolder)context.Request.GetArgument(0)!;
                DispatchStarted.TryAdd(notification.Id, 0);
                var shouldBlock = notification.Name == BlockNotificationName &&
                    Interlocked.CompareExchange(ref blocked, 1, 0) == 0;
                if (shouldBlock)
                {
                    BlockEntered.TrySetResult();
                    await ReleaseBlockedDelivery.Task;
                }

                if (FailBeforeInvokeOnce.TryRemove(notification.Name, out _))
                {
                    RecordAttempt(new(notification.Name, notification.Id, EventStatus.Failed, true));
                    throw new InvalidOperationException("Injected notification delivery failure.");
                }

                if (ReturnStatuses.TryGetValue(notification.Name, out var requestedStatus))
                {
                    context.Result = new OrleansRequestResult
                    {
                        Status = requestedStatus,
                        Validation = new EventValidation(true)
                    };
                    RecordAttempt(new(notification.Name, notification.Id, requestedStatus, false));
                    return;
                }

                if (ReturnStatusesOnce.TryRemove(notification.Name, out requestedStatus))
                {
                    context.Result = new OrleansRequestResult
                    {
                        Status = requestedStatus,
                        Validation = new EventValidation(true)
                    };
                    RecordAttempt(new(
                        notification.Name, notification.Id, requestedStatus, requestedStatus is EventStatus.Failed or EventStatus.Overloaded));
                    return;
                }

                var attemptRecorded = false;
                try
                {
                    await context.Invoke();
                    var status = context.Result is OrleansRequestResult result
                        ? result.Status
                        : EventStatus.Failed;
                    if (LoseResponseAfterInvokeOnce.TryRemove(notification.Name, out _))
                    {
                        RecordAttempt(new(notification.Name, notification.Id, status, true));
                        attemptRecorded = true;
                        throw new InvalidOperationException("Injected lost acknowledgement after delivery.");
                    }

                    RecordAttempt(new(notification.Name, notification.Id, status, false));
                    attemptRecorded = true;
                }
                catch
                {
                    if (!attemptRecorded)
                    {
                        RecordAttempt(new(notification.Name, notification.Id, EventStatus.Failed, true));
                    }
                    throw;
                }
                return;
            }

            if (context.Grain is ISubscriptionsGrain)
            {
                System.Threading.Volatile.Write(ref lastSubscriptionsActivation, context.Grain);
                var method = context.InterfaceMethod.Name;
                var isWatchRegistration = method == "AddWatchAsync";
                var isConfirmation = method == nameof(ISubscriptionsGrain.ConfirmDeliveriesAsync);
                var confirmationKey = isConfirmation
                    ? (string)context.Request.GetArgument(0)!
                    : null;
                var confirmationIds = isConfirmation
                    ? (Guid[])context.Request.GetArgument(1)!
                    : null;

                await context.Invoke();

                if (isWatchRegistration)
                {
                    WatchActivations.Writer.TryWrite(context.Grain);
                }
                if (isConfirmation)
                {
                    Confirmations.Writer.TryWrite(new(confirmationKey!, confirmationIds!));
                }
                return;
            }

            await context.Invoke();
        }

        private void RecordAttempt(DeliveryAttempt attempt)
        {
            ObservedAttempts[attempt.NotificationId] = attempt;
            Attempts.Writer.TryWrite(attempt);
        }

        public async Task<DeliveryAttempt> ReadAttemptAsync(string notificationName, bool failed)
        {
            var key = (notificationName, failed);
            if (deferredAttempts.TryGetValue(key, out var deferred) && deferred.TryDequeue(out var saved))
            {
                return saved;
            }

            using var timeout = new CancellationTokenSource(DeliveryTimeout);
            while (await Attempts.Reader.WaitToReadAsync(timeout.Token))
            {
                while (Attempts.Reader.TryRead(out var attempt))
                {
                    var attemptKey = (attempt.NotificationName, attempt.Failed);
                    if (attemptKey == key)
                    {
                        return attempt;
                    }

                    deferredAttempts.GetOrAdd(attemptKey, _ => new ConcurrentQueue<DeliveryAttempt>())
                        .Enqueue(attempt);
                }
            }

            throw new TimeoutException($"No matching delivery attempt for '{notificationName}' was observed.");
        }

        public async Task ReadConfirmationAsync(string subscriberGrainKey)
        {
            if (deferredConfirmations.TryRemove(subscriberGrainKey, out _))
            {
                return;
            }

            using var timeout = new CancellationTokenSource(DeliveryTimeout);
            while (await Confirmations.Reader.WaitToReadAsync(timeout.Token))
            {
                while (Confirmations.Reader.TryRead(out var confirmation))
                {
                    if (confirmation.SubscriberGrainKey == subscriberGrainKey)
                    {
                        return;
                    }

                    deferredConfirmations[confirmation.SubscriberGrainKey] = 0;
                }
            }

            throw new TimeoutException($"No confirmation for subscriber '{subscriberGrainKey}' was observed.");
        }

        public async Task<object> ReadWatchActivationAsync()
        {
            using var timeout = new CancellationTokenSource(DeliveryTimeout);
            return await WatchActivations.Reader.ReadAsync(timeout.Token);
        }

        public async Task<object> ReadDifferentWatchActivationAsync(object previous)
        {
            using var timeout = new CancellationTokenSource(DeliveryTimeout);
            while (await WatchActivations.Reader.WaitToReadAsync(timeout.Token))
            {
                while (WatchActivations.Reader.TryRead(out var activation))
                {
                    if (!ReferenceEquals(previous, activation))
                    {
                        return activation;
                    }
                }
            }

            throw new TimeoutException("A renewed watch was not registered after subscriptions-grain reactivation.");
        }
    }

    private sealed class DeliveryLogger : ILogger<GrainBehavior>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) { }
    }
}
