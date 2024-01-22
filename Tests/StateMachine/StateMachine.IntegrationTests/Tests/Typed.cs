using Stateflows.Common;
using Stateflows.StateMachines.Sync;
using Stateflows.StateMachines.Typed;
using StateMachine.IntegrationTests.Classes.StateMachines;
using StateMachine.IntegrationTests.Classes.States;
using StateMachine.IntegrationTests.Classes.Transitions;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Typed : StateflowsTestClass
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
                    .AddStateMachine<StateMachine1>()
                )
                ;
        }

        [TestMethod]
        public async Task TypedStateMachine()
        {
            var status1 = EventStatus.Rejected;
            var status2 = EventStatus.Rejected;
            string currentState = StateInfo<State1>.Name;

            StateMachine1.Reset();
            State1.Reset();
            State2.Reset();
            SomeEventTransition.Reset();

            if (Locator.TryLocateStateMachine(new StateMachineId(StateMachineInfo<StateMachine1>.Name, "x"), out var sm))
            {
                await sm.InitializeAsync();

                status1 = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.First();

                status2 = (await sm.SendAsync(new SomeEvent())).Status;
            }

            Assert.AreEqual(EventStatus.Consumed, status1);
            Assert.IsTrue(StateMachine1.InitializeFired);
            Assert.IsTrue(State1.ExitFired);
            Assert.IsTrue(SomeEventTransition.GuardFired);
            Assert.IsTrue(SomeEventTransition.EffectFired);
            Assert.IsTrue(State2.EntryFired);
            Assert.AreEqual(StateInfo<State2>.Name, currentState);
            Assert.AreEqual(EventStatus.Consumed, status2);
            Assert.IsTrue(State2.ExitFired);
            Assert.IsTrue(StateMachine1.FinalizeFired);
        }
    }
}