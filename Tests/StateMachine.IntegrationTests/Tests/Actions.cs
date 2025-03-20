using Stateflows.Activities;
using StateMachine.IntegrationTests.Utils;
using System.Diagnostics;
using Stateflows.Actions;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Common.Utilities;

namespace StateMachine.IntegrationTests.Tests
{
    public class TypedAction : IAction
    {
        private int ProcessId;
        private IActionContext ActionContext;

        public TypedAction(
            IActionContext actionContext,
            [GlobalValue] int processId
        )
        {
            ActionContext = actionContext;
            ProcessId = processId;
        }
        
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (ProcessId == 42)
            {
                ActionContext.Publish(new SomeEvent());
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
                            c.Behavior.Values.Set("value", c.InitializationEvent.Value);
                            return true;
                        })
                        .AddInitialState("stateA", b => b
                            .AddTransition<SomeEvent>("stateB", b => b
                                .AddGuardAction("guard")
                                .AddEffectAction("effect")
                            )
                        )
                        .AddState("stateB", b => b
                            .AddOnEntryAction("entry")
                            .AddOnExitAction("exit")
                        )
                    )
                    .AddStateMachine("subscription", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitialState("initial", b => b
                            .AddOnEntryAction("subscribe", b => b
                                .AddSubscription<SomeEvent>()
                            )
                            .AddTransition<SomeEvent>("final")
                        )
                        .AddFinalState("final")
                    )
                    .AddStateMachine("values", b => b
                        .AddInitialState("initial", b => b
                            .AddOnEntry(Stateflows.StateMachines.Actions.Global.Value("processId").Set(42))
                            .AddDefaultTransition("second")
                        )
                        .AddState("second", b => b
                            .AddOnEntryAction<TypedAction>(b => b
                                .AddSubscription<SomeEvent>()
                            )
                            .AddTransition<SomeEvent>("third")
                        )
                        .AddState("third")
                    )
                )
                .AddActions(b => b
                    .AddAction("guard", async c =>
                    {
                        GuardRun = true;
                        if (c.Behavior.Values.TryGet<bool>("value", out var value))
                        {
                            Debug.WriteLine($"value: {value}");
                            c.Output(value);
                        }
                        else
                        {
                            Debug.WriteLine($"value: not available");
                        }
                    })
                    .AddAction("effect", async c => EffectRun = true)
                    .AddAction("entry", async c => EntryRun = true)
                    .AddAction("exit", async c => ExitRun = true)
                    .AddAction("subscribe", async c => c.Behavior.Publish(new SomeEvent()))
                    .AddAction<TypedAction>()
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

                var currentState = (await sm.GetCurrentStateAsync()).Response;

                currentState1 = currentState.StatesTree.Value;
            }

            ExecutionSequence.Verify(b => b
                .StateEntry("stateA")
                .StateExit("stateA")
                .StateEntry("stateB")
            );

            await Task.Delay(100);

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

                var currentState = (await sm.GetCurrentStateAsync()).Response;

                currentState1 = currentState.StatesTree.Value;
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
                
                var currentState = (await sm.GetCurrentStateAsync()).Response;

                currentState1 = currentState.StatesTree.Value;
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
                
                var currentState = (await sm.GetCurrentStateAsync()).Response;

                currentState1 = currentState.StatesTree.Value;
            }

            // ExecutionSequence.Verify(b => b
            //     .StateEntry("initial")
            //     .StateExit("initial")
            //     .StateEntry("final")
            // );

            Assert.AreEqual("third", currentState1);
        }
    }
}