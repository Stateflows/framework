using StateMachine.IntegrationTests.Utils;
using Stateflows.Actions;
using Stateflows.Activities;

namespace Action.IntegrationTests.Tests
{
    [TestClass]
    public class Basic : StateflowsTestClass
    {
        public static bool BasicActionExecuted = false;
        public static bool InputActionExecuted = false;
        public static bool MultipleInputActionExecuted = false;
        public static bool OutputActionExecuted = false;
        
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
                    .AddAction("basic", async c => BasicActionExecuted = true)
                    .AddAction("input", async c => InputActionExecuted = c.Action.GetTokensOfType<bool>().FirstOrDefault())
                    .AddAction("multipleInput", async c => MultipleInputActionExecuted = c.Action.GetTokensOfType<bool>().Count() == 3)
                    .AddAction("output", async c =>
                    {
                        OutputActionExecuted = true;
                        c.Action.Output(true);
                    })
                )
                ;
        }

        [TestMethod]
        public async Task BasicOK()
        {
            if (ActionLocator.TryLocateAction(new ActionId("basic", "x"), out var a))
            {
                await a.ExecuteAsync();
            }
            
            Assert.IsTrue(BasicActionExecuted);
        }

        [TestMethod]
        public async Task InputOK()
        {
            if (ActionLocator.TryLocateAction(new ActionId("input", "x"), out var a))
            {
                await a.SendInputAsync(b => b.Add(true));
            }
            
            Assert.IsTrue(InputActionExecuted);
        }

        [TestMethod]
        public async Task MultipleInputOK()
        {
            if (ActionLocator.TryLocateAction(new ActionId("multipleInput", "x"), out var a))
            {
                await a.SendInputAsync(b => b
                    .Add(true)
                    .Add(false)
                    .Add(false)
                );
            }
            
            Assert.IsTrue(MultipleInputActionExecuted);
        }

        [TestMethod]
        public async Task OutputOK()
        {
            TokensOutput output = null;
            if (ActionLocator.TryLocateAction(new ActionId("output", "x"), out var a))
            {
                var result = await a.ExecuteAsync();
                output = result.Response;
            }
            
            Assert.IsTrue(output != null && output.GetOfType<bool>().FirstOrDefault());
            Assert.IsTrue(OutputActionExecuted);
        }
    }
}