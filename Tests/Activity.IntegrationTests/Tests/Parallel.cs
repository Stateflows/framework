using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Parallel : StateflowsTestClass
    {
        private int ExecutionCounter = 0;
        private int ExecutionSum = 0;

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
                    .AddActivity("parallel", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction("generate",
                            async c => c.OutputRange(Enumerable.Repeat(1, 100)),
                            b => b.AddFlow<int>("parallel")
                        )
                        .AddParallelActivity<int>("parallel", b => b
                            .AddInput(b => b
                                .AddFlow<int>("action1")
                            )
                            .AddAction("action1",
                                async c =>
                                {
                                    lock (this)
                                    {
                                        ExecutionCounter++;
                                        ExecutionSum += c.GetTokensOfType<int>().Sum();
                                    }
                                }
                            ),
                            5
                        )
                    )
                )
                ;
        }

        [TestMethod]
        public async Task BasicParallelActivity()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("parallel", "x"), out var a))
            {
                await a.InitializeAsync();
            }

            Assert.AreEqual(20, ExecutionCounter);
            Assert.AreEqual(100, ExecutionSum);
        }
    }
}