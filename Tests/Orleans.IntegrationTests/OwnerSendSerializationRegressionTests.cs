using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans.Hosting;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

[TestClass]
[DoNotParallelize]
public sealed class OwnerSendSerializationRegressionTests
{
    private static readonly TimeSpan Deadline = TimeSpan.FromSeconds(20);

    [TestMethod]
    [DataRow("ambiguous")]
    [DataRow("unmatched")]
    [DataRow("annotated-unmatched")]
    [DataRow("nested-unmatched")]
    public async Task DefectiveRegisteredEventPreventsStartup(string contract)
    {
        using var host = contract switch
        {
            "ambiguous" => CreateHost<DetachedFailure>(_ => Task.CompletedTask, validate: true),
            "unmatched" => CreateHost<EventSerializationRegressionTests.UnmatchedFailure>(_ => Task.CompletedTask, validate: true),
            "annotated-unmatched" => CreateHost<EventSerializationRegressionTests.AnnotatedUnmatchedFailure>(_ => Task.CompletedTask, validate: true),
            "nested-unmatched" => CreateHost<EventSerializationRegressionTests.NestedFailure>(_ => Task.CompletedTask, validate: true),
            _ => throw new ArgumentOutOfRangeException(nameof(contract))
        };
        Exception? startupFailure = null;
        try
        {
            await host.StartAsync().WaitAsync(Deadline);
        }
        catch (Exception exception) when (exception is not TimeoutException)
        {
            startupFailure = exception;
        }
        finally
        {
            if (startupFailure == null)
                await host.StopAsync().WaitAsync(Deadline);
        }

        Assert.IsNotNull(startupFailure, "A defective registered event must fail startup.");
        var expectedType = contract switch
        {
            "ambiguous" => typeof(DetachedFailure),
            "unmatched" => typeof(EventSerializationRegressionTests.UnmatchedFailure),
            "annotated-unmatched" => typeof(EventSerializationRegressionTests.AnnotatedUnmatchedFailure),
            _ => typeof(EventSerializationRegressionTests.NestedFailure)
        };
        StringAssert.Contains(startupFailure.ToString(), expectedType.Name);
    }

