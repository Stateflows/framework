using Stateflows.Common;
using Stateflows.StateMachines.Sync;
using Stateflows.StateMachines;
using StateMachine.IntegrationTests.Classes.States;
using StateMachine.IntegrationTests.Classes.Transitions;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Transitions : StateflowsTestClass
    {
        private bool? StateExited = null;
        private bool? StateEntered = null;
        private bool? TransitionHappened = null;

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
                        .AddInitialState("state1", b => b
                            .AddOnExit(c => StateExited = true)
                            .AddTransition<SomeEvent>("state2", b => b
                                .AddEffect(c => TransitionHappened = true)
                            )
                        )
                        .AddState("state2", b => b
                            .AddOnEntry(c => StateEntered = c.ExecutionSteps.LastOrDefault()?.SourceName == "state1")
                        )
                    )

                    .AddStateMachine("guarded", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<OtherEvent>("state2", b => b
                                .AddGuard(c => c.Event.AnswerToLifeUniverseAndEverything == 42)
                            )
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("default", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<SomeEvent>("state2")
                        )
                        .AddState("state2", b => b
                            .AddDefaultTransition("state3")
                        )
                        .AddState("state3")
                    )

                    .AddStateMachine("internal", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddOnExit(c => StateExited = true)
                            .AddInternalTransition<SomeEvent>(b => b
                                .AddEffect(c => TransitionHappened = true)
                            )
                        )
                    )

                    .AddStateMachine("self", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddOnExit(c => StateExited = true)
                            .AddTransition<SomeEvent>("state1", b => b
                                .AddEffect(c => TransitionHappened = true)
                            )
                        )
                    )
                )
                ;
        }

        [TestMethod]
        public async Task SimpleTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.IsTrue(StateExited);
            Assert.IsTrue(TransitionHappened);
            Assert.IsTrue(StateEntered);
            Assert.AreEqual("state2", currentState);
        }

        [TestMethod]
        public async Task GuardedTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("guarded", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 43 })).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;
            }

            Assert.AreEqual(EventStatus.NotConsumed, status);
            Assert.IsNull(StateExited);
            Assert.IsNull(StateEntered);
            Assert.AreNotEqual("state2", currentState);
        }

        [TestMethod]
        public async Task GuardedTransitionInvalid()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("guarded", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 42, RequiredParameter = string.Empty })).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;
            }

            Assert.AreEqual(EventStatus.Invalid, status);
            Assert.IsNull(StateExited);
            Assert.IsNull(StateEntered);
            Assert.AreNotEqual("state2", currentState);
        }

        [TestMethod]
        public async Task DefaultTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("default", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state3", currentState);
        }

        [TestMethod]
        public async Task InternalTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("internal", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
            );
            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.IsTrue(TransitionHappened);
            Assert.IsNull(StateExited);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task SelfTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("self", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
            );
            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.IsTrue(TransitionHappened);
            Assert.IsTrue(StateExited);
            Assert.AreEqual("state1", currentState);
        }
    }
}