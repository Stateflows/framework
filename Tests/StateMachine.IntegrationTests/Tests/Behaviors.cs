using Stateflows.Common;
using Stateflows.Activities;
using Stateflows.Activities;
using StateMachine.IntegrationTests.Classes.Events;
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
                            .AddOnEntry(c => c.Behavior.Publish(new SomeNotification()))
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

                    .AddStateMachine("effectActivity", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddInternalTransition<SomeEvent>(b => b
                                .AddEffectActivity(
                                    "integrated",
                                    b => b.AddSubscription<SomeNotification>()
                                )
                            )
                            .AddTransition<SomeNotification>("state2")
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("guardActivity", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddTransition<SomeEvent>("state2", b => b
                                .AddGuardActivity("guard")
                            )
                        )
                        .AddState("state2")
                    )
                )
                .AddActivities(b => b
                    .AddActivity("nested", b => b
                        .AddAcceptEventAction<SomeEvent>(async c =>
                        {
                            eventConsumed = true;
                            c.Behavior.Publish(new SomeNotification());
                        })
                    )
                    .AddActivity("integrated", b => b
                        .AddInitial(b => b
                            .AddControlFlow("effect")
                        )
                        .AddAction(
                            "effect",
                            async c =>
                            {
                                eventConsumed = true;
                                c.Behavior.Publish(new SomeNotification());
                            }
                        )
                    )
                    .AddActivity("guard", b => b
                        .AddInput(b => b
                            .AddFlow<SomeEvent>("guard")
                        )
                        .AddAction(
                            "guard",
                            async c =>
                            {
                                var t = c.GetTokensOfType<SomeEvent>().First();
                                c.Output(t.TheresSomethingHappeningHere != string.Empty);
                            },
                            b => b.AddFlow<bool, OutputNode>()
                        )
                        .AddOutput()
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

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("submachine", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                someStatus1 = (await sm.SendAsync(new SomeEvent())).Status;

                await Task.Delay(100);

                var currentState = (await sm.GetStatusAsync()).Response;

                currentState1 = currentState.CurrentStates.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                .StateEntry("stateA")
                .StateExit("stateA")
                .StateEntry("stateB")
                .StateEntry("state2")
                .StateExit("stateB")
                .StateMachineFinalize()
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(EventStatus.Forwarded, someStatus1);
            Assert.AreEqual("state2", currentState1);
        }

        [TestMethod]
        public async Task SimpleDoActivity()
        {
            var initialized = false;
            string currentState1 = "";
            var someStatus1 = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("doActivity", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                someStatus1 = (await sm.SendAsync(new SomeEvent())).Status;

                await Task.Delay(200);

                currentState1 = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                .StateEntry("state2")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(EventStatus.Forwarded, someStatus1);
            Assert.AreEqual(true, eventConsumed);
            Assert.AreEqual("state2", currentState1);
        }

        [TestMethod]
        public async Task SimpleEffectIntegration()
        {
            var initialized = false;
            string currentState1 = "";
            var someStatus1 = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("effectActivity", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                someStatus1 = (await sm.SendAsync(new SomeEvent())).Status;

                await Task.Delay(500);

                currentState1 = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
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

        [TestMethod]
        public async Task SimpleGuardIntegration()
        {
            var initialized = false;
            string currentState1 = "";
            var someStatus1 = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("guardActivity", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                someStatus1 = (await sm.SendAsync(new SomeEvent())).Status;

                await Task.Delay(200);

                currentState1 = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                .StateEntry("state2")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(EventStatus.Consumed, someStatus1);
            Assert.AreEqual("state2", currentState1);
        }
    }
}