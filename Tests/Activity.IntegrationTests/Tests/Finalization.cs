using Activity.IntegrationTests.Classes.Events;
using Stateflows.Common;
using Stateflows.Activities;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{

    [TestClass]
    public class Finalization : StateflowsTestClass
    {
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