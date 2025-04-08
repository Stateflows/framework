using Activity.IntegrationTests.Classes.Events;
using OneOf;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class AcceptEvent : StateflowsTestClass
    {
        private bool Executed = false;
        private int Counter1 = 0;
        private int Counter2 = 0;

        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddOneOf()
                .AddActivities(b => b
                    .AddActivity("acceptSingleEvent", b => b
                        .AddInitial(b => b
                            .AddControlFlow("final")
                            .AddControlFlow("accept")
                        )
                        .AddAcceptEventAction<SomeEvent>(
                            "accept",
                            async c => Counter1++,
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
                            async c => Counter1++,
                            b => b.AddControlFlow("final")
                        )
                        .AddAction("final", async c =>
                        {
                            Executed = true;
                        })
                    )
                    .AddActivity("acceptOneOfEvents", b => b
                        .AddInitial(b => b
                            .AddControlFlow("final")
                        )
                        .AddAcceptEventAction<OneOf<SomeEvent, OtherEvent>>(
                            "accept",
                            async c =>
                                c.Event.Match(
                                    some => Counter2++,
                                    other => Counter2++
                                ),
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
            Assert.AreEqual(1, Counter1);
            Assert.AreEqual(EventStatus.NotConsumed, result?.Status);
        }

        [TestMethod]
        public async Task AcceptInheritedEvent()
        {
            SendResult result = null;
            if (ActivityLocator.TryLocateActivity(new ActivityId("acceptSingleEvent", "x"), out var a))
            {
                await a.SendAsync(new SomeInheritedEvent());
                result = await a.SendAsync(new SomeInheritedEvent());
            }

            Assert.IsTrue(Executed);
            Assert.AreEqual(1, Counter1);
            Assert.AreEqual(EventStatus.NotConsumed, result?.Status);
        }

        [TestMethod]
        public async Task AcceptMultipleEvents()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("acceptMultipleEvents", "x"), out var a))
            {
                await a.SendAsync(new SomeEvent());
                await a.SendAsync(new SomeEvent());
            }

            Assert.IsTrue(Executed);
            Assert.AreEqual(2, Counter1);
        }

        [TestMethod]
        public async Task AcceptOneOfEvents()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("acceptOneOfEvents", "x"), out var a))
            {
                await a.SendAsync(new SomeEvent());
                await a.SendAsync(new OtherEvent());
            }

            Assert.IsTrue(Executed);
            Assert.AreEqual(2, Counter2);
        }
    }
}