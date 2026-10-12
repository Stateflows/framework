using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Common.Context;
using Stateflows.Entities.Attributes;

namespace Entity.IntegrationTests.Tests
{
    // ── Event records ─────────────────────────────────────────────────────────

    public record ExceptionCustomInitEvent(string Value);

    public record ExceptionAttributedInitEvent(string Value);

    public record ExceptionMutationSetValueEvent(string NewValue);

    public record ExceptionComputedTriggerEvent(string NewValue);

    public record ExceptionProjectionSetValueEvent(string NewValue);

    // ── Templates ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Template with an [Initializer] method that partially writes a field and then throws.
    /// Used to verify that the partial state change is NOT persisted (atomicity).
    /// </summary>
    public interface IExceptionAttributedInitTemplate
    {
        [Field]
        string Value { get; set; }

        [Initializer]
        void Initialize(ExceptionAttributedInitEvent e)
        {
            Value = "partial"; // partial state – must not be persisted
            throw new InvalidOperationException("attributed initializer failed intentionally");
        }
    }

    /// <summary>
    /// Plain stored field only. Used for default and custom initializer tests.
    /// No mutations or projections are auto-discovered.
    /// </summary>
    public interface IExceptionInitTemplate
    {
        [Field]
        string Value { get; set; }
    }

    /// <summary>
    /// Mutation that first writes to a stored field, then throws.
    /// Used to verify that the partial field change is NOT persisted (atomicity).
    /// </summary>
    public interface IExceptionMutationTemplate
    {
        [Field]
        string Value { get; set; }

        [Mutation]
        void ThrowingMutate(ExceptionMutationSetValueEvent e)
        {
            Value = e.NewValue; // partial state change – must be rolled back on failure
            throw new InvalidOperationException("mutation failed intentionally");
        }
    }

    /// <summary>
    /// Stored field plus a computed field that throws when <c>Value == "trigger"</c>.
    /// Used to verify that a computed-field exception during a mutation leaves the stored
    /// field unchanged (atomicity).
    /// </summary>
    public interface IExceptionComputedFieldTemplate
    {
        [Field]
        string Value { get; set; }

        [Field]
        int ComputedFromValue => Value == "trigger"
            ? throw new InvalidOperationException("computed field failed intentionally")
            : Value.Length;

        [Mutation]
        void SetValue(ExceptionComputedTriggerEvent e)
        {
            Value = e.NewValue;
        }
    }

    /// <summary>
    /// Plain stored field plus a harmless mutation.
    /// Projections are registered explicitly in the entity builder so that
    /// each test entity can control whether they throw.
    /// </summary>
    public interface IExceptionProjectionFieldTemplate
    {
        [Field]
        string Value { get; set; }

        [Mutation]
        void SetValue(ExceptionProjectionSetValueEvent e)
        {
            Value = e.NewValue;
        }
    }

    // ── Projection snapshots ──────────────────────────────────────────────────

    public record ExceptionThrowingProjectionSnapshot
    {
        public string? Value { get; set; }
    }

    public record ExceptionConditionalProjectionSnapshot
    {
        public string? Value { get; set; }
    }

    // ── Test class ────────────────────────────────────────────────────────────

    [TestClass]
    public class Exceptions : StateflowsTestClass
    {
        /// <summary>
        /// Controls whether the conditional projection throws.
        /// MSTest creates a new instance of the test class per test method,
        /// so this is effectively per-test state captured via lambda closure.
        /// </summary>
        private bool _throwOnProjectionRequest = false;

        [TestInitialize]
        public override void Initialize() => base.Initialize();

