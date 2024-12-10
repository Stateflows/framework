using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Scheduling : StateflowsTestClass
    {
        public bool Initialized = false;
        public bool Started = false;

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
                    .AddStateMachine("default", b => b
                        .AddInitialState("state1", b => b
                            .AddOnEntry(async c => Initialized = true)
                        )
                    )
                    .AddStateMachine("startup", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<Startup>("state2")
                        )
                        .AddState("state2", b => b
                            .AddOnEntry(async c => Started = true)
                        )
                    )
                )

                .AddDefaultInstance(new StateMachineClass("default"))
                .AddDefaultInstance(new StateMachineClass("startup"))
                ;
        }

        [TestMethod]
        public async Task DefaultInstanceInitialization()
        {
            await Task.Delay(100);
            Assert.IsTrue(Initialized);
        }

        [TestMethod]
        public async Task StartupTransition()
        {
            await Task.Delay(100);
            Assert.IsTrue(Started);
        }
    }
}