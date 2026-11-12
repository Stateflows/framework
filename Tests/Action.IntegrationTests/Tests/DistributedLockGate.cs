using Medallion.Threading;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Action.IntegrationTests.Tests;

[TestClass]
public class DistributedLockGate
{
    private static readonly BehaviorId Id = new(BehaviorType.StateMachine, "gate-test", "1");
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(10);

    private static ServiceProvider CreateProvider(Func<string, Task<IDistributedLock>> factory)
    {
        var services = new ServiceCollection();
        services.AddStateflows(builder => builder.AddDistributedLock((_, key) => factory(key)));
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task SameKeyWaitsBeforeFactoryAcrossScopes()
    {
        var calls = 0;
        using var provider = CreateProvider(key =>
        {
            Interlocked.Increment(ref calls);
            return Task.FromResult<IDistributedLock>(new FakeLock(key));
        });
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var first = await firstScope.ServiceProvider.GetRequiredService<IStateflowsLock>().AquireLockAsync(Id);
        var waiting = secondScope.ServiceProvider.GetRequiredService<IStateflowsLock>().AquireLockAsync(Id);
        try
        {
            Assert.IsFalse(waiting.IsCompleted);
            Assert.AreEqual(1, calls);
        }
        finally
        {
            await first.DisposeAsync();
        }

        await using var second = await waiting.WaitAsync(TestTimeout);
        Assert.AreEqual(2, calls);
    }

    [TestMethod]
    public async Task DifferentBehaviorsAndScopesAreIndependent()
    {
        using var provider = CreateProvider(key => Task.FromResult<IDistributedLock>(new FakeLock(key)));
        using var scope = provider.CreateScope();
        var locks = scope.ServiceProvider.GetRequiredService<IStateflowsLock>();
        await using var first = await locks.AquireLockAsync(Id);
        await using var otherScope = await locks.AquireLockAsync(Id, "values", TimeSpan.Zero);
        await using var otherBehavior = await locks.AquireLockAsync(
            new BehaviorId(BehaviorType.StateMachine, "gate-test", "2"), TimeSpan.Zero);
    }

    [TestMethod]
    public async Task LocalTimeoutDoesNotInvokeFactoryOrLeakGate()
    {
        var calls = 0;
        using var provider = CreateProvider(key =>
        {
            Interlocked.Increment(ref calls);
            return Task.FromResult<IDistributedLock>(new FakeLock(key));
        });
        var locks = provider.GetRequiredService<IStateflowsLock>();
        var first = await locks.AquireLockAsync(Id);
        try
        {
            await Assert.ThrowsExactlyAsync<TimeoutException>(() => locks.AquireLockAsync(Id, TimeSpan.Zero));
            await Assert.ThrowsExactlyAsync<TimeoutException>(() => locks.AquireLockAsync(Id, TimeSpan.FromMilliseconds(20)));
            Assert.AreEqual(1, calls);
        }
        finally
        {
            await first.DisposeAsync();
        }

        await using var next = await locks.AquireLockAsync(Id, TimeSpan.Zero);
        Assert.AreEqual(2, calls);
    }

    [TestMethod]
    public async Task FactoryAndAcquisitionFailuresReleaseGate()
    {
        var calls = 0;
        using var provider = CreateProvider(key =>
        {
            var call = Interlocked.Increment(ref calls);
            if (call == 1)
            {
                throw new InvalidOperationException("Factory failure");
            }
            return Task.FromResult<IDistributedLock>(new FakeLock(key)
            {
                Acquire = _ => call == 2
                    ? ValueTask.FromException<IDistributedSynchronizationHandle>(new TimeoutException("SQL timeout"))
                    : ValueTask.FromResult<IDistributedSynchronizationHandle>(new FakeHandle())
            });
        });
        var locks = provider.GetRequiredService<IStateflowsLock>();
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => locks.AquireLockAsync(Id));
        await Assert.ThrowsExactlyAsync<TimeoutException>(() => locks.AquireLockAsync(Id));
        await using var next = await locks.AquireLockAsync(Id, TimeSpan.Zero);
    }

