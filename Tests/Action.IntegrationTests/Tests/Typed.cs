using StateMachine.IntegrationTests.Utils;
using Stateflows.Actions;
using Stateflows.Activities;

namespace Action.IntegrationTests.Tests
{
    public class TypedAction : IAction
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Typed.TypedExecuted = true;

            return Task.CompletedTask;
        }
    }
    
    public class InputAction : IAction
    {
        private readonly IInputTokens<bool> BoolTokens;
        
        public InputAction(IInputTokens<bool> boolTokens)
        {
            BoolTokens = boolTokens;
        }
        
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Typed.InputExecuted = BoolTokens.FirstOrDefault();

            return Task.CompletedTask;
        }
    }
    
    [TestClass]
    public class Typed : StateflowsTestClass
    {
        public static bool TypedExecuted = false;
        public static bool InputExecuted = false;
        
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
                    .AddAction<TypedAction>()
                    .AddAction<InputAction>()
                )
                ;
        }

        [TestMethod]
        public async Task TypedAction()
        {
            if (ActionLocator.TryLocateAction(new ActionId(Stateflows.Actions.Action<TypedAction>.Name, "x"), out var a))
            {
                await a.ExecuteAsync();
            }
            
            Assert.IsTrue(TypedExecuted);
        }

        [TestMethod]
        public async Task TypedActionWithInput()
        {
            if (ActionLocator.TryLocateAction(new ActionId(Stateflows.Actions.Action<InputAction>.Name, "x"), out var a))
            {
                await a.SendInputAsync(b => b
                    .Add(true)
                );
            }
            
            Assert.IsTrue(InputExecuted);
        }
    }
}