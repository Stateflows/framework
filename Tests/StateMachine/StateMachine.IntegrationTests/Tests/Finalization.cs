using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Finalization : StateflowsTestClass
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
                .AddStateMachine("simple", b => b
                    .AddExecutionSequenceObserver()
                    .AddInitialState("state1", b => b
                        .AddDefaultTransition<FinalState>()
                    )
                    .AddFinalState()
                )

                .AddStateMachine("cascade", b => b
                    .AddExecutionSequenceObserver()
                    .AddInitialCompositeState("state1", b => b
                        .AddInitialState("state1.1", b => b
                            .AddDefaultTransition<FinalState>()
                        )
                        .AddFinalState()
                    )
                    .AddFinalState()
                )
                ;
        }

        [TestMethod]
        public async Task NoFinalization()
        {
            var status = EventStatus.Consumed;
            string? currentState = "";

            if (Locator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync())?.Name;
            }

            Assert.AreEqual(EventStatus.Rejected, status);
            Assert.AreNotEqual(FinalState.Name, currentState);
        }

        [TestMethod]
        public async Task SimpleFinalization()
        {
            var initialized = false;
            var finalized = false;
            string currentState = string.Empty;

            if (Locator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                initialized = (await sm.InitializeAsync()).Response.InitializationSuccessful;

                currentState = (await sm.GetCurrentStateAsync()).StatesStack.First();

                finalized = (await sm.GetStatusAsync())?.Response.BehaviorStatus == BehaviorStatus.Finalized;
            }

            ExecutionSequence.Verify(b => b
                .StateMachineFinalize()
            );
            Assert.IsTrue(initialized);
            Assert.IsTrue(finalized);
            Assert.AreEqual(StateInfo<FinalState>.Name, currentState);
        }

        [TestMethod]
        public async Task CascadeFinalization()
        {
            var initialized = false;
            string currentState = string.Empty;

            if (Locator.TryLocateStateMachine(new StateMachineId("cascade", "x"), out var sm))
            {
                initialized = (await sm.InitializeAsync()).Response.InitializationSuccessful;

                currentState = (await sm.GetCurrentStateAsync()).StatesStack.Skip(1).First() ?? string.Empty;
            }

            ExecutionSequence.Verify(b => b
                .StateFinalize("state1")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(StateInfo<FinalState>.Name, currentState);
        }
    }
}