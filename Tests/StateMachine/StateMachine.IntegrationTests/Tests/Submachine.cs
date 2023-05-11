using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Submachine : StateflowsTestClass
    {
        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddStateMachine("submachine", b => b
                    .AddExecutionSequenceObserver()
                    .AddInitialState("state1", b => b
                        .AddSubmachine("nested")
                        .AddTransition<SomeEvent>("state2")
                    )
                    .AddState("state2")
                )

                .AddStateMachine("nested", b => b
                    .AddExecutionSequenceObserver()
                    .AddInitialState("stateA", b => b
                        .AddTransition<SomeEvent>("stateB")
                    )
                    .AddState("stateB")
                )
                ;
        }

        [TestMethod]
        public async Task SimpleSubmachine()
        {
            var initialized = false;
            string currentState1 = "";
            string currentState2 = "";
            var someConsumed1 = false;
            var someConsumed2 = false;

            if (Locator.TryLocateStateMachine(new StateMachineId("submachine", "x"), out var sm))
            {
                initialized = await sm.InitializeAsync();

                someConsumed1 = await sm.SendAsync(new SomeEvent());

                currentState1 = (await sm.GetCurrentStateAsync()).Name;

                someConsumed2 = await sm.SendAsync(new SomeEvent());

                currentState2 = (await sm.GetCurrentStateAsync()).Name;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                .StateEntry("stateA")
                .StateExit("stateA")
                .StateEntry("stateB")
                .StateExit("stateB")
                .StateEntry("state2")
            );
            Assert.IsTrue(initialized);
            Assert.IsTrue(someConsumed1);
            Assert.AreEqual("state1", currentState1);
            Assert.IsTrue(someConsumed2);
            Assert.AreEqual("state2", currentState2);
        }
    }
}