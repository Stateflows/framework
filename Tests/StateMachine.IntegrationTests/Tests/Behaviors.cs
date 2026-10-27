using System.Diagnostics;
using Stateflows.Common;
using Stateflows.Activities;
using Stateflows.Common.Attributes;
using Stateflows.Common.Classes;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;
using StateMachine.IntegrationTests.Classes.Events;
using StateMachine.IntegrationTests.Utils;
using Deny = Stateflows.Common.Deny;

namespace StateMachine.IntegrationTests.Tests
{
    public class TypedAcceptEventAction(IBehaviorContext context, [FromProjection<ExtendedState<bool>>(EntityScope.Owner)] bool value, IParentBehaviorContext parentContext, IActivityContext activityContext) : IAcceptEventActionNode<SomeEvent>
    {
        public async Task ExecuteAsync(SomeEvent @event, CancellationToken cancellationToken)
        {
            Behaviors.eventConsumed = value;// await parentContext.TryGetProjectionAsync<bool>() is (true, true);
            context.Send(new SomeNotification());
        }
    }

    public class AcceptOtherEventAction : IAcceptEventActionNode<OtherEvent>
    {
        public static bool EventConsumed { get; private set; } = false;
        public async Task ExecuteAsync(OtherEvent @event, CancellationToken cancellationToken)
        {
            EventConsumed = true;
        }
    }

    public class AcceptOtherEventAction2 : IAcceptEventActionNode<OtherEvent>
    {
        public static bool EventConsumed { get; private set; } = false;
        public async Task ExecuteAsync(OtherEvent @event, CancellationToken cancellationToken)
        {
            await Task.Delay(100);
            EventConsumed = true;
        }
    }
    

    [TestClass]
    public class Behaviors : StateflowsTestClass
    {
        public interface EntityWithList
        {
            [Field]
            public List<string>? Messages { get; set; }
    
            [Mutation]
            public void SetMessages(List<string> messages) => Messages = messages;
            
            [Projection(PublishScope.Owner)]
            public string[] MessagesProjection => Messages?.ToArray() ?? [];
        }
        
        public static bool eventConsumed = false;
        public static int executionCount = 0;
        public static bool GloballyIntercepted = false;
        
