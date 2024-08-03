using Stateflows.Common;
using Stateflows.Activities;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Exceptions : StateflowsTestClass
    {
        public bool eventConsumed = false;

        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddPlantUml()
                .AddStateMachines(b => b
                    .AddStateMachine("uncatched", b => b
                        .AddInitialState("state1", b => b
                            .AddOnEntry(c => throw new Exception("example"))
                            .AddTransition<SomeNotification>("state2")
                        )
                        .AddState("state2")
                    )
                )
                ;
        }

        [TestMethod]
        public async Task Uncatched()
        {
            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("uncatched", "x"), out var sm))
            {
                try
                {
                    await sm.SendAsync(new Initialize());
                }
                catch (Exception)
                {
                    throw;
                }
            }

            Assert.IsTrue(true);
        }
    }
}