using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class AcceptEvent : StateflowsTestClass
    {
        private bool Executed = false;
        private int Counter = 0;

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
                    .AddActivity("acceptSingleEvent", b => b
                        .AddInitial(b => b
                            .AddControlFlow("final")
                            .AddControlFlow("accept")
                        )
                        .AddAcceptEventAction<SomeEvent>(
                            "accept",
                            async c => Counter++,
                            b => b.AddControlFlow("final")
                        )
                        .AddAction("final", async c =>
                        {
                            Executed = true;
                        })
                    )
                    .AddActivity("acceptMultipleEvents", b => b
                        .AddInitial(b => b
                            .AddControlFlow("final")
                        )
                        .AddAcceptEventAction<SomeEvent>(
                            "accept",
                            async c => Counter++,
                            b => b.AddControlFlow("final")
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
        public async Task AcceptSingleEvent()
        {
            SendResult result = null;
            if (ActivityLocator.TryLocateActivity(new ActivityId("acceptSingleEvent", "x"), out var a))
            {
                await a.SendAsync(new SomeEvent());
                result = await a.SendAsync(new SomeEvent());
            }

            Assert.IsTrue(Executed);
            Assert.AreEqual(1, Counter);
            Assert.AreEqual(EventStatus.NotConsumed, result?.Status);
        }

        [TestMethod]
        public async Task AcceptMultipleEvents()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("acceptMultipleEvents", "x"), out var a))
            {
                //await a.InitializeAsync();
                await a.SendAsync(new SomeEvent());
                await a.SendAsync(new SomeEvent());
            }

            Assert.IsTrue(Executed);
            Assert.AreEqual(2, Counter);
        }
    }
}