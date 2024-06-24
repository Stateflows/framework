using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Concurrency : StateflowsTestClass
    {
        public bool eventConsumed = false;

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
                    .AddStateMachine("a", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("a_state1", b => b
                            .AddTransition<SomeEvent>("a_state2", b => b
                                .AddEffect(async c => await Task.Delay(100))
                            )
                        )
                        .AddState("a_state2")
                    )

                    .AddStateMachine("b", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("b_state1", b => b
                            .AddTransition<SomeEvent>("b_state2")
                        )
                        .AddState("b_state2")
                    )

                    .AddStateMachine("instance", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddTransition<SomeEvent>("state2", b => b
                                .AddGuard(async c => c.Event.DelaySize > 0)
                                .AddEffect(async c => await Task.Delay(c.Event.DelaySize))
                            )
                            .AddElseTransition<SomeEvent>("state3")
                        )
                        .AddState("state2")
                        .AddState("state3")
                    )
                )
                ;
        }

        [TestMethod]
        public async Task TwoConcurrentBehaviors()
        {
            var initialized = false;
            string currentState1 = "";
            var someStatus1 = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("a", "x"), out var a) &&
                StateMachineLocator.TryLocateStateMachine(new StateMachineId("b", "x"), out var b))
            {
                _ = await a.InitializeAsync();
                _ = await b.InitializeAsync();
                await Task.WhenAll(
                    a.SendAsync(new SomeEvent()),
                    b.SendAsync(new SomeEvent())
                );
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("a_state1")
                .StateEntry("b_state1")
                .StateEntry("b_state2")
                .StateEntry("a_state2")
            );
        }

        [TestMethod]
        public async Task TwoConcurrentInstances()
        {
            var initialized = false;
            string currentState1 = "";
            var someStatus1 = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("instance", "a"), out var a) &&
                StateMachineLocator.TryLocateStateMachine(new StateMachineId("instance", "b"), out var b))
            {
                _ = await a.InitializeAsync();
                _ = await b.InitializeAsync();
                await Task.WhenAll(
                    a.SendAsync(new SomeEvent() { DelaySize = 100 }),
                    b.SendAsync(new SomeEvent() { DelaySize = 0 })
                );
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                .StateEntry("state1")
                .StateEntry("state3")
                .StateEntry("state2")
            );
        }
    }
}