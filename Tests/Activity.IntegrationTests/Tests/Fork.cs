using Stateflows.Activities;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Fork : StateflowsTestClass
    {
        private bool Execution1 = false;
        private bool Execution2 = false;

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
                    .AddActivity("fork", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction(
                            "generate",
                            async c => c.OutputRange(Enumerable.Range(0, 10)),
                            b => b.AddFlow<int, ForkNode>()
                        )
                        .AddFork(b => b
                            .AddFlow<int>("final1")
                            .AddFlow<int>("final2")
                        )
                        .AddAction("final1", async c => Execution1 = true)
                        .AddAction("final2", async c => Execution2 = true)
                    )
                )
                ;
        }

        [TestMethod]
        public async Task FlowForked()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("fork", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Execution1);
            Assert.IsTrue(Execution2);
        }
    }
}