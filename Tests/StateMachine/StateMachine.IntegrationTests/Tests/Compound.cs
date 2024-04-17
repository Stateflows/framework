using Stateflows.Common;
using Stateflows.Common.Extensions;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Compound : StateflowsTestClass
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
                            .AddTransition<SomeEvent>("state2")
                        )
                        .AddState("state2", b => b
                            .AddTransition<OtherEvent>("state3")
                        )
                        .AddState("state3")
                    )
                )
                ;
        }

        [TestMethod]
        public async Task CompoundRequestOK()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                await sm.InitializeAsync();

                var request = new CompoundRequest()
                {
                    Events = new List<Event>()
                    {
                        new SomeEvent(),
                        new OtherEvent(),
                    }
                };

                status = (await sm.SendAsync(request)).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.First();
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state3", currentState);
        }

        [TestMethod]
        public async Task CompoundRequestNOK()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                await sm.InitializeAsync();

                var request = new CompoundRequest()
                {
                    Events = new List<Event>()
                    {
                        new OtherEvent(),
                        new SomeEvent(),
                    }
                };

                var result = await sm.SendAsync(request);
                status = result.Status;
                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.First();
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreNotEqual("state3", currentState);
        }

        [TestMethod]
        public async Task CompoundRequestInvalid()
        {
            var status = EventStatus.Rejected;
            SendResult? result = null;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                await sm.InitializeAsync();

                var request = new CompoundRequest()
                {
                    Events = new List<Event>()
                    {
                        new SomeEvent(),
                        new OtherEvent() { RequiredParameter = "" },
                    }
                };

                result = await sm.SendAsync(request);
                status = result.Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.First();
            }

            var response = result?.Event.GetResponse() as CompoundResponse;

            Assert.AreEqual(EventStatus.Invalid, status);
            Assert.AreEqual("state1", currentState);
            Assert.AreNotEqual(null, response);
            Assert.AreEqual(true, response?.Results.First().Validation.IsValid);
            Assert.AreEqual(false, response?.Results.Last().Validation.IsValid);
        }
    }
}