using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Entities.Attributes;

namespace Entity.IntegrationTests.Tests
{
    public interface IAttributedEntityTemplate
    {
        int IntValue { get; set; }
    }

    public interface ITypedEntityTemplate
    {
        int IntValue { get; set; }
        string StringValue { get; set; }
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
                    .AddObservation(async c => await Task.CompletedTask)
                )
                .AddMutation<TypedMutation>(_ => { });
        }
    }

    public record TypedMutation;
    public record TypedInitializationEvent;

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
                            .AddObservation(async _ => await Task.CompletedTask)
                        )
                        .AddMutation<PlainMutation>(_ => { })
                    )
                    .AddEntity<TypedEntity>("typed")
                    .AddFromAssembly(Assembly.GetExecutingAssembly())
                )
                ;
        }

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
            CollectionAssert.Contains(visitor.AddedEntities, "attributedEntity.3");
            Assert.IsTrue(visitor.AddedTypes.Contains(typeof(TypedEntity)));
            Assert.IsTrue(visitor.AddedTypes.Contains(typeof(AttributedEntity)));

            var allClasses = provider.AllBehaviorClasses.ToList();
            CollectionAssert.Contains(allClasses, new BehaviorClass(BehaviorType.Entity, "plain"));
            CollectionAssert.Contains(allClasses, new BehaviorClass(BehaviorType.Entity, "typed"));
            CollectionAssert.Contains(allClasses, new BehaviorClass(BehaviorType.Entity, "attributedEntity"));

            var entitiesProperty = register.GetType().GetField("Entities");
            var entities = (System.Collections.IDictionary)entitiesProperty!.GetValue(register)!;
            var plainRegistration = entities["plain.current"]!;
            var typedRegistration = entities["typed.current"]!;
            var plainModel = plainRegistration.GetType().GetProperty("Model")!.GetValue(plainRegistration)!;
            var typedModel = typedRegistration.GetType().GetProperty("Model")!.GetValue(typedRegistration)!;

            var plainFields = (System.Collections.IDictionary)plainModel.GetType().GetProperty("Fields")!.GetValue(plainModel)!;
            var plainMutations = (System.Collections.IDictionary)plainModel.GetType().GetProperty("Mutations")!.GetValue(plainModel)!;
            var typedFields = (System.Collections.IDictionary)typedModel.GetType().GetProperty("Fields")!.GetValue(typedModel)!;
            var typedMutations = (System.Collections.IDictionary)typedModel.GetType().GetProperty("Mutations")!.GetValue(typedModel)!;

            var plainFieldModel = plainFields["IntValue"]!;
            var typedFieldModel = typedFields["IntValue"]!;
            var plainComputation = plainFieldModel.GetType().GetProperty("Computation")!.GetValue(plainFieldModel);
            var typedComputation = typedFieldModel.GetType().GetProperty("Computation")!.GetValue(typedFieldModel);
            var plainObservations = (System.Collections.ICollection)plainFieldModel.GetType().GetProperty("Observations")!.GetValue(plainFieldModel)!;
            var typedObservations = (System.Collections.ICollection)typedFieldModel.GetType().GetProperty("Observations")!.GetValue(typedFieldModel)!;

            var plainInitializers = (System.Collections.IDictionary)plainModel.GetType().GetProperty("Initializers")!.GetValue(plainModel)!;
            var typedInitializers = (System.Collections.IDictionary)typedModel.GetType().GetProperty("Initializers")!.GetValue(typedModel)!;
            var plainDefaultInitializer = plainModel.GetType().GetProperty("DefaultInitializer")!.GetValue(plainModel);
            var typedDefaultInitializer = typedModel.GetType().GetProperty("DefaultInitializer")!.GetValue(typedModel);

            Assert.IsTrue(plainFields.Contains("IntValue"));
            Assert.IsNotNull(plainComputation);
            Assert.AreEqual(1, plainObservations.Count);
            Assert.IsTrue(plainMutations.Contains(typeof(PlainMutation)));
            Assert.IsNotNull(plainDefaultInitializer);
            Assert.IsTrue(plainInitializers.Contains(typeof(PlainInitializationEvent)));

            Assert.IsTrue(typedFields.Contains("IntValue"));
            Assert.IsNotNull(typedComputation);
            Assert.AreEqual(1, typedObservations.Count);
            Assert.IsTrue(typedMutations.Contains(typeof(TypedMutation)));
            Assert.IsNotNull(typedDefaultInitializer);
            Assert.IsTrue(typedInitializers.Contains(typeof(TypedInitializationEvent)));
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
    }
}

