using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Common.Context;
using Stateflows.Entities.Attributes;

namespace Entity.IntegrationTests.Tests
{
    public record HandlingSetValue(string Value, bool Throw = false);
    public record HandlingCustomInitialize(string Value);
    public record HandlingOtherEvent;

    public record HandlingProjection(string Value);
    public record HandlingRequestProjection(string Value);

    public interface IExceptionHandlingTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string Value { get; set; }

        [Field(FieldAccess.Get | FieldAccess.Set)]
        int Unrelated { get; set; }

        [Field]
        int Length => Value == "computed-failure"
            ? throw new InvalidOperationException("computed failure")
            : Value.Length;

        [Projection]
        HandlingProjection Snapshot => Value == "projection-failure"
            ? throw new InvalidOperationException("projection failure")
            : new HandlingProjection(Value);

        [Mutation]
        void SetValue(HandlingSetValue change)
        {
            Value = change.Value;
            if (change.Throw)
                throw new InvalidOperationException("mutation failure");
        }
    }

    public interface IExceptionInitializationTemplate
    {
        [Field]
        string Value { get; set; }
    }

    public sealed record ExceptionHandlerCall(
        string Handler,
        string Hook,
        string EntityName,
        object? Trigger,
        object? OperationEvent,
        bool? ImplicitInitialization,
        Exception Exception);

    public sealed class ExceptionHandlingLog
    {
        public List<ExceptionHandlerCall> Calls { get; } = new();
        public List<(string Handler, string EntityName)> Factories { get; } = new();
        public List<Exception> SecondaryExceptions { get; } = new();
        public List<Exception> LoggedExceptions { get; } = new();
        public List<Exception> ObserverExceptions { get; } = new();
        public HashSet<string> HandledBy { get; } = new();
        public string? ThrowFromHandler { get; set; }
        public string? ThrowForHook { get; set; }

        public bool Record(
            string handler,
            string hook,
            IBehaviorActionContext context,
            Exception exception,
            object? operationEvent = null,
            bool? implicitInitialization = null)
        {
            Calls.Add(new ExceptionHandlerCall(
                handler,
                hook,
                context.Behavior.Id.Name,
                context.ExecutionTrigger,
                operationEvent,
                implicitInitialization,
                exception));

            if (handler == ThrowFromHandler && hook == ThrowForHook)
            {
                var secondary = new ApplicationException("exception handler failed");
                SecondaryExceptions.Add(secondary);
                throw secondary;
            }

            return HandledBy.Contains(handler);
        }

        public void Clear()
        {
            Calls.Clear();
            Factories.Clear();
            SecondaryExceptions.Clear();
            LoggedExceptions.Clear();
            ObserverExceptions.Clear();
        }
    }

    public sealed class ExceptionHandlingObserver(ExceptionHandlingLog log) : IEntityObserver
    {
        private async Task<T> ObserveAsync<T>(string hook, Func<Task<T>> next)
        {
            try
            {
                return await next();
            }
            catch (Exception exception)
            {
                Assert.IsTrue(log.Calls.Any(call =>
                    call.Hook == hook && ReferenceEquals(call.Exception, exception)),
                    "Handlers must see the operation failure before observers catch it.");
                log.ObserverExceptions.Add(exception);
                throw;
            }
        }

        public Task OnMutationAsync<TTemplate, TMutationEvent>(
            IMutationObservationContext<TTemplate, TMutationEvent> context, Func<Task> next)
            => ObserveAsync("mutation", async () =>
            {
                await next();
                return true;
            });

        public Task OnComputedFieldAsync<TTemplate, TField>(
            IComputedFieldObservationContext<TTemplate> context, Func<Task<TField>> next)
            => ObserveAsync("computed", next);

        public Task OnProjectionAsync<TTemplate, TProjection>(
            IProjectionObservationContext<TTemplate> context, Func<Task<TProjection>> next)
            => ObserveAsync("projection", next);
    }

    public sealed class ExceptionHandlingLogProvider(ExceptionHandlingLog log) : ILoggerProvider
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName) => new ExceptionLogger(log);

        private sealed class ExceptionLogger(ExceptionHandlingLog log) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;

            public void Log<TState>(
                LogLevel logLevel,
                Microsoft.Extensions.Logging.EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (exception is not null)
                    log.LoggedExceptions.Add(exception);
            }
        }
    }

    public abstract class RecordingEntityExceptionHandler(ExceptionHandlingLog log, string name)
        : IEntityExceptionHandler
    {
        public async Task<bool> OnEntityInitializationExceptionAsync(
            IEntityInitializationExceptionContext context,
            Exception exception)
        {
            await Task.Yield();
            return log.Record(
                name, "initialization", context, exception,
                implicitInitialization: context.ImplicitInitialization);
        }

        public async Task<bool> OnMutationExceptionAsync<TTemplate, TMutationEvent>(
            IMutationObservationContext<TTemplate, TMutationEvent> context,
            Exception exception)
        {
            await Task.Yield();
            return log.Record(name, "mutation", context, exception, context.MutationEvent);
        }

        public async Task<bool> OnComputedFieldExceptionAsync<TTemplate, TField>(
            IComputedFieldObservationContext<TTemplate> context,
            Exception exception)
        {
            await Task.Yield();
            if (context.Entity is IExceptionHandlingTemplate entity)
                _ = entity.Unrelated;
            return log.Record(name, "computed", context, exception);
        }

        public async Task<bool> OnProjectionExceptionAsync<TTemplate, TProjection>(
            IProjectionObservationContext<TTemplate> context,
            Exception exception)
        {
            await Task.Yield();
            if (context.Entity is IExceptionHandlingTemplate entity)
                _ = entity.Unrelated;
            return log.Record(name, "projection", context, exception);
        }
    }

    public sealed class GlobalDiExceptionHandler(ExceptionHandlingLog log)
        : RecordingEntityExceptionHandler(log, "global-di");

    public sealed class LocalDiExceptionHandler(ExceptionHandlingLog log)
        : RecordingEntityExceptionHandler(log, "local-di");

    public sealed class GlobalSyncExceptionHandler(ExceptionHandlingLog log)
        : RecordingEntityExceptionHandler(log, "global-sync");

    public sealed class GlobalAsyncExceptionHandler(ExceptionHandlingLog log)
        : RecordingEntityExceptionHandler(log, "global-async");

    public sealed class LocalSyncExceptionHandler(ExceptionHandlingLog log)
        : RecordingEntityExceptionHandler(log, "local-sync");

    public sealed class LocalAsyncExceptionHandler(ExceptionHandlingLog log)
        : RecordingEntityExceptionHandler(log, "local-async");

    [TestClass]
    public class ExceptionHandling : StateflowsTestClass
    {
        private readonly ExceptionHandlingLog log = new();
        private readonly ExceptionHandlingLogProvider logProvider;
        private bool throwProjectionOnRequest;

        public ExceptionHandling()
        {
            logProvider = new ExceptionHandlingLogProvider(log);
        }

        [TestInitialize]
        public override void Initialize()
        {
            ServiceCollection.AddSingleton(log);
            ServiceCollection.AddLogging(logging => logging.AddProvider(logProvider).SetMinimumLevel(LogLevel.Error));
            base.Initialize();
        }

        [TestCleanup]
        public override void Cleanup() => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder.AddEntities(entities => entities
                .AddObserver<ExceptionHandlingObserver>()
                .AddExceptionHandler<GlobalDiExceptionHandler>()
                .AddExceptionHandler((services, context) =>
                {
                    Assert.AreSame(log, services.GetRequiredService<ExceptionHandlingLog>());
                    log.Factories.Add(("global-sync", context.Behavior.Id.Name));
                    return new GlobalSyncExceptionHandler(log);
                })
                .AddExceptionHandler(async (services, context) =>
                {
                    await Task.Yield();
                    Assert.AreSame(log, services.GetRequiredService<ExceptionHandlingLog>());
                    log.Factories.Add(("global-async", context.Behavior.Id.Name));
                    return new GlobalAsyncExceptionHandler(log);
                })
                .AddEntity<IExceptionHandlingTemplate>("handled", entity => entity
                    .AddDefaultInitializer(context => context.Entity.Value = "initial")
                    .AddExceptionHandler<LocalDiExceptionHandler>()
                    .AddExceptionHandler((services, context) =>
                    {
                        Assert.AreSame(log, services.GetRequiredService<ExceptionHandlingLog>());
                        log.Factories.Add(("local-sync", context.Behavior.Id.Name));
                        return new LocalSyncExceptionHandler(log);
                    })
                    .AddExceptionHandler(async (services, context) =>
                    {
                        await Task.Yield();
                        Assert.AreSame(log, services.GetRequiredService<ExceptionHandlingLog>());
                        log.Factories.Add(("local-async", context.Behavior.Id.Name));
                        return new LocalAsyncExceptionHandler(log);
                    })
                )
                .AddEntity<IExceptionHandlingTemplate>("globalOnly", entity =>
                    entity.AddDefaultInitializer(context => context.Entity.Value = "initial"))
                .AddEntity<IExceptionHandlingTemplate>("computedDuringInit", entity =>
                    entity.AddDefaultInitializer(context => context.Entity.Value = "computed-failure"))
                .AddEntity<IExceptionHandlingTemplate>("projectionDuringInit", entity =>
                    entity.AddDefaultInitializer(context => context.Entity.Value = "projection-failure"))
                .AddEntity<IExceptionHandlingTemplate>("projectionOnRequest", entity => entity
                    .AddDefaultInitializer(context => context.Entity.Value = "initial")
                    .AddProjection<HandlingRequestProjection>(value =>
                        throwProjectionOnRequest
                            ? throw new InvalidOperationException("projection request failure")
                            : new HandlingRequestProjection(value.Value))
                )
                .AddEntity<IExceptionInitializationTemplate>("explicitInit", entity =>
                    entity.AddDefaultInitializer(context =>
                    {
                        context.Entity.Value = "partial";
                        throw new InvalidOperationException("explicit initialization failure");
                    }))
                .AddEntity<IExceptionInitializationTemplate>("customInit", entity =>
                    entity.AddInitializer<HandlingCustomInitialize>(context =>
                    {
                        context.Entity.Value = context.InitializationEvent.Value;
                        throw new InvalidOperationException("custom initialization failure");
                    }))
                .AddEntity<IExceptionInitializationTemplate>("implicitInit", entity =>
                    entity.AddDefaultInitializer(context =>
                    {
                        context.Entity.Value = "partial";
                        throw new InvalidOperationException("implicit initialization failure");
                    }))
            );
        }

        private IEntityBehavior Locate(string name, string instance = "x")
        {
            var locator = ServiceProvider.GetRequiredService<IEntityLocator>();
            Assert.IsTrue(locator.TryLocateEntity(new EntityClass(name).ToId(instance), out var entity));
            return entity;
        }

        private async Task<StateflowsContext> HydrateAsync(string name, string instance = "x")
        {
            var tenantAccessor = ServiceProvider.GetRequiredService<ITenantAccessor>();
            tenantAccessor.CurrentTenantId = ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            return await ServiceProvider.GetRequiredService<IStateflowsStorage>()
                .HydrateAsync(new EntityClass(name).ToId(instance));
        }

        private void AssertHandlerOrder(string hook, params string[] names)
        {
            CollectionAssert.AreEqual(
                names,
                log.Calls.Where(call => call.Hook == hook).Select(call => call.Handler).ToArray());
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task Handlers_RunGlobalThenLocalInRegistrationOrder_EvenWhenHandled(bool handled)
        {
            if (handled)
            {
                log.HandledBy.Add("global-di");
                log.HandledBy.Add("local-di");
            }
            var entity = Locate("handled");
            await entity.SendAsync(new Initialize());
            log.Clear();

            var mutation = new HandlingSetValue("partial", Throw: true);
            var result = await entity.SendAsync(mutation);

            Assert.AreEqual(EventStatus.Failed, result.Status,
                "A true handler result does not suppress the original operation failure.");
            AssertHandlerOrder("mutation",
                "global-di", "global-sync", "global-async",
                "local-di", "local-sync", "local-async");
            CollectionAssert.AreEquivalent(
                new[] { "global-sync", "global-async", "local-sync", "local-async" },
                log.Factories.Select(factory => factory.Handler).ToArray());
            Assert.IsTrue(log.Factories.All(factory => factory.EntityName == "handled"));
            Assert.AreSame(log.Calls[0].Exception, log.Calls[^1].Exception);
            Assert.AreEqual(mutation, log.Calls[0].OperationEvent);
            Assert.AreSame(log.Calls[0].Exception, log.ObserverExceptions.Single());
            var context = await HydrateAsync("handled");
            Assert.AreEqual("initial", context.Values["$field:Value"]);
        }

        [TestMethod]
        public async Task LocalHandlers_AreIsolatedToTheirEntityDefinition()
        {
            var entity = Locate("globalOnly");
            await entity.SendAsync(new Initialize());
            log.Clear();

            Assert.AreEqual(EventStatus.Failed,
                (await entity.SendAsync(new HandlingSetValue("initial", Throw: true))).Status);

            AssertHandlerOrder("mutation", "global-di", "global-sync", "global-async");
            Assert.IsTrue(log.Factories.All(factory => factory.EntityName == "globalOnly"));
        }

        [TestMethod]
        public async Task InitializationHandlers_ReceiveImplicitFlagAndTriggerMetadata()
        {
            var explicitResult = await Locate("explicitInit").SendAsync(new Initialize());
            var explicitCall = log.Calls.First(call => call.EntityName == "explicitInit");
            Assert.AreEqual(EventStatus.Failed, explicitResult.Status);
            Assert.AreEqual("initialization", explicitCall.Hook);
            Assert.IsFalse(explicitCall.ImplicitInitialization);
            Assert.IsInstanceOfType<Initialize>(explicitCall.Trigger);

            log.Clear();
            var customEvent = new HandlingCustomInitialize("custom");
            var customResult = await Locate("customInit").SendAsync(customEvent);
            var customCall = log.Calls.First(call => call.EntityName == "customInit");
            Assert.AreEqual(EventStatus.Failed, customResult.Status);
            Assert.IsFalse(customCall.ImplicitInitialization);
            Assert.AreEqual(customEvent, customCall.Trigger);

            log.Clear();
            var implicitEvent = new HandlingOtherEvent();
            var implicitResult = await Locate("implicitInit").SendAsync(implicitEvent);
            var implicitCall = log.Calls.First(call => call.EntityName == "implicitInit");
            Assert.AreEqual(EventStatus.Failed, implicitResult.Status);
            Assert.IsTrue(implicitCall.ImplicitInitialization);
            Assert.AreEqual(implicitEvent, implicitCall.Trigger);

            foreach (var name in new[] { "explicitInit", "customInit", "implicitInit" })
            {
                var context = await HydrateAsync(name);
                Assert.AreNotEqual(BehaviorStatus.Initialized, context.Status);
                Assert.IsFalse(context.Values.ContainsKey("$field:Value"));
            }
        }

        [TestMethod]
        public async Task HandlerDispatch_CoversMutationComputationAndProjectionFailures()
        {
            var entity = Locate("handled");
            await entity.SendAsync(new Initialize());
            log.Clear();
            Assert.AreEqual(EventStatus.Failed,
                (await entity.SendAsync(new HandlingSetValue("initial", Throw: true))).Status);
            AssertHandlerOrder("mutation",
                "global-di", "global-sync", "global-async",
                "local-di", "local-sync", "local-async");

            log.Clear();
            Assert.AreEqual(EventStatus.Failed,
                (await entity.SendAsync(new HandlingSetValue("computed-failure"))).Status);
            AssertHandlerOrder("computed",
                "global-di", "global-sync", "global-async",
                "local-di", "local-sync", "local-async");
            Assert.AreEqual(0, log.Calls.Count(call => call.Hook == "mutation"),
                "A failed dependent computation is not a mutation-delegate exception.");
            Assert.AreEqual("initial", (await HydrateAsync("handled")).Values["$field:Value"]);

            log.Clear();
            Assert.IsFalse(await entity.TrySetAsync(nameof(IExceptionHandlingTemplate.Value), "computed-failure"));
            AssertHandlerOrder("computed",
                "global-di", "global-sync", "global-async",
                "local-di", "local-sync", "local-async");
            Assert.AreEqual(0, log.Calls.Count(call => call.Hook == "mutation"),
                "Dependency recalculation failures must not be reported as mutation failures.");
            Assert.AreEqual("initial", (await HydrateAsync("handled")).Values["$field:Value"]);

            log.Clear();
            Assert.IsFalse(await entity.TrySetAsync(nameof(IExceptionHandlingTemplate.Value), "projection-failure"));
            AssertHandlerOrder("projection",
                "global-di", "global-sync", "global-async",
                "local-di", "local-sync", "local-async");
            Assert.AreEqual(0, log.Calls.Count(call => call.Hook == "mutation"));
            Assert.AreEqual("initial", (await HydrateAsync("handled")).Values["$field:Value"]);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task InitializationFailures_AlsoDispatchComputedAndProjectionHooks(bool implicitInitialization)
        {
            object trigger = implicitInitialization ? new HandlingOtherEvent() : new Initialize();
            var computedStatus = await Locate("computedDuringInit").SendAsync(trigger);
            Assert.AreEqual(EventStatus.Failed, computedStatus.Status);
            CollectionAssert.AreEquivalent(
                new[] { "computed", "initialization" },
                log.Calls.Where(call => call.EntityName == "computedDuringInit").Select(call => call.Hook).Distinct().ToArray());
            Assert.IsTrue(log.Calls.Where(call => call.Hook == "initialization")
                .All(call => call.ImplicitInitialization == implicitInitialization));
            Assert.IsTrue(log.Calls.All(call => ReferenceEquals(call.Exception, log.Calls[0].Exception)));
            Assert.IsFalse((await HydrateAsync("computedDuringInit")).Values.ContainsKey("$field:Value"));

            log.Clear();
            var projectionStatus = await Locate("projectionDuringInit").SendAsync(trigger);
            Assert.AreEqual(EventStatus.Failed, projectionStatus.Status);
            CollectionAssert.AreEquivalent(
                new[] { "projection", "initialization" },
                log.Calls.Where(call => call.EntityName == "projectionDuringInit").Select(call => call.Hook).Distinct().ToArray());
            Assert.IsTrue(log.Calls.Where(call => call.Hook == "initialization")
                .All(call => call.ImplicitInitialization == implicitInitialization));
            Assert.IsTrue(log.Calls.All(call => ReferenceEquals(call.Exception, log.Calls[0].Exception)));
            Assert.IsFalse((await HydrateAsync("projectionDuringInit")).Values.ContainsKey("$field:Value"));
        }

        [TestMethod]
        public async Task ProjectionRequests_DispatchFailureWithoutFabricatingAResult()
        {
            var entity = Locate("projectionOnRequest");
            await entity.SendAsync(new Initialize());
            log.Clear();
            throwProjectionOnRequest = true;

            var (success, projection) = await entity.TryGetProjectionAsync<HandlingRequestProjection>();

            Assert.IsFalse(success);
            Assert.IsNull(projection);
            AssertHandlerOrder("projection", "global-di", "global-sync", "global-async");
            Assert.AreEqual("initial", (await HydrateAsync("projectionOnRequest")).Values["$field:Value"]);
        }

        [TestMethod]
        public async Task HandlerExceptions_DoNotReplaceOriginalFailureOrStopLaterHandlers()
        {
            var entity = Locate("handled");
            await entity.SendAsync(new Initialize());
            log.Clear();
            log.ThrowFromHandler = "global-sync";
            log.ThrowForHook = "mutation";

            var result = await entity.SendAsync(new HandlingSetValue("partial", Throw: true));

            Assert.AreEqual(EventStatus.Failed, result.Status);
            AssertHandlerOrder("mutation",
                "global-di", "global-sync", "global-async",
                "local-di", "local-sync", "local-async");
            Assert.AreEqual("mutation failure", log.Calls[0].Exception.Message);
            Assert.IsTrue(log.Calls.All(call => ReferenceEquals(log.Calls[0].Exception, call.Exception)));
            Assert.AreEqual("exception handler failed", log.SecondaryExceptions.Single().Message);
            Assert.IsTrue(log.LoggedExceptions.Any(exception => ReferenceEquals(exception, log.SecondaryExceptions[0])));
            Assert.AreEqual("initial", (await HydrateAsync("handled")).Values["$field:Value"]);
        }
    }
}
