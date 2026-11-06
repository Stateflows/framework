using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines;
using ExecutionContext = Stateflows.Common.IExecutionContext;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class HandlerLifetimeTests
{
    [TestMethod]
    [DataRow(false, false)]
    [DataRow(true, false)]
    [DataRow(false, true)]
    [DataRow(true, true)]
    public async Task CancellationTokenResolutionUsesExecutionToken(bool canceled, bool registerToken)
    {
        using var cancellation = new CancellationTokenSource();
        if (canceled)
            cancellation.Cancel();
        var services = new ServiceCollection();
        services.AddSingleton<ExecutionContext>(new TokenContext(cancellation.Token));
        if (registerToken)
            services.AddSingleton(typeof(CancellationToken), CancellationToken.None);
        await using var provider = services.BuildServiceProvider();
        var parameters = typeof(TokenConsumer).GetConstructors().Single().GetParameters();

        var values = await StateflowsActivator.ResolveParameterValuesAsync(provider, typeof(TokenConsumer), "test", parameters);
        var factories = StateflowsActivator.ResolveParameterValueFactories(provider, typeof(TokenConsumer), "test", parameters);
        var consumer = await StateflowsActivator.CreateModelElementInstanceAsync<TokenConsumer>(provider);

        Assert.AreEqual(cancellation.Token, (CancellationToken)values[0]);
        Assert.AreEqual(cancellation.Token, (CancellationToken)await factories[0]());
        Assert.AreEqual(cancellation.Token, consumer.Token);
    }

    [TestMethod]
    [DataRow("success")]
    [DataRow("handler-failure")]
    [DataRow("cancellation")]
    [DataRow("activation-failure")]
    [DataRow("disposal-failure")]
    public async Task OrleansTypedHandlerScopesAreDisposedAfterAwaitedWork(string scenario)
    {
        var probe = new LifetimeProbe { Scenario = scenario };
        var ports = GetFreeTcpPorts();
        var cluster = Guid.NewGuid().ToString("N");
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(probe);
                services.AddScoped<ScopedResource>();
                services.AddScoped<SynchronousResource>();
                services.AddStateflows(builder => builder.AddStateMachines(machines => machines
                    .AddStateMachine("handler-lifetime", machine => machine
                        .AddInitialState("initial", state => state
                            .AddOnEntry<AwaitedEntry>()
                            .AddOnEntry<AwaitedEntry>()
                            .AddOnExit<AwaitedExit>()
                            .AddTransition<Advance>("next", transition => transition
                                .AddGuard<AwaitedGuard>()
                                .AddEffect<AwaitedEffect>()))
                        .AddState("next"))));
            })
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
            .UseOrleans(silo => silo
                .UseLocalhostClustering(ports[0], ports[1], clusterId: cluster, serviceId: cluster)
                .AddMemoryGrainStorageAsDefault()
                .UseInMemoryReminderService()
                .AddStateflowsRuntime())
            .Build();
        try
        {
            await host.StartAsync();
            using var behavior = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new StateMachineId("handler-lifetime", "one"));
            var result = await behavior.SendAsync(new Initialize());

            Assert.AreEqual(scenario == "success" ? EventStatus.Initialized : EventStatus.Failed, result.Status);
            Assert.AreEqual(scenario == "success" ? 2 : 1, probe.Resources.Count);
            AssertDisposed(probe);
            if (scenario == "success")
            {
                Assert.AreEqual(EventStatus.Consumed, (await behavior.SendAsync(new Advance())).Status);
                Assert.AreEqual(5, probe.Resources.Count, "Each entry, guard, exit and effect must have a distinct scope.");
                AssertDisposed(probe);
            }
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private static void AssertDisposed(LifetimeProbe probe)
    {
        foreach (var resource in probe.Resources)
        {
            Assert.AreEqual(1, resource.DisposeCalls);
            Assert.IsTrue(resource.Disposed, "Async disposal must finish before the response.");
            Assert.AreEqual(probe.Scenario != "activation-failure", resource.HandlerFinished);
        }
        Assert.AreEqual(probe.Resources.Count, probe.SynchronousResources.Count);
        // Microsoft DI stops disposing a scope's remaining services when one throws.
        var synchronousDisposeCalls = probe.Scenario == "disposal-failure" ? 0 : 1;
        Assert.IsTrue(probe.SynchronousResources.All(resource => resource.DisposeCalls == synchronousDisposeCalls));
    }

    private static int[] GetFreeTcpPorts()
    {
        var listeners = new[] { new TcpListener(IPAddress.Loopback, 0), new TcpListener(IPAddress.Loopback, 0) };
        try
        {
            foreach (var listener in listeners)
                listener.Start();
            return listeners.Select(listener => ((IPEndPoint)listener.LocalEndpoint).Port).ToArray();
        }
        finally
        {
            foreach (var listener in listeners)
                listener.Stop();
        }
    }

    public sealed class Advance;

    public sealed class LifetimeProbe
    {
        public string Scenario { get; init; } = "success";
        public ConcurrentBag<ScopedResource> Resources { get; } = [];
        public ConcurrentBag<SynchronousResource> SynchronousResources { get; } = [];
    }

    public sealed class SynchronousResource : IDisposable
    {
        public int DisposeCalls { get; private set; }

        public SynchronousResource(LifetimeProbe probe) => probe.SynchronousResources.Add(this);

        public void Dispose() => DisposeCalls++;
    }

    public sealed class ScopedResource : IAsyncDisposable
    {
        private readonly LifetimeProbe probe;
        private readonly SynchronousResource synchronous;
        public bool HandlerFinished { get; private set; }
        public bool Disposed { get; private set; }
        public int DisposeCalls { get; private set; }

        public ScopedResource(LifetimeProbe probe, SynchronousResource synchronous)
        {
            this.probe = probe;
            this.synchronous = synchronous;
            probe.Resources.Add(this);
        }

        public void ValidateActivation()
        {
            if (probe.Scenario == "activation-failure")
                throw new InvalidOperationException("activation failed");
        }

        public async Task RunAsync()
        {
            Assert.IsFalse(Disposed);
            Assert.AreEqual(0, synchronous.DisposeCalls);
            await Task.Delay(10);
            Assert.IsFalse(Disposed, "Dependencies must remain alive across handler awaits.");
            HandlerFinished = true;
            if (probe.Scenario == "handler-failure")
                throw new InvalidOperationException("handler failed");
            if (probe.Scenario == "cancellation")
                throw new OperationCanceledException(new CancellationToken(true));
        }

        public async ValueTask DisposeAsync()
        {
            DisposeCalls++;
            await Task.Delay(10);
            Disposed = true;
            if (probe.Scenario == "disposal-failure")
                throw new InvalidOperationException("disposal failed");
        }
    }

    public sealed class AwaitedEntry : IStateEntry
    {
        private readonly ScopedResource resource;

        public AwaitedEntry(ScopedResource resource, CancellationToken token, ExecutionContext context)
        {
            Assert.AreEqual(context.CancellationToken, token);
            this.resource = resource;
            resource.ValidateActivation();
        }

        public Task OnEntryAsync() => resource.RunAsync();
    }

    public sealed class AwaitedExit(ScopedResource resource) : IStateExit
    {
        public Task OnExitAsync() => resource.RunAsync();
    }

    public sealed class AwaitedGuard(ScopedResource resource) : ITransitionGuard<Advance>
    {
        public async Task<bool> GuardAsync(Advance @event)
        {
            await resource.RunAsync();
            return true;
        }
    }

    public sealed class AwaitedEffect(ScopedResource resource) : ITransitionEffect<Advance>
    {
        public Task EffectAsync(Advance @event) => resource.RunAsync();
    }

    public sealed class TokenConsumer(CancellationToken token)
    {
        public CancellationToken Token { get; } = token;
    }

    private sealed class TokenContext(CancellationToken token) : ExecutionContext
    {
        public ExecutionMode ExecutionMode => ExecutionMode.Transactional;
        public object? ExecutionTrigger => null;
        public Guid? ExecutionTriggerId => null;
        public IDictionary<string, EventHeader>? Headers => null;
        public CancellationToken CancellationToken => token;
    }
}
