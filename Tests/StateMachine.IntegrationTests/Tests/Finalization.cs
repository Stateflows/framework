using Stateflows.Common;
using Stateflows.StateMachines;
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

        //        currentState = (await sm.GetCurrentStateAsync()).Response?.StatesTree.Value;
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

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;

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
            BehaviorStatus status = BehaviorStatus.Unknown;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("cascade", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                var result = await sm.GetCurrentStateAsync();

                status = result.Response.BehaviorStatus;
                
                currentState = result.Response.StatesTree.Root.Nodes.First().Value ?? string.Empty;
            }

            ExecutionSequence.Verify(b => b
                .StateFinalize("state1")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(BehaviorStatus.Initialized, status);
            Assert.AreEqual("state1-final", currentState);
        }
    }
}