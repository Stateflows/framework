using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Entities.Attributes;

namespace Entity.IntegrationTests.Tests
{
    public record StatusIncrement(int Amount);

    public record StatusOpen(string Owner);

    public interface IStatusEntityTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        int Count { get; set; }

        [Mutation]
        void Increment(StatusIncrement mutation) { Count += mutation.Amount; }
    }

    public interface IStatusInitializedEntityTemplate
    {
        [Field(FieldAccess.Get)]
        string Owner { get; set; }

        [Mutation]
        void Increment(StatusIncrement mutation) { }
    }

    [TestClass]
    public class Status : StateflowsTestClass
    {
        private static readonly Dictionary<string, EventHeader> NoImplicitInitializationHeaders
            = new() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } };

        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddEntities(entities => entities
                    .AddEntity<IStatusEntityTemplate>("status")
                    .AddEntity<IStatusInitializedEntityTemplate>("statusWithInitializer", entity => entity
                        .AddInitializer<StatusOpen>(context => context.Entity.Owner = context.InitializationEvent.Owner)
                    )
                );
        }

        private IEntityBehavior Locate(string entityName, string instance)
        {
            Assert.IsTrue(ServiceProvider.GetRequiredService<IEntityLocator>()
                .TryLocateEntity(new EntityClass(entityName).ToId(instance), out var entity));
            return entity;
        }

        [TestMethod]
        public async Task StatusRequestWithoutImplicitInitializationDoesNotInitialize()
        {
            var entity = Locate("status", "x");

            var info = await entity.RequestAsync(new EntityInfoRequest(), NoImplicitInitializationHeaders);
            var behaviorInfo = await entity.GetStatusAsync(NoImplicitInitializationHeaders);

            Assert.AreEqual(EventStatus.Consumed, info.Status);
            Assert.AreEqual(BehaviorStatus.NotInitialized, info.Response.BehaviorStatus);
            Assert.AreEqual(new EntityId("status", "x"), info.Response.Id);
            CollectionAssert.AreEqual(new[] { Event<Initialize>.Name }, info.Response.ExpectedEvents.ToArray());
            Assert.AreEqual(BehaviorStatus.NotInitialized, behaviorInfo.Response.BehaviorStatus);
            CollectionAssert.AreEqual(new[] { Event<Initialize>.Name }, behaviorInfo.Response.ExpectedEvents.ToArray());

            var mutation = await entity.SendAsync(new StatusIncrement(1), NoImplicitInitializationHeaders);
            Assert.AreEqual(EventStatus.NotInitialized, mutation.Status);

            info = await entity.RequestAsync(new EntityInfoRequest(), NoImplicitInitializationHeaders);
            Assert.AreEqual(BehaviorStatus.NotInitialized, info.Response.BehaviorStatus);
        }

        [TestMethod]
        public async Task StatusRequestWithImplicitInitializationInitializes()
        {
            var entity = Locate("status", "x");

            var info = await entity.RequestAsync(new EntityInfoRequest());

            Assert.AreEqual(EventStatus.Consumed, info.Status);
            Assert.AreEqual(BehaviorStatus.Initialized, info.Response.BehaviorStatus);
            CollectionAssert.AreEqual(new[] { Event<StatusIncrement>.Name }, info.Response.ExpectedEvents.ToArray());
        }

        [TestMethod]
        public async Task ExpectedEventsOfUninitializedEntityAreItsInitializers()
        {
            var entity = Locate("statusWithInitializer", "x");

            var info = await entity.RequestAsync(new EntityInfoRequest(), NoImplicitInitializationHeaders);
            Assert.AreEqual(BehaviorStatus.NotInitialized, info.Response.BehaviorStatus);
            CollectionAssert.AreEqual(new[] { Event<StatusOpen>.Name }, info.Response.ExpectedEvents.ToArray());

            Assert.AreEqual(EventStatus.Initialized, (await entity.SendAsync(new StatusOpen("owner"))).Status);

            info = await entity.RequestAsync(new EntityInfoRequest(), NoImplicitInitializationHeaders);
            Assert.AreEqual(BehaviorStatus.Initialized, info.Response.BehaviorStatus);
            CollectionAssert.AreEqual(new[] { Event<StatusIncrement>.Name }, info.Response.ExpectedEvents.ToArray());
            Assert.AreEqual("owner", (await entity.TryGetAsync<string>("Owner")).FieldValue);
        }

        [TestMethod]
        public async Task ResetClearsStateAndReturnsEntityToNotInitialized()
        {
            var entity = Locate("status", "x");
            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(new StatusIncrement(5))).Status);
            Assert.AreEqual(5, (await entity.TryGetAsync<int>("Count")).FieldValue);

            var reset = await entity.ResetAsync();

            Assert.AreEqual(EventStatus.Consumed, reset.Status);
            var info = await entity.RequestAsync(new EntityInfoRequest(), NoImplicitInitializationHeaders);
            Assert.AreEqual(BehaviorStatus.NotInitialized, info.Response.BehaviorStatus);

            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(new StatusIncrement(1))).Status);
            Assert.AreEqual(1, (await entity.TryGetAsync<int>("Count")).FieldValue);
        }

        [TestMethod]
        public async Task ResetOfUninitializedEntityIsConsumedAndDoesNotInitialize()
        {
            var entity = Locate("status", "fresh");

            var reset = await entity.ResetAsync();

            Assert.AreEqual(EventStatus.Consumed, reset.Status);
            var info = await entity.RequestAsync(new EntityInfoRequest(), NoImplicitInitializationHeaders);
            Assert.AreEqual(BehaviorStatus.NotInitialized, info.Response.BehaviorStatus);
        }

        [TestMethod]
        public async Task StatusChangesArePublished()
        {
            var entity = Locate("status", "x");
            var published = new List<EntityInfo>();
            await using var watcher = await entity.WatchAsync<EntityInfo>(info =>
            {
                lock (published)
                {
                    published.Add(info);
                }
            });

            await entity.SendAsync(new Initialize());
            await entity.SendAsync(new StatusIncrement(1));
            await entity.ResetAsync();

            var deadline = DateTime.UtcNow.AddSeconds(5);
            while (DateTime.UtcNow < deadline)
            {
                lock (published)
                {
                    if (published.Count >= 2)
                    {
                        break;
                    }
                }

                await Task.Delay(50);
            }

            lock (published)
            {
                CollectionAssert.AreEqual(
                    new[] { BehaviorStatus.Initialized, BehaviorStatus.NotInitialized },
                    published.Select(info => info.BehaviorStatus).ToArray(),
                    "Only status changes are published, mutations of an initialized entity are not."
                );
                CollectionAssert.AreEqual(new[] { Event<StatusIncrement>.Name }, published[0].ExpectedEvents.ToArray());
            }
        }
    }
}
