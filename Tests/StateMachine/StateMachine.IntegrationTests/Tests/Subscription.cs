using StateMachine.IntegrationTests.Utils;
using Stateflows.Common;
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
                            //.AddDeferredEvent<OtherEvent>()
                            .AddOnEntry(c => c.StateMachine.SubscribeAsync<SomeNotification>(new StateMachineId("subscribee", c.StateMachine.Id.Instance)))
                            .AddTransition<SomeNotification>("state2")
                        )
                        .AddState("state2", b => b
                            .AddInternalTransition<OtherEvent>(b => b
                                .AddEffect(c => c.StateMachine.UnsubscribeAsync<SomeNotification>(new StateMachineId("subscribee", c.StateMachine.Id.Instance)))
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
                StateMachineLocator.TryLocateStateMachine(new StateMachineId("subscriber", "x"), out var subscriber) &&
                StateMachineLocator.TryLocateStateMachine(new StateMachineId("subscribee", "x"), out var subscribee)
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

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("subscribee", "y"), out var subscribee))
            {
                _ = subscribee.WatchAsync<SomeNotification>(n =>
                {
                    lock (StateMachineLocator)
                    {
                        watchHit = true;
                    }
                });

                await subscribee.InitializeAsync();

                await subscribee.SendAsync(new OtherEvent());

                await subscribee.GetCurrentStateAsync();
            }

            lock (StateMachineLocator)
            {
                Assert.AreEqual(true, watchHit);
            }
        }

        [TestMethod]
        public async Task WatchStandardNotifications()
        {
            var currentStatus = BehaviorStatus.Unknown;
            var currentState = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("subscribee", "z"), out var subscribee))
            {
                _ = subscribee.WatchCurrentStateAsync(n => currentState = n.StatesStack.First());

                _ = subscribee.WatchStatusAsync(n => currentStatus = n.BehaviorStatus);

                await subscribee.InitializeAsync();

                await subscribee.GetCurrentStateAsync();
            }

            Assert.AreEqual("state1", currentState);
            Assert.AreEqual(BehaviorStatus.Initialized, currentStatus);
        }
    }
}