using Stateflows.Common;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Sync;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class ExternalTransitions : StateflowsTestClass
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

                    .AddStateMachine("nested-to-root", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialCompositeState("state1", b => b
                            .AddInitialState("state1.1", b => b
                                .AddDefaultTransition("state1.2")
                            )
                            .AddState("state1.2", b => b
                                .AddDefaultTransition("state2")
                            )
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("root-to-nested", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddDefaultTransition("state2.1.2")
                        )
                        .AddCompositeState("state2", b => b
                            .AddInitialCompositeState("state2.1", b => b
                                .AddInitialState("state2.1.1", b => b
                                    .AddDefaultTransition("state2.1.2")
                                )
                                .AddState("state2.1.2")
                            )
                        )
                    )

                    .AddStateMachine("nested-to-nested", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialCompositeState("state1", b => b
                            .AddInitialState("state1.1", b => b
                                .AddDefaultTransition("state1.2")
                            )
                            .AddState("state1.2", b => b
                                .AddDefaultTransition("state2.1")
                            )
                        )
                        .AddCompositeState("state2", b => b
                            .AddInitialCompositeState("state2.1", b => b
                                .AddInitialState("state2.1.1", b => b
                                    .AddDefaultTransition("state2.1.2")
                                )
                                .AddState("state2.1.2")
                            )
                        )
                    )

                    .AddStateMachine("nested-to-parent", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialCompositeState("state1", b => b
                            .AddInitialCompositeState("state1.1", b => b
                                .AddInitialState("state1.1.1", b => b
                                    .AddTransition<SomeEvent>("state1.1.2")
                                )
                                .AddState("state1.1.2", b => b
                                    .AddDefaultTransition("state1.1")
                                )
                            )
                        )
                    )

                )
                ;
        }

        [TestMethod]
        public async Task NestedToRootTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("nested-to-root", "x"), out var sm))
            {
                status = (await sm.InitializeAsync()).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.First();
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .StateInitialize("state1")
                .StateEntry("state1.1")
                .TransitionGuard(EventInfo<CompletionEvent>.Name, "state1.1", "state1.2")
                .StateExit("state1.1")
                .TransitionEffect(EventInfo<CompletionEvent>.Name, "state1.1", "state1.2")
                .StateEntry("state1.2")
                .TransitionGuard(EventInfo<CompletionEvent>.Name, "state1.2", "state2")
                .StateExit("state1.2")
                .StateExit("state1")
                .TransitionEffect(EventInfo<CompletionEvent>.Name, "state1.2", "state2")
                .StateEntry("state2")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state2", currentState);
        }

        [TestMethod]
        public async Task RootToNestedTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("root-to-nested", "x"), out var sm))
            {
                status = (await sm.InitializeAsync()).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.Last();
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .TransitionGuard(EventInfo<CompletionEvent>.Name, "state1", "state2.1.2")
                .StateExit("state1")
                .TransitionEffect(EventInfo<CompletionEvent>.Name, "state1", "state2.1.2")
                .StateEntry("state2")
                .StateEntry("state2.1")
                .StateEntry("state2.1.2")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state2.1.2", currentState);
        }

        [TestMethod]
        public async Task NestedToNestedTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("nested-to-nested", "x"), out var sm))
            {
                status = (await sm.InitializeAsync()).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.Last();
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .StateInitialize("state1")
                .StateEntry("state1.1")
                .TransitionGuard(EventInfo<CompletionEvent>.Name, "state1.1", "state1.2")
                .StateExit("state1.1")
                .TransitionEffect(EventInfo<CompletionEvent>.Name, "state1.1", "state1.2")
                .StateEntry("state1.2")
                .TransitionGuard(EventInfo<CompletionEvent>.Name, "state1.2", "state2.1")
                .StateExit("state1.2")
                .StateExit("state1")
                .TransitionEffect(EventInfo<CompletionEvent>.Name, "state1.2", "state2.1")
                .StateEntry("state2")
                .StateEntry("state2.1")
                .StateEntry("state2.1.2")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state2.1.2", currentState);
        }

        [TestMethod]
        public async Task NestedToParentTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("nested-to-parent", "x"), out var sm))
            {
                await sm.InitializeAsync();
                
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.Last();
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .StateInitialize("state1")
                .StateEntry("state1.1")
                .StateInitialize("state1.1")
                .StateEntry("state1.1.1")
                .TransitionGuard(EventInfo<SomeEvent>.Name, "state1.1.1", "state1.1.2")
                .StateExit("state1.1.1")
                .TransitionEffect(EventInfo<SomeEvent>.Name, "state1.1.1", "state1.1.2")
                .StateEntry("state1.1.2")
                .TransitionGuard(EventInfo<CompletionEvent>.Name, "state1.1.2", "state1.1")
                .StateExit("state1.1.2")
                .StateExit("state1.1")
                .TransitionEffect(EventInfo<CompletionEvent>.Name, "state1.1.2", "state1.1")
                .StateEntry("state1.1")
                .StateInitialize("state1.1")
                .StateEntry("state1.1.1")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state1.1.1", currentState);
        }
    }
}