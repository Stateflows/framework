using Stateflows.Common;
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
                .AddStateMachine("simple", b => b
                    .AddInitialState("state1", b => b
                        .AddOnExit(c => StateExited = true)
                        .AddTransition<SomeEvent>("state2", b => b
                            .AddEffect(c => TransitionHappened = true)
                        )
                    )
                    .AddState("state2", b => b
                        .AddOnEntry(c => StateEntered = true)
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
                    .AddInitialState("state1", b => b
                        .AddOnExit(c => StateExited = true)
                        .AddInternalTransition<SomeEvent>(b => b
                            .AddEffect(c => TransitionHappened = true)
                        )
                    )
                )

                .AddStateMachine("self", b => b
                    .AddInitialState("state1", b => b
                        .AddOnExit(c => StateExited = true)
                        .AddTransition<SomeEvent>("state1", b => b
                            .AddEffect(c => TransitionHappened = true)
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

            if (Locator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                await sm.InitializeAsync();

                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).StatesStack.First();
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

            if (Locator.TryLocateStateMachine(new StateMachineId("guarded", "x"), out var sm))
            {
                await sm.InitializeAsync();

                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 43 })).Status;

                currentState = (await sm.GetCurrentStateAsync()).StatesStack.First();
            }

            Assert.AreEqual(EventStatus.NotConsumed, status);
            Assert.IsNull(StateExited);
            Assert.IsNull(StateEntered);
            Assert.AreNotEqual("state2", currentState);
        }

        [TestMethod]
        public async Task DefaultTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (Locator.TryLocateStateMachine(new StateMachineId("default", "x"), out var sm))
            {
                await sm.InitializeAsync();

                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).StatesStack.First();
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state3", currentState);
        }

        [TestMethod]
        public async Task InternalTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (Locator.TryLocateStateMachine(new StateMachineId("internal", "x"), out var sm))
            {
                await sm.InitializeAsync();

                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).StatesStack.First();
            }

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

            if (Locator.TryLocateStateMachine(new StateMachineId("self", "x"), out var sm))
            {
                await sm.InitializeAsync();

                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).StatesStack.First();
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.IsTrue(TransitionHappened);
            Assert.IsTrue(StateExited);
            Assert.AreEqual("state1", currentState);
        }
    }
}