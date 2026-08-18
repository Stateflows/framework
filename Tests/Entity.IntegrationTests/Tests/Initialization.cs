using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Common.Context;
using Stateflows.Entities.Attributes;

namespace Entity.IntegrationTests.Tests
{
    public interface IInitEntityTemplate
    {
        [Field]
        bool InitializerCalled { get; set; }

        [Field]
        string InitValue { get; set; }

        [Field]
        int ComputedValue { get; set; }

        [Projection]
        InitEntityProjection Snapshot => new()
        {
            InitializerCalled = InitializerCalled,
            InitValue = InitValue,
        };
    }

    public record InitEntityProjection
    {
        public bool InitializerCalled { get; set; }

        public string? InitValue { get; set; }
    }

    [TestClass]
    public class Initialization : StateflowsTestClass
    {
        public record CustomEntityInitEvent(string Value = "");

        [TestInitialize]
        public override void Initialize() => base.Initialize();

        [TestCleanup]
        public override void Cleanup() => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder.AddEntities(entities => entities

                .AddEntity<IInitEntityTemplate>("withDefault", entity => entity
                    .AddDefaultInitializer(ctx => { ctx.Entity.InitializerCalled = true; })
                )

                .AddEntity<IInitEntityTemplate>("withCustom", entity => entity
                    .AddInitializer<CustomEntityInitEvent>(ctx => { ctx.Entity.InitValue = ctx.InitializationEvent.Value; })
                )

                .AddEntity<IInitEntityTemplate>("withBoth", entity => entity
                    .AddDefaultInitializer(ctx => { ctx.Entity.InitializerCalled = true; })
                    .AddInitializer<CustomEntityInitEvent>(ctx => { ctx.Entity.InitValue = ctx.InitializationEvent.Value; })
                )

                .AddEntity<IInitEntityTemplate>("noInitializer", _ => { })
            );
        }

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

        // ── Default initializer ──────────────────────────────────────────────

        [TestMethod]
        public async Task DefaultInitializer_InitializeEvent_Initializes()
        {
            var status = EventStatus.Undelivered;
            var initializerCalled = false;

            if (TryLocateEntity("withDefault", "x", out var entity))
            {
                status = (await entity.SendAsync(new Initialize())).Status;
                var context = await HydrateContextAsync("withDefault", "x");
                initializerCalled = context.Values.TryGetValue("$field:InitializerCalled", out var value) && value is bool fieldValue && fieldValue;
            }

            Assert.AreEqual(EventStatus.Initialized, status);
            Assert.IsTrue(initializerCalled);
        }

        [TestMethod]
        public async Task DefaultInitializer_UnknownEvent_NotInitialized()
        {
            var status = EventStatus.Undelivered;
            var initializerCalled = false;

            if (TryLocateEntity("withDefault", "x", out var entity))
            {
                status = (await entity.SendAsync(new CustomEntityInitEvent())).Status;
                var context = await HydrateContextAsync("withDefault", "x");
                initializerCalled = context.Values.TryGetValue("$field:InitializerCalled", out var value) && value is bool fieldValue && fieldValue;
            }

            Assert.AreEqual(EventStatus.NotConsumed, status);
            Assert.IsTrue(initializerCalled);
        }

        [TestMethod]
        public async Task DefaultInitializer_FirstMutationEvent_InitializesBeforeProcessing()
        {
            var status = EventStatus.Undelivered;
            var initializerCalled = false;

            if (TryLocateEntity("withDefault", "x", out var entity))
            {
                status = (await entity.SendAsync(new CustomEntityInitEvent("hello"))).Status;
                var context = await HydrateContextAsync("withDefault", "x");
                initializerCalled = context.Values.TryGetValue("$field:InitializerCalled", out var value) && value is bool fieldValue && fieldValue;
            }

            Assert.AreEqual(EventStatus.NotConsumed, status);
            Assert.IsTrue(initializerCalled);
        }

        // ── Custom initializer ───────────────────────────────────────────────

