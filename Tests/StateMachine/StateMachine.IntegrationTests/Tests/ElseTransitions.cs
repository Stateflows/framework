using Stateflows.Common;
using Stateflows.StateMachines.Sync;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class ElseTransitions : StateflowsTestClass
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

                    .AddStateMachine("single", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<OtherEvent>("state2", b => b
                                .AddGuard(c => c.Event.AnswerToLifeUniverseAndEverything == 42)
                            )
                            .AddElseTransition<OtherEvent>("state3")
                        )
                        .AddState("state2")
                        .AddState("state3")
                    )

                    .AddStateMachine("multiple", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<OtherEvent>("state2", b => b
                                .AddGuard(c => c.Event.AnswerToLifeUniverseAndEverything == 42)
                            )
                            .AddElseTransition<OtherEvent>("state4")
                            .AddTransition<OtherEvent>("state3", b => b
                                .AddGuard(c => c.Event.AnswerToLifeUniverseAndEverything == 43)
                            )
                        )
                        .AddState("state2")
                        .AddState("state3")
                        .AddState("state4")
                    )
                )
                ;
        }

        [TestMethod]
        public async Task ElseFromOneTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("single", "x"), out var sm))
            {
                //await sm.InitializeAsync();

                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 43 })).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.First();
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state3", currentState);
        }

        [TestMethod]
        public async Task ElseFromTwoTransitions()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("multiple", "x"), out var sm))
            {
                //await sm.InitializeAsync();

                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 43 })).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.First();
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state3", currentState);
        }
    }
}