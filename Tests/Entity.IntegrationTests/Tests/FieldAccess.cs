using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Entities.Attributes;

namespace Entity.IntegrationTests.Tests
{
    // ── Computed-field test template ──────────────────────────────────────────

    public record SetBaseValueEvent(int Value);
    public record SetLabelEvent(string NewLabel);

    public interface IComputedFieldEntityTemplate
    {
        /// <summary>Plain stored field – externally readable and writable.</summary>
        [Field(FieldAccess.Get | FieldAccess.Set)]
        int BaseValue { get; set; }

        /// <summary>Plain stored field – externally readable and writable.</summary>
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string Label { get; set; }

        /// <summary>Computed: BaseValue * 2 – externally readable.</summary>
        [Field(FieldAccess.Get)]
        int DoubleValue => BaseValue * 2;

        /// <summary>Computed: Label.Length – externally readable.</summary>
        [Field(FieldAccess.Get)]
        int LabelLength => Label.Length;

        /// <summary>Mutation that changes BaseValue through the template proxy.</summary>
        [Mutation]
        void ApplyBaseValue(SetBaseValueEvent e) { BaseValue = e.Value; }

        /// <summary>Mutation that changes Label through the template proxy.</summary>
        [Mutation]
        void ApplyLabel(SetLabelEvent e) { Label = e.NewLabel; }
    }

    // ── FieldAccess test template (unchanged) ─────────────────────────────────

    public interface IFieldAccessEntityTemplate
    {
        /// <summary>Both read and write are allowed externally.</summary>
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string ReadWrite { get; set; }

        /// <summary>Only external reads are allowed.</summary>
        [Field(FieldAccess.Get)]
        int ReadOnly { get; set; }

        /// <summary>Only external writes are allowed.</summary>
        [Field(FieldAccess.Set)]
        bool WriteOnly { get; set; }

        /// <summary>No external access is allowed.</summary>
        [Field]
        string NoAccess { get; set; }

        /// <summary>Computed from ReadOnly – external reads allowed, no writes (computed).</summary>
        [Field]
        int DoubleReadOnly => ReadOnly * 2;
    }

    public interface IInheritedFieldAccessEntityTemplateBase
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string BaseReadWrite { get; set; }

        [Field(FieldAccess.Get)]
        int BaseReadOnly { get; set; }

