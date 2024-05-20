using Activity.IntegrationTests.Classes.Tokens;
using Stateflows.Activities.Typed;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;
using System.Diagnostics;

namespace Activity.IntegrationTests.Tests
{
    public class TypedAction : ActionNode
    {
        internal readonly Input<SomeToken> someTokens;
        internal readonly Input<string> strings;

        public override Task ExecuteAsync()
        {
            Typed.TokenValue = someTokens.First().Foo;
            
            return Task.CompletedTask;
        }
    }

    [TestClass]
    public class Typed : StateflowsTestClass
    {
        private bool Executed = false;
        public static string TokenValue = string.Empty;

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
                    .AddActivity("typed", b => b
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddStructuredActivity("main", b => b
                            .AddInitial(b => b
                                .AddControlFlow("initial")
                            )
                            .AddAction(
                                "initial",
                                async c =>
                                {
                                    c.Output(new SomeToken() { Foo = "bar" });
                                    c.Output("boo");
                                },
                                b => b
                                    .AddFlow<SomeToken, TypedAction>()
                                    .AddFlow<string, TypedAction>()
                            )
                            .AddAction<TypedAction>(b => b
                                .AddControlFlow("final")
                            )
                            .AddAction("final", async c => Executed = true)
                        )
                    )
                )
                ;
        }

        [TestMethod]
        public async Task InputVerification()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("typed", "x"), out var a))
            {
                await a.InitializeAsync();
            }

            Assert.IsTrue(Executed);
            Assert.AreEqual("bar", TokenValue);
        }
    }
}