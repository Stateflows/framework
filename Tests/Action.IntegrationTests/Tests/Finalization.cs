using StateMachine.IntegrationTests.Utils;
using Stateflows.Actions;
using Stateflows.Actions.Context.Interfaces;

namespace Action.IntegrationTests.Tests
{
    public class Observer : ActionObserver
    {
        public override void BeforeActionInitialize(IActionDelegateContext context)
        {
            if (Finalization.InitializationCounter == 0)
            {
                Finalization.InitializationCounter++;   
            }
        }

        public override void AfterActionInitialize(IActionDelegateContext context)
        {
            if (Finalization.InitializationCounter == 1)
            {
                Finalization.InitializationCounter++;
            }
        }
        public override void BeforeActionFinalize(IActionDelegateContext context)
        {
            if (Finalization.FinalizationCounter == 0)
            {
                Finalization.FinalizationCounter++;   
            }
        }

        public override void AfterActionFinalize(IActionDelegateContext context)
        {
            if (Finalization.FinalizationCounter == 1)
            {
                Finalization.FinalizationCounter++;
            }
        }
    }
    
    [TestClass]
    public class Finalization : StateflowsTestClass
    {
        public static bool Action1Executed = false;
        public static bool Action2Executed = false;
        public static int InitializationCounter = 0;
        public static int FinalizationCounter = 0;
        
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
                    .AddObserver<Observer>()
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
        public async Task FinalizeOK()
        {
            if (ActionLocator.TryLocateAction(new ActionId("finalization", "x"), out var a))
            {
                _ = a.ExecuteAsync();
                
                await Task.Delay(100);
                
                await a.FinalizeAsync();
            }
            
            Assert.IsTrue(Action1Executed);
            Assert.IsFalse(Action2Executed);
            Assert.AreEqual(2, InitializationCounter);
            Assert.AreEqual(2, FinalizationCounter);
        }
    }
}