using Orleans.Concurrency;

namespace Stateflows.Interfaces;

/// <summary>
/// Delivery worker for a single publisher/subscriber pair, keyed by <see cref="StateflowsGrainKeys.ForDelivery"/>.
/// </summary>
[Alias("Stateflows.Interfaces.INotificationDeliveryGrain")]
internal interface INotificationDeliveryGrain : IGrainWithStringKey
{
    /// <summary>
    /// Requests draining of pending deliveries and completes when the (shared) drain finishes; concurrent requests
    /// join the running drain. Never completes with an exception. Callers inside grain calls (in particular the
    /// subscriptions grain) must NOT await it; signal it in a supervised background task instead.
    /// </summary>
    [AlwaysInterleave]
    [Alias("DrainAsync")]
    Task DrainAsync();
}
