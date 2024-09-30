using Stateflows.Common;
using Stateflows.Common.Data;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class TokenIdentity : StateflowsTestClass
    {
        private int Count = -1;

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
                    .AddActivity("identity", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction(
                            "generate",
                            async c => c.OutputRange(Enumerable.Range(0, 10)),
                            b => b
                                .AddFlow<int>("process1")
                                .AddFlow<int>("process2")
                        )
                        .AddAction(
                            "process1",
                            async c => c.PassAllTokensOn(),
                            b => b.AddFlow<int>("join")
                        )
                        .AddAction(
                            "process2",
                            async c => c.PassAllTokensOn(),
                            b => b.AddFlow<int>("join")
                        )
                        .AddAction("join", async c =>
                        {
                            Count = c.GetTokensOfType<int>().Count();
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task TokensNotDuplicated()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("identity", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.AreEqual(10, Count);
        }
    }
}