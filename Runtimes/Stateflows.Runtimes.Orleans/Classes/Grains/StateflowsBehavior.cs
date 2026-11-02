using Stateflows.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Engine.Interfaces;
using Stateflows.Activities.Streams;
using Stateflows.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using System.Collections.Generic;
using System.Threading;

namespace Stateflows;

[BehaviorPlacementFilter]
internal class StateflowsBehavior( 
    [StateflowsPersistentState(StateflowsStateNames.BehaviorContext)]
    IPersistentState<BehaviorContextState> contextState,
    IStateflowsInterceptor interceptor,
    IStateflowsSubscriber subscriber,
    IEnumerable<IStateflowsValidator> validators,
    IEnumerable<IEventProcessor> processors,
    IStateflowsTenantExecutor tenantExecutor,
    IStateflowsResources resources,
    LocalBehaviorCatalog catalog,
    IOptions<ReminderOptions> reminderOptions,
    IOptions<StateflowsRuntimeOptions> runtimeOptions,
    ILogger<StateflowsBehavior> logger
) : Grain, IBehaviorGrain, IStateflowsExecutor
{
    private IGrainTimer? Timer;
    private string? TenantId;
    private BehaviorId BehaviorId;
    private readonly AsyncLocal<OrleansEventHolder?> incomingEvent = new();
    private StateflowsContext? committedContext;
    // Trigger time of the reminder this activation knows to be registered; unknown until the activation registers one.
    private DateTime? registeredTrigger;
    private bool registrationKnown;
    // Whether this instance is known to be in the instance index.
    private bool indexed;
    // Set when a state write failed: the provider may still have persisted it.
    private bool commitOutcomeUnknown;
    private readonly object streamSync = new();
    private readonly SemaphoreSlim streamAdmissionGate = new(1, 1);
    private readonly SemaphoreSlim streamCheckpointGate = new(1, 1);
    private readonly Queue<AdmittedStreamEvent> streamQueue = new();
    private readonly Queue<AdmittedStreamEvent> streamNextEpochQueue = new();
    private readonly HashSet<Task> streamExecutions = [];
    private readonly HashSet<Guid> queuedLiveTimeEvents = [];
    private int streamInFlight;
    private TaskCompletionSource? streamPumpCompletion;
    private TaskCompletionSource? streamQueueChanged;
    private StateflowsContext? streamLiveContext;
    private StateflowsContext? streamFinalizeContext;
    private bool streamFinalizationRequested;
    private long streamGeneration;
    private readonly AsyncLocal<long?> streamExecutionGeneration = new();
    private volatile bool stoppingStreamAdmission;
    private bool checkpointRequested;
    private bool checkpointResumeRequired;
    private bool streamCommitInProgress;
    private TaskCompletionSource? checkpointReached;
    private TaskCompletionSource? resumeStreamAfterCheckpoint;
    private readonly Dictionary<string, IEventProcessor> Processors = processors
        .SelectMany(p => p.BehaviorTypes.Select(type => new { Type = type, Processor = p }))
        .ToDictionary(p => p.Type, p => p.Processor);
    private readonly IStateflowsValidator[] Validators = validators.ToArray();

    private sealed record AdmittedStreamEvent(OrleansEventHolder Event, IDisposable ResourceLease, Guid? TimeEventId = null);

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        try
        {
            var tenantBehaviorId = StateflowsGrainKeys.ParseBehavior(this.GetPrimaryKeyString());
            TenantId = tenantBehaviorId.TenantId;
            BehaviorId = tenantBehaviorId.BehaviorId;
            if (!catalog.Classes.Contains(BehaviorId.BehaviorClass))
            {
                throw new InvalidOperationException($"This silo does not host Stateflows behavior '{BehaviorId.BehaviorClass}'.");
            }

            committedContext = contextState.RecordExists && !string.IsNullOrEmpty(contextState.State?.Payload)
                ? StateflowsJsonConverter.DeserializeObject<StateflowsContext>(contextState.State.Payload)
                : null;
            contextState.State.Outbox ??= [];

            indexed = committedContext != null && contextState.State?.Tombstone != true;

            if (committedContext != null && !catalog.Supports(BehaviorId.BehaviorClass, committedContext.Version))
            {
                throw new InvalidOperationException(
                    $"This silo does not host version {committedContext.Version} of Stateflows behavior '{BehaviorId.BehaviorClass}' required by instance '{BehaviorId.Instance}'.");
            }
            
            Timer = this.RegisterGrainTimer(
                static (state, ct) => state.PollActivityTimeEventsAsync(ct),
                this,
                dueTime: TimeSpan.FromMilliseconds(250),
                period: TimeSpan.FromMilliseconds(250));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        await base.OnActivateAsync(cancellationToken);
        if (contextState.State.Outbox.Count > 0)
            await DrainOutboxAsync();
        if (BehaviorId.Type == BehaviorType.Activity &&
            Processors.TryGetValue(BehaviorType.Activity, out var hostingProcessor) &&
            hostingProcessor is IActivityStreamSessionProcessor hostingSessionProcessor)
        {
            hostingSessionProcessor.SetActivitySessionWorkHost(BehaviorId, HostHandedOffActivityWork);
        }
        if (BehaviorId.Type == BehaviorType.Activity && committedContext != null &&
            Processors.TryGetValue(BehaviorType.Activity, out var recoveredProcessor) &&
            recoveredProcessor is IActivityStreamSessionProcessor recoveredSessionProcessor)
        {
            await tenantExecutor.ExecuteByTenantAsync(TenantId, async () =>
            {
                StateflowsContext? resumedContext = null;
                using var storageScope = GrainStorageBridge.EnterScope(
                    () => Task.FromResult(streamLiveContext ?? committedContext ?? new StateflowsContext(BehaviorId)),
                    context =>
                    {
                        resumedContext = context;
                        return Task.CompletedTask;
                    });
                try
                {
                    if (await recoveredSessionProcessor.ResumeCheckpointedActivityAsync(BehaviorId))
                    {
                        // Recovery runs as its own unit; later ordinary events use the regular per-event path.
                        await recoveredSessionProcessor.EndActivitySessionAsync(BehaviorId, persist: true);
                        if (resumedContext != null)
                            await CommitAsync(resumedContext);
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Failed to resume checkpointed Activity work for {BehaviorId}.", BehaviorId);
                }
            });
        }
    }

    public async Task CancelAsync()
    {
        Common.Context.Classes.BaseContext[] contexts;
        lock (Common.Context.Classes.BaseContext.Instances)
        {
            contexts = Common.Context.Classes.BaseContext.Instances.TryGetValue(BehaviorId, out var current)
                ? current.ToArray()
                : [];
        }

        foreach (var context in contexts)
        {
            try
            {
                await context.CancellationTokenSource.CancelAsync();
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    public async Task<OrleansRequestResult> ProcessEventAsync(
        OrleansEventHolder orleansEventHolder, CancellationToken cancellationToken)
    {
        var previousIncomingEvent = incomingEvent.Value;
        var queuedFinalize = false;
        var standaloneCheckpoint = ((EventHolder)orleansEventHolder).BoxedPayload is ActivityCheckpoint;
        if (standaloneCheckpoint && BehaviorId.Type != BehaviorType.Activity)
            return new RequestResult(null, EventStatus.Rejected, new EventValidation(true));

        if (orleansEventHolder.IsRequest &&
            ((EventHolder)orleansEventHolder).BoxedPayload is TokensInputEvent &&
            HasActiveStream())
        {
            return new RequestResult(null, EventStatus.Rejected, new EventValidation(true));
        }

        if (((EventHolder)orleansEventHolder).BoxedPayload is Finalize { Mode: FinalizationMode.Immediate } && HasActiveStream())
        {
            return await RequestImmediateStreamFinalizeAsync(orleansEventHolder, cancellationToken);
        }

        if (((EventHolder)orleansEventHolder).BoxedPayload is Finalize { Mode: FinalizationMode.Queued } && HasActiveStream())
        {
            stoppingStreamAdmission = true;
            queuedFinalize = true;
            Task? active;
            lock (streamSync)
            {
                while (streamQueue.TryDequeue(out var pending))
                    ReleaseAdmittedStreamEvent(pending);
                while (streamNextEpochQueue.TryDequeue(out var pending))
                    ReleaseAdmittedStreamEvent(pending);
                streamQueueChanged?.TrySetResult();
                active = streamPumpCompletion?.Task;
            }
            _ = CancelStreamContextsAsync();
            if (active != null)
                await active.WaitAsync(cancellationToken);
        }

        var checkpointRequestedByCall = standaloneCheckpoint || orleansEventHolder.Headers.ContainsKey(nameof(Checkpoint));
        if (HasActiveStream() && checkpointRequestedByCall)
        {
            Task checkpointTask;
            lock (streamSync)
            {
                checkpointRequested = true;
                checkpointReached ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                resumeStreamAfterCheckpoint ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                while (streamQueue.TryDequeue(out var pending))
                    streamNextEpochQueue.Enqueue(pending);
                streamQueueChanged?.TrySetResult();
                checkpointTask = checkpointReached.Task;
            }

            try
            {
                if (Processors.TryGetValue(BehaviorType.Activity, out var checkpointProcessor) &&
                    checkpointProcessor is IActivityStreamSessionProcessor checkpointSessionProcessor)
                    await checkpointSessionProcessor.RequestActivitySessionCheckpointPauseAsync(BehaviorId);
                runtimeOptions.Value.ActivityCheckpointPauseRequestedForTesting?.Invoke(BehaviorId);

                var timeout = runtimeOptions.Value.ActivityStreamBarrierTimeout;
                if (timeout == Timeout.InfiniteTimeSpan)
                    await checkpointTask.WaitAsync(cancellationToken);
                else
                    await checkpointTask.WaitAsync(timeout, cancellationToken);
            }
            catch
            {
                lock (streamSync)
                    checkpointResumeRequired = true;
                ReleaseCheckpointBarrier();
                throw;
            }
        }
        orleansEventHolder.Headers.Remove(nameof(Checkpoint));
        // Every regular Activity event runs in the activation-owned session, so a stream admitted while it is in flight
        // joins the very same network (Executor/RootContext) instead of a separate per-event instance. Only resets
        // without a live stream replace the context and keep the ordinary per-event path.
        var isReset = ((EventHolder)orleansEventHolder).BoxedPayload is Reset ||
            orleansEventHolder.Headers.ContainsKey(nameof(ForcedReset));
        var ordinaryActivityCall = BehaviorId.Type == BehaviorType.Activity && isReset && !HasActiveStream();
        if (BehaviorId.Type == BehaviorType.Activity && !ordinaryActivityCall)
        {
            orleansEventHolder.Headers[nameof(ActivitySessionExecution)] =
                StateflowsJsonConverter.SerializePolymorphicObject(new ActivitySessionExecution
                {
                    Checkpoint = checkpointRequestedByCall
                });
        }

        if (commitOutcomeUnknown)
        {
            ReleaseCheckpointBarrier();
            throw new InvalidOperationException("The previous state write has an unknown outcome; retry on a fresh activation.");
        }

        if (!resources.ResourcesByBehaviorClass.TryGetValue(BehaviorId.BehaviorClass, out var resource))
        {
            ReleaseCheckpointBarrier();
            throw new InvalidOperationException($"No resource is registered for Stateflows behavior '{BehaviorId.BehaviorClass}'.");
        }

        var admission = await resource.TryAcquireAsync(cancellationToken);
        if (!admission.IsAdmitted)
        {
            ReleaseCheckpointBarrier();
            // Nothing is executed, committed or recorded as received, so the sender may retry (here or on another silo).
            StateflowsOrleansDiagnostics.AdmissionRejected(resource.Name, admission.Rejection.ToString(), BehaviorId.BehaviorClass);
            logger.LogDebug("Event {EventName} for Stateflows behavior {BehaviorId} was rejected by resource '{Resource}': {Reason}.",
                orleansEventHolder.Name, BehaviorId, resource.Name, admission.Rejection);
            return new RequestResult(null, EventStatus.Overloaded, new EventValidation(true));
        }

        using var lease = admission.Lease;
        var status = EventStatus.Invalid;
        var responses = new Dictionary<object, EventHolder>();
        var eventHolder = (EventHolder)orleansEventHolder;

        ResponseHolder.SetResponses(responses);
        if (ordinaryActivityCall)
            BeginOrdinaryActivityCall();
        var sessionActivityCall = BehaviorId.Type == BehaviorType.Activity && !ordinaryActivityCall;
        if (sessionActivityCall)
            Interlocked.Increment(ref sessionActivityCalls);
        try
        {
            if (new StoredNotification { Notification = orleansEventHolder }.IsExpired(DateTime.UtcNow))
            {
                return new RequestResult(null, EventStatus.Invalid, new EventValidation(false));
            }

            if (Processors.TryGetValue(BehaviorId.Type, out var processor) &&
                (processor is not Actions.Engine.Processor actions ||
                    actions.IsStateful(BehaviorId, committedContext?.Version ?? 0)) &&
                !IsDeduplicationDisabled(eventHolder))
            {
                if (committedContext?.EventReceipts.TryGetValue(orleansEventHolder.Id, out var expiresAt) == true &&
                    expiresAt > DateTime.UtcNow)
                {
                    return new RequestResult(null, EventStatus.Duplicated, new EventValidation(true));
                }

                incomingEvent.Value = orleansEventHolder;
            }

            var validation = await eventHolder.ValidateAsync(Validators);

            await tenantExecutor.ExecuteByTenantAsync(TenantId, async () =>
            {
                try
                {
                    if (validation.IsValid)
                    {
                        status = await eventHolder.ExecuteAsync(this);
                    }
                    else if (incomingEvent.Value.HasValue)
                    {
                        var context = CloneCommittedContext();
                        RecordEventReceipt(context);
                        await CommitAsync(context);
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Failed to execute Stateflows behavior {BehaviorId}.", BehaviorId);
                    status = EventStatus.Failed;
                }
            });

            return new RequestResult(eventHolder.GetResponseHolder(), status, eventHolder.Validation);
        }
        finally
        {
            if (checkpointRequestedByCall && status != EventStatus.Failed && committedContext != null)
            {
                lock (streamSync)
                    streamLiveContext = StateflowsJsonConverter.Clone(committedContext);
            }
            incomingEvent.Value = previousIncomingEvent;
            ResponseHolder.ClearResponses();
            if (queuedFinalize)
                stoppingStreamAdmission = false;
            if (checkpointRequestedByCall)
                ReleaseCheckpointBarrier();
            if (ordinaryActivityCall)
                EndOrdinaryActivityCall();
            if (sessionActivityCall)
                Interlocked.Decrement(ref sessionActivityCalls);
        }
    }

    private int sessionActivityCalls;
    private int ordinaryActivityCalls;
    private TaskCompletionSource? ordinaryActivityCallsIdle;

    private void BeginOrdinaryActivityCall()
    {
        lock (streamSync)
        {
            if (ordinaryActivityCalls++ == 0)
                ordinaryActivityCallsIdle = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    private void EndOrdinaryActivityCall()
    {
        lock (streamSync)
        {
            if (--ordinaryActivityCalls == 0)
            {
                ordinaryActivityCallsIdle?.TrySetResult();
                ordinaryActivityCallsIdle = null;
            }
        }
    }

    private void ReleaseCheckpointBarrier()
    {
        lock (streamSync)
        {
            resumeStreamAfterCheckpoint?.TrySetResult();
            checkpointRequested = false;
            checkpointReached = null;
            resumeStreamAfterCheckpoint = null;
            while (streamNextEpochQueue.TryDequeue(out var pending))
                streamQueue.Enqueue(pending);
        }
    }

    public async Task<OrleansRequestResult> StreamEventAsync(
        OrleansEventHolder orleansEventHolder, CancellationToken cancellationToken)
    {
        await streamAdmissionGate.WaitAsync(cancellationToken);
        try
        {
            return await AdmitStreamEventAsync(orleansEventHolder, cancellationToken);
        }
        finally
        {
            streamAdmissionGate.Release();
        }
    }

    private async Task<OrleansRequestResult> AdmitStreamEventAsync(
        OrleansEventHolder orleansEventHolder, CancellationToken cancellationToken)
    {
        EventHolder original = orleansEventHolder;
        if (BehaviorId.Type != BehaviorType.Activity ||
            original.BoxedPayload is not TokensInputEvent ||
            !original.Headers.Values.Any(header => header is Common.Stream))
        {
            return new RequestResult(null, EventStatus.Rejected, new EventValidation(true));
        }

        if (commitOutcomeUnknown || stoppingStreamAdmission)
            return new RequestResult(null, EventStatus.Overloaded, new EventValidation(true));

        if (orleansEventHolder.IsRequest)
            return new RequestResult(null, EventStatus.Rejected, new EventValidation(true));

        // A context-replacing reset in flight owns the context; the stream joins the state it leaves behind.
        Task? ordinaryIdle;
        lock (streamSync)
            ordinaryIdle = ordinaryActivityCallsIdle?.Task;
        if (ordinaryIdle != null)
            await ordinaryIdle.WaitAsync(cancellationToken);

        if (!Processors.TryGetValue(BehaviorType.Activity, out var activityProcessor) ||
            activityProcessor is not IActivityStreamAdmissionPolicy streamPolicy ||
            !streamPolicy.CanAdmitStream(BehaviorId, streamLiveContext ?? committedContext ?? new StateflowsContext(BehaviorId)) ||
            !streamPolicy.AcceptsInput(BehaviorId, streamLiveContext ?? committedContext ?? new StateflowsContext(BehaviorId),
                (TokensInputEvent)original.BoxedPayload))
        {
            return new RequestResult(null, EventStatus.Rejected, new EventValidation(true));
        }

        if (activityProcessor is not IActivityStreamSessionProcessor sessionProcessor)
            return new RequestResult(null, EventStatus.Rejected, new EventValidation(true));
        var sessionInitialized = await sessionProcessor.WaitForActivitySessionInitializationAsync(BehaviorId);
        // A regular call admitted just before this stream may not have opened the activation session yet; the stream
        // must join that session (and wait for its initialization) instead of initializing a separate network.
        while (sessionInitialized == null && System.Threading.Volatile.Read(ref sessionActivityCalls) > 0 && !HasActiveStream())
        {
            await Task.Delay(5, cancellationToken);
            sessionInitialized = await sessionProcessor.WaitForActivitySessionInitializationAsync(BehaviorId);
        }
        if (sessionInitialized == false)
            return new RequestResult(null, EventStatus.Rejected, new EventValidation(true));

        var maximum = runtimeOptions.Value.MaxQueuedActivityStreamEvents;
        if (maximum < 1)
            throw new InvalidOperationException("MaxQueuedActivityStreamEvents must be greater than zero.");

        if (!resources.ResourcesByBehaviorClass.TryGetValue(BehaviorId.BehaviorClass, out var resource))
            throw new InvalidOperationException($"No resource is registered for Stateflows behavior '{BehaviorId.BehaviorClass}'.");

        lock (streamSync)
        {
            if (stoppingStreamAdmission || streamQueue.Count + streamNextEpochQueue.Count + streamInFlight >= maximum)
                return new RequestResult(null, EventStatus.Overloaded, new EventValidation(true));
        }

        var admission = await resource.TryAcquireAsync(cancellationToken);
        if (!admission.IsAdmitted)
            return new RequestResult(null, EventStatus.Overloaded, new EventValidation(true));

        var lease = admission.Lease!;
        try
        {
            var validation = await original.ValidateAsync(Validators);
            if (!validation.IsValid)
                return new RequestResult(null, EventStatus.Invalid, validation);

            var current = streamLiveContext ?? committedContext;
            if (sessionInitialized != true && current?.Status is not BehaviorStatus.Initialized)
            {
                var initialization = new Initialize().ToEventHolder();
                var initialized = EventStatus.Undelivered;
                try
                {
                    await tenantExecutor.ExecuteByTenantAsync(TenantId, async () =>
                        initialized = await initialization.ExecuteAsync(this));
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception, "Cold Activity initialization failed before stream admission for {BehaviorId}.", BehaviorId);
                    return new RequestResult(null, EventStatus.Rejected, validation);
                }
                if (initialized is not (EventStatus.Initialized or EventStatus.Consumed))
                    return new RequestResult(null, EventStatus.Rejected, validation);
            }

            lock (streamSync)
            {
                if (stoppingStreamAdmission || streamQueue.Count + streamNextEpochQueue.Count + streamInFlight >= maximum)
                    return new RequestResult(null, EventStatus.Overloaded, new EventValidation(true));

                var streamed = (EventHolder)StateflowsJsonConverter.Clone(original);
                streamed.Headers.Remove(nameof(Common.Stream));
                streamed.Headers[nameof(StreamedActivityExecution)] = new StreamedActivityExecution();
                if (streamed.BoxedPayload is TokensInputEvent tokensInput)
                {
                    foreach (var token in tokensInput.Tokens)
                        token.IsStreamed = true;
                    if (streamed.BoxedPayload is IRequest<TokensOutput>)
                    {
                        streamed = CopyEnvelope(streamed, new TokensInputEvent { Tokens = tokensInput.Tokens });
                    }
                }

                var work = new AdmittedStreamEvent((OrleansEventHolder)streamed, lease);
                (checkpointRequested || streamCommitInProgress ? streamNextEpochQueue : streamQueue).Enqueue(work);
                streamQueueChanged?.TrySetResult();
                lease = null!;
                if (streamPumpCompletion == null)
                {
                    streamPumpCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                    streamGeneration++;
                    _ = DrainStreamQueueAsync(streamPumpCompletion, streamGeneration);
                }
            }

            return new RequestResult(null, EventStatus.Accepted, validation);
        }
        finally
        {
            lease?.Dispose();
        }
    }

    private bool HasActiveStream()
    {
        lock (streamSync)
            return streamPumpCompletion != null;
    }

    private async Task<StateflowsContext?> EndActivitySessionAsync(StateflowsContext checkpoint, bool persist = true)
    {
        if (!Processors.TryGetValue(BehaviorType.Activity, out var processor) ||
            processor is not IActivityStreamSessionProcessor sessionProcessor)
            return checkpoint;

        StateflowsContext? finalContext = null;
        using var scope = GrainStorageBridge.EnterScope(
            () => Task.FromResult(checkpoint),
            context =>
            {
                finalContext = context;
                return Task.CompletedTask;
            });
        await sessionProcessor.EndActivitySessionAsync(BehaviorId, persist);
        return finalContext;
    }

    private async Task ResumeCheckpointedActivitySessionAsync()
    {
        if (!Processors.TryGetValue(BehaviorType.Activity, out var processor) ||
            processor is not IActivityStreamSessionProcessor sessionProcessor ||
            runtimeOptions.Value.SuspendActivityCheckpointResumeForTesting)
            return;

        StateflowsContext? resumedContext = null;
        using var storageScope = GrainStorageBridge.EnterScope(
            () => Task.FromResult(streamLiveContext ?? committedContext ?? new StateflowsContext(BehaviorId)),
            context =>
            {
                resumedContext = context;
                lock (streamSync)
                    streamLiveContext = StateflowsJsonConverter.Clone(context);
                return Task.CompletedTask;
            });
        await tenantExecutor.ExecuteByTenantAsync(TenantId,
            () => sessionProcessor.ResumeCheckpointedActivityAsync(BehaviorId));
        if (resumedContext != null)
        {
            lock (streamSync)
                streamLiveContext = StateflowsJsonConverter.Clone(resumedContext);
        }
    }

    private static EventHolder CopyEnvelope(EventHolder source, TokensInputEvent payload)
    {
        var result = payload.ToTypedEventHolder(source.Headers);
        result.Id = source.Id;
        result.Retained = source.Retained;
        result.SenderId = source.SenderId;
        result.SentAt = source.SentAt;
        result.TimeToLive = source.TimeToLive;
        return result;
    }

    private async Task PollActivityTimeEventsAsync(CancellationToken cancellationToken)
    {
        if (!HasActiveStream() || stoppingStreamAdmission ||
            !Processors.TryGetValue(BehaviorType.Activity, out var processor) ||
            processor is not IActivityStreamSessionProcessor sessionProcessor ||
            !resources.ResourcesByBehaviorClass.TryGetValue(BehaviorId.BehaviorClass, out var resource))
            return;

        TimeEvent[] dueEvents;
        try
        {
            dueEvents = (await sessionProcessor.GetActivitySessionTimeEventsAsync(BehaviorId))
                .Where(timeEvent => timeEvent.TriggerTime <= DateTime.UtcNow)
                .OrderBy(timeEvent => timeEvent.TriggerTime)
                .ToArray();
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to inspect live Activity timers for {BehaviorId}.", BehaviorId);
            return;
        }

        foreach (var timeEvent in dueEvents)
        {
            lock (streamSync)
            {
                var maximum = runtimeOptions.Value.MaxQueuedActivityStreamEvents;
                if (stoppingStreamAdmission || streamQueue.Count + streamNextEpochQueue.Count + streamInFlight >= maximum)
                    return;
                if (!queuedLiveTimeEvents.Add(timeEvent.Id))
                    continue;
            }

            var admission = await resource.TryAcquireAsync(cancellationToken);
            if (!admission.IsAdmitted)
            {
                lock (streamSync)
                    queuedLiveTimeEvents.Remove(timeEvent.Id);
                return;
            }

            try
            {
                var holder = timeEvent.ToTypedEventHolder();
                holder.Id = timeEvent.Id;
                // The live timer only delivers the time event; its source is transactional (see contract), so the
                // accepting firing runs as a regular transaction inside the live session.
                holder.Headers[nameof(StreamedActivityExecution)] = new StreamedActivityExecution { TransactionalSource = true };
                var work = new AdmittedStreamEvent((OrleansEventHolder)holder, admission.Lease!, timeEvent.Id);
                lock (streamSync)
                {
                    if (stoppingStreamAdmission || streamPumpCompletion == null)
                    {
                        ReleaseAdmittedStreamEvent(work);
                        continue;
                    }

                    (checkpointRequested || streamCommitInProgress ? streamNextEpochQueue : streamQueue).Enqueue(work);
                    streamQueueChanged?.TrySetResult();
                }
            }
            catch
            {
                admission.Lease?.Dispose();
                lock (streamSync)
                    queuedLiveTimeEvents.Remove(timeEvent.Id);
                throw;
            }
        }
    }

    private void ReleaseAdmittedStreamEvent(AdmittedStreamEvent work)
    {
        work.ResourceLease.Dispose();
        if (work.TimeEventId.HasValue)
        {
            lock (streamSync)
                queuedLiveTimeEvents.Remove(work.TimeEventId.Value);
        }
    }

    private async Task<OrleansRequestResult> RequestImmediateStreamFinalizeAsync(
        OrleansEventHolder eventHolder, CancellationToken cancellationToken)
    {
        stoppingStreamAdmission = true;
        Interlocked.Increment(ref streamGeneration);
        lock (streamSync)
        {
            streamFinalizationRequested = true;
            streamFinalizeContext = streamLiveContext;
            streamLiveContext = null;
            while (streamQueue.TryDequeue(out var pending))
                ReleaseAdmittedStreamEvent(pending);
            while (streamNextEpochQueue.TryDequeue(out var pending))
                ReleaseAdmittedStreamEvent(pending);
        }
        _ = CancelStreamContextsAsync();
        _ = FinalizeAfterStreamAsync(eventHolder);
        return new RequestResult(null, EventStatus.Cancelled, new EventValidation(true));
    }

    private async Task CancelStreamContextsAsync()
    {
        try
        {
            await CancelAsync();
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Cancellation callbacks failed while stopping Activity stream {BehaviorId}.", BehaviorId);
        }
    }

    private async Task FinalizeAfterStreamAsync(OrleansEventHolder eventHolder)
    {
        Task? active;
        lock (streamSync)
            active = streamPumpCompletion?.Task;
        if (active != null)
        {
            try { await active; }
            catch (Exception exception) { logger.LogError(exception, "Activity stream drain failed before finalization for {BehaviorId}.", BehaviorId); }
        }
        try
        {
            StateflowsContext checkpoint;
            lock (streamSync)
                checkpoint = streamFinalizeContext ?? streamLiveContext ?? committedContext ?? new StateflowsContext(BehaviorId);
            streamLiveContext = await EndActivitySessionAsync(checkpoint, persist: false) ?? checkpoint;
            var result = await ProcessEventAsync(eventHolder, CancellationToken.None);
            logger.LogDebug("Activity stream finalization completed for {BehaviorId} with status {Status}.", BehaviorId, result.Status);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Activity stream finalization failed for {BehaviorId}.", BehaviorId);
        }
        finally
        {
            lock (streamSync)
            {
                streamLiveContext = null;
                streamFinalizeContext = null;
                streamFinalizationRequested = false;
            }
            stoppingStreamAdmission = false;
        }
    }

    private async Task DrainStreamQueueAsync(TaskCompletionSource completion, long generation)
    {
        await Task.Yield();
        try
        {
            while (true)
            {
                AdmittedStreamEvent? work = null;
                StateflowsContext? checkpoint = null;
                var checkpointAtQuiescence = false;
                var resumeCheckpointSession = false;
                Task? resume = null;
                Task? cancellationDrain = null;
                lock (streamSync)
                {
                    if (generation != Interlocked.Read(ref streamGeneration))
                    {
                        while (streamQueue.TryDequeue(out var pending))
                            ReleaseAdmittedStreamEvent(pending);
                        while (streamNextEpochQueue.TryDequeue(out var pending))
                            ReleaseAdmittedStreamEvent(pending);
                        if (streamExecutions.Count > 0)
                        {
                            cancellationDrain = Task.WhenAll(streamExecutions);
                        }
                        else
                        {
                            streamLiveContext = null;
                            streamPumpCompletion = null;
                            completion.TrySetResult();
                            return;
                        }
                    }
                    else
                    {
                        if (checkpointResumeRequired)
                        {
                            if (streamExecutions.Count > 0)
                            {
                                resume = Task.WhenAll(streamExecutions);
                            }
                            else
                            {
                                checkpointResumeRequired = false;
                                resumeCheckpointSession = true;
                                resume = Task.CompletedTask;
                            }
                        }
                        else if (streamQueue.Count == 0)
                        {
                            if (streamExecutions.Count > 0)
                            {
                                streamQueueChanged ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                                resume = Task.WhenAny(Task.WhenAll(streamExecutions), streamQueueChanged.Task);
                            }
                            else if (checkpointRequested)
                            {
                                checkpointReached?.TrySetResult();
                                resume = resumeStreamAfterCheckpoint?.Task;
                                resumeCheckpointSession = resume != null;
                            }
                            else if (streamLiveContext != null)
                            {
                                checkpointAtQuiescence = true;
                            }
                            else
                            {
                                streamPumpCompletion = null;
                                completion.TrySetResult();
                                return;
                            }
                        }
                        work = checkpoint == null && !checkpointAtQuiescence &&
                            (resume == null || streamQueue.Count > 0) ? streamQueue.Dequeue() : null;
                        if (work != null)
                            streamInFlight++;
                    }
                }

                if (cancellationDrain != null)
                {
                    try { await cancellationDrain; }
                    catch (Exception exception) { logger.LogDebug(exception, "Cancelled Activity stream work ended with an error for {BehaviorId}.", BehaviorId); }
                    lock (streamSync)
                        streamExecutions.Clear();
                    continue;
                }

                if (checkpointAtQuiescence)
                {
                    Task? checkpointResume = null;
                    await streamCheckpointGate.WaitAsync();
                    await streamAdmissionGate.WaitAsync();
                    try
                    {
                        var waitForCheckpointCall = false;
                        lock (streamSync)
                        {
                            if (generation == Interlocked.Read(ref streamGeneration) &&
                                streamQueue.Count == 0 &&
                                streamExecutions.Count == 0 &&
                                streamLiveContext != null)
                            {
                                checkpoint = streamLiveContext;
                                waitForCheckpointCall = checkpointRequested;
                                if (!waitForCheckpointCall)
                                    streamLiveContext = null;
                                streamCommitInProgress = true;
                            }
                        }

                        if (checkpoint != null)
                        {
                            checkpoint = await EndActivitySessionAsync(checkpoint, persist: true) ?? checkpoint;
                            if (waitForCheckpointCall)
                            {
                                lock (streamSync)
                                {
                                    streamLiveContext = checkpoint;
                                    checkpointReached?.TrySetResult();
                                    checkpointResume = resumeStreamAfterCheckpoint?.Task;
                                }
                            }
                            else
                            {
                                await CommitAsync(checkpoint);
                            }
                        }
                    }
                    finally
                    {
                        lock (streamSync)
                        {
                            streamCommitInProgress = false;
                            if (!checkpointRequested)
                            {
                                while (streamNextEpochQueue.TryDequeue(out var pending))
                                    streamQueue.Enqueue(pending);
                            }
                            streamQueueChanged?.TrySetResult();
                        }
                        streamAdmissionGate.Release();
                        streamCheckpointGate.Release();
                    }
                    if (checkpointResume != null)
                    {
                        await checkpointResume;
                        await ResumeCheckpointedActivitySessionAsync();
                        lock (streamSync)
                            streamQueueChanged = null;
                    }
                    continue;
                }

                if (resume != null)
                {
                    await resume;
                    if (resumeCheckpointSession)
                        await ResumeCheckpointedActivitySessionAsync();
                    lock (streamSync)
                        streamQueueChanged = null;
                    continue;
                }

                var execution = ExecuteAdmittedStreamEventAsync(work!.Event, generation);
                lock (streamSync)
                    streamExecutions.Add(execution);
                _ = execution.ContinueWith(completed =>
                {
                    lock (streamSync)
                    {
                        streamExecutions.Remove(completed);
                        streamInFlight--;
                    }
                    ReleaseAdmittedStreamEvent(work);
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Activity stream execution failed for {BehaviorId}.", BehaviorId);
            lock (streamSync)
            {
                while (streamQueue.TryDequeue(out var pending))
                    ReleaseAdmittedStreamEvent(pending);
                while (streamNextEpochQueue.TryDequeue(out var pending))
                    ReleaseAdmittedStreamEvent(pending);
                streamPumpCompletion = null;
                checkpointReached?.TrySetException(exception);
            }
            completion.TrySetException(exception);
        }
    }

    /// <summary>
    /// Accepts streamed work that a regular transaction handed off at a mixed acceptance. The work is tracked as
    /// live stream execution of the current busy period (so quiescence and checkpoints account for it) and runs
    /// on the activation scheduler with immediate, generation-fenced publication and live-marking ownership.
    /// </summary>
    private bool HostHandedOffActivityWork(Func<Task> work)
    {
        long generation;
        lock (streamSync)
        {
            if (stoppingStreamAdmission || streamFinalizationRequested || streamPumpCompletion == null)
                return false;
            generation = Interlocked.Read(ref streamGeneration);
            streamInFlight++;
        }

        EventHolder? trigger = incomingEvent.Value is OrleansEventHolder incoming ? incoming : null;
        var execution = RunHandedOffActivityWorkAsync(work, generation, trigger);
        lock (streamSync)
        {
            streamExecutions.Add(execution);
            streamQueueChanged?.TrySetResult();
        }
        _ = execution.ContinueWith(completed =>
        {
            lock (streamSync)
            {
                streamExecutions.Remove(completed);
                streamInFlight--;
            }
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        return true;
    }

    private async Task RunHandedOffActivityWorkAsync(Func<Task> work, long generation, EventHolder? trigger)
    {
        await Task.Yield();
        incomingEvent.Value = null;
        ResponseHolder.SetResponses(new Dictionary<object, EventHolder>());
        var previousGeneration = streamExecutionGeneration.Value;
        streamExecutionGeneration.Value = generation;
        try
        {
            await tenantExecutor.ExecuteByTenantAsync(TenantId, async () =>
            {
                using var publications = new Stateflows.Common.Engine.PublicationScope(
                    runtimeOwned: true,
                    dispatchGuard: () => generation == Interlocked.Read(ref streamGeneration),
                    dispatchImmediatelyWhen: () => true);
                using var outbox = new OutboxPublicationScope();
                using var storageScope = GrainStorageBridge.EnterScope(
                    hydrateAsync: () => Task.FromResult(streamLiveContext ?? committedContext ?? new StateflowsContext(BehaviorId)),
                    dehydrateAsync: context =>
                    {
                        if (generation == Interlocked.Read(ref streamGeneration))
                        {
                            lock (streamSync)
                                streamLiveContext = StateflowsJsonConverter.Clone(context);
                        }
                        return Task.CompletedTask;
                    });
                try
                {
                    await work();
                }
                catch (Exception exception)
                {
                    var failures = exception is AggregateException aggregate ? aggregate.InnerExceptions.ToArray() : [exception];
                    foreach (var failure in failures)
                        logger.LogError(failure, "Streamed Activity work handed off by a regular transaction failed for {BehaviorId}.", BehaviorId);
                    if (trigger != null)
                        await PublishActivityStreamFailureAsync(trigger, failures);
                }
                finally
                {
                    await publications.WaitForImmediateAsync();
                }
            });
        }
        finally
        {
            streamExecutionGeneration.Value = previousGeneration;
            ResponseHolder.ClearResponses();
        }
    }

    private async Task ExecuteAdmittedStreamEventAsync(OrleansEventHolder eventHolder, long generation)
    {
        var responses = new Dictionary<object, EventHolder>();
        ResponseHolder.SetResponses(responses);
        var previousGeneration = streamExecutionGeneration.Value;
        streamExecutionGeneration.Value = generation;
        try
        {
            await tenantExecutor.ExecuteByTenantAsync(TenantId, async () =>
            {
                try
                {
                    EventHolder typedEvent = eventHolder;
                    var result = await typedEvent.ExecuteAsync(this);
                    if (result == EventStatus.Failed)
                        logger.LogError("Streamed Activity invocation failed for {BehaviorId}.", BehaviorId);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Streamed Activity invocation failed for {BehaviorId}.", BehaviorId);
                }
            });
        }
        finally
        {
            streamExecutionGeneration.Value = previousGeneration;
            ResponseHolder.ClearResponses();
        }
    }

    public async Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
    {
        var transactionalSource = eventHolder.Headers.Values.OfType<StreamedActivityExecution>().Any(header => header.TransactionalSource);
        // One regular transaction at a time: regular session calls and transactional live deliveries share the gate.
        var checkpointGateHeld = transactionalSource ||
            eventHolder.Headers.Values.Any(header => header is ActivitySessionExecution);
        if (checkpointGateHeld)
            await streamCheckpointGate.WaitAsync();

        try
        {
            var result = EventStatus.Undelivered;
            if (!Processors.TryGetValue(BehaviorId.Type, out var processor) || !interceptor.BeforeExecute(BehaviorId, eventHolder))
            {
                return result;
            }
            
            var exceptions = new List<Exception>();
            StateflowsContext? pendingContext = null;
            using var publications = new Stateflows.Common.Engine.PublicationScope(
                runtimeOwned: true,
                dispatchGuard: () => streamExecutionGeneration.Value is not long generation ||
                    generation == Interlocked.Read(ref streamGeneration),
                dispatchImmediatelyWhen: () => (streamExecutionGeneration.Value.HasValue && !transactionalSource) ||
                    Processors.TryGetValue(BehaviorType.Activity, out var activityProcessor) &&
                    activityProcessor is IActivityStreamSessionProcessor sessionProcessor &&
                    sessionProcessor.IsCurrentActivityInvocationStreamed(BehaviorId));
            using var outbox = new OutboxPublicationScope();

            using var storageScope = GrainStorageBridge.EnterScope(
                hydrateAsync: () =>
                {
                    return Task.FromResult(streamLiveContext ?? committedContext ?? new StateflowsContext(BehaviorId));
                },
                dehydrateAsync: context =>
                {
                    pendingContext = context;
                    if (streamExecutionGeneration.Value is long generation &&
                        generation == Interlocked.Read(ref streamGeneration))
                    {
                        lock (streamSync)
                            streamLiveContext = StateflowsJsonConverter.Clone(context);
                    }
                    return Task.CompletedTask;
                }
            );

            try
            {
                result = await processor.ProcessEventAsync(BehaviorId, eventHolder, exceptions);
                foreach (var exception in exceptions)
                {
                    logger.LogError(exception, "Stateflows behavior {BehaviorId} reported an execution error.", BehaviorId);
                }
                if (streamExecutionGeneration.Value.HasValue &&
                    (result == EventStatus.Failed || exceptions.Count > 0))
                {
                    await PublishActivityStreamFailureAsync(eventHolder, exceptions);
                }
            }
            finally
            {
                try
                {
                    await publications.WaitForImmediateAsync();
                }
                finally
                {
                    var response = eventHolder.GetResponseHolder();
                    if (response != null)
                    {
                        response.SenderId = BehaviorId;
                        response.SentAt = DateTime.Now;
                    }
                    interceptor.AfterExecute(BehaviorId, eventHolder);
                }
            }

            if (streamExecutionGeneration.Value is long activeGeneration)
            {
                if (activeGeneration == Interlocked.Read(ref streamGeneration) && pendingContext != null)
                    streamLiveContext = StateflowsJsonConverter.Clone(pendingContext);
                else if (streamFinalizationRequested && streamFinalizeContext == null && pendingContext != null)
                    streamFinalizeContext = pendingContext;
                if (activeGeneration == Interlocked.Read(ref streamGeneration))
                {
                    if (transactionalSource)
                    {
                        // A transactional delivery commits its deferred publications only when it succeeds; a failed
                        // one discards them (its live effects stay, as for every regular call during streaming).
                        if (result != EventStatus.Failed && exceptions.Count == 0)
                        {
                            await streamAdmissionGate.WaitAsync();
                            try
                            {
                                var staged = outbox.GetPublications();
                                if (staged.Length > 0)
                                    await CommitAsync(CloneCommittedContext(), staged);
                                if (contextState.State.Outbox.Count > 0)
                                    await DrainOutboxAsync();
                                await publications.FlushAsync();
                            }
                            finally
                            {
                                streamAdmissionGate.Release();
                            }
                        }
                    }
                    else
                    {
                        await publications.FlushAsync();
                    }
                }
            }
            else if (eventHolder.Headers.Values.Any(header => header is ActivitySessionExecution))
            {
                await streamAdmissionGate.WaitAsync();
                try
                {
                    var staged = outbox.GetPublications();
                    if (HasActiveStream())
                    {
                        if (pendingContext != null)
                        {
                            lock (streamSync)
                                streamLiveContext = StateflowsJsonConverter.Clone(pendingContext);
                        }

                        if (eventHolder.Headers.Values.OfType<ActivitySessionExecution>().Any(header => header.Checkpoint) &&
                            result != EventStatus.Failed && exceptions.Count == 0)
                        {
                            pendingContext ??= CloneCommittedContext();
                            if (incomingEvent.Value.HasValue && NotificationDeliveryStatuses.IsAcknowledged(result))
                                RecordEventReceipt(pendingContext);
                            await CommitAsync(pendingContext, staged);
                            lock (streamSync)
                                streamLiveContext = StateflowsJsonConverter.Clone(pendingContext);
                        }
                        else if (incomingEvent.Value.HasValue && NotificationDeliveryStatuses.IsAcknowledged(result))
                        {
                            var receiptContext = CloneCommittedContext();
                            RecordEventReceipt(receiptContext);
                            await CommitAsync(receiptContext, staged);
                        }
                        else if (staged.Length > 0 && result != EventStatus.Failed && exceptions.Count == 0)
                        {
                            await CommitAsync(CloneCommittedContext(), staged);
                        }
                    }
                    else
                    {
                        if (Processors.TryGetValue(BehaviorType.Activity, out var activityProcessor) &&
                            activityProcessor is IActivityStreamSessionProcessor sessionProcessor)
                            await sessionProcessor.EndActivitySessionAsync(BehaviorId);

                        if (result != EventStatus.Failed && exceptions.Count == 0)
                        {
                            pendingContext ??= CloneCommittedContext();
                            if (incomingEvent.Value.HasValue && NotificationDeliveryStatuses.IsAcknowledged(result))
                                RecordEventReceipt(pendingContext);
                            if ((incomingEvent.Value.HasValue && NotificationDeliveryStatuses.IsAcknowledged(result)) ||
                                staged.Length > 0 || HasChanged(pendingContext))
                                await CommitAsync(pendingContext, staged);
                        }
                    }

                    if (contextState.State.Outbox.Count > 0)
                        await DrainOutboxAsync();
                    await publications.FlushAsync();
                }
                finally
                {
                    streamAdmissionGate.Release();
                }
            }
            else if (result != EventStatus.Failed && exceptions.Count == 0)
            {
                var staged = outbox.GetPublications();
                if (incomingEvent.Value.HasValue && NotificationDeliveryStatuses.IsAcknowledged(result))
                {
                    pendingContext ??= CloneCommittedContext();
                    RecordEventReceipt(pendingContext);
                    await CommitAsync(pendingContext, staged);
                }
                else if (staged.Length > 0 || (pendingContext != null && HasChanged(pendingContext)))
                {
                    // Executions that change nothing (e.g. status requests) are not persisted.
                    await CommitAsync(pendingContext ?? CloneCommittedContext(), staged);
                }
                if (contextState.State.Outbox.Count > 0)
                    await DrainOutboxAsync();
                await publications.FlushAsync();
            }

            return result;
        }
        catch (Exception e)
        {
            if (streamExecutionGeneration.Value.HasValue)
                await PublishActivityStreamFailureAsync(eventHolder, [e]);

            if (eventHolder.Headers.Values.Any(header => header is ActivitySessionExecution) &&
                !HasActiveStream() &&
                Processors.TryGetValue(BehaviorType.Activity, out var activityProcessor) &&
                activityProcessor is IActivityStreamSessionProcessor sessionProcessor)
            {
                await streamAdmissionGate.WaitAsync();
                try
                {
                    if (!HasActiveStream())
                        await sessionProcessor.EndActivitySessionAsync(BehaviorId);
                }
                catch (Exception cleanupException)
                {
                    logger.LogWarning(cleanupException, "Failed to discard a failed Activity transaction session for {BehaviorId}.", BehaviorId);
                }
                finally
                {
                    streamAdmissionGate.Release();
                }
            }
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            if (checkpointGateHeld)
                streamCheckpointGate.Release();
        }
    }

    private async Task PublishActivityStreamFailureAsync(EventHolder eventHolder, IEnumerable<Exception> exceptions)
    {
        var context = StateflowsJsonConverter.Clone(
            streamLiveContext ?? committedContext ?? new StateflowsContext(BehaviorId));
        context.Id = BehaviorId;
        using var publications = new Stateflows.Common.Engine.PublicationScope(
            runtimeOwned: true,
            dispatchGuard: () => streamExecutionGeneration.Value is not long generation ||
                generation == Interlocked.Read(ref streamGeneration));
        using var trigger = context.PushExecutionTriggerHolder(eventHolder);
        try
        {
            var exceptionTypes = exceptions
                .Select(exception => exception.GetType().FullName ?? exception.GetType().Name)
                .ToArray();
            await subscriber.PublishRangeAsync(
                BehaviorId,
                [
                    new ActivityStreamFailure
                    {
                        BehaviorId = BehaviorId,
                        EventId = eventHolder.Id,
                        EventName = eventHolder.Name,
                        ExceptionTypes = exceptionTypes.Length == 0 ? ["UnhandledActivityStreamFailure"] : exceptionTypes
                    }
                ],
                context);
            await publications.WaitForImmediateAsync();
        }
        catch (Exception notificationException)
        {
            logger.LogError(notificationException, "Failed to publish the streamed Activity failure notification for {BehaviorId}.", BehaviorId);
        }
    }

    private static bool IsDeduplicationDisabled(EventHolder eventHolder)
        => Attribute.IsDefined(eventHolder.PayloadType, typeof(NoDeduplicationAttribute)) ||
            eventHolder.Headers.Values.Any(header => header is NoDeduplication);

    // Compares with the committed context, ignoring LastExecutedAt, which every delivered execution refreshes.
    private bool HasChanged(StateflowsContext context)
    {
        if (!IsLive(context))
        {
            // Persisted as a receipts-only tombstone or nothing at all (see CommitContextAsync).
            return committedContext == null
                ? context.EventReceipts.Count > 0
                : contextState.State?.Tombstone != true ||
                    !committedContext.EventReceipts.Keys.ToHashSet().SetEquals(context.EventReceipts.Keys);
        }

        if (committedContext == null)
            return true;

        return GetFingerprint(context) != GetFingerprint(committedContext);
    }

    // A deleted context, or one that is still not initialized and holds no runtime data, is not a live behavior:
    // it is never indexed and is never persisted as behavior data (e.g. a status probe after Reset must not resurrect it).
    private static bool IsLive(StateflowsContext context)
        => !context.Deleted && !IsPristine(context);

    // Values are not checked: processors fill them with empty structures (such as an empty states tree) on hydration.
    // Event receipts are not checked: they belong to the behavior identity and are kept in a tombstone.
    private static bool IsPristine(StateflowsContext context)
        => context.Status is BehaviorStatus.Unknown or BehaviorStatus.NotInitialized &&
            context.TriggerTime == null &&
            !context.TriggerOnStartup &&
            context.PendingTimeEvents.Count == 0 &&
            context.PendingStartupEvents.Count == 0 &&
            context.Subscriptions.Values.All(names => names.Count == 0) &&
            context.Subscribers.Values.All(ids => ids.Count == 0) &&
            context.GlobalValues.Count == 0 &&
            context.ContextOwnerId == null &&
            context.ContextParentId == null;

    private static string GetFingerprint(StateflowsContext context)
    {
        var lastExecutedAt = context.LastExecutedAt;
        context.LastExecutedAt = default;
        try
        {
            return StateflowsJsonConverter.SerializePolymorphicObject(context);
        }
        finally
        {
            context.LastExecutedAt = lastExecutedAt;
        }
    }

    private StateflowsContext CloneCommittedContext()
    {
        var stored = committedContext;
        var context = stored != null
            ? StateflowsJsonConverter.Clone(stored)
            : new StateflowsContext(BehaviorId);
        context.Deleted = stored?.Deleted ?? false;
        context.Stored = stored?.Stored ?? false;
        return context;
    }

    private void RecordEventReceipt(StateflowsContext context)
    {
        var incoming = incomingEvent.Value!.Value;
        context.EventReceipts[incoming.Id] = incoming.TimeToLive > 0
            ? incoming.SentAt.ToUniversalTime().AddSeconds(incoming.TimeToLive)
            : DateTime.UtcNow.AddHours(1);
    }

    private async Task CommitContextAsync(StateflowsContext context, OutboxPublication[]? publications = null)
    {
        var committed = committedContext;
        var committedState = contextState.State;
        try
        {
            var now = DateTime.UtcNow;
            // Forced resets can replace the entire context; receipts belong to the behavior identity, not its lifecycle.
            foreach (var receipt in committed?.EventReceipts ?? [])
            {
                if (receipt.Value > now)
                    context.EventReceipts.TryAdd(receipt.Key, receipt.Value);
            }
            foreach (var id in context.EventReceipts.Where(p => p.Value <= now).Select(p => p.Key).ToArray())
                context.EventReceipts.Remove(id);

            // Deleted is intentionally not serialized. Persist a metadata-only tombstone, never deleted behavior data.
            var outbox = new List<OutboxPublication>(committedState.Outbox);
            var sequence = committedState.LastPublicationSequence;
            foreach (var publication in publications ?? [])
            {
                publication.Sequence = checked(++sequence);
                outbox.Add(publication);
            }
            var live = !IsStatelessAction() && IsLive(context);
            var tombstone = !live && (context.EventReceipts.Count > 0 || sequence > 0);
            if (tombstone)
            {
                context = new StateflowsContext(BehaviorId) { EventReceipts = context.EventReceipts };
            }
            if (!live && !tombstone)
            {
                await contextState.ClearStateAsync();
                committedContext = null;
            }
            else
            {
                contextState.State = new BehaviorContextState
                {
                    Payload = StateflowsJsonConverter.SerializePolymorphicObject(context),
                    Tombstone = tombstone,
                    Outbox = outbox,
                    LastPublicationSequence = sequence
                };
                await contextState.WriteStateAsync();
                committedContext = context;
            }
        }
        catch (Exception exception)
        {
            contextState.State = committedState;
            commitOutcomeUnknown = true;
            StateflowsOrleansDiagnostics.PersistenceFailed("Stateflows.BehaviorContext", BehaviorId.BehaviorClass);
            logger.LogError(exception, "Failed to persist state of Stateflows behavior {BehaviorId}; the event fails and the activation reloads its state.", BehaviorId);
            // A provider may have committed before reporting failure; reload on the next activation.
            DeactivateOnIdle();
            throw;
        }
    }

    public const string TimeTriggerReminderName = "Stateflows.TimeTrigger";
    public const string OutboxReminderName = "Stateflows.Outbox";

    // A reminder keeps ticking with this period until the schedule changes, so failed or interrupted callbacks are retried.
    private TimeSpan ReminderPeriod
        => reminderOptions.Value.MinimumReminderPeriod > TimeSpan.Zero
            ? reminderOptions.Value.MinimumReminderPeriod
            : TimeSpan.FromMinutes(1);

    private static DateTime? GetScheduledTrigger(StateflowsContext? context)
        => context is { Deleted: false } ? context.TriggerTime : null;

    private async Task CommitAsync(StateflowsContext context, OutboxPublication[]? publications = null)
    {
        var previousTrigger = GetScheduledTrigger(committedContext);
        var trigger = GetScheduledTrigger(context);
        var live = !IsStatelessAction() && IsLive(context);

        // Indexed before commit, so a committed instance is never missing from listings. A failure fails the event.
        if (live && !indexed)
        {
            await AddToIndexAsync();
        }

        // Registered before commit, so a committed time event never lacks its reminder. A failure fails the event.
        if (trigger != null && (!registrationKnown || registeredTrigger != trigger))
        {
            await RegisterReminderAsync(trigger.Value);
        }

        try
        {
            // Persist the wakeup before its obligations: orphan reminders are safe, lost wakeups are not.
            if (publications is { Length: > 0 })
            {
                try
                {
                    await this.RegisterOrUpdateReminder(OutboxReminderName, ReminderPeriod, ReminderPeriod);
                }
                catch
                {
                    StateflowsOrleansDiagnostics.ReminderFailed("outbox-register", BehaviorId.BehaviorClass);
                    throw;
                }
            }
            await CommitContextAsync(context, publications);
        }
        catch
        {
            await RestoreReminderAsync(previousTrigger, trigger);
            throw;
        }

        if (trigger == null && (!registrationKnown ? previousTrigger != null : registeredTrigger != null))
        {
            // Best effort: a reminder left behind finds no schedule on its next tick and unregisters itself.
            await TryUnregisterReminderAsync();
        }

        if (context.Deleted || (!live && indexed))
        {
            // Best effort: a stale entry is pruned when this instance is next deactivated without state.
            await TryRemoveFromIndexAsync();
        }
    }

    private async Task DrainOutboxAsync()
    {
        if (commitOutcomeUnknown)
            return;

        try
        {
            while (contextState.State.Outbox.Count > 0)
            {
                var publication = contextState.State.Outbox[0];
                await GrainFactory.GetGrain<ISubscriptionsGrain>(publication.PublisherKey)
                    .PublishOutboxAsync(this.GetPrimaryKeyString(), publication.Sequence, publication.Notifications);

                var previous = contextState.State;
                contextState.State = new BehaviorContextState
                {
                    Payload = previous.Payload,
                    Tombstone = previous.Tombstone,
                    LastPublicationSequence = previous.LastPublicationSequence,
                    Outbox = previous.Outbox.Skip(1).ToList()
                };
                try
                {
                    await contextState.WriteStateAsync();
                }
                catch
                {
                    contextState.State = previous;
                    commitOutcomeUnknown = true;
                    StateflowsOrleansDiagnostics.PersistenceFailed("Stateflows.BehaviorContext", BehaviorId.BehaviorClass);
                    DeactivateOnIdle();
                    throw;
                }
            }

            var reminder = await this.GetReminder(OutboxReminderName);
            if (reminder != null)
                await this.UnregisterReminder(reminder);
        }
        catch (Exception exception)
        {
            StateflowsOrleansDiagnostics.PublicationFailed("outbox");
            logger.LogError(exception, "Failed to drain notification outbox of {BehaviorId}; committed publications will be retried.", BehaviorId);
        }
    }

    private IInstancesGrain InstancesIndex
        => GrainFactory.GetGrain<IInstancesGrain>(StateflowsInstances.GetKey(TenantId, BehaviorId.BehaviorClass));

    private async Task AddToIndexAsync()
    {
        try
        {
            await InstancesIndex.AddInstanceAsync(BehaviorId.Instance);
            indexed = true;
        }
        catch (Exception exception)
        {
            StateflowsOrleansDiagnostics.IndexFailed("add", BehaviorId.BehaviorClass);
            logger.LogError(exception, "Failed to add Stateflows behavior {BehaviorId} to the instance index; the event fails and can be retried.", BehaviorId);
            throw;
        }
    }

    private async Task TryRemoveFromIndexAsync()
    {
        indexed = false;
        try
        {
            await InstancesIndex.RemoveInstanceAsync(BehaviorId.Instance);
        }
        catch (Exception exception)
        {
            StateflowsOrleansDiagnostics.IndexFailed("remove", BehaviorId.BehaviorClass);
            logger.LogWarning(exception, "Failed to remove Stateflows behavior {BehaviorId} from the instance index; the entry is pruned when the instance is next deactivated without state.", BehaviorId);
        }
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        stoppingStreamAdmission = true;
        if (Processors.TryGetValue(BehaviorType.Activity, out var hostingProcessor) &&
            hostingProcessor is IActivityStreamSessionProcessor hostingSessionProcessor)
        {
            hostingSessionProcessor.SetActivitySessionWorkHost(BehaviorId, null!);
        }
        Interlocked.Increment(ref streamGeneration);
        await CancelAsync();
        Task? streamDrain;
        lock (streamSync)
        {
            while (streamQueue.TryDequeue(out var pending))
                ReleaseAdmittedStreamEvent(pending);
            while (streamNextEpochQueue.TryDequeue(out var pending))
                ReleaseAdmittedStreamEvent(pending);
            streamDrain = streamPumpCompletion?.Task;
        }

        if (streamDrain != null)
        {
            try
            {
                await streamDrain.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _ = DisposeActivitySessionAfterDrainAsync(streamDrain);
            }
            catch (Exception exception)
            {
                logger.LogDebug(exception, "Activity stream work ended during deactivation for {BehaviorId}.", BehaviorId);
            }
        }

        if (streamDrain == null || streamDrain.IsCompleted)
        {
            if (Processors.TryGetValue(BehaviorType.Activity, out var activityProcessor) &&
                activityProcessor is IActivityStreamSessionProcessor sessionProcessor)
            {
                try
                {
                    await sessionProcessor.EndActivitySessionAsync(BehaviorId, persist: false);
                }
                catch (Exception exception)
                {
                    logger.LogDebug(exception, "Failed to dispose the live Activity session during deactivation for {BehaviorId}.", BehaviorId);
                }
            }
        }

        // Lazy pruning: an instance without state must not be listed. Skipped when a commit outcome is unknown
        // (the state may have been persisted) and for stateless actions, which are never indexed.
        // Skipped on silo shutdown to keep shutdown fast; the instance prunes on a later deactivation.
        if ((committedContext == null || contextState.State?.Tombstone == true) &&
            reason.ReasonCode != DeactivationReasonCode.ShuttingDown &&
            !commitOutcomeUnknown &&
            !IsStatelessAction())
        {
            await TryRemoveFromIndexAsync();
        }

        await base.OnDeactivateAsync(reason, cancellationToken);
    }

    private async Task DisposeActivitySessionAfterDrainAsync(Task drain)
    {
        try { await drain; }
        catch (Exception exception) { logger.LogDebug(exception, "Activity stream work ended during deferred deactivation cleanup for {BehaviorId}.", BehaviorId); }
        if (Processors.TryGetValue(BehaviorType.Activity, out var activityProcessor) &&
            activityProcessor is IActivityStreamSessionProcessor sessionProcessor)
        {
            try { await sessionProcessor.EndActivitySessionAsync(BehaviorId, persist: false); }
            catch (Exception exception) { logger.LogDebug(exception, "Failed deferred Activity session cleanup for {BehaviorId}.", BehaviorId); }
        }
    }

    private bool IsStatelessAction()
        => Processors.TryGetValue(BehaviorId.Type, out var processor) &&
           processor is Actions.Engine.Processor actions &&
           !actions.IsStateful(BehaviorId, committedContext?.Version ?? 0);

    private async Task RegisterReminderAsync(DateTime trigger)
    {
        var dueTime = trigger - DateTime.UtcNow;
        registrationKnown = false;
        try
        {
            await this.RegisterOrUpdateReminder(
                TimeTriggerReminderName,
                dueTime > TimeSpan.Zero ? dueTime : TimeSpan.Zero,
                ReminderPeriod);
        }
        catch
        {
            StateflowsOrleansDiagnostics.ReminderFailed("register", BehaviorId.BehaviorClass);
            throw;
        }
        registeredTrigger = trigger;
        registrationKnown = true;
    }

    private async Task TryUnregisterReminderAsync()
    {
        try
        {
            registrationKnown = false;
            var reminder = await this.GetReminder(TimeTriggerReminderName);
            if (reminder != null)
            {
                await this.UnregisterReminder(reminder);
            }

            registeredTrigger = null;
            registrationKnown = true;
        }
        catch (Exception exception)
        {
            StateflowsOrleansDiagnostics.ReminderFailed("unregister", BehaviorId.BehaviorClass);
            logger.LogWarning(exception, "Failed to unregister time trigger reminder for Stateflows behavior {BehaviorId}; it will be removed on its next tick.", BehaviorId);
        }
    }

    // The commit outcome is unknown (the activation is deactivated and reloads state), so keep the earlier of both triggers:
    // an early tick is harmless, a late one would delay a committed time event.
    private async Task RestoreReminderAsync(DateTime? previousTrigger, DateTime? attemptedTrigger)
    {
        if (previousTrigger == null || attemptedTrigger == null || previousTrigger >= attemptedTrigger)
        {
            return;
        }

        try
        {
            await RegisterReminderAsync(previousTrigger.Value);
        }
        catch (Exception exception)
        {
            StateflowsOrleansDiagnostics.ReminderFailed("restore", BehaviorId.BehaviorClass);
            logger.LogError(exception, "Failed to restore time trigger reminder for Stateflows behavior {BehaviorId}; due time events may be delayed until {TriggerTime:O}.", BehaviorId, attemptedTrigger);
        }
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName == OutboxReminderName)
        {
            await DrainOutboxAsync();
            return;
        }

        if (reminderName != TimeTriggerReminderName)
        {
            return;
        }

        if (HasActiveStream())
        {
            await PollActivityTimeEventsAsync(CancellationToken.None);
            return;
        }

        // Each due occurrence is processed against the latest committed state, so an earlier one may clear or reschedule later ones.
        var attempted = new HashSet<Guid>();
        while (true)
        {
            var now = DateTime.UtcNow;
            var timeEvent = GetScheduledTrigger(committedContext) == null
                ? null
                : committedContext!.PendingTimeEvents.Values
                    .Where(e => e.TriggerTime <= now && !attempted.Contains(e.Id))
                    .OrderBy(e => e.TriggerTime)
                    .FirstOrDefault();
            if (timeEvent == null)
            {
                break;
            }

            attempted.Add(timeEvent.Id);
            var holder = timeEvent.ToTypedEventHolder();
            // The occurrence id makes duplicate ticks hit the event receipt instead of executing again.
            holder.Id = timeEvent.Id;
            var result = await ProcessEventAsync(holder, CancellationToken.None);
            if (result.Status is EventStatus.Failed or EventStatus.Overloaded)
            {
                // The reminder stays registered; the next tick retries. Later occurrences wait, preserving order.
                logger.LogWarning("Time event {TimeEvent} of Stateflows behavior {BehaviorId} returned {Status}; it will be retried on the next reminder tick.", Event.GetName(timeEvent.GetType()), BehaviorId, result.Status);
                return;
            }
        }

        // Commits keep the reminder in sync; only early or stale ticks reach this point with a mismatch.
        var trigger = GetScheduledTrigger(committedContext);
        if (trigger == null)
        {
            if (!registrationKnown || registeredTrigger != null)
            {
                await TryUnregisterReminderAsync();
            }
        }
        else if (!registrationKnown || registeredTrigger != trigger)
        {
            await RegisterReminderAsync(trigger.Value);
        }
    }
}