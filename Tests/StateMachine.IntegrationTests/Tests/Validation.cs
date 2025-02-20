using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using StateMachine.IntegrationTests.Classes.States;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    public class FluentEvent
    {
        public string Email { get; set; }
    }

    public class FluentEventValidator : AbstractValidator<FluentEvent>
    {
        public FluentEventValidator()
        {
            RuleFor(x => x.Email).EmailAddress();
        }
    }
    
    [TestClass]
    public class Validation : StateflowsTestClass
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
                .AddFluentValidation()
                .AddStateMachines(b => b
                    .AddStateMachine("validation", b => b
                        .AddInitialState("initial", b => b
                            .AddTransition<FluentEvent>("final")
                        )
                        .AddFinalState("final")
                    )
                )
                .ServiceCollection
                .AddSingleton<IValidator<FluentEvent>, FluentEventValidator>()
                ;
        }

        [TestMethod]
        public async Task FluentValidation()
        {
            string currentState = State<State1>.Name;
            SendResult result = null;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("validation", "x"), out var sm))
            {
                result = await sm.SendAsync(new FluentEvent() { Email = "user#example.com" });

                currentState = (await sm.GetCurrentStateAsync()).Response?.StatesTree?.Value;
            }

            Assert.AreEqual(EventStatus.Invalid, result?.Status);
            Assert.IsFalse(result?.Validation.IsValid);
            Assert.AreEqual(1, result?.Validation.ValidationResults.Count());
            Assert.AreEqual(nameof(FluentEvent.Email), result?.Validation.ValidationResults.First().MemberNames.First());
            Assert.AreEqual("initial", currentState);
        }
    }
}