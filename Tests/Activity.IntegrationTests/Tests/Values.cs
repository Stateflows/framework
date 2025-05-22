using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    public class ValueAction([GlobalValue] IValue<int> x) : IActionNode
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await x.SetAsync(42);
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
                            var (success, globalCounter) = await c.Behavior.Values.TryGetAsync<int>("x");
                            Executed = success && globalCounter == 42;
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