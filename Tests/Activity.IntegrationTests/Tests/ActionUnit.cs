using Stateflows.Common;
using Stateflows.Activities.Typed;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    public class TestedAction : ActionNode
    {
        private readonly GlobalValue<int> Foo = new("foo");
        private readonly SingleInput<int> Input;

        public override Task ExecuteAsync()
        {
            Foo.Set(Input.Token);
            ActionUnit.Executed = true;

            return Task.CompletedTask;
        }
    }

    public class TestedFlow : Flow<int>
    {
        public override Task<bool> GuardAsync()
        {
            return Task.FromResult(true);
        }
    }

    [TestClass]
    public class ActionUnit : StateflowsTestClass
    {
        public static bool? Executed = null;

        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder.AddActivities(b => b
                .AddActivity("unit", b => b
                    .AddInitial(b => b
                        .AddControlFlow("generate")
                    )
                    .AddAction(
                        "generate",
                        async c => c.Output(42),
                        b => b.AddFlow<int, TestedAction>()
                    )
                    .AddAction<TestedAction>()
                )
            );
        }

        [TestMethod]
        public async Task ActionUnitTest()
        {
            var cradle = SetupCradle<TestedAction>()
                .AddInputToken(42)
                .Build();

            var results = await cradle.SwingAsync();

            var isSet = results.TryGetGlobalContextValue<int>("foo", out var value);

            Assert.IsTrue(isSet);
            Assert.AreEqual(42, value);
            Assert.IsTrue(Executed);
        }
    }
}