    [TestMethod]
    public async Task GateRemainsHeldUntilDistributedDisposalCompletes()
    {
        var disposing = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        using var provider = CreateProvider(key =>
        {
            var call = Interlocked.Increment(ref calls);
            return Task.FromResult<IDistributedLock>(new FakeLock(key)
            {
                Acquire = _ => ValueTask.FromResult<IDistributedSynchronizationHandle>(new FakeHandle(async () =>
                {
                    if (call == 1)
                    {
                        disposing.SetResult();
                        await release.Task;
                    }
                }))
            });
        });
        var locks = provider.GetRequiredService<IStateflowsLock>();
        var first = await locks.AquireLockAsync(Id);
        var disposal = first.DisposeAsync().AsTask();
        await disposing.Task.WaitAsync(TestTimeout);
        var waiting = locks.AquireLockAsync(Id);
        try
        {
            Assert.IsFalse(waiting.IsCompleted);
            Assert.AreEqual(1, calls);
        }
        finally
        {
            release.SetResult();
        }
        await disposal.WaitAsync(TestTimeout);
        await first.DisposeAsync();
        await using var next = await waiting.WaitAsync(TestTimeout);
        Assert.AreEqual(2, calls);
    }

    [TestMethod]
    public async Task DisposalFailureReleasesGate()
    {
        var calls = 0;
        using var provider = CreateProvider(key =>
        {
            var call = Interlocked.Increment(ref calls);
            return Task.FromResult<IDistributedLock>(new FakeLock(key)
            {
                Acquire = _ => ValueTask.FromResult<IDistributedSynchronizationHandle>(new FakeHandle(
                    () => call == 1 ? Task.FromException(new InvalidOperationException("Disposal failure")) : Task.CompletedTask))
            });
        });
        var locks = provider.GetRequiredService<IStateflowsLock>();
        var first = await locks.AquireLockAsync(Id);
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => first.DisposeAsync().AsTask());
        await using var next = await locks.AquireLockAsync(Id, TimeSpan.Zero);
    }

    [TestMethod]
    public async Task FactoryTimeIsDeductedFromDistributedTimeout()
    {
        TimeSpan? receivedTimeout = null;
        using var provider = CreateProvider(async key =>
        {
            await Task.Delay(50);
            return new FakeLock(key)
            {
                Acquire = timeout =>
                {
                    receivedTimeout = timeout;
                    return ValueTask.FromResult<IDistributedSynchronizationHandle>(new FakeHandle());
                }
            };
        });
        await using var handle = await provider.GetRequiredService<IStateflowsLock>()
            .AquireLockAsync(Id, TimeSpan.FromMilliseconds(10));
        Assert.AreEqual(TimeSpan.Zero, receivedTimeout);
    }

    [TestMethod]
    public async Task LocalWaitIsDeductedFromDistributedTimeout()
    {
        var factoryReached = new TaskCompletionSource<TimeSpan?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        using var provider = CreateProvider(key =>
        {
            var call = Interlocked.Increment(ref calls);
            return Task.FromResult<IDistributedLock>(new FakeLock(key)
            {
                Acquire = timeout =>
                {
                    if (call == 2)
                    {
                        factoryReached.SetResult(timeout);
                    }
                    return ValueTask.FromResult<IDistributedSynchronizationHandle>(new FakeHandle());
                }
            });
        });
        var locks = provider.GetRequiredService<IStateflowsLock>();
        var first = await locks.AquireLockAsync(Id);
        var timeout = TimeSpan.FromSeconds(5);
        var waiting = locks.AquireLockAsync(Id, timeout);
        await Task.Delay(50);
        await first.DisposeAsync();
        await using var next = await waiting.WaitAsync(TestTimeout);
        var remaining = await factoryReached.Task.WaitAsync(TestTimeout);
        Assert.IsTrue(remaining >= TimeSpan.Zero && remaining < timeout - TimeSpan.FromMilliseconds(20));
    }

    [TestMethod]
    public async Task SeparateProvidersDoNotShareLocalGate()
    {
        using var firstProvider = CreateProvider(key => Task.FromResult<IDistributedLock>(new FakeLock(key)));
        using var secondProvider = CreateProvider(key => Task.FromResult<IDistributedLock>(new FakeLock(key)));
        await using var first = await firstProvider.GetRequiredService<IStateflowsLock>().AquireLockAsync(Id);
        await using var second = await secondProvider.GetRequiredService<IStateflowsLock>().AquireLockAsync(Id, TimeSpan.Zero);
    }

    [TestMethod]
    public async Task ContentionAndRepeatedHandoffsKeepOneDistributedHolder()
    {
        var active = 0;
        var overlap = 0;
        var calls = 0;
        using var provider = CreateProvider(key =>
        {
            Interlocked.Increment(ref calls);
            if (Interlocked.Increment(ref active) != 1)
            {
                Interlocked.Increment(ref overlap);
            }
            return Task.FromResult<IDistributedLock>(new FakeLock(key)
            {
                Acquire = _ => ValueTask.FromResult<IDistributedSynchronizationHandle>(new FakeHandle(() =>
                {
                    Interlocked.Decrement(ref active);
                    return Task.CompletedTask;
                }))
            });
        });
        for (var round = 0; round < 3; round++)
        {
            var tasks = Enumerable.Range(0, 100).Select(_ => Task.Run(async () =>
            {
                using var scope = provider.CreateScope();
                await using var handle = await scope.ServiceProvider.GetRequiredService<IStateflowsLock>()
                    .AquireLockAsync(Id, Timeout.InfiniteTimeSpan);
                await Task.Yield();
            }));
            await Task.WhenAll(tasks).WaitAsync(TestTimeout);
        }
        Assert.AreEqual(0, overlap);
        Assert.AreEqual(0, active);
        Assert.AreEqual(300, calls);
    }

    [TestMethod]
    public async Task InvalidTimeoutDoesNotInvokeFactory()
    {
        var calls = 0;
        using var provider = CreateProvider(key =>
        {
            calls++;
            return Task.FromResult<IDistributedLock>(new FakeLock(key));
        });
        var locks = provider.GetRequiredService<IStateflowsLock>();
        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(
            () => locks.AquireLockAsync(Id, TimeSpan.FromMilliseconds(-2)));
        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(
            () => locks.AquireLockAsync(Id, TimeSpan.FromMilliseconds((double)int.MaxValue + 1)));
        Assert.AreEqual(0, calls);
        await using var next = await locks.AquireLockAsync(Id, TimeSpan.Zero);
    }

    private sealed class FakeLock(string name) : IDistributedLock
    {
        public string Name => name;
        public Func<TimeSpan?, ValueTask<IDistributedSynchronizationHandle>> Acquire { get; init; }
            = _ => ValueTask.FromResult<IDistributedSynchronizationHandle>(new FakeHandle());

        IDistributedSynchronizationHandle IDistributedLock.Acquire(TimeSpan? timeout, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public IDistributedSynchronizationHandle? TryAcquire(TimeSpan timeout, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public ValueTask<IDistributedSynchronizationHandle> AcquireAsync(TimeSpan? timeout, CancellationToken cancellationToken)
            => Acquire(timeout);

        public ValueTask<IDistributedSynchronizationHandle?> TryAcquireAsync(TimeSpan timeout, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class FakeHandle(Func<Task>? dispose = null) : IDistributedSynchronizationHandle
    {
        public CancellationToken HandleLostToken => CancellationToken.None;
        public void Dispose() => throw new NotSupportedException();
        public ValueTask DisposeAsync() => new(dispose?.Invoke() ?? Task.CompletedTask);
    }
}
