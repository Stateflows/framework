using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Events;
using Stateflows.StateMachines;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class StructuralEvents : StateflowsTestClass
{
    private IEventFactory Factory => ServiceProvider.GetRequiredService<IEventFactory>();
    private string? accepted;

    public class ClrResponse
    {
        [Required]
        public string OrderId { get; set; } = "";
    }

    public sealed class DerivedClrResponse : ClrResponse;

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder
            .AddEvents(b => b
                .AddEvent("submit", b => b
                    .AddField("orderId", FieldType.String, f => f.AddValidation(new RequiredAttribute()))
                    .AddField("quantity", FieldType.Int32, f => f.AddValidation(new RangeAttribute(1, 10)))
                    .AddObjectField("address", b => b.AddField("city", FieldType.String))
                    .AddCollectionField("tags", FieldType.String, b => b
                        .AddValidation(new MaxLengthAttribute(2))
                        .AddItemValidation(new StringLengthAttribute(5)))
                    .AddCollectionField("items", b => b
                        .AddField("code", FieldType.String, f => f.AddValidation(new RequiredAttribute()))))
                .AddEvent("other", b => b.AddField("orderId", FieldType.String))
                .AddEvent("response", b => b.AddField("orderId", FieldType.String, f => f.AddValidation(new RequiredAttribute())))
                .AddRequest("query", "response", b => b.AddField("orderId", FieldType.String))
                .AddRequest<ClrResponse>("clrQuery", b => b.AddField("orderId", FieldType.String)))
            .AddStateMachines(b => b
                .AddStateMachine("structural", b => b
                    .AddInitialState("waiting", b => b
                        .AddTransition("submit", "done", b => b
                            .AddGuard(c => c.Event.Get<int>("quantity") > 0)))
                    .AddState("done"))
                .AddStateMachine("structuralRequests", b => b
                    .AddInitialState("waiting", b => b
                        .AddInternalTransition("query", b => b.AddEffect(c =>
                            c.Event.Respond(c.Behavior.ServiceProvider.GetRequiredService<IEventFactory>()
                                .Create("response", b => b.Set("orderId", c.Event.Get<string>("orderId"))))))
                        .AddInternalTransition("clrQuery", b => b.AddEffect(c =>
                            c.Event.Respond(new DerivedClrResponse { OrderId = c.Event.Get<string>("orderId") })))))
                .AddStateMachine("structuralDeferred", b => b
                    .AddInitialState("waiting", b => b
                        .AddDeferredEvent("submit")
                        .AddTransition("other", "ready"))
                    .AddState("ready", b => b.AddTransition("submit", "done"))
                    .AddState("done"))
                .AddStateMachine("structuralElse", b => b
                    .AddInitialState("waiting", b => b
                        .AddTransition("submit", "normal", b => b.AddGuard(_ => false))
                        .AddElseTransition("submit", "fallback"))
                    .AddState("normal")
                    .AddState("fallback")))
            .AddActivities(b => b.AddActivity("structuralActivity", b => b
                .AddInitial(b => b.AddControlFlow("receive"))
                .AddAcceptEventAction("submit", "receive", c =>
                {
                    accepted = c.Event.Get<string>("orderId");
                    return Task.CompletedTask;
                }, b => b.AddControlFlow("done"))
                .AddAction("done", _ => Task.CompletedTask)));

    [TestMethod]
    public async Task RoutesByNameAndValidatesBeforeTransition()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("structural", "one"), out var machine));
        await machine.SendAsync(new Initialize());
        CollectionAssert.Contains((await machine.GetStatusAsync()).Response.ExpectedEvents.ToArray(), "submit");
        Assert.AreEqual(EventStatus.NotConsumed, (await machine.SendAsync(Factory.Create("other", b => b.Set("orderId", "123")))).Status);
        var invalid = await machine.SendAsync(Factory.Create("submit", b => b.Set("quantity", 0)));
        Assert.AreEqual(EventStatus.Invalid, invalid.Status);
        Assert.IsTrue(invalid.Validation.ValidationResults.Any());
        var valid = await machine.SendAsync(Factory.Create("submit", b => b.Set("orderId", "123").Set("quantity", 1)));
        Assert.AreEqual(EventStatus.Consumed, valid.Status);
        Assert.AreEqual("done", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
    }

    [TestMethod]
    public void PreservesMissingNullAndStrictTypes()
    {
        var value = Factory.Create("submit", b => b.Set<string?>("orderId", null));
        Assert.IsTrue(value.ContainsField("orderId"));
        Assert.IsNull(value.Get<string?>("orderId"));
        Assert.IsFalse(value.ContainsField("quantity"));
        Assert.IsFalse(value.TryGet<int>("quantity", out _));
        Assert.ThrowsExactly<KeyNotFoundException>(() => value.Get<int>("quantity"));
        Assert.ThrowsExactly<ArgumentException>(() => Factory.Create("submit", b => b.Set("quantity", "1")));
    }

    [TestMethod]
    public void DiscardsUnknownFieldsRecursivelyAndSupportsCollections()
    {
        var value = Factory.Create("submit", b => b
            .Set("unknown", 42)
            .SetObject("address", b => b.Set("city", "London").Set("unknown", true))
            .Set("tags", new[] { "one", "two" })
            .SetObjectCollection("items", b => b.Set("code", "A")));
        Assert.IsFalse(value.ContainsField("unknown"));
        Assert.AreEqual("London", value.Get<StructuralValue>("address").Get<string>("city"));
        Assert.IsFalse(value.Get<StructuralValue>("address").ContainsField("unknown"));
        CollectionAssert.AreEqual(new[] { "one", "two" }, value.GetCollection<string>("tags").ToArray());
        Assert.AreEqual("A", value.GetCollection<StructuralValue>("items")[0].Get<string>("code"));
    }

    [TestMethod]
    public async Task SupportsStructuralAndClrResponses()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("structuralRequests", "one"), out var machine));
        await machine.SendAsync(new Initialize());
        var structural = await machine.RequestAsync(Factory.Create("query", b => b.Set("orderId", "123")));
        Assert.AreEqual(EventStatus.Consumed, structural.Status);
        Assert.AreEqual("123", structural.Response.Get<string>("orderId"));
        var clr = await machine.RequestAsync<ClrResponse>(Factory.Create("clrQuery", b => b.Set("orderId", "456")));
        Assert.AreEqual(EventStatus.Consumed, clr.Status);
        Assert.AreEqual("456", clr.Response.OrderId);
        Assert.IsInstanceOfType<DerivedClrResponse>(clr.Response);
    }

    [TestMethod]
    public void RejectsWrongAndInvalidResponses()
    {
        var structural = (StructuralRequest)Factory.Create("query", _ => { });
        Assert.ThrowsExactly<InvalidOperationException>(() => structural.Respond(Factory.Create("other", _ => { })));
        Assert.ThrowsExactly<System.ComponentModel.DataAnnotations.ValidationException>(() => structural.Respond(Factory.Create("response", _ => { })));
        var clr = (StructuralRequest)Factory.Create("clrQuery", _ => { });
        Assert.ThrowsExactly<InvalidOperationException>(() => clr.Respond("wrong"));
        Assert.ThrowsExactly<System.ComponentModel.DataAnnotations.ValidationException>(() => clr.Respond(new ClrResponse()));
    }

    [TestMethod]
    [DataRow(typeof(Initialize))]
    [DataRow(typeof(NotificationsRequest))]
    [DataRow(typeof(Subscribe))]
    [DataRow(typeof(TokensInput))]
    public void RejectsBuiltInEventNameCollisions(Type eventType)
        => Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddStateflows(b => b
            .AddEvents(b => b.AddEvent(Event.GetName(eventType), _ => { }))));

    [TestMethod]
    public async Task AcceptsNamedEventInActivity()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("structuralActivity", "one"), out var activity));
        await activity.SendAsync(new Initialize());
        Assert.AreEqual(EventStatus.NotConsumed, (await activity.SendAsync(Factory.Create("other", _ => { }))).Status);
        var result = await activity.SendAsync(Factory.Create("submit", b => b.Set("orderId", "123").Set("quantity", 1)));
        Assert.AreEqual(EventStatus.Consumed, result.Status);
        Assert.AreEqual("123", accepted);
    }

    [TestMethod]
    public void SerializationPreservesIdentityAndValues()
    {
        var original = Factory.Create("submit", b => b.Set("quantity", 3)
            .SetObject("address", b => b.Set("city", "London"))
            .Set("tags", new[] { "one" }));
        var roundtrip = (StructuralEvent)StateflowsJsonConverter.DeserializeObject(
            StateflowsJsonConverter.SerializePolymorphicObject(original));
        Assert.AreEqual("submit", roundtrip.Name);
        Assert.AreEqual(3, roundtrip.Get<int>("quantity"));
        Assert.AreEqual("London", roundtrip.Get<StructuralValue>("address").Get<string>("city"));
        Assert.AreEqual("one", roundtrip.GetCollection<string>("tags")[0]);
        Assert.AreEqual("submit", roundtrip.ToTypedEventHolder().Name);
    }

    [TestMethod]
    public async Task RevalidatingPersistedPayloadDiscardsUndeclaredFields()
    {
        var original = Factory.Create("submit", b => b.Set("orderId", "123").Set("quantity", 3));
        var document = Newtonsoft.Json.Linq.JObject.Parse(StateflowsJsonConverter.SerializePolymorphicObject(original));
        var fields = (Newtonsoft.Json.Linq.JObject)document["values"]!;
        fields["undeclared"] = fields["quantity"]!.DeepClone();
        var restored = (StructuralEvent)StateflowsJsonConverter.DeserializeObject(document.ToString());
        Assert.IsTrue(restored.ContainsField("undeclared"));

        var errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        Assert.IsTrue(await ((IStateflowsValidator)Factory).ValidateAsync(restored, errors));
        Assert.IsFalse(restored.ContainsField("undeclared"));
        Assert.AreEqual(3, restored.Get<int>("quantity"));
    }

    [TestMethod]
    public async Task DeferredEventRetainsItsContractAndPayload()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("structuralDeferred", "one"), out var machine));
        await machine.SendAsync(new Initialize());
        var deferred = await machine.SendAsync(Factory.Create("submit", b => b.Set("orderId", "123").Set("quantity", 1)));
        Assert.AreEqual(EventStatus.Deferred, deferred.Status);
        await machine.SendAsync(Factory.Create("other", _ => { }));
        Assert.AreEqual("done", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
    }

    [TestMethod]
    public async Task ElseTransitionMatchesNamedContract()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("structuralElse", "one"), out var machine));
        var result = await machine.SendAsync(Factory.Create("submit", b => b.Set("orderId", "123").Set("quantity", 1)));
        Assert.AreEqual(EventStatus.Consumed, result.Status);
        Assert.AreEqual("fallback", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
    }

    [TestMethod]
    public async Task ReportsNestedAndCollectionValidationPaths()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("structural", "invalid"), out var machine));
        var result = await machine.SendAsync(Factory.Create("submit", b => b
            .Set("orderId", "123")
            .Set("quantity", 1)
            .Set("tags", new[] { "too long" })
            .SetObjectCollection("items", _ => { })));
        Assert.AreEqual(EventStatus.Invalid, result.Status);
        var members = result.Validation.ValidationResults.SelectMany(r => r.MemberNames).ToArray();
        CollectionAssert.Contains(members, "tags[0]");
        CollectionAssert.Contains(members, "items[0].code");
    }

    [TestMethod]
    public void RejectsDuplicateAndUnknownResponseDefinitions()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddStateflows(b => b
            .AddEvents(b => b.AddEvent("duplicate", _ => { }).AddEvent("duplicate", _ => { }))));
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddStateflows(b => b
            .AddEvents(b => b.AddRequest("query", "unknown", _ => { }))));
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddStateflows(b => b
            .AddEvents(b => b.AddEvent(Event<Initialize>.Name, _ => { }))));
    }

    [TestMethod]
    public void DefinitionsCannotBeRegisteredAfterConfiguration()
    {
        IEventsBuilder? retained = null;
        new ServiceCollection().AddStateflows(b => b.AddEvents(b => retained = b));
        Assert.IsNotNull(retained);
        Assert.ThrowsExactly<InvalidOperationException>(() => retained.AddEvent("late", _ => { }));
    }

    [TestMethod]
    public void RejectsClrNameCollisionsInEitherRegistrationOrder()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddStateflows(b => b
            .AddEvents(b => b.AddEvent(Event<ClrResponse>.Name, _ => { }))
            .AddStateMachines(b => b.AddStateMachine("collision", b => b
                .AddInitialState("one", b => b.AddTransition<ClrResponse>("two"))
                .AddState("two")))));
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddStateflows(b => b
            .AddStateMachines(b => b.AddStateMachine("collision", b => b
                .AddInitialState("one", b => b.AddTransition<ClrResponse>("two"))
                .AddState("two")))
            .AddEvents(b => b.AddEvent(Event<ClrResponse>.Name, _ => { }))));
    }

    [TestMethod]
    public async Task FluentValidationAppliesStructuralBaseValidatorsToRequests()
    {
        var services = new ServiceCollection();
        var validator = new FluentValidation.InlineValidator<StructuralEvent>();
        validator.RuleFor(value => value.Name).Must(name => name != "query");
        services.AddSingleton<FluentValidation.IValidator<StructuralEvent>>(validator);
        using var provider = services.BuildServiceProvider();
        var results = new List<ValidationResult>();
        var request = (StructuralRequest)Factory.Create("query", b => b.Set("orderId", "123"));
        var valid = await new Stateflows.Extensions.FluentValidation.FluentValidator(provider)
            .ValidateAsync(request, results);
        Assert.IsFalse(valid);
        Assert.HasCount(1, results);
    }
}
