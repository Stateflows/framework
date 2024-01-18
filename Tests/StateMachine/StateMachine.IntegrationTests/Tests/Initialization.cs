using Stateflows.Common;
using Stateflows.Common.Data;
using Stateflows.StateMachines.Sync;
using Stateflows.StateMachines.Data;
using Stateflows.StateMachines.Sync.Data;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    public class ValueInitializationRequest : InitializationRequest
    {
        public string Value { get; set; } = String.Empty;
    }

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
                .AddStateMachines(b => b
                    .AddStateMachine("simple", b => b
                        .AddInitialState("state1", b => b
                            .AddOnEntry(c => StateEntered = true)
                        )
                    )

                    .AddStateMachine("value", b => b
                        .AddOnInitialize<ValueInitializationRequest>(c =>
                        {
                            c.StateMachine.Values.Set<string>("foo", c.InitializationRequest.Value);

                            return true;
                        })
                        .AddInitialState("state1", b => b
                            .AddOnEntry(c =>
                            {
                                if (c.StateMachine.Values.TryGet<string>("foo", out var v))
                                {
                                    Value = v;
                                }
                            })
                        )
                    )

                    .AddStateMachine("invalid", b => b
                        .AddOnInitialize<ValueInitializationRequest>(c => c.InitializationRequest.Value != null)
                        .AddInitialState("state1")
                    )

                    .AddStateMachine("payload", b => b
                        .AddOnInitialize<string>(c => c.InitializationRequest.Payload != null)
                        .AddInitialState("state1")
                    )

                    .AddStateMachine("failed", b => b
                        .AddOnInitialize(c => throw new Exception("Initialization failed"))
                        .AddInitialState("state1")
                    )

                    .AddStateMachine("completion", b => b
                        .AddInitialState("state1", b => b
                            .AddDefaultTransition("state2")
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("nested-completion", b => b
                        .AddInitialCompositeState("state1", b => b
                            .AddInitialState("state1.1", b => b
                                .AddDefaultTransition("state1.2")
                            )
                            .AddState("state1.2")
                        )
                    )
                )
                ;
        }

        [TestMethod]
        public async Task NoInitialization()
        {
            var status = EventStatus.Rejected;
            string? currentState = "";

            if (Locator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.FirstOrDefault();
            }

            Assert.AreEqual(EventStatus.Rejected, status);
            Assert.IsNull(StateEntered);
            Assert.AreNotEqual("state1", currentState);
        }

        [TestMethod]
        public async Task SimpleInitialization()
        {
            var initialized = false;
            string currentState = string.Empty;

            if (Locator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                initialized = (await sm.InitializeAsync()).Response.InitializationSuccessful;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.FirstOrDefault() ?? string.Empty;
            }

            Assert.IsTrue(initialized);
            Assert.IsTrue(StateEntered);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task DoubleInitialization()
        {
            var initialized = false;
            string currentState = string.Empty;

            if (Locator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                await sm.InitializeAsync();

                initialized = (await sm.InitializeAsync()).Response.InitializationSuccessful;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.First() ?? string.Empty;
            }

            Assert.IsFalse(initialized);
            Assert.IsTrue(StateEntered);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task ValueInitialization()
        {
            var initialized = false;
            string currentState = string.Empty;

            if (Locator.TryLocateStateMachine(new StateMachineId("value", "x"), out var sm))
            {
                initialized = (await sm.InitializeAsync(new ValueInitializationRequest() { Value = "bar" })).Response.InitializationSuccessful;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.First() ?? string.Empty;
            }

            Assert.IsTrue(initialized);
            Assert.AreEqual("bar", Value);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task PayloadInitialization()
        {
            var initialized = false;
            string currentState = string.Empty;

            if (Locator.TryLocateStateMachine(new StateMachineId("payload", "x"), out var sm))
            {
                initialized = (await sm.InitializeAsync("bar")).Response.InitializationSuccessful;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.First() ?? string.Empty;
            }

            Assert.IsTrue(initialized);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task InvalidInitialization()
        {
            var initialized = false;

            if (Locator.TryLocateStateMachine(new StateMachineId("invalid", "x"), out var sm))
            {
                initialized = (await sm.InitializeAsync()).Response.InitializationSuccessful;
            }

            Assert.IsFalse(initialized);
        }

        [TestMethod]
        public async Task FailedInitialization()
        {
            var initialized = false;

            if (Locator.TryLocateStateMachine(new StateMachineId("failed", "x"), out var sm))
            {
                initialized = (await sm.InitializeAsync()).Response.InitializationSuccessful;
            }

            Assert.IsFalse(initialized);
        }

        [TestMethod]
        public async Task InitializationWithCompletion()
        {
            var initialized = false;
            string currentState = string.Empty;

            if (Locator.TryLocateStateMachine(new StateMachineId("completion", "x"), out var sm))
            {
                initialized = (await sm.InitializeAsync()).Response.InitializationSuccessful;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.First() ?? string.Empty;
            }

            Assert.IsTrue(initialized);
            Assert.AreEqual("state2", currentState);
        }

        [TestMethod]
        public async Task InitializationWithNestedCompletion()
        {
            var initialized = false;
            string? currentState = string.Empty;

            if (Locator.TryLocateStateMachine(new StateMachineId("nested-completion", "x"), out var sm))
            {
                initialized = (await sm.InitializeAsync()).Response.InitializationSuccessful;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.Skip(1).First() ?? string.Empty;
            }

            Assert.IsTrue(initialized);
            Assert.AreEqual("state1.2", currentState);
        }
    }
}