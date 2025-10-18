using Activity.IntegrationTests.Classes.Events;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class DataStore : StateflowsTestClass
    {
        private int ExecutionCount = 0;
        private int TokenCount = 0;

        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddActivities(b => b
                    .AddActivity("dataStore", b => b
                        .AddInitial(b => b
                            .AddControlFlow("generate")
                        )
                        .AddAction(
                            "generate",
                            async c => c.OutputRange(Enumerable.Range(0, 10)),
                            b => b.AddFlow<int>("dataStore")
                        )
                        .AddDataStore("dataStore", b => b
                            .AddFlow<int>("final")
                        )
                        .AddAcceptEventAction<SomeEvent>(b => b
                            .AddControlFlow("final")
                        )
                        .AddAction("final", async c =>
                        {
                            ExecutionCount++;
                            TokenCount += c.GetTokensOfType<int>().Count();
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task DataStored()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("dataStore", "x"), out var a))
            {
                //await a.InitializeAsync();
                await a.SendAsync(new SomeEvent());
                await a.SendAsync(new SomeEvent());
                await a.SendAsync(new SomeEvent());
            }

            Assert.AreEqual(3, ExecutionCount);
            Assert.AreEqual(30, TokenCount);
        }
    }
}