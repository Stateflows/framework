using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Deferral : StateflowsTestClass
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
                .AddStateMachine("defer", b => b
                    .AddInitialState("state1", b => b
                        .AddDeferredEvent<OtherEvent>()
                        .AddTransition<SomeEvent>("state2")
                    )
                    .AddState("state2", b => b
                        .AddTransition<OtherEvent>("state3")
                    )
                    .AddState("state3")
                )
                ;
        }

        [TestMethod]
        public async Task SimpleDeferral()
        {
            var initialized = false;
            string currentState = "";
            var otherConsumed = false;
            var someConsumed = false;

            if (Locator.TryLocateStateMachine(new StateMachineId("defer", "x"), out var sm))
            {
                initialized = await sm.InitializeAsync();

                otherConsumed = await sm.SendAsync(new OtherEvent());

                someConsumed = await sm.SendAsync(new SomeEvent());

                currentState = (await sm.GetCurrentStateAsync()).Name;
            }

            Assert.IsTrue(initialized);
            Assert.IsTrue(someConsumed);
            Assert.IsFalse(otherConsumed);
            Assert.AreEqual("state3", currentState);
        }
    }
}