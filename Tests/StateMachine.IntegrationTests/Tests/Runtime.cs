using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Classes.Events;
using StateMachine.IntegrationTests.Utils;
using StateMachine.IntegrationTests.Classes.States;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Runtime : StateflowsTestClass
    {
        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder.AddStateMachines(b => b
                .AddStateMachine("standalone", b => b
                    .AddInitialState("initial", b => b
                        .AddTransition<SomeEvent>("final")
                    )
                    .AddFinalState("final")
                )
            );
        }

        [TestMethod]
        public async Task RuntimeRegistration()
        {
            var register = ServiceProvider.GetRequiredService<IStateMachinesRegister>();
            register
                .AddStateMachine("runtime", b => b
                    .AddInitialState("initial", b => b
                        .AddTransition<SomeEvent>("final")
                    )
                    .AddFinalState("final")
                );
            
            string currentState = State<State1>.Name;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("runtime", "x"), out var sm))
            {
                await sm.SendAsync(new SomeEvent());

                currentState = (await sm.GetStatusAsync()).Response?.CurrentStates?.Value;
            }

            Assert.AreEqual("final", currentState);
        }
    }
}