        [TestMethod]
        public async Task CustomInitializer_MatchingEvent_Initializes()
        {
            var status = EventStatus.Undelivered;
            string? initValue = null;

            if (TryLocateEntity("withCustom", "x", out var entity))
            {
                status = (await entity.SendAsync(new CustomEntityInitEvent("hello"))).Status;
                var context = await HydrateContextAsync("withCustom", "x");
                initValue = context.Values.TryGetValue("$field:InitValue", out var value) ? value as string : null;
            }

            Assert.AreEqual(EventStatus.Initialized, status);
            Assert.AreEqual("hello", initValue);
        }

        [TestMethod]
        public async Task CustomInitializer_InitializeEvent_Initializes()
        {
            // Initialize event always initializes, even without a default initializer
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("withCustom", "x", out var entity))
                status = (await entity.SendAsync(new Initialize())).Status;

            Assert.AreEqual(EventStatus.Initialized, status);
        }

        [TestMethod]
        public async Task CustomInitializer_UnknownEvent_NotInitialized()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("withCustom", "x", out var entity))
                status = (await entity.SendAsync(new object())).Status;

            Assert.AreEqual(EventStatus.NotConsumed, status);
        }

        // ── No initializer ───────────────────────────────────────────────────

        [TestMethod]
        public async Task NoInitializer_InitializeEvent_Initializes()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("noInitializer", "x", out var entity))
                status = (await entity.SendAsync(new Initialize())).Status;

            Assert.AreEqual(EventStatus.Initialized, status);
        }

        [TestMethod]
        public async Task NoInitializer_UnknownEvent_NotInitialized()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("noInitializer", "x", out var entity))
                status = (await entity.SendAsync(new object())).Status;

            Assert.AreEqual(EventStatus.NotConsumed, status);
        }

        [TestMethod]
        public async Task DefaultInitializer_FirstProjectionRequest_InitializesBeforeProjection()
        {
            var success = false;
            InitEntityProjection? projection = null;

            if (TryLocateEntity("withDefault", "x", out var entity))
            {
                (success, projection) = await entity.TryGetProjectionAsync<InitEntityProjection>();
            }

            Assert.IsTrue(success);
            Assert.IsNotNull(projection);
            Assert.IsTrue(projection.InitializerCalled);
        }

        // ── Already initialized ──────────────────────────────────────────────

        [TestMethod]
        public async Task AlreadyInitialized_SecondInitializeEvent_NotConsumed()
        {
            var status1 = EventStatus.Undelivered;
            var status2 = EventStatus.Undelivered;

            if (TryLocateEntity("withDefault", "x", out var entity))
            {
                status1 = (await entity.SendAsync(new Initialize())).Status;
                status2 = (await entity.SendAsync(new Initialize())).Status;
            }

            Assert.AreEqual(EventStatus.Initialized, status1);
            Assert.AreEqual(EventStatus.NotConsumed, status2);
        }

        [TestMethod]
        public async Task AlreadyInitialized_CustomEvent_NotConsumed()
        {
            var status1 = EventStatus.Undelivered;
            var status2 = EventStatus.Undelivered;

            if (TryLocateEntity("withCustom", "x", out var entity))
            {
                status1 = (await entity.SendAsync(new CustomEntityInitEvent("first"))).Status;
                status2 = (await entity.SendAsync(new CustomEntityInitEvent("second"))).Status;
            }

            Assert.AreEqual(EventStatus.Initialized, status1);
            Assert.AreEqual(EventStatus.NotConsumed, status2);
        }

        // ── With both initializers ───────────────────────────────────────────

        [TestMethod]
        public async Task BothInitializers_InitializeEvent_UsesDefault()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("withBoth", "x", out var entity))
                status = (await entity.SendAsync(new Initialize())).Status;

            Assert.AreEqual(EventStatus.Initialized, status);
        }

        [TestMethod]
        public async Task BothInitializers_CustomEvent_UsesCustom()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("withBoth", "x", out var entity))
                status = (await entity.SendAsync(new CustomEntityInitEvent("test"))).Status;

            Assert.AreEqual(EventStatus.Initialized, status);
        }
    }
}


