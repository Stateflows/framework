using Microsoft.Extensions.Logging;
using Stateflows.Interfaces;

namespace Stateflows;

/// <summary>
/// Delivers pending notifications of a single publisher/subscriber pair in order, outside of the subscriptions grain,
/// so that the subscriptions grain never awaits behavior processing.
/// </summary>
internal class StateflowsNotificationDelivery(
    IGrainFactory grainFactory,
    ILogger<StateflowsNotificationDelivery> logger
) : Grain, INotificationDeliveryGrain
{
    internal const int BatchSize = 50;
    internal const int MaxBatchesPerTurn = 20;

    private string publisherGrainKey = string.Empty;
    private string subscriberGrainKey = string.Empty;
    private ISubscriptionsGrain? subscriptions;
    private IBehaviorGrain? subscriber;
    private Task? draining;
    private bool drainRequested;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        (publisherGrainKey, subscriberGrainKey) = StateflowsGrainKeys.ParseDelivery(this.GetPrimaryKeyString());
        subscriptions = grainFactory.GetGrain<ISubscriptionsGrain>(publisherGrainKey);
        subscriber = grainFactory.GetGrain<IBehaviorGrain>(subscriberGrainKey);
        return base.OnActivateAsync(cancellationToken);
    }

    public Task DrainAsync()
    {
        drainRequested = true;
        // Returning the shared drain task keeps a request in flight for the whole drain, so Orleans does not
        // deactivate the worker while it processes; concurrent (interleaved) requests join the same drain
        draining ??= RunAsync();
        return draining;
    }

    private async Task RunAsync()
    {
        // Ensures 'draining' is assigned before the loop runs and that it runs on the activation scheduler
        await Task.Yield();
        try
        {
            while (drainRequested)
            {
                drainRequested = false;
                if (!await DrainBatchesAsync())
                {
                    // Failed deliveries are retried by the publisher's reminder or the next publication; no busy retry
                    drainRequested = false;
                    break;
                }

                await Task.Yield();
            }
        }
        catch (Exception exception)
        {
            drainRequested = false;
            logger.LogError(exception,
                "Notification delivery from {Publisher} to {Subscriber} failed; it will be retried.",
                publisherGrainKey, subscriberGrainKey);
        }
        finally
        {
            draining = null;
        }
    }

    /// <returns><c>false</c> when a delivery failed and draining should stop until next retry.</returns>
    private async Task<bool> DrainBatchesAsync()
    {
        for (var batchIndex = 0; batchIndex < MaxBatchesPerTurn; batchIndex++)
        {
            var batch = await subscriptions!.GetPendingDeliveriesAsync(subscriberGrainKey, BatchSize);
            if (batch.Length == 0)
            {
                return true;
            }

            var acknowledged = new List<Guid>(batch.Length);
            var failed = false;
            foreach (var delivery in batch)
            {
                if (await TryDeliverAsync(delivery))
                {
                    acknowledged.Add(delivery.DeliveryId);
                }
                else
                {
                    failed = true;
                    break;
                }
            }

            if (acknowledged.Count > 0)
            {
                await subscriptions.ConfirmDeliveriesAsync(subscriberGrainKey, acknowledged.ToArray());
            }

            if (failed)
            {
                return false;
            }

            if (batch.Length < BatchSize)
            {
                return true;
            }

            await Task.Yield();
        }

        // Bounded turn exhausted while more deliveries may remain; continue after yielding
        drainRequested = true;
        return true;
    }

    private async Task<bool> TryDeliverAsync(NotificationDelivery delivery)
    {
        // Re-check TTL right before dispatch: earlier deliveries in the batch may have taken long enough for this one to expire
        if (new StoredNotification { Notification = delivery.Notification }.IsExpired(DateTime.UtcNow))
        {
            logger.LogDebug(
                "Notification {NotificationName} ({NotificationId}, delivery {DeliveryId}) from {Publisher} to {Subscriber} expired before dispatch; dropping.",
                delivery.Notification.Name, delivery.Notification.Id, delivery.DeliveryId, publisherGrainKey, subscriberGrainKey);
            return true;
        }

        try
        {
            // Authoritative re-check right before dispatch: the subscriber may have unsubscribed (cancelling pending
            // deliveries) or the notification may have expired while earlier deliveries of this batch were processed
            if (!await subscriptions!.IsDeliveryPendingAsync(subscriberGrainKey, delivery.DeliveryId))
            {
                logger.LogDebug(
                    "Delivery {DeliveryId} of notification {NotificationName} ({NotificationId}) from {Publisher} to {Subscriber} is no longer pending; skipping.",
                    delivery.DeliveryId, delivery.Notification.Name, delivery.Notification.Id, publisherGrainKey, subscriberGrainKey);
                return true;
            }

            // No application-level timeout: processing may legitimately be long; Orleans' configured response timeout applies
            var result = await subscriber!.ProcessEventAsync(delivery.Notification, CancellationToken.None);

            if (NotificationDeliveryStatuses.IsAcknowledged(result.Status))
            {
                return true;
            }

            StateflowsOrleansDiagnostics.PublicationFailed(result.Status == Common.EventStatus.Overloaded ? "delivery.overloaded" : "delivery.status");
            logger.LogWarning(
                "Notification {NotificationName} ({NotificationId}, delivery {DeliveryId}) from {Publisher} to {Subscriber} returned {Status}; it will be retried.",
                delivery.Notification.Name, delivery.Notification.Id, delivery.DeliveryId, publisherGrainKey, subscriberGrainKey, result.Status);
        }
        catch (TimeoutException exception)
        {
            StateflowsOrleansDiagnostics.PublicationFailed("delivery.timeout");
            logger.LogWarning(exception,
                "Notification {NotificationName} ({NotificationId}, delivery {DeliveryId}) from {Publisher} to {Subscriber} timed out (Orleans response timeout); it will be retried.",
                delivery.Notification.Name, delivery.Notification.Id, delivery.DeliveryId, publisherGrainKey, subscriberGrainKey);
        }
        catch (Exception exception)
        {
            StateflowsOrleansDiagnostics.PublicationFailed("delivery.exception");
            logger.LogError(exception,
                "Notification {NotificationName} ({NotificationId}, delivery {DeliveryId}) from {Publisher} to {Subscriber} failed; it will be retried.",
                delivery.Notification.Name, delivery.Notification.Id, delivery.DeliveryId, publisherGrainKey, subscriberGrainKey);
        }

        return false;
    }
}
