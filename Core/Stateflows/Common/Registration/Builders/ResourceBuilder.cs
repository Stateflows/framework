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
    public Resource(string name, int? maxConcurrentBehaviorExecutions = null)
    {
        Name = name;

        if (maxConcurrentBehaviorExecutions == null) return;
        
        MaxConcurrentBehaviorExecutions = (maxConcurrentBehaviorExecutions ?? 0) > 0
            ? maxConcurrentBehaviorExecutions!.Value
            : Environment.ProcessorCount;

        ConcurrencySemaphore = new SemaphoreSlim(MaxConcurrentBehaviorExecutions, MaxConcurrentBehaviorExecutions);
    }
    
    private int behaviorExecutionCounter = 0;
    
    private Channel<ExecutionToken> EventChannel { get; } = Channel.CreateUnbounded<ExecutionToken>();

    private readonly SemaphoreSlim? ConcurrencySemaphore;

    public async Task WriteAsync(ExecutionToken executionToken, CancellationToken cancellationToken = default)
    {
        await EventChannel.Writer.WriteAsync(executionToken, cancellationToken);
    }

    public async Task<ExecutionToken> ReadAsync(CancellationToken cancellationToken = default)
    {
        var executionToken = await EventChannel.Reader.ReadAsync(cancellationToken);
        
        if (ConcurrencySemaphore != null)
        {
            await ConcurrencySemaphore.WaitAsync(cancellationToken);
            Interlocked.Increment(ref behaviorExecutionCounter);
        }

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
        if (ConcurrencySemaphore != null)
        {
            await ConcurrencySemaphore.WaitAsync(cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
        }
        
        return new Releaser(ConcurrencySemaphore);
    }

    private sealed class Releaser(SemaphoreSlim? semaphore) : IDisposable
    {
        private SemaphoreSlim? semaphore = semaphore;
        public void Dispose()
            => Interlocked.Exchange(ref semaphore, null)?.Release();
    }

    public int MaxConcurrentBehaviorExecutions { get; } = -1;

    public int EventQueueLength => EventChannel.Reader.Count;
    
    public int BehaviorExecutionsCount => System.Threading.Volatile.Read(ref behaviorExecutionCounter);
}

internal class ResourceBuilder(string resourceName) : IResourceBuilder
{
    private int? MaxConcurrentBehaviorExecutions = null;
    
    public IResourceBuilder SetMaxConcurrentBehaviorExecutions(int maxConcurrentBehaviorExecutions)
    {
        MaxConcurrentBehaviorExecutions = maxConcurrentBehaviorExecutions;

        return this;
    }

    public Resource Build()
        => new Resource(resourceName, MaxConcurrentBehaviorExecutions);
}