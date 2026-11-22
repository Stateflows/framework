using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Payloads;
using Stateflows.StateMachines;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class Payloads : StateflowsTestClass
{
    private IPayloadFactory Factory => ServiceProvider.GetRequiredService<IPayloadFactory>();
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
            .AddPayloads(b => b
                .AddPayload("submit", b => b
                    .AddField("orderId", FieldType.String, f => f.AddValidation(new RequiredAttribute()))
                    .AddField("quantity", FieldType.Int32, f => f.AddValidation(new RangeAttribute(1, 10)))
                    .AddObjectField("address", b => b.AddField("city", FieldType.String))
                    .AddCollectionField("tags", FieldType.String, b => b
                        .AddValidation(new MaxLengthAttribute(2))
                        .AddItemValidation(new StringLengthAttribute(5)))
                    .AddCollectionField("items", b => b
                        .AddField("code", FieldType.String, f => f.AddValidation(new RequiredAttribute()))))
                .AddPayload("other", b => b.AddField("orderId", FieldType.String))
                .AddPayload("response", b => b.AddField("orderId", FieldType.String, f => f.AddValidation(new RequiredAttribute())))
                .AddRequest("query", "response", b => b.AddField("orderId", FieldType.String))
                .AddRequest<ClrResponse>("clrQuery", b => b.AddField("orderId", FieldType.String)))
            .AddStateMachines(b => b
                .AddStateMachine("structural", b => b
                    .AddInitialState("waiting", b => b
                        .AddPayloadTransition(["submit"], "done", b => b
                            .AddGuard(c => c.Event.Get<int>("quantity") > 0)))
                    .AddState("done"))
                .AddStateMachine("payloadRequests", b => b
                    .AddInitialState("waiting", b => b
                        .AddInternalPayloadTransition(["query"], b => b.AddEffect(c =>
                            c.Event.Respond(c.Behavior.ServiceProvider.GetRequiredService<IPayloadFactory>()
                                .Create("response", b => b.Set("orderId", c.Event.Get<string>("orderId"))))))
                        .AddInternalPayloadTransition(["clrQuery"], b => b.AddEffect(c =>
                            c.Event.Respond(new DerivedClrResponse { OrderId = c.Event.Get<string>("orderId") })))))
                .AddStateMachine("payloadDeferred", b => b
                    .AddInitialState("waiting", b => b
                        .AddDeferredPayloadEvent(["submit"])
                        .AddPayloadTransition(["other"], "ready"))
                    .AddState("ready", b => b.AddPayloadTransition(["submit"], "done"))
                    .AddState("done"))
                .AddStateMachine("payloadElse", b => b
                    .AddInitialState("waiting", b => b
                        .AddPayloadTransition(["submit"], "normal", b => b.AddGuard(_ => false))
                        .AddElsePayloadTransition(["submit"], "fallback"))
                    .AddState("normal")
                    .AddState("fallback")))
            .AddActivities(b => b.AddActivity("payloadActivity", b => b
                .AddInitial(b => b.AddControlFlow("receive"))
                .AddAcceptPayloadEventAction(["submit"], "receive", c =>
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
        Assert.AreEqual("London", value.Get<PayloadValue>("address").Get<string>("city"));
        Assert.IsFalse(value.Get<PayloadValue>("address").ContainsField("unknown"));
        CollectionAssert.AreEqual(new[] { "one", "two" }, value.GetCollection<string>("tags").ToArray());
        Assert.AreEqual("A", value.GetCollection<PayloadValue>("items")[0].Get<string>("code"));
    }

    [TestMethod]
    public async Task SupportsPayloadAndClrResponses()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("payloadRequests", "one"), out var machine));
        await machine.SendAsync(new Initialize());
        var payload = await machine.RequestAsync(Factory.Create("query", b => b.Set("orderId", "123")));
        Assert.AreEqual(EventStatus.Consumed, payload.Status);
        Assert.AreEqual("123", payload.Response.Get<string>("orderId"));
        var clr = await machine.RequestAsync<ClrResponse>(Factory.Create("clrQuery", b => b.Set("orderId", "456")));
        Assert.AreEqual(EventStatus.Consumed, clr.Status);
        Assert.AreEqual("456", clr.Response.OrderId);
        Assert.IsInstanceOfType<DerivedClrResponse>(clr.Response);
    }

    [TestMethod]
    public void RejectsWrongAndInvalidResponses()
    {
        var payload = (PayloadRequest)Factory.Create("query", _ => { });
        Assert.ThrowsExactly<InvalidOperationException>(() => payload.Respond(Factory.Create("other", _ => { })));
        Assert.ThrowsExactly<System.ComponentModel.DataAnnotations.ValidationException>(() => payload.Respond(Factory.Create("response", _ => { })));
        var clr = (PayloadRequest)Factory.Create("clrQuery", _ => { });
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
            .AddPayloads(b => b.AddPayload(Event.GetName(eventType), _ => { }))));

    [TestMethod]
    public async Task AcceptsNamedEventInActivity()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("payloadActivity", "one"), out var activity));
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
        var roundtrip = (Payload)StateflowsJsonConverter.DeserializeObject(
            StateflowsJsonConverter.SerializePolymorphicObject(original));
        Assert.AreEqual("submit", roundtrip.Name);
        Assert.AreEqual(3, roundtrip.Get<int>("quantity"));
        Assert.AreEqual("London", roundtrip.Get<PayloadValue>("address").Get<string>("city"));
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
        var restored = (Payload)StateflowsJsonConverter.DeserializeObject(document.ToString());
        Assert.IsTrue(restored.ContainsField("undeclared"));

        var errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        Assert.IsTrue(await ((IStateflowsValidator)Factory).ValidateAsync(restored, errors));
        Assert.IsFalse(restored.ContainsField("undeclared"));
        Assert.AreEqual(3, restored.Get<int>("quantity"));
    }

    [TestMethod]
    public async Task DeferredEventRetainsItsContractAndPayload()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("payloadDeferred", "one"), out var machine));
        await machine.SendAsync(new Initialize());
        var deferred = await machine.SendAsync(Factory.Create("submit", b => b.Set("orderId", "123").Set("quantity", 1)));
        Assert.AreEqual(EventStatus.Deferred, deferred.Status);
        await machine.SendAsync(Factory.Create("other", _ => { }));
        Assert.AreEqual("done", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
    }

    [TestMethod]
    public async Task ElseTransitionMatchesNamedContract()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("payloadElse", "one"), out var machine));
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
            .AddPayloads(b => b.AddPayload("duplicate", _ => { }).AddPayload("duplicate", _ => { }))));
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddStateflows(b => b
            .AddPayloads(b => b.AddRequest("query", "unknown", _ => { }))));
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddStateflows(b => b
            .AddPayloads(b => b.AddPayload(Event<Initialize>.Name, _ => { }))));
    }

    [TestMethod]
    public void DefinitionsCannotBeRegisteredAfterConfiguration()
    {
        IPayloadsBuilder? retained = null;
        new ServiceCollection().AddStateflows(b => b.AddPayloads(b => retained = b));
        Assert.IsNotNull(retained);
        Assert.ThrowsExactly<InvalidOperationException>(() => retained.AddPayload("late", _ => { }));
    }

    [TestMethod]
    public void RejectsClrNameCollisionsInEitherRegistrationOrder()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddStateflows(b => b
            .AddPayloads(b => b.AddPayload(Event<ClrResponse>.Name, _ => { }))
            .AddStateMachines(b => b.AddStateMachine("collision", b => b
                .AddInitialState("one", b => b.AddTransition<ClrResponse>("two"))
                .AddState("two")))));
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddStateflows(b => b
            .AddStateMachines(b => b.AddStateMachine("collision", b => b
                .AddInitialState("one", b => b.AddTransition<ClrResponse>("two"))
                .AddState("two")))
            .AddPayloads(b => b.AddPayload(Event<ClrResponse>.Name, _ => { }))));
    }

    [TestMethod]
    public async Task FluentValidationAppliesPayloadBaseValidatorsToRequests()
    {
        var services = new ServiceCollection();
        var validator = new FluentValidation.InlineValidator<Payload>();
        validator.RuleFor(value => value.Name).Must(name => name != "query");
        services.AddSingleton<FluentValidation.IValidator<Payload>>(validator);
        using var provider = services.BuildServiceProvider();
        var results = new List<ValidationResult>();
        var request = (PayloadRequest)Factory.Create("query", b => b.Set("orderId", "123"));
        var valid = await new Stateflows.Extensions.FluentValidation.FluentValidator(provider)
            .ValidateAsync(request, results);
        Assert.IsFalse(valid);
        Assert.HasCount(1, results);
    }
}
