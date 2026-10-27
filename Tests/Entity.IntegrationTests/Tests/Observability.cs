using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Common.Context;
using Stateflows.Common.Context.Interfaces;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;

namespace Entity.IntegrationTests.Tests
{
    public record ObservedMutation(int Value);
    public record BlockedEntityEvent;
    public record ReplacedEntityEvent;
    public record ObservedSnapshot(int Value, int Double, int Quadruple);

    public interface IObservedEntityTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        int Value { get; set; }

        [Field(FieldAccess.Get | FieldAccess.Set)]
        int Unrelated { get; set; }

        [Field(FieldAccess.Get)]
        int Double => Value == -2
            ? throw new InvalidOperationException("computed failure")
            : Value * 2;

        [Field(FieldAccess.Get)]
        int Quadruple => Double * 2;

        [Projection(PublishScope = PublishScope.Self)]
        ObservedSnapshot Snapshot => Value == -3
            ? throw new InvalidOperationException("projection failure")
            : new(Value, Double, Quadruple);

        [Mutation]
        void Apply(ObservedMutation mutation)
        {
            Value = mutation.Value;
            if (Value == -1)
                throw new InvalidOperationException("mutation failure");
        }
    }

    public sealed class ObservationLog
    {
        public List<string> Entries { get; } = new();
        public List<(string Name, Guid Instance)> Instances { get; } = new();
        public List<object?> Results { get; } = new();
        public List<Exception> Exceptions { get; } = new();
        public List<(string Name, BehaviorId Behavior)> Factories { get; } = new();
        public int Initializations { get; set; }
        public string? ContinuationViolation { get; set; }

        public void Clear()
        {
            Entries.Clear();
            Instances.Clear();
            Results.Clear();
            Exceptions.Clear();
            Factories.Clear();
        }
    }

    public class RecordingEntityObserver(ObservationLog log, string name) : IEntityObserver
    {
        private readonly Guid instance = Guid.NewGuid();

        private async Task ObserveAsync(IBehaviorActionContext context, string operation, Func<Task> next)
        {
            Assert.IsNotNull(context.Behavior);
            Assert.AreEqual("x", context.Behavior.Id.Instance);
            Assert.IsTrue(context.Behavior.Id.Name is "observed" or "globalOnly");
            log.Instances.Add((name, instance));
            log.Entries.Add($"{operation}:{name}:before");
            await Task.Yield();
            try
            {
                await next();
                await Task.Yield();
                log.Entries.Add($"{operation}:{name}:after");
            }
            catch (Exception exception)
            {
                log.Exceptions.Add(exception);
                throw;
            }
            finally
            {
                log.Entries.Add($"{operation}:{name}:finally");
            }
        }

        public Task OnMutationAsync<TTemplate, TMutationEvent>(
            IMutationObservationContext<TTemplate, TMutationEvent> context, Func<Task> next)
        {
            Assert.AreEqual(typeof(IObservedEntityTemplate), typeof(TTemplate));
            Assert.AreEqual(typeof(ObservedMutation), typeof(TMutationEvent));
            Assert.IsInstanceOfType<IObservedEntityTemplate>(context.Entity);
            Assert.IsInstanceOfType<ObservedMutation>(context.MutationEvent);
            return ObserveAsync(context, "mutation", async () =>
            {
                if (name == "global" && log.ContinuationViolation == "observer-missing")
                    return;

                if (name == "global" && log.ContinuationViolation == "observer-forgotten-await")
                {
                    _ = next();
                    return;
                }

                if (name == "global" && log.ContinuationViolation == "observer-swallowed-failure")
                {
                    try
                    {
                        await next();
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    return;
                }

                await next();
                if (name == "global" && log.ContinuationViolation == "observer-double")
                    await next();
            });
        }

        public Task OnComputedFieldAsync<TTemplate, TField>(
            IComputedFieldObservationContext<TTemplate> context, Func<Task<TField>> next)
        {
            Assert.AreEqual(typeof(IObservedEntityTemplate), typeof(TTemplate));
            Assert.AreEqual(typeof(int), typeof(TField));
            Assert.IsInstanceOfType<IObservedEntityTemplate>(context.Entity);
            _ = ((IObservedEntityTemplate)(object)context.Entity!).Unrelated;
            Assert.IsTrue(context.FieldName is nameof(IObservedEntityTemplate.Double) or nameof(IObservedEntityTemplate.Quadruple));
            return ObserveAsync(context, context.FieldName, async () => log.Results.Add(await next()));
        }

        public Task OnProjectionAsync<TTemplate, TProjection>(
            IProjectionObservationContext<TTemplate> context, Func<Task<TProjection>> next)
        {
            Assert.AreEqual(typeof(IObservedEntityTemplate), typeof(TTemplate));
            Assert.AreEqual(typeof(ObservedSnapshot), typeof(TProjection));
            Assert.IsInstanceOfType<IObservedEntityTemplate>(context.Entity);
            _ = ((IObservedEntityTemplate)(object)context.Entity!).Unrelated;
            Assert.AreEqual(PublishScope.Self, context.PublishScope);
            return ObserveAsync(context, "projection", async () => log.Results.Add(await next()));
        }
    }

    public sealed class GlobalEntityObserver(ObservationLog log) : RecordingEntityObserver(log, "global") { }
    public sealed class LocalEntityObserver(ObservationLog log) : RecordingEntityObserver(log, "local") { }

    public class RecordingEntityInterceptor(ObservationLog log, string name) : IEntityInterceptor
    {
        public async Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
        {
            Assert.IsNotNull(context.Behavior);
            Assert.AreNotEqual(Guid.Empty, context.EventId);
            log.Entries.Add($"event:{name}:before");
            await Task.Yield();
            try
            {
                if (name == "global" && context.Event is BlockedEntityEvent)
                    return EventStatus.Rejected;

                if (name == "global" && log.ContinuationViolation == "interceptor-forgotten-await")
                {
                    _ = next();
                    return EventStatus.Consumed;
                }

                var status = await next();
                if (name == "global" && log.ContinuationViolation == "interceptor-double")
                    await next();
                await Task.Yield();
                log.Entries.Add($"event:{name}:after:{status}");
                return name == "global" && context.Event is ReplacedEntityEvent
                    ? EventStatus.Consumed
                    : status;
            }
            finally
            {
                log.Entries.Add($"event:{name}:finally");
            }
        }
    }

    public sealed class GlobalEntityInterceptor(ObservationLog log) : RecordingEntityInterceptor(log, "global") { }
    public sealed class LocalEntityInterceptor(ObservationLog log) : RecordingEntityInterceptor(log, "local") { }

    [TestClass]
    public class Observability : StateflowsTestClass
    {
        private readonly ObservationLog log = new();

        [TestInitialize]
        public override void Initialize()
        {
            ServiceCollection.AddSingleton(log);
            base.Initialize();
        }

        [TestCleanup]
        public override void Cleanup() => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder.AddEntities(entities => entities
                .AddObserver<GlobalEntityObserver>()
                .AddObserver((services, context) =>
                {
                    Assert.AreSame(log, services.GetRequiredService<ObservationLog>());
                    log.Factories.Add(("global", context.Behavior.Id));
                    return new RecordingEntityObserver(log, "global-factory");
                })
                .AddInterceptor<GlobalEntityInterceptor>()
                .AddInterceptor((services, context) =>
                {
                    Assert.AreSame(log, services.GetRequiredService<ObservationLog>());
                    log.Factories.Add(("global-interceptor", context.Behavior.Id));
                    return new RecordingEntityInterceptor(log, "global-factory");
                })
                .AddEntity<IObservedEntityTemplate>("observed", entity => entity
                    .AddDefaultInitializer(context =>
                    {
                        log.Initializations++;
                        context.Entity.Value = 1;
                    })
                    .AddObserver<LocalEntityObserver>()
                    .AddObserver(async (services, context) =>
                    {
                        await Task.Yield();
                        Assert.AreSame(log, services.GetRequiredService<ObservationLog>());
                        log.Factories.Add(("local", context.Behavior.Id));
                        return log.ContinuationViolation == "observer-null-factory"
                            ? null!
                            : new RecordingEntityObserver(log, "local-factory");
                    })
                    .AddInterceptor<LocalEntityInterceptor>()
                    .AddInterceptor(async (services, context) =>
                    {
                        await Task.Yield();
                        Assert.AreSame(log, services.GetRequiredService<ObservationLog>());
                        log.Factories.Add(("local-interceptor", context.Behavior.Id));
                        return log.ContinuationViolation == "interceptor-null-factory"
                            ? null!
                            : new RecordingEntityInterceptor(log, "local-factory");
                    })
                )
                .AddEntity<IObservedEntityTemplate>("globalOnly", entity => entity
                    .AddDefaultInitializer(context => context.Entity.Value = 1))
            );
        }

        private IEntityBehavior Locate(string name = "observed")
        {
            var locator = ServiceProvider.GetRequiredService<IEntityLocator>();
            Assert.IsTrue(locator.TryLocateEntity(new EntityClass(name).ToId("x"), out var entity));
            return entity;
        }

        private void AssertObserverOrder(string operation, bool failed = false)
        {
            var names = new[] { "global", "global-factory", "local", "local-factory" };
            var expected = names.Select(name => $"{operation}:{name}:before")
                .Concat(names.Reverse().SelectMany(name => failed
                    ? new[] { $"{operation}:{name}:finally" }
                    : new[] { $"{operation}:{name}:after", $"{operation}:{name}:finally" }));
            var expectedEvaluation = expected.ToArray();
            var actual = log.Entries.Where(entry => entry.StartsWith(operation + ":")).ToArray();
            var message = $"Actual {operation} hook sequence: {string.Join(", ", actual)}";
            Assert.IsTrue(actual.Length > 0 && actual.Length % expectedEvaluation.Length == 0, message);
            // Initialization may evaluate a dependent computed field again while stabilizing dependencies.
            foreach (var evaluation in actual.Chunk(expectedEvaluation.Length))
                CollectionAssert.AreEqual(expectedEvaluation, evaluation, message);
        }

        private void AssertInterceptorOrder(EventStatus innerStatus)
        {
            var names = new[] { "global", "global-factory", "local", "local-factory" };
            var expected = names.Select(name => $"event:{name}:before")
                .Concat(names.Reverse().SelectMany(name => new[]
                {
                    $"event:{name}:after:{innerStatus}", $"event:{name}:finally",
                }));
            CollectionAssert.AreEqual(expected.ToArray(),
                log.Entries.Where(entry => entry.StartsWith("event:")).ToArray());
        }

        [TestMethod]
        public async Task InitializationAndMutation_AwaitNestedHooksAndResolveOncePerEvent()
        {
            var entity = Locate();
            Assert.AreEqual(EventStatus.Initialized, (await entity.SendAsync(new Initialize())).Status);
            Assert.AreEqual(1, log.Initializations);
            AssertObserverOrder(nameof(IObservedEntityTemplate.Double));
            AssertObserverOrder(nameof(IObservedEntityTemplate.Quadruple));
            AssertObserverOrder("projection");
            AssertInterceptorOrder(EventStatus.Initialized);
            Assert.IsFalse(log.Entries.Any(entry => entry.StartsWith("mutation:")));
            var previousInstances = log.Instances.Select(entry => entry.Instance).ToHashSet();

            log.Clear();
            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(new ObservedMutation(3))).Status);
            AssertObserverOrder("mutation");
            AssertObserverOrder(nameof(IObservedEntityTemplate.Double));
            AssertObserverOrder(nameof(IObservedEntityTemplate.Quadruple));
            AssertObserverOrder("projection");
            AssertInterceptorOrder(EventStatus.Consumed);
            CollectionAssert.AreEquivalent(new object[] { 6, 12, new ObservedSnapshot(3, 6, 12) },
                log.Results.Distinct().ToArray());
            foreach (var group in log.Instances.GroupBy(entry => entry.Name))
            {
                Assert.AreEqual(1, group.Select(entry => entry.Instance).Distinct().Count());
                Assert.IsFalse(previousInstances.Contains(group.First().Instance));
            }
            CollectionAssert.AreEquivalent(
                new[] { "global", "local", "global-interceptor", "local-interceptor" },
                log.Factories.Select(entry => entry.Name).ToArray());
            Assert.IsTrue(log.Factories.All(entry => entry.Behavior == entity.Id));
        }

        [TestMethod]
        public async Task FieldWrites_RecalculateDependenciesAndProjection_ButFieldReadsAreCached()
        {
            var entity = Locate();
            await entity.SendAsync(new Initialize());
            log.Clear();

            Assert.IsTrue(await entity.TrySetAsync(nameof(IObservedEntityTemplate.Value), 5));
            AssertObserverOrder(nameof(IObservedEntityTemplate.Double));
            AssertObserverOrder(nameof(IObservedEntityTemplate.Quadruple));
            AssertObserverOrder("projection");
            Assert.IsFalse(log.Entries.Any(entry => entry.StartsWith("mutation:")));
            Assert.IsTrue(log.Results.Contains(new ObservedSnapshot(5, 10, 20)));
            log.Clear();

            var (success, value) = await entity.TryGetAsync<int>(nameof(IObservedEntityTemplate.Quadruple));
            Assert.IsTrue(success);
            Assert.AreEqual(20, value);
            Assert.AreEqual(0, log.Instances.Count);
            Assert.AreEqual(0, log.Results.Count);
            AssertInterceptorOrder(EventStatus.Consumed);
        }

        [TestMethod]
        public async Task ProjectionRequests_EvaluateEveryTime_AndExposeTypedResults()
        {
            var entity = Locate();
            await entity.SendAsync(new Initialize());
            for (var request = 0; request < 2; request++)
            {
                log.Clear();
                var (success, projection) = await entity.TryGetProjectionAsync<ObservedSnapshot>();
                Assert.IsTrue(success);
                Assert.AreEqual(new ObservedSnapshot(1, 2, 4), projection);
                AssertObserverOrder("projection");
                Assert.IsFalse(log.Entries.Any(entry => entry.StartsWith("Double:") || entry.StartsWith("Quadruple:")));
                Assert.AreEqual(4, log.Results.Count);
            }

        }

        [TestMethod]
        public async Task ObserverFieldReads_DoNotBecomeComputedOrProjectionDependencies()
        {
            var entity = Locate();
            await entity.SendAsync(new Initialize());
            log.Clear();

            Assert.IsTrue(await entity.TrySetAsync(nameof(IObservedEntityTemplate.Unrelated), 7));
            Assert.AreEqual(0, log.Instances.Count,
                "Reading Unrelated inside observers must not add dependencies to either computed field or projection.");
            Assert.AreEqual(0, log.Results.Count);

            Assert.IsTrue(await entity.TrySetAsync(nameof(IObservedEntityTemplate.Value), 2));
            AssertObserverOrder(nameof(IObservedEntityTemplate.Double));
            AssertObserverOrder(nameof(IObservedEntityTemplate.Quadruple));
            AssertObserverOrder("projection");
            Assert.IsTrue(log.Results.Contains(new ObservedSnapshot(2, 4, 8)));
        }

        [TestMethod]
        [DataRow("observer-missing", 9)]
        [DataRow("observer-double", 9)]
        [DataRow("observer-forgotten-await", 9)]
        [DataRow("observer-swallowed-failure", -1)]
        [DataRow("interceptor-double", 9)]
        [DataRow("interceptor-forgotten-await", 9)]
        [DataRow("observer-null-factory", 9)]
        [DataRow("interceptor-null-factory", 9)]
        public async Task InvalidHookUsage_FailsAndDoesNotCommitMutation(string violation, int mutationValue)
        {
            var entity = Locate();
            await entity.SendAsync(new Initialize());
            log.Clear();
            log.ContinuationViolation = violation;

            Assert.AreEqual(EventStatus.Failed, (await entity.SendAsync(new ObservedMutation(mutationValue))).Status);
            log.ContinuationViolation = null;
            var (success, value) = await entity.TryGetAsync<int>(nameof(IObservedEntityTemplate.Value));
            Assert.IsTrue(success);
            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public async Task MutationOnNewEntity_ObservesImplicitInitializationAndReactiveUpdates()
        {
            var entity = Locate();
            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(new ObservedMutation(4))).Status);
            Assert.AreEqual(1, log.Initializations);
            AssertObserverOrder("mutation");
            Assert.IsTrue(log.Results.Contains(new ObservedSnapshot(1, 2, 4)));
            Assert.IsTrue(log.Results.Contains(new ObservedSnapshot(4, 8, 16)));
            AssertInterceptorOrder(EventStatus.Consumed);
        }

        [TestMethod]
        [DataRow(-1, "mutation", "mutation failure")]
        [DataRow(-2, "Double", "computed failure")]
        [DataRow(-3, "projection", "projection failure")]
        public async Task Exceptions_PropagateThroughObservers_RunFinallyAndRollback(
            int mutationValue, string operation, string message)
        {
            var entity = Locate();
            await entity.SendAsync(new Initialize());
            log.Clear();

            Assert.AreEqual(EventStatus.Failed, (await entity.SendAsync(new ObservedMutation(mutationValue))).Status);
            AssertObserverOrder(operation, failed: true);
            Assert.AreEqual(4, log.Exceptions.Count(exception => exception.Message == message));
            Assert.IsTrue(log.Exceptions.All(exception => ReferenceEquals(exception, log.Exceptions[0])));

            var (success, storedValue) = await entity.TryGetAsync<int>(nameof(IObservedEntityTemplate.Value));
            Assert.IsTrue(success);
            Assert.AreEqual(1, storedValue);
        }

        [TestMethod]
        public async Task InterceptorShortCircuit_PreventsImplicitInitialization()
        {
            var entity = Locate();
            Assert.AreEqual(EventStatus.Rejected, (await entity.SendAsync(new BlockedEntityEvent())).Status);
            Assert.AreEqual(0, log.Initializations);
            Assert.AreEqual(0, log.Instances.Count);
            CollectionAssert.AreEqual(new[] { "event:global:before", "event:global:finally" }, log.Entries);

            var tenantAccessor = ServiceProvider.GetRequiredService<ITenantAccessor>();
            tenantAccessor.CurrentTenantId = ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            var context = await ServiceProvider.GetRequiredService<IStateflowsStorage>().HydrateAsync(entity.Id);
            Assert.AreNotEqual(BehaviorStatus.Initialized, context.Status);
            Assert.IsFalse(context.Values.ContainsKey("$field:Value"));

            log.Clear();
            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(new ObservedMutation(2))).Status);
            Assert.AreEqual(1, log.Initializations);
        }

        [TestMethod]
        public async Task Interceptors_CanReplaceStatusAfterAwaitingProcessing()
        {
            var entity = Locate();
            await entity.SendAsync(new Initialize());
            log.Clear();

            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(new ReplacedEntityEvent())).Status);
            AssertInterceptorOrder(EventStatus.NotConsumed);
            Assert.AreEqual(0, log.Instances.Count);
        }

        [TestMethod]
        public async Task LocalRegistrations_DoNotLeakToOtherEntityDefinitions()
        {
            var entity = Locate("globalOnly");
            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(new ObservedMutation(2))).Status);
            CollectionAssert.AreEquivalent(new[] { "global", "global-factory" },
                log.Instances.Select(entry => entry.Name).Distinct().ToArray());
            Assert.IsFalse(log.Entries.Any(entry => entry.Contains(":local")));
            Assert.IsTrue(log.Results.Contains(new ObservedSnapshot(2, 4, 8)));
        }
    }
}
