using Stateflows.Common;
using Stateflows.Common.Data;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Structured : StateflowsTestClass
    {
        private bool Executed = false;
        private int Counter = 0;
        private readonly object LockHandle = new();

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
                    .AddActivity("structured", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction(
                            "generate",
                            async c => c.OutputRange(Enumerable.Range(0, 10)),
                            b => b.AddFlow<int>("main")
                        )
                        .AddStructuredActivity("main", b => b
                            .AddInitial(b => b
                                .AddControlFlow("action1")
                            )
                            .AddAction("action1", async c => { })
                            .AddControlFlow("final")
                        )
                        .AddAction("final", async c =>
                        {
                            Executed = true;
                        })
                    )
                    .AddActivity("parallel", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction(
                            "generate",
                            async c => c.OutputRange(Enumerable.Range(0, 10)),
                            b => b.AddFlow<int>("main")
                        )
                        .AddParallelActivity<int>("main", b => b
                            .AddInitial(b => b
                                .AddControlFlow("action1")
                            )
                            .AddAction("action1", async c =>
                            {
                                lock (LockHandle)
                                {
                                    Counter++;
                                }
                            })
                            .AddControlFlow("final")
                        )
                        .AddAction("final", async c =>
                        {
                            Executed = true;
                        })
                    )
                    .AddActivity("iterative", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction(
                            "generate",
                            async c => c.OutputRange(Enumerable.Range(0, 10)),
                            b => b.AddFlow<int>("main")
                        )
                        .AddParallelActivity<int>("main", b => b
                            .AddInitial(b => b
                                .AddControlFlow("action1")
                            )
                            .AddAction("action1", async c => Counter++)
                            .AddControlFlow("final")
                        )
                        .AddAction("final", async c =>
                        {
                            Executed = true;
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task StructuredActivityFinished()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("structured", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed);
        }

        [TestMethod]
        public async Task ParallelActivityFinished()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("parallel", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed);
            Assert.AreEqual(10, Counter);
        }

        [TestMethod]
        public async Task IterativeActivityFinished()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("iterative", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed);
            Assert.AreEqual(10, Counter);
        }
    }
}