using Stateflows.Activities.Typed;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Output : StateflowsTestClass
    {
        private bool Executed1 = false;

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
                            .AddControlFlow("main")
                        )
                        .AddStructuredActivity("main", b => b
                            .AddInitial(b => b
                                .AddControlFlow("action1")
                            )
                            .AddAction("action1",
                                async c =>
                                {
                                    c.Output(42);
                                },
                                b => b.AddFlow<int, OutputNode>()
                            )
                            .AddOutput()
                            .AddFlow<int>("final")
                        )
                        .AddAction("final", async c =>
                        {
                            Executed1 = c.Input.OfType<int>().Any();
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task ExceptionHandled()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("structured", "x"), out var a))
            {
                await a.InitializeAsync();
            }

            Assert.IsTrue(Executed1);
        }
    }
}