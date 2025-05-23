using Stateflows.Common;
using StateMachine.IntegrationTests.Classes.Events;
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
                .AddStateMachines(b => b
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

                    .AddStateMachine("nested", b => b
                        .AddInitialCompositeState("state1", b => b
                            .AddInitialState("state1.1", b => b
                                .AddDeferredEvent<OtherEvent>()
                                .AddTransition<SomeEvent>("state1.2")
                            )
                            .AddState("state1.2")
                            .AddTransition<OtherEvent>("state2")
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("sequence", b => b
                        .AddInitialState("state1", b => b
                            .AddDeferredEvent<OtherEvent>()
                            .AddTransition<SomeEvent>("state2")
                        )
                        .AddState("state2", b => b
                            .AddInternalTransition<OtherEvent>(b => b
                                .AddEffect(async c =>
                                {
                                    var (success, counter) = await c.Behavior.Values.TryGetAsync<int>("c");
                                    if (success)
                                    {
                                        if (counter == c.Event.AnswerToLifeUniverseAndEverything - 1)
                                        {
                                            await c.Behavior.Values.SetAsync("result", true);
                                        }
                                    }
                                    else
                                    {
                                        await c.Behavior.Values.SetAsync("c", c.Event.AnswerToLifeUniverseAndEverything);
                                    }
                                })
                            )
                            .AddDefaultTransition("state3", b => b
                                .AddGuard(c => c.Behavior.Values.GetOrDefaultAsync("result", false))
                            )
                        )
                        .AddState("state3")
                    )
                )
                ;
        }

        [TestMethod]
        public async Task SimpleDeferral()
        {
            var initialized = false;
            string currentState = "";
            var otherStatus = EventStatus.Rejected;
            var someStatus = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("defer", "x"), out var sm))
            {
                var x = (await sm.SendAsync(new Initialize())).Status;
                initialized = x == EventStatus.Initialized;

                otherStatus = (await sm.SendAsync(new OtherEvent())).Status;

                someStatus = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            Assert.IsTrue(initialized);
            Assert.AreEqual(EventStatus.Deferred, otherStatus);
            Assert.AreEqual(EventStatus.Consumed, someStatus);
            Assert.AreEqual("state3", currentState);
        }

        [TestMethod]
        public async Task SequenceDeferral()
        {
            var initialized = false;
            string currentState = "";
            var otherStatus1 = EventStatus.Rejected;
            var otherStatus2 = EventStatus.Rejected;
            var someStatus = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("sequence", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                otherStatus1 = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 42 })).Status;

                otherStatus2 = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 43 })).Status;

                someStatus = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            Assert.IsTrue(initialized);
            Assert.AreEqual(EventStatus.Deferred, otherStatus1);
            Assert.AreEqual(EventStatus.Deferred, otherStatus2);
            Assert.AreEqual(EventStatus.Consumed, someStatus);
            Assert.AreEqual("state3", currentState);
        }

        [TestMethod]
        public async Task NestedDeferral()
        {
            var initialized = false;
            string currentState = "";
            var otherStatus1 = EventStatus.Rejected;
            var otherStatus2 = EventStatus.Rejected;
            var someStatus = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("nested", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                otherStatus1 = (await sm.SendAsync(new OtherEvent())).Status;

                someStatus = (await sm.SendAsync(new SomeEvent())).Status;

                otherStatus2 = (await sm.SendAsync(new OtherEvent())).Status;

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            Assert.IsTrue(initialized);
            Assert.AreEqual(EventStatus.Deferred, otherStatus1);
            Assert.AreEqual(EventStatus.Consumed, someStatus);
            Assert.AreEqual(EventStatus.NotConsumed, otherStatus2);
            Assert.AreEqual("state2", currentState);
        }
    }
}