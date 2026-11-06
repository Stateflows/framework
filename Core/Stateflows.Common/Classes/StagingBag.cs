using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Common.Classes;

/// <summary>Owns in-memory operations staged by one behavior execution.</summary>
/// <remarks>Runtime infrastructure. Target changes are not rolled back if a later operation or source commit fails.</remarks>
public sealed class StagingBag : IDisposable
{
    private static readonly AsyncLocal<StagingBag> current = new();
    private readonly StagingBag previous;
    private readonly Func<bool> isStreamed;
    private readonly Queue<Func<Task>> beforeCommit = new();
    private readonly Queue<Action> afterCommit = new();
    private readonly List<IDisposable> resources = new();
    private bool closed;

    /// <summary>Gets whether an execution already owns a staging bag.</summary>
    public static bool IsActive => current.Value != null;

    /// <summary>Starts an execution-local staging bag.</summary>
    public StagingBag(Func<bool> isStreamed = null)
    {
        this.isStreamed = isStreamed;
        previous = current.Value;
        current.Value = this;
    }

    /// <summary>Requires an open transactional staging bag.</summary>
    public static StagingBag RequireCurrent()
    {
        var bag = current.Value ?? throw new InvalidOperationException("Staging requires an active behavior transaction.");
        if (bag.closed)
            throw new InvalidOperationException("This transaction's staging bag is closed.");
        if (bag.isStreamed?.Invoke() == true)
            throw new InvalidOperationException("Staging is not supported during streamed execution.");
        return bag;
    }

    /// <summary>Registers a pre-commit operation and its owned resource.</summary>
    public void AddBeforeCommit(Func<Task> operation, IDisposable resource)
    {
        lock (beforeCommit)
        {
            beforeCommit.Enqueue(operation);
            resources.Add(resource);
        }
    }

    /// <summary>Registers a post-commit send and its owned resource.</summary>
    public void AddAfterCommit(Action operation, IDisposable resource)
    {
        lock (beforeCommit)
        {
            afterCommit.Enqueue(operation);
            resources.Add(resource);
        }
    }

    /// <summary>Executes staged entity operations in registration order; the first failure stops dispatch.</summary>
    public async Task ExecuteBeforeCommitAsync()
    {
        while (true)
        {
            Func<Task> operation;
            lock (beforeCommit)
            {
                if (beforeCommit.Count == 0)
                    return;
                operation = beforeCommit.Dequeue();
            }
            await operation();
        }
    }

    /// <summary>Queues staged sends after a successful source commit without awaiting target processing.</summary>
    public void DispatchAfterCommit()
    {
        closed = true;
        while (afterCommit.Count > 0)
            afterCommit.Dequeue()();
    }

    /// <summary>Discards remaining operations and releases handles without dispatching them.</summary>
    public void Dispose()
    {
        closed = true;
        current.Value = previous;
        foreach (var resource in resources)
            resource.Dispose();
        beforeCommit.Clear();
        afterCommit.Clear();
        resources.Clear();
    }
}
