using Stateflows.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Engine.Interfaces;
using Stateflows.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

namespace Stateflows;

[BehaviorPlacementFilter]
internal class StateflowsBehavior( 
    [StateflowsPersistentState(StateflowsStateNames.BehaviorContext)]
    IPersistentState<BehaviorContextState> contextState,
    IStateflowsInterceptor interceptor,
    IEnumerable<IStateflowsValidator> validators,
    IEnumerable<IEventProcessor> processors,
    IStateflowsTenantExecutor tenantExecutor,
    IStateflowsResources resources,
    LocalBehaviorCatalog catalog,
    IOptions<ReminderOptions> reminderOptions,
    ILogger<StateflowsBehavior> logger
) : Grain, IBehaviorGrain, IStateflowsExecutor
{
    private IGrainTimer? Timer;
    private string? TenantId;
    private BehaviorId BehaviorId;
    private OrleansEventHolder? incomingEvent;
    private StateflowsContext? committedContext;
    // Trigger time of the reminder this activation knows to be registered; unknown until the activation registers one.
    private DateTime? registeredTrigger;
    private bool registrationKnown;
    // Whether this instance is known to be in the instance index.
    private bool indexed;
    // Set when a state write failed: the provider may still have persisted it.
    private bool commitOutcomeUnknown;
    private readonly Dictionary<string, IEventProcessor> Processors = processors
        .SelectMany(p => p.BehaviorTypes.Select(type => new { Type = type, Processor = p }))
        .ToDictionary(p => p.Type, p => p.Processor);
    private readonly IStateflowsValidator[] Validators = validators.ToArray();
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
            
            // Timer = this.RegisterGrainTimer(
            //     static (state, ct) => state.CancelAsync(),
            //     this,
            //     dueTime: TimeSpan.Zero,
            //     period: TimeSpan.FromMinutes(1)
            // );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        await base.OnActivateAsync(cancellationToken);
        if (contextState.State.Outbox.Count > 0)
            await DrainOutboxAsync();
    }

    public async Task CancelAsync()
    {
        if (Common.Context.Classes.BaseContext.Instances.TryGetValue(BehaviorId, out var contexts))
        {
            foreach (var context in contexts)
            {
                await context.CancellationTokenSource.CancelAsync();
            }
        }
    }

    public async Task<OrleansRequestResult> ProcessEventAsync(
        OrleansEventHolder orleansEventHolder, CancellationToken cancellationToken)
    {
        if (commitOutcomeUnknown)
            throw new InvalidOperationException("The previous state write has an unknown outcome; retry on a fresh activation.");

        if (!resources.ResourcesByBehaviorClass.TryGetValue(BehaviorId.BehaviorClass, out var resource))
        {
            throw new InvalidOperationException($"No resource is registered for Stateflows behavior '{BehaviorId.BehaviorClass}'.");
        }

        var admission = await resource.TryAcquireAsync(cancellationToken);
        if (!admission.IsAdmitted)
        {
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

                incomingEvent = orleansEventHolder;
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
                    else if (incomingEvent.HasValue)
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
            incomingEvent = null;
            ResponseHolder.ClearResponses();
        }
    }

    public async Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
    {
        try
        {
            var result = EventStatus.Undelivered;
            if (!Processors.TryGetValue(BehaviorId.Type, out var processor) || !interceptor.BeforeExecute(BehaviorId, eventHolder))
            {
                return result;
            }
            
            var exceptions = new List<Exception>();
            StateflowsContext? pendingContext = null;
            using var publications = new Stateflows.Common.Engine.PublicationScope(runtimeOwned: true);
            using var outbox = new OutboxPublicationScope();

            using var storageScope = GrainStorageBridge.EnterScope(
                hydrateAsync: () =>
                {
                    return Task.FromResult(committedContext ?? new StateflowsContext(BehaviorId));
                },
                dehydrateAsync: context =>
                {
                    pendingContext = context;
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

            if (result != EventStatus.Failed && exceptions.Count == 0)
            {
                var staged = outbox.GetPublications();
                if (incomingEvent.HasValue && NotificationDeliveryStatuses.IsAcknowledged(result))
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
            Console.WriteLine(e);
            throw;
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
        var incoming = incomingEvent!.Value;
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