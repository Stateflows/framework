using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    public class TransformationFlow : IFlowTransformation<int, string>, IFlowGuard<int>
    {
        public Task<string> TransformAsync(int token)
            => Task.FromResult(token.ToString());

        public Task<bool> GuardAsync(int token)
            => Task.FromResult(token % 2 == 0);
    }

    [TestClass]
    public class Transformation : StateflowsTestClass
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
                                .AddTransformation<string>(async c => c.Token.ToString())
                            )
                        )
                        .AddAction("check", async c =>
                        {
                            Executed = true;
                            Count = c.GetTokensOfType<string>().Count();
                        })
                    )
                    .AddActivity("typed", b => b
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddAction(
                            "main",
                            async c => c.OutputRange(Enumerable.Range(0, 100)),
                            b => b.AddFlow<int, string, TransformationFlow>("check")
                        )
                        .AddAction("check", async c =>
                        {
                            Executed = true;
                            Count = c.GetTokensOfType<string>().Count();
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task PlainTransformation()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("plain", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed);
            Assert.AreEqual(100, Count);
        }

        [TestMethod]
        public async Task TypedTransformation()
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