using Stateflows.Common;
using StateMachine.IntegrationTests.Classes.Events;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Orthogonal : StateflowsTestClass
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
                .AddStateMachines(b => b
                        
                    .AddStateMachine("fork", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddTransition<SomeEvent>(Fork.Name)
                        )
                        .AddFork(b => b
                            .AddTransition("stateA")
                            .AddTransition("stateB")
                        )
                        .AddOrthogonalState("orthogonal", b => b
                            .AddRegion(b => b
                                .AddState("stateA")
                            )
                            .AddRegion(b => b
                                .AddState("stateB")
                            )
                        )
                    )
                        
                    .AddStateMachine("join", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddTransition<SomeEvent>(Fork.Name)
                        )
                        .AddFork(b => b
                            .AddTransition("stateA")
                            .AddTransition("stateB")
                        )
                        .AddOrthogonalState("orthogonal", b => b
                            .AddRegion(b => b
                                .AddState("stateA", b => b
                                    .AddDefaultTransition(Join.Name)
                                )
                            )
                            .AddRegion(b => b
                                .AddState("stateB", b => b
                                    .AddDefaultTransition(Join.Name)
                                )
                            )
                        )
                        .AddJoin(b => b
                            .AddTransition("final")
                        )
                        .AddFinalState("final")
                    )
                        
                    .AddStateMachine("not-joined", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddTransition<SomeEvent, Fork>()
                        )
                        .AddFork(b => b
                            .AddTransition("stateA")
                            .AddTransition("stateB")
                        )
                        .AddOrthogonalState("orthogonal", b => b
                            .AddRegion(b => b
                                .AddState("stateA", b => b
                                    .AddDefaultTransition<Join>()
                                )
                            )
                            .AddRegion(b => b
                                .AddState("stateB", b => b
                                    .AddDefaultTransition<Join>()
                                )
                            )
                            .AddRegion(b => b
                                .AddState("stateC", b => b
                                    .AddDefaultTransition<Join>()
                                )
                            )
                        )
                        .AddJoin(b => b
                            .AddTransition("final")
                        )
                        .AddFinalState("final")
                    )
                        
                    .AddStateMachine("nonExitedRegion", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddTransition<SomeEvent>(Fork.Name)
                        )
                        .AddFork(b => b
                            .AddTransition("stateA")
                            .AddTransition("stateB")
                        )
                        .AddOrthogonalState("orthogonal", b => b
                            .AddRegion(b => b
                                .AddState("stateA")
                            )
                            .AddRegion(b => b
                                .AddState("stateB", b => b
                                    .AddDefaultTransition("final")
                                )
                            )
                        )
                        .AddFinalState("final")
                    )

                    .AddStateMachine("defaultTransition", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<OtherEvent>("state2")
                        )
                        .AddOrthogonalState("state2", b => b
                            .AddRegion(b => b
                                .AddInitialState("state2.1.1", b => b
                                    .AddDefaultTransition("state2.1.2")
                                )
                                .AddState("state2.1.2")
                            )
                            .AddRegion(b => b
                                .AddInitialState("state2.2.1")
                            )
                        )
                    )

                    .AddStateMachine("defaultTransitions", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<OtherEvent>("state2")
                        )
                        .AddOrthogonalState("state2", b => b
                            .AddRegion(b => b
                                .AddInitialState("state2.1.1", b => b
                                    .AddDefaultTransition("state2.1.2")
                                )
                                .AddState("state2.1.2")
                            )
                            .AddRegion(b => b
                                .AddInitialState("state2.2.1", b => b
                                    .AddDefaultTransition("state2.2.2")
                                )
                                .AddState("state2.2.2")
                            )
                        )
                    )

                    .AddStateMachine("outsideTransition", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<OtherEvent>("state2")
                        )
                        .AddOrthogonalState("state2", b => b
                            .AddRegion(b => b
                                .AddInitialState("stateA", b => b
                                    .AddDefaultTransition("state3")
                                )
                            )
                            .AddRegion(b => b
                                .AddInitialState("stateB", b => b
                                    .AddDefaultTransition("stateB-final")
                                )
                                .AddState("stateB-final")
                            )
                        )
                        .AddState("state3")
                    )

                    .AddStateMachine("simple", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<OtherEvent>("state2")
                        )
                        .AddOrthogonalState("state2", b => b
                            .AddRegion(b => b
                                .AddInitialState("state2.1.1")
                            )
                            .AddRegion(b => b
                                .AddInitialState("state2.2.1")
                            )
                        )
                    )

                    .AddStateMachine("parallelTransitions", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<OtherEvent>("state2")
                        )
                        .AddOrthogonalState("state2", b => b
                            .AddRegion(b => b
                                .AddInitialState("state2.1.1", b => b
                                    .AddTransition<SomeEvent>("state2.1.2")
                                )
                                .AddState("state2.1.2")
                            )
                            .AddRegion(b => b
                                .AddInitialState("state2.2.1", b => b
                                    .AddTransition<SomeEvent>("state2.2.2")
                                )
                                .AddState("state2.2.2")
                            )
                        )
                    )

                    .AddStateMachine("parallelMultilevelTransitions", b => b
                        .AddInitialOrthogonalState("state1", b => b
                            .AddRegion(b => b
                                .AddInitialState("state1.A.1", b => b
                                    .AddDefaultTransition("state1.A.2")
                                )
                                .AddOrthogonalState("state1.A.2", b => b
                                    .AddRegion(b => b
                                        .AddInitialState("state1.A.2.A.1", b => b
                                            .AddDefaultTransition("state1.A.2.A.2")
                                        )
                                        .AddState("state1.A.2.A.2", b => b
                                            .AddTransition<SomeEvent>("state1.A.2.A.3")
                                        )
                                        .AddState("state1.A.2.A.3")
                                    )
                                    .AddRegion(b => b
                                        .AddInitialState("state1.A.2.B.1", b => b
                                            .AddDefaultTransition("state1.A.2.B.2")
                                        )
                                        .AddState("state1.A.2.B.2", b => b
                                            .AddTransition<SomeEvent>("state1.A.2.B.3")
                                        )
                                        .AddState("state1.A.2.B.3")
                                    )
                                )
                            )
                            .AddRegion(b => b
                                .AddInitialState("state1.B.1", b => b
                                    .AddDefaultTransition("state1.B.2")
                                )
                                .AddState("state1.B.2", b => b
                                    .AddTransition<SomeEvent>("state1.B.3")
                                )
                                .AddState("state1.B.3")
                            )
                        )
                    )

                    .AddStateMachine("transitionToRegion", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<OtherEvent>("state2.1.3")
                        )
                        .AddOrthogonalState("state2", b => b
                            .AddRegion(b => b
                                .AddInitialState("state2.1.1", b => b
                                    .AddDefaultTransition("state2.1.2")
                                )
                                .AddState("state2.1.2", b => b
                                    .AddTransition<SomeEvent>("state2.1.3")
                                )
                                .AddCompositeState("state2.1.3", b => b
                                    .AddInitialState("state2.1.3.1", b => b
                                        .AddTransition<SomeEvent>("state2.1.3.2")
                                    )
                                    .AddState("state2.1.3.2")
                                )
                            )
                            .AddRegion(b => b
                                .AddInitialState("state2.2.1")
                            )
                        )
                    )
                )
                ;
        }

        [TestMethod]
        public async Task SimpleOrthogonalState()
        {
            var status = EventStatus.Rejected;
            string currentState = "";
            string substate1 = "";
            string substate2 = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 42 })).Status;

                var tree = (await sm.GetStatusAsync()).Response.CurrentStates;
                currentState = tree.Value;
                substate1 = tree.Root.Nodes.First().Value;
                substate2 = tree.Root.Nodes.Last().Value;
            }

            Assert.AreEqual("state2", currentState);
            Assert.AreEqual("state2.1.1", substate1);
            Assert.AreEqual("state2.2.1", substate2);
        }

        [TestMethod]
        public async Task SimpleOrthogonalStateWithDefaultTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "";
            string substate1 = "";
            string substate2 = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("defaultTransition", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 42 })).Status;

                var tree = (await sm.GetStatusAsync()).Response.CurrentStates;
                currentState = tree.Value;
                substate1 = tree.Root.Nodes.First().Value;
                substate2 = tree.Root.Nodes.Last().Value;
            }

            Assert.AreEqual("state2", currentState);
            Assert.AreEqual("state2.1.2", substate1);
            Assert.AreEqual("state2.2.1", substate2);
        }

        [TestMethod]
        public async Task SimpleOrthogonalStateWithOutsideTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("outsideTransition", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 42 })).Status;

                var tree = (await sm.GetStatusAsync()).Response.CurrentStates;
                currentState = tree.Value;
            }

            Assert.AreEqual("state3", currentState);
        }

        [TestMethod]
        public async Task SimpleOrthogonalStateWithTransitionToRegion()
        {
            var status = EventStatus.Rejected;
            string currentState = "";
            string substate1 = "";
            string substate2 = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("transitionToRegion", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 42 })).Status;

                var tree = (await sm.GetStatusAsync()).Response.CurrentStates;
                currentState = tree.Value;
                substate1 = tree.Root.Nodes.First().Value;
                substate2 = tree.Root.Nodes.Last().Value;
            }

            Assert.AreEqual("state2", currentState);
            Assert.AreEqual("state2.1.3", substate1);
            Assert.AreEqual("state2.2.1", substate2);
        }

        [TestMethod]
        public async Task SimpleOrthogonalStateWithDefaultTransitions()
        {
            var status = EventStatus.Rejected;
            string currentState = "";
            string substate1 = "";
            string substate2 = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("defaultTransitions", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 42 })).Status;

                var tree = (await sm.GetStatusAsync()).Response.CurrentStates;
                currentState = tree.Value;
                substate1 = tree.Root.Nodes.First().Value;
                substate2 = tree.Root.Nodes.Last().Value;
            }

            Assert.AreEqual("state2", currentState);
            Assert.AreEqual("state2.1.2", substate1);
            Assert.AreEqual("state2.2.2", substate2);
        }

        [TestMethod]
        public async Task SimpleOrthogonalStateWithParallelTransitions()
        {
            var status = EventStatus.Rejected;
            string currentState = "";
            string substate1 = "";
            string substate2 = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("parallelTransitions", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 42 })).Status;
                status = (await sm.SendAsync(new SomeEvent())).Status;

                var tree = (await sm.GetStatusAsync()).Response.CurrentStates;
                currentState = tree.Value;
                substate1 = tree.Root.Nodes.First().Value;
                substate2 = tree.Root.Nodes.Last().Value;
            }

            Assert.AreEqual("state2", currentState);
            Assert.AreEqual("state2.1.2", substate1);
            Assert.AreEqual("state2.2.2", substate2);
        }

        [TestMethod]
        public async Task MultilevelOrthogonalStateWithParallelTransitions()
        {
            var status = EventStatus.Rejected;
            string currentState = "";
            string substate1 = "";
            string substate2 = "";
            string substate3 = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("parallelMultilevelTransitions", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                var tree = (await sm.GetStatusAsync()).Response.CurrentStates;
                currentState = tree.Value;
                var allNodes = tree.GetAllNodes_FromTheTop().ToArray();
                substate1 = allNodes.Skip(0).Take(1).First().Value;
                substate2 = allNodes.Skip(1).Take(1).First().Value;
                substate3 = allNodes.Skip(2).Take(1).First().Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state1", currentState);
            Assert.AreEqual("state1.A.2.A.3", substate1);
            Assert.AreEqual("state1.A.2.B.3", substate2);
            Assert.AreEqual("state1.A.2", substate3);
        }

        [TestMethod]
        public async Task PlainFork()
        {
            var status = EventStatus.Rejected;
            string currentState = "";
            string substate1 = "";
            string substate2 = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("fork", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                var tree = (await sm.GetStatusAsync()).Response.CurrentStates;
                currentState = tree.Value;
                substate1 = tree.Root.Nodes.First().Value;
                substate2 = tree.Root.Nodes.Last().Value;
            }
            
            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .StateExit("state1")
                .TransitionEffect(Event<SomeEvent>.Name, "state1", Fork.Name)
                .StateEntry(Fork.Name)
                .StateExit(Fork.Name)
                .DefaultTransitionEffect(Fork.Name, "stateA")
                .StateEntry("orthogonal")
                .StateInitialize("orthogonal")
                .StateEntry("stateA")
                .DefaultTransitionEffect(Fork.Name, "stateB")
                .StateEntry("stateB")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("orthogonal", currentState);
            Assert.AreEqual("stateA", substate1);
            Assert.AreEqual("stateB", substate2);
        }

        [TestMethod]
        public async Task PlainJoin()
        {
            var status = EventStatus.Rejected;
            string currentState = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("join", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                var tree = (await sm.GetStatusAsync()).Response.CurrentStates;
                currentState = tree.Value;
            }
            
            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .StateExit("state1")
                .TransitionEffect(Event<SomeEvent>.Name, "state1", Fork.Name)
                .StateEntry(Fork.Name)
                .StateExit(Fork.Name)
                .DefaultTransitionEffect(Fork.Name, "stateA")
                .StateEntry("orthogonal")
                .StateInitialize("orthogonal")
                .StateEntry("stateA")
                .DefaultTransitionEffect(Fork.Name, "stateB")
                .StateEntry("stateB")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("final", currentState);
        }

        [TestMethod]
        public async Task NotJoined()
        {
            var status = EventStatus.Rejected;
            string currentState = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("not-joined", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                var tree = (await sm.GetStatusAsync()).Response.CurrentStates;
                currentState = tree.Value;
            }
            
            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .StateExit("state1")
                .TransitionEffect(Event<SomeEvent>.Name, "state1", Fork.Name)
                .StateEntry(Fork.Name)
                .StateExit(Fork.Name)
                .DefaultTransitionEffect(Fork.Name, "stateA")
                .StateEntry("orthogonal")
                .StateInitialize("orthogonal")
                .StateEntry("stateA")
                .DefaultTransitionEffect(Fork.Name, "stateB")
                .StateEntry("stateB")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("orthogonal", currentState);
        }

        [TestMethod]
        public async Task NonExitedRegionSuccessfulExit()
        {
            var status = EventStatus.Rejected;
            string currentState = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("nonExitedRegion", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                var tree = (await sm.GetStatusAsync()).Response.CurrentStates;
                currentState = tree.Value;
            }
            
            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("state1")
                .StateExit("state1")
                .TransitionEffect(Event<SomeEvent>.Name, "state1", Fork.Name)
                .StateEntry(Fork.Name)
                .StateExit(Fork.Name)
                .DefaultTransitionEffect(Fork.Name, "stateA")
                .StateEntry("orthogonal")
                .StateInitialize("orthogonal")
                .StateEntry("stateA")
                .DefaultTransitionEffect(Fork.Name, "stateB")
                .StateEntry("stateB")
                .StateExit("stateB")
                .StateExit("stateA")
                .StateExit("orthogonal")
                .DefaultTransitionEffect("stateB", "final")
                .StateEntry("final")
            );

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("final", currentState);
        }
    }
}