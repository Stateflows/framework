using OneOf;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class OneOfTriggers : StateflowsTestClass
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
                .AddOneOf()
                .AddStateMachines(b => b

                    .AddStateMachine("simple", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<OneOf<OtherEvent, SomeEvent>>("state2")
                        )
                        .AddState("state2")
                    )
                )
                ;
        }

        [TestMethod]
        public async Task MultipleTriggers()
        {
            var status1 = EventStatus.Rejected;
            var status2 = EventStatus.Rejected;
            string currentState1 = "state1";
            string currentState2 = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("simple", "1"), out var sm1))
            {
                status1 = (await sm1.SendAsync(new OtherEvent())).Status;

                currentState1 = (await sm1.GetCurrentStateAsync()).Response.StatesStack.First();
            }

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("simple", "2"), out var sm2))
            {
                status2 = (await sm2.SendAsync(new SomeEvent())).Status;

                currentState2 = (await sm2.GetCurrentStateAsync()).Response.StatesStack.First();
            }

            Assert.AreEqual(EventStatus.Consumed, status1);
            Assert.AreEqual(EventStatus.Consumed, status2);
            Assert.AreEqual("state2", currentState1);
            Assert.AreEqual("state2", currentState2);
        }
    }
}