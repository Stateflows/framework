using System.Threading.Channels;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Stateflows;
using Stateflows.Actions;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;

namespace Orleans.IntegrationTests;

public sealed partial class BehaviorNotificationTests
{
    public sealed class RecordValue
    {
        public int Value { get; set; }
    }

    [TestMethod]
    public void EventIdHeaderIsAppliedWhenConstructingHoldersButDoesNotReplaceExistingEnvelopeId()
    {
        var id = Guid.NewGuid();
        var headers = new Dictionary<string, EventHeader> { ["external-message-id"] = new EventId(id) };
        var payload = new RecordValue { Value = 7 };
        var holder = payload.ToEventHolder(headers);
        Assert.AreEqual(id, holder.Id);
        Assert.AreEqual(id, payload.ToTypedEventHolder(headers).Id);
        object boxed = payload;
        Assert.AreEqual(id, boxed.ToTypedEventHolder(headers).Id);
        Assert.AreEqual(typeof(RecordValue), boxed.ToTypedEventHolder(headers).PayloadType);
        Assert.AreNotEqual(payload.ToEventHolder().Id, payload.ToEventHolder().Id);
        Assert.AreNotEqual(payload.ToTypedEventHolder().Id, payload.ToTypedEventHolder().Id);

        var envelopeId = Guid.NewGuid();
        holder.Id = envelopeId;
        Assert.AreEqual(envelopeId, holder.ToTypedEventHolder().Id);
        Assert.AreEqual(envelopeId, holder.ToTypedEventHolder(headers).Id);
        Assert.AreEqual(envelopeId, holder.Id);

        var serializedHeaders = StateflowsJsonConverter.SerializePolymorphicObject(headers);
        var restoredHeaders = StateflowsJsonConverter.DeserializeObject<Dictionary<string, EventHeader>>(serializedHeaders);
        Assert.AreEqual(id, payload.ToEventHolder(restoredHeaders).Id);
        OrleansEventHolder wireHolder = holder;
        Assert.AreEqual(envelopeId, ((EventHolder)wireHolder).Id,
            "Deserializing an envelope must preserve its ID even if its header contains another ID.");
    }

