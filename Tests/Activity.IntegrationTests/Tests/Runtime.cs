using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class Runtime : StateflowsTestClass
    {
        private bool Executed = false;

        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder.AddActivities();
        }

        [TestMethod]
        public async Task RuntimeRegistration()
        {
            var register = ServiceProvider.GetRequiredService<IActivitiesRegister>();
            register.AddActivity("runtime", b => b
                .AddInitial(b => b
                    .AddControlFlow("initial")
                )
                .AddAction("initial", async c => Executed = true)
            );

            if (ActivityLocator.TryLocateActivity(new ActivityId("runtime", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed);
        }
    }
}