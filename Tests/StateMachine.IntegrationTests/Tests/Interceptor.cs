using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
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

        public class Interceptor : StateMachineInterceptor
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
                .AddStateMachines(b => b
                    .AddStateMachine("simple", b => b
                        .AddInterceptor<Interceptor>()
                        .AddInitialState("initial", b => b
                            .AddOnEntry(c => Executed = true)
                            .AddDefaultTransition<FinalState>()
                        )
                        .AddFinalState()
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

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("simple", "x"), out var a))
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