using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Context;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;
using StateMachine.IntegrationTests.Utils;

namespace Entity.IntegrationTests.Tests
{
    public record MutationEvent(string StringValue);
    
    public record FlagSwitchingEvent(bool Flag);

    public record InheritedMutationSetBaseEvent(string Value);

    public record InheritedMutationSetSuffixEvent(string Value);
    
    public interface IMutationEntityTemplate
    {
        [Field]
        bool Flag { get; set; }

        [Field]
        bool HelperMethodExecuted { get; set; }

        [Field]
        bool ProjectionAccessBlocked { get; set; }

        [Field]
        bool MutationAccessBlocked { get; set; }
        
        [Field]
        string StringValue { get; set; }
        
        [Field]
        int IntValue => Flag
            ? StringValue.Length
            : 42;
        
        [Field]
        int ComputedValue => StringValue.Length;
        
        [Field]
        int ComputedIntValue => IntValue * 2;
        
        [Field]
        int DerivedIntValue => ComputedIntValue * 2;
        
        [Projection(PublishScope = PublishScope.Self | PublishScope.Owner)]
        MutationEntityTemplate ToTemplate
            => new MutationEntityTemplate()
            {
                Int = IntValue,
                String = StringValue,
                ComputedInt = ComputedIntValue,
                DerivedInt = DerivedIntValue
            };

        void ProbeAccessRestrictions()
        {
            HelperMethodExecuted = true;

            try
            {
                _ = ToTemplate;
            }
            catch (InvalidOperationException)
            {
                ProjectionAccessBlocked = true;
            }

            try
            {
                Mutate(new MutationEvent("blocked"));
            }
            catch (InvalidOperationException)
            {
                MutationAccessBlocked = true;
            }
        }
        
        [Mutation]
        void Mutate(MutationEvent mutationEvent)
        {
            StringValue = mutationEvent.StringValue;
        }

        [Mutation]
        void SwitchFlag(FlagSwitchingEvent mutationEvent)
        {
            Flag = mutationEvent.Flag;
        }
    }

    public interface IInheritedMutationEntityTemplateBase
    {
        [Field]
        string BaseValue { get; set; }

        [Mutation]
        void SetBase(InheritedMutationSetBaseEvent mutation)
        {
            BaseValue = mutation.Value;
        }
    }

    public interface IInheritedMutationEntityTemplate : IInheritedMutationEntityTemplateBase
    {
        [Field]
        string Suffix { get; set; }

        [Field]
        string CombinedValue => $"{BaseValue}:{Suffix}";

        [Mutation]
        void SetSuffix(InheritedMutationSetSuffixEvent mutation)
        {
            Suffix = mutation.Value;
        }

        [Projection(PublishScope = PublishScope.Self)]
        InheritedMutationProjection Snapshot => new()
        {
            BaseValue = BaseValue,
            Suffix = Suffix,
            CombinedValue = CombinedValue,
        };
    }

    public record MutationEntityTemplate
    {
        public string? String { get; set; }
        public int Int { get; set; }
        public int ComputedInt { get; set; }
        public int DerivedInt { get; set; }
    }

    public record InheritedMutationProjection
    {
        public string? BaseValue { get; set; }

        public string? Suffix { get; set; }