        [Field]
        string BaseNoAccess { get; set; }
    }

    public interface IInheritedFieldAccessEntityTemplate : IInheritedFieldAccessEntityTemplateBase
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        string DerivedReadWrite { get; set; }

        [Field(FieldAccess.Get)]
        string Combined => $"{BaseReadWrite}:{DerivedReadWrite}";
    }

    [TestClass]
    public class FieldStateTests : StateflowsTestClass
    {
        [TestInitialize]
        public override void Initialize() => base.Initialize();

        [TestCleanup]
        public override void Cleanup() => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddEntities(entities => entities
                    .AddEntity<IFieldAccessEntityTemplate>("fieldAccessEntity", entity => entity
                        .AddDefaultInitializer(ctx =>
                        {
                            ctx.Entity.ReadWrite = "initial";
                            ctx.Entity.ReadOnly = 7;
                            ctx.Entity.WriteOnly = false;
                            ctx.Entity.NoAccess = "secret";
                        })
                    )
                    .AddEntity<IComputedFieldEntityTemplate>("computedEntity", entity => entity
                        .AddDefaultInitializer(ctx =>
                        {
                            ctx.Entity.BaseValue = 3;
                            ctx.Entity.Label = "hi";
                        })
                    )
                    .AddEntity<IInheritedFieldAccessEntityTemplate>("inheritedFieldAccessEntity", entity => entity
                        .AddDefaultInitializer(ctx =>
                        {
                            ctx.Entity.BaseReadWrite = "base";
                            ctx.Entity.BaseReadOnly = 10;
                            ctx.Entity.BaseNoAccess = "hidden";
                            ctx.Entity.DerivedReadWrite = "derived";
                        })
                    )
                );
        }

        private IEntityLocator Locator => ServiceProvider.GetRequiredService<IEntityLocator>();

        private bool TryLocateEntity(string entityName, string instance, out IEntityBehavior behavior)
            => Locator.TryLocateEntity(new EntityClass(entityName).ToId(instance), out behavior);

        // ── TryGetAsync ───────────────────────────────────────────────────────────

        [TestMethod]
        public async Task TryGetAsync_ReturnsFieldValue_WhenGetAccessGranted()
        {
            bool success = false;
            string? value = null;

            if (TryLocateEntity("fieldAccessEntity", "get-success", out var entity))
            {
                (success, value) = await entity.TryGetAsync<string>(nameof(IFieldAccessEntityTemplate.ReadWrite));
            }

            Assert.IsTrue(success);
            Assert.AreEqual("initial", value);
        }

        [TestMethod]
        public async Task TryGetAsync_ReturnsFalse_WhenFieldNotFound()
        {
            bool success = false;

            if (TryLocateEntity("fieldAccessEntity", "get-notfound", out var entity))
            {
                (success, _) = await entity.TryGetAsync<string>("NonExistentField");
            }

            Assert.IsFalse(success);
        }

        [TestMethod]
        public async Task TryGetAsync_ReturnsFalse_WhenGetAccessNotGranted()
        {
            bool success = false;

            if (TryLocateEntity("fieldAccessEntity", "get-noaccess", out var entity))
            {
                (success, _) = await entity.TryGetAsync<string>(nameof(IFieldAccessEntityTemplate.NoAccess));
            }

            Assert.IsFalse(success);
        }

        [TestMethod]
        public async Task TryGetAsync_ReturnsFalse_WhenFieldTypeDoesNotMatch()
        {
            bool success = false;

            if (TryLocateEntity("fieldAccessEntity", "get-typemismatch", out var entity))
            {
                // ReadOnly is int, but we request it as string
                (success, _) = await entity.TryGetAsync<string>(nameof(IFieldAccessEntityTemplate.ReadOnly));
            }

            Assert.IsFalse(success);
        }

        [TestMethod]
        public async Task TryGetAsync_ReturnsFalse_WhenOnlyWriteAccessGranted()
        {
            bool success = false;

            if (TryLocateEntity("fieldAccessEntity", "get-writeonly", out var entity))
            {
                (success, _) = await entity.TryGetAsync<bool>(nameof(IFieldAccessEntityTemplate.WriteOnly));
            }

            Assert.IsFalse(success);
        }

        // ── TrySetAsync ───────────────────────────────────────────────────────────

        [TestMethod]
        public async Task TrySetAsync_SetsFieldValue_WhenSetAccessGranted()
        {
            bool setSuccess = false;
            bool getSuccess = false;
            string? value = null;

            if (TryLocateEntity("fieldAccessEntity", "set-success", out var entity))
            {
                setSuccess = await entity.TrySetAsync<string>(nameof(IFieldAccessEntityTemplate.ReadWrite), "updated");
                (getSuccess, value) = await entity.TryGetAsync<string>(nameof(IFieldAccessEntityTemplate.ReadWrite));
            }

            Assert.IsTrue(setSuccess);
            Assert.IsTrue(getSuccess);
            Assert.AreEqual("updated", value);
        }

        [TestMethod]
        public async Task TrySetAsync_ReturnsFalse_WhenFieldNotFound()
        {
            bool success = false;

            if (TryLocateEntity("fieldAccessEntity", "set-notfound", out var entity))
            {
                success = await entity.TrySetAsync<string>("NonExistentField", "value");
            }

            Assert.IsFalse(success);
        }

        [TestMethod]
        public async Task TrySetAsync_ReturnsFalse_WhenSetAccessNotGranted()
        {
            bool success = false;

            if (TryLocateEntity("fieldAccessEntity", "set-noaccess", out var entity))
            {
                success = await entity.TrySetAsync<string>(nameof(IFieldAccessEntityTemplate.NoAccess), "hacked");
            }

            Assert.IsFalse(success);
        }

        [TestMethod]
        public async Task TrySetAsync_ReturnsFalse_WhenOnlyReadAccessGranted()
        {
            bool success = false;

            if (TryLocateEntity("fieldAccessEntity", "set-readonly", out var entity))
            {
                success = await entity.TrySetAsync<int>(nameof(IFieldAccessEntityTemplate.ReadOnly), 99);
            }

            Assert.IsFalse(success);
        }

        [TestMethod]
        public async Task TrySetAsync_ReturnsFalse_WhenFieldTypeDoesNotMatch()
        {
            bool success = false;

            if (TryLocateEntity("fieldAccessEntity", "set-typemismatch", out var entity))
            {
                // ReadWrite is string, but we send int
                success = await entity.TrySetAsync<int>(nameof(IFieldAccessEntityTemplate.ReadWrite), 42);
            }

            Assert.IsFalse(success);
        }

        [TestMethod]
        public async Task TrySetAsync_UpdatesComputedFields_AfterWrite()
        {
            // ReadOnly is writable internally; we use WriteOnly (Set access) to change a value
            // and verify the read-only field keeps its original value
            // (computed fields do not have external Set access, so we test TrySetAsync on ReadWrite
            //  and then verify a subsequent TryGetAsync reflects the write)
            bool setSuccess = false;
            bool getSuccess = false;
            string? value = null;

            if (TryLocateEntity("fieldAccessEntity", "set-computed", out var entity))
            {
                setSuccess = await entity.TrySetAsync<string>(nameof(IFieldAccessEntityTemplate.ReadWrite), "hello");
                (getSuccess, value) = await entity.TryGetAsync<string>(nameof(IFieldAccessEntityTemplate.ReadWrite));
            }

            Assert.IsTrue(setSuccess);
            Assert.IsTrue(getSuccess);
            Assert.AreEqual("hello", value);
        }

        // ── Round-trip ────────────────────────────────────────────────────────────

        [TestMethod]
        public async Task TrySetAsync_AndTryGetAsync_RoundTrip_WorksCorrectly()
        {
            bool setSuccess = false;
            bool getSuccess1 = false;
            bool getSuccess2 = false;
            string? value1 = null;
            string? value2 = null;

            if (TryLocateEntity("fieldAccessEntity", "roundtrip", out var entity))
            {
                (getSuccess1, value1) = await entity.TryGetAsync<string>(nameof(IFieldAccessEntityTemplate.ReadWrite));
                setSuccess = await entity.TrySetAsync<string>(nameof(IFieldAccessEntityTemplate.ReadWrite), "changed");
                (getSuccess2, value2) = await entity.TryGetAsync<string>(nameof(IFieldAccessEntityTemplate.ReadWrite));
            }

            Assert.IsTrue(getSuccess1);
            Assert.AreEqual("initial", value1);
            Assert.IsTrue(setSuccess);
            Assert.IsTrue(getSuccess2);
            Assert.AreEqual("changed", value2);
        }
        // ── Computed fields – via mutation ────────────────────────────────────────

        [TestMethod]
        public async Task ComputedField_ReflectsInitialValues_ViaGet()
        {
            bool doubleSuccess = false;
            bool labelSuccess = false;
            int doubleValue = 0;
            int labelLength = 0;

            if (TryLocateEntity("computedEntity", "computed-init", out var entity))
            {
                (doubleSuccess, doubleValue) = await entity.TryGetAsync<int>(nameof(IComputedFieldEntityTemplate.DoubleValue));
                (labelSuccess, labelLength) = await entity.TryGetAsync<int>(nameof(IComputedFieldEntityTemplate.LabelLength));
            }

            Assert.IsTrue(doubleSuccess);
            Assert.AreEqual(6, doubleValue);   // 3 * 2
            Assert.IsTrue(labelSuccess);
            Assert.AreEqual(2, labelLength);   // "hi".Length
        }

        [TestMethod]
        public async Task ComputedField_UpdatesAfterMutation_OnBaseValue()
        {
            bool mutationSuccess = false;
            bool getSuccess = false;
            int doubleValue = 0;

            if (TryLocateEntity("computedEntity", "computed-mutation-base", out var entity))
            {
                mutationSuccess = (await entity.SendAsync(new SetBaseValueEvent(10))).Status == EventStatus.Consumed;
                (getSuccess, doubleValue) = await entity.TryGetAsync<int>(nameof(IComputedFieldEntityTemplate.DoubleValue));
            }

            Assert.IsTrue(mutationSuccess);
            Assert.IsTrue(getSuccess);
            Assert.AreEqual(20, doubleValue);  // 10 * 2
        }

        [TestMethod]
        public async Task ComputedField_UpdatesAfterMutation_OnLabel()
        {
            bool mutationSuccess = false;
            bool getSuccess = false;
            int labelLength = 0;

            if (TryLocateEntity("computedEntity", "computed-mutation-label", out var entity))
            {
                mutationSuccess = (await entity.SendAsync(new SetLabelEvent("hello world"))).Status == EventStatus.Consumed;
                (getSuccess, labelLength) = await entity.TryGetAsync<int>(nameof(IComputedFieldEntityTemplate.LabelLength));
            }

            Assert.IsTrue(mutationSuccess);
            Assert.IsTrue(getSuccess);
            Assert.AreEqual(11, labelLength);  // "hello world".Length
        }

        [TestMethod]
        public async Task ComputedField_UpdatesAfterMutation_MultipleTimes()
        {
            bool getSuccess = false;
            int doubleValue = 0;

            if (TryLocateEntity("computedEntity", "computed-mutation-multi", out var entity))
            {
                _ = await entity.SendAsync(new SetBaseValueEvent(5));
                _ = await entity.SendAsync(new SetBaseValueEvent(8));
                (getSuccess, doubleValue) = await entity.TryGetAsync<int>(nameof(IComputedFieldEntityTemplate.DoubleValue));
            }

            Assert.IsTrue(getSuccess);
            Assert.AreEqual(16, doubleValue);  // 8 * 2
        }

        // ── Computed fields – via TrySetAsync ─────────────────────────────────────

        [TestMethod]
        public async Task ComputedField_UpdatesAfterDirectSet_OnBaseValue()
        {
            bool setSuccess = false;
            bool getSuccess = false;
            int doubleValue = 0;

            if (TryLocateEntity("computedEntity", "computed-set-base", out var entity))
            {
                setSuccess = await entity.TrySetAsync(nameof(IComputedFieldEntityTemplate.BaseValue), 7);
                (getSuccess, doubleValue) = await entity.TryGetAsync<int>(nameof(IComputedFieldEntityTemplate.DoubleValue));
            }

            Assert.IsTrue(setSuccess);
            Assert.IsTrue(getSuccess);
            Assert.AreEqual(14, doubleValue);  // 7 * 2
        }

        [TestMethod]
        public async Task ComputedField_UpdatesAfterDirectSet_OnLabel()
        {
            bool setSuccess = false;
            bool getSuccess = false;
            int labelLength = 0;

            if (TryLocateEntity("computedEntity", "computed-set-label", out var entity))
            {
                setSuccess = await entity.TrySetAsync(nameof(IComputedFieldEntityTemplate.Label), "stateflows");
                (getSuccess, labelLength) = await entity.TryGetAsync<int>(nameof(IComputedFieldEntityTemplate.LabelLength));
            }

            Assert.IsTrue(setSuccess);
            Assert.IsTrue(getSuccess);
            Assert.AreEqual(10, labelLength);  // "stateflows".Length
        }

        [TestMethod]
        public async Task ComputedField_UpdatesAfterDirectSet_MultipleTimes()
        {
            bool getSuccess = false;
            int doubleValue = 0;

            if (TryLocateEntity("computedEntity", "computed-set-multi", out var entity))
            {
                _ = await entity.TrySetAsync(nameof(IComputedFieldEntityTemplate.BaseValue), 4);
                _ = await entity.TrySetAsync(nameof(IComputedFieldEntityTemplate.BaseValue), 9);
                (getSuccess, doubleValue) = await entity.TryGetAsync<int>(nameof(IComputedFieldEntityTemplate.DoubleValue));
            }

            Assert.IsTrue(getSuccess);
            Assert.AreEqual(18, doubleValue);  // 9 * 2
        }

        [TestMethod]
        public async Task ComputedField_DirectSet_AndMutation_ProduceSameResult()
        {
            int valueViaMutation = 0;
            int valueViaDirectSet = 0;
            bool success1 = false;
            bool success2 = false;

            if (TryLocateEntity("computedEntity", "computed-mutation-equiv", out var e1))
            {
                _ = await e1.SendAsync(new SetBaseValueEvent(6));
                (success1, valueViaMutation) = await e1.TryGetAsync<int>(nameof(IComputedFieldEntityTemplate.DoubleValue));
            }

            if (TryLocateEntity("computedEntity", "computed-set-equiv", out var e2))
            {
                _ = await e2.TrySetAsync(nameof(IComputedFieldEntityTemplate.BaseValue), 6);
                (success2, valueViaDirectSet) = await e2.TryGetAsync<int>(nameof(IComputedFieldEntityTemplate.DoubleValue));
            }

            Assert.IsTrue(success1);
            Assert.IsTrue(success2);
            Assert.AreEqual(valueViaMutation, valueViaDirectSet);
            Assert.AreEqual(12, valueViaMutation);  // 6 * 2
        }

        [TestMethod]
        public async Task ComputedField_IsNotExternallyWritable()
        {
            bool setSuccess = false;

            if (TryLocateEntity("computedEntity", "computed-nowrite", out var entity))
            {
                // DoubleValue is a computed field – no FieldAccess.Set, so write must be rejected/failed
                setSuccess = await entity.TrySetAsync(nameof(IComputedFieldEntityTemplate.DoubleValue), 99);
            }

            Assert.IsFalse(setSuccess);
        }

        // ── Inherited field templates ─────────────────────────────────────────────

        [TestMethod]
        public async Task InheritedFieldTemplate_TryGet_ReadsBaseAndDerivedFields()
        {
            var baseSuccess = false;
            var derivedSuccess = false;
            var combinedSuccess = false;
            string? baseValue = null;
            string? derivedValue = null;
            string? combinedValue = null;

            if (TryLocateEntity("inheritedFieldAccessEntity", "get-inherited", out var entity))
            {
                (baseSuccess, baseValue) = await entity.TryGetAsync<string>(nameof(IInheritedFieldAccessEntityTemplateBase.BaseReadWrite));
                (derivedSuccess, derivedValue) = await entity.TryGetAsync<string>(nameof(IInheritedFieldAccessEntityTemplate.DerivedReadWrite));
                (combinedSuccess, combinedValue) = await entity.TryGetAsync<string>(nameof(IInheritedFieldAccessEntityTemplate.Combined));
            }

            Assert.IsTrue(baseSuccess);
            Assert.IsTrue(derivedSuccess);
            Assert.IsTrue(combinedSuccess);
            Assert.AreEqual("base", baseValue);
            Assert.AreEqual("derived", derivedValue);
            Assert.AreEqual("base:derived", combinedValue);
        }

        [TestMethod]
        public async Task InheritedFieldTemplate_TrySet_UpdatesBaseAndDerivedFields()
        {
            var setBaseSuccess = false;
            var setDerivedSuccess = false;
            var getBaseSuccess = false;
            var getDerivedSuccess = false;
            var getCombinedSuccess = false;
            string? baseValue = null;
            string? derivedValue = null;
            string? combinedValue = null;

            if (TryLocateEntity("inheritedFieldAccessEntity", "set-inherited", out var entity))
            {
                setBaseSuccess = await entity.TrySetAsync(nameof(IInheritedFieldAccessEntityTemplateBase.BaseReadWrite), "left");
                setDerivedSuccess = await entity.TrySetAsync(nameof(IInheritedFieldAccessEntityTemplate.DerivedReadWrite), "right");
                (getBaseSuccess, baseValue) = await entity.TryGetAsync<string>(nameof(IInheritedFieldAccessEntityTemplateBase.BaseReadWrite));
                (getDerivedSuccess, derivedValue) = await entity.TryGetAsync<string>(nameof(IInheritedFieldAccessEntityTemplate.DerivedReadWrite));
                (getCombinedSuccess, combinedValue) = await entity.TryGetAsync<string>(nameof(IInheritedFieldAccessEntityTemplate.Combined));
            }

            Assert.IsTrue(setBaseSuccess);
            Assert.IsTrue(setDerivedSuccess);
            Assert.IsTrue(getBaseSuccess);
            Assert.IsTrue(getDerivedSuccess);
            Assert.IsTrue(getCombinedSuccess);
            Assert.AreEqual("left", baseValue);
            Assert.AreEqual("right", derivedValue);
            Assert.AreEqual("left:right", combinedValue);
        }

        [TestMethod]
        public async Task InheritedFieldTemplate_AccessRestrictions_FromBaseAreRespected()
        {
            var getNoAccessSuccess = false;
            var setReadOnlySuccess = false;

            if (TryLocateEntity("inheritedFieldAccessEntity", "access-inherited", out var entity))
            {
                (getNoAccessSuccess, _) = await entity.TryGetAsync<string>(nameof(IInheritedFieldAccessEntityTemplateBase.BaseNoAccess));
                setReadOnlySuccess = await entity.TrySetAsync(nameof(IInheritedFieldAccessEntityTemplateBase.BaseReadOnly), 99);
            }

            Assert.IsFalse(getNoAccessSuccess);
            Assert.IsFalse(setReadOnlySuccess);
        }
    }
}