        [TestCleanup]
        public override void Cleanup() => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder.AddEntities(entities => entities

                // Default initializer that writes a partial value and then throws.
                .AddEntity<IExceptionInitTemplate>("throwDefaultInit", entity => entity
                    .AddDefaultInitializer(ctx =>
                    {
                        ctx.Entity.Value = "partial"; // partial state – must not be persisted
                        throw new InvalidOperationException("default initializer failed intentionally");
                    })
                )

                // Custom initializer that writes a partial value and then throws.
                .AddEntity<IExceptionInitTemplate>("throwCustomInit", entity => entity
                    .AddInitializer<ExceptionCustomInitEvent>(ctx =>
                    {
                        ctx.Entity.Value = "partial"; // partial state – must not be persisted
                        throw new InvalidOperationException("custom initializer failed intentionally");
                    })
                )

                // Attributed initializer (template-defined) that writes a partial value and then throws.
                .AddEntity<IExceptionAttributedInitTemplate>("throwAttributedInit")

                // Mutation entity: default init sets Value = "initial"; the [Mutation]
                // ThrowingMutate is auto-discovered and partially changes Value before throwing.
                .AddEntity<IExceptionMutationTemplate>("throwMutation", entity => entity
                    .AddDefaultInitializer(ctx => { ctx.Entity.Value = "initial"; })
                )

                // Computed-field entity: default init sets Value = "hello"; setting
                // Value = "trigger" via mutation causes the computed field to throw.
                .AddEntity<IExceptionComputedFieldTemplate>("throwComputedField", entity => entity
                    .AddDefaultInitializer(ctx => { ctx.Entity.Value = "hello"; })
                )

                // Projection entity whose only projection always throws.
                // Used to verify that initialization fails atomically when a projection throws.
                .AddEntity<IExceptionProjectionFieldTemplate>("throwProjectionOnInit", entity => entity
                    .AddDefaultInitializer(ctx => { ctx.Entity.Value = "initial"; })
                    .AddProjection<ExceptionThrowingProjectionSnapshot>(_ =>
                        throw new InvalidOperationException("projection failed intentionally"))
                )

                // Projection entity whose projection throws only when _throwOnProjectionRequest
                // is true. Initialization sets the flag to false so it succeeds; the test
                // flips it to true to exercise the request-time failure path.
                .AddEntity<IExceptionProjectionFieldTemplate>("throwProjectionOnRequest", entity => entity
                    .AddDefaultInitializer(ctx => { ctx.Entity.Value = "initial"; })
                    .AddProjection<ExceptionConditionalProjectionSnapshot>(e =>
                    {
                        if (_throwOnProjectionRequest)
                            throw new InvalidOperationException("projection request failed intentionally");
                        return new ExceptionConditionalProjectionSnapshot { Value = e.Value };
                    })
                )
            );
        }

        // ── Infrastructure helpers ────────────────────────────────────────────

        private IEntityLocator EntityLocator => ServiceProvider.GetRequiredService<IEntityLocator>();

        private async Task<StateflowsContext> HydrateContextAsync(string entityName, string instance)
        {
            var storage = ServiceProvider.GetRequiredService<IStateflowsStorage>();
            var tenantAccessor = ServiceProvider.GetRequiredService<ITenantAccessor>();
            var tenantProvider = ServiceProvider.GetRequiredService<IStateflowsTenantProvider>();
            tenantAccessor.CurrentTenantId = await tenantProvider.GetCurrentTenantIdAsync();
            return await storage.HydrateAsync(new EntityClass(entityName).ToId(instance));
        }

        private bool TryLocateEntity(string entityName, string instance, out IEntityBehavior behavior)
            => EntityLocator.TryLocateEntity(new EntityClass(entityName).ToId(instance), out behavior);

        // ── Default initializer exceptions ────────────────────────────────────

