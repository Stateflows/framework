using Stateflows.Common;
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
                    .AddStateMachine("scale", b => b
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
                .Select(i => StateMachineLocator.TryLocateStateMachine(new StateMachineId($"scale", i.ToString()), out var stateMachine)
                    ? stateMachine
                    : null
                )
                .Where(stateMachine => stateMachine is not null)
                .Select(stateMachine => stateMachine.SendAsync(new Initialize()))
                .ToArray();

            var badResults = (await Task.WhenAll(sequence)).Count(r => r.Status != EventStatus.Initialized);

            Assert.AreEqual(0, badResults);
        }
    }
}