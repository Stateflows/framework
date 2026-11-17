using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Interfaces;

namespace Stateflows;

internal sealed class StateflowsBehaviorOutbox(
    IGrainFactory grains, TransactionWorkTracker transactions, ILogger<StateflowsBehaviorOutbox> logger,
    IServiceProvider services)
    : Grain, IBehaviorOutboxGrain
{
    private Task? draining;
    private bool requested;

    public Task DrainAsync()
    {
        requested = true;
        return draining ??= RunAsync();
    }

    private async Task RunAsync()
    {
        await Task.Yield();
        try
        {
            using var lifetime = transactions.TryEnter(allowDuringShutdown: true)
                ?? throw new Orleans.Runtime.SiloUnavailableException("The silo is stopping; outbox delivery will be retried.");
            var sourceKey = this.GetPrimaryKeyString();
            var source = grains.GetGrain<IBehaviorGrain>(sourceKey);
            while (requested)
            {
                requested = false;
                for (var index = 0; index < 50; index++)
                {
                    var publication = await source.GetNextOutboxPublicationAsync();
                    if (publication == null)
                        return;

                    if (publication.DestinationKey is { } destination)
                    {
                        var envelope = publication.Notifications.Single();
                        if (!new StoredNotification { Notification = envelope }.IsExpired(DateTime.UtcNow))
                        {
                            var observer = services.GetService<IBehaviorDispatchObserver>();
                            Task<OrleansRequestResult>? dispatch = null;
                            void Send() => dispatch = grains.GetGrain<IBehaviorGrain>(destination)
                                .ProcessEventAsync(envelope, CancellationToken.None);
                            if (observer == null)
                                Send();
                            else
                                observer.DispatchCommitted(
                                    StateflowsGrainKeys.ParseBehavior(sourceKey).BehaviorId,
                                    StateflowsGrainKeys.ParseBehavior(destination).BehaviorId,
                                    envelope, services, Send);
                            var result = await dispatch!;
                            if (!NotificationDeliveryStatuses.IsAcknowledged(result.Status))
                            {
                                logger.LogWarning("Staged send {EventId} from {Source} to {Destination} returned {Status}; it remains pending.",
                                    envelope.Id, sourceKey, destination, result.Status);
                                return;
                            }
                        }
                    }
                    else
                    {
                        await grains.GetGrain<ISubscriptionsGrain>(publication.PublisherKey)
                            .PublishOutboxAsync(sourceKey, publication.Sequence, publication.Notifications);
                    }
                    await source.ConfirmOutboxPublicationAsync(publication.Sequence);
                    requested = true;
                }
                await Task.Yield();
            }
        }
        catch (Exception exception)
        {
            StateflowsOrleansDiagnostics.PublicationFailed("outbox");
            logger.LogError(exception, "Committed outbox of {BehaviorKey} could not be delivered; its reminder will retry.", this.GetPrimaryKeyString());
        }
        finally
        {
            draining = null;
        }
    }
}
