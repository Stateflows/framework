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
                        .AddInitializer<SomeEvent>(async c => true)
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
            StateMachineInfo? currentState1 = null;
            StateMachineInfo? currentState2 = null;
            EventStatus? resetResponse = null;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("reset", "x"), out var sm))
            {
                await sm.SendAsync(new SomeEvent());

                currentState1 = (await sm.GetCurrentStateAsync()).Response;

                resetResponse = (await sm.ResetAsync()).Status;

                currentState2 = (await sm.GetCurrentStateAsync()).Response;
            }

            Assert.AreEqual(BehaviorStatus.Initialized, currentState1?.BehaviorStatus);
            Assert.AreEqual("state1", currentState1?.StatesStack.FirstOrDefault());
            Assert.IsTrue(StateEntered);
            Assert.AreEqual(BehaviorStatus.NotInitialized, currentState2?.BehaviorStatus);
            Assert.AreNotEqual("state1", currentState2?.StatesStack.FirstOrDefault());
            Assert.AreEqual(EventStatus.Consumed, resetResponse);
        }

        [TestMethod]
        public async Task ResettingNotInitialized()
        {
            StateMachineInfo? currentState1 = null;
            StateMachineInfo? currentState2 = null;
            EventStatus? resetStatus = null;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("reset", "x"), out var sm))
            {
                currentState1 = (await sm.GetCurrentStateAsync()).Response;

                resetStatus = (await sm.ResetAsync()).Status;

                currentState2 = (await sm.GetCurrentStateAsync()).Response;
            }

            Assert.AreEqual(BehaviorStatus.NotInitialized, currentState1?.BehaviorStatus);
            Assert.AreNotEqual("state1", currentState1?.StatesStack.FirstOrDefault());
            Assert.AreEqual(BehaviorStatus.NotInitialized, currentState2?.BehaviorStatus);
            Assert.AreNotEqual("state1", currentState2?.StatesStack.FirstOrDefault());
            Assert.AreEqual(EventStatus.Rejected, resetStatus);
        }
    }
}