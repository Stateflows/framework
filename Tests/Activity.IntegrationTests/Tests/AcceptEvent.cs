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
        private int Counter3 = 0;

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
                    .AddActivity("acceptSingleEventAndTokens", b => b
                        .AddInitial(b => b
                            .AddControlFlow("final")
                            .AddControlFlow("generate")
                        )
                        .AddAction(
                            "generate",
                            async c => c.OutputRange(Enumerable.Range(0, 3)),
                            b => b.AddFlow<int>("accept")
                        )
                        .AddAcceptEventAction<SomeEvent>(
                            "accept",
                            async c => Counter3 += c.GetTokensOfType<int>().Sum(),
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
                    .AddActivity("structuredAcceptMultipleEventsAndTokens", b => b
                        .AddInitial(b => b
                            .AddControlFlow("structured")
                            .AddControlFlow("generate")
                        )
                        .AddAction(
                            "generate",
                            async c => c.OutputRange(Enumerable.Range(0, 3)),
                            b => b.AddFlow<int>("structured")
                        )
                        .AddStructuredActivity("structured", b => b
                            .AddInput(b => b
                                .AddFlow<int>("report")
                            )
                            .AddAcceptEventAction<SomeEvent>(
                                "accept",
                                async c => { },
                                b => b.AddControlFlow("report")
                            )
                            .AddAction("report", async c =>
                            {
                                Counter1++;
                                Executed = true;
                            })
                        )
                    )
                    .AddActivity("structuredAcceptMultipleEvents", b => b
                        .AddInitial(b => b
                            .AddControlFlow("structured")
                        )
                        .AddStructuredActivity("structured", b => b
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
        public async Task AcceptSingleEventAndTokens()
        {
            SendResult result = null;
            if (ActivityLocator.TryLocateActivity(new ActivityId("acceptSingleEventAndTokens", "x"), out var a))
            {
                await a.SendAsync(new SomeEvent());
                result = await a.SendAsync(new SomeEvent());
            }

            Assert.IsTrue(Executed);
            Assert.AreEqual(3, Counter3);
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
        public async Task StructuredAcceptMultipleEvents()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("structuredAcceptMultipleEvents", "x"), out var a))
            {
                await a.SendAsync(new SomeEvent());
                await a.SendAsync(new SomeEvent());
                var activityInfo = (await a.GetStatusAsync()).Response;
                Assert.IsTrue(activityInfo.ActiveNodes.Contains("accept"));
            }

            Assert.IsTrue(Executed);
            Assert.AreEqual(2, Counter1);
        }

        [TestMethod]
        public async Task StructuredAcceptMultipleEventsAndTokens()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("structuredAcceptMultipleEventsAndTokens", "x"), out var a))
            {
                await a.SendAsync(new SomeEvent());
                await a.SendAsync(new SomeEvent());
                var activityInfo = (await a.GetStatusAsync()).Response;
                Assert.IsTrue(activityInfo.ActiveNodes.Contains("accept"));
            }

            Assert.IsTrue(Executed);
            Assert.AreEqual(1, Counter1);
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