    [TestMethod]
    public async Task OwnerSendRuntimeSerializationFailureIsLoggedWithoutBlockingEffect()
    {
        var effectReturned = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var received = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var logs = new FailureLogs();
        using var host = CreateHost<RuntimeFailure>(context =>
        {
            Assert.IsTrue(context.TryGetOwnerBehaviorContext(out var owner));
            owner.Send(new RuntimeFailure());
            effectReturned.TrySetResult();
            return Task.CompletedTask;
        }, _ => received.TrySetResult(), logs);
        await host.StartAsync().WaitAsync(Deadline);
        try
        {
            using var scope = host.Services.CreateScope();
            using var behavior = scope.ServiceProvider.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new StateMachineId("owner-send-regression", "failure"));
            Assert.AreEqual(EventStatus.Initialized,
                (await behavior.SendAsync(new Initialize()).WaitAsync(Deadline)).Status);

            Assert.AreEqual(EventStatus.Consumed,
                (await behavior.SendAsync(new Trigger()).WaitAsync(Deadline)).Status);
            await effectReturned.Task.WaitAsync(Deadline);
            // Shutdown drains the action's work before checking for error reports.
            await host.StopAsync().WaitAsync(Deadline);
            Assert.IsFalse(received.Task.IsCompleted);
            Assert.IsTrue(logs.Errors.Any(entry =>
                entry.Contains(nameof(RuntimeFailure), StringComparison.Ordinal) &&
                entry.Contains("Injected serialization failure", StringComparison.Ordinal)),
                "A detached owner-send failure must log the event type and original error.");
        }
        finally
        {
            await host.StopAsync().WaitAsync(Deadline);
        }
    }

    [TestMethod]
    public async Task OwnerSendWithMatchingConstructorPreservesMessages()
    {
        var payload = await SendFromEffectAsync(() =>
            new EventSerializationRegressionTests.MatchingFailure(["failure"], "effect"));
        CollectionAssert.AreEqual(new[] { "failure" }, payload.Messages);
        Assert.AreEqual("effect", payload.MethodContext);
    }

    private static async Task<T> SendFromEffectAsync<T>(Func<T> createEvent)
    {
        var received = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var host = CreateHost<T>(context =>
        {
            Assert.IsTrue(context.TryGetOwnerBehaviorContext(out var owner));
            owner.Send(createEvent());
            return Task.CompletedTask;
        }, payload => received.TrySetResult(payload));
        await host.StartAsync().WaitAsync(Deadline);
        try
        {
            using var scope = host.Services.CreateScope();
            using var behavior = scope.ServiceProvider.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new StateMachineId("owner-send-regression", "payload"));
            Assert.AreEqual(EventStatus.Initialized,
                (await behavior.SendAsync(new Initialize()).WaitAsync(Deadline)).Status);
            Assert.AreEqual(EventStatus.Consumed,
                (await behavior.SendAsync(new Trigger()).WaitAsync(Deadline)).Status);
            return await received.Task.WaitAsync(Deadline);
        }
        finally
        {
            await host.StopAsync().WaitAsync(Deadline);
        }
    }

    private static IHost CreateHost<T>(
        Stateflows.Actions.Registration.ActionDelegateAsync effect,
        Action<T>? onReceived = null, FailureLogs? logs = null, bool validate = false)
    {
        var listeners = new[] { new TcpListener(IPAddress.Loopback, 0), new TcpListener(IPAddress.Loopback, 0) };
        int[] ports;
        try
        {
            foreach (var listener in listeners)
                listener.Start();
            ports = listeners.Select(listener => ((IPEndPoint)listener.LocalEndpoint).Port).ToArray();
        }
        finally
        {
            foreach (var listener in listeners)
                listener.Stop();
        }
        var cluster = Guid.NewGuid().ToString("N");
        return Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders().SetMinimumLevel(LogLevel.Error);
                if (logs != null)
                    logging.AddProvider(logs);
            })
            .ConfigureServices(services => services.AddStateflows(builder =>
            {
                if (validate) builder.AddStartupSerializationValidation();
                builder
                .AddStateMachines(machines => machines.AddStateMachine("owner-send-regression", machine => machine
                    .AddInitialState("ready", state => state
                        .AddInternalTransition<Trigger>(transition => transition
                            .AddEffectAction(effect, action => action.AddProducedEvent<T>()))
                        .AddInternalTransition<T>(transition => transition.AddEffect(context =>
                        {
                            onReceived?.Invoke(context.Event);
                        })))));
            }))
            .UseOrleans(silo => silo
                .UseLocalhostClustering(ports[0], ports[1], clusterId: cluster, serviceId: cluster)
                .AddMemoryGrainStorageAsDefault()
                .UseInMemoryReminderService()
                .AddStateflowsRuntime())
            .Build();
    }

    public sealed class Trigger;

    public sealed class RuntimeFailure
    {
        public string Message => throw new JsonSerializationException("Injected serialization failure");
    }

    public sealed class DetachedFailure
    {
        public DetachedFailure(List<string> errorMessages)
        {
            Messages = errorMessages;
            Exceptions = [];
        }

        public DetachedFailure(List<Exception> errorList)
        {
            Exceptions = errorList;
            Messages = [];
        }

        public List<string> Messages { get; }
        public List<Exception> Exceptions { get; }
    }

    private sealed class FailureLogs : ILoggerProvider
    {
        public ConcurrentQueue<string> Errors { get; } = new();
        public ILogger CreateLogger(string categoryName) => new Logger(Errors);
        public void Dispose() { }

        private sealed class Logger(ConcurrentQueue<string> errors) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;
            public void Log<TState>(LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId,
                TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (IsEnabled(logLevel))
                    errors.Enqueue($"{formatter(state, exception)} {exception}");
            }
        }
    }
}
