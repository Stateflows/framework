using StateMachine.IntegrationTests.Utils;
using System.Diagnostics;
using Stateflows.Actions;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using StateMachine.IntegrationTests.Classes.Events;

namespace StateMachine.IntegrationTests.Tests
{
    public class TypedAction : IAction
    {
        private int ProcessId;
        private IBehaviorContext BehaviorContext;

        public TypedAction(
            IBehaviorContext behaviorContext,
            [GlobalValue] int processId
        )
        {
            BehaviorContext = behaviorContext;
            ProcessId = processId;
        }
        
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (ProcessId == 42)
            {
                BehaviorContext.Send(new SomeEvent());
            }
            
            return Task.CompletedTask;
        }
    }
    
    [TestClass]
    public class Actions : StateflowsTestClass
    {
        public bool GuardRun = false;
        public bool EffectRun = false;
        public bool EntryRun = false;
        public bool ExitRun = false;

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
                    .AddStateMachine("extended", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitializer<BoolInit>(async c =>
                        {
                            Debug.WriteLine($"InitializationEvent.Value: {c.InitializationEvent.Value}");
                            await c.Behavior.Values.SetAsync("value", c.InitializationEvent.Value);
                            return true;
                        })
                        .AddInitialState("stateA", b => b
                            .AddTransition<SomeEvent>("stateB", b => b
                                .AddGuardAction(async c =>
                                {
                                    GuardRun = true;
                                    var (success, value) = await c.Behavior.Values.TryGetAsync<bool>("value");
                                    if (success)
                                    {
                                        Debug.WriteLine($"value: {value}");
                                        c.Output(value);
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"value: not available");
                                    }
                                })
                                .AddEffectAction(async c => EffectRun = true)
                            )
                        )
                        .AddState("stateB", b => b
                            .AddOnEntryAction(async c => EntryRun = true)
                            .AddOnExitAction(async c => ExitRun = true)
                        )
                    )
                    .AddStateMachine("subscription", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("initial", b => b
                            .AddOnEntryAction(async c => c.Behavior.Send(new SomeEvent()))
                            .AddTransition<SomeEvent>("final")
                        )
                        .AddFinalState("final")
                    )
                    .AddStateMachine("relay", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("initial", b => b
                            .AddOnEntryAction(async c =>
                            {
                                c.Behavior.Publish(new SomeEvent() { TheresSomethingHappeningHere = "42" });
                            })
                        )
                    )
                    .AddStateMachine("values", b => b
                        .AddInitialState("initial", b => b
                            .AddOnEntry(Stateflows.StateMachines.Actions.Global.Value("processId").Set(42))
                            .AddDefaultTransition("second")
                        )
                        .AddState("second", b => b
                            .AddOnEntryAction<TypedAction>()
                            .AddTransition<SomeEvent>("third")
                        )
                        .AddState("third")
                    )
                )
                .AddActions(b => b
                    .AddAction("guard", async c =>
                    {
                        GuardRun = true;
                        var (success, value) = await c.Behavior.Values.TryGetAsync<bool>("value");
                        if (success)
                        {
                            Debug.WriteLine($"value: {value}");
                            c.Output(value);
                        }
                        else
                        {
                            Debug.WriteLine($"value: not available");
                        }
                    })
                )
                ;
        }

        [TestMethod]
        public async Task ActionActions()
        {
            string currentState1 = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("extended", "x"), out var sm))
            {
                await sm.SendAsync(new BoolInit() { Value = true });

                await sm.SendAsync(new SomeEvent());

                await Task.Delay(200);
                
                var currentState = (await sm.GetStatusAsync()).Response;

                currentState1 = currentState.CurrentStates.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("stateA")
                .StateExit("stateA")
                .StateEntry("stateB")
            );

            Assert.AreEqual("stateB", currentState1);
            Assert.AreEqual(true, GuardRun);
            Assert.AreEqual(true, EffectRun);
            Assert.AreEqual(true, EntryRun);
            Assert.AreEqual(false, ExitRun);
        }

        [TestMethod]
        public async Task ActionGuard()
        {
            string currentState1 = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("extended", "y"), out var sm))
            {
                await sm.SendAsync(new BoolInit() { Value = false });

                await sm.SendAsync(new SomeEvent());

                await Task.Delay(100);

                var currentState = (await sm.GetStatusAsync()).Response;

                currentState1 = currentState.CurrentStates.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("stateA")
            );

            Assert.AreEqual("stateA", currentState1);
            Assert.AreEqual(true, GuardRun);
            Assert.AreEqual(false, EffectRun);
            Assert.AreEqual(false, EntryRun);
            Assert.AreEqual(false, ExitRun);
        }

        [TestMethod]
        public async Task ActionSubscription()
        {
            string currentState1 = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("subscription", "y"), out var sm))
            {
                await sm.SendAsync(new Initialize());

                await Task.Delay(100);
                
                var currentState = (await sm.GetStatusAsync()).Response;

                currentState1 = currentState.CurrentStates.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("initial")
                .StateExit("initial")
                .StateEntry("final")
            );

            Assert.AreEqual("final", currentState1);
        }

        [TestMethod]
        public async Task ActionValues()
        {
            string currentState1 = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("values", "y"), out var sm))
            {
                await sm.SendAsync(new Initialize());

                await Task.Delay(100);
                
                var currentState = (await sm.GetStatusAsync()).Response;

                currentState1 = currentState.CurrentStates.Value;
            }

            Assert.AreEqual("third", currentState1);
        }

        [TestMethod]
        public async Task ActionRelay()
        {
            bool notificationDelivered = false;
        
            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("relay", "y"), out var sm))
            {
                await using var watcher = await sm.WatchAsync<SomeEvent>(n => notificationDelivered = true);
                
                await sm.SendAsync(new Initialize());
                
                await Task.Delay(100);
            }
            
            Assert.IsTrue(notificationDelivered);
        }
    }
}