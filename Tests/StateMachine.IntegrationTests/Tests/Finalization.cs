using Stateflows.Common;
using StateMachine.IntegrationTests.Classes.Events;
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
                
                    .AddStateMachine("forceFinalization", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddTransition<SomeEvent>("state2")
                        )
                        .AddState("state2", b => b
                            .AddOnEntry(_ => Task.Delay(1000))
                            .AddTransition<OtherEvent>("state3")
                        )
                        .AddState("state3")
                    )
                
                    .AddStateMachine("noReinitialization", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddTransition<SomeEvent>("state2")
                        )
                        .AddState("state2")
                    )
                )
                ;
        }

        [TestMethod]
        public async Task ForceFinalization()
        {
            var status1 = EventStatus.Undelivered;
            var status2 = EventStatus.Undelivered;
            var status3 = EventStatus.Undelivered;
            var behaviorStatus = BehaviorStatus.Unknown;
            string? currentState = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("forceFinalization", "x"), out var sm))
            {
                var task1 = Task.Run(async () =>
                {
                    status1 = (await sm.SendAsync(new SomeEvent())).Status;
                });
                
                var task2 = Task.Run(async () =>
                {
                    await Task.Delay(100);
                    status2 = (await sm.SendAsync(new OtherEvent())).Status;
                });
                
                var task3 = Task.Run(async () => {
                    await Task.Delay(200);
                    status3 = (await sm.SendAsync(new Finalize())).Status;
                });
                
                await Task.WhenAll(task1, task2, task3);

                var statusResponse = (await sm.GetStatusAsync()).Response;
                currentState = statusResponse?.CurrentStates?.Value;
                behaviorStatus = statusResponse?.BehaviorStatus ?? BehaviorStatus.Unknown;
            }

            Assert.AreEqual(EventStatus.Cancelled, status1);
            Assert.AreEqual(EventStatus.NotConsumed, status2);
            Assert.AreEqual(EventStatus.NotConsumed, status3);
            Assert.AreEqual(BehaviorStatus.Finalized, behaviorStatus);
        }

        [TestMethod]
        public async Task SimpleFinalization()
        {
            var initialized = false;
            var finalized = false;
            string currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates.Value;

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

                var result = await sm.GetStatusAsync();

                status = result.Response.BehaviorStatus;
                
                currentState = result.Response.CurrentStates.Root.Nodes.First().Value ?? string.Empty;
            }

            ExecutionSequence.Verify(b => b
                .StateFinalize("state1")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(BehaviorStatus.Initialized, status);
            Assert.AreEqual("state1-final", currentState);
        }

        [TestMethod]
        public async Task NoReinitialization()
        {
            EventStatus status1 = EventStatus.Undelivered;
            EventStatus status2 = EventStatus.Undelivered;
            EventStatus status3 = EventStatus.Undelivered;
            var initialized = false;
            var finalized = false;
            string currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("noReinitialization", "x"), out var sm))
            {
                status1 = (await sm.SendAsync(new SomeEvent())).Status;

                status2 = (await sm.FinalizeAsync()).Status;
                
                status3 = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateMachineFinalize()
            );
            Assert.AreEqual(EventStatus.Consumed, status1);
            Assert.AreEqual(EventStatus.Consumed, status2);
            Assert.AreEqual(EventStatus.NotConsumed, status3);
            Assert.AreEqual(null, currentState);
        }
    }
}