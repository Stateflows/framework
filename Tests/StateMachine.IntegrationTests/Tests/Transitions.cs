using Stateflows.Common;
using StateMachine.IntegrationTests.Classes.Events;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Transitions : StateflowsTestClass
    {
        private bool? StateExited = null;
        private bool? StateEntered = null;
        private bool? TransitionHappened = null;

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
                    .AddStateMachine("simple", b => b
                        .AddInitialState("state1", b => b
                            .AddOnExit(c => StateExited = true)
                            .AddTransition<SomeEvent>("state2", b => b
                                .AddEffect(c => TransitionHappened = !c.CurrentStates.HasValue)
                            )
                        )
                        .AddState("state2", b => b
                            .AddOnEntry(c => StateEntered = c.ExecutionSteps.LastOrDefault()?.SourceName == "state1")
                        )
                    )
                    
                    // .AddStateMachine("polymorphic", b => b
                    //     .AddInitialState("state1", b => b
                    //         .AddOnExit(c => StateExited = true)
                    //         .AddTransition<SomeEvent>("state2", b => b
                    //             .SetPolymorphicTriggers(true)
                    //             .AddEffect(c => TransitionHappened = !c.CurrentState.HasValue)
                    //         )
                    //     )
                    //     .AddState("state2", b => b
                    //         .AddOnEntry(c => StateEntered = c.ExecutionSteps.LastOrDefault()?.SourceName == "state1")
                    //     )
                    // )

                    .AddStateMachine("guarded", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<OtherEvent>("state2", b => b
                                .AddGuard(c => c.Event.AnswerToLifeUniverseAndEverything == 42)
                                .AddGuard(Guards.Source.Namespace("x").Value("x").IsSet)
                            )
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("guardedWithAndExpression", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<OtherEvent>("state2", b => b
                                .AddGuardExpression(b => b
                                    .AddAndExpression(b => b
                                        .AddGuard(c => c.Event.AnswerToLifeUniverseAndEverything == 42)
                                        .AddNegatedGuard(c => String.IsNullOrEmpty(c.Event.RequiredParameter))
                                    )
                                )
                            )
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("guardedWithOrExpression", b => b
                        .AddInitialState("state1", b => b
                            .AddDefaultTransition("state2", b => b
                                .AddGuardExpression(b => b
                                    .AddOrExpression(b => b
                                        .AddNegatedGuard(c => true)
                                    )
                                )
                            )
                            .AddTransition<OtherEvent>("state2", b => b
                                .AddGuardExpression(b => b
                                    .AddOrExpression(b => b
                                        .AddGuard(c => c.Event.AnswerToLifeUniverseAndEverything == 42)
                                        .AddGuard(c => String.IsNullOrEmpty(c.Event.RequiredParameter))
                                    )
                                )
                            )
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("default", b => b
                        .AddInitialState("state1", b => b
                            .AddTransition<SomeEvent>("state2")
                        )
                        .AddState("state2", b => b
                            .AddDefaultTransition("state3")
                        )
                        .AddState("state3")
                    )

                    .AddStateMachine("internal", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddOnExit(c => StateExited = true)
                            .AddInternalTransition<SomeEvent>(b => b
                                .AddEffect(c => TransitionHappened = true)
                            )
                        )
                    )

                    .AddStateMachine("self", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddOnExit(c => StateExited = true)
                            .AddTransition<SomeEvent>("state1", b => b
                                .AddEffect(c => TransitionHappened = true)
                            )
                        )
                    )
                
                    .AddStateMachine("parentToChildDefault", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialCompositeState("parent", b => b
                            .AddDefaultTransition("child", b => b
                                .AddNegatedGuard(Guards.Global.Value("counter").IsEqualTo(10))
                            )
                            .AddState("child", b => b
                                .AddOnEntry(Effects.Global.Value("counter").Update(c => c + 1, 0))
                            )
                        )
                    )
                )
                ;
        }

        [TestMethod]
        public async Task SimpleTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";
            string[] expectedEvents = Array.Empty<string>();

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var sm))
            {
                expectedEvents = (await sm.GetStatusAsync()).Response.ExpectedEvents.ToArray();
                
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            Assert.IsTrue(expectedEvents.Contains(Event<SomeEvent>.Name));
            Assert.AreEqual(1, expectedEvents.Count());
            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.IsTrue(StateExited);
            Assert.IsTrue(TransitionHappened);
            Assert.IsTrue(StateEntered);
            Assert.AreEqual("state2", currentState);
        }

        // [TestMethod]
        // public async Task InheritedTransition()
        // {
        //     var status = EventStatus.Rejected;
        //     string currentState = "state1";
        //     string[] expectedEvents = Array.Empty<string>();
        //
        //     if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("polymorphic", "x"), out var sm))
        //     {
        //         expectedEvents = (await sm.GetStatusAsync()).Response.ExpectedEvents.ToArray();
        //         
        //         status = (await sm.SendAsync(new SomeInheritedEvent())).Status;
        //
        //         currentState = (await sm.GetStatusAsync()).Response.StatesTree.Value;
        //     }
        //
        //     Assert.IsTrue(expectedEvents.Contains(Event<SomeEvent>.Name));
        //     Assert.AreEqual(1, expectedEvents.Count());
        //     Assert.AreEqual(EventStatus.Consumed, status);
        //     Assert.IsTrue(StateExited);
        //     Assert.IsTrue(TransitionHappened);
        //     Assert.IsTrue(StateEntered);
        //     Assert.AreEqual("state2", currentState);
        // }

        [TestMethod]
        public async Task GuardedTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("guarded", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 43 })).Status;

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            Assert.AreEqual(EventStatus.NotConsumed, status);
            Assert.IsNull(StateExited);
            Assert.IsNull(StateEntered);
            Assert.AreNotEqual("state2", currentState);
        }

        [TestMethod]
        public async Task AndExpressionGuardedTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("guardedWithAndExpression", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 43 })).Status;

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            Assert.AreEqual(EventStatus.NotConsumed, status);
            Assert.IsNull(StateExited);
            Assert.IsNull(StateEntered);
            Assert.AreNotEqual("state2", currentState);
        }

        [TestMethod]
        public async Task OrExpressionGuardedTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("guardedWithOrExpression", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 43 })).Status;

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            Assert.AreEqual(EventStatus.NotConsumed, status);
            Assert.IsNull(StateExited);
            Assert.IsNull(StateEntered);
            Assert.AreNotEqual("state2", currentState);
        }

        [TestMethod]
        public async Task GuardedTransitionInvalid()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("guarded", "x"), out var sm))
            {
                status = (await sm.SendAsync(new OtherEvent() { AnswerToLifeUniverseAndEverything = 42, RequiredParameter = string.Empty })).Status;

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            Assert.AreEqual(EventStatus.Invalid, status);
            Assert.IsNull(StateExited);
            Assert.IsNull(StateEntered);
            Assert.AreNotEqual("state2", currentState);
        }

        [TestMethod]
        public async Task DefaultTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("default", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state3", currentState);
        }

        [TestMethod]
        public async Task InternalTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = string.Empty;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("internal", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
            );
            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.IsTrue(TransitionHappened);
            Assert.IsNull(StateExited);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task SelfTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("self", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
            );
            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.IsTrue(TransitionHappened);
            Assert.IsTrue(StateExited);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task ParentToChildDefaultTransition()
        {
            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("parentToChildDefault", "x"), out var sm))
            {
                await sm.SendAsync(new Initialize());
            }
            
            
            ExecutionSequence.Verify(b => b
                .StateMachineInitialize()
                .StateEntry("parent")
                .DefaultTransitionEffect("parent", "child")
                .StateEntry("child")
            );
        }
    }
}