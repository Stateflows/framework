using Stateflows.Activities;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Activities.Typed;
using Stateflows.Common;

namespace StateMachine.IntegrationTests.Tests
{
    public class BoolInit : Event
    {
        public bool Value { get; set; }
    }

    [TestClass]
    public class Activities : StateflowsTestClass
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
                .AddPlantUml()
                .AddStateMachines(b => b
                    .AddStateMachine("extended", b => b
                        .AddExecutionSequenceObserver()
                        .AddInitializer<BoolInit>(async c =>
                        {
                            c.StateMachine.Values.Set("value", c.InitializationEvent.Value);
                            return true;
                        })
                        .AddInitialState("stateA", b => b
                            .AddTransition<SomeEvent>("stateB", b => b
                                .AddGuardActivity("guard")
                                .AddEffectActivity("effect")
                            )
                        )
                        .AddState("stateB", b => b
                            .AddOnEntryActivity("entry")
                            .AddOnExitActivity("exit")
                        )
                    )
                )
                .AddActivities(b => b
                    .AddActivity("guard", b => b
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddAction(
                            "main",
                            async c =>
                            {
                                GuardRun = true;
                                if (c.Activity.Values.TryGet<bool>("value", out var value))
                                {
                                    c.Output(value);
                                }
                            },
                            b => b.AddFlow<bool, OutputNode>()
                        )
                        .AddOutput()
                    )
                    .AddActivity("effect", b => b
                        .AddInput(b => b
                            .AddFlow<SomeEvent>("main")
                        )
                        .AddAction("main", async c =>
                        {
                            EffectRun = true;
                        })
                    )
                    .AddActivity("entry", b => b
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddAction("main", async c =>
                        {
                            EntryRun = true;
                        })
                    )
                    .AddActivity("exit", b => b
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddAction("main", async c =>
                        {
                            ExitRun = true;
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task ActivityActions()
        {
            string currentState1 = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("extended", "x"), out var sm))
            {
                await sm.SendAsync(new BoolInit() { Value = true });

                await sm.SendAsync(new SomeEvent());

                var currentState = (await sm.GetCurrentStateAsync()).Response;

                currentState1 = currentState.StatesStack.First();
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
        public async Task ActivityGuard()
        {
            string currentState1 = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("extended", "x"), out var sm))
            {
                await sm.SendAsync(new BoolInit() { Value = false });

                await sm.SendAsync(new SomeEvent());

                var currentState = (await sm.GetCurrentStateAsync()).Response;

                currentState1 = currentState.StatesStack.First();
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
    }
}