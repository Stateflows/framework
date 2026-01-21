using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Iterative : StateflowsTestClass
    {
        private int ExecutionCounter = 0;
        private int ExecutionSum1 = 0;
        private int ExecutionSum2 = 0;

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
                    .AddActivity("iterative", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction("generate",
                            async c => c.OutputRange(Enumerable.Repeat(1, 100)),
                            b => b.AddFlow<int>("iterative")
                        )
                        .AddIterativeActivity<int>("iterative", b => b
                            .AddInput(b => b
                                .AddFlow<int>("action1")
                            )
                            .AddAction("action1",
                                async c =>
                                {
                                    ExecutionCounter++;
                                    ExecutionSum1 += c.GetTokensOfType<int>().Sum();
                                }
                            ),
                            5
                        )
                    )
                    .AddActivity("iterativeOutput" , b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction("generate",
                            async c => c.OutputRange(Enumerable.Repeat(1, 100)),
                            b => b.AddFlow<int>("iterative")
                        )
                        .AddIterativeActivity<int>("iterative", b => b
                            .AddInput(b => b
                                .AddFlow<int>("action1")
                            )
                            .AddAction("action1",
                                async c =>
                                {
                                    ExecutionCounter++;
                                    var sum = c.GetTokensOfType<int>().Sum();
                                    ExecutionSum1 += sum;
                                    c.Output(sum);
                                },
                                b => b.AddFlow<int, OutputNode>()
                            )
                            .AddOutput()
                            // .AddControlFlow("collect"),
                            .AddFlow<int>("collect"),
                            5
                        )
                        .AddAction("collect", async c =>
                        {
                            ExecutionSum2 += c.GetTokensOfType<int>().Sum();
                        })
                    )
                    .AddActivity("iterativeWithBreak", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction("generate",
                            async c => c.OutputRange(Enumerable.Repeat(1, 100)),
                            b => b.AddFlow<int>("iterative")
                        )
                        .AddIterativeActivity<int>("iterative", b => b
                            .AddInput(b => b
                                .AddFlow<int>("action1")
                            )
                            .AddAction("action1",
                                async c =>
                                {
                                    ExecutionCounter++;
                                    ExecutionSum1 += c.GetTokensOfType<int>().Sum();
                                    if (ExecutionSum1 > 42)
                                    {
                                        c.Output(ExecutionSum1);
                                    }
                                },
                                b => b.AddFlow<int, FinalNode>()
                            )
                            .AddFinal(),
                            5
                        )
                    )
                )
                ;
        }

        [TestMethod]
        public async Task BasicIterativeActivity()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("iterative", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.AreEqual(20, ExecutionCounter);
            Assert.AreEqual(100, ExecutionSum1);
        }

        [TestMethod]
        public async Task IterativeActivityWithOutput()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("iterativeOutput", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.AreEqual(20, ExecutionCounter);
            Assert.AreEqual(100, ExecutionSum1);
            Assert.AreEqual(ExecutionSum1, ExecutionSum2);
        }

        [TestMethod]
        public async Task IterativeActivityWithBreak()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("iterativeWithBreak", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.AreEqual(9, ExecutionCounter);
            Assert.AreEqual(45, ExecutionSum1);
        }
    }
}