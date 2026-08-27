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

    public record SetInheritedProjectionBaseEvent(string Value);

    public record SetInheritedProjectionSuffixEvent(string Value);

    public record SetMultiInheritedProjectionFirstEvent(string Value);

    public record SetMultiInheritedProjectionSecondEvent(int Value);

    public interface IMultiInheritedProjectionEntityTemplateFirst
    {
        [Field]
        string FirstValue { get; set; }
    }

    public interface IMultiInheritedProjectionEntityTemplateSecond
    {
        [Field]
        int SecondValue { get; set; }
    }

    public interface IMultiInheritedProjectionEntityTemplate : IMultiInheritedProjectionEntityTemplateFirst, IMultiInheritedProjectionEntityTemplateSecond
    {
        [Field]
        string CombinedValue => $"{FirstValue}:{SecondValue}";
    }

    public record MultiInheritedProjectionSnapshot
    {
        public string? FirstValue { get; set; }

        public int SecondValue { get; set; }

        public string? CombinedValue { get; set; }
    }

    public record MultiInheritedProjectionNotification
    {
        public string? FirstValue { get; set; }

        public int SecondValue { get; set; }

        public string? CombinedValue { get; set; }
    }

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
        string ComputedName => Name;

        [Field]
        int DoubleQuantity => Quantity * 2;
    }

    public interface IInheritedProjectionEntityTemplateBase
    {
        [Field]
        string BaseValue { get; set; }
    }

    public interface IInheritedProjectionEntityTemplate : IInheritedProjectionEntityTemplateBase
    {
        [Field]
        string Suffix { get; set; }

        [Field]
        string CombinedValue => $"{BaseValue}:{Suffix}";
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

    public record InheritedProjectionSnapshot
    {
        public string? BaseValue { get; set; }

        public string? Suffix { get; set; }

        public string? CombinedValue { get; set; }
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
                            Name = template.ComputedName,
                            NameLength = template.NameLength,
                        }, PublishScope.Self)
                        .AddProjection<QuantityProjectionNotification>(template => new QuantityProjectionNotification
                        {
                            Quantity = template.Quantity,
                            DoubleQuantity = template.DoubleQuantity,
                        }, PublishScope.Self)
                    )
                    .AddEntity<IInheritedProjectionEntityTemplate>("inheritedProjectionEntity", entity => entity
                        .AddDefaultInitializer(context =>
                        {
                            context.Entity.BaseValue = "seed";
                            context.Entity.Suffix = "0";
                        })
                        .AddMutation<SetInheritedProjectionBaseEvent>(context => context.Entity.BaseValue = context.MutationEvent.Value)
                        .AddMutation<SetInheritedProjectionSuffixEvent>(context => context.Entity.Suffix = context.MutationEvent.Value)
                        .AddProjection<InheritedProjectionSnapshot>(template => new InheritedProjectionSnapshot
                        {
                            BaseValue = template.BaseValue,
                            Suffix = template.Suffix,
                            CombinedValue = template.CombinedValue,
                        })
                    )
                    .AddEntity<IMultiInheritedProjectionEntityTemplate>("multiInheritedProjectionEntity", entity => entity
                        .AddDefaultInitializer(context =>
                        {
                            context.Entity.FirstValue = "seed";
                            context.Entity.SecondValue = 1;
                        })
                        .AddMutation<SetMultiInheritedProjectionFirstEvent>(context => context.Entity.FirstValue = context.MutationEvent.Value)
                        .AddMutation<SetMultiInheritedProjectionSecondEvent>(context => context.Entity.SecondValue = context.MutationEvent.Value)
                        .AddProjection<MultiInheritedProjectionSnapshot>(template => new MultiInheritedProjectionSnapshot
                        {
                            FirstValue = template.FirstValue,
                            SecondValue = template.SecondValue,
                            CombinedValue = template.CombinedValue,
                        })
                        .AddProjection<MultiInheritedProjectionNotification>(template => new MultiInheritedProjectionNotification
                        {
                            FirstValue = template.FirstValue,
                            SecondValue = template.SecondValue,
                            CombinedValue = template.CombinedValue,
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

                // _ = await entity.TryGetProjectionAsync<NameProjectionNotification>();
                // _ = await entity.TryGetProjectionAsync<QuantityProjectionNotification>();

                toggleStatus = (await entity.SendAsync(new ToggleProjectionFlagEvent(true))).Status;
                await Task.Delay(100);
                renameStatus1 = (await entity.SendAsync(new RenameProjectionEvent("not updated"))).Status;
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
            Assert.AreEqual(2, namePublishCount);
            Assert.AreEqual(1, quantityPublishCount);
            Assert.IsNotNull(publishedNameProjection);
            Assert.AreEqual("updated", publishedNameProjection.Name);
            Assert.AreEqual(7, publishedNameProjection.NameLength);
            Assert.IsNotNull(publishedQuantityProjection);
            Assert.AreEqual(4, publishedQuantityProjection.Quantity);
            Assert.AreEqual(8, publishedQuantityProjection.DoubleQuantity);
        }

        [TestMethod]
        public async Task InheritedTemplateProjection_RequestReturnsBaseAndDerivedFields()
        {
            var success = false;
            InheritedProjectionSnapshot? projection = null;

            if (TryLocateEntity("inheritedProjectionEntity", "x", out var entity))
            {
                (success, projection) = await entity.TryGetProjectionAsync<InheritedProjectionSnapshot>();
            }

            Assert.IsTrue(success);
            Assert.IsNotNull(projection);
            Assert.AreEqual("seed", projection.BaseValue);
            Assert.AreEqual("0", projection.Suffix);
            Assert.AreEqual("seed:0", projection.CombinedValue);
        }

        [TestMethod]
        public async Task InheritedTemplateProjection_RecalculatesAfterBaseAndDerivedMutations()
        {
            var baseStatus = EventStatus.Undelivered;
            var suffixStatus = EventStatus.Undelivered;
            var success = false;
            InheritedProjectionSnapshot? projection = null;

            if (TryLocateEntity("inheritedProjectionEntity", "mutated", out var entity))
            {
                _ = await entity.TryGetProjectionAsync<InheritedProjectionSnapshot>();
                baseStatus = (await entity.SendAsync(new SetInheritedProjectionBaseEvent("alpha"))).Status;
                suffixStatus = (await entity.SendAsync(new SetInheritedProjectionSuffixEvent("beta"))).Status;
                (success, projection) = await entity.TryGetProjectionAsync<InheritedProjectionSnapshot>();
            }

            Assert.AreEqual(EventStatus.Consumed, baseStatus);
            Assert.AreEqual(EventStatus.Consumed, suffixStatus);
            Assert.IsTrue(success);
            Assert.IsNotNull(projection);
            Assert.AreEqual("alpha", projection.BaseValue);
            Assert.AreEqual("beta", projection.Suffix);
            Assert.AreEqual("alpha:beta", projection.CombinedValue);
        }

        [TestMethod]
        public async Task MultiInheritedTemplateProjection_RequestReturnsFirstSecondAndDerivedFields()
        {
            var success = false;
            MultiInheritedProjectionSnapshot? projection = null;

            if (TryLocateEntity("multiInheritedProjectionEntity", "x", out var entity))
            {
                (success, projection) = await entity.TryGetProjectionAsync<MultiInheritedProjectionSnapshot>();
            }

            Assert.IsTrue(success);
            Assert.IsNotNull(projection);
            Assert.AreEqual("seed", projection.FirstValue);
            Assert.AreEqual(1, projection.SecondValue);
            Assert.AreEqual("seed:1", projection.CombinedValue);
        }

        [TestMethod]
        public async Task MultiInheritedTemplateProjection_RecalculatesAfterFirstAndSecondMutations()
        {
            var firstStatus = EventStatus.Undelivered;
            var secondStatus = EventStatus.Undelivered;
            var success = false;
            MultiInheritedProjectionSnapshot? projection = null;

            if (TryLocateEntity("multiInheritedProjectionEntity", "mutated", out var entity))
            {
                _ = await entity.TryGetProjectionAsync<MultiInheritedProjectionSnapshot>();
                firstStatus = (await entity.SendAsync(new SetMultiInheritedProjectionFirstEvent("alpha"))).Status;
                secondStatus = (await entity.SendAsync(new SetMultiInheritedProjectionSecondEvent(42))).Status;
                (success, projection) = await entity.TryGetProjectionAsync<MultiInheritedProjectionSnapshot>();
            }

            Assert.AreEqual(EventStatus.Consumed, firstStatus);
            Assert.AreEqual(EventStatus.Consumed, secondStatus);
            Assert.IsTrue(success);
            Assert.IsNotNull(projection);
            Assert.AreEqual("alpha", projection.FirstValue);
            Assert.AreEqual(42, projection.SecondValue);
            Assert.AreEqual("alpha:42", projection.CombinedValue);
        }

        [TestMethod]
        public async Task MultiInheritedTemplateProjection_CachesDependenciesCorrectly()
        {
            var success = false;
            MultiInheritedProjectionSnapshot? projection = null;

            if (TryLocateEntity("multiInheritedProjectionEntity", "depends", out var entity))
            {
                (success, projection) = await entity.TryGetProjectionAsync<MultiInheritedProjectionSnapshot>();
            }

            Assert.IsTrue(success);
            Assert.IsNotNull(projection);

            var context = await HydrateContextAsync("multiInheritedProjectionEntity", "depends");
            CollectionAssert.AreEquivalent(
                new[] { nameof(IMultiInheritedProjectionEntityTemplate.FirstValue), nameof(IMultiInheritedProjectionEntityTemplate.SecondValue), nameof(IMultiInheritedProjectionEntityTemplate.CombinedValue) },
                GetStringArrayValue(context, $"$dependencies:projection:{typeof(MultiInheritedProjectionSnapshot).AssemblyQualifiedName}")
            );
            CollectionAssert.AreEquivalent(
                new[] { nameof(IMultiInheritedProjectionEntityTemplateFirst.FirstValue), nameof(IMultiInheritedProjectionEntityTemplateSecond.SecondValue) },
                GetStringArrayValue(context, "$dependencies:field:CombinedValue")
            );
        }

        [TestMethod]
        public async Task MultiInheritedTemplateProjection_PublishesChangesAfterMutations()
        {
            var sync = new object();
            MultiInheritedProjectionNotification? publishedProjection = null;
            var publishCount = 0;
            var firstStatus = EventStatus.Undelivered;
            var secondStatus = EventStatus.Undelivered;

            if (TryLocateEntity("multiInheritedProjectionEntity", "published", out var entity))
            {
                await using var watcher = await entity.WatchAsync<MultiInheritedProjectionNotification>(projection =>
                {
                    lock (sync)
                    {
                        publishedProjection = projection;
                        publishCount++;
                    }
                });

                firstStatus = (await entity.SendAsync(new SetMultiInheritedProjectionFirstEvent("alpha"))).Status;
                await Task.Delay(100);
                secondStatus = (await entity.SendAsync(new SetMultiInheritedProjectionSecondEvent(42))).Status;
                await Task.Delay(100);
            }

            Assert.AreEqual(EventStatus.Consumed, firstStatus);
            Assert.AreEqual(EventStatus.Consumed, secondStatus);
            Assert.AreEqual(2, publishCount);
            Assert.IsNotNull(publishedProjection);
            Assert.AreEqual("alpha", publishedProjection.FirstValue);
            Assert.AreEqual(42, publishedProjection.SecondValue);
            Assert.AreEqual("alpha:42", publishedProjection.CombinedValue);
        }
    }
}
