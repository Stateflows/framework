using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OneOf;
using Stateflows.Common.Context;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;
using Stateflows.Payloads;
using StateMachine.IntegrationTests.Utils;

namespace Entity.IntegrationTests.Tests
{
    public record PayloadEntityApproved(string By);
    public record PayloadEntityRejected(string Reason);
    public record PayloadEntityTyped(string Value);

    public interface IPayloadEntityTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string Value { get; set; }
    }

    public interface IAttributedPayloadEntityTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string Value { get; set; }

        [Initializer("order.placed", "order.imported")]
        void Open(Payload e) => Value = $"{e.Name}:{e.Get<string>("value")}";

        [Mutation("order.cancelled", "order.expired")]
        void Close(Payload e) => Value = $"{e.Name}:{e.Get<string>("value")}";

        [Mutation]
        void Resolve(OneOf<PayloadEntityApproved, PayloadEntityRejected> decision)
            => Value = decision.Match(a => $"approved:{a.By}", r => $"rejected:{r.Reason}");

        [Mutation]
        void SetTyped(PayloadEntityTyped e) => Value = $"typed:{e.Value}";
    }

    public interface IMappedInitializerEntityTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string Value { get; set; }

        [Initializer]
        void Open(OneOf<PayloadEntityApproved, PayloadEntityRejected> e)
            => Value = e.Match(a => $"approved:{a.By}", r => $"rejected:{r.Reason}");
    }

    public interface IPayloadWithoutNamesTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string Value { get; set; }

        [Mutation]
        void Close(Payload e) => Value = e.Name;
    }

    public interface INamesOnTypedTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string Value { get; set; }

        [Mutation("order.cancelled")]
        void Close(PayloadEntityTyped e) => Value = e.Value;
    }

    public interface IInvalidNamesTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string Value { get; set; }

        [Initializer("order.placed", "order.placed")]
        void Open(Payload e) => Value = e.Name;
    }

    public interface IDuplicatePayloadNamesTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string Value { get; set; }

        [Mutation("order.cancelled", "order.expired")]
        void Close(Payload e) => Value = e.Name;

        [Mutation("order.expired")]
        void Expire(Payload e) => Value = e.Name;
    }

    public interface IDuplicateMappedTypesTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string Value { get; set; }

        [Mutation]
        void Resolve(OneOf<PayloadEntityApproved, PayloadEntityRejected> decision) => Value = "resolved";

        [Mutation]
        void Approve(PayloadEntityApproved approved) => Value = "approved";
    }

    public class CloseOrderMutation : IMutation<IPayloadEntityTemplate, Payload>
    {
        public static void Mutate(IPayloadEntityTemplate entity, Payload mutationEvent)
            => entity.Value = $"class:{mutationEvent.Name}";
    }

    public class PayloadRecordingEntityVisitor : EntityVisitor
    {
        public List<string> Initializers { get; } = [];
        public List<string> Mutations { get; } = [];

        public override Task InitializerAddedAsync<TTemplate, TInitializationEvent>(string id, string entityName, int entityVersion, IReadOnlyList<string> eventNames)
        {
            Initializers.Add($"{entityName}:{typeof(TInitializationEvent).Name}:{string.Join(",", eventNames)}:{id[(id.LastIndexOf('/') + 1)..]}");
            return Task.CompletedTask;
        }

        public override Task MutationAddedAsync<TTemplate, TMutationEvent>(string id, string entityName, int entityVersion, IReadOnlyList<string> eventNames)
        {
            Mutations.Add($"{entityName}:{typeof(TMutationEvent).Name}:{string.Join(",", eventNames)}:{id[(id.LastIndexOf('/') + 1)..]}");
            return Task.CompletedTask;
        }
    }

    [TestClass]
    public class PayloadEntities : StateflowsTestClass
    {
        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        private static readonly Dictionary<string, EventHeader> NoImplicitInitializationHeaders
            = new() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } };

        private static void RegisterPayloads(IStateflowsBuilder builder)
            => builder
                .AddOneOf()
                .AddPayloads(p => p
                    .AddPayload("order.placed", e => e.AddField("value", FieldType.String))
                    .AddPayload("order.imported", e => e.AddField("value", FieldType.String))
                    .AddPayload("order.cancelled", e => e.AddField("value", FieldType.String))
                    .AddPayload("order.expired", e => e.AddField("value", FieldType.String))
                    .AddPayload("order.other", e => e.AddField("value", FieldType.String)));

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            RegisterPayloads(builder);
            builder
                .AddEntities(entities => entities
                    .AddEntity<IPayloadEntityTemplate>("builderPayload", b => b
                        .AddInitializer(["order.placed", "order.imported"], c => c.Entity.Value = $"init:{c.InitializationEvent.Name}")
                        .AddMutation(["order.cancelled", "order.expired"], c => c.Entity.Value = $"{c.MutationEvent.Name}:{c.MutationEvent.Get<string>("value")}")
                        .AddMutation<OneOf<PayloadEntityApproved, PayloadEntityRejected>>(c =>
                            c.Entity.Value = c.MutationEvent.Match(a => $"approved:{a.By}", r => $"rejected:{r.Reason}")))
                    .AddEntity<IPayloadEntityTemplate>("sugarPayload", b => b
                        .AddInitializer("order.placed", c => c.Entity.Value = "init")
                        .AddMutation("order.cancelled", c => c.Entity.Value = "cancelled"))
                    .AddEntity<IPayloadEntityTemplate>("classPayload", b => b
                        .AddDefaultInitializer(c => c.Entity.Value = "default")
                        .AddMutation<CloseOrderMutation>(["order.cancelled", "order.expired"]))
                    .AddEntity<IPayloadEntityTemplate>("overriddenPayload", b => b
                        .AddDefaultInitializer(c => c.Entity.Value = "default")
                        .AddMutation(["order.cancelled", "order.expired"], c => c.Entity.Value = "first")
                        .AddMutation("order.expired", c => c.Entity.Value = "second"))
                    .AddEntity<IPayloadEntityTemplate>("mappedInitializer", b => b
                        .AddInitializer<OneOf<PayloadEntityApproved, PayloadEntityRejected>>(c =>
                            c.Entity.Value = c.InitializationEvent.Match(a => $"approved:{a.By}", r => $"rejected:{r.Reason}")))
                    .AddEntity<IAttributedPayloadEntityTemplate>("attributedPayload")
                    .AddEntity<IMappedInitializerEntityTemplate>("attributedMappedInitializer"));
        }

        private IPayloadFactory Factory => ServiceProvider.GetRequiredService<IPayloadFactory>();

        private Payload Create(string name, string value = "v")
            => Factory.Create(name, b => b.Set("value", value));

        private IEntityBehavior Locate(string entityName, string? instance = null)
        {
            Assert.IsTrue(ServiceProvider.GetRequiredService<IEntityLocator>()
                .TryLocateEntity(new EntityClass(entityName).ToId(instance ?? Guid.NewGuid().ToString()), out var entity));
            return entity;
        }

        private async Task<string?> ValueAsync(string entityName, IEntityBehavior entity)
        {
            var storage = ServiceProvider.GetRequiredService<IStateflowsStorage>();
            var tenantAccessor = ServiceProvider.GetRequiredService<ITenantAccessor>();
            tenantAccessor.CurrentTenantId = ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            var context = await storage.HydrateAsync(entity.Id);
            return context.Values.TryGetValue("$field:Value", out var value) ? value as string : null;
        }

        [TestMethod]
        [DataRow("order.placed")]
        [DataRow("order.imported")]
        public async Task BuilderPayloadInitializer_RunsForEveryName(string name)
        {
            var entity = Locate("builderPayload");

            Assert.AreEqual(EventStatus.Initialized, (await entity.SendAsync(Create(name))).Status);
            Assert.AreEqual($"init:{name}", await ValueAsync("builderPayload", entity));
        }

        [TestMethod]
        [DataRow("order.cancelled")]
        [DataRow("order.expired")]
        public async Task BuilderPayloadMutation_HandlesEveryName(string name)
        {
            var entity = Locate("builderPayload");
            await entity.SendAsync(Create("order.placed"));

            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(Create(name, "x"))).Status);
            Assert.AreEqual($"{name}:x", await ValueAsync("builderPayload", entity));
        }

        [TestMethod]
        public async Task PayloadWithUndeclaredName_IsNotConsumed()
        {
            var entity = Locate("builderPayload");
            await entity.SendAsync(Create("order.placed"));

            Assert.AreEqual(EventStatus.NotConsumed, (await entity.SendAsync(Create("order.other"))).Status);
            Assert.AreEqual("init:order.placed", await ValueAsync("builderPayload", entity));
        }

        [TestMethod]
        public async Task PayloadInitializer_RunsOnlyForNotInitializedEntity()
        {
            var entity = Locate("builderPayload");
            await entity.SendAsync(Create("order.placed"));

            Assert.AreNotEqual(EventStatus.Initialized, (await entity.SendAsync(Create("order.imported"))).Status);
            Assert.AreEqual("init:order.placed", await ValueAsync("builderPayload", entity));
        }

        [TestMethod]
        public async Task PayloadMutation_HonoursNoImplicitInitializationHeader()
        {
            var entity = Locate("builderPayload");

            Assert.AreEqual(EventStatus.NotInitialized, (await entity.SendAsync(Create("order.cancelled"), NoImplicitInitializationHeaders)).Status);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task MappedTypeMutation_HandlesEveryMappedType(bool approved)
        {
            var entity = Locate("builderPayload");
            await entity.SendAsync(Create("order.placed"));

            var status = approved
                ? (await entity.SendAsync(new PayloadEntityApproved("ann"))).Status
                : (await entity.SendAsync(new PayloadEntityRejected("late"))).Status;

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual(approved ? "approved:ann" : "rejected:late", await ValueAsync("builderPayload", entity));
        }

        [TestMethod]
        [DataRow("mappedInitializer")]
        [DataRow("attributedMappedInitializer")]
        public async Task MappedTypeInitializer_RunsForEveryMappedType(string entityName)
        {
            var approved = Locate(entityName);
            var rejected = Locate(entityName);

            Assert.AreEqual(EventStatus.Initialized, (await approved.SendAsync(new PayloadEntityApproved("ann"))).Status);
            Assert.AreEqual(EventStatus.Initialized, (await rejected.SendAsync(new PayloadEntityRejected("late"))).Status);
            Assert.AreEqual("approved:ann", await ValueAsync(entityName, approved));
            Assert.AreEqual("rejected:late", await ValueAsync(entityName, rejected));
        }

        [TestMethod]
        public async Task SugarSingleNameForms_Work()
        {
            var entity = Locate("sugarPayload");

            Assert.AreEqual(EventStatus.Initialized, (await entity.SendAsync(Create("order.placed"))).Status);
            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(Create("order.cancelled"))).Status);
            Assert.AreEqual("cancelled", await ValueAsync("sugarPayload", entity));
        }

        [TestMethod]
        [DataRow("order.cancelled")]
        [DataRow("order.expired")]
        public async Task ClassMutationWithNames_HandlesEveryName(string name)
        {
            var entity = Locate("classPayload");

            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(Create(name))).Status);
            Assert.AreEqual($"class:{name}", await ValueAsync("classPayload", entity));
        }

        [TestMethod]
        [DataRow("order.cancelled", "first")]
        [DataRow("order.expired", "second")]
        public async Task LaterRegistrationReplacesEarlierPerName(string name, string expected)
        {
            var entity = Locate("overriddenPayload");

            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(Create(name))).Status);
            Assert.AreEqual(expected, await ValueAsync("overriddenPayload", entity));
        }

        [TestMethod]
        public async Task AttributedTemplate_PayloadInitializerAndMutation()
        {
            var entity = Locate("attributedPayload");

            Assert.AreEqual(EventStatus.Initialized, (await entity.SendAsync(Create("order.imported", "a"))).Status);
            Assert.AreEqual("order.imported:a", await ValueAsync("attributedPayload", entity));
            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(Create("order.expired", "b"))).Status);
            Assert.AreEqual("order.expired:b", await ValueAsync("attributedPayload", entity));
        }

        [TestMethod]
        public async Task AttributedTemplate_MappedAndTypedMutationsCoexistWithPayloads()
        {
            var entity = Locate("attributedPayload");
            await entity.SendAsync(Create("order.placed"));

            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(new PayloadEntityRejected("late"))).Status);
            Assert.AreEqual("rejected:late", await ValueAsync("attributedPayload", entity));
            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(new PayloadEntityTyped("t"))).Status);
            Assert.AreEqual("typed:t", await ValueAsync("attributedPayload", entity));
        }

        [TestMethod]
        public async Task ExpectedEvents_ReportPayloadAndMappedNames()
        {
            var entity = Locate("builderPayload");

            var notInitialized = await entity.GetStatusAsync(NoImplicitInitializationHeaders);
            CollectionAssert.AreEquivalent(new[] { "order.placed", "order.imported" }, notInitialized.Response.ExpectedEvents.ToArray());

            await entity.SendAsync(Create("order.placed"));
            var initialized = await entity.GetStatusAsync();
            CollectionAssert.AreEquivalent(
                new[] { "order.cancelled", "order.expired", Event<PayloadEntityApproved>.Name, Event<PayloadEntityRejected>.Name },
                initialized.Response.ExpectedEvents.ToArray());
        }

        [TestMethod]
        public async Task Visitor_ReportsDeclaredTypeAndNamesPerRegistration()
        {
            var visitor = new PayloadRecordingEntityVisitor();
            await ServiceProvider.GetRequiredService<IEntitiesRegister>().VisitEntitiesAsync(visitor);

            var mapped = $"{Event<PayloadEntityApproved>.Name},{Event<PayloadEntityRejected>.Name}";
            var oneOfKey = Event.GetName(typeof(OneOf<PayloadEntityApproved, PayloadEntityRejected>));
            CollectionAssert.Contains(visitor.Initializers, "builderPayload:Payload:order.placed,order.imported:initializer:order.imported|order.placed");
            CollectionAssert.Contains(visitor.Mutations, "builderPayload:Payload:order.cancelled,order.expired:mutation:order.cancelled|order.expired");
            CollectionAssert.Contains(visitor.Mutations, $"builderPayload:OneOf`2:{mapped}:mutation:{oneOfKey}");
            CollectionAssert.Contains(visitor.Initializers, $"mappedInitializer:OneOf`2:{mapped}:initializer:{oneOfKey}");
            CollectionAssert.Contains(visitor.Mutations, "overriddenPayload:Payload:order.cancelled:mutation:order.cancelled|order.expired");
            CollectionAssert.Contains(visitor.Mutations, "overriddenPayload:Payload:order.expired:mutation:order.expired");
        }

        private static Exception DefinitionError(Stateflows.Entities.Registration.Interfaces.EntitiesBuildAction build)
        {
            var exception = Assert.Throws<Exception>(() =>
            {
                var services = new ServiceCollection();
                services.AddStateflows(b =>
                {
                    RegisterPayloads(b);
                    b.AddEntities(build);
                });
                services.BuildServiceProvider().GetRequiredService<IEntitiesRegister>();
            });
            while (exception is TargetInvocationException { InnerException: not null } wrapped)
                exception = wrapped.InnerException;
            return exception;
        }

        [TestMethod]
        public void TypedPayloadInitializer_IsRejected()
        {
            var exception = DefinitionError(e => e.AddEntity<IPayloadEntityTemplate>("bad", b => b.AddInitializer<Payload>(_ => { })));
            Assert.IsInstanceOfType<InvalidOperationException>(exception);
            StringAssert.Contains(exception.Message, "AddInitializer(eventNames, action)");
        }

        [TestMethod]
        public void TypedPayloadMutation_IsRejected()
        {
            var exception = DefinitionError(e => e.AddEntity<IPayloadEntityTemplate>("bad", b => b.AddMutation<Payload>(_ => { })));
            Assert.IsInstanceOfType<InvalidOperationException>(exception);
            StringAssert.Contains(exception.Message, "AddMutation(eventNames, mutationAction)");
        }

        [TestMethod]
        public void TypedPayloadClassMutation_IsRejected()
        {
            var exception = DefinitionError(e => e.AddEntity<IPayloadEntityTemplate>("bad", b => b.AddMutation<Payload, CloseOrderMutation>()));
            Assert.IsInstanceOfType<InvalidOperationException>(exception);
        }

        [TestMethod]
        [DataRow(new string[0])]
        [DataRow(new[] { "" })]
        [DataRow(new[] { "order.placed", "order.placed" })]
        public void InvalidBuilderNames_AreRejected(string[] names)
        {
            var exception = DefinitionError(e => e.AddEntity<IPayloadEntityTemplate>("bad", b => b.AddMutation(names, _ => { })));
            Assert.IsInstanceOfType<ArgumentException>(exception);
        }

        [TestMethod]
        public void PayloadMethodWithoutNames_IsRejected()
        {
            var exception = DefinitionError(e => e.AddEntity<IPayloadWithoutNamesTemplate>("bad"));
            Assert.IsInstanceOfType<InvalidOperationException>(exception);
            StringAssert.Contains(exception.Message, "must list its event names");
        }

        [TestMethod]
        public void NamesOnTypedMethod_AreRejected()
        {
            var exception = DefinitionError(e => e.AddEntity<INamesOnTypedTemplate>("bad"));
            Assert.IsInstanceOfType<InvalidOperationException>(exception);
            StringAssert.Contains(exception.Message, "allowed only for a Payload parameter");
        }

        [TestMethod]
        public void InvalidAttributeNames_AreRejected()
        {
            var exception = DefinitionError(e => e.AddEntity<IInvalidNamesTemplate>("bad"));
            Assert.IsInstanceOfType<InvalidOperationException>(exception);
            StringAssert.Contains(exception.Message, "declares invalid event names");
        }

        [TestMethod]
        public void DuplicatePayloadNamesOnTemplate_AreRejected()
        {
            var exception = DefinitionError(e => e.AddEntity<IDuplicatePayloadNamesTemplate>("bad"));
            Assert.IsInstanceOfType<InvalidOperationException>(exception);
            StringAssert.Contains(exception.Message, "'order.expired' is declared multiple times");
        }

        [TestMethod]
        public void OverlappingMappedTypesOnTemplate_AreRejected()
        {
            var exception = DefinitionError(e => e.AddEntity<IDuplicateMappedTypesTemplate>("bad"));
            Assert.IsInstanceOfType<InvalidOperationException>(exception);
            StringAssert.Contains(exception.Message, $"'{Event<PayloadEntityApproved>.Name}' is declared multiple times");
        }
    }
}
