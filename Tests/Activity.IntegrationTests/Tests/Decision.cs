using Stateflows.Activities;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Decision : StateflowsTestClass
    {
        private int ExecutionCount1 = 0;
        private int TokenCount1 = 0;
        private int ExecutionCount2 = 0;
        private int TokenCount2 = 0;
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
                    .AddActivity("tokensDecision", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction(
                            "generate",
                            async c => c.OutputRange(Enumerable.Range(0, 10)),
                            b => b.AddFlow<int, DecisionNode<int>>()
                        )
                        .AddDecision<int>(b => b
                            .AddFlow("final1", b => b.AddGuard(async c => c.Token % 2 == 0))
                            .AddElseFlow("final2")
                        )
                        .AddAction("final1", async c =>
                        {
                            ExecutionCount1++;
                            TokenCount1 += c.GetTokensOfType<int>().Count();
                        })
                        .AddAction("final2", async c =>
                        {
                            ExecutionCount2++;
                            TokenCount2 += c.GetTokensOfType<int>().Count();
                        })
                    )
                    .AddActivity("multipleTokensDecision", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction(
                            "generate",
                            async c =>
                            {
                                c.OutputRange(Enumerable.Range(0, 10));
                                c.Output("test");
                            },
                            b => b
                                .AddFlow<int>("main")
                                .AddFlow<string>("main")
                        )
                        .AddStructuredActivity("main", b => b
                            .AddInput(b => b
                                .AddFlow<int, DecisionNode<int>>()
                                .AddFlow<string>("final1")
                                .AddFlow<string>("final2")
                            )
                            .AddDecision<int>(b => b
                                .AddFlow("final1", b => b.AddGuard(async c => c.Token % 2 == 0))
                                .AddElseFlow("final2")
                            )
                            .AddAction(
                                "final1",
                                async c =>
                                {
                                    ExecutionCount1++;
                                    TokenCount1 += c.GetTokensOfType<int>().Count();
                                    await Task.Delay(1);
                                },
                                b => b.AddFlow<int, OutputNode>()
                            )
                            .AddAction(
                                "final2",
                                async c =>
                                {
                                    ExecutionCount2++;
                                    TokenCount2 += c.GetTokensOfType<int>().Count();
                                },
                                b => b.AddFlow<int, OutputNode>()
                            )
                            .AddOutput()
                        )
                    )
                    .AddActivity("controlDecision", b => b
                        .AddInitial(b => b
                            .AddControlFlow("setup")
                        )
                        .AddAction(
                            "setup",
                            async c => await c.Behavior.Values.SetAsync("value", true),
                            b => b.AddControlFlow<ControlDecisionNode>()
                        )
                        .AddControlDecision(b => b
                            .AddFlow("final1", b => b
                                .AddGuard(async c => await c.Behavior.Values.GetOrDefaultAsync("value", false))
                            )
                            .AddElseFlow("final2")
                        )
                        .AddAction("final1", async c => Execution1 = true)
                        .AddAction("final2", async c => Execution2 = true)
                    )
                )
                ;
        }

        [TestMethod]
        public async Task TokenDecision()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("tokensDecision", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.AreEqual(1, ExecutionCount1);
            Assert.AreEqual(5, TokenCount1);
            Assert.AreEqual(1, ExecutionCount2);
            Assert.AreEqual(5, TokenCount2);
        }

        [TestMethod]
        public async Task MultipleTokenDecision()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("multipleTokensDecision", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.AreEqual(1, ExecutionCount1);
            Assert.AreEqual(5, TokenCount1);
            Assert.AreEqual(1, ExecutionCount2);
            Assert.AreEqual(5, TokenCount2);
        }

        [TestMethod]
        public async Task ControlDecision()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("controlDecision", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Execution1);
            Assert.IsFalse(Execution2);
        }
    }
}