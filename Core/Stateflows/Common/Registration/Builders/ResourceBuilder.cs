using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using Stateflows.Common.Classes;
using Stateflows.Common.Registration.Interfaces;
using System;
using System.Diagnostics;
using Stateflows.Common.Engine.Interfaces;

namespace Stateflows.Common.Registration.Builders;

internal class Resource : IStateflowsResource
{
    public string Name { get; }
    public Resource(string name, int? maxConcurrentBehaviorExecutions = null)
    {
        Name = name;
        if (maxConcurrentBehaviorExecutions != null)
        {
            var maxConcurrency = maxConcurrentBehaviorExecutions.Value > 0
                 ? maxConcurrentBehaviorExecutions.Value
                 : Environment.ProcessorCount;

            ConcurrencySemaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        }
    }
    
    private int behaviorExecutionCounter = 0;
    
    private Channel<ExecutionToken> EventChannel { get; } = Channel.CreateUnbounded<ExecutionToken>();

    private readonly SemaphoreSlim ConcurrencySemaphore = null;

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