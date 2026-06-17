using Microsoft.Extensions.DependencyInjection;
using Stateflows.Entities.Attributes;
using StateMachine.IntegrationTests.Utils;

namespace Entity.IntegrationTests.Tests
{
    public record MutationEvent(string StringValue);
    
    public record FlagSwitchingEvent(bool Flag);
    
    public interface IMutationEntityTemplate
    {
        bool Flag { get; set; }
        string StringValue { get; set; }
        int IntValue { get; set; }
        
        // [Computation(nameof(IntValue))]
        int ComputedValue => StringValue.Length;
        
        int ComputedIntValue { get; }
        int DerivedIntValue { get; }
        
        [Projection]
        public MutationEntityTemplate ToTemplate()
            => new MutationEntityTemplate()
            {
                Int = IntValue,
                String = StringValue,
                ComputedInt = ComputedIntValue,
                DerivedInt = DerivedIntValue
            };
        
        [Mutation]
        public void Mutate(MutationEvent mutationEvent)
        {
            StringValue = mutationEvent.StringValue;
        }
    }

    public record MutationEntityTemplate
    {
        public string String { get; set; }
        public int Int { get; set; }
        public int ComputedInt { get; set; }
        public int DerivedInt { get; set; }
    }

    [TestClass]
    public class Mutation : StateflowsTestClass
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
                    .AddEntity<IMutationEntityTemplate>("plainMutation", b => b
                        .AddDefaultInitializer(context =>
                        {
                            context.Entity.StringValue = string.Empty;
                        })
                        .AddField(t => t.Flag)
                        .AddField(t => t.StringValue)
                        .AddField(t => t.IntValue, b => b
                            .AddComputation(x => x.Flag
                                ? x.StringValue.Length
                                : 42
                            )
                        )
                        .AddField(t => t.ComputedIntValue, b => b
                            .AddComputation(x => x.IntValue * 2)
                        )
                        .AddField(t => t.DerivedIntValue, b => b
                            .AddComputation(x => x.ComputedIntValue * 2)
                        )
                        .AddMutation<MutationEvent>(x =>
                        {
                            x.Entity.StringValue = x.MutationEvent.StringValue;
                        })
                        .AddMutation<FlagSwitchingEvent>(x =>
                        {
                            x.Entity.Flag = x.MutationEvent.Flag;
                        })
                        .AddProjection<MutationEntityTemplate>(x => new MutationEntityTemplate()
                            {
                                Int = x.IntValue,
                                String = x.StringValue,
                                ComputedInt = x.ComputedValue,
                                DerivedInt = x.DerivedIntValue
                            }
                        )
                    )
                )
                ;
        }
        
        private IEntityLocator Locator => ServiceProvider.GetRequiredService<IEntityLocator>();

        private bool TryLocateEntity(string entityName, string instance, out IEntityBehavior behavior)
            => Locator.TryLocateEntity(new EntityClass(entityName).ToId(instance), out behavior);

        [TestMethod]
        public async Task RegistersEntitiesAndPublishesBehaviorClasses()
        {
            var status1 = EventStatus.Undelivered;
            var status2 = EventStatus.Undelivered;
            var status3 = EventStatus.Undelivered;
            var success1 = false;
            MutationEntityTemplate projection1 = null;
            MutationEntityTemplate projection2 = null;
            var success2 = false;
            string fieldValue1 = null;
            var success3 = false;
            long fieldValue2 = 0;

            if (TryLocateEntity("plainMutation", "x", out var entity))
            {
                status1 = (await entity.SendAsync(new Initialize())).Status;
                status2 = (await entity.SendAsync(new MutationEvent("Lorem ipsum"))).Status;
                (success1, projection1) = await entity.TryGetProjection<MutationEntityTemplate>();
                status3 = (await entity.SendAsync(new FlagSwitchingEvent(true))).Status;
                (success1, projection2) = await entity.TryGetProjection<MutationEntityTemplate>();
                (success2, fieldValue1) = await entity.TryGetFieldValue<string>("StringValue");
                (success3, fieldValue2) = await entity.TryGetFieldValue<long>("IntValue");
            }

            Assert.AreEqual(EventStatus.Initialized, status1);
            Assert.AreEqual(EventStatus.Consumed, status2);
            Assert.IsTrue(success1);
            Assert.IsNotNull(projection1);
            Assert.AreEqual("Lorem ipsum", projection1.String);
            Assert.AreEqual(42, projection1.Int);
            Assert.IsNotNull(projection2);
            Assert.AreEqual("Lorem ipsum", projection2.String);
            Assert.AreEqual(11, projection2.Int);
            Assert.IsTrue(success2);
            Assert.IsNotNull(fieldValue1);
            Assert.AreEqual("Lorem ipsum", fieldValue1);
            Assert.IsTrue(success3);
            Assert.IsNotNull(fieldValue2);
            Assert.AreEqual(11, fieldValue2);
        }
    }
}

