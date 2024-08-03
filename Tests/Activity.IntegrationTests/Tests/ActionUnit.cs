using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Classes;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    public class TestedAction : IActionNode
    {
        private readonly GlobalValue<int> Foo = new("foo");
        private readonly SingleInput<int> Input;

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Foo.Set(Input.Token);
            Input.PassOn();

            Unit.Executed = true;

            return Task.CompletedTask;
        }
    }

    public class TestedFlow : IFlowGuard<int>
    {
        private readonly GlobalValue<int> Foo = new("foo");

        public Task<bool> GuardAsync(int token)
        {
            Foo.Set(token);
            return Task.FromResult(Foo.TryGet(out var value) && value == 42);
        }
    }

    [TestClass]
    public class Unit : StateflowsTestClass
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
            // Registering activity also registers all of its node and flow classes;
            // yet, tested classes can be registered manually
            builder.ServiceCollection
                .AddTransient<TestedAction>()
                .AddTransient<TestedFlow>()
            ;
        }

        [TestMethod]
        public async Task ActionUnitTest()
        {
            // Use InputTokens static class to add tokens to be used by tested action class
            InputTokens.Add(42);

            // Use DI to obtain tested action class instance
            var action = ServiceProvider.GetRequiredService<TestedAction>();

            await action.ExecuteAsync(CancellationToken.None);

            // Use ContextValues static class to manage context values before or after running tested code
            var isSet = ContextValues.GlobalValues.TryGet<int>("foo", out var value);

            // Use OutputTokens static class to get tokens that are produced by tested action class
            var output = OutputTokens.Get<int>().FirstOrDefault();

            Assert.IsTrue(isSet);
            Assert.AreEqual(42, value);
            Assert.AreEqual(42, output);
            Assert.IsTrue(Executed);
        }

        [TestMethod]
        public async Task FlowUnitTest()
        {
            // Use DI to obtain tested flow class instance
            var flow = ServiceProvider.GetRequiredService<TestedFlow>();

            var result = await flow.GuardAsync(42);

            // Use ContextValues static class to manage context values before or after running tested code
            var isSet = ContextValues.GlobalValues.TryGet<int>("foo", out var value);

            Assert.IsTrue(isSet);
            Assert.AreEqual(42, value);
            Assert.IsTrue(result);
        }
    }
}