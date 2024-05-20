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
        public Foo EnumValue = Foo.First;
        public int IntValue = 0;

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
                            .AddOnEntry(async c => c.StateMachine.Values.Set(nameof(Foo), Foo.Last))
                            .AddDefaultTransition("state2")
                        )
                        .AddState("state2", b => b
                            .AddOnEntry(async c =>
                            {
                                if (c.StateMachine.Values.TryGet<Foo>(nameof(Foo), out EnumValue))
                                {
                                    EnumGet = true;
                                }

                                if (c.StateMachine.Values.TryGet<int>(nameof(Foo), out IntValue))
                                {
                                    IntGet = true;
                                }
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
                await stateMachine.InitializeAsync();
            }

            Assert.IsTrue(EnumGet);
            Assert.IsTrue(IntGet);
            Assert.AreEqual(Foo.Last, EnumValue);
            Assert.AreEqual((int)Foo.Last, IntValue);
        }
    }
}