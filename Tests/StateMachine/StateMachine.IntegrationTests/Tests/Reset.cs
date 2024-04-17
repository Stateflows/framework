using Stateflows.Common;
using Stateflows.StateMachines.Sync;
using Stateflows.StateMachines.Events;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Reset : StateflowsTestClass
    {
        private bool? StateEntered = null;

        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddStateMachines(b => b
                    .AddStateMachine("reset", b => b
                        .AddInitialState("state1", b => b
                            .AddOnEntry(c => StateEntered = true)
                        )
                    )
                )
                ;
        }

        [TestMethod]
        public async Task ResettingInitialized()
        {
            CurrentStateResponse? currentState1 = null;
            CurrentStateResponse? currentState2 = null;
            ResetResponse? resetResponse = null;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("reset", "x"), out var sm))
            {
                await sm.InitializeAsync();

                currentState1 = (await sm.GetCurrentStateAsync()).Response;

                resetResponse = (await sm.ResetAsync()).Response;

                currentState2 = (await sm.GetCurrentStateAsync()).Response;
            }

            Assert.AreEqual(BehaviorStatus.Initialized, currentState1?.BehaviorStatus);
            Assert.AreEqual("state1", currentState1?.StatesStack.FirstOrDefault());
            Assert.IsTrue(StateEntered);
            Assert.AreEqual(BehaviorStatus.NotInitialized, currentState2?.BehaviorStatus);
            Assert.AreNotEqual("state1", currentState2?.StatesStack.FirstOrDefault());
            Assert.IsTrue(resetResponse?.ResetSuccessful);
        }

        [TestMethod]
        public async Task ResettingNotInitialized()
        {
            CurrentStateResponse? currentState1 = null;
            CurrentStateResponse? currentState2 = null;
            ResetResponse? resetResponse = null;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("reset", "x"), out var sm))
            {
                currentState1 = (await sm.GetCurrentStateAsync()).Response;

                resetResponse = (await sm.ResetAsync()).Response;

                currentState2 = (await sm.GetCurrentStateAsync()).Response;
            }

            Assert.AreEqual(BehaviorStatus.NotInitialized, currentState1?.BehaviorStatus);
            Assert.AreNotEqual("state1", currentState1?.StatesStack.FirstOrDefault());
            Assert.AreEqual(BehaviorStatus.NotInitialized, currentState2?.BehaviorStatus);
            Assert.AreNotEqual("state1", currentState2?.StatesStack.FirstOrDefault());
            Assert.IsTrue(resetResponse?.ResetSuccessful);
        }
    }
}