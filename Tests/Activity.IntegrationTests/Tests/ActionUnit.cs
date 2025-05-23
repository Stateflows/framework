using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Common.Classes;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    public interface IServiceX
    {
        void DoSomething();
    }

    public class ServiceX : IServiceX
    {
        public void DoSomething()
        {
            Debug.WriteLine("DoSomething");
        }
    }
    
    public class TestedAction(IServiceX serviceX, IInputToken<int> input, [GlobalValue] IValue<int> foo) : IActionNode
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            serviceX.DoSomething();
            await foo.SetAsync(input.Token);
            input.PassOn();

            Unit.Executed = true;
        }
    }

    public class TestedFlow([GlobalValue] IValue<int> foo) : IFlowGuard<int>
    {
        public async Task<bool> GuardAsync(int token)
        {
            await foo.SetAsync(token);
            var (valueSet, value) = await foo.TryGetAsync();

            return valueSet && value == 42;
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
            builder.ServiceCollection.AddScoped<IServiceX, ServiceX>();

            // Run AddActivities to register necessary services
            builder.AddActivities();
        }

        [TestMethod]
        public async Task ActionUnitTest()
        {
            // Use InputTokens static class to add tokens to be used by tested action class
            InputTokens.Add(42);
            
            // If tested class uses context values, you need to initialize proper values collection
            ContextValues.InitializeGlobalValues();

            // Use StateflowsActivator to obtain tested action class instance
            var action = await StateflowsActivator.CreateInstanceAsync<TestedAction>(ServiceProvider);
            
            await action.ExecuteAsync(CancellationToken.None);

            // Use ContextValues static class to manage context values before or after running tested code
            var (isFooSet, fooValue) = await ContextValues.GlobalValues.TryGetAsync<int>("foo");

            // Use OutputTokens static class to get tokens that are produced by tested action class
            var output = OutputTokens.GetAllOfType<int>().FirstOrDefault();

            Assert.IsTrue(isFooSet);
            Assert.AreEqual(42, fooValue);
            Assert.AreEqual(42, output);
            Assert.IsTrue(Executed);
        }

        [TestMethod]
        public async Task FlowUnitTest()
        {
            // If tested class uses context values, you need to initialize proper values collection 
            ContextValues.InitializeGlobalValues();
            
            // Use StateflowsActivator to obtain tested flow class instance
            var flow = await StateflowsActivator.CreateInstanceAsync<TestedFlow>(ServiceProvider);

            var result = await flow.GuardAsync(42);

            // Use ContextValues static class to manage context values before or after running tested code
            var (isSet, value) = await ContextValues.GlobalValues.TryGetAsync<int>("foo");

            Assert.IsTrue(isSet);
            Assert.AreEqual(42, value);
            Assert.IsTrue(result);
        }
    }
}