        public string? CombinedValue { get; set; }
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
                            context.Entity.ProbeAccessRestrictions();
                        })
                    )
                    .AddEntity<IInheritedMutationEntityTemplate>("inheritedMutation", b => b
                        .AddDefaultInitializer(context =>
                        {
                            context.Entity.BaseValue = "seed";
                            context.Entity.Suffix = "0";
                        })
                    )
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

        [TestMethod]
        public async Task RegistersEntitiesAndPublishesBehaviorClasses()
        {
            var status1 = EventStatus.Undelivered;
            var status2 = EventStatus.Undelivered;
            var status3 = EventStatus.Undelivered;
            var success1 = false;
            var success2 = false;
            MutationEntityTemplate? projection1 = null;
            MutationEntityTemplate? projection2 = null;
            var success4 = false;
            var helperMethodExecuted = false;
            var success5 = false;
            var projectionAccessBlocked = false;
            var success6 = false;
            var mutationAccessBlocked = false;
            string? fieldValue1 = null;
            long fieldValue2 = 0;
            MutationEntityTemplate? publishedProjection = null;
            var publishedProjectionCount = 0;
            var publishedProjectionCountBeforeChange = 0;
            var publishedProjectionCountAfterChange = 0;
            var status4 = EventStatus.Undelivered;

            if (TryLocateEntity("plainMutation", "x", out var entity))
            {
                await using var watcher = await entity.WatchAsync<MutationEntityTemplate>(projection =>
                {
                    lock (Locator)
                    {
                        publishedProjection = projection;
                        publishedProjectionCount++;
                    }
                });

                status1 = (await entity.SendAsync(new MutationEvent("Lorem ipsum"))).Status;
                var initializedContext = await HydrateContextAsync("plainMutation", "x");
                success4 = initializedContext.Values.TryGetValue("$field:HelperMethodExecuted", out var helperValue) && helperValue is bool;
                helperMethodExecuted = helperValue as bool? == true;
                success5 = initializedContext.Values.TryGetValue("$field:ProjectionAccessBlocked", out var projectionBlockedValue) && projectionBlockedValue is bool;
                projectionAccessBlocked = projectionBlockedValue as bool? == true;
                success6 = initializedContext.Values.TryGetValue("$field:MutationAccessBlocked", out var mutationBlockedValue) && mutationBlockedValue is bool;
                mutationAccessBlocked = mutationBlockedValue as bool? == true;
                status2 = status1;
                (success1, projection1) = await entity.TryGetProjectionAsync<MutationEntityTemplate>();
                publishedProjectionCountBeforeChange = publishedProjectionCount;
                status3 = (await entity.SendAsync(new FlagSwitchingEvent(true))).Status;
                (success2, projection2) = await entity.TryGetProjectionAsync<MutationEntityTemplate>();
                await Task.Delay(100);
                publishedProjectionCountAfterChange = publishedProjectionCount;
                status4 = (await entity.SendAsync(new FlagSwitchingEvent(true))).Status;
                var mutatedContext = await HydrateContextAsync("plainMutation", "x");
                fieldValue1 = mutatedContext.Values.TryGetValue("$field:StringValue", out var stringValue) ? stringValue as string : null;
                fieldValue2 = mutatedContext.Values.TryGetValue("$field:IntValue", out var intValue)
                    ? intValue switch
                    {
                        long longValue => longValue,
                        int integerValue => integerValue,
                        _ => 0,
                    }
                    : 0;

                await Task.Delay(100);
            }

            Assert.AreEqual(EventStatus.Consumed, status1);
            Assert.AreEqual(EventStatus.Consumed, status2);
            Assert.AreEqual(EventStatus.Consumed, status3);
            Assert.AreEqual(EventStatus.Consumed, status4);
            Assert.IsTrue(success1);
            Assert.IsTrue(success2);
            Assert.IsTrue(success4);
            Assert.IsTrue(success5);
            Assert.IsTrue(success6);
            Assert.IsTrue(helperMethodExecuted);
            Assert.IsTrue(projectionAccessBlocked);
            Assert.IsTrue(mutationAccessBlocked);
            Assert.IsNotNull(projection1);
            Assert.AreEqual("Lorem ipsum", projection1.String);
            Assert.AreEqual(42, projection1.Int);
            Assert.IsNotNull(projection2);
            Assert.AreEqual("Lorem ipsum", projection2.String);
            Assert.AreEqual(11, projection2.Int);
            Assert.AreEqual(22, projection2.ComputedInt);
            Assert.AreEqual(44, projection2.DerivedInt);
            Assert.IsNotNull(publishedProjection);
            Assert.AreEqual("Lorem ipsum", publishedProjection.String);
            Assert.AreEqual(11, publishedProjection.Int);
            Assert.AreEqual(22, publishedProjection.ComputedInt);
            Assert.AreEqual(44, publishedProjection.DerivedInt);
            Assert.AreEqual(publishedProjectionCountBeforeChange + 1, publishedProjectionCountAfterChange);
            Assert.AreEqual(publishedProjectionCountAfterChange, publishedProjectionCount);
            Assert.IsNotNull(fieldValue1);
            Assert.AreEqual("Lorem ipsum", fieldValue1);
            Assert.IsNotNull(fieldValue2);
            Assert.AreEqual(11, fieldValue2);
        }

        [TestMethod]
        public async Task InheritedTemplate_BaseAndDerivedMutations_AreConsumed()
        {
            var initStatus = EventStatus.Undelivered;
            var baseStatus = EventStatus.Undelivered;
            var suffixStatus = EventStatus.Undelivered;
            string? baseValue = null;
            string? suffixValue = null;

            if (TryLocateEntity("inheritedMutation", "x", out var entity))
            {
                initStatus = (await entity.SendAsync(new Initialize())).Status;
                baseStatus = (await entity.SendAsync(new InheritedMutationSetBaseEvent("left"))).Status;
                suffixStatus = (await entity.SendAsync(new InheritedMutationSetSuffixEvent("right"))).Status;

                var context = await HydrateContextAsync("inheritedMutation", "x");
                baseValue = context.Values.TryGetValue("$field:BaseValue", out var baseObject) ? baseObject as string : null;
                suffixValue = context.Values.TryGetValue("$field:Suffix", out var suffixObject) ? suffixObject as string : null;
            }

            Assert.AreEqual(EventStatus.Initialized, initStatus);
            Assert.AreEqual(EventStatus.Consumed, baseStatus);
            Assert.AreEqual(EventStatus.Consumed, suffixStatus);
            Assert.AreEqual("left", baseValue);
            Assert.AreEqual("right", suffixValue);
        }

        [TestMethod]
        public async Task InheritedTemplate_Projection_ReflectsMutationChanges()
        {
            var baseStatus = EventStatus.Undelivered;
            var suffixStatus = EventStatus.Undelivered;
            var success = false;
            InheritedMutationProjection? projection = null;

            if (TryLocateEntity("inheritedMutation", "projection", out var entity))
            {
                baseStatus = (await entity.SendAsync(new InheritedMutationSetBaseEvent("alpha"))).Status;
                suffixStatus = (await entity.SendAsync(new InheritedMutationSetSuffixEvent("beta"))).Status;
                (success, projection) = await entity.TryGetProjectionAsync<InheritedMutationProjection>();
            }

            Assert.AreEqual(EventStatus.Consumed, baseStatus);
            Assert.AreEqual(EventStatus.Consumed, suffixStatus);
            Assert.IsTrue(success);
            Assert.IsNotNull(projection);
            Assert.AreEqual("alpha", projection.BaseValue);
            Assert.AreEqual("beta", projection.Suffix);
            Assert.AreEqual("alpha:beta", projection.CombinedValue);
        }
    }
}

