using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Join : StateflowsTestClass
    {
        private int ExecutionCount = 0;
        private int TokenCount = 0;

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
                    .AddActivity("join", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate1")
                            .AddControlFlow("generate2")
                        )
                        .AddAction(
                            "generate1",
                            async c => c.OutputRange(Enumerable.Range(0, 10)),
                            b => b.AddFlow<int, JoinNode>()
                        )
                        .AddAction(
                            "generate2",
                            async c => c.OutputRange(Enumerable.Range(0, 10)),
                            b => b.AddFlow<int, JoinNode>()
                        )
                        .AddJoin(b => b.AddFlow<int>("final"))
                        .AddAction("final", async c =>
                        {
                            ExecutionCount++;
                            TokenCount += c.GetTokensOfType<int>().Count();
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task FlowsJoined()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("join", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.AreEqual(1, ExecutionCount);
            Assert.AreEqual(20, TokenCount);
        }
    }
}