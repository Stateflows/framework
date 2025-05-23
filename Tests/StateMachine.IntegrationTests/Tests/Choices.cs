using Stateflows.Common;
using Stateflows.StateMachines;
using StateMachine.IntegrationTests.Classes.Events;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Choices : StateflowsTestClass
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
                            .AddTransition<OtherEvent, Choice>()
                        )
                        .AddChoice(b => b
                            .AddTransition("state2", b => b
                                .AddGuard(c => (c.ExecutionTrigger as OtherEvent)?.AnswerToLifeUniverseAndEverything == 42)
                            )
                            .AddElseTransition("state3")
                        )
                        .AddState("state2")
                        .AddState("state3")
                    )

                    .AddStateMachine("dynamic", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<OtherEvent, Choice>(b => b
                                .AddEffect(c => c.Behavior.Values.SetAsync("answer", c.Event.AnswerToLifeUniverseAndEverything))
                            )
                        )
                        .AddChoice(b => b
                            .AddTransition("state2", b => b
                                .AddGuard(async c => await c.Behavior.Values.GetOrDefaultAsync<int>("answer") == 42)
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
        public async Task SimpleChoice()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 43 })).Status;

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state3", currentState);
        }

        [TestMethod]
        public async Task DynamicChoice()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("dynamic", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 43 })).Status;

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state3", currentState);
        }
    }
}