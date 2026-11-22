using Activity.IntegrationTests.Classes.Events;
using Microsoft.Extensions.DependencyInjection;
using OneOf;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Common;
using Stateflows.Common.Registration.Builders;
using Stateflows.Payloads;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public class PayloadInitializers : StateflowsTestClass
{
    private IPayloadFactory Factory => ServiceProvider.GetRequiredService<IPayloadFactory>();
    internal static string? InitializedBy;
    internal static bool Executed;

    public sealed class StartInitializer : IInitializer<Payload>
    {
        public Task<bool> OnInitializeAsync(Payload initializationEvent)
        {
            InitializedBy = initializationEvent.Name;
            return Task.FromResult(initializationEvent.Get<bool>("ok"));
        }
    }

    private sealed class InitializerVisitor : ActivityVisitor
    {
        public readonly List<(Type Type, string[] Names)> Initializers = [];

        public override Task InitializerAddedAsync<TInitializationEvent>(string id, string activityName, int activityVersion, IReadOnlyList<string> eventNames, int order)
        {
            if (activityName == "mixed")
                Initializers.Add((typeof(TInitializationEvent), eventNames.ToArray()));
            return Task.CompletedTask;
        }
    }

    [TestInitialize]
    public override void Initialize()
    {
        InitializedBy = null;
        Executed = false;
        base.Initialize();
    }

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder
            .AddOneOf()
            .AddPayloads(b => b
                .AddPayload("start", b => b.AddField("ok", FieldType.Boolean))
                .AddPayload("begin", b => b.AddField("ok", FieldType.Boolean))
                .AddPayload("other", b => b.AddField("ok", FieldType.Boolean)))
            .AddActivities(b => b
                .AddActivity("single", b => b
                    .AddPayloadInitializer(["start"], async c =>
                    {
                        InitializedBy = c.InitializationEvent.Name;
                        return c.InitializationEvent.Get<bool>("ok");
                    })
                    .AddInitial(b => b.AddControlFlow("run"))
                    .AddAction("run", async c => Executed = true))
                .AddActivity("many", b => b
                    .AddPayloadInitializer(["start", "begin"], async c =>
                    {
                        InitializedBy = c.InitializationEvent.Name;
                        return true;
                    })
                    .AddInitial(b => b.AddControlFlow("run"))
                    .AddAction("run", async c => Executed = true))
                .AddActivity("class", b => b
                    .AddPayloadInitializer<StartInitializer>(["start", "begin"])
                    .AddInitial(b => b.AddControlFlow("run"))
                    .AddAction("run", async c => Executed = true))
                .AddActivity("classSugar", b => b
                    .AddPayloadInitializer<StartInitializer>(["start"])
                    .AddInitial(b => b.AddControlFlow("run"))
                    .AddAction("run", async c => Executed = true))
                .AddActivity("oneOf", b => b
                    .AddInitializer<OneOf<SomeEvent, OtherEvent>>(async c =>
                    {
                        InitializedBy = c.InitializationEvent.Match(_ => "some", _ => "other");
                        return true;
                    })
                    .AddInitial(b => b.AddControlFlow("run"))
                    .AddAction("run", async c => Executed = true))
                .AddActivity("mixed", b => b
                    .AddInitializer<SomeEvent>(async c => true)
                    .AddPayloadInitializer(["start"], async c => true)
                    .AddPayloadInitializer(["begin", "other"], async c => true)
                    .AddInitial(b => b.AddControlFlow("run"))
                    .AddAction("run", async c => Executed = true)));

    private IActivityBehavior Locate(string name)
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId(name, Guid.NewGuid().ToString()), out var activity));
        return activity;
    }

    [TestMethod]
    public async Task SingleNameInitializes()
    {
        var activity = Locate("single");
        Assert.AreEqual(EventStatus.Initialized, (await activity.SendAsync(Factory.Create("start", b => b.Set("ok", true)))).Status);
        Assert.AreEqual("start", InitializedBy);
        Assert.IsTrue(Executed);
    }

    [TestMethod]
    public async Task FalseResultLeavesActivityNotInitialized()
    {
        var activity = Locate("single");
        Assert.AreEqual(EventStatus.NotInitialized, (await activity.SendAsync(Factory.Create("start", b => b.Set("ok", false)))).Status);
        Assert.IsFalse(Executed);
    }

    [TestMethod]
    public async Task EachNameInSetInitializesAndOthersDoNot()
    {
        Assert.AreEqual(EventStatus.Initialized, (await Locate("many").SendAsync(Factory.Create("begin", b => b.Set("ok", true)))).Status);
        Assert.AreEqual("begin", InitializedBy);
        Assert.AreEqual(EventStatus.Initialized, (await Locate("many").SendAsync(Factory.Create("start", b => b.Set("ok", true)))).Status);

        InitializedBy = null;
        Executed = false;
        var typedStatus = (await Locate("mixed").SendAsync(new OtherEvent())).Status;
        Assert.AreNotEqual(EventStatus.Initialized, typedStatus);
        Executed = false;
        Assert.AreEqual(typedStatus, (await Locate("many").SendAsync(Factory.Create("other", b => b.Set("ok", true)))).Status);
        Assert.IsNull(InitializedBy);
        Assert.IsFalse(Executed);
    }

    [TestMethod]
    public async Task ClassInitializerHandlesEveryName()
    {
        Assert.AreEqual(EventStatus.Initialized, (await Locate("class").SendAsync(Factory.Create("begin", b => b.Set("ok", true)))).Status);
        Assert.AreEqual("begin", InitializedBy);
    }

    [TestMethod]
    public async Task ClassInitializerWithSugarNameHandlesOnlyThatName()
    {
        Assert.AreEqual(EventStatus.Initialized, (await Locate("classSugar").SendAsync(Factory.Create("start", b => b.Set("ok", true)))).Status);
        Assert.AreEqual("start", InitializedBy);

        InitializedBy = null;
        Executed = false;
        Assert.AreNotEqual(EventStatus.Initialized, (await Locate("classSugar").SendAsync(Factory.Create("begin", b => b.Set("ok", true)))).Status);
        Assert.IsNull(InitializedBy);
        Assert.IsFalse(Executed);
    }

    [TestMethod]
    public void TypedPayloadClassInitializerIsRejected()
        => Assert.ThrowsExactly<ActivityDefinitionException>(() => NewBuilder().AddInitializer<Payload, StartInitializer>());

    [TestMethod]
    public async Task OneOfInitializerMatchesEveryMappedType()
    {
        Assert.AreEqual(EventStatus.Initialized, (await Locate("oneOf").SendAsync(new SomeEvent())).Status);
        Assert.AreEqual("some", InitializedBy);
        Assert.AreEqual(EventStatus.Initialized, (await Locate("oneOf").SendAsync(new OtherEvent())).Status);
        Assert.AreEqual("other", InitializedBy);
    }

    [TestMethod]
    public async Task ExpectedEventsListAllInitializers()
    {
        var expected = (await Locate("mixed").GetStatusAsync()).Response.ExpectedEvents.ToArray();
        foreach (var name in new[] { "start", "begin", "other", Event<SomeEvent>.Name })
            CollectionAssert.Contains(expected, name);

        var oneOf = (await Locate("oneOf").GetStatusAsync()).Response.ExpectedEvents.ToArray();
        CollectionAssert.Contains(oneOf, Event<SomeEvent>.Name);
        CollectionAssert.Contains(oneOf, Event<OtherEvent>.Name);
    }

    [TestMethod]
    public async Task TypedAndNamedInitializersCoexist()
    {
        Assert.AreEqual(EventStatus.Initialized, (await Locate("mixed").SendAsync(new SomeEvent())).Status);
        Assert.AreEqual(EventStatus.Initialized, (await Locate("mixed").SendAsync(Factory.Create("other", b => b.Set("ok", true)))).Status);
    }

    [TestMethod]
    public async Task VisitorReportsNamedInitializers()
    {
        var visitor = new InitializerVisitor();
        await ServiceProvider.GetRequiredService<IActivitiesRegister>().VisitActivitiesAsync(visitor);
        Assert.AreEqual(3, visitor.Initializers.Count);
        Assert.IsTrue(visitor.Initializers.Any(i => i.Type == typeof(SomeEvent)));
        Assert.IsTrue(visitor.Initializers.Any(i => i.Type == typeof(Payload) && i.Names.SequenceEqual(["start"])));
        Assert.IsTrue(visitor.Initializers.Any(i => i.Type == typeof(Payload) && i.Names.SequenceEqual(["begin", "other"])));
    }

    private static IActivityBuilder NewBuilder()
        => new ActivityBuilder("definition", 1, null!, new StateflowsBuilder(new ServiceCollection()), null, null);

    [TestMethod]
    public void TypedPayloadInitializerIsRejected()
        => Assert.ThrowsExactly<ActivityDefinitionException>(() => NewBuilder().AddInitializer<Payload>(async c => true));

    [TestMethod]
    [DataRow(new string[0])]
    [DataRow(new[] { "" })]
    [DataRow(new[] { " " })]
    [DataRow(new[] { "start", "start" })]
    public void InvalidNameSetsAreRejected(string[] names)
        => Assert.ThrowsExactly<ArgumentException>(() => NewBuilder().AddPayloadInitializer(names, async c => true));
}
