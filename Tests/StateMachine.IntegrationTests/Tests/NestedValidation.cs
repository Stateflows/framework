using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using StateMachine.IntegrationTests.Classes.States;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    // thanks to KyleMit
    // code taken from here (with small adjustment):
    // https://stackoverflow.com/questions/2493800/how-can-i-tell-the-data-annotations-validator-to-also-validate-complex-child-pro/28433753#28433753
    public class ValidateObjectAttribute: ValidationAttribute {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(value, null, null);

            Validator.TryValidateObject(value, context, results, true);

            if (results.Count != 0) {
                var compositeResults = new CompositeValidationResult(String.Format("Validation for {0} failed!", validationContext.DisplayName),
                    new [] { validationContext.MemberName });
                results.ForEach(compositeResults.AddResult);

                return compositeResults;
            }

            return ValidationResult.Success;
        }
    }
    
    public class CompositeValidationResult: ValidationResult {
        private readonly List<ValidationResult> _results = new List<ValidationResult>();

        public IEnumerable<ValidationResult> Results {
            get {
                return _results;
            }
        }

        public CompositeValidationResult(string errorMessage) : base(errorMessage) {}
        public CompositeValidationResult(string errorMessage, IEnumerable<string> memberNames) : base(errorMessage, memberNames) {}
        protected CompositeValidationResult(ValidationResult validationResult) : base(validationResult) {}

        public void AddResult(ValidationResult validationResult) {
            _results.Add(validationResult);
        }
    }

    public class WrapperEvent<T>
    {
        [ValidateObject]
        public T Data { get; set; }
    }
    
    [TestClass]
    public class NestedValidation : StateflowsTestClass
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
                            .AddTransition<WrapperEvent<AttributeEvent>>("final")
                        )
                        .AddFinalState("final")
                    )
                )
                .ServiceCollection
                .AddSingleton<IValidator<FluentEvent>, FluentEventValidator>()
                ;
        }

        [TestMethod]
        public async Task NestedAttributeValidation()
        {
            string currentState = State<State1>.Name;
            SendResult result1 = null;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("validation", "x"), out var sm))
            {
                result1 = await sm.SendAsync(new WrapperEvent<AttributeEvent>() { Data = new AttributeEvent() { Email = "user#example.com" }});

                currentState = (await sm.GetStatusAsync()).Response?.CurrentStates?.Value;
            }

            Assert.AreEqual(EventStatus.Invalid, result1?.Status);
            
            Assert.IsFalse(result1?.Validation.IsValid);
            Assert.AreEqual(1, result1?.Validation.ValidationResults.Count());
            Assert.AreEqual(nameof(WrapperEvent<AttributeEvent>.Data), result1?.Validation.ValidationResults.First().MemberNames.First());
            Assert.AreEqual(nameof(AttributeEvent.Email), (result1?.Validation.ValidationResults.First() as CompositeValidationResult).Results.First().MemberNames.First());

            Assert.AreEqual("initial", currentState);
        }
    }
}