        public class GlobalInterceptor : BehaviorInterceptor
        {
            public override Task NotificationPublishedAsync<TNotification>(IBehaviorActionContext context, TNotification notification, IDictionary<string, EventHeader> headers)
            {
                if (typeof(TNotification) == typeof(string))
                {
                    GloballyIntercepted = true;
                }

                return Task.CompletedTask;
            }
        }

        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddInterceptor<GlobalInterceptor>()
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
                        .AddEntity<ExtendedState<bool>>()
                        .AddInitialState("state1", b => b
                            .AddOnEntry(c => c.Behavior.TryMutateAsync<ExtendedState<bool>, bool>(true))
                            .AddDoActivity(b => b
                                .AddAcceptEventAction<SomeEvent, TypedAcceptEventAction>()
                            )
                            .AddTransition<SomeNotification>("state2")
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("compositeDoActivity", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialCompositeState("state1", b => b
                            .AddDoActivity(b => b
                                .AddAcceptEventAction<OtherEvent, AcceptOtherEventAction>()
                            )
                            .AddTransition<OtherEvent>("state2")
                            .AddInitialState("state1.1", b => b
                                .AddTransition<OtherEvent>("state1.2")
                            )
                            .AddState("state1.2")
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("hierarchicalDoActivity", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialCompositeState("state1", b => b
                            .AddDoActivity(b => b
                                .AddAcceptEventAction<OtherEvent, AcceptOtherEventAction>()
                            )
                            .AddTransition<OtherEvent>("state2")
                            .AddInitialState("state1.1", b => b
                                .AddTransition<OtherEvent>("state1.2")
                            )
                            .AddState("state1.2", b => b
                                .AddDoActivity(b => b
                                    .AddAcceptEventAction<OtherEvent, AcceptOtherEventAction2>()
                                )
                            )
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("doActivityWithEntitySubscriptions", b => b
                        .AddExecutionSequenceObserver()
                        .AddEntity<ExtendedState<bool>>()
                        .AddEntity<EntityWithList>()
                        .AddInitialState("state1", b => b
                            .AddProjectionSubscription<ExtendedState<bool>, bool>()
                            .AddDoAction(async c =>
                            {
                                if (c.TryGetOwnerBehaviorContext(out var ownerBehavior))
                                {
                                    await ownerBehavior.TryMutateAsync<ExtendedState<bool>, bool>(true);
                                    await ownerBehavior.TryMutateAsync<EntityWithList, List<string>>(["a", "b", "c"]);
                                }
                            })
                            .AddTransition<bool>("state2", b => b
                                .AddGuard(async c =>
                                {
                                    var (success, value) = await c.Behavior.TryGetProjectionAsync<ExtendedState<bool>, bool>();
                                    return value;
                                })
                            )
                        )
                        .AddState("state2")
                    )

                    .AddStateMachine("repeatedDoActivity", b => b
                        .AddExecutionSequenceObserver()
                        .AddDefaultInitializer(async c =>
                        {
                            await c.Behavior.Values.SetAsync("correlationId", 42);
                            return true;
                        })
                        .AddInitialState("state1", b => b
                            .AddDoActivity(b => b
                                .AddAcceptEventAction<SomeEvent>(actionAsync: async c =>
                                {
                                    if (await c.Behavior.Values.GetOrDefaultAsync("correlationId", -1) == 42)
                                    {
                                        executionCount++;
                                        if (executionCount == 1)
                                        {
                                            c.Behavior.Publish(new SomeNotification());
                                        }
                                        if (executionCount == 2)
                                        {
                                            c.Behavior.Publish(new OtherNotification());
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException();
                                    }
                                })
                            )
                            .AddTransition<OtherEvent>("state2")
                        )
                        .AddState("state2", b => b
                            .AddTransition<OtherEvent>("state1")
                        )
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
                                            if (c.TryGetOwnerBehaviorContext(out var ownerBehavior))
                                            {
                                                ownerBehavior.Send(new SomeNotification());
                                            }
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

                await Task.Delay(100);
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                
                .StateEntry("stateA")
                .StateExit("stateA")
                .StateEntry("stateB")
                .StateEntry("state2")
                .StateExit("stateB")
                .StateMachineFinalize()
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
                
                .TransitionEffect(Event<SomeEvent>.Name, "state1", "state2")
                
                .StateEntry("state2")
            );
            ExecutionSequence.Verify(b => b
                .StateExit("state1")
                .StateExit("stateA")
                .StateExit("stateX")
            );
            ExecutionSequence.Verify(b => b
                .StateMachineFinalize()
                .StateMachineFinalize()
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
        public async Task CompositeDoActivity()
        {
            var initialized = false;
            IReadOnlyTree<string> currentState = null;
            var someStatus1 = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("compositeDoActivity", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                someStatus1 = (await sm.SendAsync(new OtherEvent())).Status;

                await Task.Delay(200);

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                .StateEntry("state1.1")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(EventStatus.Forwarded, someStatus1);
            Assert.AreEqual(true, AcceptOtherEventAction.EventConsumed);
            Assert.AreEqual("state1", currentState?.Value);
            Assert.AreEqual("state1.1", currentState?.Root.Nodes.FirstOrDefault()?.Value);
        }

        [TestMethod]
        public async Task HierarchicalDoActivity()
        {
            var initialized = false;
            IReadOnlyTree<string> currentState = null;
            var otherStatus1 = EventStatus.Rejected;
            var otherStatus2 = EventStatus.Rejected;
            var otherStatus3 = EventStatus.Rejected;
            var otherStatus4 = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("hierarchicalDoActivity", "x"), out var sm))
            {
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                otherStatus1 = (await sm.SendAsync(new OtherEvent())).Status;
                otherStatus2 = (await sm.SendAsync(new OtherEvent(), new Dictionary<string, EventHeader>(){{nameof(NoForwarding), new NoForwarding()}})).Status;
                otherStatus3 = (await sm.SendAsync(new OtherEvent())).Status;
                otherStatus4 = (await sm.SendAsync(new OtherEvent(), new Dictionary<string, EventHeader>(){{nameof(NoForwarding), new NoForwarding()}})).Status;

                await Task.Delay(200);

                currentState = (await sm.GetStatusAsync()).Response.CurrentStates;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                .StateEntry("state1.1")
                .StateExit("state1.1")
                .StateEntry("state1.2")
                .StateExit("state1.2")
                .StateExit("state1")
                .StateEntry("state2")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(EventStatus.Forwarded, otherStatus1);
            Assert.AreEqual(true, AcceptOtherEventAction.EventConsumed);
            Assert.AreEqual(true, AcceptOtherEventAction2.EventConsumed);
            Assert.AreEqual("state2", currentState?.Value);
        }

        [TestMethod]
        public async Task DoActivityWithEntitySubscriptions()
        {
            var initialized = false;
            string currentState1 = "";
            var notification1ReceivedCounter = 0;
            var notification2ReceivedCounter = 0;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("doActivityWithEntitySubscriptions", "x"), out var sm))
            {
                GloballyIntercepted = false;
                await using var stringWatcher = await sm.WatchAsync<string>(s => notification1ReceivedCounter++);
                await using var stringArrayWatcher = await sm.WatchAsync<string[]>(s => notification2ReceivedCounter++);
                
                initialized = (await sm.SendAsync(new Initialize())).Status == EventStatus.Initialized;

                await Task.Delay(1200);

                currentState1 = (await sm.GetStatusAsync()).Response.CurrentStates.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("state1")
                .StateEntry("state2")
            );
            Assert.IsTrue(initialized);
            Assert.AreEqual(1, notification1ReceivedCounter);
            Assert.AreEqual(1, notification2ReceivedCounter);
            Assert.AreEqual("state2", currentState1);
            Assert.IsTrue(GloballyIntercepted, "Global interceptor should have intercepted the string notification");
        }
        
        [TestMethod]
        public async Task RepeatedDoActivity()
        {
            var initialized = false;
            string currentState1 = "";
            var someStatus1 = EventStatus.Rejected;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("repeatedDoActivity", "x"), out var sm))
            {
                await sm.SendAsync(new Initialize());
                await sm.SendAsync(new SomeEvent());
                // await sm.NextAsync<SomeNotification>(DateTime.Now.AddSeconds(-1)).WaitAsync(new TimeSpan(0, 0, 0, 1));
                await sm.SendAsync(new OtherEvent());
                await sm.SendAsync(new OtherEvent());
                await sm.SendAsync(new SomeEvent());
                // await sm.NextAsync<OtherNotification>(DateTime.Now.AddSeconds(-1)).WaitAsync(new TimeSpan(0, 0, 0, 1));
            }

            // ExecutionSequence.Verify(b => b
            //     .StateEntry("state1")
            //     .StateEntry("state2")
            //     .StateEntry("state1")
            // );
            // Assert.AreEqual(2, executionCount);
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