    [TestMethod]
    public async Task PublicSendAndRequestApisUseStableEventIdHeadersAcrossReactivation()
    {
        var values = Channel.CreateUnbounded<int>();
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), new NotificationDeliveryCallFilter(), values.Writer, storage);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "header-id");
            using var scope = host.Services.CreateScope();
            Assert.IsTrue(scope.ServiceProvider.GetRequiredService<IBehaviorLocator>().TryLocateBehavior(id, out var behavior));
            using (behavior)
            {
                var eventId = Guid.NewGuid();
                var headers = new Dictionary<string, EventHeader> { [nameof(EventId)] = new EventId(eventId) };
                Assert.AreEqual(EventStatus.Consumed,
                    (await behavior.SendAsync(new RecordValue { Value = 45 }, headers)).Status);
                Assert.IsTrue(storage.GetContext(id).EventReceipts.ContainsKey(eventId));
                Assert.AreEqual(EventStatus.Duplicated,
                    (await behavior.SendAsync(new RecordValue { Value = 45 }, headers)).Status);

                var requestId = Guid.NewGuid();
                var requestHeaders = new Dictionary<string, EventHeader> { [nameof(EventId)] = new EventId { Id = requestId } };
                // Status requests are marked [NoDeduplication]: a redelivery is answered again and no receipt is recorded.
                var firstRequest = await behavior.RequestAsync(new BehaviorInfoRequest(), requestHeaders);
                Assert.IsNotNull(firstRequest.Response);
                Assert.IsFalse(storage.GetContext(id).EventReceipts.ContainsKey(requestId));
                var duplicateRequest = await behavior.RequestAsync(new BehaviorInfoRequest(), requestHeaders);
                Assert.AreNotEqual(EventStatus.Duplicated, duplicateRequest.Status);
                Assert.IsNotNull(duplicateRequest.Response);

                var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
                await management.ForceActivationCollection(TimeSpan.Zero);
                Assert.AreEqual(EventStatus.Duplicated,
                    (await behavior.SendAsync(new RecordValue { Value = 45 }, headers)).Status);
                duplicateRequest = await behavior.RequestAsync(new BehaviorInfoRequest(), requestHeaders);
                Assert.AreNotEqual(EventStatus.Duplicated, duplicateRequest.Status);
                Assert.IsNotNull(duplicateRequest.Response);

                headers[nameof(EventId)] = new EventId(Guid.NewGuid());
                Assert.AreEqual(EventStatus.Consumed,
                    (await behavior.SendAsync(new RecordValue { Value = 46 }, headers)).Status);
                Assert.AreEqual(EventStatus.Consumed,
                    (await behavior.SendAsync(new RecordValue { Value = 47 })).Status);
                Assert.AreEqual(EventStatus.Consumed,
                    (await behavior.SendAsync(new RecordValue { Value = 47 })).Status);

                var noDeduplicationId = Guid.NewGuid();
                var noDeduplicationHeaders = new Dictionary<string, EventHeader>
                {
                    [nameof(EventId)] = new EventId(noDeduplicationId),
                    [nameof(NoDeduplication)] = new NoDeduplication()
                };
                Assert.AreEqual(EventStatus.Consumed,
                    (await behavior.SendAsync(new RecordValue { Value = 48 }, noDeduplicationHeaders)).Status);
                Assert.AreEqual(EventStatus.Consumed,
                    (await behavior.SendAsync(new RecordValue { Value = 48 }, noDeduplicationHeaders)).Status,
                    "The NoDeduplication header must disable deduplication of a single event.");
                Assert.IsFalse(storage.GetContext(id).EventReceipts.ContainsKey(noDeduplicationId));
                CollectionAssert.AreEqual(new[] { 45, 46, 47, 47, 48, 48 }, DrainValues(values.Reader));
            }
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    public async Task PublicSendApisPreserveIncomingEnvelopeIdentityAndMetadata()
    {
        var values = Channel.CreateUnbounded<int>();
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), new NotificationDeliveryCallFilter(), values.Writer, storage);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "external-channel");
            using var scope = host.Services.CreateScope();
            var locator = scope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
            Assert.IsTrue(locator.TryLocateBehavior(id, out var behavior));
            using (behavior)
            {
                var sentAt = DateTime.UtcNow;
                var envelope = new EventHolder<RecordValue>
                {
                    Id = Guid.NewGuid(),
                    Payload = new RecordValue { Value = 31 },
                    SentAt = sentAt,
                    TimeToLive = 300,
                    Headers = new Dictionary<string, EventHeader> { ["initial"] = new NoImplicitInitialization() }
                };
                // Initialize separately because the header above intentionally disallows implicit initialization.
                Assert.AreEqual(EventStatus.Initialized, (await behavior.SendAsync(new Initialize())).Status);
                Assert.AreEqual(EventStatus.Consumed, (await behavior.SendAsync(envelope)).Status);
                Assert.AreEqual(sentAt.AddSeconds(300), storage.GetContext(id).EventReceipts[envelope.Id]);
                Assert.AreEqual(EventStatus.Duplicated, (await envelope.SendAsync(behavior)).Status);
                Assert.AreEqual(EventStatus.Duplicated,
                    (await behavior.SendAsync(envelope, new Dictionary<string, EventHeader> { ["extra"] = new NoImplicitInitialization() })).Status);
                Assert.AreEqual(1, envelope.Headers.Count, "Extra delivery headers must not mutate the source envelope.");

                var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
                await management.ForceActivationCollection(TimeSpan.Zero);
                Assert.AreEqual(EventStatus.Duplicated, (await envelope.SendAsync(behavior)).Status);
                Assert.AreEqual(EventStatus.Consumed, (await behavior.SendAsync(new RecordValue { Value = 32 })).Status);
                Assert.AreEqual(EventStatus.Consumed, (await behavior.SendAsync(new RecordValue { Value = 32 })).Status);
                CollectionAssert.AreEqual(new[] { 31, 32, 32 }, DrainValues(values.Reader));

                var expired = new EventHolder<RecordValue>
                {
                    Payload = new RecordValue { Value = 33 },
                    SentAt = DateTime.UtcNow.AddMinutes(-1),
                    TimeToLive = 1
                };
                Assert.AreEqual(EventStatus.Invalid, (await expired.SendAsync(behavior)).Status);
                Assert.IsFalse(values.Reader.TryRead(out _), "Envelope TTL must survive the public send path.");
            }
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    public async Task NoDeduplicationRequestsAreReexecutedWithoutReceiptsOrStateWrites()
    {
        var values = Channel.CreateUnbounded<int>();
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), new NotificationDeliveryCallFilter(), values.Writer, storage);
        await host.StartAsync();
        try
        {
            var factory = host.Services.GetRequiredService<IGrainFactory>();
            var id = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "requests");
            var grain = factory.GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
            Assert.AreEqual(EventStatus.Initialized, (await grain.ProcessEventAsync(new Initialize().ToEventHolder(), default)).Status);
            var writes = storage.ContextWrites;

            OrleansEventHolder request = new BehaviorInfoRequest().ToEventHolder();
            var result = await grain.ProcessEventAsync(request, default);
            Assert.IsNotNull(result.Response);
            Assert.AreNotEqual(EventStatus.Failed, result.Status);
            result = await grain.ProcessEventAsync(request, default);
            Assert.AreNotEqual(EventStatus.Duplicated, result.Status, "A retried status request must be answered again.");
            Assert.IsNotNull(result.Response);
            Assert.IsTrue(result.Validation.IsValid);
            Assert.AreEqual(writes, storage.ContextWrites, "Status requests must not write behavior state.");
            Assert.IsFalse(storage.GetContext(id).EventReceipts.ContainsKey(request.Id));

            // A deduplicated event that changes nothing is still persisted, because its receipt is new state.
            OrleansEventHolder ignored = new MoveNextEvent().ToEventHolder();
            Assert.AreEqual(EventStatus.NotConsumed, (await grain.ProcessEventAsync(ignored, default)).Status);
            Assert.AreEqual(writes + 1, storage.ContextWrites);
            Assert.IsTrue(storage.GetContext(id).EventReceipts.ContainsKey(ignored.Id));
            Assert.AreEqual(EventStatus.Duplicated, (await grain.ProcessEventAsync(ignored, default)).Status);

            // A status request that may not initialize the behavior leaves no record behind.
            var uninitializedId = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "requests-uninitialized");
            OrleansEventHolder probe = new BehaviorInfoRequest().ToEventHolder(
                new Dictionary<string, EventHeader> { [nameof(NoImplicitInitialization)] = new NoImplicitInitialization() });
            writes = storage.ContextWrites;
            result = await factory.GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(uninitializedId)).ProcessEventAsync(probe, default);
            Assert.AreNotEqual(EventStatus.Failed, result.Status);
            Assert.AreEqual(writes, storage.ContextWrites);
            Assert.IsFalse(storage.HasContext(uninitializedId));
            var instances = await factory.GetGrain<IInstancesGrain>(StateflowsInstances.GetKey(null, uninitializedId.BehaviorClass))
                .GetInstancesAsync();
            Assert.IsFalse(instances.Contains(uninitializedId.Instance), "An unpersisted behavior must not be listed.");

            // A status request that implicitly initializes the behavior changes its state, so it is persisted.
            var implicitId = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "requests-implicit");
            result = await factory.GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(implicitId))
                .ProcessEventAsync(new BehaviorInfoRequest().ToEventHolder(), default);
            Assert.AreNotEqual(EventStatus.Failed, result.Status);
            Assert.IsTrue(storage.HasContext(implicitId));
            Assert.AreEqual(BehaviorStatus.Initialized, storage.GetContext(implicitId).Status);

            using var scope = host.Services.CreateScope();
            Assert.IsTrue(scope.ServiceProvider.GetRequiredService<IBehaviorLocator>().TryLocateBehavior(id, out var behavior));
            using (behavior)
            {
                Assert.IsNotNull((await behavior.RequestAsync(new BehaviorInfoRequest())).Response);
                Assert.IsNotNull((await behavior.RequestAsync(new BehaviorInfoRequest())).Response);
            }
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    public async Task IncomingEventReceiptSurvivesReactivationAndSuppressesDuplicates()
    {
        var values = Channel.CreateUnbounded<int>();
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), new NotificationDeliveryCallFilter(), values.Writer, storage);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "dedup");
            var grain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
            var notification = CreateNotification(new RetainedCustomNotification { Value = 42 }, CreatePublisherId("source"));
            Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(notification, default)).Status);
            var firstCommit = storage.ContextCommits.First();
            Assert.IsTrue(StateflowsJsonConverter.DeserializeObject<Stateflows.Common.Context.StateflowsContext>(firstCommit.Payload)!
                .EventReceipts.ContainsKey(notification.Id), "The first commit must include the incoming receipt.");
            Assert.IsTrue(firstCommit.Outbox.Count > 0, "Notifications must be included in that same commit.");
            Assert.AreEqual(1 + firstCommit.Outbox.Count, storage.ContextWrites,
                "Only outbox acknowledgment writes may follow the atomic state/receipt/outbox commit.");
            var writes = storage.ContextWrites;
            Assert.AreEqual(42, await values.Reader.ReadAsync().AsTask().WaitAsync(DeliveryTimeout));
            Assert.AreEqual(EventStatus.Duplicated, (await grain.ProcessEventAsync(notification, default)).Status);
            Assert.AreEqual(writes, storage.ContextWrites);
            Assert.IsFalse(values.Reader.TryRead(out _));

            var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
            await management.ForceActivationCollection(TimeSpan.Zero);
            Assert.AreEqual(EventStatus.Duplicated, (await grain.ProcessEventAsync(notification, default)).Status);
            notification.Id = Guid.NewGuid();
            Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(notification, default)).Status);
            Assert.AreEqual(42, await values.Reader.ReadAsync().AsTask().WaitAsync(DeliveryTimeout));
            Assert.IsFalse(values.Reader.TryRead(out _));
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task NotificationReceiptIsAtomicWithContextWrite(bool committedBeforeFailure)
    {
        var values = Channel.CreateUnbounded<int>();
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), new NotificationDeliveryCallFilter(), values.Writer, storage);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "atomic");
            var grain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
            var notification = CreateNotification(new RetainedCustomNotification { Value = 7 }, CreatePublisherId("source"));
            storage.FailNextContextWrite(committedBeforeFailure);
            Assert.AreEqual(EventStatus.Failed, (await grain.ProcessEventAsync(notification, default)).Status);
            Assert.AreEqual(7, await values.Reader.ReadAsync().AsTask().WaitAsync(DeliveryTimeout));
            Assert.AreEqual(committedBeforeFailure ? EventStatus.Duplicated : EventStatus.Consumed,
                (await grain.ProcessEventAsync(notification, default)).Status);
            // Both successful retries and recovered committed writes retain the receipt.
            Assert.IsTrue(storage.GetContext(id).EventReceipts.ContainsKey(notification.Id));
            Assert.AreEqual(!committedBeforeFailure, values.Reader.TryRead(out _));
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    public async Task IncomingEventReceiptsUseAbsoluteTtlAndOneHourForZeroTtl()
    {
        var values = Channel.CreateUnbounded<int>();
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), new NotificationDeliveryCallFilter(), values.Writer, storage);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "expiry");
            var grain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
            var zero = CreateNotification(new RetainedCustomNotification { Value = 1 }, CreatePublisherId("source"));
            var before = DateTime.UtcNow;
            Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(zero, default)).Status);
            var expiry = storage.GetContext(id).EventReceipts[zero.Id];
            Assert.IsTrue(expiry >= before.AddHours(1) && expiry <= DateTime.UtcNow.AddHours(1));
            Assert.AreEqual(EventStatus.Duplicated, (await grain.ProcessEventAsync(zero, default)).Status);
            Assert.AreEqual(expiry, storage.GetContext(id).EventReceipts[zero.Id],
                "A duplicate must not extend the original receipt's window.");

            var positive = CreateNotification(new RetainedCustomNotification { Value = 2 }, CreatePublisherId("source"),
                timeToLive: 120, sentAt: DateTime.UtcNow.AddSeconds(-60));
            Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(positive, default)).Status);
            Assert.AreEqual(positive.SentAt.ToUniversalTime().AddSeconds(120),
                storage.GetContext(id).EventReceipts[positive.Id]);

            storage.UpdateContext(id, context => context.EventReceipts[zero.Id] = DateTime.UtcNow.AddSeconds(-1));
            var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
            await management.ForceActivationCollection(TimeSpan.Zero);
            Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(zero, default)).Status);

            positive.SentAt = DateTime.UtcNow.AddMinutes(-5);
            Assert.AreEqual(EventStatus.Invalid, (await grain.ProcessEventAsync(positive, default)).Status);
            var expiredUnseen = CreateNotification(new RetainedCustomNotification { Value = 3 }, CreatePublisherId("source"),
                timeToLive: 1, sentAt: DateTime.UtcNow.AddMinutes(-1));
            var expiredResult = await grain.ProcessEventAsync(expiredUnseen, default);
            Assert.AreEqual(EventStatus.Invalid, expiredResult.Status);
            Assert.IsFalse(expiredResult.Validation.IsValid);
            CollectionAssert.AreEqual(new[] { 1, 2, 1 }, DrainValues(values.Reader));

            storage.UpdateContext(id, context => context.EventReceipts[positive.Id] = DateTime.UtcNow.AddSeconds(-1));
            await management.ForceActivationCollection(TimeSpan.Zero);
            await grain.ProcessEventAsync(new RecordValue { Value = 4 }.ToEventHolder(), default);
            Assert.IsFalse(storage.GetContext(id).EventReceipts.ContainsKey(positive.Id),
                "A subsequent state write must prune expired receipts.");
            Assert.AreEqual(EventStatus.Invalid, (await grain.ProcessEventAsync(positive, default)).Status);
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ResetAndForcedResetPreserveIncomingEventReceipts(bool forced)
    {
        var values = Channel.CreateUnbounded<int>();
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), new NotificationDeliveryCallFilter(), values.Writer, storage);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "reset");
            var grain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
            var notification = CreateNotification(new RetainedCustomNotification { Value = 4 }, CreatePublisherId("source"));
            Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(notification, default)).Status);
            EventHolder reset = forced
                ? new RetainedCustomNotification { Value = 5 }.ToEventHolder()
                : new Reset().ToEventHolder();
            if (forced)
                reset.Headers["reset"] = (EventHeader)Activator.CreateInstance(
                    typeof(IEventProcessor).Assembly.GetType("Stateflows.Common.ForcedReset")!, nonPublic: true)!;
            Assert.AreNotEqual(EventStatus.Failed, (await grain.ProcessEventAsync(reset, default)).Status);
            var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
            await management.ForceActivationCollection(TimeSpan.Zero);
            Assert.AreEqual(EventStatus.Duplicated, (await grain.ProcessEventAsync(notification, default)).Status);
            if (!forced)
            {
                Assert.AreEqual(BehaviorStatus.Unknown, storage.GetContext(id).Status);
                Assert.AreEqual(0, storage.GetContext(id).Values.Count);
            }
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    public async Task StatelessActionsDoNotDeduplicateNotifications()
    {
        var values = Channel.CreateUnbounded<int>();
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), new NotificationDeliveryCallFilter(), values.Writer, storage);
        await host.StartAsync();
        try
        {
            BehaviorId id = new ActionId("publishing-action", "stateless");
            var grain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
            OrleansEventHolder notification = new TokensInput().ToEventHolder();
            Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(notification, default)).Status);
            Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(notification, default)).Status);
            Assert.AreEqual(2, (await GetSubscriptions(host.Services, id)
                .GetNotificationsAsync(DateTime.MinValue, [Event<CustomNotification>.Name])).Length);
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    public async Task StatefulActionReceiptsAreScopedToEachBehavior()
    {
        var values = Channel.CreateUnbounded<int>();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), new NotificationDeliveryCallFilter(), values.Writer);
        await host.StartAsync();
        try
        {
            var factory = host.Services.GetRequiredService<IGrainFactory>();
            OrleansEventHolder notification = new TokensInput().ToEventHolder();
            foreach (var instance in new[] { "one", "two" })
            {
                BehaviorId id = new ActionId("stateful-action", instance);
                var grain = factory.GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
                Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(notification, default)).Status);
                Assert.AreEqual(EventStatus.Duplicated, (await grain.ProcessEventAsync(notification, default)).Status);
            }
            CollectionAssert.AreEqual(new[] { 99, 99 }, DrainValues(values.Reader));
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    public async Task NotificationResetCommitsItsOwnReceiptWithLogicalDeletion()
    {
        var values = Channel.CreateUnbounded<int>();
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), new NotificationDeliveryCallFilter(), values.Writer, storage);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "delete");
            var grain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
            await grain.ProcessEventAsync(new Initialize().ToEventHolder(), default);
            OrleansEventHolder notification = new Reset().ToEventHolder();
            Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(notification, default)).Status);
            var context = storage.GetContext(id);
            Assert.IsTrue(context.EventReceipts.ContainsKey(notification.Id));
            Assert.AreEqual(BehaviorStatus.Unknown, context.Status);
            Assert.AreEqual(0, context.Values.Count);
            Assert.AreEqual(0, context.PendingTimeEvents.Count);
            var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
            await management.ForceActivationCollection(TimeSpan.Zero);
            Assert.AreEqual(EventStatus.Duplicated, (await grain.ProcessEventAsync(notification, default)).Status);
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    public async Task FailedNotificationExecutionCanBeRetriedWithTheSameId()
    {
        var values = Channel.CreateUnbounded<int>();
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), new NotificationDeliveryCallFilter(), values.Writer, storage);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "failure");
            var grain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
            var payload = new RetainedCustomNotification { Value = -1 };
            var notification = CreateNotification(payload, CreatePublisherId("source"));
            Assert.AreEqual(EventStatus.Failed, (await grain.ProcessEventAsync(notification, default)).Status);
            var retry = CreateNotification(new RetainedCustomNotification { Value = 8 }, CreatePublisherId("source"));
            retry.Id = notification.Id;
            Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(retry, default)).Status);
            Assert.AreEqual(EventStatus.Duplicated, (await grain.ProcessEventAsync(retry, default)).Status);
            CollectionAssert.AreEqual(new[] { 8 }, DrainValues(values.Reader));
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    public async Task InvalidNotificationValidationPersistsReceiptWithoutExecuting()
    {
        var values = Channel.CreateUnbounded<int>();
        var storage = new FaultingNotificationsStorage();
        var validator = new RejectNotificationValidator();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), new NotificationDeliveryCallFilter(),
            values.Writer, storage, validator);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "self-subscribing-machine", "invalid");
            var grain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
            var notification = CreateNotification(new RetainedCustomNotification { Value = 10 }, CreatePublisherId("source"));
            storage.FailNextContextWrite(false);
            Assert.AreEqual(EventStatus.Failed, (await grain.ProcessEventAsync(notification, default)).Status);
            var result = await grain.ProcessEventAsync(notification, default);
            Assert.AreEqual(EventStatus.Invalid, result.Status);
            Assert.IsFalse(result.Validation.IsValid);
            Assert.IsTrue(storage.GetContext(id).EventReceipts.ContainsKey(notification.Id));
            var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
            await management.ForceActivationCollection(TimeSpan.Zero);
            result = await grain.ProcessEventAsync(notification, default);
            Assert.AreEqual(EventStatus.Duplicated, result.Status);
            Assert.IsTrue(result.Validation.IsValid);
            Assert.AreEqual(2, validator.Calls);
            Assert.IsFalse(values.Reader.TryRead(out _));
            Assert.AreEqual(BehaviorStatus.Unknown, storage.GetContext(id).Status);
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    public async Task DeferredNotificationReceiptDoesNotSuppressInternalDeferredProcessing()
    {
        var values = Channel.CreateUnbounded<int>();
        using var host = CreateDeliveryScenarioHost(GetFreeTcpPorts(2), new NotificationDeliveryCallFilter(), values.Writer);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "deferred-machine", "deferred");
            var grain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(GetBehaviorGrainKey(id));
            var notification = CreateNotification(new RetainedCustomNotification { Value = 25 }, CreatePublisherId("source"));
            Assert.AreEqual(EventStatus.Deferred, (await grain.ProcessEventAsync(notification, default)).Status);
            Assert.AreEqual(EventStatus.Duplicated, (await grain.ProcessEventAsync(notification, default)).Status);
            Assert.IsFalse(values.Reader.TryRead(out _));
            Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(new MoveNextEvent().ToEventHolder(), default)).Status);
            CollectionAssert.AreEqual(new[] { 25 }, DrainValues(values.Reader));
            Assert.AreEqual(EventStatus.Duplicated, (await grain.ProcessEventAsync(notification, default)).Status);
        }
        finally { await host.StopAsync(); }
    }

    private sealed class RejectNotificationValidator : IStateflowsValidator
    {
        public int Calls;
        public Task<bool> ValidateAsync<TEvent>(TEvent @event, List<ValidationResult> validationResults)
        {
            if (@event is not RetainedCustomNotification) return Task.FromResult(true);
            Interlocked.Increment(ref Calls);
            validationResults.Add(new ValidationResult("Injected rejection."));
            return Task.FromResult(false);
        }
    }

    private static int[] DrainValues(ChannelReader<int> reader)
    {
        var values = new List<int>();
        while (reader.TryRead(out var value)) values.Add(value);
        return values.ToArray();
    }
}
