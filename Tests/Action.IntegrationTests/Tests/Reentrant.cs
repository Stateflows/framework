using StateMachine.IntegrationTests.Utils;
using Stateflows.Actions;
using Stateflows.Actions.Context.Interfaces;

namespace Action.IntegrationTests.Tests
{
    [TestClass]
    public class Reentrant : StateflowsTestClass
    {
        public int Counter1 = 0;
        public int Counter2 = 0;
        
        public static object LockObject = new object();
        
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
                    .AddAction("reentrant", ReentrantActionAsync)
                    .AddAction("non-reentrant", ReentrantActionAsync, false)
                )
                ;
        }
        
        private async Task ReentrantActionAsync(IActionDelegateContext c)
        {
            lock (LockObject)
            {
                Counter1++;
            }

            await Task.Delay(500);
                        
            lock (LockObject)
            {
                if (Counter1 == 3)
                {
                    Counter2++;
                }
            }
        }

        [TestMethod]
        public async Task ReentrantAction()
        {
            if (ActionLocator.TryLocateAction(new ActionId("reentrant", "x"), out var a))
            {
                await Task.WhenAll(
                    a.ExecuteAsync(),
                    a.ExecuteAsync(),
                    a.ExecuteAsync()
                );
            }
            
            Assert.AreEqual(3, Counter1);
            Assert.AreEqual(3, Counter2);
        }

        [TestMethod]
        public async Task NonReentrantAction()
        {
            if (ActionLocator.TryLocateAction(new ActionId("non-reentrant", "x"), out var a))
            {
                await Task.WhenAll(
                    a.ExecuteAsync(),
                    a.ExecuteAsync(),
                    a.ExecuteAsync()
                );
            }
            
            Assert.AreEqual(3, Counter1);
            Assert.AreEqual(1, Counter2);
        }
    }
}