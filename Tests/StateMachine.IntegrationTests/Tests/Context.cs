using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    public enum Foo
    {
        First = 0,
        Last = 1
    }

    [TestClass]
    public class Context : StateflowsTestClass
    {
        public bool EnumGet = false;
        public bool IntGet = false;
        public bool GuidGet = false;
        public Foo EnumValue = Foo.First;
        public int IntValue = 0;
        public Guid GuidValue = Guid.Empty;

        public Guid TestedGuid = Guid.NewGuid();

        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddStateMachines(b => b
                    .AddStateMachine("context", b => b
                        .AddInitialState("state1", b => b
                            .AddOnEntry(async c =>
                            {
                                await c.Behavior.Values.SetAsync(nameof(Foo), Foo.Last);
                                await c.Behavior.Values.SetAsync(nameof(Guid), TestedGuid);
                            })
                            .AddDefaultTransition("state2")
                        )
                        .AddState("state2", b => b
                            .AddOnEntry(async c =>
                            {
                                (EnumGet, EnumValue) = await c.Behavior.Values.TryGetAsync<Foo>(nameof(Foo));
                                (IntGet, IntValue) = await c.Behavior.Values.TryGetAsync<int>(nameof(Foo));
                                (GuidGet, GuidValue) = await c.Behavior.Values.TryGetAsync<Guid>(nameof(Guid));
                            })
                        )
                    )
                )
                ;
        }

        [TestMethod]
        public async Task EnumSerialization()
        {
            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("context", "x"), out var stateMachine))
            {
                await stateMachine.SendAsync(new Initialize());
            }

            Assert.IsTrue(EnumGet);
            Assert.IsTrue(IntGet);
            Assert.AreEqual(Foo.Last, EnumValue);
            Assert.AreEqual((int)Foo.Last, IntValue);

            Assert.IsTrue(GuidGet);
            Assert.AreEqual(TestedGuid, GuidValue);
        }
    }
}