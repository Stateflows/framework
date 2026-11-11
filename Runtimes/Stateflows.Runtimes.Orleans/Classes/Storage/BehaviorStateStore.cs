using Orleans.Transactions;
using Orleans.Transactions.Abstractions;

namespace Stateflows.Storage;

internal sealed class BehaviorStateStore(
    ITransactionalState<BehaviorContextState> transactionalState,
    ITransactionClient transactions)
{
    public Func<IDisposable>? IndependentTransactionScope { get; set; }
    public System.Action? IndependentTransactionStarted { get; set; }
    public System.Action? IndependentTransactionCompleted { get; set; }
    public BehaviorContextState State { get; set; } = new();
    public bool RecordExists => !string.IsNullOrEmpty(State.Payload);

    public async Task ReadStateAsync()
        => State = await ReadSnapshotAsync();

    public async Task<BehaviorContextState> ReadSnapshotAsync()
    {
        BehaviorContextState? snapshot = null;
        if (TransactionContext.GetTransactionInfo() == null)
            await RunIndependentAsync(async () =>
                snapshot = await transactionalState.PerformRead(state => state));
        else
            snapshot = await transactionalState.PerformRead(state => state);
        return snapshot ?? throw new InvalidOperationException("The transaction completed without a behavior state snapshot.");
    }

    public async Task WriteStateAsync()
    {
        var snapshot = State;
        if (TransactionContext.GetTransactionInfo() == null)
            await RunIndependentAsync(() => WriteAsync(snapshot));
        else
            await WriteAsync(snapshot);
    }

    private Task WriteAsync(BehaviorContextState snapshot)
        => transactionalState.PerformUpdate(state =>
        {
            state.Payload = snapshot.Payload;
            state.Tombstone = snapshot.Tombstone;
            state.Outbox = snapshot.Outbox;
            state.LastPublicationSequence = snapshot.LastPublicationSequence;
            return true;
        });

    public Task ClearStateAsync()
    {
        State = new();
        return WriteStateAsync();
    }

    public Task RemovePublicationAsync(long sequence)
        => RunIndependentAsync(async () =>
        {
            await transactionalState.PerformUpdate(state =>
            {
                state.Outbox.RemoveAll(item => item.Sequence == sequence);
                return true;
            });
        });

    public async Task RunIndependentAsync(Func<Task> operation)
    {
        using var scope = IndependentTransactionScope?.Invoke();
        IndependentTransactionStarted?.Invoke();
        try
        {
            await transactions.RunTransaction(TransactionOption.Create, operation);
        }
        finally
        {
            IndependentTransactionCompleted?.Invoke();
        }
    }
}
