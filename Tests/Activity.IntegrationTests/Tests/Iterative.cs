using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Iterative : StateflowsTestClass
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
                                    ExecutionSum += c.GetTokensOfType<int>().Sum();
                                }
                            ),
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
                await a.ExecuteAsync();
            }

            Assert.AreEqual(20, ExecutionCounter);
            Assert.AreEqual(100, ExecutionSum);
        }
    }
}