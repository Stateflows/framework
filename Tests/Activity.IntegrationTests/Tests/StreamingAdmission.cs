using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Activities.Context.Classes;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class StreamingAdmission
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void TokenCopiesAndSerializationPreservePayloadTypeIdentityAndColor(bool streamed)
    {
        TokenHolder source = new TokenHolder<Animal>
        {
            Payload = new Dog("Spot"),
            IsStreamed = !streamed
        };
        var copy = source.CopyForExecution(streamed);
        var restored = StateflowsJsonConverter.Clone(copy);

        Assert.AreEqual(source.Id, restored.Id);
        Assert.AreEqual(typeof(Animal), restored.PayloadType);
        Assert.AreEqual(typeof(Dog), restored.ActualPayloadType);
        Assert.AreEqual("Spot", ((Dog)restored.BoxedPayload).Name);
        Assert.AreEqual(streamed, restored.IsStreamed);
        Assert.AreEqual(!streamed, source.IsStreamed);
        Assert.AreNotSame(source, copy);
        Assert.AreNotSame(copy, restored);
        Assert.AreEqual(source, copy, "Provenance must not change logical token equality.");
        Assert.AreEqual(source.GetHashCode(), copy.GetHashCode());
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void NullTokenPayloadKeepsDeclaredTypeWhenCopiedAndSerialized(bool streamed)
    {
        TokenHolder source = new TokenHolder<string> { Payload = null! };
        var restored = StateflowsJsonConverter.Clone(source.CopyForExecution(streamed));

        Assert.AreEqual(source.Id, restored.Id);
        Assert.IsNull(restored.BoxedPayload);
        Assert.AreEqual(typeof(string), restored.PayloadType);
        Assert.AreEqual(typeof(string), restored.ActualPayloadType);
        Assert.AreEqual(streamed, restored.IsStreamed);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void ExceptionAndControlTokenCopiesKeepSpecializedEnvelopes(bool streamed)
    {
        var exception = new InvalidOperationException("routing failure");
        var source = new ExceptionHolder<InvalidOperationException> { TypedException = exception };
        var copy = source.CopyForExecution(streamed);
        var control = new ControlToken().ToTokenHolder();
        var controlCopy = StateflowsJsonConverter.Clone(control.CopyForExecution(streamed));

        Assert.IsInstanceOfType<ExceptionHolder<InvalidOperationException>>(copy);
        Assert.AreSame(exception, copy.BoxedPayload);
        Assert.AreEqual(source.Id, copy.Id);
        Assert.AreEqual(streamed, copy.IsStreamed);
        Assert.IsFalse(source.IsStreamed);
        Assert.IsInstanceOfType<TokenHolder<ControlToken>>(controlCopy);
        Assert.AreEqual(control.Id, controlCopy.Id);
        Assert.AreEqual(streamed, controlCopy.IsStreamed);
    }

    [TestMethod]
    public void TokenRoutingCopiesKeepIndependentProvenance()
    {
        var source = new TokenHolder<int> { Payload = 42 };
        var transactionalRoute = source.CopyForExecution(false);
        var streamedRoute = source.CopyForExecution(true);

        Assert.AreEqual(source.Id, transactionalRoute.Id);
        Assert.AreEqual(source.Id, streamedRoute.Id);
        Assert.AreNotSame(source, transactionalRoute);
        Assert.AreNotSame(transactionalRoute, streamedRoute);
        Assert.IsFalse(source.IsStreamed);
        Assert.IsFalse(transactionalRoute.IsStreamed);
        Assert.IsTrue(streamedRoute.IsStreamed);
        Assert.AreNotEqual(transactionalRoute.IsStreamed, streamedRoute.IsStreamed);
        Assert.AreEqual(42, ((TokenHolder<int>)streamedRoute).Payload);
    }

    [TestMethod]
    public void StreamRetainsStreamedProvenanceForControlAndObjectTokens()
    {
        var stream = new Stateflows.Activities.Streams.Stream { EdgeIdentifier = "flow" };
        var objectToken = new TokenHolder<int> { Payload = 42, IsStreamed = true };
        var controlToken = new TokenHolder<ControlToken>
        {
            Payload = new ControlToken(),
            IsStreamed = true
        };

        stream.Consume([objectToken], isPersistent: true);
        stream.Consume(controlToken);

        Assert.IsTrue(stream.IsActivated);
        Assert.IsTrue(stream.IsStreamed);
        Assert.IsTrue(stream.IsPersistent);
        Assert.IsTrue(stream.Tokens.Single().IsStreamed);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void StreamActivationColorIsMonotonicAcrossMixedControlArrivals(bool streamedFirst)
    {
        var stream = new Stateflows.Activities.Streams.Stream { EdgeIdentifier = "mixed-control" };
        stream.Consume(new TokenHolder<ControlToken>
        {
            Payload = new ControlToken(),
            IsStreamed = streamedFirst
        });
        Assert.AreEqual(streamedFirst, stream.IsStreamed);
        stream.Consume(new TokenHolder<ControlToken>
        {
            Payload = new ControlToken(),
            IsStreamed = !streamedFirst
        });
        var restored = StateflowsJsonConverter.Clone(stream);

        Assert.IsTrue(restored.IsActivated);
        Assert.IsTrue(restored.IsStreamed);
        Assert.AreEqual(0, restored.Tokens.Count, "Control tokens must not become object inputs.");
    }

    [TestMethod]
    public void PersistentQueueSerializationRetainsOrderMultiplicityAndOccurrenceColors()
    {
        var source = 42.ToTokenHolder();
        var stream = new Stateflows.Activities.Streams.Stream { EdgeIdentifier = "persistent" };
        stream.Consume(
            [source.CopyForExecution(false), source.CopyForExecution(true), 17.ToTokenHolder()],
            isPersistent: true);
        var restored = StateflowsJsonConverter.Clone(stream);
        var tokens = restored.Tokens.ToArray();

        Assert.IsTrue(restored.IsPersistent);
        CollectionAssert.AreEqual(new object[] { 42, 42, 17 }, tokens.Select(token => token.BoxedPayload).ToArray());
        Assert.AreEqual(source.Id, tokens[0].Id);
        Assert.AreEqual(source.Id, tokens[1].Id);
        Assert.AreNotSame(tokens[0], tokens[1]);
        Assert.IsFalse(tokens[0].IsStreamed);
        Assert.IsTrue(tokens[1].IsStreamed);
        Assert.IsFalse(tokens[2].IsStreamed);
    }

    [TestMethod]
    public void StreamingAdmissionDoesNotRecolorAnOutstandingRegularTransaction()
    {
        var root = NewRoot();
        var thread = Guid.NewGuid();
        var flow = root.GetStream("waiting", thread);
        flow.Consume([42.ToTokenHolder()], isPersistent: false);
        flow.Consume(new ControlToken().ToTokenHolder());
        root.GetOutputTokens("output", thread).Add(17.ToTokenHolder());
        root.ActivityOutputTokens.Add(23.ToTokenHolder());

        var transaction = root.BeginInvocationProvenance(false, trackTransaction: true, callRoot: true);
        root.BeginStreamingBusyPeriod();
        Assert.AreEqual(1, root.OutstandingTransactions);
        Assert.IsFalse(root.IsCurrentInvocationStreamed);
        Assert.IsFalse(flow.IsStreamed);
        Assert.IsFalse(flow.Tokens.Single().IsStreamed);

        transaction.Dispose();
        transaction.Dispose();
        Assert.AreEqual(0, root.OutstandingTransactions);
        Assert.IsTrue(flow.IsStreamed);
        Assert.IsTrue(flow.Tokens.Single().IsStreamed);
        Assert.IsTrue(root.GetOutputTokens("output", thread).Single().IsStreamed);
        Assert.IsTrue(root.ActivityOutputTokens.Single().IsStreamed);

        var hydrated = new RootContext(StateflowsJsonConverter.Clone(root.Context));
        Assert.IsTrue(hydrated.GetStream("waiting", thread).IsStreamed);
        Assert.IsTrue(hydrated.GetStream("waiting", thread).Tokens.Single().IsStreamed);
    }

    [TestMethod]
    public void StreamedInvocationDoesNotOwnOrCloseTheRegularTransaction()
    {
        var root = NewRoot();
        using var regular = root.BeginInvocationProvenance(false, trackTransaction: true, callRoot: true);
        root.BeginStreamingBusyPeriod();
        using (root.BeginInvocationProvenance(true, trackTransaction: true, callRoot: true))
        {
            Assert.AreEqual(1, root.OutstandingTransactions);
            Assert.IsTrue(root.IsCurrentInvocationStreamed);
            Assert.AreEqual(true, root.IsCurrentCallStreamed);
        }
        Assert.AreEqual(1, root.OutstandingTransactions);
        Assert.IsFalse(root.IsCurrentInvocationStreamed);
        Assert.AreEqual(false, root.IsCurrentCallStreamed);
    }

    [TestMethod]
    public async Task ConcurrentInvocationProvenanceIsIsolatedAcrossAsyncFlows()
    {
        var root = NewRoot();
        root.BeginStreamingBusyPeriod();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var regular = root.BeginInvocationProvenance(false, trackTransaction: true, callRoot: true);
        var streaming = Task.Run(async () =>
        {
            using var invocation = root.BeginInvocationProvenance(true, callRoot: true);
            entered.SetResult();
            await release.Task;
            Assert.IsTrue(root.IsCurrentInvocationStreamed);
            Assert.AreEqual(true, root.IsCurrentCallStreamed);
        });
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.IsFalse(root.IsCurrentInvocationStreamed);
            Assert.AreEqual(false, root.IsCurrentCallStreamed);
            Assert.AreEqual(1, root.OutstandingTransactions);
        }
        finally
        {
            release.TrySetResult();
            await streaming.WaitAsync(TimeSpan.FromSeconds(5));
        }
        Assert.IsFalse(root.IsCurrentInvocationStreamed);
    }

    [TestMethod]
    public void EndingStreamingBusyPeriodRestoresTransactionalMarking()
    {
        var root = NewRoot();
        var flow = root.GetStream("idle", Guid.NewGuid());
        flow.Consume([42.ToTokenHolder()], isPersistent: true);
        flow.Consume(new ControlToken().ToTokenHolder());

        root.BeginStreamingBusyPeriod();
        Assert.IsTrue(root.IsStreamingBusyPeriod);
        Assert.IsTrue(flow.IsStreamed);
        Assert.IsTrue(flow.Tokens.Single().IsStreamed);
        root.EndStreamingBusyPeriod();
        Assert.IsFalse(root.IsStreamingBusyPeriod);
        Assert.IsFalse(flow.IsStreamed);
        Assert.IsFalse(flow.Tokens.Single().IsStreamed);
        Assert.IsTrue(flow.IsActivated);
        Assert.IsTrue(flow.IsPersistent);
        Assert.AreEqual(42, ((TokenHolder<int>)flow.Tokens.Single()).Payload);
    }

    [TestMethod]
    public async Task FluentStreamInputSendsTokensWithoutMutatingCallerHeaders()
    {
        var recording = new RecordingActivity();
        IActivityBehavior behavior = recording;
        var suppliedStream = new Stateflows.Common.Stream();
        var metadata = new Marker();
        var headers = new Dictionary<string, EventHeader>
        {
            ["metadata"] = metadata,
            [nameof(Stateflows.Common.Stream)] = suppliedStream
        };

        var result = await behavior.StreamInputAsync(tokens => tokens.Add(42).Add("payload"), headers);

        Assert.AreEqual(EventStatus.Accepted, result.Status);
        Assert.AreEqual(1, recording.Sends);
        Assert.IsInstanceOfType<TokensInput>(recording.Payload);
        var input = (TokensInput)recording.Payload!;
        CollectionAssert.AreEqual(new object[] { 42, "payload" }, input.Tokens.Select(token => token.BoxedPayload).ToArray());
        Assert.AreNotSame(headers, recording.Headers);
        Assert.AreSame(metadata, recording.Headers!["metadata"]);
        Assert.IsInstanceOfType<Stateflows.Common.Stream>(recording.Headers[nameof(Stateflows.Common.Stream)]);
        Assert.AreSame(suppliedStream, headers[nameof(Stateflows.Common.Stream)]);
        Assert.AreEqual(2, headers.Count);
    }

    [TestMethod]
    public async Task FluentCheckpointSendsDedicatedEventWithCallerHeaders()
    {
        var recording = new RecordingActivity();
        IActivityBehavior behavior = recording;
        var headers = new Dictionary<string, EventHeader> { ["metadata"] = new Marker() };
        await behavior.CheckpointAsync(headers);

        Assert.AreEqual(1, recording.Sends);
        Assert.IsInstanceOfType<ActivityCheckpoint>(recording.Payload);
        Assert.AreSame(headers, recording.Headers);
        Assert.AreEqual(1, headers.Count);
    }

    [TestMethod]
    public void FluentStreamInputRejectsNullOrFailingProducerWithoutSending()
    {
        var recording = new RecordingActivity();
        IActivityBehavior behavior = recording;
        Assert.Throws<ArgumentNullException>(() => behavior.StreamInputAsync(null!));
        Assert.Throws<InvalidOperationException>(() => behavior.StreamInputAsync(tokens =>
        {
            tokens.Add(42);
            throw new InvalidOperationException("producer failed");
        }));
        Assert.AreEqual(0, recording.Sends);
    }

    [TestMethod]
    public async Task FluentStreamInputPropagatesAdmissionFailureWithoutRetry()
    {
        var failure = new InvalidOperationException("transport unavailable");
        var recording = new RecordingActivity { Failure = failure };
        IActivityBehavior behavior = recording;
        var observed = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => behavior.StreamInputAsync(tokens => tokens.Add(42)));

        Assert.AreSame(failure, observed);
        Assert.AreEqual(1, recording.Sends);
    }

    [TestMethod]
    public async Task LegacyRuntimeRejectsStreamAndCheckpointHeaders()
    {
        using var provider = new ServiceCollection()
            .AddLogging()
            .AddStateflows(sf => sf.AddLegacyRuntime().AddActivities(activities =>
                activities.AddActivity("streaming-admission", _ => { })))
            .BuildServiceProvider();
        provider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
            provider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
        var processor = provider.GetServices<IEventProcessor>()
            .Single(candidate => candidate.BehaviorTypes.Contains(BehaviorType.Activity));
        var id = new BehaviorId(BehaviorType.Activity, "streaming-admission", Guid.NewGuid().ToString("N"));
        var exceptions = new List<Exception>();

        var streamStatus = await processor.ProcessEventAsync(
            id,
            new TokensInput().Add(42).ToEventHolder(new Dictionary<string, EventHeader>
            {
                [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream()
            }),
            exceptions);
        var checkpointStatus = await processor.ProcessEventAsync(
            id,
            new Initialize().ToEventHolder(new Dictionary<string, EventHeader>
            {
                [nameof(Checkpoint)] = new Checkpoint()
            }),
            exceptions);

        Assert.AreEqual(EventStatus.Rejected, streamStatus);
        Assert.AreEqual(EventStatus.Rejected, checkpointStatus);
        Assert.AreEqual(EventStatus.Rejected, await processor.ProcessEventAsync(
            id, new ActivityCheckpoint().ToEventHolder(), exceptions));
        Assert.AreEqual(0, exceptions.Count);
    }

    private static RootContext NewRoot()
        => new(new StateflowsContext(new BehaviorId(BehaviorType.Activity, "provenance", Guid.NewGuid().ToString("N"))));

    public abstract record Animal;
    public sealed record Dog(string Name) : Animal;
    public sealed class Marker : EventHeader;

    private sealed class RecordingActivity : IActivityBehavior
    {
        public BehaviorId Id => new ActivityId("recording", "client");
        public int Sends { get; private set; }
        public object? Payload { get; private set; }
        public IDictionary<string, EventHeader>? Headers { get; private set; }
        public Exception? Failure { get; init; }

        public Task<SendResult> SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers = null)
        {
            Sends++;
            Payload = @event;
            Headers = headers;
            return Failure == null
                ? Task.FromResult(new SendResult(EventStatus.Accepted, new EventValidation(true)))
                : Task.FromException<SendResult>(Failure);
        }

        public Task<RequestResult<T>> RequestAsync<T>(IRequest<T> request, IDictionary<string, EventHeader>? headers = null)
            => throw new InvalidOperationException("StreamInputAsync must send, not request.");
        public Task<RequestResult<TokensOutput>> SendInputAsync(Action<ITokensInput> tokensAction, IDictionary<string, EventHeader>? headers = null)
            => throw new InvalidOperationException("StreamInputAsync must use the streaming send API.");
        public Task<RequestResult<TokensOutput>> SendInputAsync<TToken>(params TToken[] tokens)
            => throw new InvalidOperationException("StreamInputAsync must use the streaming send API.");
        public Task<IWatcher> WatchAsync<T>(Func<T, Task> handler, DateTime? replayNotificationsSince = null)
            => throw new InvalidOperationException("Unexpected watch.");
        public Task<IWatcher> WatchAsync(string[] names, Func<EventHolder, Task> handler, DateTime? replayNotificationsSince = null)
            => throw new InvalidOperationException("Unexpected watch.");
        public Task<IEnumerable<T>> GetNotificationsAsync<T>(DateTime? lastNotificationsCheck = null)
            => throw new InvalidOperationException("Unexpected notification request.");
        public Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] names, DateTime? lastNotificationsCheck = null)
            => throw new InvalidOperationException("Unexpected notification request.");
        public void Dispose() { }
    }
}
