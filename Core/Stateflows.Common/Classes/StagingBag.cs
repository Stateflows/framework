using System;
using System.Collections.Generic;
using System.Threading;

namespace Stateflows.Common.Classes;

/// <summary>Owns staged operations and the optional durable send hook of one behavior execution.</summary>
/// <remarks>Runtime infrastructure for post-commit sends.</remarks>
public sealed class StagingBag : IDisposable
{
    private static readonly AsyncLocal<StagingBag> current = new();
    private readonly StagingBag previous;
    private readonly Func<bool> isStreamed;
    private readonly Action<BehaviorId, EventHolder> stageDurably;
    private readonly Queue<Action> afterCommit = new();
    private readonly object sync = new();
    private readonly List<IDisposable> resources = new();
    private bool closed;

    /// <summary>Gets whether an execution already owns a staging bag.</summary>
    public static bool IsActive => current.Value != null;

    /// <summary>Starts an execution-local staging bag.</summary>
    public StagingBag(Func<bool> isStreamed = null, Action<BehaviorId, EventHolder> stageDurably = null)
    {
        this.isStreamed = isStreamed;
        this.stageDurably = stageDurably;
        previous = current.Value;
        current.Value = this;
    }

    /// <summary>Records a send through the runtime's durable staging mechanism when available.</summary>
    public bool TryStageDurably(BehaviorId destination, EventHolder envelope)
    {
        if (stageDurably == null)
            return false;
        stageDurably(destination, envelope);
        return true;
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

    /// <summary>Registers a post-commit send and its owned resource.</summary>
    public void AddAfterCommit(Action operation, IDisposable resource)
    {
        lock (sync)
        {
            afterCommit.Enqueue(operation);
            resources.Add(resource);
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
        afterCommit.Clear();
        resources.Clear();
    }
}
