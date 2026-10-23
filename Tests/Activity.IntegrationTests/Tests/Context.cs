using System;
using System.Threading.Tasks;
using System.ComponentModel;
using Activity.IntegrationTests.Classes.Events;
using Stateflows.Common;
using Stateflows.Entities.Attributes;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    public class FlowObserver : ActivityObserver
    {
        public override async Task OnNodeActivateAsync(IActivityNodeContext context, bool activated, Func<Task> next)
        {
            if (context.Node.Name.EndsWith("action3"))
            {
                Context.ActivationAttempted = true;
                Context.Activated1 = context.Node.IncomingFlows.First().Activated;
                Context.Activated2 = context.Node.IncomingFlows.Last().Activated;
            }
            await next();
        }
    }

    [TestClass]
    public class Context : StateflowsTestClass
    {
        public record Properties(string Name);
        
        public interface Entity
        {
            [Field, DefaultValue("Test")] string Name { get; set; }
            [Projection] string UpperName => Name.ToUpperInvariant();
            [Projection] Properties Props => new Properties(Name);
        }
        
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
                        .AddEntity<Entity>()
                        .SetPropertiesProjection<Entity, Properties>()
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
            BehaviorInfo? behaviorInfo = null;
            if (ActivityLocator.TryLocateActivity(new ActivityId("flow", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
                behaviorInfo = (await a.GetStatusAsync()).Response;
            }

            Assert.IsFalse(Activated1);
            Assert.IsTrue(Activated2);
            Assert.IsTrue(ActivationAttempted);
            Assert.AreNotEqual(true, Executed);
            Assert.IsNotNull(behaviorInfo);
            Assert.AreEqual("Test", behaviorInfo.Properties["Name"]);
        }
    }
}