using Orleans.Concurrency;

namespace Stateflows.Interfaces;

[Alias("Stateflows.Interfaces.IBehaviorOutboxGrain")]
internal interface IBehaviorOutboxGrain : IGrainWithStringKey
{
    [Alias("DrainAsync")]
    [AlwaysInterleave]
    [Transaction(TransactionOption.Suppress)]
    Task DrainAsync();
}
