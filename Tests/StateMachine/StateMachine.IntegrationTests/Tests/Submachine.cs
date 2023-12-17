using Stateflows.Common;
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
                .AddPlantUml()
                .AddStateMachines(b => b
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
                )
                ;
        }

        [TestMethod]
        public async Task SimpleSubmachine()
        {
            var initialized = false;
            string currentState1 = "";
            string currentSubState1 = "";
            string currentState2 = "";
            var someStatus1 = EventStatus.Rejected;
            var someStatus2 = EventStatus.Rejected;

            if (Locator.TryLocateStateMachine(new StateMachineId("submachine", "x"), out var sm))
            {
                initialized = (await sm.InitializeAsync()).Response.InitializationSuccessful;

                someStatus1 = (await sm.SendAsync(new SomeEvent())).Status;

                var uml = await sm.GetPlantUmlAsync();

                var currentState = (await sm.GetCurrentStateAsync()).Response;

                currentState1 = currentState.StatesStack.First();

                currentSubState1 = currentState.StatesStack.Skip(1).First();

                someStatus2 = (await sm.SendAsync(new SomeEvent())).Status;

                currentState2 = (await sm.GetCurrentStateAsync()).Response.StatesStack.First();
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
            Assert.AreEqual(EventStatus.Consumed, someStatus1);
            Assert.AreEqual("state1", currentState1);
            Assert.AreEqual("stateB", currentSubState1);
            Assert.AreEqual(EventStatus.Consumed, someStatus2);
            Assert.AreEqual("state2", currentState2);
        }
    }
}