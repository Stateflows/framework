using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;

namespace Orleans.IntegrationTests;

public sealed partial class BehaviorNotificationTests
{
    private sealed class OutboxFaultFilter(FaultingNotificationsStorage storage) : IIncomingGrainCallFilter
    {
        public bool Reject;
        public bool LoseAcknowledgment;
        public bool? FailCleanup;
        public string NotificationName = Event<RetainedCustomNotification>.Name;
        public ConcurrentQueue<Guid> Attempts { get; } = new();

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            if (context.Grain is ISubscriptionsGrain &&
                context.InterfaceMethod.Name == nameof(ISubscriptionsGrain.PublishOutboxAsync) &&
                ((OrleansEventHolder[])context.Request.GetArgument(2)!).Any(n => n.Name == NotificationName))
            {
                var notification = ((OrleansEventHolder[])context.Request.GetArgument(2)!)
                    .Single(n => n.Name == NotificationName);
                Attempts.Enqueue(notification.Id);
                if (Reject)
                    throw new InvalidOperationException("Injected outbox dispatch failure.");
                await context.Invoke();
                if (FailCleanup is { } commitBeforeFailure)
                {
                    FailCleanup = null;
                    storage.FailNextContextWrite(commitBeforeFailure);
                }
                if (LoseAcknowledgment)
                {
                    LoseAcknowledgment = false;
                    throw new InvalidOperationException("Injected lost outbox acceptance acknowledgment.");
                }
                return;
            }
            await context.Invoke();
        }
    }

    [TestMethod]
    public async Task CommittedOutboxSurvivesPublisherResetAndFullHostRestart()
    {
        var storage = new FaultingNotificationsStorage();
        var calls = new OutboxFaultFilter(storage) { Reject = true };
        var values = Channel.CreateUnbounded<int>();
        var id = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "outbox-restart");
        using (var first = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), calls, values.Writer, storage))
        {
            await first.StartAsync();
            try
            {
                var grain = first.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
                Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(
                    CreateNotification(new PublishRetainedNotification { Value = 23 }, id), default)).Status);
                Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(
                    CreateNotification(new Reset(), id), default)).Status);
                Assert.IsTrue(storage.ContextCommits.Last().Tombstone);
                Assert.IsTrue(storage.ContextCommits.Last().Outbox.Count > 0);
            }
            finally { await first.StopAsync(); }
        }

        calls.Reject = false;
        using var second = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), calls, values.Writer, storage);
        await second.StartAsync();
        try
        {
            var grain = second.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
            await grain.ReceiveReminder(StateflowsBehavior.OutboxReminderName, default);
            var history = await GetSubscriptions(second.Services, id)
                .GetNotificationsAsync(DateTime.MinValue, [Event<RetainedCustomNotification>.Name]);
            Assert.AreEqual(1, history.Length);
            Assert.AreEqual(23, ((EventHolder<RetainedCustomNotification>)(EventHolder)history[0]).Payload.Value);
            Assert.AreEqual(1, calls.Attempts.Distinct().Count(), "Recovery must reuse the original notification ID.");
            Assert.AreEqual(0, storage.ContextCommits.Last().Outbox.Count);
            Assert.IsTrue(storage.ContextCommits.Last().Tombstone);
        }
        finally { await second.StopAsync(); }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task AmbiguousPublisherCommitNeverDispatchesUntilAuthoritativeReload(bool committedBeforeFailure)
    {
        var storage = new FaultingNotificationsStorage();
        var calls = new OutboxFaultFilter(storage);
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), calls, Channel.CreateUnbounded<int>().Writer, storage);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "outbox-commit");
            var grain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
            storage.FailNextContextWrite(committedBeforeFailure);
            var input = CreateNotification(new PublishRetainedNotification { Value = 8 }, id);
            Assert.AreEqual(EventStatus.Failed, (await grain.ProcessEventAsync(input, default)).Status);
            Assert.IsTrue(calls.Attempts.IsEmpty, "An unconfirmed local commit must never be dispatched.");

            await host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0)
                .ForceActivationCollection(TimeSpan.Zero);
            await grain.ReceiveReminder(StateflowsBehavior.OutboxReminderName, default);
            var history = await GetSubscriptions(host.Services, id)
                .GetNotificationsAsync(DateTime.MinValue, [Event<RetainedCustomNotification>.Name]);
            Assert.AreEqual(committedBeforeFailure ? 1 : 0, history.Length);
            Assert.AreEqual(committedBeforeFailure ? EventStatus.Duplicated : EventStatus.Consumed,
                (await grain.ProcessEventAsync(input, default)).Status);
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    [DataRow("acceptance")]
    [DataRow("cleanup-before")]
    [DataRow("cleanup-after")]
    public async Task ReplayAfterLostAcceptanceOrCleanupDoesNotDuplicateHistory(string failure)
    {
        var storage = new FaultingNotificationsStorage();
        var calls = new OutboxFaultFilter(storage)
        {
            LoseAcknowledgment = failure == "acceptance",
            FailCleanup = failure == "acceptance" ? null : failure == "cleanup-after"
        };
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), calls, Channel.CreateUnbounded<int>().Writer, storage);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "outbox-replay");
            var grain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
            Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(
                CreateNotification(new PublishRetainedNotification { Value = 11 }, id), default)).Status);
            await host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0)
                .ForceActivationCollection(TimeSpan.Zero);
            await grain.ReceiveReminder(StateflowsBehavior.OutboxReminderName, default);
            var history = await GetSubscriptions(host.Services, id)
                .GetNotificationsAsync(DateTime.MinValue, [Event<RetainedCustomNotification>.Name]);
            Assert.AreEqual(1, history.Length);
            Assert.AreEqual(1, calls.Attempts.Distinct().Count());
            Assert.AreEqual(failure == "cleanup-after" ? 1 : 2, calls.Attempts.Count);
            Assert.AreEqual(0, storage.ContextCommits.Last().Outbox.Count);
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    public async Task StatelessActionOutboxRetriesWithoutPersistingDomainStateOrReusingSequences()
    {
        var storage = new FaultingNotificationsStorage();
        var calls = new OutboxFaultFilter(storage) { Reject = true, NotificationName = Event<CustomNotification>.Name };
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), calls, Channel.CreateUnbounded<int>().Writer, storage);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.Action, "publishing-action", "outbox-stateless");
            var grain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
            var input = CreateNotification(new PublishCustomNotification(), id);
            Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(input, default)).Status);
            var committed = storage.ContextCommits.Last();
            Assert.IsTrue(committed.Tombstone);
            var context = storage.GetContext(id);
            Assert.AreEqual(BehaviorStatus.Unknown, context.Status);
            Assert.AreEqual(0, context.Values.Count);
            Assert.AreEqual(0, context.EventReceipts.Count);
            Assert.IsTrue(committed.Outbox.Count > 0);
            var sequence = committed.LastPublicationSequence;
            calls.Reject = false;
            await grain.ReceiveReminder(StateflowsBehavior.OutboxReminderName, default);
            Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(input, default)).Status,
                "Stateless actions must still execute a repeated input ID.");
            Assert.IsTrue(storage.ContextCommits.Last().LastPublicationSequence > sequence);
            var history = await GetSubscriptions(host.Services, id)
                .GetNotificationsAsync(DateTime.MinValue, [Event<CustomNotification>.Name]);
            Assert.AreEqual(2, history.Length);
            Assert.AreEqual(2, history.Select(n => n.Id).Distinct().Count());
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task OutboxAcceptanceAtomicallyDeduplicatesHistoryAndPendingAfterStorageFailure(bool commitBeforeFailure)
    {
        var storage = new FaultingNotificationsStorage();
        var calls = new NotificationDeliveryCallFilter();
        calls.ReturnStatuses[Event<CustomNotification>.Name] = EventStatus.Failed;
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), calls, Channel.CreateUnbounded<int>().Writer, storage);
        await host.StartAsync();
        try
        {
            var publisherId = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "outbox-pending");
            var subscriberId = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "outbox-subscriber");
            var subscriptions = GetSubscriptions(host.Services, publisherId);
            var name = Event<CustomNotification>.Name;
            await subscriptions.AddSubscriptionAsync(subscriberId, [name]);
            var notification = CreateNotification(new CustomNotification { Text = "one-batch" }, publisherId,
                retained: true, timeToLive: 300, sentAt: DateTime.UtcNow.AddMinutes(-1));
            var header = new TimeToLive(300);
            notification.Headers["ttl"] = StateflowsJsonConverter.SerializePolymorphicObject(header);
            var source = GetBehaviorGrainKey(publisherId);
            storage.FailNextNotificationsWrite(commitBeforeFailure);
            var failure = await Assert.ThrowsExactlyAsync<OrleansException>(
                () => subscriptions.PublishOutboxAsync(source, 1, [notification]));
            Assert.IsInstanceOfType<InvalidOperationException>(failure.InnerException);
            await host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0)
                .ForceActivationCollection(TimeSpan.Zero);
            await subscriptions.PublishOutboxAsync(source, 1, [notification]);
            await subscriptions.PublishOutboxAsync(source, 1, [notification]);
            var history = await subscriptions.GetNotificationsAsync(DateTime.MinValue, [name]);
            var pending = await subscriptions.GetPendingDeliveriesAsync(GetBehaviorGrainKey(subscriberId), 10);
            Assert.AreEqual(1, history.Length);
            Assert.AreEqual(1, pending.Length, "Acceptance replay must not append pending deliveries.");
            var restored = pending[0].Notification;
            Assert.AreEqual(notification.Id, restored.Id);
            Assert.AreEqual(notification.Payload, restored.Payload);
            Assert.AreEqual(notification.SentAt, restored.SentAt);
            Assert.AreEqual(notification.TimeToLive, restored.TimeToLive);
            Assert.AreEqual(notification.Retained, restored.Retained);
            Assert.AreEqual(notification.SenderId, restored.SenderId);
            Assert.AreEqual(notification.Headers["ttl"], restored.Headers["ttl"]);
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    public async Task AcceptanceWatermarkSurvivesHistoryExpirationAndReactivation()
    {
        using var host = CreateHost(GetFreeTcpPorts(2));
        await host.StartAsync();
        try
        {
            var id = CreatePublisherId("outbox-watermark");
            var subscriptions = GetSubscriptions(host.Services, id);
            var expired = CreateNotification(new CustomNotification(), id, timeToLive: 1, sentAt: DateTime.UtcNow.AddMinutes(-1));
            var source = GetBehaviorGrainKey(id);
            await subscriptions.PublishOutboxAsync(source, 1, [expired]);
            await host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0)
                .ForceActivationCollection(TimeSpan.Zero);
            // Even a changed envelope cannot turn an old batch sequence into a new publication.
            var replay = CreateNotification(new CustomNotification { Text = "must-not-appear" }, id);
            await subscriptions.PublishOutboxAsync(source, 1, [replay]);
            Assert.AreEqual(0, (await subscriptions.GetNotificationsAsync(DateTime.MinValue, [expired.Name])).Length);
            await subscriptions.PublishOutboxAsync(source, 2, [replay]);
            Assert.AreEqual(1, (await subscriptions.GetNotificationsAsync(DateTime.MinValue, [expired.Name])).Length);
        }
        finally { await host.StopAsync(); }
    }
}
