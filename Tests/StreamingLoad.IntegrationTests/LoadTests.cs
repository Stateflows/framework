using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stateflows;
using Stateflows.Examples.StreamingLoad;

namespace StreamingLoad.IntegrationTests;

[TestClass]
public sealed class LoadTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [TestCategory("Performance")]
    [Timeout(90_000)]
    public async Task ZeroWorkSustainsThousandsOfCompletedTokensPerSecond()
    {
        await using var factory = CreateFactory();
        using var http = factory.CreateClient();
        var coordinator = factory.Services.GetRequiredService<LoadCoordinator>();
        await coordinator.StartRunAsync(new LoadSettings(TokensPerSecond: 50_000, BatchSize: 256),
            new WorkflowSettings(Workers: 16, CpuMilliseconds: 0, DelayMilliseconds: 0, AllocationKiB: 0));
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            var before = coordinator.Snapshot;
            using var process = Process.GetCurrentProcess();
            var cpu = process.TotalProcessorTime;
            var allocated = GC.GetTotalAllocatedBytes();
            var started = Stopwatch.GetTimestamp();
            await Task.Delay(TimeSpan.FromSeconds(10));
            var seconds = Stopwatch.GetElapsedTime(started).TotalSeconds;
            var after = coordinator.Snapshot;
            var completed = after.Completed - before.Completed;
            var perSecond = completed / seconds;
            var bytesPerToken = (GC.GetTotalAllocatedBytes() - allocated) / (double)Math.Max(1, completed);
            var cpuPercent = (process.TotalProcessorTime - cpu).TotalSeconds / seconds / Environment.ProcessorCount * 100;
            TestContext.WriteLine(FormattableString.Invariant(
                $"THROUGHPUT completed/s={perSecond:F0}; allocated-bytes/token={bytesPerToken:F0}; cpu={cpuPercent:F1}%; rejected={after.Rejected - before.Rejected}; overloaded-batches={after.OverloadedAdmissions - before.OverloadedAdmissions}; peak-outstanding={after.PeakOutstanding}"));
            Assert.AreEqual("Running", after.State, after.Error);
            Assert.IsTrue(after.PeakOutstanding <= after.Load.OutstandingLimit);
            Assert.AreEqual(0, after.Rejected);
            await coordinator.DrainAsync();
            await Until(() => coordinator.Snapshot.State == "Completed");
            var drained = coordinator.Snapshot;
            Assert.AreEqual(drained.Admitted, drained.Completed);
            Assert.AreEqual(drained.Completed, drained.Normal + drained.Priority);
            Assert.IsTrue(perSecond >= 2_000, $"Expected at least 2,000 completed tokens/s; measured {perSecond:F0}.");
        }
        finally
        {
            if (coordinator.Snapshot.State != "Completed")
                await coordinator.EmergencyStopAsync();
        }
    }

    [TestMethod]
    [DataRow(LoadProfile.Steady, 0, 1000)]
    [DataRow(LoadProfile.Steady, 19, 1000)]
    [DataRow(LoadProfile.Ramp, 0, 100)]
    [DataRow(LoadProfile.Ramp, 10, 550)]
    [DataRow(LoadProfile.Ramp, 20, 100)]
    [DataRow(LoadProfile.Burst, 0, 1000)]
    [DataRow(LoadProfile.Burst, 5, 100)]
    [DataRow(LoadProfile.Wave, 0, 100)]
    [DataRow(LoadProfile.Wave, 10, 1000)]
    [DataRow(LoadProfile.Wave, 20, 100)]
    public void ProfilesHaveDocumentedRates(LoadProfile profile, int seconds, int expected)
        => Assert.AreEqual(expected, new LoadSettings(Profile: profile).RateAt(TimeSpan.FromSeconds(seconds)), 0.001);

    [TestMethod]
    public void ValidationRejectsInvalidAndUnsafeSettings()
    {
        var limits = new SafetyLimits();
        foreach (var load in new[]
        {
            new LoadSettings(TokensPerSecond: 100001), new LoadSettings(TokensPerSecond: 0),
            new LoadSettings(OutstandingLimit: 100001), new LoadSettings(BatchSize: 257),
            new LoadSettings(Profile: (LoadProfile)99), new LoadSettings(PeriodSeconds: 1)
        })
            Assert.ThrowsExactly<ArgumentException>(() => load.Validate(limits));
        foreach (var workflow in new[]
        {
            new WorkflowSettings(Workers: 65), new WorkflowSettings(CpuMilliseconds: 21),
            new WorkflowSettings(DelayMilliseconds: 2001), new WorkflowSettings(AllocationKiB: 257),
            new WorkflowSettings(PriorityPercent: 101), new WorkflowSettings(Workers: 0)
        })
            Assert.ThrowsExactly<ArgumentException>(() => workflow.Validate(limits));
        Assert.ThrowsExactly<ArgumentException>(() => new SafetyLimits { MaxOutstanding = 100001 }.Validate());
    }

    [TestMethod]
    public void ThroughputAndSyntheticPresetsAreDistinctAndValid()
    {
        var limits = new SafetyLimits();
        foreach (var scenario in new[] { LoadScenario.Throughput, LoadScenario.Synthetic })
        {
            scenario.Load.Validate(limits);
            scenario.Workflow.Validate(limits);
        }
        Assert.IsTrue(LoadScenario.Throughput.Load.TokensPerSecond >= 10_000);
        Assert.AreEqual(0, LoadScenario.Throughput.Workflow.CpuMilliseconds);
        Assert.AreEqual(0, LoadScenario.Throughput.Workflow.DelayMilliseconds);
        Assert.AreEqual(0, LoadScenario.Throughput.Workflow.AllocationKiB);
        Assert.AreEqual(new WorkflowSettings(), LoadScenario.Synthetic.Workflow);
        var idle = new LoadMetrics().Snapshot(0);
        Assert.AreEqual(LoadScenario.Throughput.Load, idle.Load);
        Assert.AreEqual(LoadScenario.Throughput.Workflow, idle.Workflow);
    }

    [TestMethod]
    public void MetricsBoundOutstandingAndIgnoreDuplicateAndPreviousRunCompletions()
    {
        var metrics = new LoadMetrics();
        metrics.Begin(Guid.NewGuid(), new LoadSettings(OutstandingLimit: 4), new WorkflowSettings());
        var tokens = metrics.Reserve(100);
        Assert.HasCount(4, tokens);
        Assert.IsEmpty(metrics.Reserve(1));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            metrics.SetLoad(new LoadSettings(OutstandingLimit: 3)));
        Assert.AreEqual(4, metrics.Snapshot(0).Load.OutstandingLimit);
        metrics.Admitted(4);
        metrics.Complete(tokens[0], false);
        metrics.Complete(tokens[0], false);
        metrics.Complete(tokens[1], true);
        var snapshot = metrics.Snapshot(0);
        Assert.AreEqual(2, snapshot.Completed);
        Assert.AreEqual(1, snapshot.Normal);
        Assert.AreEqual(1, snapshot.Priority);
        Assert.AreEqual(2, snapshot.Outstanding);
        Assert.AreEqual(4, snapshot.PeakOutstanding);
        metrics.SetLoad(new LoadSettings(OutstandingLimit: 2));
        Assert.IsEmpty(metrics.Reserve(1));
        metrics.Abandon();
        Assert.AreEqual(2, metrics.Snapshot(0).Abandoned);
        metrics.Begin(Guid.NewGuid(), new LoadSettings(), new WorkflowSettings());
        metrics.Reserve(1);
        metrics.Complete(tokens[0], false);
        Assert.AreEqual(0, metrics.Snapshot(0).Completed);
        Assert.AreEqual(1, metrics.Outstanding);
    }

    [TestMethod]
    public void MetricsUseElapsedIntervalsAndKeepHistoryBounded()
    {
        var metrics = new LoadMetrics();
        metrics.Begin(Guid.NewGuid(), new LoadSettings(), new WorkflowSettings());
        var tokens = metrics.Reserve(20);
        metrics.Admitted(tokens.Length);
        foreach (var token in tokens) metrics.Complete(token, false);
        metrics.Sample(2, 25, 100, 50, 2, [1, 2, 3]);
        var snapshot = metrics.Snapshot(0);
        Assert.AreEqual(10, snapshot.AdmittedPerSecond);
        Assert.AreEqual(10, snapshot.CompletedPerSecond);
        Assert.AreEqual(25, snapshot.CpuPercent);
        for (var index = 0; index < 130; index++) metrics.Sample(1, 0, 0, 0, 0, [0, 0, 0]);
        Assert.HasCount(120, metrics.Snapshot(0).History);
    }

    [TestMethod]
    public void CancelledAdmissionReleasesReservationsWithoutCountingTokenLoss()
    {
        var metrics = new LoadMetrics();
        metrics.Begin(Guid.NewGuid(), new LoadSettings(OutstandingLimit: 4), new WorkflowSettings());
        var tokens = metrics.Reserve(4);
        metrics.AdmissionOverloaded();
        metrics.CancelReservation(tokens);
        metrics.Complete(tokens[0], false);
        var snapshot = metrics.Snapshot(0);
        Assert.AreEqual(1, snapshot.OverloadedAdmissions);
        Assert.AreEqual(0, snapshot.Rejected);
        Assert.AreEqual(0, snapshot.Admitted);
        Assert.AreEqual(0, snapshot.Completed);
        Assert.AreEqual(0, snapshot.Outstanding);
        Assert.HasCount(4, metrics.Reserve(4));
    }

    [TestMethod]
    public async Task WorkerBoundSupportsLiveDecreaseIncreaseAndCancellation()
    {
        var pool = new WorkerPool();
        pool.Configure(2);
        var first = await pool.EnterAsync(CancellationToken.None);
        var second = await pool.EnterAsync(CancellationToken.None);
        pool.Configure(1);
        var waiting = pool.EnterAsync(CancellationToken.None);
        Assert.IsFalse(waiting.IsCompleted);
        first.Dispose();
        Assert.IsFalse(waiting.IsCompleted);
        second.Dispose();
        using var third = await waiting.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.AreEqual(1, pool.Active);
        using var cancellation = new CancellationTokenSource();
        var cancelled = pool.EnterAsync(cancellation.Token);
        cancellation.Cancel();
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => cancelled);
        pool.Configure(2);
        using var fourth = await pool.EnterAsync(CancellationToken.None);
        Assert.AreEqual(2, pool.Active);
    }

    [TestMethod]
    public async Task WorkerQueueAdmitsInOrderAndRemovesCancelledWaiters()
    {
        var pool = new WorkerPool();
        pool.Configure(1);
        using var initial = await pool.EnterAsync(CancellationToken.None);
        using var cancellation = new CancellationTokenSource();
        var cancelled = pool.EnterAsync(cancellation.Token);
        var first = pool.EnterAsync(CancellationToken.None);
        var second = pool.EnterAsync(CancellationToken.None);
        cancellation.Cancel();
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => cancelled);
        initial.Dispose();
        using var next = await first.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.IsFalse(second.IsCompleted);
        pool.Configure(2);
        using var last = await second.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.AreEqual(2, pool.Active);
        next.Dispose();
        last.Dispose();
        Assert.AreEqual(0, pool.Active);
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => pool.EnterAsync(cancellation.Token));
        Assert.AreEqual(0, pool.Active);
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task StreamingRunThrottlesSteersTransactionallyDrainsAndRestarts()
    {
        await using var factory = CreateFactory();
        using var http = factory.CreateClient();
        var html = await http.GetStringAsync("/");
        StringAssert.Contains(html, "Streaming Load Lab");
        StringAssert.Contains(html, "_framework/blazor.web.");
        StringAssert.Contains(html, "Framework throughput preset");
        StringAssert.Contains(WebUtility.HtmlDecode(html), "Synthetic CPU + I/O preset");
        var coordinator = factory.Services.GetRequiredService<LoadCoordinator>();
        await coordinator.StartRunAsync(new LoadSettings(TokensPerSecond: 2000, OutstandingLimit: 32, BatchSize: 8),
            new WorkflowSettings(Workers: 1, CpuMilliseconds: 0, DelayMilliseconds: 300, PriorityPercent: 0));
        await Until(() => coordinator.Snapshot.Outstanding == 32 && coordinator.Snapshot.ActiveWorkers == 1);
        Assert.IsTrue(coordinator.Snapshot.PeakOutstanding <= 32);
        await coordinator.PauseAsync();
        var admitted = coordinator.Snapshot.Admitted;
        var normalBefore = coordinator.Snapshot.Normal;
        await coordinator.ApplyWorkflowAsync(new WorkflowSettings(
            Workers: 4, CpuMilliseconds: 0, DelayMilliseconds: 0, PriorityPercent: 100));
        var control = coordinator.Snapshot.LastControl!;
        Assert.AreEqual("Transactional", control.Mode);
        Assert.IsNotNull(control.TriggerId);
        Assert.AreEqual(2, control.Revision);
        await Until(() => coordinator.Snapshot.Outstanding == 0);
        Assert.AreEqual(admitted, coordinator.Snapshot.Admitted, "Pausing must stop new admission.");
        Assert.AreEqual(admitted, coordinator.Snapshot.Completed);
        Assert.IsTrue(coordinator.Snapshot.Priority > 0);
        Assert.AreEqual(normalBefore, coordinator.Snapshot.Normal, "The in-flight batch should read the new branch control after I/O.");
        await coordinator.ResumeAsync();
        await Until(() => coordinator.Snapshot.Admitted > admitted);
        await coordinator.DrainAsync();
        await Until(() => coordinator.Snapshot.State == "Completed");
        var drained = coordinator.Snapshot;
        Assert.AreEqual(drained.Admitted, drained.Completed);
        Assert.AreEqual(drained.Completed, drained.Normal + drained.Priority);
        Assert.AreEqual(0, drained.Outstanding);
        Assert.AreEqual(0, drained.Abandoned);
        Assert.IsTrue(drained.WorkingSetMiB > 0);
        Assert.HasCount(1, drained.History.Take(1).ToArray());
        Assert.IsTrue(drained.PeakOutstanding <= 32);

        await coordinator.StartRunAsync(new LoadSettings(TokensPerSecond: 500, OutstandingLimit: 16),
            new WorkflowSettings(Workers: 1, CpuMilliseconds: 1, DelayMilliseconds: 2000));
        await Until(() => coordinator.Snapshot.Outstanding > 0 && coordinator.Snapshot.ActiveWorkers > 0);
        await coordinator.EmergencyStopAsync().WaitAsync(TimeSpan.FromSeconds(10));
        await Until(() => coordinator.Snapshot.ActiveWorkers == 0);
        var stopped = coordinator.Snapshot;
        Assert.AreEqual("Stopped", stopped.State);
        Assert.AreEqual(0, stopped.Outstanding);
        Assert.IsTrue(stopped.Abandoned > 0);
        Assert.AreEqual(stopped.Admitted, stopped.Completed + stopped.Abandoned);
        Assert.AreNotEqual(drained.RunId, stopped.RunId);
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task OverloadedAdmissionWaitsWithoutDroppingTokensAndPauseCancelsTheWait()
    {
        await using var factory = CreateFactory().WithWebHostBuilder(builder => builder
            .ConfigureServices(services => services.Configure<StateflowsRuntimeOptions>(
                options => options.MaxQueuedActivityStreamEvents = 1)));
        using var http = factory.CreateClient();
        var coordinator = factory.Services.GetRequiredService<LoadCoordinator>();
        await coordinator.StartRunAsync(new LoadSettings(TokensPerSecond: 2000, BatchSize: 8, OutstandingLimit: 64),
            new WorkflowSettings(Workers: 1, CpuMilliseconds: 0, DelayMilliseconds: 300));
        await Until(() => coordinator.Snapshot.OverloadedAdmissions > 0);
        await Until(() => coordinator.Snapshot.Admitted >= 16);
        await coordinator.PauseAsync().WaitAsync(TimeSpan.FromSeconds(2));
        var paused = coordinator.Snapshot;
        Assert.AreEqual("Paused", paused.State);
        Assert.AreEqual(0, paused.Rejected);
        Assert.AreEqual(paused.Admitted - paused.Completed, paused.Outstanding);
        Assert.IsTrue(paused.PeakOutstanding <= 64);
        await coordinator.ApplyWorkflowAsync(new WorkflowSettings(Workers: 8, CpuMilliseconds: 0, DelayMilliseconds: 0));
        await coordinator.DrainAsync();
        await Until(() => coordinator.Snapshot.State == "Completed");
        Assert.AreEqual(coordinator.Snapshot.Admitted, coordinator.Snapshot.Completed);
        Assert.AreEqual(0, coordinator.Snapshot.Abandoned);
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task ZeroWorkBatchCanSwitchToSyntheticWorkAndBackTransactionally()
    {
        await using var factory = CreateFactory();
        using var http = factory.CreateClient();
        var coordinator = factory.Services.GetRequiredService<LoadCoordinator>();
        await coordinator.StartRunAsync(LoadScenario.Throughput.Load with { TokensPerSecond = 5000, OutstandingLimit = 512 },
            LoadScenario.Throughput.Workflow with { PriorityPercent = 0 });
        await Until(() => coordinator.Snapshot.Normal >= 256);
        Assert.AreEqual(0, coordinator.Snapshot.Priority);
        await coordinator.ApplyWorkflowAsync(new WorkflowSettings(
            Workers: 4, CpuMilliseconds: 1, DelayMilliseconds: 10, AllocationKiB: 4, PriorityPercent: 100));
        await Until(() => coordinator.Snapshot.Priority > 0);
        await coordinator.PauseAsync();
        await Until(() => coordinator.Snapshot.Outstanding == 0);
        var priority = coordinator.Snapshot.Priority;
        var normal = coordinator.Snapshot.Normal;
        await coordinator.ApplyWorkflowAsync(LoadScenario.Throughput.Workflow with { PriorityPercent = 0 });
        Assert.AreEqual(3, coordinator.Snapshot.LastControl!.Revision);
        Assert.AreEqual("Transactional", coordinator.Snapshot.LastControl!.Mode);
        await coordinator.ResumeAsync();
        await Until(() => coordinator.Snapshot.Normal > normal);
        await coordinator.DrainAsync();
        await Until(() => coordinator.Snapshot.State == "Completed");
        Assert.AreEqual(priority, coordinator.Snapshot.Priority);
        Assert.AreEqual(coordinator.Snapshot.Admitted, coordinator.Snapshot.Completed);
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task InvalidControlsLeaveRunningActivitySettingsUnchanged()
    {
        await using var factory = CreateFactory();
        using var http = factory.CreateClient();
        var coordinator = factory.Services.GetRequiredService<LoadCoordinator>();
        await coordinator.StartRunAsync(new LoadSettings(TokensPerSecond: 100, OutstandingLimit: 16),
            new WorkflowSettings(CpuMilliseconds: 0, DelayMilliseconds: 20));
        await Until(() => coordinator.Snapshot.Admitted > 0);
        var acknowledged = coordinator.Snapshot.LastControl;
        await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
            coordinator.ApplyWorkflowAsync(new WorkflowSettings(Workers: 1000)));
        Assert.AreEqual(acknowledged, coordinator.Snapshot.LastControl);
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
            coordinator.StartRunAsync(new LoadSettings(), new WorkflowSettings()));
        Assert.AreEqual("Running", coordinator.Snapshot.State);
        await coordinator.EmergencyStopAsync();
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        using var first = new TcpListener(IPAddress.Loopback, 0);
        using var second = new TcpListener(IPAddress.Loopback, 0);
        first.Start(); second.Start();
        var silo = ((IPEndPoint)first.LocalEndpoint).Port;
        var gateway = ((IPEndPoint)second.LocalEndpoint).Port;
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder
            .UseEnvironment("Development")
            .ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Orleans:SiloPort"] = silo.ToString(), ["Orleans:GatewayPort"] = gateway.ToString()
                })));
    }

    private static async Task Until(Func<bool> condition)
    {
        var started = Stopwatch.GetTimestamp();
        while (!condition())
        {
            if (Stopwatch.GetElapsedTime(started) > TimeSpan.FromSeconds(15))
                Assert.Fail("Timed out waiting for benchmark state.");
            await Task.Delay(20);
        }
    }
}
