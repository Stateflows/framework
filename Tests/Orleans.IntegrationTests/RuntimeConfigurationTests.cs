using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class RuntimeConfigurationTests
{
    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(false, true, "workflow-storage")]
    public async Task MissingInfrastructureFailsHostStartup(bool storage, bool reminders, string? providerName = null)
    {
        using var host = CreateHost(storage, reminders, runtimeFirst: true, providerName);
        var failure = await Assert.ThrowsExactlyAsync<OrleansConfigurationException>(
            () => host.StartAsync());
        StringAssert.Contains(failure.Message, "Stateflows Orleans runtime requires");
        if (!storage) StringAssert.Contains(failure.Message, providerName ?? "default grain storage");
        if (!reminders) StringAssert.Contains(failure.Message, "reminders");
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    [DataRow(false, "workflow-storage")]
    [DataRow(true, "workflow-storage")]
    public async Task RegisteredInfrastructureStartsRegardlessOfRegistrationOrder(bool runtimeFirst, string? providerName = null)
    {
        var selected = new RecordingStorage();
        var other = new RecordingStorage();
        using var host = CreateHost(storage: true, reminders: true, runtimeFirst, providerName, selected, other);
        await host.StartAsync();
        try
        {
            var grain = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(
                StateflowsGrainKeys.ForBehavior("host", new BehaviorId(BehaviorType.StateMachine, "storage-test", "instance")));
            EventHolder input = new Initialize().ToEventHolder();
            input.SentAt = DateTime.UtcNow;
            Assert.AreEqual(EventStatus.Initialized, (await grain.ProcessEventAsync(input, default)).Status);
            CollectionAssert.AreEquivalent(new[]
            {
                "Stateflows.BehaviorContext", "Stateflows.BehaviorInstances", "Stateflows.Notifications",
                "Stateflows.RetainedNotifications", "Stateflows.Subscriptions"
            }, selected.Reads.Distinct().ToArray(), "Every persistent-state facet must use the selected provider and unified names.");
            Assert.IsTrue(selected.Writes.Contains("Stateflows.BehaviorContext"));
            Assert.IsTrue(selected.Writes.Contains("Stateflows.BehaviorInstances"));
            Assert.IsTrue(selected.Writes.Contains("Stateflows.Notifications"));
            Assert.IsTrue(other.Reads.IsEmpty, "An explicitly selected provider must never fall back to another provider.");
        }
        finally { await host.StopAsync(); }
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public async Task EmptyProviderNameFailsStartup(string providerName)
    {
        using var host = CreateHost(storage: true, reminders: true, runtimeFirst: true, providerName);
        var failure = await Assert.ThrowsExactlyAsync<OrleansConfigurationException>(() => host.StartAsync());
        StringAssert.Contains(failure.Message, nameof(StateflowsRuntimeOptions.StorageProviderName));
    }

    private static IHost CreateHost(bool storage, bool reminders, bool runtimeFirst, string? providerName = null,
        RecordingStorage? selected = null, RecordingStorage? other = null)
    {
        var listeners = Enumerable.Range(0, 2).Select(_ => new TcpListener(IPAddress.Loopback, 0)).ToArray();
        int[] ports;
        try
        {
            foreach (var listener in listeners) listener.Start();
            ports = listeners.Select(listener => ((IPEndPoint)listener.LocalEndpoint).Port).ToArray();
        }
        finally
        {
            foreach (var listener in listeners) listener.Stop();
        }

        return Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddStateflows(sf => sf.AddStateMachines(machines =>
                    machines.AddStateMachine("storage-test", machine => machine.AddInitialState("idle", state =>
                        state.AddOnEntry(c => c.Behavior.Publish(new StorageNotification()))))));
            })
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
            .UseOrleans(silo =>
            {
                var cluster = Guid.NewGuid().ToString("N");
                silo.UseLocalhostClustering(ports[0], ports[1], null, cluster, cluster);
                if (runtimeFirst) silo.AddStateflowsRuntime(options => options.StorageProviderName = providerName);
                var selectedName = providerName ?? ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME;
                var otherName = providerName == null ? "unrelated-storage" : ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME;
                silo.ConfigureServices(services =>
                {
                    services.AddKeyedSingleton<IGrainStorage>(otherName, other ?? new RecordingStorage());
                    if (storage) services.AddKeyedSingleton<IGrainStorage>(selectedName, selected ?? new RecordingStorage());
                });
                if (reminders) silo.UseInMemoryReminderService();
                if (!runtimeFirst) silo.AddStateflowsRuntime(options => options.StorageProviderName = providerName);
            })
            .Build();
    }

    public sealed class StorageNotification;

    private sealed class RecordingStorage : IGrainStorage
    {
        public ConcurrentBag<string> Reads { get; } = [];
        public ConcurrentBag<string> Writes { get; } = [];

        public Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            Reads.Add(stateName);
            grainState.State = Activator.CreateInstance<T>();
            grainState.RecordExists = false;
            return Task.CompletedTask;
        }

        public Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            Writes.Add(stateName);
            grainState.RecordExists = true;
            grainState.ETag = Guid.NewGuid().ToString("N");
            return Task.CompletedTask;
        }

        public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            grainState.RecordExists = false;
            grainState.ETag = null;
            return Task.CompletedTask;
        }
    }
}
