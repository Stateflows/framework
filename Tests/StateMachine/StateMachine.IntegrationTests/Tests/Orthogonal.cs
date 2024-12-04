using Stateflows.Common;
using Stateflows.StateMachines.Events;
using StateMachine.IntegrationTests.Utils;
using System.Diagnostics;

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

                var tree = (await sm.GetCurrentStateAsync()).Response.StatesTree;
                currentState = tree.Value;
                substate1 = tree.Root.Items.First().Value;
                substate2 = tree.Root.Items.Last().Value;
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

                var tree = (await sm.GetCurrentStateAsync()).Response.StatesTree;
                currentState = tree.Value;
                substate1 = tree.Root.Items.First().Value;
                substate2 = tree.Root.Items.Last().Value;
            }

            Assert.AreEqual("state2", currentState);
            Assert.AreEqual("state2.1.2", substate1);
            Assert.AreEqual("state2.2.1", substate2);
        }
    }
}