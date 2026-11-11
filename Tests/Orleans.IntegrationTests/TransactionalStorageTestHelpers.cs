using Orleans.Transactions;
using Stateflows;

namespace Orleans.IntegrationTests;

internal static class TransactionalStorageTestHelpers
{
    public static BehaviorContextState? GetCommittedContextState(object? state)
        => state switch
        {
            BehaviorContextState context => context,
            TransactionalStateRecord<BehaviorContextState> record => record.CommittedState,
            _ => null,
        };

    public static long GetLatestSequenceId(TransactionalStateRecord<BehaviorContextState>? record)
    {
        if (record == null)
            return 0;
        var latest = record.CommittedSequenceId;
        if (record.PendingStates != null)
        {
            foreach (var pending in record.PendingStates)
                latest = Math.Max(latest, pending.SequenceId);
        }
        return latest;
    }

    // Commit failures have an unknown durable outcome, so the runtime must surface the Orleans transaction exception
    // instead of reporting a definite Failed status.
    public static async Task<OrleansTransactionException> AssertTransactionCommitFailureAsync(Func<Task> action, string injectedMessage)
    {
        try
        {
            await action();
        }
        catch (OrleansTransactionException exception)
        {
            Exception? current = exception;
            var found = false;
            for (; current != null && !found; current = current.InnerException)
                found = current.Message.Contains(injectedMessage, StringComparison.Ordinal);
            Assert.IsTrue(found || exception.ToString().Contains(injectedMessage, StringComparison.Ordinal),
                $"The transaction failure must be caused by the injected storage fault: {exception}");
            return exception;
        }

        Assert.Fail("A failed transactional commit must propagate its Orleans transaction exception.");
        return null!;
    }

    // Infrastructure prerequisites (index add, time/outbox reminder registration) must surface distinctly instead of
    // looking like an application Failed result or transaction commit uncertainty.
    public static async Task<StateflowsInfrastructureException> AssertInfrastructureFailureAsync(Func<Task> action, string injectedMessage)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            Assert.IsInstanceOfType<StateflowsInfrastructureException>(exception,
                $"Expected StateflowsInfrastructureException but got: {exception}");
            var infrastructure = (StateflowsInfrastructureException)exception;
            Assert.IsNotNull(infrastructure.InnerException, "StateflowsInfrastructureException must preserve its inner cause.");
            Assert.IsTrue(infrastructure.InnerException.ToString().Contains(injectedMessage, StringComparison.Ordinal),
                $"The infrastructure failure must be caused by the injected fault: {infrastructure}");
            return infrastructure;
        }

        Assert.Fail("An infrastructure prerequisite failure must throw StateflowsInfrastructureException, not return a status.");
        return null!;
    }

    public static IEnumerable<BehaviorContextState> GetBehaviorContextStates(object? state)
    {
        if (state is BehaviorContextState context)
        {
            yield return context;
            yield break;
        }

        if (state is not TransactionalStateRecord<BehaviorContextState> record)
        {
            yield break;
        }

        if (record.CommittedState is { } committed)
        {
            yield return committed;
        }

        if (record.PendingStates != null)
        {
            foreach (var pending in record.PendingStates)
            {
                if (pending.State is { } pendingState)
                {
                    yield return pendingState;
                }
            }
        }
    }
}
