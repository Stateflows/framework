using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using Stateflows.Common.Classes;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Common.Engine.Interfaces;

namespace Stateflows.Common.Registration.Builders;

internal class Resource : IStateflowsResource
{
    public string Name { get; }

    private readonly int? configuredMaxConcurrentBehaviorExecutions;

    public Resource(string name, int? maxConcurrentBehaviorExecutions = null, int maxQueuedBehaviorExecutions = -1, TimeSpan? maxQueueWaitTime = null)
    {
        Name = name;
        configuredMaxConcurrentBehaviorExecutions = maxConcurrentBehaviorExecutions;
        MaxQueuedBehaviorExecutions = maxQueuedBehaviorExecutions < 0 ? -1 : maxQueuedBehaviorExecutions;
        MaxQueueWaitTime = maxQueueWaitTime;

        if (maxConcurrentBehaviorExecutions == null) return;
        
        MaxConcurrentBehaviorExecutions = (maxConcurrentBehaviorExecutions ?? 0) > 0
            ? maxConcurrentBehaviorExecutions!.Value
            : Environment.ProcessorCount;

        ConcurrencySemaphore = new SemaphoreSlim(MaxConcurrentBehaviorExecutions, MaxConcurrentBehaviorExecutions);
    }

    /// <summary>Creates an independent resource (own permits and counters) with the same configuration.</summary>
    public Resource CreateInstance()
        => new(Name, configuredMaxConcurrentBehaviorExecutions, MaxQueuedBehaviorExecutions, MaxQueueWaitTime);
    
    private int behaviorExecutionCounter = 0;
    private int waitingExecutionCounter = 0;
    
    private Channel<ExecutionToken> EventChannel { get; } = Channel.CreateUnbounded<ExecutionToken>();

    private readonly SemaphoreSlim? ConcurrencySemaphore;

    private readonly CancellationTokenSource shutdownSource = new();

    public async Task WriteAsync(ExecutionToken executionToken, CancellationToken cancellationToken = default)
    {
        await EventChannel.Writer.WriteAsync(executionToken, cancellationToken);
    }

    public async Task<ExecutionToken> ReadAsync(CancellationToken cancellationToken = default)
    {
        var executionToken = await EventChannel.Reader.ReadAsync(cancellationToken);
        
        await EnterAsync(cancellationToken);

        return executionToken;
    }

    public Task ReleaseAsync(CancellationToken cancellationToken = default)
    {
        Interlocked.Decrement(ref behaviorExecutionCounter);
        ConcurrencySemaphore?.Release();

        return Task.CompletedTask;
    }

    public async ValueTask<IDisposable?> AcquireAsync(CancellationToken? cancellationToken)
    {
        await EnterAsync(cancellationToken ?? CancellationToken.None);
        return new Releaser(this);
    }

    public async ValueTask<ResourceAdmission> TryAcquireAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (shutdownSource.IsCancellationRequested)
        {
            return ResourceAdmission.Rejected(ResourceRejectionReason.ShuttingDown);
        }

        if (ConcurrencySemaphore == null || ConcurrencySemaphore.Wait(0))
        {
            Interlocked.Increment(ref behaviorExecutionCounter);
            return ResourceAdmission.Admitted(new Releaser(this));
        }

        var waiting = Interlocked.Increment(ref waitingExecutionCounter);
        try
        {
            if (MaxQueuedBehaviorExecutions >= 0 && waiting > MaxQueuedBehaviorExecutions)
            {
                return ResourceAdmission.Rejected(ResourceRejectionReason.QueueFull);
            }

            using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, shutdownSource.Token);
            bool acquired;
            try
            {
                acquired = await ConcurrencySemaphore
                    .WaitAsync(MaxQueueWaitTime ?? Timeout.InfiniteTimeSpan, linkedSource.Token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (shutdownSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                return ResourceAdmission.Rejected(ResourceRejectionReason.ShuttingDown);
            }

            if (!acquired)
            {
                return ResourceAdmission.Rejected(ResourceRejectionReason.WaitTimeout);
            }

            Interlocked.Increment(ref behaviorExecutionCounter);
            return ResourceAdmission.Admitted(new Releaser(this));
        }
        finally
        {
            Interlocked.Decrement(ref waitingExecutionCounter);
        }
    }

    public void BeginShutdown()
    {
        try
        {
            shutdownSource.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private async Task EnterAsync(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref waitingExecutionCounter);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (ConcurrencySemaphore != null)
            {
                await ConcurrencySemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            Interlocked.Increment(ref behaviorExecutionCounter);
        }
        finally
        {
            Interlocked.Decrement(ref waitingExecutionCounter);
        }
    }

    private sealed class Releaser(Resource resource) : IDisposable
    {
        private Resource? resource = resource;
        public void Dispose()
            => Interlocked.Exchange(ref resource, null)?.ReleaseAsync();
    }

    public int MaxConcurrentBehaviorExecutions { get; } = -1;

    public int MaxQueuedBehaviorExecutions { get; }

    public TimeSpan? MaxQueueWaitTime { get; }

    [Obsolete("Use WaitingExecutionsCount; under Orleans this value does not include grain mailbox depth.")]
    public int EventQueueLength => EventChannel.Reader.Count + System.Threading.Volatile.Read(ref waitingExecutionCounter);

    public int WaitingExecutionsCount => System.Threading.Volatile.Read(ref waitingExecutionCounter);
    
    public int BehaviorExecutionsCount => System.Threading.Volatile.Read(ref behaviorExecutionCounter);
}

internal class ResourceBuilder(string resourceName) : IResourceBuilder
{
    private int? MaxConcurrentBehaviorExecutions = null;
    private int MaxQueuedBehaviorExecutions = -1;
    private TimeSpan? MaxQueueWaitTime = null;
    
    public IResourceBuilder SetMaxConcurrentBehaviorExecutions(int maxConcurrentBehaviorExecutions)
    {
        MaxConcurrentBehaviorExecutions = maxConcurrentBehaviorExecutions;

        return this;
    }

    public IResourceBuilder SetMaxQueuedBehaviorExecutions(int maxQueuedBehaviorExecutions)
    {
        MaxQueuedBehaviorExecutions = maxQueuedBehaviorExecutions;

        return this;
    }

    public IResourceBuilder SetMaxQueueWaitTime(TimeSpan maxQueueWaitTime)
    {
        if (maxQueueWaitTime <= TimeSpan.Zero && maxQueueWaitTime != Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(maxQueueWaitTime), "Queue wait time must be positive or Timeout.InfiniteTimeSpan.");
        }

        MaxQueueWaitTime = maxQueueWaitTime == Timeout.InfiniteTimeSpan ? null : maxQueueWaitTime;

        return this;
    }

    public Resource Build()
        => new Resource(resourceName, MaxConcurrentBehaviorExecutions, MaxQueuedBehaviorExecutions, MaxQueueWaitTime);
}