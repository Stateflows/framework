using Stateflows.Activities.Typed.Data;
using Stateflows.Activities.Data;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Output : StateflowsTestClass
    {
        private bool Executed1 = false;
        private bool Executed2 = false;
        public static string Value = "boo";

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
                                    c.Output(new Token<int>() { Payload = 42 });
                                },
                                b => b.AddDataFlow<int, OutputNode>()
                            )
                            .AddOutput()
                            .AddDataFlow<int>("final")
                        )
                        .AddAction("final", async c =>
                        {
                            Executed1 = c.Input.OfType<Token<int>>().Any();
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