using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Common.Context;
using Stateflows.Entities.Attributes;

namespace Entity.IntegrationTests.Tests
{
    public interface IAttributedEntityTemplate
    {
        [Field]
        int IntValue { get; set; }
    }

    public interface ITypedEntityTemplate
    {
        int IntValue { get; set; }
        string StringValue { get; set; }
    }

    public interface IAutoTypedEntityTemplate
    {
        [Field]
        string StringValue { get; set; }

        [Field]
        int Length => StringValue.Length;

        [Projection]
        AutoTypedProjection Snapshot
            => new()
            {
                StringValue = StringValue,
                Length = Length,
            };

        [Mutation]
        void Rename(AutoTypedMutationEvent mutation)
        {
            StringValue = mutation.Value;
        }
    }

    public interface IInheritedAutoTypedEntityTemplateBase
    {
        [Field]
        string BaseValue { get; set; }

        [Mutation]
        void RenameBase(InheritedAutoTypedRenameBaseMutationEvent mutation)
        {
            BaseValue = mutation.Value;
        }
    }

    public interface IInheritedAutoTypedEntityTemplate : IInheritedAutoTypedEntityTemplateBase
    {
        [Field]
        string Suffix { get; set; }

        [Field]
        string CombinedValue => $"{BaseValue}:{Suffix}";

        [Projection]
        InheritedAutoTypedProjection Snapshot
            => new()
            {
                BaseValue = BaseValue,
                Suffix = Suffix,
                CombinedValue = CombinedValue,
            };

        [Mutation]
        void AppendSuffix(InheritedAutoTypedAppendSuffixMutationEvent mutation)
        {
            Suffix += mutation.Value;
        }
    }

    public interface IDefaultValueEntityTemplate
    {
        [Field]
        [DefaultValue("Lorem ipsum")]
        string StringValue { get; set; }

        [Field]
        [DefaultValue(7)]
        int IntValue { get; set; }
    }

    public interface IInvalidDefaultValueEntityTemplate
    {
        [Field]
        [DefaultValue("not-an-int")]
        int IntValue { get; set; }
    }

    public interface IBuilderDefaultValueEntityTemplate
    {
        string StringValue { get; set; }

        int IntValue { get; set; }
    }

    public interface IBuilderComputedDefaultValueEntityTemplate
    {
        int IntValue { get; set; }
    }

    [EntityBehavior("attributedEntity", 3)]
    public class AttributedEntity : IEntity<IAttributedEntityTemplate>
    {
        public static void Build(IEntityBuilder<IAttributedEntityTemplate> builder)
        { }
    }

    public class TypedEntity : IEntity<ITypedEntityTemplate>
    {
        public static void Build(IEntityBuilder<ITypedEntityTemplate> builder)
        {
            builder
                .AddDefaultInitializer(_ => { })
                .AddInitializer<TypedInitializationEvent>(c => c.Entity.StringValue = "Lorem Ipsum")
                .AddField(t => t.IntValue, b => b
                    .AddComputation(x => x.StringValue.Length)
                )
                .AddMutation<TypedMutation>(_ => { });
        }
    }

    public class AutoTypedEntity : IEntity<IAutoTypedEntityTemplate>
    {
        public static void Build(IEntityBuilder<IAutoTypedEntityTemplate> builder)
        { }
    }

    public class InheritedAutoTypedEntity : IEntity<IInheritedAutoTypedEntityTemplate>
    {
        public static void Build(IEntityBuilder<IInheritedAutoTypedEntityTemplate> builder)
        { }
    }

    public record TypedMutation;
    public record TypedInitializationEvent;
    public record AutoTypedMutationEvent(string Value);
    public record InheritedAutoTypedRenameBaseMutationEvent(string Value);
    public record InheritedAutoTypedAppendSuffixMutationEvent(string Value);

    public record AutoTypedProjection
    {
        public string? StringValue { get; set; }

        public int Length { get; set; }
    }

    public record InheritedAutoTypedProjection
    {
        public string? BaseValue { get; set; }

        public string? Suffix { get; set; }

        public string? CombinedValue { get; set; }
    }

    public class RecordingEntityVisitor : EntityVisitor
    {
        public List<string> AddedEntities { get; } = [];
        public List<Type> AddedTypes { get; } = [];

