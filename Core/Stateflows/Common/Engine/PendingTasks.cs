using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Common.Engine;

internal sealed class PendingTasks
{
    private readonly object sync = new();
    private readonly List<Task> tasks = [];
    private readonly HashSet<Task> failedTasks = [];
    private readonly List<Exception> exceptions = [];

    public Exception[] Exceptions
    {
        get
        {
            lock (sync)
                return exceptions.ToArray();
        }
    }

    public void Add(Task task)
    {
        ArgumentNullException.ThrowIfNull(task);
        lock (sync)
            tasks.Add(task);
    }

    // Keep in-flight tasks visible to concurrent drains from activity node scopes.
    // Failures are reported by the execution boundary, not by scope cleanup.
    public async Task DrainAsync()
    {
        var count = 0;
        while (true)
        {
            Task[] batch;
            lock (sync)
            {
                batch = tasks.Skip(count).ToArray();
                count = tasks.Count;
            }

            if (batch.Length == 0)
                return;

            var completion = Task.WhenAll(batch);
            try
            {
                await completion;
            }
            catch (Exception)
            {
                lock (sync)
                {
                    foreach (var task in batch.Where(task => task.IsFaulted || task.IsCanceled))
                    {
                        if (!failedTasks.Add(task))
                            continue;

                        var failures = task.Exception?.InnerExceptions.ToArray() ?? [new TaskCanceledException(task)];
                        foreach (var failure in failures)
                        {
                            if (!exceptions.Any(existing => ReferenceEquals(existing, failure)))
                                exceptions.Add(failure);
                        }
                    }
                }
            }
        }
    }

    public void ThrowIfFailed()
    {
        var failure = Exceptions.FirstOrDefault();
        if (failure != null)
            ExceptionDispatchInfo.Capture(failure).Throw();
    }
}

internal sealed class PendingTasksScope : IDisposable
{
    private static readonly AsyncLocal<PendingTasks?> current = new();
    private readonly PendingTasks? previous = current.Value;

    public PendingTasks Tasks { get; } = new();

    public static PendingTasks? Current => current.Value;

    public static PendingTasks RequireCurrent()
        => Current ?? throw new InvalidOperationException("Pending tasks require an active behavior execution.");

    public PendingTasksScope()
        => current.Value = Tasks;

    public void Dispose()
        => current.Value = previous;
}
