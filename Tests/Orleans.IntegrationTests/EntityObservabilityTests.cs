using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Context.Interfaces;
using Stateflows.Entities;
using Stateflows.Entities.Attributes;

namespace Orleans.IntegrationTests;

public sealed partial class BehaviorNotificationTests
{
    [TestMethod]
    public async Task EntityHooksAreAwaitedThroughGrainsAndAfterReactivation()
    {
        var ports = GetFreeTcpPorts(2);
        var cluster = Guid.NewGuid().ToString("N");
        var hooks = new EntityHookProbe();
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) => services
                .AddStateflows(sf => sf.AddEntities(entities => entities
                    .AddObserver((_, _) => hooks)
                    .AddInterceptor((_, _) => hooks)
                    .AddEntity<IObservedCounter>("observed-counter"))
                )
            )
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
            .UseOrleans(silo => silo
                .UseLocalhostClustering(ports[0], ports[1], null, cluster, cluster)
                .AddMemoryGrainStorage("stateflows")
                .UseInMemoryReminderService()
                .AddStateflowsRuntime())
            .Build();
        await host.StartAsync();
        try
        {
            using var scope = host.Services.CreateScope();
            var locator = scope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
            Assert.IsTrue(locator.TryLocateBehavior(new EntityId("observed-counter", "one"), out var behavior));
            using (behavior)
            {
                Assert.AreEqual(EventStatus.Rejected, (await behavior.SendAsync(new BlockCounter())).Status);
                Assert.AreEqual(0, hooks.Operations.Count, "Short-circuiting must bypass initialization.");
                Assert.AreEqual(EventStatus.Consumed, (await behavior.SendAsync(new AddCounter { Amount = 3 })).Status);
                CollectionAssert.Contains(hooks.Operations.ToArray(), "mutation:after");
                CollectionAssert.Contains(hooks.Operations.ToArray(), "field:6");
                CollectionAssert.Contains(hooks.Operations.ToArray(), "projection:6");

                var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
                await management.ForceActivationCollection(TimeSpan.Zero);
                while (hooks.Operations.TryDequeue(out _)) { }

                Assert.AreEqual(EventStatus.Consumed, (await behavior.SendAsync(new AddCounter { Amount = 2 })).Status);
                var result = await behavior.RequestAsync(new ProjectionRequest<ObservedCounterView>());
                Assert.AreEqual(10, result.Response!.Value);
                CollectionAssert.Contains(hooks.Operations.ToArray(), "field:10");
                CollectionAssert.Contains(hooks.Operations.ToArray(), "projection:10");
                CollectionAssert.Contains(hooks.Operations.ToArray(), "mutation:after");
            }
        }
        finally
        {
            await host.StopAsync();
        }
    }

    public sealed class AddCounter
    {
        public int Amount { get; set; }
    }

    [TestMethod]
    public async Task EntityExceptionHandlersAreAwaitedAndFailuresRollBackAcrossReactivation()
    {
        var ports = GetFreeTcpPorts(2);
        var cluster = Guid.NewGuid().ToString("N");
        var handler = new EntityFailureHandler();
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) => services
                .AddStateflows(sf => sf.AddEntities(entities => entities
                    .AddExceptionHandler((_, _) => handler)
                    .AddEntity<IFailingCounter>("failing-counter")
                    .AddEntity<IFailingCounter>("failing-initialization", entity => entity
                        .AddDefaultInitializer(_ => throw new InvalidOperationException("initialization")))
                    )
                )
            )            
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
            .UseOrleans(silo => silo
                .UseLocalhostClustering(ports[0], ports[1], null, cluster, cluster)
                .AddMemoryGrainStorage("stateflows")
                .UseInMemoryReminderService()
                .AddStateflowsRuntime())
            .Build();
        await host.StartAsync();
        try
        {
            using var scope = host.Services.CreateScope();
            var locator = scope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
            Assert.IsTrue(locator.TryLocateBehavior(new EntityId("failing-initialization", "one"), out var initializing));
            using (initializing)
            {
                Assert.AreEqual(EventStatus.Failed, (await initializing.SendAsync(new Initialize())).Status);
                CollectionAssert.Contains(handler.Calls.ToArray(), "initialization:False");
            }

            Assert.IsTrue(locator.TryLocateBehavior(new EntityId("failing-counter", "one"), out var behavior));
            using (behavior)
            {
                Assert.AreEqual(EventStatus.Consumed, (await behavior.SendAsync(new AddCounter { Amount = 3 })).Status);
                foreach (var amount in new[] { -1, -2, -3 })
                {
                    Assert.AreEqual(EventStatus.Failed, (await behavior.SendAsync(new AddCounter { Amount = amount })).Status);
                    var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
                    await management.ForceActivationCollection(TimeSpan.Zero);
                    var value = await behavior.RequestAsync(new ProjectionRequest<ObservedCounterView>());
                    Assert.AreEqual(3, value.Response!.Value, "Handled exceptions must not commit partial state.");
                }

                CollectionAssert.AreEqual(new[] { "initialization:False", "mutation", "computed", "projection" }, handler.Calls.ToArray());
            }
        }
        finally
        {
            await host.StopAsync();
        }
    }

    public interface IFailingCounter
    {
        [Field]
        int Count { get; set; }

        [Field]
        int Computed => Count == -2 ? throw new InvalidOperationException("computed") : Count;

        [Projection]
        ObservedCounterView View => Count == -3 ? throw new InvalidOperationException("projection") : new(Computed);

        [Mutation]
        void Change(AddCounter mutation)
        {
            Count = mutation.Amount;
            if (Count == -1)
            {
                throw new InvalidOperationException("mutation");
            }
        }
    }

    private sealed class EntityFailureHandler : IEntityExceptionHandler
    {
        public ConcurrentQueue<string> Calls { get; } = new();

        public async Task<bool> OnEntityInitializationExceptionAsync(IEntityInitializationExceptionContext context, Exception exception)
        {
            await Task.Yield();
            Calls.Enqueue($"initialization:{context.ImplicitInitialization}");
            return true;
        }

        public async Task<bool> OnMutationExceptionAsync<TTemplate, TMutationEvent>(
            IMutationObservationContext<TTemplate, TMutationEvent> context, Exception exception)
        {
            await Task.Yield();
            Calls.Enqueue("mutation");
            return true;
        }

        public async Task<bool> OnComputedFieldExceptionAsync<TTemplate, TField>(
            IComputedFieldObservationContext<TTemplate> context, Exception exception)
        {
            await Task.Yield();
            Calls.Enqueue("computed");
            return true;
        }

        public async Task<bool> OnProjectionExceptionAsync<TTemplate, TProjection>(
            IProjectionObservationContext<TTemplate> context, Exception exception)
        {
            await Task.Yield();
            Calls.Enqueue("projection");
            return true;
        }
    }

    public sealed class BlockCounter;

    public sealed record ObservedCounterView(int Value);

    public interface IObservedCounter
    {
        [Field]
        int Count { get; set; }

        [Field]
        int Doubled => Count * 2;

        [Projection]
        ObservedCounterView View => new(Doubled);

        [Mutation]
        void Add(AddCounter mutation) => Count += mutation.Amount;
    }

    private sealed class EntityHookProbe : IEntityObserver, IEntityInterceptor
    {
        public ConcurrentQueue<string> Operations { get; } = new();

        public async Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
        {
            await Task.Yield();
            return context.Event is BlockCounter ? EventStatus.Rejected : await next();
        }

        public async Task OnMutationAsync<TTemplate, TMutationEvent>(
            IMutationObservationContext<TTemplate, TMutationEvent> context, Func<Task> next)
        {
            await Task.Yield();
            await next();
            await Task.Yield();
            Operations.Enqueue("mutation:after");
        }

        public async Task OnComputedFieldAsync<TTemplate, TField>(
            IComputedFieldObservationContext<TTemplate> context, Func<Task<TField>> next)
        {
            await Task.Yield();
            var value = await next();
            await Task.Yield();
            Operations.Enqueue($"field:{value}");
        }

        public async Task OnProjectionAsync<TTemplate, TProjection>(
            IProjectionObservationContext<TTemplate> context, Func<Task<TProjection>> next)
        {
            await Task.Yield();
            var value = await next();
            await Task.Yield();
            Assert.IsInstanceOfType<ObservedCounterView>(value);
            if (value is ObservedCounterView view)
            {
                Operations.Enqueue($"projection:{view.Value}");
            }
        }
    }
}