        public override Task EntityAddedAsync<TTemplate>(string entityName, int entityVersion)
        {
            AddedEntities.Add($"{entityName}.{entityVersion}");
            return Task.CompletedTask;
        }

        public override Task EntityTypeAddedAsync<TTemplate, TEntity>(string entityName, int entityVersion)
        {
            AddedTypes.Add(typeof(TEntity));
            return Task.CompletedTask;
        }
    }

    [TestClass]
    public class Registration : StateflowsTestClass
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
                    .AddEntity<ITypedEntityTemplate>("plain", entity => entity
                        .AddDefaultInitializer(_ => { })
                        .AddInitializer<PlainInitializationEvent>(_ => { })
                        .AddField(t => t.IntValue,field => field
                            .AddComputation(_ => 7)
                        )
                        .AddMutation<PlainMutation>(_ => { })
                    )
                    .AddEntity<ITypedEntityTemplate, TypedEntity>("typed")
                    .AddEntity<IAutoTypedEntityTemplate, AutoTypedEntity>("typedAuto", buildAction: entity => entity
                        .AddDefaultInitializer(context => context.Entity.StringValue = string.Empty)
                    )
                    .AddEntity<IInheritedAutoTypedEntityTemplate, InheritedAutoTypedEntity>("typedAutoInherited", buildAction: entity => entity
                        .AddDefaultInitializer(context =>
                        {
                            context.Entity.BaseValue = string.Empty;
                            context.Entity.Suffix = string.Empty;
                        })
                    )
                    .AddEntity<IDefaultValueEntityTemplate>("typedDefaults")
                    .AddEntity<IBuilderDefaultValueEntityTemplate>("builderDefaults", entity => entity
                        .AddField(t => t.StringValue, field => field.AddDefaultValue("configured in builder"))
                        .AddField(t => t.IntValue, field => field.AddDefaultValue(11))
                    )
                    .AddFromAssembly(Assembly.GetExecutingAssembly())
                )
                ;
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

        public record PlainMutation;
        public record PlainInitializationEvent;

        [TestMethod]
        public async Task RegistersEntitiesAndPublishesBehaviorClasses()
        {
            var register = ServiceProvider.GetRequiredService<IEntitiesRegister>();
            var provider = ServiceProvider.GetRequiredService<IBehaviorClassesProvider>();
            var visitor = new RecordingEntityVisitor();

            await register.VisitEntitiesAsync(visitor);

            CollectionAssert.Contains(visitor.AddedEntities, "plain.1");
            CollectionAssert.Contains(visitor.AddedEntities, "typed.1");
            CollectionAssert.Contains(visitor.AddedEntities, "typedAuto.1");
            CollectionAssert.Contains(visitor.AddedEntities, "typedAutoInherited.1");
            CollectionAssert.Contains(visitor.AddedEntities, "typedDefaults.1");
            CollectionAssert.Contains(visitor.AddedEntities, "builderDefaults.1");
            CollectionAssert.Contains(visitor.AddedEntities, "attributedEntity.3");
            Assert.IsTrue(visitor.AddedTypes.Contains(typeof(TypedEntity)));
            Assert.IsTrue(visitor.AddedTypes.Contains(typeof(AutoTypedEntity)));
            Assert.IsTrue(visitor.AddedTypes.Contains(typeof(InheritedAutoTypedEntity)));
            Assert.IsTrue(visitor.AddedTypes.Contains(typeof(AttributedEntity)));

            var allClasses = provider.AllBehaviorClasses.ToList();
            CollectionAssert.Contains(allClasses, new BehaviorClass(BehaviorType.Entity, "plain"));
            CollectionAssert.Contains(allClasses, new BehaviorClass(BehaviorType.Entity, "typed"));
            CollectionAssert.Contains(allClasses, new BehaviorClass(BehaviorType.Entity, "typedAuto"));
            CollectionAssert.Contains(allClasses, new BehaviorClass(BehaviorType.Entity, "typedAutoInherited"));
            CollectionAssert.Contains(allClasses, new BehaviorClass(BehaviorType.Entity, "typedDefaults"));
            CollectionAssert.Contains(allClasses, new BehaviorClass(BehaviorType.Entity, "builderDefaults"));
            CollectionAssert.Contains(allClasses, new BehaviorClass(BehaviorType.Entity, "attributedEntity"));

            var entitiesProperty = register.GetType().GetField("Entities");
            var entities = (System.Collections.IDictionary)entitiesProperty!.GetValue(register)!;
            var plainRegistration = entities["plain.current"]!;
            var typedRegistration = entities["typed.current"]!;
            var typedAutoRegistration = entities["typedAuto.current"]!;
            var typedDefaultsRegistration = entities["typedDefaults.current"]!;
            var builderDefaultsRegistration = entities["builderDefaults.current"]!;
            var attributedRegistration = entities["attributedEntity.current"]!;
            var plainModel = plainRegistration.GetType().GetProperty("Model")!.GetValue(plainRegistration)!;
            var typedModel = typedRegistration.GetType().GetProperty("Model")!.GetValue(typedRegistration)!;
            var typedAutoModel = typedAutoRegistration.GetType().GetProperty("Model")!.GetValue(typedAutoRegistration)!;
            var typedDefaultsModel = typedDefaultsRegistration.GetType().GetProperty("Model")!.GetValue(typedDefaultsRegistration)!;
            var builderDefaultsModel = builderDefaultsRegistration.GetType().GetProperty("Model")!.GetValue(builderDefaultsRegistration)!;
            var attributedModel = attributedRegistration.GetType().GetProperty("Model")!.GetValue(attributedRegistration)!;

            var plainFields = (System.Collections.IDictionary)plainModel.GetType().GetProperty("Fields")!.GetValue(plainModel)!;
            var plainMutations = (System.Collections.IDictionary)plainModel.GetType().GetProperty("Mutations")!.GetValue(plainModel)!;
            var typedFields = (System.Collections.IDictionary)typedModel.GetType().GetProperty("Fields")!.GetValue(typedModel)!;
            var typedMutations = (System.Collections.IDictionary)typedModel.GetType().GetProperty("Mutations")!.GetValue(typedModel)!;
            var typedAutoFields = (System.Collections.IDictionary)typedAutoModel.GetType().GetProperty("Fields")!.GetValue(typedAutoModel)!;
            var typedAutoMutations = (System.Collections.IDictionary)typedAutoModel.GetType().GetProperty("Mutations")!.GetValue(typedAutoModel)!;
            var typedAutoProjections = (System.Collections.IDictionary)typedAutoModel.GetType().GetProperty("Projections")!.GetValue(typedAutoModel)!;
            var typedDefaultsFields = (System.Collections.IDictionary)typedDefaultsModel.GetType().GetProperty("Fields")!.GetValue(typedDefaultsModel)!;
            var builderDefaultsFields = (System.Collections.IDictionary)builderDefaultsModel.GetType().GetProperty("Fields")!.GetValue(builderDefaultsModel)!;
            var attributedFields = (System.Collections.IDictionary)attributedModel.GetType().GetProperty("Fields")!.GetValue(attributedModel)!;

            var plainFieldModel = plainFields["IntValue"]!;
            var typedFieldModel = typedFields["IntValue"]!;
            var typedAutoFieldModel = typedAutoFields["Length"]!;
            var plainComputation = plainFieldModel.GetType().GetProperty("Computation")!.GetValue(plainFieldModel);
            var typedComputation = typedFieldModel.GetType().GetProperty("Computation")!.GetValue(typedFieldModel);
            var typedAutoComputation = typedAutoFieldModel.GetType().GetProperty("Computation")!.GetValue(typedAutoFieldModel);
            var typedDefaultsStringFieldModel = typedDefaultsFields["StringValue"]!;
            var typedDefaultsIntFieldModel = typedDefaultsFields["IntValue"]!;
            var builderDefaultsStringFieldModel = builderDefaultsFields["StringValue"]!;
            var builderDefaultsIntFieldModel = builderDefaultsFields["IntValue"]!;

            var plainInitializers = (System.Collections.IDictionary)plainModel.GetType().GetProperty("Initializers")!.GetValue(plainModel)!;
            var typedInitializers = (System.Collections.IDictionary)typedModel.GetType().GetProperty("Initializers")!.GetValue(typedModel)!;
            var plainDefaultInitializers = (System.Collections.ICollection)plainModel.GetType().GetProperty("DefaultInitializerInvoke")!.GetValue(plainModel)!;
            var typedDefaultInitializers = (System.Collections.ICollection)typedModel.GetType().GetProperty("DefaultInitializerInvoke")!.GetValue(typedModel)!;
            var typedAutoDefaultInitializers = (System.Collections.ICollection)typedAutoModel.GetType().GetProperty("DefaultInitializerInvoke")!.GetValue(typedAutoModel)!;

            Assert.IsTrue(plainFields.Contains("IntValue"));
            Assert.IsNotNull(plainComputation);
            Assert.IsTrue(plainMutations.Contains(typeof(PlainMutation)));
            Assert.AreEqual(1, plainDefaultInitializers.Count);
            Assert.IsTrue(plainInitializers.Contains(typeof(PlainInitializationEvent)));

            Assert.IsTrue(typedFields.Contains("IntValue"));
            Assert.IsNotNull(typedComputation);
            Assert.IsNull(typedFieldModel.GetType().GetProperty("ComputationTriggers"));
            Assert.IsTrue(typedMutations.Contains(typeof(TypedMutation)));
            Assert.AreEqual(1, typedDefaultInitializers.Count);
            Assert.IsTrue(typedInitializers.Contains(typeof(TypedInitializationEvent)));

            Assert.IsTrue(typedAutoFields.Contains("StringValue"));
            Assert.IsTrue(typedAutoFields.Contains("Length"));
            Assert.IsNotNull(typedAutoComputation);
            Assert.IsTrue(typedAutoMutations.Contains(typeof(AutoTypedMutationEvent)));
            Assert.IsTrue(typedAutoProjections.Contains(typeof(AutoTypedProjection)));
            Assert.AreEqual(1, typedAutoDefaultInitializers.Count);

            Assert.IsTrue(typedDefaultsFields.Contains("StringValue"));
            Assert.IsTrue(typedDefaultsFields.Contains("IntValue"));
            Assert.AreEqual(true, typedDefaultsStringFieldModel.GetType().GetProperty("HasDefaultValue")!.GetValue(typedDefaultsStringFieldModel));
            Assert.AreEqual("Lorem ipsum", typedDefaultsStringFieldModel.GetType().GetProperty("DefaultValue")!.GetValue(typedDefaultsStringFieldModel));
            Assert.AreEqual(true, typedDefaultsIntFieldModel.GetType().GetProperty("HasDefaultValue")!.GetValue(typedDefaultsIntFieldModel));
            Assert.AreEqual(7, typedDefaultsIntFieldModel.GetType().GetProperty("DefaultValue")!.GetValue(typedDefaultsIntFieldModel));

            Assert.IsTrue(builderDefaultsFields.Contains("StringValue"));
            Assert.IsTrue(builderDefaultsFields.Contains("IntValue"));
            Assert.AreEqual(true, builderDefaultsStringFieldModel.GetType().GetProperty("HasDefaultValue")!.GetValue(builderDefaultsStringFieldModel));
            Assert.AreEqual("configured in builder", builderDefaultsStringFieldModel.GetType().GetProperty("DefaultValue")!.GetValue(builderDefaultsStringFieldModel));
            Assert.AreEqual(true, builderDefaultsIntFieldModel.GetType().GetProperty("HasDefaultValue")!.GetValue(builderDefaultsIntFieldModel));
            Assert.AreEqual(11, builderDefaultsIntFieldModel.GetType().GetProperty("DefaultValue")!.GetValue(builderDefaultsIntFieldModel));

            Assert.IsTrue(attributedFields.Contains("IntValue"));
        }

        [TestMethod]
        public async Task TypedRegistrationAutoAnalyzesAnnotatedTemplate()
        {
            var storage = ServiceProvider.GetRequiredService<IStateflowsStorage>();
            var tenantAccessor = ServiceProvider.GetRequiredService<ITenantAccessor>();
            var tenantProvider = ServiceProvider.GetRequiredService<IStateflowsTenantProvider>();
            var status1 = EventStatus.Undelivered;
            var status2 = EventStatus.Undelivered;
            var success1 = false;
            AutoTypedProjection? projection = null;
            long length = 0;
            string? fieldValue = null;

            if (TryLocateEntity("typedAuto", "x", out var entity))
            {
                status1 = (await entity.SendAsync(new Initialize())).Status;
                status2 = (await entity.SendAsync(new AutoTypedMutationEvent("Lorem"))).Status;
                (success1, projection) = await entity.TryGetProjectionAsync<AutoTypedProjection>();
            }

            Assert.AreEqual(EventStatus.Initialized, status1);
            Assert.AreEqual(EventStatus.Consumed, status2);
            Assert.IsTrue(success1);
            Assert.IsNotNull(projection);
            Assert.AreEqual("Lorem", projection.StringValue);
            Assert.AreEqual(5, projection.Length);

            tenantAccessor.CurrentTenantId = await tenantProvider.GetCurrentTenantIdAsync();
            var context = await storage.HydrateAsync(new EntityClass("typedAuto").ToId("x"));
            length = context.Values.TryGetValue("$field:Length", out var lengthValue)
                ? lengthValue switch
                {
                    long longValue => longValue,
                    int integerValue => integerValue,
                    _ => 0,
                }
                : 0;
            fieldValue = context.Values.TryGetValue("$field:StringValue", out var stringValue)
                ? stringValue as string
                : null;
            Assert.AreEqual(5, length);
            Assert.AreEqual("Lorem", fieldValue);
            Assert.IsTrue(context.Values.ContainsKey("$dependencies:field:Length"));
            Assert.IsTrue(context.Values.ContainsKey($"$dependencies:projection:{typeof(AutoTypedProjection).AssemblyQualifiedName}"));
            Assert.IsTrue(context.Values.ContainsKey($"$projection:{typeof(AutoTypedProjection).AssemblyQualifiedName}"));
        }

        [TestMethod]
        public async Task TypedRegistrationAutoAnalyzesAnnotatedInheritedTemplate()
        {
            var storage = ServiceProvider.GetRequiredService<IStateflowsStorage>();
            var tenantAccessor = ServiceProvider.GetRequiredService<ITenantAccessor>();
            var tenantProvider = ServiceProvider.GetRequiredService<IStateflowsTenantProvider>();
            var status1 = EventStatus.Undelivered;
            var status2 = EventStatus.Undelivered;
            var status3 = EventStatus.Undelivered;
            var success = false;
            InheritedAutoTypedProjection? projection = null;
            string? baseValue = null;
            string? suffix = null;

            if (TryLocateEntity("typedAutoInherited", "x", out var entity))
            {
                var r = await entity.SendAsync(new Initialize());
                status1 = (r).Status;
                status2 = (await entity.SendAsync(new InheritedAutoTypedRenameBaseMutationEvent("Lorem"))).Status;
                status3 = (await entity.SendAsync(new InheritedAutoTypedAppendSuffixMutationEvent("Ipsum"))).Status;
                (success, projection) = await entity.TryGetProjectionAsync<InheritedAutoTypedProjection>();
            }

            Assert.AreEqual(EventStatus.Initialized, status1);
            Assert.AreEqual(EventStatus.Consumed, status2);
            Assert.AreEqual(EventStatus.Consumed, status3);
            Assert.IsTrue(success);
            Assert.IsNotNull(projection);
            Assert.AreEqual("Lorem", projection.BaseValue);
            Assert.AreEqual("Ipsum", projection.Suffix);
            Assert.AreEqual("Lorem:Ipsum", projection.CombinedValue);

            tenantAccessor.CurrentTenantId = await tenantProvider.GetCurrentTenantIdAsync();
            var context = await storage.HydrateAsync(new EntityClass("typedAutoInherited").ToId("x"));
            baseValue = context.Values.TryGetValue("$field:BaseValue", out var baseValueObject)
                ? baseValueObject as string
                : null;
            suffix = context.Values.TryGetValue("$field:Suffix", out var suffixValueObject)
                ? suffixValueObject as string
                : null;

            Assert.AreEqual("Lorem", baseValue);
            Assert.AreEqual("Ipsum", suffix);
            Assert.IsTrue(context.Values.ContainsKey("$field:CombinedValue"));
            Assert.IsTrue(context.Values.ContainsKey($"$dependencies:projection:{typeof(InheritedAutoTypedProjection).AssemblyQualifiedName}"));
            Assert.IsTrue(context.Values.ContainsKey($"$projection:{typeof(InheritedAutoTypedProjection).AssemblyQualifiedName}"));
        }

        [TestMethod]
        public async Task VisitsSingleEntityByNameAndVersion()
        {
            var register = ServiceProvider.GetRequiredService<IEntitiesRegister>();
            var visitor = new RecordingEntityVisitor();

            await register.VisitEntityAsync("attributedEntity", 3, visitor);

            CollectionAssert.AreEqual(new List<string> { "attributedEntity.3" }, visitor.AddedEntities);
            Assert.AreEqual(1, visitor.AddedTypes.Count);
            Assert.AreEqual(typeof(AttributedEntity), visitor.AddedTypes[0]);
        }

        [TestMethod]
        public async Task AttributeDefaultValues_AreAppliedAtInitialization()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("typedDefaults", "x", out var entity))
            {
                status = (await entity.SendAsync(new Initialize())).Status;
            }

            Assert.AreEqual(EventStatus.Initialized, status);

            var context = await HydrateContextAsync("typedDefaults", "x");
            var stringValue = context.Values.TryGetValue("$field:StringValue", out var stringObject)
                ? stringObject as string
                : null;
            var intValue = context.Values.TryGetValue("$field:IntValue", out var intObject)
                ? intObject switch
                {
                    long longValue => longValue,
                    int integerValue => integerValue,
                    _ => 0,
                }
                : 0;

            Assert.AreEqual("Lorem ipsum", stringValue);
            Assert.AreEqual(7, intValue);
        }

        [TestMethod]
        public async Task BuilderDefaultValues_AreAppliedAtInitialization()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("builderDefaults", "x", out var entity))
            {
                status = (await entity.SendAsync(new Initialize())).Status;
            }

            Assert.AreEqual(EventStatus.Initialized, status);

            var context = await HydrateContextAsync("builderDefaults", "x");
            var stringValue = context.Values.TryGetValue("$field:StringValue", out var stringObject)
                ? stringObject as string
                : null;
            var intValue = context.Values.TryGetValue("$field:IntValue", out var intObject)
                ? intObject switch
                {
                    long longValue => longValue,
                    int integerValue => integerValue,
                    _ => 0,
                }
                : 0;

            Assert.AreEqual("configured in builder", stringValue);
            Assert.AreEqual(11, intValue);
        }

        [TestMethod]
        public void InvalidAttributeDefaultValue_IsRejectedDuringRegistration()
        {
            var services = new ServiceCollection();

            var exception = Assert.ThrowsException<InvalidOperationException>(() =>
                services.AddStateflows(builder => builder
                    .UseFullNamesFor(TypedElements.All)
                    .AddEntities(entities => entities
                        .AddEntity<IInvalidDefaultValueEntityTemplate>("invalid-default")
                    )
                )
            );

            StringAssert.Contains(exception.Message, "not compatible with property type");
            StringAssert.Contains(exception.Message, nameof(IInvalidDefaultValueEntityTemplate.IntValue));
        }

        [TestMethod]
        public void BuilderDefaultValue_OnComputedField_IsRejectedDuringRegistration()
        {
            var services = new ServiceCollection();

            var exception = Assert.ThrowsException<InvalidOperationException>((Action)(() =>
                services.AddStateflows(builder => builder
                    .UseFullNamesFor(TypedElements.All)
                    .AddEntities(entities => entities
                        .AddEntity<IBuilderComputedDefaultValueEntityTemplate>("invalid-builder-default", entity => entity
                            .AddField(t => t.IntValue, field =>
                            {
                                field.AddDefaultValue(7);
                                field.AddComputation(_ => 11);
                            })
                        )
                    )
                )
            ));

            StringAssert.Contains(exception.Message, "cannot declare a default value");
            StringAssert.Contains(exception.Message, nameof(IBuilderComputedDefaultValueEntityTemplate.IntValue));
        }
    }
}

