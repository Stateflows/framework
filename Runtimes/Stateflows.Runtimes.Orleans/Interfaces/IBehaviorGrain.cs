namespace Stateflows.Interfaces;

using Orleans.Concurrency;

[Alias("Stateflows.Interfaces.IBehaviorGrain")]
internal interface IBehaviorGrain : IGrainWithStringKey, IRemindable
{
    [Alias("GetNextOutboxPublicationAsync")]
    [AlwaysInterleave]
    [Transaction(TransactionOption.Suppress)]
    Task<OutboxPublication?> GetNextOutboxPublicationAsync();

    [Alias("ConfirmOutboxPublicationAsync")]
    [AlwaysInterleave]
    [Transaction(TransactionOption.Suppress)]
    Task ConfirmOutboxPublicationAsync(long sequence);

    [Alias("ProcessEventAsync")]
    [Transaction(TransactionOption.Suppress)]
    Task<OrleansRequestResult> ProcessEventAsync(OrleansEventHolder eventHolder, CancellationToken cancellationToken);

    [Alias("ProcessJoinedEventAsync")]
    [Transaction(TransactionOption.Supported)]
    Task<OrleansRequestResult> ProcessJoinedEventAsync(OrleansEventHolder eventHolder, CancellationToken cancellationToken);

    [Alias("StreamEventAsync")]
    [AlwaysInterleave]
    [Transaction(TransactionOption.Suppress)]
    Task<OrleansRequestResult> StreamEventAsync(OrleansEventHolder eventHolder, CancellationToken cancellationToken);
}