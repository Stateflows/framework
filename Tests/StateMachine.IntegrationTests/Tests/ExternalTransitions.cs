using Stateflows.Common;
using StateMachine.IntegrationTests.Classes.Events;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class ExternalTransitions : StateflowsTestClass
    {
        private static int counter = 0;
        
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

                    .AddStateMachine("parent-to-nested-external", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialCompositeState("state1", b => b
                            .AddState("state1.1")
                            .AddTransition<SomeEvent>("state1.1", b => b
                                .SetIsLocal(false)
                            )
                        )
                    )

                    .AddStateMachine("parent-to-nested-local", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialCompositeState("state1", b => b
                            .AddState("state1.1")
                            .AddTransition<SomeEvent>("state1.1", b => b
                                .SetIsLocal(true)
                            )
                        )
                    )

                    .AddStateMachine("nested-to-parent-external", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialCompositeState("state1", b => b
                            .AddState("state1.1", b => b
                                .AddTransition<OtherEvent>("state1", b => b
                                    .SetIsLocal(false)
                                )
                            )
                            .AddTransition<SomeEvent>("state1.1")
                        )
                    )

                    .AddStateMachine("nested-to-parent-local", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialCompositeState("state1", b => b
                            .AddState("state1.1", b => b
                                .AddTransition<OtherEvent>("state1", b => b
                                    .SetIsLocal(true)
                                )
                            )
                            .AddTransition<SomeEvent>("state1.1")
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
                status = (await sm.SendAsync(new Initialize())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .StateInitialize("state1")
                .StateEntry("state1.1")
                .TransitionGuard(Event<Completion>.Name, "state1.1", "state1.2")
                .StateExit("state1.1")
                .TransitionEffect(Event<Completion>.Name, "state1.1", "state1.2")
                .StateEntry("state1.2")
                .TransitionGuard(Event<Completion>.Name, "state1.2", "state2")
                .StateExit("state1.2")
                .StateExit("state1")
                .TransitionEffect(Event<Completion>.Name, "state1.2", "state2")
                .StateEntry("state2")
            );

            Assert.AreEqual(EventStatus.Initialized, status);
            Assert.AreEqual("state2", currentState);
        }

        [TestMethod]
        public async Task RootToNestedTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("root-to-nested", "x"), out var sm))
            {
                status = (await sm.SendAsync(new Initialize())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.GetAllNodes_ChildrenFirst().First().Value;
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .TransitionGuard(Event<Completion>.Name, "state1", "state2.1.2")
                .StateExit("state1")
                .TransitionEffect(Event<Completion>.Name, "state1", "state2.1.2")
                .StateEntry("state2")
                .StateEntry("state2.1")
                .StateEntry("state2.1.2")
            );

            Assert.AreEqual(EventStatus.Initialized, status);
            Assert.AreEqual("state2.1.2", currentState);
        }

        [TestMethod]
        public async Task ParentToNestedExternalTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("parent-to-nested-external", "x"), out var sm))
            {
                await sm.SendAsync(new Initialize());
                
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.GetAllNodes_ChildrenFirst().First().Value;
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .TransitionGuard(Event<SomeEvent>.Name, "state1", "state1.1")
                .StateExit("state1")
                .TransitionEffect(Event<SomeEvent>.Name, "state1", "state1.1")
                .StateEntry("state1")
                .StateEntry("state1.1")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state1.1", currentState);
        }

        [TestMethod]
        public async Task ParentToNestedLocalTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("parent-to-nested-local", "x"), out var sm))
            {
                await sm.SendAsync(new Initialize());
                
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.GetAllNodes_ChildrenFirst().First().Value;
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .TransitionGuard(Event<SomeEvent>.Name, "state1", "state1.1")
                .TransitionEffect(Event<SomeEvent>.Name, "state1", "state1.1")
                .StateEntry("state1.1")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state1.1", currentState);
        }

        [TestMethod]
        public async Task NestedToParentExternalTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("nested-to-parent-external", "x"), out var sm))
            {
                await sm.SendAsync(new Initialize());
                
                await sm.SendAsync(new SomeEvent());
                
                status = (await sm.SendAsync(new OtherEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.GetAllNodes_ChildrenFirst().First().Value;
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .StateEntry("state1.1")
                .TransitionGuard(Event<OtherEvent>.Name, "state1.1", "state1")
                .StateExit("state1.1")
                .StateExit("state1")
                .TransitionEffect(Event<OtherEvent>.Name, "state1.1", "state1")
                .StateEntry("state1")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task NestedToParentLocalTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("nested-to-parent-local", "x"), out var sm))
            {
                await sm.SendAsync(new Initialize());
                
                await sm.SendAsync(new SomeEvent());
                
                status = (await sm.SendAsync(new OtherEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.GetAllNodes_ChildrenFirst().First().Value;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                .StateEntry("state1.1")
                .TransitionGuard(Event<OtherEvent>.Name, "state1.1", "state1")
                .StateExit("state1.1")
                .TransitionEffect(Event<OtherEvent>.Name, "state1.1", "state1")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task NestedToNestedTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("nested-to-nested", "x"), out var sm))
            {
                status = (await sm.SendAsync(new Initialize())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.GetAllNodes_ChildrenFirst().First().Value;
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .StateInitialize("state1")
                .StateEntry("state1.1")
                .TransitionGuard(Event<Completion>.Name, "state1.1", "state1.2")
                .StateExit("state1.1")
                .TransitionEffect(Event<Completion>.Name, "state1.1", "state1.2")
                .StateEntry("state1.2")
                .TransitionGuard(Event<Completion>.Name, "state1.2", "state2.1")
                .StateExit("state1.2")
                .StateExit("state1")
                .TransitionEffect(Event<Completion>.Name, "state1.2", "state2.1")
                .StateEntry("state2")
                .StateEntry("state2.1")
                .StateEntry("state2.1.2")
            );

            Assert.AreEqual(EventStatus.Initialized, status);
            Assert.AreEqual("state2.1.2", currentState);
        }

        [TestMethod]
        public async Task NestedToParentTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("nested-to-parent", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.GetAllNodes_ChildrenFirst().First().Value;
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .StateInitialize("state1")
                .StateEntry("state1.1")
                .StateInitialize("state1.1")
                .StateEntry("state1.1.1")
                .TransitionGuard(Event<SomeEvent>.Name, "state1.1.1", "state1.1.2")
                .StateExit("state1.1.1")
                .TransitionEffect(Event<SomeEvent>.Name, "state1.1.1", "state1.1.2")
                .StateEntry("state1.1.2")
                .TransitionGuard(Event<Completion>.Name, "state1.1.2", "state1.1")
                .StateExit("state1.1.2")
                .TransitionEffect(Event<Completion>.Name, "state1.1.2", "state1.1")
                .StateEntry("state1.1.1")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state1.1.1", currentState);
        }
    }
}