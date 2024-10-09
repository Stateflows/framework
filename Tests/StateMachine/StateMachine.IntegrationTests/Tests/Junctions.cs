using Stateflows.Common;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Junctions : StateflowsTestClass
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

                    .AddStateMachine("simple", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<OtherEvent, Junction>()
                        )
                        .AddJunction(b => b
                            .AddTransition("state2", b => b
                                .AddGuard(c =>
                                    c.Event.GetType().GetEventName() == Event<CompletionEvent>.Name
                                    && (c.ExecutionTrigger as OtherEvent)?.AnswerToLifeUniverseAndEverything == 42
                                )
                            )
                            .AddElseTransition("state3")
                        )
                        .AddState("state2")
                        .AddState("state3")
                    )

                    .AddStateMachine("dynamic", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<OtherEvent, Junction>(b => b
                                .AddEffect(async c => c.StateMachine.Values.Set("answer", c.Event.AnswerToLifeUniverseAndEverything))
                            )
                        )
                        .AddJunction(b => b
                            .AddTransition("state2", b => b
                                .AddGuard(c => c.StateMachine.Values.GetOrDefault<int>("answer") == 42)
                            )
                            .AddElseTransition("state3")
                        )
                        .AddState("state2")
                        .AddState("state3")
                    )
                )
                ;
        }

        [TestMethod]
        public async Task SimpleJunction()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 43 })).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.First();
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state3", currentState);
        }

        [TestMethod]
        public async Task DynamicJunctionFail()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("dynamic", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 42 })).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.First();
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state3", currentState);
        }
    }
}