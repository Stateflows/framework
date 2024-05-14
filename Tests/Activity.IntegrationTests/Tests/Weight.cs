using Stateflows.Common;
using Stateflows.Common.Data;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Weight : StateflowsTestClass
    {
        private bool Executed = false;

        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddActivities(b => b
                    .AddActivity("optional", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction(
                            "generate",
                            async c => { },
                            b => b.AddFlow<SomeEvent>("final", b => b.SetWeight(0))
                        )
                        .AddAction("final", async c =>
                        {
                            Executed = true;
                        })
                    )
                    .AddActivity("required", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction(
                            "generate",
                            async c => c.OutputRange(Enumerable.Range(0, 5)),
                            b => b.AddFlow<int>("final", b => b.SetWeight(10))
                        )
                        .AddAction("final", async c =>
                        {
                            Executed = true;
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task OptionalFlow()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("optional", "x"), out var a))
            {
                await a.InitializeAsync();
            }

            Assert.IsTrue(Executed);
        }

        [TestMethod]
        public async Task RequiredFlow()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("required", "x"), out var a))
            {
                await a.InitializeAsync();
            }

            Assert.IsFalse(Executed);
        }
    }
}