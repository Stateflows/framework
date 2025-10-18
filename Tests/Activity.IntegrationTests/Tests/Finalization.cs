using Activity.IntegrationTests.Classes.Events;
using Stateflows.Common;
using Stateflows.StateMachines;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Finalization : StateflowsTestClass
    {
        private bool Action1Executed = false;
        private bool Action2Executed = false;
        private int Action1ExecutionCounter = 0;
        private int Action2ExecutionCounter = 0;
        
        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddActivities(b => b
                    .AddActivity("simple", b => b
                        .AddInitial(b => b
                            .AddControlFlow<FinalNode>()
                        )
                        .AddFinal()
                    )
                    .AddActivity("forceFinalization", b => b
                        .AddInitial(b => b
                            .AddControlFlow("action1")
                        )
                        .AddAction("action1",
                            c =>
                            {
                                Action1Executed = true;
                                return Task.Delay(1000);
                            },
                            b => b.AddControlFlow("action2")
                        )
                        .AddAction("action2",
                            c =>
                            {
                                Action2Executed = true;
                                return Task.CompletedTask;
                            },
                            b => b.AddControlFlow<FinalNode>()
                        )
                        .AddFinal()
                    )
                    .AddActivity("immediateFinalization", b => b
                        .AddInitial(b => b
                            .AddControlFlow("action1")
                        )
                        .AddAction("action1",
                            async c =>
                            {
                                Action1ExecutionCounter++;
                                await Task.Delay(1000);
                            }
                        )
                        .AddAcceptEventAction<SomeEvent>(async c =>
                        {
                            Action2ExecutionCounter++;
                        })
                    )
                    .AddActivity("non-finalized-input", b => b
                        .AddInput(b => b
                            .AddFlow<int>("action")
                        )
                        .AddAction("action", async c => { })
                    )
                    .AddActivity("non-finalized-event", b => b
                        .AddInitial(b => b
                            .AddControlFlow("action")
                        )
                        .AddAction("action", async c => { })
                        .AddAcceptEventAction<SomeEvent>(async c => { })
                    )
                    .AddActivity("non-finalized-nested-event", b => b
                        .AddInitial(b => b
                            .AddControlFlow("action")
                        )
                        .AddStructuredActivity("action", b => b
                            .AddAcceptEventAction<SomeEvent>(async c => { })
                        )
                    )
                )
                ;
        }

        [TestMethod]
        public async Task SimpleFinalization()
        {
            var initialized = false;
            var finalized = false;

            if (ActivityLocator.TryLocateActivity(new ActivityId("simple", "x"), out var a))
            {
                initialized = (await a.SendAsync(new Initialize())).Status == EventStatus.Initialized;
                finalized = (await a.GetStatusAsync()).Response.BehaviorStatus == BehaviorStatus.Finalized;
            }

            Assert.IsTrue(initialized);
            Assert.IsTrue(finalized);
        }

        [TestMethod]
        public async Task ForceFinalization()
        {
            var initialized = false;
            var status1 = EventStatus.Undelivered;
            var finalized = false;

            if (ActivityLocator.TryLocateActivity(new ActivityId("forceFinalization", "x"), out var a))
            {
                var task1 = Task.Run(async () => status1 = (await a.SendAsync(new Initialize())).Status);
                await Task.Delay(100);
                var task2 = Task.Run(async () => await a.FinalizeAsync());
                await Task.WhenAll(task1, task2);
                
                finalized = (await a.GetStatusAsync()).Response.BehaviorStatus == BehaviorStatus.Finalized;
            }

            Assert.AreEqual(EventStatus.Initialized, status1);
            Assert.IsTrue(Action1Executed);
            Assert.IsFalse(Action2Executed);
            Assert.IsTrue(finalized);
        }

        [TestMethod]
        public async Task ImmediateFinalization()
        {
            var initialized = false;
            var status1 = EventStatus.Undelivered;
            var finalized = false;

            if (ActivityLocator.TryLocateActivity(new ActivityId("immediateFinalization", "x"), out var a))
            {
                _ = a.SendAsync(new Initialize());
                await Task.Delay(100);
                _ = a.SendAsync(new SomeEvent());
                _ = a.SendAsync(new SomeEvent());
                _ = a.SendAsync(new SomeEvent());
                _ = a.SendAsync(new SomeEvent());
                _ = a.SendAsync(new SomeEvent());
                _ = a.SendAsync(new SomeEvent());
                _ = a.SendAsync(new SomeEvent());
                _ = a.SendAsync(new SomeEvent());
                _ = a.FinalizeAsync();

                await Task.Delay(200);
                
                finalized = (await a.GetStatusAsync()).Response.BehaviorStatus == BehaviorStatus.Finalized;
            }

            Assert.AreEqual(1, Action1ExecutionCounter);
            Assert.AreEqual(0, Action2ExecutionCounter);
            Assert.IsTrue(finalized);
        }

        [TestMethod]
        public async Task NoFinalizationInput()
        {
            var initialized = false;
            var finalized = false;

            if (ActivityLocator.TryLocateActivity(new ActivityId("non-finalized-input", "x"), out var a))
            {
                initialized = (await a.SendAsync(new Initialize())).Status == EventStatus.Initialized;
                finalized = (await a.GetStatusAsync()).Response.BehaviorStatus == BehaviorStatus.Finalized;
            }

            Assert.IsTrue(initialized);
            Assert.IsFalse(finalized);
        }

        [TestMethod]
        public async Task NoFinalizationEvent()
        {
            var initialized = false;
            var finalized = false;

            if (ActivityLocator.TryLocateActivity(new ActivityId("non-finalized-event", "x"), out var a))
            {
                initialized = (await a.SendAsync(new Initialize())).Status == EventStatus.Initialized;
                finalized = (await a.GetStatusAsync()).Response.BehaviorStatus == BehaviorStatus.Finalized;
            }

            Assert.IsTrue(initialized);
            Assert.IsFalse(finalized);
        }

        [TestMethod]
        public async Task NoFinalizationNestedEvent()
        {
            var initialized = false;
            var finalized = false;

            if (ActivityLocator.TryLocateActivity(new ActivityId("non-finalized-nested-event", "x"), out var a))
            {
                initialized = (await a.SendAsync(new Initialize())).Status == EventStatus.Initialized;
                finalized = (await a.GetStatusAsync()).Response.BehaviorStatus == BehaviorStatus.Finalized;
            }

            Assert.IsTrue(initialized);
            Assert.IsFalse(finalized);
        }
    }
}