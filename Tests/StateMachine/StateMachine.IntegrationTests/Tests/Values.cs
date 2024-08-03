using StateMachine.IntegrationTests.Classes.StateMachines;
using StateMachine.IntegrationTests.Classes.States;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Values : StateflowsTestClass
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
                    .AddStateMachine<ValuesStateMachine>()
                )
                ;
        }

        [TestMethod]
        public async Task TypedStateMachine()
        {
            string currentState = State<State1>.Name;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId(StateMachine<ValuesStateMachine>.Name, "x"), out var sm))
            {
                //await sm.InitializeAsync();
                await sm.SendAsync(new SomeEvent());

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesStack.First();
            }

            Assert.AreEqual(State<FinalState>.Name, currentState);
        }
    }
}