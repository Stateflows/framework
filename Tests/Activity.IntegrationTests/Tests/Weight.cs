using Activity.IntegrationTests.Classes.Events;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Weight : StateflowsTestClass
    {
        private bool Executed = false;

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
                    .AddActivity("optional", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction(
                            "generate",
                            async c => { },
                            b => b.AddFlow<SomeEvent>("final", b => b.SetWeight(0))
                        )
                        .AddAction("final", async c =>
                        {
                            Executed = true;
                        })
                    )
                    .AddActivity("required", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction(
                            "generate",
                            async c => c.OutputRange(Enumerable.Range(0, 5)),
                            b => b.AddFlow<int>("final", b => b.SetWeight(10))
                        )
                        .AddAction("final", async c =>
                        {
                            Executed = true;
                        })
                    )
                    .AddActivity("gradual", b => b
                        .AddInitial(b => b
                            .AddControlFlow(MergeNode.Name)
                        )
                        .AddAcceptEventAction<SomeEvent>(b => b
                            .AddControlFlow(MergeNode.Name)
                        )
                        .AddMerge(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction(
                            "generate",
                            async c => c.OutputRange<int>(Enumerable.Range(0, 5)),
                            b => b.AddFlow<int>("final", b => b.SetWeight(15))
                        )
                        .AddAction("final", async c =>
                        {
                            Executed = true;
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task OptionalFlow()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("optional", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed);
        }

        [TestMethod]
        public async Task RequiredFlow()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("required", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsFalse(Executed);
        }

        [TestMethod]
        public async Task GradualFlow()
        {
            var executed1 = false;
            var executed2 = false;
            var executed3 = false;
            if (ActivityLocator.TryLocateActivity(new ActivityId("gradual", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
                executed1 = Executed;
                await a.SendAsync(new SomeEvent());
                executed2 = Executed;
                await a.SendAsync(new SomeEvent());
                executed3 = Executed;
                await a.SendAsync(new SomeEvent());
            }

            Assert.IsFalse(executed1);
            Assert.IsFalse(executed2);
            Assert.IsTrue(executed3);
            Assert.IsTrue(Executed);
        }
    }
}