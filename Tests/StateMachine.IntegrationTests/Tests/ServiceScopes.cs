using StateMachine.IntegrationTests.Classes.StateMachines;
using StateMachine.IntegrationTests.Classes.States;
using StateMachine.IntegrationTests.Utils;
using Stateflows.StateMachines;
using Stateflows.Common;
using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Classes.Events;

namespace StateMachine.IntegrationTests.Tests
{
    public class Service
    {
        public Service()
        {
            Value = Random.Shared.Next().ToString();
        }

        public readonly string Value;
    }

    public class ScopeState : IStateEntry, IStateExit
    {
        private readonly Service service;
        public ScopeState(Service service)
        {
            this.service = service;
        }

        public Task OnEntryAsync()
        {
            ServiceScopes.EntryValue = service.Value;

            return Task.CompletedTask;
        }

        public Task OnExitAsync()
        {
            ServiceScopes.ExitValue = service.Value;

            return Task.CompletedTask;
        }
    }

    public class Some : ITransitionEffect<SomeEvent>
    {
        private readonly Service service;
        public Some(Service service)
        {
            this.service = service;
        }

        public Task EffectAsync(SomeEvent @event)
        {
            ServiceScopes.SomeValue = service.Value;

            return Task.CompletedTask;
        }
    }

    public class Other : ITransitionEffect<OtherEvent>
    {
        private readonly Service service;
        public Other(Service service)
        {
            this.service = service;
        }

        public Task EffectAsync(OtherEvent @event)
        {
            ServiceScopes.OtherValue = service.Value;

            return Task.CompletedTask;
        }
    }

    [TestClass]
    public class ServiceScopes : StateflowsTestClass
    {
        public static string SomeValue = string.Empty;
        public static string OtherValue = string.Empty;
        public static string EntryValue = string.Empty;
        public static string ExitValue = string.Empty;

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
                    .AddStateMachine("compound", b => b
                        .AddInitialState("initial", b => b
                            .AddInternalTransition<SomeEvent>(b => b
                                .AddEffect<Some>()
                            )
                            .AddInternalTransition<OtherEvent>(b => b
                                .AddEffect<Other>()
                            )
                        )
                    )

                    .AddStateMachine("state", b => b
                        .AddInitialState<ScopeState>(b => b
                            .AddDefaultTransition("state2")
                        )
                        .AddState("state2")
                    )
                )

                .ServiceCollection.AddScoped<Service>()
                ;
        }

        [TestMethod]
        public async Task SeparateScopesOnCompoundEvents()
        {
            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("compound", "x"), out var sm))
            {
                var compoundRequest = new CompoundRequest()
                    .Add(new SomeEvent())
                    .Add(new OtherEvent());

                await sm.SendAsync(compoundRequest);
            }

            Assert.AreNotEqual(ServiceScopes.SomeValue, ServiceScopes.OtherValue);
        }

        [TestMethod]
        public async Task SeparateScopesOnStateEvents()
        {
            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("state", "x"), out var sm))
            {
                await sm.SendAsync(new Initialize());
            }

            Assert.AreNotEqual(ServiceScopes.EntryValue, ServiceScopes.ExitValue);
        }
    }
}