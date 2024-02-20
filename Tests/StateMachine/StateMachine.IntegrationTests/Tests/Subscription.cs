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
                            .AddOnEntry(c => c.StateMachine.SubscribeAsync<SomeEvent>(new StateMachineId("subscribee", "x")))
                            .AddTransition<SomeEvent>("state2")
                        )
                        .AddState("state2", b => b
                            .AddInternalTransition<OtherEvent>(b => b
                                .AddEffect(c => c.StateMachine.UnsubscribeAsync<SomeEvent>(new StateMachineId("subscribee", "x")))
                            )
                            .AddTransition<SomeEvent>("state3")
                        )
                        .AddState("state3")
                    )

                    .AddStateMachine("subscribee", b => b
                        .AddInitialState("state1", b => b
                            .AddInternalTransition<OtherEvent>(b => b
                                .AddEffect(c => c.StateMachine.Publish(new SomeEvent()))
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

                //await Task.Delay(100);

                await subscribee.SendAsync(new OtherEvent());

                currentState = (await subscriber.GetCurrentStateAsync()).Response?.StatesStack.FirstOrDefault();
            }

            Assert.AreEqual("state2", currentState);
        }
    }
}