using Stateflows.Activities.Typed;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Merge : StateflowsTestClass
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
                    .AddActivity("merge", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate1")
                            .AddControlFlow("generate2")
                        )
                        .AddAction(
                            "generate1",
                            async c => c.OutputRange(Enumerable.Range(0, 10)),
                            b => b.AddFlow<int, MergeNode>()
                        )
                        .AddAction(
                            "generate2",
                            async c => c.OutputRange(Enumerable.Range(0, 10)),
                            b => b.AddFlow<int, MergeNode>()
                        )
                        .AddMerge(b => b.AddFlow<int>("final"))
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
        public async Task FlowsMerged()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("merge", "x"), out var a))
            {
                await a.InitializeAsync();
            }

            Assert.AreEqual(2, ExecutionCount);
            Assert.AreEqual(20, TokenCount);
        }
    }
}