using StateMachine.IntegrationTests.Utils;
using Stateflows.Actions;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Basic : StateflowsTestClass
    {
        public static bool ActionExecuted = false;
        
        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddActions(b => b
                    .AddAction("basic", async c => ActionExecuted = true)
                )
                ;
        }

        [TestMethod]
        public async Task ActivityActions()
        {
            if (ActionLocator.TryLocateAction(new ActionId("basic", "x"), out var a))
            {
                await a.ExecuteAsync();
            }
            
            Assert.IsTrue(ActionExecuted);
        }
    }
}