using Stateflows.Common;
using StateMachine.IntegrationTests.Classes.Events;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Exceptions : StateflowsTestClass
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
                    .AddStateMachine("uncatched", b => b
                        .AddInitialState("state1", b => b
                            .AddOnEntry(c => throw new Exception("example"))
                            .AddTransition<SomeNotification>("state2")
                        )
                        .AddState("state2")
                    )
                    
                    .AddStateMachine("reset", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<SomeEvent>("state2")
                        )
                        .AddState("state2", b => b
                            .AddOnEntry(c => throw new Exception("example"))
                        )
                    )
                    
                    .AddStateMachine("transition", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<SomeEvent>("state2")
                            .AddTransition<Exception>("state3")
                        )
                        .AddState("state2", b => b
                            .AddOnEntry(c => throw new Exception("example"))
                        )
                        .AddState("state3")
                    )
                    
                    .AddStateMachine("generalTransition", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<SomeEvent>("state2")
                            .AddTransition<SystemException>("state3")
                        )
                        .AddState("state2", b => b
                            .AddOnEntry(c => throw new NotImplementedException("example"))
                        )
                        .AddState("state3")
                    )
                )
                ;
        }

        [TestMethod]
        public async Task Uncatched()
        {
            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("uncatched", "x"), out var sm))
            {
                await sm.SendAsync(new Initialize());
            }

            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task ResetOnUnhandledException()
        {
            var state1 = string.Empty;
            var state2 = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("reset", "x"), out var sm))
            {
                await sm.SendAsync(new Initialize());
                state1 = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
                await sm.SendAsync(new SomeEvent());
                await Task.Delay(100);
                state2 = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            Assert.AreEqual(state1, state2);
        }

        [TestMethod]
        public async Task ExceptionHandledByTransition()
        {
            var state1 = string.Empty;
            var state2 = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("transition", "x"), out var sm))
            {
                await sm.SendAsync(new Initialize());
                state1 = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
                
                await sm.SendAsync(new SomeEvent());
                await Task.Delay(100);
                state2 = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            Assert.AreEqual("state1", state1);
            Assert.AreEqual("state3", state2);
        }

        [TestMethod]
        public async Task ExceptionHandledByGeneralTransition()
        {
            var state1 = string.Empty;
            var state2 = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("generalTransition", "x"), out var sm))
            {
                await sm.SendAsync(new Initialize());
                state1 = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
                await sm.SendAsync(new SomeEvent());
                await Task.Delay(100);
                state2 = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            Assert.AreEqual("state1", state1);
            Assert.AreEqual("state3", state2);
        }
    }
}