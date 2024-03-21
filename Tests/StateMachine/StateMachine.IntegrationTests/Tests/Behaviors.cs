using Stateflows.Common;
using Stateflows.Activities;
using Stateflows.StateMachines.Sync;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Behaviors : StateflowsTestClass
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
                    .AddStateMachine("submachine", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddSubmachine("nested", b => b
                                .AddForwardedEvent<SomeEvent>()
                                .AddSubscription<SomeNotification>()
                            )
                            .AddTransition<SomeNotification>("state2")
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("nested", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("stateA", b => b
                            .AddTransition<SomeEvent>("stateB")
                        )
                        .AddState("stateB", b => b
                            .AddOnEntry(c => c.StateMachine.Publish(new SomeNotification()))
                        )
                    )

                    .AddStateMachine("doActivity", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddDoActivity("nested", b => b
                                .AddForwardedEvent<SomeEvent>()
                                .AddSubscription<SomeNotification>()
                            )
                            .AddTransition<SomeNotification>("state2")
                        )
                        .AddState("state2")
                    )
                )
                .AddActivities(b => b
                    .AddActivity("nested", b => b
                        .AddAcceptEventAction<SomeEvent>(async c =>
                        {
                            eventConsumed = true;
                            c.Activity.Publish(new SomeNotification());
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task SimpleSubmachine()
        {
            var initialized = false;
            string currentState1 = "";
            var someStatus1 = EventStatus.Rejected;

            if (Locator.TryLocateStateMachine(new StateMachineId("submachine", "x"), out var sm))
            {
                initialized = (await sm.InitializeAsync()).Response.InitializationSuccessful;

                someStatus1 = (await sm.SendAsync(new SomeEvent())).Status;

                await Task.Delay(100);

                var currentState = (await sm.GetCurrentStateAsync()).Response;

                currentState1 = currentState.StatesStack.First();
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                .StateEntry("stateA")
                .StateExit("stateA")
                .StateEntry("stateB")
                .StateExit("stateB")
                .StateMachineFinalize()
                .StateEntry("state2")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(EventStatus.Consumed, someStatus1);
            Assert.AreEqual("state2", currentState1);
        }

        [TestMethod]
        public async Task SimpleDoActivity()
        {
            var initialized = false;
            string currentState1 = "";
            var someStatus1 = EventStatus.Rejected;

            if (Locator.TryLocateStateMachine(new StateMachineId("doActivity", "x"), out var sm))
            {
                initialized = (await sm.InitializeAsync()).Response.InitializationSuccessful;

                someStatus1 = (await sm.SendAsync(new SomeEvent())).Status;

                await Task.Delay(200);

                currentState1 = (await sm.GetCurrentStateAsync()).Response.StatesStack.First();
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                .StateEntry("state2")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(EventStatus.Consumed, someStatus1);
            Assert.AreEqual(true, eventConsumed);
            Assert.AreEqual("state2", currentState1);
        }
    }
}