using Stateflows.Common;
using Stateflows.Activities.Typed;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Execution : StateflowsTestClass
    {
        public static bool Executed = false;

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
                            .AddControlFlow("main")
                        )
                        .AddAction(
                            "main",
                            async c => Executed = true,
                            b => b.AddControlFlow<FinalNode>()
                        )
                        .AddFinal()
                    )
                    .AddActivity("output", b => b
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddAction(
                            "main",
                            async c =>
                            {
                                Executed = true;
                                c.Output(42);
                            },
                            b => b
                                .AddFlow<int, OutputNode>()
                                .AddControlFlow<FinalNode>()
                        )
                        .AddOutput()
                        .AddFinal()
                    )
                    .AddActivity("initialized", b => b
                        .AddInitializer<SomeEvent>(async c => true)
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddAction(
                            "main",
                            async c => Executed = true,
                            b => b.AddControlFlow<FinalNode>()
                        )
                        .AddFinal()
                    )
                    .AddActivity("interactive", b => b
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddAcceptEventAction<SomeEvent>(
                            async c => { },
                            b => b.AddControlFlow("main")
                        )
                        .AddAction(
                            "main",
                            async c => Executed = true,
                            b => b.AddControlFlow<FinalNode>()
                        )
                        .AddFinal()
                    )
                )
                ;
        }

        [TestMethod]
        public async Task SimpleExecutionOK()
        {
            var initialized = false;
            var finalized = false;
            Executed = false;

            if (ActivityLocator.TryLocateActivity(new ActivityId("simple", "x"), out var a))
            {
                initialized = (await a.ExecuteAsync()).Status == EventStatus.Initialized;
                finalized = (await a.GetStatusAsync()).Response.BehaviorStatus == BehaviorStatus.Finalized;
            }

            Assert.IsTrue(initialized);
            Assert.IsTrue(finalized);
            Assert.IsTrue(Executed);
        }

        [TestMethod]
        public async Task SimpleExecutionFails()
        {
            var initialized = false;
            var finalized = false;
            Executed = false;

            if (ActivityLocator.TryLocateActivity(new ActivityId("simple", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
                Executed = false;
                initialized = (await a.ExecuteAsync()).Status == EventStatus.Initialized;
                finalized = (await a.GetStatusAsync()).Response.BehaviorStatus == BehaviorStatus.Finalized;
            }

            Assert.IsFalse(initialized);
            Assert.IsTrue(finalized);
            Assert.IsFalse(Executed);
        }

        [TestMethod]
        public async Task OutputExecutionOK()
        {
            var initialized = false;
            var finalized = false;
            var outputAvailable = false;
            var output = 0;
            Executed = false;

            if (ActivityLocator.TryLocateActivity(new ActivityId("output", "x"), out var a))
            {
                var result = await a.ExecuteAsync();
                outputAvailable = result.Response.TryGetOutputTokenOfType<int>(out output);
                initialized = result.Status == EventStatus.Initialized;
                finalized = (await a.GetStatusAsync()).Response.BehaviorStatus == BehaviorStatus.Finalized;
            }

            Assert.IsTrue(initialized);
            Assert.IsTrue(finalized);
            Assert.IsTrue(Executed);
            Assert.IsTrue(outputAvailable);
            Assert.AreEqual(42, output);
        }

        [TestMethod]
        public async Task InitializedExecutionOK()
        {
            var initialized = false;
            var finalized = false;
            Executed = false;

            if (ActivityLocator.TryLocateActivity(new ActivityId("initialized", "x"), out var a))
            {
                initialized = (await a.ExecuteAsync(new SomeEvent())).Status == EventStatus.Initialized;
                finalized = (await a.GetStatusAsync()).Response.BehaviorStatus == BehaviorStatus.Finalized;
            }

            Assert.IsTrue(initialized);
            Assert.IsTrue(finalized);
            Assert.IsTrue(Executed);
        }

        [TestMethod]
        public async Task InteractiveExecutionFailed()
        {
            var executed = false;
            var initialized = false;
            Executed = false;

            if (ActivityLocator.TryLocateActivity(new ActivityId("interactive", "x"), out var a))
            {
                executed = (await a.ExecuteAsync()).Status == EventStatus.NotInitialized;
                initialized = (await a.GetStatusAsync()).Response.BehaviorStatus == BehaviorStatus.Initialized;
            }

            Assert.IsFalse(executed);
            Assert.IsTrue(initialized);
            Assert.IsFalse(Executed);
        }
    }
}