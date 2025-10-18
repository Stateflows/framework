using Activity.IntegrationTests.Classes.Tokens;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    public class Struct : IStructuredActivityNode
    {

    }

    public class TypedAction(
        IInputTokens<SomeToken> someTokens,
        IInputTokens<string> strings,
        IOutputTokens<string> output,
        [GlobalValue("global")] IValue<string> value
    ) : IActionNode
    {

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Typed.TokenValue = someTokens.First().Foo;

            await value.SetAsync("foo");
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
                                .AddFlow<string>("string")
                            )
                            .AddAction("string", async c => { })
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
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed);
            Assert.AreEqual("bar", TokenValue);
        }
    }
}