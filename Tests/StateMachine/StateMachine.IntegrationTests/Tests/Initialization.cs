using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Initialization : StateflowsTestClass
    {
        private bool? StateEntered = null;
        private string Value = "boo";

        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddStateMachine("simple", b => b
                    .AddInitialState("state1", b => b
                        .AddOnEntry(c => StateEntered = true)
                    )
                )

                .AddStateMachine("value", b => b
                    .AddInitialState("state1", b => b
                        .AddOnEntry(c =>
                        {
                            if (c.StateMachine.GlobalValues.TryGet<string>("foo", out var v))
                            {
                                Value = v;
                            }
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task NoInitialization()
        {
            var consumed = false;
            string? currentState = "";

            if (Locator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                consumed = await sm.SendAsync(new SomeEvent());

                currentState = (await sm.GetCurrentStateAsync())?.Name;
            }

            Assert.IsFalse(consumed);
            Assert.IsNull(StateEntered);
            Assert.AreNotEqual("state1", currentState);
        }

        [TestMethod]
        public async Task SimpleInitialization()
        {
            var initialized = false;
            string currentState = "";

            if (Locator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                initialized = await sm.InitializeAsync();

                currentState = (await sm.GetCurrentStateAsync()).Name;
            }

            Assert.IsTrue(initialized);
            Assert.IsTrue(StateEntered);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task DoubleInitialization()
        {
            var initialized = false;
            string currentState = "";

            if (Locator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                await sm.InitializeAsync();

                initialized = await sm.InitializeAsync();

                currentState = (await sm.GetCurrentStateAsync()).Name;
            }

            Assert.IsFalse(initialized);
            Assert.IsTrue(StateEntered);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task ValueInitialization()
        {
            var initialized = false;
            string currentState = "";

            if (Locator.TryLocateStateMachine(new StateMachineId("value", "x"), out var sm))
            {
                var values = new Dictionary<string, object>() { { "foo", "bar" } };
                initialized = await sm.InitializeAsync(values);

                currentState = (await sm.GetCurrentStateAsync()).Name;
            }

            Assert.IsTrue(initialized);
            Assert.AreEqual("bar", Value);
            Assert.AreEqual("state1", currentState);
        }
    }
}