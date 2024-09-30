using Stateflows.Activities.Typed;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    public class IntHolder
    {
        public int Value { get; set; }
    }

    public class GuardFlow : IFlowGuard<int>
    {
        public Task<bool> GuardAsync(int token)
            => Task.FromResult(token % 2 == 0);
    }

    [TestClass]
    public class Guard : StateflowsTestClass
    {
        private bool Executed = false;
        private int Count = 0;

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
                    .AddActivity("plain", b => b
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddAction(
                            "main",
                            async c => c.OutputRange(Enumerable.Range(0, 100)),
                            b => b.AddFlow<int>("check", b => b
                                .AddGuard(async c => c.Token % 2 == 0)
                            )
                        )
                        .AddAction("check", async c =>
                        {
                            Executed = true;
                            Count = c.GetTokensOfType<int>().Count();
                        })
                    )
                    .AddActivity("typed", b => b
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddAction(
                            "main",
                            async c => c.OutputRange(Enumerable.Range(0, 100)),
                            b => b.AddFlow<int, GuardFlow>("check")
                        )
                        .AddAction("check", async c =>
                        {
                            Executed = true;
                            Count = c.GetTokensOfType<int>().Count();
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task PlainGuard()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("plain", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed);
            Assert.AreEqual(50, Count);
        }

        [TestMethod]
        public async Task TypedGuard()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("typed", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed);
            Assert.AreEqual(50, Count);
        }
    }
}