using Stateflows.Common;
using Stateflows.StateMachines.Events;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    internal class MyState : State
    {
        public override Task OnEntryAsync()
        {
            return base.OnEntryAsync();
        }
    }

    [TestClass]
    public class Composite : StateflowsTestClass
    {
        private bool? ParentStateExited = null;
        private bool? ChildStateExited = null;
        private int InitializeCounter = 0;

        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddStateMachine("composite", b => b
                    .AddExecutionSequenceObserver()
                    .AddInitialCompositeState("state1", b => b
                        .AddInitialState("state2")
                        .AddTransition<OtherEvent>("state3")
                    )
                    .AddCompositeState("state3", b => b
                        .AddInitialState("state4")
                    )
                )

                .AddStateMachine("default", b => b
                    .AddExecutionSequenceObserver()
                    .AddInitialState("state1", b => b
                        .AddTransition<OtherEvent>("state2")
                    )
                    .AddCompositeState("state2", b => b
                        .AddInitialState("state3", b => b
                            .AddDefaultTransition("state4")
                        )
                        .AddCompositeState("state4", b => b
                            .AddInitialState("state5", b => b
                                .AddDefaultTransition("state6")
                            )
                            .AddState("state6")
                        )
                    )
                )

                .AddStateMachine("exits", b => b
                    .AddExecutionSequenceObserver()
                    .AddInitialCompositeState("state1", b => b
                        .AddOnExit(c => ParentStateExited = true)
                        .AddInitialState("state2", b => b
                            .AddOnExit(c => ChildStateExited = true)
                            .AddTransition<OtherEvent>("state3")
                        )
                        .AddState("state3")
                    )
                )

                .AddStateMachine("single", b => b
                    .AddExecutionSequenceObserver()
                    .AddInitialCompositeState("state1", b => b
                        .AddOnInitialize(c => InitializeCounter++)
                        .AddInitialState("state2", b => b
                            .AddTransition<OtherEvent>("state3")
                        )
                        .AddState("state3", b => b
                            .AddDefaultTransition("state4")
                        )
                        .AddState("state4")
                    )
                )
                ;
        }

        [TestMethod]
        public async Task SelfTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "";
            string? currentInnerState = "";

            if (Locator.TryLocateStateMachine(new StateMachineId("composite", "x"), out var sm))
            {
                await sm.InitializeAsync();

                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 42 })).Status;

                currentState = (await sm.GetCurrentStateAsync()).StatesStack.First();
                currentInnerState = (await sm.GetCurrentStateAsync()).StatesStack.Skip(1).First();
            }

            ExecutionSequence.Verify(b => b
                .StateExit("state2")
                .StateExit("state1")
                .TransitionEffect(EventInfo<OtherEvent>.Name, "state1", "state3")
                .StateEntry("state3")
                .StateInitialize("state3")
                .StateEntry("state4")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state3", currentState);
            Assert.AreEqual("state4", currentInnerState);
        }

        [TestMethod]
        public async Task DefaultTransition()
        {
            var status = EventStatus.Rejected;
            CurrentStateResponse? currentState = null;

            if (Locator.TryLocateStateMachine(new StateMachineId("default", "x"), out var sm))
            {
                await sm.InitializeAsync();

                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 42 })).Status;

                currentState = await sm.GetCurrentStateAsync();
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .TransitionGuard(EventInfo<OtherEvent>.Name, "state1", "state2")
                .StateExit("state1")
                .TransitionEffect(EventInfo<OtherEvent>.Name, "state1", "state2")
                .StateEntry("state2")
                .StateInitialize("state2")
                .StateEntry("state3")
                .TransitionGuard(EventInfo<Completion>.Name, "state3", "state4")
                .StateExit("state3")
                .TransitionEffect(EventInfo<Completion>.Name, "state3", "state4")
                .StateEntry("state4")
                .StateInitialize("state4")
                .StateEntry("state5")
                .TransitionGuard(EventInfo<Completion>.Name, "state5", "state6")
                .StateExit("state5")
                .TransitionEffect(EventInfo<Completion>.Name, "state5", "state6")
                .StateEntry("state6")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state2", currentState?.StatesStack.First());
            Assert.AreEqual("state4", currentState?.StatesStack.Skip(1).First());
            Assert.AreEqual("state6", currentState?.StatesStack.Skip(2).First());
        }

        [TestMethod]
        public async Task LocalExits()
        {
            var status = EventStatus.Rejected;
            CurrentStateResponse? currentState = null;

            if (Locator.TryLocateStateMachine(new StateMachineId("exits", "x"), out var sm))
            {
                await sm.InitializeAsync();

                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 42 })).Status;

                currentState = await sm.GetCurrentStateAsync();
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .StateInitialize("state1")
                .StateEntry("state2")
                .TransitionEffect(EventInfo<OtherEvent>.Name, "state2", "state3")
                .StateEntry("state3")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.IsNull(ParentStateExited);
            Assert.IsTrue(ChildStateExited);
            Assert.AreEqual("state1", currentState?.StatesStack.First());
            Assert.AreEqual("state3", currentState?.StatesStack.Skip(1).First());
        }

        [TestMethod]
        public async Task SingleInitialization()
        {
            var status = EventStatus.Rejected;
            CurrentStateResponse currentState = null;

            if (Locator.TryLocateStateMachine(new StateMachineId("single", "x"), out var sm))
            {
                await sm.InitializeAsync();

                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 42 })).Status;

                currentState = await sm.GetCurrentStateAsync();
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .StateInitialize("state1")
                .StateEntry("state2")
                .TransitionEffect(EventInfo<OtherEvent>.Name, "state2", "state3")
                .StateEntry("state3")
                .TransitionEffect(EventInfo<Completion>.Name, "state3", "state4")
                .StateEntry("state4")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual(1, InitializeCounter);
            Assert.AreEqual("state1", currentState?.StatesStack.First());
            Assert.AreEqual("state4", currentState?.StatesStack.Skip(1).First());
        }
    }
}