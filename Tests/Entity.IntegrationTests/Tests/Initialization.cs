using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Common.Context;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;

namespace Entity.IntegrationTests.Tests
{
    public record InheritedRenameBaseEvent(string Value);

    public record InheritedRenameSuffixEvent(string Value);

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

    public interface IInheritedInitEntityTemplateBase
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string BaseValue { get; set; }

        [Mutation]
        void RenameBase(InheritedRenameBaseEvent mutation)
        {
            BaseValue = mutation.Value;
        }
    }

    public interface IInheritedInitEntityTemplate : IInheritedInitEntityTemplateBase
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string Suffix { get; set; }

        [Field(FieldAccess.Get)]
        string CombinedValue => $"{BaseValue}:{Suffix}";

        [Mutation]
        void RenameSuffix(InheritedRenameSuffixEvent mutation)
        {
            Suffix = mutation.Value;
        }

        [Projection]
        InheritedInitEntityProjection Snapshot => new()
        {
            BaseValue = BaseValue,
            Suffix = Suffix,
            CombinedValue = CombinedValue,
        };
    }

    public interface IInheritedAttributedDefaultInitEntityTemplateBase
    {
        [Field]
        string BaseValue { get; set; }

        [DefaultInitializer]
        void InitializeBase()
        {
            BaseValue = "base";
        }
    }

    public interface IInheritedAttributedDefaultInitEntityTemplate : IInheritedAttributedDefaultInitEntityTemplateBase
    {
        [Field]
        string Suffix { get; set; }

        [DefaultInitializer]
        void InitializeSuffix()
        {
            Suffix = "suffix";
        }
    }

    public record AttributedInitEvent(string Name, int Score);

    public record AttributedInitProjection
    {
        public string? Name { get; set; }
        public int Score { get; set; }
        public int DoubleScore { get; set; }
    }

    public interface IAttributedInitializerTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string Name { get; set; }

        [Field(FieldAccess.Get | FieldAccess.Set)]
        int Score { get; set; }

        [Field(FieldAccess.Get)]
        int DoubleScore => Score * 2;

        [Projection]
        AttributedInitProjection Snapshot => new() { Name = Name, Score = Score, DoubleScore = DoubleScore };

        [Initializer]
        void Initialize(AttributedInitEvent e)
        {
            Name = e.Name;
            Score = e.Score;
        }
    }

    public record AttributedTwoEventsInitEventA(string Tag);
    public record AttributedTwoEventsInitEventB(int Count);

    public record CombinedAttributedInitEvent(string Source);

    public interface ICombinedAttributedInitTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string InitSource { get; set; }

        [DefaultInitializer]
        void SetDefault() { InitSource = "default"; }

        [Initializer]
        void InitFromEvent(CombinedAttributedInitEvent e) { InitSource = e.Source; }
    }

    public record InheritedAttributedInitEvent(string BasePart, string DerivedPart);

    public interface IInheritedAttributedInitBaseTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string BasePart { get; set; }
    }

    public interface IInheritedAttributedInitTemplate : IInheritedAttributedInitBaseTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string DerivedPart { get; set; }

        [Initializer]
        void Initialize(InheritedAttributedInitEvent e)
        {
            BasePart = e.BasePart;
            DerivedPart = e.DerivedPart;
        }
    }

    public interface IAttributedTwoInitializersTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string Tag { get; set; }

        [Field(FieldAccess.Get | FieldAccess.Set)]
        int Count { get; set; }

        [Initializer]
        void InitializeA(AttributedTwoEventsInitEventA e) { Tag = e.Tag; }

        [Initializer]
        void InitializeB(AttributedTwoEventsInitEventB e) { Count = e.Count; }
    }

    public record InitEntityProjection
    {
        public bool InitializerCalled { get; set; }

        public string? InitValue { get; set; }
    }

    public record InheritedInitEntityProjection
    {
        public string? BaseValue { get; set; }

        public string? Suffix { get; set; }

        public string? CombinedValue { get; set; }
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

                .AddEntity<IInheritedInitEntityTemplate>("withInheritedTemplate", entity => entity
                    .AddDefaultInitializer(ctx =>
                    {
                        ctx.Entity.BaseValue = "seed";
                        ctx.Entity.Suffix = "init";
                    })
                )
                .AddEntity<IInheritedAttributedDefaultInitEntityTemplate>("withInheritedAttributedDefault")
                .AddEntity<IAttributedInitializerTemplate>("withAttributedInit")
                .AddEntity<IAttributedTwoInitializersTemplate>("withTwoAttributedInits")
                .AddEntity<ICombinedAttributedInitTemplate>("withCombinedAttributedInit")
                .AddEntity<IInheritedAttributedInitTemplate>("withInheritedAttributedInit")
            );
        }

        private IEntityLocator EntityLocator => ServiceProvider.GetRequiredService<IEntityLocator>();

        private async Task<StateflowsContext> HydrateContextAsync(string entityName, string instance)
        {
            var storage = ServiceProvider.GetRequiredService<IStateflowsStorage>();
            var tenantAccessor = ServiceProvider.GetRequiredService<ITenantAccessor>();
            var tenantProvider = ServiceProvider.GetRequiredService<IStateflowsTenantProvider>();
            tenantAccessor.CurrentTenantId = tenantProvider.GetCurrentTenantId();
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

        // ── Inherited template interfaces ───────────────────────────────────────

        [TestMethod]
        public async Task InheritedTemplate_InitializeEvent_InitializesBaseAndDerivedFields()
        {
            var status = EventStatus.Undelivered;
            string? baseValue = null;
            string? suffix = null;

            if (TryLocateEntity("withInheritedTemplate", "init", out var entity))
            {
                status = (await entity.SendAsync(new Initialize())).Status;
                var context = await HydrateContextAsync("withInheritedTemplate", "init");
                baseValue = context.Values.TryGetValue("$field:BaseValue", out var baseObject) ? baseObject as string : null;
                suffix = context.Values.TryGetValue("$field:Suffix", out var suffixObject) ? suffixObject as string : null;
            }

            Assert.AreEqual(EventStatus.Initialized, status);
            Assert.AreEqual("seed", baseValue);
            Assert.AreEqual("init", suffix);
        }

        [TestMethod]
        public async Task InheritedTemplate_MutationsFromBaseAndDerived_AreConsumedAndAffectProjection()
        {
            var initStatus = EventStatus.Undelivered;
            var baseMutationStatus = EventStatus.Undelivered;
            var suffixMutationStatus = EventStatus.Undelivered;
            var success = false;
            InheritedInitEntityProjection? projection = null;

            if (TryLocateEntity("withInheritedTemplate", "mutation", out var entity))
            {
                initStatus = (await entity.SendAsync(new Initialize())).Status;
                baseMutationStatus = (await entity.SendAsync(new InheritedRenameBaseEvent("left"))).Status;
                suffixMutationStatus = (await entity.SendAsync(new InheritedRenameSuffixEvent("right"))).Status;
                (success, projection) = await entity.TryGetProjectionAsync<InheritedInitEntityProjection>();
            }

            Assert.AreEqual(EventStatus.Initialized, initStatus);
            Assert.AreEqual(EventStatus.Consumed, baseMutationStatus);
            Assert.AreEqual(EventStatus.Consumed, suffixMutationStatus);
            Assert.IsTrue(success);
            Assert.IsNotNull(projection);
            Assert.AreEqual("left", projection.BaseValue);
            Assert.AreEqual("right", projection.Suffix);
            Assert.AreEqual("left:right", projection.CombinedValue);
        }

        [TestMethod]
        public async Task InheritedTemplate_FirstProjectionRequest_InitializesAndReturnsSnapshot()
        {
            var success = false;
            InheritedInitEntityProjection? projection = null;

            if (TryLocateEntity("withInheritedTemplate", "projection", out var entity))
            {
                (success, projection) = await entity.TryGetProjectionAsync<InheritedInitEntityProjection>();
            }

            Assert.IsTrue(success);
            Assert.IsNotNull(projection);
            Assert.AreEqual("seed", projection.BaseValue);
            Assert.AreEqual("init", projection.Suffix);
            Assert.AreEqual("seed:init", projection.CombinedValue);
        }

        [TestMethod]
        public async Task InheritedTemplate_TrySetTryGet_WorksForBaseAndDerivedFields()
        {
            var initStatus = EventStatus.Undelivered;
            var setBaseSuccess = false;
            var setSuffixSuccess = false;
            var getBaseSuccess = false;
            var getSuffixSuccess = false;
            var getCombinedSuccess = false;
            string? baseValue = null;
            string? suffix = null;
            string? combinedValue = null;

            if (TryLocateEntity("withInheritedTemplate", "field", out var entity))
            {
                initStatus = (await entity.SendAsync(new Initialize())).Status;
                setBaseSuccess = await entity.TrySetAsync(nameof(IInheritedInitEntityTemplateBase.BaseValue), "alpha");
                setSuffixSuccess = await entity.TrySetAsync(nameof(IInheritedInitEntityTemplate.Suffix), "beta");
                (getBaseSuccess, baseValue) = await entity.TryGetAsync<string>(nameof(IInheritedInitEntityTemplateBase.BaseValue));
                (getSuffixSuccess, suffix) = await entity.TryGetAsync<string>(nameof(IInheritedInitEntityTemplate.Suffix));
                (getCombinedSuccess, combinedValue) = await entity.TryGetAsync<string>(nameof(IInheritedInitEntityTemplate.CombinedValue));
            }

            Assert.AreEqual(EventStatus.Initialized, initStatus);
            Assert.IsTrue(setBaseSuccess);
            Assert.IsTrue(setSuffixSuccess);
            Assert.IsTrue(getBaseSuccess);
            Assert.IsTrue(getSuffixSuccess);
            Assert.IsTrue(getCombinedSuccess);
            Assert.AreEqual("alpha", baseValue);
            Assert.AreEqual("beta", suffix);
            Assert.AreEqual("alpha:beta", combinedValue);
        }

        [TestMethod]
        public async Task InheritedAttributedDefaultInitializer_InitializeEvent_InitializesBaseAndDerivedFields()
        {
            var status = EventStatus.Undelivered;
            string? baseValue = null;
            string? suffix = null;

            if (TryLocateEntity("withInheritedAttributedDefault", "init", out var entity))
            {
                status = (await entity.SendAsync(new Initialize())).Status;
                var context = await HydrateContextAsync("withInheritedAttributedDefault", "init");
                baseValue = context.Values.TryGetValue("$field:BaseValue", out var baseObject) ? baseObject as string : null;
                suffix = context.Values.TryGetValue("$field:Suffix", out var suffixObject) ? suffixObject as string : null;
            }

            Assert.AreEqual(EventStatus.Initialized, status);
            Assert.AreEqual("base", baseValue);
            Assert.AreEqual("suffix", suffix);
        }

        // ── Template-defined [Initializer] attribute ─────────────────────────

        [TestMethod]
        public async Task AttributedInitializer_MatchingEvent_InitializesWithEventData()
        {
            var status = EventStatus.Undelivered;
            string? name = null;
            int score = 0;

            if (TryLocateEntity("withAttributedInit", "attr-init", out var entity))
            {
                status = (await entity.SendAsync(new AttributedInitEvent("hero", 42))).Status;
                var ctx = await HydrateContextAsync("withAttributedInit", "attr-init");
                name = ctx.Values.TryGetValue("$field:Name", out var nv) ? nv as string : null;
                score = ctx.Values.TryGetValue("$field:Score", out var sv) && sv is long l ? (int)l : 0;
            }

            Assert.AreEqual(EventStatus.Initialized, status);
            Assert.AreEqual("hero", name);
            Assert.AreEqual(42, score);
        }

        [TestMethod]
        public async Task AttributedInitializer_MatchingEvent_CascadesComputedFieldsAndProjection()
        {
            var projSuccess = false;
            AttributedInitProjection? snapshot = null;

            if (TryLocateEntity("withAttributedInit", "attr-cascade", out var entity))
            {
                _ = await entity.SendAsync(new AttributedInitEvent("cascade", 5));
                (projSuccess, snapshot) = await entity.TryGetProjectionAsync<AttributedInitProjection>();
            }

            Assert.IsTrue(projSuccess);
            Assert.IsNotNull(snapshot);
            Assert.AreEqual("cascade", snapshot.Name);
            Assert.AreEqual(5, snapshot.Score);
            Assert.AreEqual(10, snapshot.DoubleScore); // 5 * 2
        }

        [TestMethod]
        public async Task AttributedInitializer_InitializeEvent_AlsoInitializes()
        {
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("withAttributedInit", "attr-init-default", out var entity))
                status = (await entity.SendAsync(new Initialize())).Status;

            Assert.AreEqual(EventStatus.Initialized, status);
        }

        [TestMethod]
        public async Task AttributedInitializer_UnknownEvent_DoesNotInitialize()
        {
            // An event type with no matching initializer must NOT initialize the entity.
            var status = EventStatus.Undelivered;

            if (TryLocateEntity("withAttributedInit", "attr-unknown", out var entity))
                status = (await entity.SendAsync(new object())).Status;

            Assert.AreEqual(EventStatus.NotConsumed, status);
        }

        [TestMethod]
        public async Task AttributedInitializer_SecondMatchingEvent_IsNotConsumedAfterInit()
        {
            var status1 = EventStatus.Undelivered;
            var status2 = EventStatus.Undelivered;

            if (TryLocateEntity("withAttributedInit", "attr-second", out var entity))
            {
                status1 = (await entity.SendAsync(new AttributedInitEvent("first", 1))).Status;
                status2 = (await entity.SendAsync(new AttributedInitEvent("second", 2))).Status;
            }

            Assert.AreEqual(EventStatus.Initialized, status1);
            Assert.AreEqual(EventStatus.NotConsumed, status2);
        }

        [TestMethod]
        public async Task AttributedInitializer_TwoEventTypes_EachInitializesCorrectFields()
        {
            var statusA = EventStatus.Undelivered;
            var statusB = EventStatus.Undelivered;
            string? tagA = null;
            int countB = 0;

            if (TryLocateEntity("withTwoAttributedInits", "two-a", out var entityA))
            {
                statusA = (await entityA.SendAsync(new AttributedTwoEventsInitEventA("tagValue"))).Status;
                var ctxA = await HydrateContextAsync("withTwoAttributedInits", "two-a");
                tagA = ctxA.Values.TryGetValue("$field:Tag", out var tv) ? tv as string : null;
            }

            if (TryLocateEntity("withTwoAttributedInits", "two-b", out var entityB))
            {
                statusB = (await entityB.SendAsync(new AttributedTwoEventsInitEventB(99))).Status;
                var ctxB = await HydrateContextAsync("withTwoAttributedInits", "two-b");
                countB = ctxB.Values.TryGetValue("$field:Count", out var cv) && cv is long l ? (int)l : 0;
            }

            Assert.AreEqual(EventStatus.Initialized, statusA);
            Assert.AreEqual(EventStatus.Initialized, statusB);
            Assert.AreEqual("tagValue", tagA);
            Assert.AreEqual(99, countB);
        }

        [TestMethod]
        public async Task CombinedAttributedInitializers_InitializeEvent_UsesDefaultInitializer()
        {
            var status = EventStatus.Undelivered;
            string? initSource = null;

            if (TryLocateEntity("withCombinedAttributedInit", "combined-default", out var entity))
            {
                status = (await entity.SendAsync(new Initialize())).Status;
                (_, initSource) = await entity.TryGetAsync<string>(nameof(ICombinedAttributedInitTemplate.InitSource));
            }

            Assert.AreEqual(EventStatus.Initialized, status);
            Assert.AreEqual("default", initSource);
        }

        [TestMethod]
        public async Task CombinedAttributedInitializers_MatchingEvent_UsesAttributedInitializer()
        {
            var status = EventStatus.Undelivered;
            string? initSource = null;

            if (TryLocateEntity("withCombinedAttributedInit", "combined-event", out var entity))
            {
                status = (await entity.SendAsync(new CombinedAttributedInitEvent("from-event"))).Status;
                (_, initSource) = await entity.TryGetAsync<string>(nameof(ICombinedAttributedInitTemplate.InitSource));
            }

            Assert.AreEqual(EventStatus.Initialized, status);
            Assert.AreEqual("from-event", initSource);
        }

        [TestMethod]
        public async Task InheritedAttributedInitializer_MatchingEvent_InitializesBothFields()
        {
            var status = EventStatus.Undelivered;
            string? basePart = null;
            string? derivedPart = null;

            if (TryLocateEntity("withInheritedAttributedInit", "inherited-attr", out var entity))
            {
                status = (await entity.SendAsync(new InheritedAttributedInitEvent("base-val", "derived-val"))).Status;
                (_, basePart) = await entity.TryGetAsync<string>(nameof(IInheritedAttributedInitBaseTemplate.BasePart));
                (_, derivedPart) = await entity.TryGetAsync<string>(nameof(IInheritedAttributedInitTemplate.DerivedPart));
            }

            Assert.AreEqual(EventStatus.Initialized, status);
            Assert.AreEqual("base-val", basePart);
            Assert.AreEqual("derived-val", derivedPart);
        }
    }
}

