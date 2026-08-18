using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Common.Context;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;

namespace Entity.IntegrationTests.Tests
{
    public record RenameProjectionEvent(string Name);

    public record SetProjectionQuantityEvent(int Quantity);

    public record ToggleProjectionFlagEvent(bool Flag);

    public interface IProjectionEntityTemplate
    {
        [Field]
        string Name { get; set; }

        [Field]
        int Quantity { get; set; }

        [Field]
        bool Flag { get; set; }

        [Field]
        int NameLength => Name.Length;

        [Field]
        int DoubleQuantity => Quantity * 2;
    }

    public record ProjectionSnapshot
    {
        public string? Name { get; set; }

        public int Quantity { get; set; }

        public int NameLength { get; set; }

        public int DoubleQuantity { get; set; }
    }

    public record NameProjectionNotification
    {
        public string? Name { get; set; }

        public int NameLength { get; set; }
    }

    public record QuantityProjectionNotification
    {
        public int Quantity { get; set; }

        public int DoubleQuantity { get; set; }
    }

    [TestClass]
    public class Projection : StateflowsTestClass
    {
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
                    .AddEntity<IProjectionEntityTemplate>("projectionEntity", entity => entity
                        .AddDefaultInitializer(context =>
                        {
                            context.Entity.Name = "seed";
                            context.Entity.Quantity = 1;
                            context.Entity.Flag = false;
                        })
                        .AddMutation<RenameProjectionEvent>(context => context.Entity.Name = context.MutationEvent.Name)
                        .AddMutation<SetProjectionQuantityEvent>(context => context.Entity.Quantity = context.MutationEvent.Quantity)
                        .AddMutation<ToggleProjectionFlagEvent>(context => context.Entity.Flag = context.MutationEvent.Flag)
                        .AddProjection<ProjectionSnapshot>(template => new ProjectionSnapshot
                        {
                            Name = template.Name,
                            Quantity = template.Quantity,
                            NameLength = template.NameLength,
                            DoubleQuantity = template.DoubleQuantity,
                        })
                        .AddProjection<NameProjectionNotification>(template => new NameProjectionNotification
                        {
                            Name = template.Name,
                            NameLength = template.NameLength,
                        }, PublishScope.Self)
                        .AddProjection<QuantityProjectionNotification>(template => new QuantityProjectionNotification
                        {
                            Quantity = template.Quantity,
                            DoubleQuantity = template.DoubleQuantity,
                        }, PublishScope.Self)
                    )
                );
        }

        private IEntityLocator Locator => ServiceProvider.GetRequiredService<IEntityLocator>();

        private async Task<StateflowsContext> HydrateContextAsync(string entityName, string instance)
        {
            var storage = ServiceProvider.GetRequiredService<IStateflowsStorage>();
            var tenantAccessor = ServiceProvider.GetRequiredService<ITenantAccessor>();
            var tenantProvider = ServiceProvider.GetRequiredService<IStateflowsTenantProvider>();
            tenantAccessor.CurrentTenantId = await tenantProvider.GetCurrentTenantIdAsync();
            return await storage.HydrateAsync(new EntityClass(entityName).ToId(instance));
        }

        private bool TryLocateEntity(string entityName, string instance, out IEntityBehavior behavior)
            => Locator.TryLocateEntity(new EntityClass(entityName).ToId(instance), out behavior);

        private static string[] GetStringArrayValue(StateflowsContext context, string key)
            => context.Values.TryGetValue(key, out var value)
                ? value switch
                {
                    string[] array => array,
                    IEnumerable<string> enumerable => enumerable.ToArray(),
                    _ => [],
                }
                : [];

        [TestMethod]
        public async Task BuilderProjection_RequestReturnsCurrentStateAndCachesDependencies()
        {
            var success = false;
            ProjectionSnapshot? projection = null;

            if (TryLocateEntity("projectionEntity", "x", out var entity))
            {
                (success, projection) = await entity.TryGetProjectionAsync<ProjectionSnapshot>();
            }

            Assert.IsTrue(success);
            Assert.IsNotNull(projection);
            Assert.AreEqual("seed", projection.Name);
            Assert.AreEqual(1, projection.Quantity);
            Assert.AreEqual(4, projection.NameLength);
            Assert.AreEqual(2, projection.DoubleQuantity);

            var context = await HydrateContextAsync("projectionEntity", "x");
            CollectionAssert.AreEquivalent(
                new[] { nameof(IProjectionEntityTemplate.DoubleQuantity), nameof(IProjectionEntityTemplate.Name), nameof(IProjectionEntityTemplate.NameLength), nameof(IProjectionEntityTemplate.Quantity) },
                GetStringArrayValue(context, $"$dependencies:projection:{typeof(ProjectionSnapshot).AssemblyQualifiedName}")
            );
            CollectionAssert.AreEquivalent(
                new[] { nameof(IProjectionEntityTemplate.Name) },
                GetStringArrayValue(context, "$dependencies:field:NameLength")
            );
            CollectionAssert.AreEquivalent(
                new[] { nameof(IProjectionEntityTemplate.Quantity) },
                GetStringArrayValue(context, "$dependencies:field:DoubleQuantity")
            );
            Assert.IsTrue(context.Values.ContainsKey($"$projection:{typeof(ProjectionSnapshot).AssemblyQualifiedName}"));
        }

        [TestMethod]
        public async Task BuilderProjection_RecalculatesAfterDependentMutations()
        {
            var renameStatus = EventStatus.Undelivered;
            var quantityStatus = EventStatus.Undelivered;
            var success = false;
            ProjectionSnapshot? projection = null;

            if (TryLocateEntity("projectionEntity", "x", out var entity))
            {
                _ = await entity.TryGetProjectionAsync<ProjectionSnapshot>();
                renameStatus = (await entity.SendAsync(new RenameProjectionEvent("renamed"))).Status;
                quantityStatus = (await entity.SendAsync(new SetProjectionQuantityEvent(3))).Status;
                (success, projection) = await entity.TryGetProjectionAsync<ProjectionSnapshot>();
            }

            Assert.AreEqual(EventStatus.Consumed, renameStatus);
            Assert.AreEqual(EventStatus.Consumed, quantityStatus);
            Assert.IsTrue(success);
            Assert.IsNotNull(projection);
            Assert.AreEqual("renamed", projection.Name);
            Assert.AreEqual(3, projection.Quantity);
            Assert.AreEqual(7, projection.NameLength);
            Assert.AreEqual(6, projection.DoubleQuantity);

            var context = await HydrateContextAsync("projectionEntity", "x");
            Assert.AreEqual("renamed", (context.Values["$field:Name"] as string));
            Assert.AreEqual(3, context.Values["$field:Quantity"] is long longQuantity ? longQuantity : (int)context.Values["$field:Quantity"]);
            Assert.IsTrue(context.Values.TryGetValue($"$projection:{typeof(ProjectionSnapshot).AssemblyQualifiedName}", out var cachedProjection));
            Assert.IsInstanceOfType<ProjectionSnapshot>(cachedProjection);
            var cachedSnapshot = (ProjectionSnapshot)cachedProjection;
            Assert.AreEqual("renamed", cachedSnapshot.Name);
            Assert.AreEqual(3, cachedSnapshot.Quantity);
            Assert.AreEqual(7, cachedSnapshot.NameLength);
            Assert.AreEqual(6, cachedSnapshot.DoubleQuantity);
        }

        [TestMethod]
        public async Task BuilderProjection_PublishesOnlyAffectedProjectionChanges()
        {
            var sync = new object();
            NameProjectionNotification? publishedNameProjection = null;
            QuantityProjectionNotification? publishedQuantityProjection = null;
            var namePublishCount = 0;
            var quantityPublishCount = 0;
            var toggleStatus = EventStatus.Undelivered;
            var renameStatus1 = EventStatus.Undelivered;
            var renameStatus2 = EventStatus.Undelivered;
            var quantityStatus1 = EventStatus.Undelivered;
            var quantityStatus2 = EventStatus.Undelivered;

            if (TryLocateEntity("projectionEntity", "x", out var entity))
            {
                await using var nameWatcher = await entity.WatchAsync<NameProjectionNotification>(projection =>
                {
                    lock (sync)
                    {
                        publishedNameProjection = projection;
                        namePublishCount++;
                    }
                });
                await using var quantityWatcher = await entity.WatchAsync<QuantityProjectionNotification>(projection =>
                {
                    lock (sync)
                    {
                        publishedQuantityProjection = projection;
                        quantityPublishCount++;
                    }
                });

                _ = await entity.TryGetProjectionAsync<NameProjectionNotification>();
                _ = await entity.TryGetProjectionAsync<QuantityProjectionNotification>();

                toggleStatus = (await entity.SendAsync(new ToggleProjectionFlagEvent(true))).Status;
                await Task.Delay(100);
                renameStatus1 = (await entity.SendAsync(new RenameProjectionEvent("updated"))).Status;
                await Task.Delay(100);
                renameStatus2 = (await entity.SendAsync(new RenameProjectionEvent("updated"))).Status;
                await Task.Delay(100);
                quantityStatus1 = (await entity.SendAsync(new SetProjectionQuantityEvent(4))).Status;
                await Task.Delay(100);
                quantityStatus2 = (await entity.SendAsync(new SetProjectionQuantityEvent(4))).Status;
                await Task.Delay(100);
            }

            Assert.AreEqual(EventStatus.Consumed, toggleStatus);
            Assert.AreEqual(EventStatus.Consumed, renameStatus1);
            Assert.AreEqual(EventStatus.Consumed, renameStatus2);
            Assert.AreEqual(EventStatus.Consumed, quantityStatus1);
            Assert.AreEqual(EventStatus.Consumed, quantityStatus2);
            Assert.AreEqual(1, namePublishCount);
            Assert.AreEqual(1, quantityPublishCount);
            Assert.IsNotNull(publishedNameProjection);
            Assert.AreEqual("updated", publishedNameProjection.Name);
            Assert.AreEqual(7, publishedNameProjection.NameLength);
            Assert.IsNotNull(publishedQuantityProjection);
            Assert.AreEqual(4, publishedQuantityProjection.Quantity);
            Assert.AreEqual(8, publishedQuantityProjection.DoubleQuantity);
        }
    }
}





