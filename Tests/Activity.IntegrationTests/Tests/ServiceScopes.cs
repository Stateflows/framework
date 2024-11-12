using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    public class Service
    {
        public Service()
        {
            Value = Random.Shared.Next().ToString();
        }

        public readonly string Value;
    }

    public class ScopeAction1 : IActionNode
    {
        private readonly Service service;
        public ScopeAction1(Service service)
        {
            this.service = service;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            ServiceScopes.Value1 = service.Value;

            return Task.CompletedTask;
        }
    }

    public class ScopeAction2 : IActionNode
    {
        private readonly Service service;
        public ScopeAction2(Service service)
        {
            this.service = service;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            ServiceScopes.Value1 = service.Value;

            return Task.CompletedTask;
        }
    }

    [TestClass]
    public class ServiceScopes : StateflowsTestClass
    {
        public static string Value1 = string.Empty;
        public static string Value2 = string.Empty;

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
                    .AddActivity("scopes", b => b
                        .AddInitial(b => b
                            .AddControlFlow<ScopeAction1>()
                        )
                        .AddAction<ScopeAction1>(b => b
                            .AddControlFlow<ScopeAction2>()
                        )
                        .AddAction<ScopeAction2>()
                    )
                )

                .ServiceCollection.AddScoped<Service>()
                ;
        }

        [TestMethod]
        public async Task SeparateScopesOnActions()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("scopes", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.AreNotEqual(Value1, Value2);
        }
    }
}