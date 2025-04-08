using Stateflows.Common;
using StateMachine.IntegrationTests.Classes.Events;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    public class Condition1 : ITransitionGuard<SomeEvent>
    {
        public async Task<bool> GuardAsync(SomeEvent @event)
            => !string.IsNullOrEmpty(@event.TheresSomethingHappeningHere);
    }

    public class Condition2 : ITransitionGuard<SomeEvent>
    {
        public async Task<bool> GuardAsync(SomeEvent @event)
            => @event.InitializationSuccessful;
    }

    public class Guard1 : ITransitionGuard
    {
        public async Task<bool> GuardAsync()
            => true;
    }

    public class Guard2 : ITransitionGuard
    {
        public async Task<bool> GuardAsync()
            => false;
    }

    public class Effect1 : ITransitionEffect<SomeEvent>
    {
        public async Task EffectAsync(SomeEvent @event)
            => TypedTransitions.Counter1++;
    }

    public class Effect2 : ITransitionEffect<SomeEvent>
    {
        public async Task EffectAsync(SomeEvent @event)
            => TypedTransitions.Counter1++;
    }

    public class Effect3 : ITransitionEffect<SomeEvent>
    {
        public async Task EffectAsync(SomeEvent @event)
            => TypedTransitions.Counter1++;
    }

    public class OnExit1 : IStateExit
    {
        public async Task OnExitAsync()
            => TypedTransitions.Counter2++;
    }

    public class OnExit2 : IStateExit
    {
        public async Task OnExitAsync()
            => TypedTransitions.Counter2++;
    }

    public class OnExit3 : IStateExit
    {
        public async Task OnExitAsync()
            => TypedTransitions.Counter2++;
    }

    [TestClass]
    public class TypedTransitions : StateflowsTestClass
    {
        private bool? StateExited = null;
        private bool? StateEntered = null;
        private bool? TransitionHappened = null;
        public static int Counter1 = 0;
        public static int Counter2 = 0;

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
                    .AddStateMachine("or", b => b
                        .AddInitialState("state1", b => b
                            .AddOnExit(c => StateExited = true)
                            .AddTransition<SomeEvent>("state2", b => b
                                .AddGuardExpression(b => b
                                    .AddOrExpression(b => b
                                        .AddGuard<Condition1>()
                                        .AddGuard<Condition2>()
                                    )
                                )
                                .AddEffect(c => TransitionHappened = true)
                            )
                        )
                        .AddState("state2", b => b
                            .AddOnEntry(c => StateEntered = true)
                        )
                    )

                    .AddStateMachine("effects", b => b
                        .AddInitialState("state1", b => b
                            .AddOnExit(c => StateExited = true)
                            .AddTransition<SomeEvent>("state2", b => b
                                .AddEffects<Effect1, Effect2, Effect3>()
                            )
                        )
                        .AddState("state2", b => b
                            .AddOnEntry(c => StateEntered = true)
                        )
                    )

                    .AddStateMachine("universal", b => b
                        .AddInitialState("state1", b => b
                            .AddOnExit(c => StateExited = true)
                            .AddTransition<SomeEvent>("state2", b => b
                                .AddGuardExpression(b => b
                                    .AddOrExpression(b => b
                                        .AddGuard<Guard1>()
                                        .AddGuard<Guard2>()
                                    )
                                )
                            )
                        )
                        .AddState("state2", b => b
                            .AddOnEntry(c => StateEntered = true)
                        )
                    )

                    .AddStateMachine("exits", b => b
                        .AddInitialState("state1", b => b
                            .AddOnExit(c => StateExited = true)
                            .AddOnExits<OnExit1, OnExit2, OnExit3>()
                            .AddTransition<SomeEvent>("state2")
                        )
                        .AddState("state2", b => b
                            .AddOnEntry(c => StateEntered = true)
                        )
                    )
                )
                ;
        }

        [TestMethod]
        public async Task OrConditionsTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("or", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent() { InitializationSuccessful = false })).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.IsTrue(StateExited);
            Assert.IsTrue(TransitionHappened);
            Assert.IsTrue(StateEntered);
            Assert.AreEqual("state2", currentState);
        }

        [TestMethod]
        public async Task EffectsTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("effects", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent() { InitializationSuccessful = false })).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.IsTrue(StateExited);
            Assert.AreEqual(3, Counter1);
            Assert.IsTrue(StateEntered);
            Assert.AreEqual("state2", currentState);
        }

        [TestMethod]
        public async Task UniversalGuardsTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("universal", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent() { InitializationSuccessful = false })).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.IsTrue(StateExited);
            Assert.IsTrue(StateEntered);
            Assert.AreEqual("state2", currentState);
        }

        [TestMethod]
        public async Task ExitsTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("exits", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent() { InitializationSuccessful = false })).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.IsTrue(StateExited);
            Assert.AreEqual(3, Counter2);
            Assert.IsTrue(StateEntered);
            Assert.AreEqual("state2", currentState);
        }
    }
}