        [TestMethod]
        public async Task DefaultInitializer_ThrowsOnInitialize_StatusIsFailed()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("throwDefaultInit", "x", out var entity))
                status = (await entity.SendAsync(new Initialize())).Status;

            Assert.AreEqual(EventStatus.Failed, status);
        }

        [TestMethod]
        public async Task DefaultInitializer_ThrowsOnInitialize_EntityNotInitialized()
        {
            if (TryLocateEntity("throwDefaultInit", "x", out var entity))
                await entity.SendAsync(new Initialize());

            var context = await HydrateContextAsync("throwDefaultInit", "x");
            Assert.AreNotEqual(BehaviorStatus.Initialized, context.Status,
                "Entity must not be marked as initialized when the default initializer threw.");
        }

        [TestMethod]
        public async Task DefaultInitializer_ThrowsOnFirstEvent_StatusIsFailed()
        {
            // Any non-init event on an uninitialized entity triggers auto-init.
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("throwDefaultInit", "x", out var entity))
                status = (await entity.SendAsync(new object())).Status;

            Assert.AreEqual(EventStatus.Failed, status);
        }

        [TestMethod]
        public async Task DefaultInitializer_ThrowsOnFirstEvent_EntityNotInitialized()
        {
            if (TryLocateEntity("throwDefaultInit", "x", out var entity))
                await entity.SendAsync(new object());

            var context = await HydrateContextAsync("throwDefaultInit", "x");
            Assert.AreNotEqual(BehaviorStatus.Initialized, context.Status,
                "Entity must not be marked as initialized when auto-init threw on the first event.");
        }

        // ── Custom initializer exceptions ─────────────────────────────────────

        [TestMethod]
        public async Task CustomInitializer_ThrowsOnMatchingEvent_StatusIsFailed()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("throwCustomInit", "x", out var entity))
                status = (await entity.SendAsync(new ExceptionCustomInitEvent("trigger"))).Status;

            Assert.AreEqual(EventStatus.Failed, status);
        }

        [TestMethod]
        public async Task CustomInitializer_ThrowsOnMatchingEvent_EntityNotInitialized()
        {
            if (TryLocateEntity("throwCustomInit", "x", out var entity))
                await entity.SendAsync(new ExceptionCustomInitEvent("trigger"));

            var context = await HydrateContextAsync("throwCustomInit", "x");
            Assert.AreNotEqual(BehaviorStatus.Initialized, context.Status,
                "Entity must not be marked as initialized when the custom initializer threw.");
        }

        // ── Mutation exceptions ───────────────────────────────────────────────

        [TestMethod]
        public async Task Mutation_ThrowsAfterPartialChange_StatusIsFailed()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("throwMutation", "x", out var entity))
            {
                await entity.SendAsync(new Initialize());
                status = (await entity.SendAsync(new ExceptionMutationSetValueEvent("new_value"))).Status;
            }

            Assert.AreEqual(EventStatus.Failed, status);
        }

        [TestMethod]
        public async Task Mutation_ThrowsAfterPartialChange_FieldValueUnchanged()
        {
            string? valueAfterFailure = null;

            if (TryLocateEntity("throwMutation", "x", out var entity))
            {
                await entity.SendAsync(new Initialize());
                await entity.SendAsync(new ExceptionMutationSetValueEvent("new_value"));
                var context = await HydrateContextAsync("throwMutation", "x");
                valueAfterFailure = context.Values.TryGetValue("$field:Value", out var v) ? v as string : null;
            }

            Assert.AreEqual("initial", valueAfterFailure,
                "Stored field Value must remain 'initial'; the partial mutation change must not be persisted.");
        }

        // ── Computed field exceptions ─────────────────────────────────────────

        [TestMethod]
        public async Task ComputedField_ThrowsDuringMutation_StatusIsFailed()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("throwComputedField", "x", out var entity))
            {
                await entity.SendAsync(new Initialize());
                status = (await entity.SendAsync(new ExceptionComputedTriggerEvent("trigger"))).Status;
            }

            Assert.AreEqual(EventStatus.Failed, status);
        }

        [TestMethod]
        public async Task ComputedField_ThrowsDuringMutation_StoredFieldUnchanged()
        {
            string? valueAfterFailure = null;

            if (TryLocateEntity("throwComputedField", "x", out var entity))
            {
                await entity.SendAsync(new Initialize());
                await entity.SendAsync(new ExceptionComputedTriggerEvent("trigger"));
                var context = await HydrateContextAsync("throwComputedField", "x");
                valueAfterFailure = context.Values.TryGetValue("$field:Value", out var v) ? v as string : null;
            }

            Assert.AreEqual("hello", valueAfterFailure,
                "Stored field Value must remain 'hello'; the mutation-driven field change must not be persisted when the computed field threw.");
        }

        // ── Projection exceptions ─────────────────────────────────────────────

        [TestMethod]
        public async Task Projection_ThrowsDuringInitialization_StatusIsFailed()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("throwProjectionOnInit", "x", out var entity))
                status = (await entity.SendAsync(new Initialize())).Status;

            Assert.AreEqual(EventStatus.Failed, status,
                "Initialize must fail when a projection throws during initialization.");
        }

        [TestMethod]
        public async Task Projection_ThrowsDuringInitialization_EntityNotInitialized()
        {
            if (TryLocateEntity("throwProjectionOnInit", "x", out var entity))
                await entity.SendAsync(new Initialize());

            var context = await HydrateContextAsync("throwProjectionOnInit", "x");
            Assert.AreNotEqual(BehaviorStatus.Initialized, context.Status,
                "Entity must not be marked as initialized when a projection threw during initialization.");
        }

        [TestMethod]
        public async Task Projection_ThrowsDuringRequest_ReturnsFailed()
        {
            var projectionSuccess = true;
            ExceptionConditionalProjectionSnapshot? projection = null;

            if (TryLocateEntity("throwProjectionOnRequest", "x", out var entity))
            {
                _throwOnProjectionRequest = false;
                await entity.SendAsync(new Initialize());
                _throwOnProjectionRequest = true;
                (projectionSuccess, projection) =
                    await entity.TryGetProjectionAsync<ExceptionConditionalProjectionSnapshot>();
            }

            Assert.IsFalse(projectionSuccess,
                "TryGetProjectionAsync must return success=false when the projection throws.");
            Assert.IsNull(projection);
        }

        [TestMethod]
        public async Task Projection_ThrowsDuringRequest_EntityFieldsUnchanged()
        {
            string? valueAfterFailure = null;

            if (TryLocateEntity("throwProjectionOnRequest", "x", out var entity))
            {
                _throwOnProjectionRequest = false;
                await entity.SendAsync(new Initialize());
                await entity.SendAsync(new ExceptionProjectionSetValueEvent("modified"));
                _throwOnProjectionRequest = true;
                await entity.TryGetProjectionAsync<ExceptionConditionalProjectionSnapshot>();
                var context = await HydrateContextAsync("throwProjectionOnRequest", "x");
                valueAfterFailure = context.Values.TryGetValue("$field:Value", out var v) ? v as string : null;
            }

            Assert.AreEqual("modified", valueAfterFailure,
                "Stored field Value must be unchanged ('modified') after a failed projection request.");
        }

        // ── Attributed initializer exceptions ─────────────────────────────────

        [TestMethod]
        public async Task AttributedInitializer_ThrowsOnMatchingEvent_StatusIsFailed()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("throwAttributedInit", "x", out var entity))
                status = (await entity.SendAsync(new ExceptionAttributedInitEvent("trigger"))).Status;

            Assert.AreEqual(EventStatus.Failed, status);
        }

        [TestMethod]
        public async Task AttributedInitializer_ThrowsOnMatchingEvent_EntityNotInitialized()
        {
            if (TryLocateEntity("throwAttributedInit", "x", out var entity))
                await entity.SendAsync(new ExceptionAttributedInitEvent("trigger"));

            var context = await HydrateContextAsync("throwAttributedInit", "x");
            Assert.AreNotEqual(BehaviorStatus.Initialized, context.Status,
                "Entity must not be marked as initialized when the attributed initializer threw.");
        }
    }
}
