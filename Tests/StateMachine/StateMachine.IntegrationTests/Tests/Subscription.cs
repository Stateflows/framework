using StateMachine.IntegrationTests.Utils;
using Stateflows.StateMachines.Sync;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Subscription : StateflowsTestClass
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
                    .AddStateMachine("subscriber", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddOnEntry(c => c.StateMachine.SubscribeAsync<SomeNotification>(new StateMachineId("subscribee", "x")))
                            .AddTransition<SomeNotification>("state2")
                        )
                        .AddState("state2", b => b
                            .AddInternalTransition<OtherEvent>(b => b
                                .AddEffect(c => c.StateMachine.UnsubscribeAsync<SomeNotification>(new StateMachineId("subscribee", "x")))
                            )
                            .AddTransition<SomeNotification>("state3")
                        )
                        .AddState("state3")
                    )

                    .AddStateMachine("subscribee", b => b
                        .AddInitialState("state1", b => b
                            .AddInternalTransition<OtherEvent>(b => b
                                .AddEffect(c => c.StateMachine.Publish(new SomeNotification()))
                            )
                        )
                    )
                )
                ;
        }

        [TestMethod]
        public async Task SubscribingSuccessful()
        {
            string? currentState = "";

            if (
                Locator.TryLocateStateMachine(new StateMachineId("subscriber", "x"), out var subscriber) &&
                Locator.TryLocateStateMachine(new StateMachineId("subscribee", "x"), out var subscribee)
            )
            {
                await subscriber.InitializeAsync();

                await subscribee.InitializeAsync();

                await subscribee.SendAsync(new OtherEvent());

                await subscriber.SendAsync(new OtherEvent());

                await subscribee.SendAsync(new OtherEvent());

                currentState = (await subscriber.GetCurrentStateAsync()).Response?.StatesStack.FirstOrDefault();
            }

            Assert.AreEqual("state2", currentState);
        }


        [TestMethod]
        public async Task WatchSuccessful()
        {
            var watchHit = false;

            if (Locator.TryLocateStateMachine(new StateMachineId("subscribee", "x"), out var subscribee))
            {
                _ = subscribee.WatchAsync<SomeNotification>(n =>
                {
                    lock (Locator)
                    {
                        watchHit = true;
                    }
                });

                await subscribee.InitializeAsync();

                await subscribee.SendAsync(new OtherEvent());

                await subscribee.GetCurrentStateAsync();
            }

            lock (Locator)
            {
                Assert.AreEqual(true, watchHit);
            }
        }
    }
}