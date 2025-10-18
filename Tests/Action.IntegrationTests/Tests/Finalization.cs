using StateMachine.IntegrationTests.Utils;
using Stateflows.Actions;

namespace Action.IntegrationTests.Tests
{
    [TestClass]
    public class Finalization : StateflowsTestClass
    {
        public static bool Action1Executed = false;
        public static bool Action2Executed = false;
        
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
                    .AddAction("finalization", async c =>
                    {
                        Action1Executed = true;
                        
                        await Task.Delay(200);
                        
                        if (c.CancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                        
                        Action2Executed = true;
                    })
                )
                ;
        }

        [TestMethod]
        public async Task StartupOK()
        {
            if (ActionLocator.TryLocateAction(new ActionId("finalization", "x"), out var a))
            {
                _ = a.ExecuteAsync();
                
                await Task.Delay(100);
                
                await a.FinalizeAsync();
            }
            
            Assert.IsTrue(Action1Executed);
            Assert.IsFalse(Action2Executed);
        }
    }
}