using Activity.IntegrationTests.Classes.Events;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    public class FlowObserver : ActivityObserver
    {
        public override void BeforeNodeActivate(IActivityNodeContext context, bool activated)
        {
            if (context.CurrentNode.Name.EndsWith("action3"))
            {
                Context.ActivationAttempted = true;
                Context.Activated1 = context.CurrentNode.IncomingFlows.First().Activated;
                Context.Activated2 = context.CurrentNode.IncomingFlows.Last().Activated;
            }
        }
    }

    [TestClass]
    public class Context : StateflowsTestClass
    {
        public static bool? ActivationAttempted = null;
        public static bool? Executed = null;
        public static bool? Activated1 = null;
        public static bool? Activated2 = null;

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
                    .AddActivity("flow", b => b
                        .AddObserver<FlowObserver>()
                        .AddInitial(b => b
                            .AddControlFlow("action1")
                            .AddControlFlow("action2")
                        )
                        .AddAction(
                            "action1",
                            async c => { },
                            b => b.AddFlow<SomeEvent>("action3")
                        )
                        .AddAction(
                            "action2",
                            async c => { },
                            b => b.AddControlFlow("action3")
                        )
                        .AddAction(
                            "action3",
                            async c => Executed = true
                        )
                    )
                )
                ;
        }

        [TestMethod]
        public async Task FlowContext()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("flow", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsFalse(Activated1);
            Assert.IsTrue(Activated2);
            Assert.IsTrue(ActivationAttempted);
            Assert.AreNotEqual(true, Executed);
        }
    }
}