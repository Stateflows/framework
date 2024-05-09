using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Exceptions : StateflowsTestClass
    {
        private bool Executed1 = false;
        private bool Executed2 = false;
        private static string Value = "boo";

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
                    .AddActivity("handled", b => b
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddStructuredActivity("main", b => b
                            .AddExceptionHandler<Exception>(async c =>
                            {
                                c.Output(c.Exception.Message);
                            })
                            .AddInitial(b => b
                                .AddControlFlow("action1")
                            )
                            .AddAction("action1",
                                async c =>
                                {
                                    c.Output(42);
                                    throw new Exception("test");
                                },
                                b => b.AddFlow<int>("action2")
                            )
                            .AddAction("action2",
                                async c =>
                                {
                                    Executed2 = true;
                                }
                            )
                            .AddFlow<string>("final")
                        )
                        .AddAction("final", async c =>
                        {
                            Executed1 = true;
                            Value = c.GetTokensOfType<string>().FirstOrDefault() ?? "foo";
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task ExceptionHandled()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("handled", "x"), out var a))
            {
                await a.InitializeAsync();
            }

            Assert.IsTrue(Executed1);
            Assert.IsFalse(Executed2);
            Assert.AreEqual("test", Value);
        }
    }
}