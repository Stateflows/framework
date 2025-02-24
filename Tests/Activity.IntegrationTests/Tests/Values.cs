using Stateflows.Activities;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    public class ValueAction : IActionNode
    {
        internal readonly GlobalValue<int> globalCounter = new("x");

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            globalCounter.Set(42);

            return Task.CompletedTask;
        }
    }

    [TestClass]
    public class Values : StateflowsTestClass
    {
        private bool Executed = false;

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
                    .AddActivity("values", b => b
                        .AddInitial(b => b
                            .AddControlFlow<ValueAction>()
                        )
                        .AddAction<ValueAction>(b => b
                            .AddControlFlow("action2")
                        )
                        .AddAction("action2", async c =>
                        {
                            Executed = c.Activity.Values.TryGet("x", out int globalCounter) && globalCounter == 42;
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task ValuesManagement()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("values", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed);
        }
    }
}