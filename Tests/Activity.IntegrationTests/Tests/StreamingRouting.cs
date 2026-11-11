using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Registration;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class StreamingRouting
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task InjectedExecutionContextExposesInvocationModeAndOptionalEventMetadata(bool streamed)
    {
        var probe = new ModeProbe();
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("inspect"))
            .AddAction<ModeAction>("inspect"), services =>
            {
                services.AddSingleton(probe);
                services.AddSingleton<IBehaviorInterceptor>(probe);
            });

        await harness.FireInputAsync([new TokenHolder<int> { Payload = 42, IsStreamed = streamed }]);

        Assert.IsNotNull(probe.Context);
        Assert.AreEqual(streamed ? ExecutionMode.Streamed : ExecutionMode.Transactional, probe.Context.ExecutionMode);
        Assert.AreEqual(42, probe.Value);
        Assert.AreEqual(1, probe.Publications);
        Assert.IsNotNull(probe.PublicationContext);
        Assert.AreEqual(probe.Context.ExecutionMode, probe.PublicationContext.ExecutionMode);
        Assert.AreSame(probe.Context.ExecutionTrigger, probe.PublicationContext.ExecutionTrigger);
        Assert.AreEqual(probe.Context.ExecutionTriggerId, probe.PublicationContext.ExecutionTriggerId);
        Assert.AreSame(probe.Context.Headers, probe.PublicationContext.Headers);
        if (streamed)
        {
            Assert.IsNull(probe.Context.ExecutionTrigger);
            Assert.IsNull(probe.Context.ExecutionTriggerId);
            Assert.IsNull(probe.Context.Headers);
        }
        else
        {
            Assert.IsInstanceOfType<TokensInputEvent>(probe.Context.ExecutionTrigger);
            Assert.IsNotNull(probe.Context.ExecutionTriggerId);
            Assert.IsNotNull(probe.Context.Headers);
        }
    }

    public sealed class ModeProbe : Stateflows.Common.Classes.BehaviorInterceptor
    {
        public IExecutionContext? Context { get; set; }
        public int Value { get; set; }
        public int Publications { get; private set; }
        public IExecutionContext? PublicationContext { get; private set; }

        public override Task NotificationPublishedAsync<TNotification>(
            IBehaviorActionContext context, TNotification notification, IDictionary<string, EventHeader> headers)
        {
            if (notification is ModeNotice)
            {
                Publications++;
                PublicationContext = context;
            }
            return Task.CompletedTask;
        }
    }

    public sealed class ModeNotice;

    public sealed class ModeAction(IExecutionContext context, IBehaviorContext behavior, IInputTokens<int> input, ModeProbe probe) : IActionNode
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var mode = context.ExecutionMode;
            var trigger = context.ExecutionTrigger;
            var id = context.ExecutionTriggerId;
            var headers = context.Headers;
            await Task.Yield();
            Assert.AreEqual(mode, context.ExecutionMode);
            Assert.AreSame(trigger, context.ExecutionTrigger);
            Assert.AreEqual(id, context.ExecutionTriggerId);
            Assert.AreSame(headers, context.Headers);
            probe.Context = context;
            probe.Value = input.First();
            behavior.Publish(new ModeNotice());
        }
    }

    [TestMethod]
    [DataRow("transformation", false)]
    [DataRow("transformation", true)]
    [DataRow("control", false)]
    [DataRow("control", true)]
    [DataRow("zero-weight", false)]
    [DataRow("zero-weight", true)]
    [DataRow("exception", false)]
    [DataRow("exception", true)]
    public async Task RoutingKeepsFiringProvenanceThroughPipelinesControlAndRecovery(string route, bool streamed)
    {
        bool? observed = null;
        bool? guardColor = null;
        var received = Array.Empty<string>();
        var handlerExecutions = 0;
        await using var harness = await Harness.CreateAsync(activity =>
        {
            activity.AddInputPin<int>(input => input.AddFlow("produce"));
            activity.AddAction("produce", context =>
            {
                if (route == "exception")
                    throw new InvalidOperationException("expected");
                if (route == "transformation")
                    context.Output(42);
                return Task.CompletedTask;
            }, node =>
            {
                if (route == "transformation")
                    node.AddFlow<int>("consume", flow => flow
                        .AddTransformation<string>(context => Task.FromResult($"value:{context.Token}"))
                        .AddGuard(context =>
                        {
                            guardColor = ((BaseContext)context).Context.IsCurrentInvocationStreamed;
                            return Task.FromResult(context.Token == "value:42");
                        }));
                else if (route == "control")
                    node.AddControlFlow("consume");
                else if (route == "zero-weight")
                    node.AddFlow<string>("consume", flow => flow.SetWeight(0));
                else
                    node.AddExceptionHandler<InvalidOperationException>(context =>
                    {
                        handlerExecutions++;
                        context.Output($"recovered:{context.Exception.Message}");
                        return Task.CompletedTask;
                    }).AddFlow<string>("consume");
            });
            activity.AddAction("consume", context =>
            {
                Assert.AreEqual(streamed ? ExecutionMode.Streamed : ExecutionMode.Transactional, context.ExecutionMode);
                if (streamed)
                {
                    Assert.IsNull(context.ExecutionTrigger);
                    Assert.IsNull(context.ExecutionTriggerId);
                    Assert.IsNull(context.Headers);
                }
                else
                {
                    Assert.IsNotNull(context.ExecutionTrigger);
                    Assert.IsNotNull(context.ExecutionTriggerId);
                    Assert.IsNotNull(context.Headers);
                }
                observed = ((ActionContext)context).IsStreamed;
                received = context.GetTokensOfType<string>().ToArray();
                return Task.CompletedTask;
            });
        });

        await harness.FireInputAsync([new TokenHolder<int> { Payload = 1, IsStreamed = streamed }]);

        Assert.AreEqual(streamed, observed, route);
        if (route == "transformation")
        {
            Assert.AreEqual(streamed, guardColor);
            CollectionAssert.AreEqual(new[] { "value:42" }, received);
        }
        else if (route == "exception")
        {
            Assert.AreEqual(1, handlerExecutions);
            CollectionAssert.AreEqual(new[] { "recovered:expected" }, received);
        }
        else
            Assert.AreEqual(0, received.Length, "A control or optional activation must not synthesize object tokens.");
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ControlOnlyStreamedInputColorsAJoinWithTransactionalObjectInputs(bool streamedControl)
    {
        bool? observed = null;
        var received = Array.Empty<int>();
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("objects"))
            .AddAction("objects", context =>
            {
                context.PassAllTokensOn();
                return Task.CompletedTask;
            }, node => node.AddFlow<int>("consume"))
            .AddAcceptEventAction<Pulse>("control", _ => Task.CompletedTask,
                node => node.AddControlFlow("consume"))
            .AddAction("consume", context =>
            {
                observed = ((ActionContext)context).IsStreamed;
                received = context.GetTokensOfType<int>().ToArray();
                return Task.CompletedTask;
            }));

        await harness.FireInputAsync([42.ToTokenHolder()]);
        Assert.IsNull(observed, "The implicit join must wait for the missing control input.");
        var node = harness.Executor.Graph.AllNamedNodes["consume"];
        var controlEdge = node.IncomingEdges.Single(edge => edge.TokenType == typeof(ControlToken));
        harness.Root.GetStream(controlEdge.Identifier, harness.Executor.NodeScope.ThreadId).Consume(
            new TokenHolder<ControlToken> { Payload = new ControlToken(), IsStreamed = streamedControl });
        await harness.FireNodeAsync("consume");

        Assert.AreEqual(streamedControl, observed);
        CollectionAssert.AreEqual(new[] { 42 }, received);
    }

    [TestMethod]
    public async Task ForkedRoutesAndTransformationsDoNotMutateTheSourceEnvelope()
    {
        var source = new TokenHolder<int> { Payload = 42, IsStreamed = true };
        var deliveries = new ConcurrentDictionary<string, TokenHolder>();
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("fork"))
            .AddAction("fork", context =>
            {
                context.PassAllTokensOn();
                return Task.CompletedTask;
            }, node => node.AddFlow<int>("left").AddFlow<int>("right", flow =>
                flow.AddTransformation<string>(context => Task.FromResult(context.Token.ToString()))))
            .AddAction("left", context =>
            {
                deliveries["left"] = ((ActionContext)context).InputTokens.Single();
                return Task.CompletedTask;
            })
            .AddAction("right", context =>
            {
                deliveries["right"] = ((ActionContext)context).InputTokens.Single();
                return Task.CompletedTask;
            }));

        await harness.FireInputAsync([source]);
        Assert.AreEqual(2, deliveries.Count);
        Assert.AreEqual(source.Id, deliveries["left"].Id);
        Assert.AreEqual(42, deliveries["left"].BoxedPayload);
        Assert.AreEqual("42", deliveries["right"].BoxedPayload);
        Assert.IsTrue(deliveries.Values.All(token => token.IsStreamed));
        Assert.AreNotSame(source, deliveries["left"]);
        Assert.AreNotSame(deliveries["left"], deliveries["right"]);
        Assert.AreEqual(42, source.Payload);
        Assert.IsTrue(source.IsStreamed);
    }

    [TestMethod]
    [DataRow(false, 1)]
    [DataRow(true, 2)]
    public async Task JoinRetainsDifferentlyColoredOccurrencesOfTheSameLogicalToken(bool mixed, int expectedCount)
    {
        bool? observed = null;
        var received = Array.Empty<TokenHolder>();
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("left").AddFlow("right"))
            .AddAction("left", _ => Task.CompletedTask, node => node.AddFlow<int>("consume"))
            .AddAction("right", _ => Task.CompletedTask, node => node.AddFlow<int>("consume"))
            .AddAction("consume", context =>
            {
                observed = ((ActionContext)context).IsStreamed;
                received = ((ActionContext)context).InputTokens.ToArray();
                return Task.CompletedTask;
            }));
        var source = 42.ToTokenHolder();
        var edges = harness.Executor.Graph.AllNamedNodes["consume"].IncomingEdges.ToArray();
        var left = harness.Root.GetStream(edges[0].Identifier, harness.Executor.NodeScope.ThreadId);
        left.Consume([source.CopyForExecution(false)], isPersistent: false);
        left.IsActivated = true;
        var right = harness.Root.GetStream(edges[1].Identifier, harness.Executor.NodeScope.ThreadId);
        right.Consume([source.CopyForExecution(mixed)], isPersistent: false);
        right.IsActivated = true;

        await harness.FireNodeAsync("consume");

        Assert.AreEqual(mixed, observed);
        Assert.AreEqual(expectedCount, received.Length);
        Assert.IsTrue(received.All(token => token.Id == source.Id));
        Assert.IsTrue(received.Any(token => !token.IsStreamed));
        Assert.AreEqual(mixed, received.Any(token => token.IsStreamed));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task DataStoreRecolorsRetainedTokensOnlyDuringStreamingBusyPeriod(bool busy)
    {
        bool? observed = null;
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("store"))
            .AddDataStore("store", store => store.AddFlow<int>("consume"))
            .AddAction("consume", context =>
            {
                observed = ((ActionContext)context).IsStreamed;
                return Task.CompletedTask;
            }));
        if (busy)
            harness.Root.BeginStreamingBusyPeriod();

        await harness.FireInputAsync([42.ToTokenHolder()]);
        Assert.AreEqual(busy, observed);
        var storeEdge = harness.Executor.Graph.AllNamedNodes["store"].Edges.Single();
        var retained = harness.Root.GetStream(storeEdge.Identifier, harness.Executor.NodeScope.ThreadId);
        Assert.IsTrue(retained.IsPersistent);
        Assert.AreEqual(42, retained.Tokens.Single().BoxedPayload);
        Assert.AreEqual(busy, retained.Tokens.Single().IsStreamed);
    }

    public sealed class Pulse;

    private sealed class Harness(
        ServiceProvider provider, PendingTasksScope pending, Executor executor, RootContext root) : IAsyncDisposable
    {
        public Executor Executor { get; } = executor;
        public RootContext Root { get; } = root;

        public static async Task<Harness> CreateAsync(Action<IActivityBuilder> configure, Action<IServiceCollection>? configureServices = null)
        {
            var pending = new PendingTasksScope();
            var services = new ServiceCollection().AddLogging();
            configureServices?.Invoke(services);
            var provider = services.AddStateflows(sf => sf.AddLegacyRuntime().AddActivities(activities =>
                    activities.AddActivity("routing-provenance", activity => configure(activity))))
                .BuildServiceProvider();
            provider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
                provider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            var register = provider.GetRequiredService<ActivitiesRegister>();
            var executor = new Executor(register, register.Activities["routing-provenance.current"], provider);
            var root = new RootContext(new StateflowsContext(
                new ActivityId("routing-provenance", Guid.NewGuid().ToString("N"))));
            await executor.HydrateAsync(root);
            return new Harness(provider, pending, executor, root);
        }

        public Task FireInputAsync(IEnumerable<TokenHolder> tokens)
            => FireAsync(Executor.Graph.InputPins.Single(), tokens);

        public Task FireNodeAsync(string name)
            => FireAsync(Executor.Graph.AllNamedNodes[name], null);

        private async Task FireAsync(Stateflows.Activities.Models.Node node, IEnumerable<TokenHolder>? tokens)
        {
            provider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
                provider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            using var tasks = new PendingTasksScope();
            using var execution = Root.BeginEventExecution();
            var holder = new TokensInputEvent().ToEventHolder();
            using var trigger = Root.Context.PushExecutionTriggerHolder(holder);
            Root.SetEvent(holder);
            try
            {
                await Executor.DoHandleNodeAsync(node, null!, Executor.NodeScope, tokens).WaitAsync(TimeSpan.FromSeconds(5));
            }
            finally
            {
                Root.ClearEvent();
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await Executor.DisposeAsync();
                await provider.DisposeAsync();
            }
            finally
            {
                pending.Dispose();
            }
        }
    }
}
