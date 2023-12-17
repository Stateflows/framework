using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class Scale : StateflowsTestClass
    {
        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddStateMachines(b => b
                    .AddStateMachine("scale1", b => b
                        .AddInitialState("state1")
                        .AddFinalState()
                    )
                    .AddStateMachine("scale2", b => b
                        .AddInitialState("state1")
                        .AddFinalState()
                    )
                    .AddStateMachine("scale3", b => b
                        .AddInitialState("state1")
                        .AddFinalState()
                    )
                    .AddStateMachine("scale4", b => b
                        .AddInitialState("state1")
                        .AddFinalState()
                    )
                    .AddStateMachine("scale5", b => b
                        .AddInitialState("state1")
                        .AddFinalState()
                    )
                )
                ;
        }

        [TestMethod]
        public async Task ProcessingInScale()
        {
            var sequence = Enumerable
                .Range(0, 10000)
                .Select(i => Locator.TryLocateStateMachine(new StateMachineId($"scale{Random.Shared.Next(1, 5)}", i.ToString()), out var stateMachine)
                    ? stateMachine
                    : null
                )
                .Where(stateMachine => stateMachine is not null)
                .Select(stateMachine => stateMachine.InitializeAsync())
                .ToArray();

            var badResults = (await Task.WhenAll(sequence)).Count(r => !r.Response.InitializationSuccessful);

            Assert.AreEqual(0, badResults);
        }
    }
}