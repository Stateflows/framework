using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class HandlerScopeIsolationTests
{
    [TestMethod]
    [DataRow("entry", true)]
    [DataRow("entry", false)]
    [DataRow("exit", true)]
    [DataRow("exit", false)]
    [DataRow("state-initialize", true)]
    [DataRow("state-initialize", false)]
    [DataRow("state-finalize", true)]
    [DataRow("state-finalize", false)]
    [DataRow("default-initialize", true)]
    [DataRow("default-initialize", false)]
    [DataRow("initialize", true)]
    [DataRow("initialize", false)]
    [DataRow("finalize", true)]
    [DataRow("finalize", false)]
    [DataRow("guard", true)]
    [DataRow("guard", false)]
    [DataRow("effect", true)]
    [DataRow("effect", false)]
    [DataRow("default-guard", true)]
    [DataRow("default-guard", false)]
    [DataRow("default-effect", true)]
    [DataRow("default-effect", false)]
    [DataRow("deferral-guard", true)]
    [DataRow("deferral-guard", false)]
    [DataRow("orthogonal-entry", true)]
    [DataRow("orthogonal-entry", false)]
    [DataRow("orthogonal-exit", true)]
    [DataRow("orthogonal-exit", false)]
    [DataRow("orthogonal-initialize", true)]
    [DataRow("orthogonal-initialize", false)]
    [DataRow("orthogonal-finalize", true)]
    [DataRow("orthogonal-finalize", false)]
    public async Task EachHandlerOnSameHookHasItsOwnScope(string hook, bool typed)
    {
        var probe = new ScopeProbe();
        var ports = GetFreeTcpPorts();
        var cluster = Guid.NewGuid().ToString("N");
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(probe);
                services.AddScoped<ScopeIdentity>();
                services.AddScoped<ScopeDependency>();
                services.AddStateflows(builder => builder.AddStateMachines(machines =>
                    machines.AddStateMachine("scope-isolation", machine =>
                    {
                        for (var i = 0; i < 2; i++)
                        {
                            switch (hook)
                            {
                                case "default-initialize":
                                    if (typed)
                                        machine.AddDefaultInitializer<RecordingHandler>();
                                    else
                                        machine.AddDefaultInitializer(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                    break;
                                case "initialize":
                                    if (typed)
                                        machine.AddInitializer<Start, RecordingHandler>();
                                    else
                                        machine.AddInitializer<Start>(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                    break;
                                case "finalize":
                                    if (typed)
                                        machine.AddFinalizer<RecordingHandler>();
                                    else
                                        machine.AddFinalizer(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                    break;
                            }
                        }

                        if (hook.StartsWith("orthogonal-"))
                        {
                            machine.AddInitialOrthogonalState("initial", state =>
                            {
                                for (var i = 0; i < 2; i++)
                                {
                                    switch (hook)
                                    {
                                        case "orthogonal-entry":
                                            if (typed)
                                                state.AddOnEntry<RecordingHandler>();
                                            else
                                                state.AddOnEntry(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                            break;
                                        case "orthogonal-exit":
                                            if (typed)
                                                state.AddOnExit<RecordingHandler>();
                                            else
                                                state.AddOnExit(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                            break;
                                        case "orthogonal-initialize":
                                            if (typed)
                                                state.AddOnInitialize<RecordingHandler>();
                                            else
                                                state.AddOnInitialize(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                            break;
                                        case "orthogonal-finalize":
                                            if (typed)
                                                state.AddOnFinalize<RecordingHandler>();
                                            else
                                                state.AddOnFinalize(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                            break;
                                    }
                                }
                                state.AddRegion(region => region
                                        .AddInitialState("child", child => child.AddTransition<Advance>("child-final"))
                                        .AddFinalState("child-final"))
                                    .AddRegion(region => region
                                        .AddInitialState("other-child", child => child.AddTransition<Advance>("other-final"))
                                        .AddFinalState("other-final"))
                                    .AddTransition<Leave>("final");
                            }).AddFinalState("final");
                            return;
                        }

                        if (hook is "state-initialize" or "state-finalize")
                        {
                            machine.AddInitialCompositeState("initial", state =>
                            {
                                for (var i = 0; i < 2; i++)
                                {
                                    if (hook == "state-initialize")
                                    {
                                        if (typed)
                                            state.AddOnInitialize<RecordingHandler>();
                                        else
                                            state.AddOnInitialize(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                    }
                                    else
                                    {
                                        if (typed)
                                            state.AddOnFinalize<RecordingHandler>();
                                        else
                                            state.AddOnFinalize(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                    }
                                }
                                state.AddInitialState("child", child => child.AddTransition<Advance>("child-final"))
                                    .AddFinalState("child-final");
                            });
                            return;
                        }

                        machine.AddInitialState("initial", state =>
                        {
                            for (var i = 0; i < 2; i++)
                            {
                                if (hook == "entry")
                                {
                                    if (typed)
                                        state.AddOnEntry<RecordingHandler>();
                                    else
                                        state.AddOnEntry(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                }
                                if (hook == "exit")
                                {
                                    if (typed)
                                        state.AddOnExit<RecordingHandler>();
                                    else
                                        state.AddOnExit(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                }
                            }

                            if (hook == "deferral-guard")
                            {
                                state.AddDeferredEvent<Advance>(deferral =>
                                {
                                    for (var i = 0; i < 2; i++)
                                    {
                                        if (typed)
                                            deferral.AddGuard<RecordingHandler>();
                                        else
                                            deferral.AddGuard(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                    }
                                });
                            }
                            else if (hook is "default-guard" or "default-effect" or "finalize")
                            {
                                state.AddDefaultTransition("final", transition =>
                                {
                                    for (var i = 0; i < 2; i++)
                                    {
                                        if (hook == "default-guard")
                                        {
                                            if (typed)
                                                transition.AddGuard<RecordingHandler>();
                                            else
                                                transition.AddGuard(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                        }
                                        if (hook == "default-effect")
                                        {
                                            if (typed)
                                                transition.AddEffect<RecordingHandler>();
                                            else
                                                transition.AddEffect(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                        }
                                    }
                                });
                            }
                            else
                            {
                                state.AddTransition<Advance>("final", transition =>
                                {
                                    for (var i = 0; i < 2; i++)
                                    {
                                        if (hook == "guard")
                                        {
                                            if (typed)
                                                transition.AddGuard<RecordingHandler>();
                                            else
                                                transition.AddGuard(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                        }
                                        if (hook == "effect")
                                        {
                                            if (typed)
                                                transition.AddEffect<RecordingHandler>();
                                            else
                                                transition.AddEffect(c => probe.RecordAsync(c.Behavior.ServiceProvider));
                                        }
                                    }
                                });
                            }
                        }).AddFinalState("final");
                    })));
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
                .CreateBehavior(new StateMachineId("scope-isolation", "one"));
            var initialization = hook == "initialize"
                ? await behavior.SendAsync(new Start())
                : await behavior.SendAsync(new Initialize());
            Assert.AreNotEqual(EventStatus.Failed, initialization.Status);
            if (hook is "exit" or "guard" or "effect" or "state-finalize" or "deferral-guard"
                or "orthogonal-exit" or "orthogonal-finalize")
            {
                var result = await behavior.SendAsync(new Advance());
                Assert.AreEqual(hook == "deferral-guard" ? EventStatus.Deferred : EventStatus.Consumed, result.Status);
                if (hook == "orthogonal-exit")
                    Assert.AreEqual(EventStatus.Consumed, (await behavior.SendAsync(new Leave())).Status);
            }

            // Orthogonal finalization currently invokes the handlers for each completed region.
            var expectedCalls = hook == "orthogonal-finalize" ? 4 : 2;
            Assert.HasCount(expectedCalls, probe.Calls, $"Both {hook} handlers must actually run.");
            for (var i = 0; i < probe.Calls.Count; i++)
            {
                for (var j = i + 1; j < probe.Calls.Count; j++)
                    Assert.AreNotSame(probe.Calls[i], probe.Calls[j],
                        $"The {hook} handlers ({(typed ? "typed" : "callback")}) shared a scoped service.");
            }
            Assert.IsTrue(probe.Calls.All(identity => identity.DisposeCalls == 1),
                "Both scopes must be disposed before event processing returns.");
        }
        finally
        {
            await host.StopAsync();
        }
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

    public sealed class Start;
    public sealed class Advance;
    public sealed class Leave;

    public sealed class ScopeIdentity : IAsyncDisposable
    {
        public int DisposeCalls { get; private set; }

        public async ValueTask DisposeAsync()
        {
            await Task.Yield();
            DisposeCalls++;
        }
    }

    public sealed class ScopeDependency(ScopeIdentity identity)
    {
        public ScopeIdentity Identity { get; } = identity;
    }

    public sealed class ScopeProbe
    {
        public List<ScopeIdentity> Calls { get; } = [];

        public async Task<bool> RecordAsync(IServiceProvider provider, ScopeIdentity? injected = null)
        {
            var identity = provider.GetRequiredService<ScopeIdentity>();
            if (injected != null)
                Assert.AreSame(injected, identity, "Constructor injection must use the invocation's scope.");
            Assert.AreSame(identity, provider.GetRequiredService<ScopeDependency>().Identity);
            Assert.AreEqual(0, identity.DisposeCalls);
            await Task.Yield();
            Assert.AreSame(identity, provider.GetRequiredService<ScopeIdentity>(),
                "Scoped identity must remain stable across awaits within one handler.");
            Assert.AreEqual(0, identity.DisposeCalls);
            Calls.Add(identity);
            return true;
        }
    }

    public sealed class RecordingHandler(
        ScopeIdentity identity,
        ScopeDependency dependency,
        IServiceProvider provider,
        ScopeProbe probe) :
        IStateEntry, IStateExit, ICompositeStateInitialization, ICompositeStateFinalization,
        IDefaultInitializer, IInitializer<Start>, IFinalizer,
        ITransitionGuard<Advance>, ITransitionEffect<Advance>,
        IDefaultTransitionGuard, IDefaultTransitionEffect, IDeferralGuard<Advance>
    {
        private Task<bool> RecordAsync()
        {
            Assert.AreSame(identity, dependency.Identity);
            return probe.RecordAsync(provider, identity);
        }

        public Task OnEntryAsync() => RecordAsync();
        public Task OnExitAsync() => RecordAsync();
        Task ICompositeStateInitialization.OnInitializeAsync() => RecordAsync();
        Task<bool> IDefaultInitializer.OnInitializeAsync() => RecordAsync();
        public Task<bool> OnInitializeAsync(Start @event) => RecordAsync();
        public Task OnFinalizeAsync() => RecordAsync();
        public Task<bool> GuardAsync(Advance @event) => RecordAsync();
        public Task<bool> GuardAsync() => RecordAsync();
        public Task EffectAsync(Advance @event) => RecordAsync();
        public Task EffectAsync() => RecordAsync();
    }
}
