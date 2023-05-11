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
                .AddStateMachine("scale", b => b
                    .AddInitialState("state1")
                )
                ;
        }

        [TestMethod]
        public async Task ProcessingInScale()
        {
            var sequence = Enumerable
                .Range(0, 10000)
                .Select(i => Locator.TryLocateStateMachine(new StateMachineId("scale", i.ToString()), out var sm)
                    ? sm
                    : null
                )
                .Where(sm => sm is not null)
                .Select(sm => sm.InitializeAsync())
                .ToArray();

            var results = await Task.WhenAll(sequence);

            var badResults = results.Count(i => !i);

            Assert.AreEqual(0, badResults);
        }
    }
}