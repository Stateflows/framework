using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;
using StateMachine.IntegrationTests.Classes.StateMachines;

namespace StateMachine.IntegrationTests.Tests
{
    public class ValueInitializationRequest
    {
        public string Value { get; set; } = String.Empty;
    }

    [TestClass]
    public class Initialization : StateflowsTestClass
    {
        private bool? StateEntered = null;
        private bool? InitializerCalled = null;
        private bool InitializationSuccessful = true;
        public static string Value = "boo";

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

                    .AddStateMachine("implicit", b => b
                        .AddInitialState("state1", b => b
                            .AddOnEntry(c => StateEntered = true)
                        )
                    )

                    .AddStateMachine("explicit", b => b
                        .AddInitializer<SomeEvent>(async c =>
                        {
                            InitializerCalled = true;
                            return c.InitializationEvent.InitializationSuccessful;
                        })
                        .AddInitialState("state1", b => b
                            .AddOnEntry(c => StateEntered = true)
                        )
                    )

                    .AddStateMachine("default", b => b
                        .AddDefaultInitializer(async c =>
                        {
                            InitializerCalled = true;
                            return InitializationSuccessful;
                        })
                        .AddInitialState("state1", b => b
                            .AddOnEntry(c => StateEntered = true)
                        )
                    )

                    .AddStateMachine("consumption", b => b
                        .AddInitializer<SomeEvent>(async c => true)
                        .AddInitialState("state1", b => b
                            .AddTransition<SomeEvent>("state2")
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("initialize-consumption", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<Initialize>("state2")
                        )
                        .AddState("state2")
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

                    .AddStateMachine<TypedValue>()
                )
                ;
        }

        [TestMethod]
        public async Task ImplicitInitialization()
        {
            string currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("implicit", "x"), out var sm))
            {
                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value ?? string.Empty;
            }

            Assert.IsTrue(StateEntered);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task ExplicitInitializationWithInitialize()
        {
            EventStatus status = EventStatus.Undelivered;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("implicit", "x"), out var sm))
            {
                status = (await sm.SendAsync(new Initialize())).Status;
            }

            Assert.IsTrue(StateEntered);
            Assert.AreEqual(EventStatus.Initialized, status);
        }

        [TestMethod]
        public async Task ExplicitDefaultInitializationWithInitialize()
        {
            EventStatus status = EventStatus.Undelivered;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("default", "x"), out var sm))
            {
                status = (await sm.SendAsync(new Initialize())).Status;
            }

            Assert.IsTrue(StateEntered);
            Assert.AreEqual(EventStatus.Initialized, status);
        }

        [TestMethod]
        public async Task ExplicitInitializationOK()
        {
            var status = EventStatus.Rejected;
            string currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("explicit", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent() { InitializationSuccessful = true })).Status;
                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value ?? string.Empty;
            }

            Assert.IsTrue(InitializerCalled);
            Assert.AreEqual(EventStatus.Initialized, status);
            Assert.IsTrue(StateEntered);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task ExplicitInitializationFailed()
        {
            var status = EventStatus.Rejected;
            string currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("explicit", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent() { InitializationSuccessful = false })).Status;
                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value ?? string.Empty;
            }

            Assert.IsTrue(InitializerCalled);
            Assert.AreEqual(EventStatus.NotInitialized, status);
            Assert.AreEqual(string.Empty, currentState);
        }

        [TestMethod]
        public async Task ConsumptionInitializeOnly()
        {
            var status = EventStatus.Rejected;
            string currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("consumption", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent() { InitializationSuccessful = true })).Status;
                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value ?? string.Empty;
            }

            Assert.AreEqual(EventStatus.Initialized, status);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task ConsumptionTransition()
        {
            var status1 = EventStatus.Rejected;
            var status2 = EventStatus.Rejected;
            string currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("consumption", "x"), out var sm))
            {
                status1 = (await sm.SendAsync(new SomeEvent() { InitializationSuccessful = true })).Status;
                status2 = (await sm.SendAsync(new SomeEvent() { InitializationSuccessful = true })).Status;
                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value ?? string.Empty;
            }

            Assert.AreEqual(EventStatus.Initialized, status1);
            Assert.AreEqual(EventStatus.Consumed, status2);
            Assert.AreEqual("state2", currentState);
        }

        [TestMethod]
        public async Task InitializeConsumptionInitializeOnly()
        {
            var status = EventStatus.Rejected;
            string currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("initialize-consumption", "x"), out var sm))
            {
                status = (await sm.SendAsync(new Initialize())).Status;
                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value ?? string.Empty;
            }

            Assert.AreEqual(EventStatus.Initialized, status);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task InitializeConsumptionTransition()
        {
            var status1 = EventStatus.Rejected;
            var status2 = EventStatus.Rejected;
            string currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("initialize-consumption", "x"), out var sm))
            {
                status1 = (await sm.SendAsync(new Initialize())).Status;
                status2 = (await sm.SendAsync(new Initialize())).Status;
                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value ?? string.Empty;
            }

            Assert.AreEqual(EventStatus.Initialized, status1);
            Assert.AreEqual(EventStatus.NotInitialized, status2);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task ImplicitDefaultInitializationOK()
        {
            string currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("default", "x"), out var sm))
            {
                var result = await sm.GetCurrentStateAsync();
                currentState = (result).Response.StatesTree.Value ?? string.Empty;
            }

            Assert.IsTrue(InitializerCalled);
            Assert.IsTrue(StateEntered);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task DefaultInitializationFailed()
        {
            string currentState = string.Empty;
            InitializationSuccessful = false;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("default", "x"), out var sm))
            {
                var result = await sm.GetCurrentStateAsync();
                currentState = (result).Response.StatesTree.Value ?? string.Empty;
            }

            Assert.IsTrue(InitializerCalled);
            Assert.AreEqual(string.Empty, currentState);
        }

        [TestMethod]
        public async Task InitializationWithCompletion()
        {
            var initialized = false;
            string currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("completion", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value ?? string.Empty;
            }

            Assert.IsTrue(initialized);
            Assert.AreEqual("state2", currentState);
        }

        [TestMethod]
        public async Task InitializationWithNestedCompletion()
        {
            var initialized = false;
            string? currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("nested-completion", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Root.Nodes.First().Value ?? string.Empty;
            }

            Assert.IsTrue(initialized);
            Assert.AreEqual("state1.2", currentState);
        }
    }
}