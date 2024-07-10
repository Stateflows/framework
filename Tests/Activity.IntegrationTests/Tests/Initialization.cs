using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Activities.Typed;
using Stateflows.Activities.Context.Interfaces;

namespace Activity.IntegrationTests.Tests
{
    public class ValueInitializationRequest : Event
    {
        public string Value { get; set; } = String.Empty;
    }

    [TestClass]
    public class Initialization : StateflowsTestClass
    {
        private bool Initialized = false;
        private bool Executed = false;
        private bool Accepted = false;
        public static string Value = "boo";

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
                        .AddDefaultInitializer(async c =>
                        {
                            Initialized = true;

                            return true;
                        })
                        .AddInitial(b => b
                            .AddControlFlow("action1")
                        )
                        .AddAction("action1", async c => Executed = true)
                    )

                    .AddActivity("auto", b => b
                        .AddDefaultInitializer(async c =>
                        {
                            Initialized = true;

                            return true;
                        })
                        .AddInitial(b => b
                            .AddControlFlow("action1")
                        )
                        .AddAction("action1", async c => Executed = true)
                        .AddAcceptEventAction<SomeEvent>(async c => Accepted = true)
                    )

                    .AddActivity("value", b => b
                        .AddInitializer<ValueInitializationRequest>(async c =>
                        {
                            c.Activity.Values.Set<string>("foo", c.InitializationEvent.Value);

                            return true;
                        })
                        .AddInitial(b => b
                            .AddControlFlow("action1")
                        )
                        .AddAction("action1", async c =>
                        {
                            if (c.Activity.Values.TryGet<string>("foo", out var v))
                            {
                                Value = v;
                            }
                        })
                    )
                )
                //.AddAutoInitialization(new ActivityClass("auto"))
                ;
        }

        [TestMethod]
        public async Task SimpleInitialization()
        {
            var initialized = false;

            if (ActivityLocator.TryLocateActivity(new ActivityId("simple", "x"), out var a))
            {
                var result = await a.InitializeAsync();
                initialized = result.Status == EventStatus.Initialized;
            }

            Assert.IsTrue(initialized);
            Assert.IsTrue(Initialized);
            Assert.IsTrue(Executed);
        }

        //[TestMethod]
        //public async Task AutoInitialization()
        //{
        //    EventStatus status = EventStatus.NotConsumed;

        //    if (ActivityLocator.TryLocateActivity(new ActivityId("auto", "x"), out var a))
        //    {
        //        status = (await a.SendAsync(new SomeEvent())).Status;
        //    }

        //    Assert.AreEqual(EventStatus.Consumed, status);
        //    Assert.IsTrue(Initialized);
        //    Assert.IsTrue(Executed);
        //    Assert.IsTrue(Accepted);
        //}

        //[TestMethod]
        //public async Task NoAutoInitialization()
        //{
        //    EventStatus status = EventStatus.NotConsumed;

        //    if (ActivityLocator.TryLocateActivity(new ActivityId("simple", "x"), out var a))
        //    {
        //        status = (await a.SendAsync(new SomeEvent())).Status;
        //    }

        //    Assert.AreEqual(EventStatus.NotConsumed, status);
        //    Assert.IsFalse(Initialized);
        //    Assert.IsFalse(Executed);
        //    Assert.IsFalse(Accepted);
        //}

        [TestMethod]
        public async Task ValueInitialization()
        {
            var initialized = false;

            if (ActivityLocator.TryLocateActivity(new ActivityId("value", "x"), out var a))
            {
                initialized = (await a.InitializeAsync(new ValueInitializationRequest() { Value = "bar" })).Status == EventStatus.Initialized;
            }

            Assert.IsTrue(initialized);
            Assert.AreEqual("bar", Value);
        }
    }
}