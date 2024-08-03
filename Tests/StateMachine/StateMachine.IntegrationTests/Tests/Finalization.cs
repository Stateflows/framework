using Stateflows.Common;
using Stateflows.StateMachines.Typed;
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
                .AddStateMachines(b => b
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
                                .AddDefaultTransition("state1-final")
                            )
                            .AddFinalState("state1-final")
                        )
                        .AddFinalState()
                    )
                )
                ;
        }

        //[TestMethod]
        //public async Task NoFinalization()
        //{
        //    var status = EventStatus.Consumed;
        //    string? currentState = "";

        //    if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
        //    {
        //        status = (await sm.SendAsync(new SomeEvent())).Status;

        //        currentState = (await sm.GetCurrentStateAsync()).Response?.StatesStack.FirstOrDefault();
        //    }

        //    Assert.AreEqual(EventStatus.NotConsumed, status);
        //    Assert.AreNotEqual(FinalState.Name, currentState);
        //}

        [TestMethod]
        public async Task SimpleFinalization()
        {
            var initialized = false;
            var finalized = false;
            string currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.First();

                finalized = (await sm.GetStatusAsync())?.Response.BehaviorStatus == BehaviorStatus.Finalized;
            }

            ExecutionSequence.Verify(b => b
                .StateMachineFinalize()
            );
            Assert.IsTrue(initialized);
            Assert.IsTrue(finalized);
            Assert.AreEqual(State<FinalState>.Name, currentState);
        }

        [TestMethod]
        public async Task CascadeFinalization()
        {
            var initialized = false;
            string currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("cascade", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.Skip(1).First() ?? string.Empty;
            }

            ExecutionSequence.Verify(b => b
                .StateFinalize("state1")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual("state1-final", currentState);
        }
    }
}