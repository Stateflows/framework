using StateMachine.IntegrationTests.Utils;
using Stateflows.Actions;

namespace Action.IntegrationTests.Tests
{
    [TestClass]
    public class Startup : StateflowsTestClass
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
                    .AddAction("startup", async c => ActionExecuted = true)
                )
                .AddDefaultInstance(new ActionClass("startup"))
                ;
        }

        [TestMethod]
        public async Task StartupOK()
        {
            Assert.IsTrue(ActionExecuted);
        }
    }
}