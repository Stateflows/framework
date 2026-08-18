using Activity.IntegrationTests.Classes.Events;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Classes;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Interception : StateflowsTestClass
    {
        public static bool Executed = false;
        public static bool Intercepted = false;
        public static bool GloballyIntercepted = false;

        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        public class GlobalInterceptor : BehaviorInterceptor
        {
            public override Task NotificationPublishedAsync<TNotification>(IBehaviorActionContext context, TNotification notification, IDictionary<string, EventHeader> headers)
            {
                GloballyIntercepted = true;
                
                return Task.CompletedTask;
            }
        }

        public class Interceptor : ActivityInterceptor
        {
            public override void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
            {
                Intercepted = true;
            }
        }

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddInterceptor<GlobalInterceptor>()
                .AddActivities(b => b
                    .AddActivity("simple", b => b
                        .AddInterceptor<Interceptor>()
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddAction(
                            "main",
                            async c =>
                            {
                                Executed = true;
                                c.Behavior.Publish(new SomeEvent());
                            },
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
                initialized = (await a.SendAsync(new Initialize())).Status == EventStatus.Initialized;
                finalized = (await a.GetStatusAsync()).Response.BehaviorStatus == BehaviorStatus.Finalized;
            }

            Assert.IsTrue(initialized);
            Assert.IsTrue(finalized);
            Assert.IsTrue(Executed);
            Assert.IsTrue(Intercepted);
            Assert.IsTrue(GloballyIntercepted);
        }
    }
}