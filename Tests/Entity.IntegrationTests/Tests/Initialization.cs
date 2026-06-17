using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;

namespace Entity.IntegrationTests.Tests
{
    public interface IInitEntityTemplate
    {
        bool InitializerCalled { get; set; }
        string InitValue { get; set; }
        int ComputedValue { get; set; }
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

        private IBehaviorLocator BehaviorLocator => ServiceProvider.GetRequiredService<IBehaviorLocator>();

        private bool TryLocateEntity(string entityName, string instance, out IBehavior behavior)
            => BehaviorLocator.TryLocateBehavior(new EntityClass(entityName).ToId(instance), out behavior);

        // ── Default initializer ──────────────────────────────────────────────

        [TestMethod]
        public async Task DefaultInitializer_InitializeEvent_Initializes()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("withDefault", "x", out var entity))
                status = (await entity.SendAsync(new Initialize())).Status;

            Assert.AreEqual(EventStatus.Initialized, status);
        }

        [TestMethod]
        public async Task DefaultInitializer_UnknownEvent_NotInitialized()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("withDefault", "x", out var entity))
                status = (await entity.SendAsync(new CustomEntityInitEvent())).Status;

            Assert.AreEqual(EventStatus.NotConsumed, status);
        }

        // ── Custom initializer ───────────────────────────────────────────────

        [TestMethod]
        public async Task CustomInitializer_MatchingEvent_Initializes()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("withCustom", "x", out var entity))
                status = (await entity.SendAsync(new CustomEntityInitEvent("hello"))).Status;

            Assert.AreEqual(EventStatus.Initialized, status);
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


