using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    public class ValueInitializationRequest
    {
        public string Value { get; set; } = String.Empty;
    }

    [TestClass]
    public class Initialization : StateflowsTestClass
    {
        private bool Initialized = false;
        private bool Executed = false;
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

                    .AddActivity("value", b => b
                        .AddInitializer<ValueInitializationRequest>(async c =>
                        {
                            c.Behavior.Values.Set<string>("foo", c.InitializationEvent.Value);

                            return true;
                        })
                        .AddInitial(b => b
                            .AddControlFlow("action1")
                        )
                        .AddAction("action1", async c =>
                        {
                            if (c.Behavior.Values.TryGet<string>("foo", out var v))
                            {
                                Value = v;
                            }
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task SimpleInitialization()
        {
            var initialized = false;

            if (ActivityLocator.TryLocateActivity(new ActivityId("simple", "x"), out var a))
            {
                var result = await a.SendAsync(new Initialize());
                initialized = result.Status == EventStatus.Initialized;
            }

            Assert.IsTrue(initialized);
            Assert.IsTrue(Initialized);
            Assert.IsTrue(Executed);
        }

        [TestMethod]
        public async Task ValueInitialization()
        {
            var initialized = false;

            if (ActivityLocator.TryLocateActivity(new ActivityId("value", "x"), out var a))
            {
                initialized = (await a.SendAsync(new ValueInitializationRequest() { Value = "bar" })).Status == EventStatus.Initialized;
            }

            Assert.IsTrue(initialized);
            Assert.AreEqual("bar", Value);
        }
    }
}