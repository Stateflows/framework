using Stateflows.Common;
using Stateflows.Activities;
using StateMachine.IntegrationTests.Classes.Events;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    public class TypedAcceptEventAction : IAcceptEventActionNode<SomeEvent>
    {
        private readonly IBehaviorContext Context;
        public TypedAcceptEventAction(IBehaviorContext context)
        {
            Context = context;
        }
        public async Task ExecuteAsync(SomeEvent @event, CancellationToken cancellationToken)
        {
            Behaviors.eventConsumed = await Context.Values.TryGetAsync<bool>("boolValue") is (true, true);
            Context.Send(new SomeNotification());
        }
    }

    [TestClass]
    public class Behaviors : StateflowsTestClass
    {
        public static bool eventConsumed = false;

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
                    .AddStateMachine("submachine", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddSubmachine(b => b
                                .AddExecutionSequenceObserver()
                                .AddInitialState("stateA", b => b
                                    .AddTransition<SomeEvent>("stateB")
                                )
                                .AddState("stateB", b => b
                                    .AddOnEntry(c => c.Behavior.Send(new SomeNotification()))
                                )
                            )
                            .AddTransition<SomeNotification>("state2")
                        )
                        .AddState("state2", b => b
                            .AddTransition<OtherEvent>("state1")
                        )
                    )
                    
                    .AddStateMachine("submachineBubbling", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddSubmachine(b => b
                                .AddExecutionSequenceObserver()
                                .AddInitialState("stateA", b => b
                                    .AddTransition<SomeEvent, Deny>("stateB")
                                    .AddSubmachine(b => b
                                        .AddExecutionSequenceObserver()
                                        .AddInitialState("stateX", b => b
                                            .AddTransition<SomeEvent, Deny>("stateY")
                                        )
                                        .AddState("stateY")
                                    )
                                )
                                .AddState("stateB")
                            )
                            .AddTransition<SomeEvent>("state2")
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("nested", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("stateA", b => b
                            .AddTransition<SomeEvent>("stateB")
                        )
                        .AddState("stateB", b => b
                            .AddOnEntry(c => c.Behavior.Send(new SomeNotification()))
                        )
                    )

                    .AddStateMachine("doActivity", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddOnEntry(c => c.Behavior.Values.SetAsync("boolValue", true))
                            .AddDoActivity(b => b
                                .AddAcceptEventAction<SomeEvent, TypedAcceptEventAction>()
                            )
                            .AddTransition<SomeNotification>("state2")
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("effectActivity", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddInternalTransition<SomeEvent>(b => b
                                .AddEffectActivity(b => b
                                    .AddInitial(b => b
                                        .AddControlFlow("effect")
                                    )
                                    .AddAction(
                                        "effect",
                                        async c =>
                                        {
                                            eventConsumed = true;
                                            c.Behavior.Send(new SomeNotification());
                                        }
                                    )
                                )
                            )
                            .AddTransition<SomeNotification>("state2")
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("guardActivity", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("state1", b => b
                            .AddTransition<SomeEvent>("state2", b => b
                                .AddGuardActivity(b => b
                                    .AddAcceptEventAction<SomeEvent>(
                                        async c =>
                                        {
                                            c.Output(c.Event.TheresSomethingHappeningHere != string.Empty);
                                        },
                                        b => b.AddFlow<bool, OutputNode>()
                                    )
                                    .AddOutput()
                                )
                            )
                        )
                        .AddState("state2", b => b
                            .AddDefaultTransition("state3", b => b
                                .AddGuardActivity(b => b
                                    .AddAcceptEventAction<Completion>(
                                        async c =>
                                        {
                                            c.Output(true);
                                        },
                                        b => b.AddFlow<bool, OutputNode>()
                                    )
                                    .AddOutput()
                                )
                            )
                        )
                        .AddState("state3")
                    )
                )
                .AddActivities(b => b
                    .AddActivity("nested", b => b
                        .AddAcceptEventAction<SomeEvent, TypedAcceptEventAction>()
                    )
                    .AddActivity("integrated", b => b
                        .AddInitial(b => b
                            .AddControlFlow("effect")
                        )
                        .AddAction(
                            "effect",
                            async c =>
                            {
                                eventConsumed = true;
                                c.Behavior.Send(new SomeNotification());
                            }
                        )
                    )
                    .AddActivity("guard", b => b
                        .AddAcceptEventAction<SomeEvent>(
                            async c =>
                            {
                                c.Output(c.Event.TheresSomethingHappeningHere != string.Empty);
                            },
                            b => b
                                .AddFlow<bool, OutputNode>(b => b
                                    .SetWeight(0)
                                )
                        )
                        .AddAcceptEventAction<Completion>(
                            async c =>
                            {
                                c.Output(true);
                            },
                            b => b
                                .AddFlow<bool, OutputNode>(b => b
                                    .SetWeight(0)
                                )
                        )
                        .AddOutput()
                    )
                )
                ;
        }

        [TestMethod]
        public async Task SimpleSubmachine()
        {
            var initialized = false;
            string currentState1 = "";
            var someStatus1 = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("submachine", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                someStatus1 = (await sm.SendAsync(new SomeEvent())).Status;

                await Task.Delay(100);

                var currentState = (await sm.GetStatusAsync()).Response;

                currentState1 = currentState.CurrentStates.Value;

                await sm.SendAsync(new OtherEvent());

                await Task.Delay(5000);
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                
                .StateEntry("stateA")
                .StateExit("stateA")
                .StateEntry("stateB")
                .StateExit("stateB")
                .StateMachineFinalize()
                
                .StateEntry("state2")
            
                .StateExit("state2")
                .StateEntry("state1")
                .StateEntry("stateA")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(EventStatus.Forwarded, someStatus1);
            Assert.AreEqual("state2", currentState1);
        }

        [TestMethod]
        public async Task SubmachineBubbling()
        {
            var initialized = false;
            string currentState1 = "";
            var someStatus1 = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("submachineBubbling", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                someStatus1 = (await sm.SendAsync(new SomeEvent())).Status;

                await Task.Delay(100);

                var currentState = (await sm.GetStatusAsync()).Response;

                currentState1 = currentState.CurrentStates.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                .StateEntry("stateA")
                .StateEntry("stateX")
                
                .TransitionGuard(Event<SomeEvent>.Name, "stateX", "stateY")
                .TransitionGuard(Event<SomeEvent>.Name, "stateA", "stateB")
                .TransitionGuard(Event<SomeEvent>.Name, "state1", "state2")
                
                .StateExit("stateX")
                .StateMachineFinalize()
                .StateExit("stateA")
                .StateMachineFinalize()
                .StateExit("state1")

                .TransitionEffect(Event<SomeEvent>.Name, "state1", "state2")
                
                .StateEntry("state2")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(EventStatus.Forwarded, someStatus1);
            Assert.AreEqual("state2", currentState1);
        }

        [TestMethod]
        public async Task SimpleDoActivity()
        {
            var initialized = false;
            string currentState1 = "";
            var someStatus1 = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("doActivity", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                someStatus1 = (await sm.SendAsync(new SomeEvent())).Status;

                await Task.Delay(200);

                currentState1 = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                .StateEntry("state2")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(EventStatus.Forwarded, someStatus1);
            Assert.AreEqual(true, eventConsumed);
            Assert.AreEqual("state2", currentState1);
        }

        [TestMethod]
        public async Task SimpleEffectIntegration()
        {
            var initialized = false;
            string currentState1 = "";
            var someStatus1 = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("effectActivity", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                someStatus1 = (await sm.SendAsync(new SomeEvent())).Status;

                await Task.Delay(500);

                currentState1 = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                .StateEntry("state2")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(EventStatus.Consumed, someStatus1);
            Assert.AreEqual(true, eventConsumed);
            Assert.AreEqual("state2", currentState1);
        }

        [TestMethod]
        public async Task SimpleGuardIntegration()
        {
            var initialized = false;
            string currentState1 = "";
            var someStatus1 = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("guardActivity", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                someStatus1 = (await sm.SendAsync(new SomeEvent())).Status;

                await Task.Delay(200);

                currentState1 = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                .StateEntry("state2")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(EventStatus.Forwarded, someStatus1);
            Assert.AreEqual("state3", currentState1);
        }
    }
}