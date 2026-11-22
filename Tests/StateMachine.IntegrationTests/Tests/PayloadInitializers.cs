using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.StateMachines.Exceptions;
using StateMachine.IntegrationTests.Classes.Events;
using Stateflows.Payloads;
using Stateflows.StateMachines;
using StateMachine.IntegrationTests.Tests.DefinitionRules;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class PayloadInitializers : StateflowsTestClass
{
    private IPayloadFactory Factory => ServiceProvider.GetRequiredService<IPayloadFactory>();
    internal static string? InitializedBy;

    public sealed class StartInitializer : IInitializer<Payload>
    {
        public Task<bool> OnInitializeAsync(Payload initializationEvent)
        {
            InitializedBy = initializationEvent.Name;
            return Task.FromResult(initializationEvent.Get<bool>("ok"));
        }
    }

    private sealed class InitializerVisitor : StateMachineVisitor
    {
        public readonly List<(Type Type, string[] Names)> Initializers = [];

        public override Task InitializerAddedAsync<TInitializationEvent>(string id, string stateMachineName, int stateMachineVersion, IReadOnlyList<string> eventNames)
        {
            if (stateMachineName == "mixed")
                Initializers.Add((typeof(TInitializationEvent), eventNames.ToArray()));
            return Task.CompletedTask;
        }
    }

    [TestInitialize]
    public override void Initialize()
    {
        InitializedBy = null;
        base.Initialize();
    }

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder
            .AddPayloads(b => b
                .AddPayload("start", b => b.AddField("ok", FieldType.Boolean))
                .AddPayload("begin", b => b.AddField("ok", FieldType.Boolean))
                .AddPayload("other", b => b.AddField("ok", FieldType.Boolean)))
            .AddStateMachines(b => b
                .AddStateMachine("single", b => b
                    .AddPayloadInitializer(["start"], async c =>
                    {
                        InitializedBy = c.InitializationEvent.Name;
                        return c.InitializationEvent.Get<bool>("ok");
                    })
                    .AddInitialState("ready"))
                .AddStateMachine("many", b => b
                    .AddPayloadInitializer(["start", "begin"], async c =>
                    {
                        InitializedBy = c.InitializationEvent.Name;
                        return true;
                    })
                    .AddInitialState("ready"))
                .AddStateMachine("class", b => b
                    .AddPayloadInitializer<StartInitializer>(["start", "begin"])
                    .AddInitialState("ready"))
                .AddStateMachine("classSugar", b => b
                    .AddPayloadInitializer<StartInitializer>(["start"])
                    .AddInitialState("ready"))
                .AddStateMachine("mixed", b => b
                    .AddInitializer<SomeEvent>(async c => true)
                    .AddPayloadInitializer(["start"], async c => true)
                    .AddPayloadInitializer(["begin", "other"], async c => true)
                    .AddInitialState("ready")));

    private async Task<IStateMachineBehavior> Locate(string name)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId(name, Guid.NewGuid().ToString()), out var machine));
        return machine;
    }

    [TestMethod]
    public async Task SingleNameInitializes()
    {
        var machine = await Locate("single");
        var result = await machine.SendAsync(Factory.Create("start", b => b.Set("ok", true)));
        Assert.AreEqual(EventStatus.Initialized, result.Status);
        Assert.AreEqual("start", InitializedBy);
        Assert.AreEqual("ready", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
    }

    [TestMethod]
    public async Task FalseResultLeavesMachineNotInitialized()
    {
        var machine = await Locate("single");
        var result = await machine.SendAsync(Factory.Create("start", b => b.Set("ok", false)));
        Assert.AreEqual(EventStatus.NotInitialized, result.Status);
    }

    [TestMethod]
    public async Task EachNameInSetInitializesAndOthersDoNot()
    {
        var first = await Locate("many");
        Assert.AreEqual(EventStatus.Initialized, (await first.SendAsync(Factory.Create("begin", b => b.Set("ok", true)))).Status);
        Assert.AreEqual("begin", InitializedBy);

        var second = await Locate("many");
        Assert.AreEqual(EventStatus.Initialized, (await second.SendAsync(Factory.Create("start", b => b.Set("ok", true)))).Status);

        var third = await Locate("many");
        InitializedBy = null;
        Assert.AreNotEqual(EventStatus.Initialized, (await third.SendAsync(Factory.Create("other", b => b.Set("ok", true)))).Status);
        Assert.IsNull(InitializedBy);
        Assert.AreEqual(EventStatus.Rejected, (await third.SendAsync(new Initialize())).Status);
    }

    [TestMethod]
    public async Task ClassInitializerHandlesEveryName()
    {
        var machine = await Locate("class");
        Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(Factory.Create("begin", b => b.Set("ok", true)))).Status);
        Assert.AreEqual("begin", InitializedBy);
    }

    [TestMethod]
    public async Task ClassInitializerWithSugarNameHandlesOnlyThatName()
    {
        Assert.AreEqual(EventStatus.Initialized, (await (await Locate("classSugar")).SendAsync(Factory.Create("start", b => b.Set("ok", true)))).Status);
        Assert.AreEqual("start", InitializedBy);

        InitializedBy = null;
        Assert.AreNotEqual(EventStatus.Initialized, (await (await Locate("classSugar")).SendAsync(Factory.Create("begin", b => b.Set("ok", true)))).Status);
        Assert.IsNull(InitializedBy);
    }

    [TestMethod]
    public void TypedPayloadClassInitializerIsRejected()
        => Assert.ThrowsExactly<StateMachineDefinitionException>(() => ((IStateMachineBuilder)new DefinitionModel().Builder).AddInitializer<Payload, StartInitializer>());

    [TestMethod]
    public async Task ExpectedEventsListAllInitializers()
    {
        var machine = await Locate("mixed");
        var expected = (await machine.GetStatusAsync()).Response.ExpectedEvents.ToArray();
        foreach (var name in new[] { "start", "begin", "other", Event<SomeEvent>.Name })
            CollectionAssert.Contains(expected, name);
    }

    [TestMethod]
    public async Task TypedAndNamedInitializersCoexist()
    {
        Assert.AreEqual(EventStatus.Initialized, (await (await Locate("mixed")).SendAsync(new SomeEvent())).Status);
        Assert.AreEqual(EventStatus.Initialized, (await (await Locate("mixed")).SendAsync(Factory.Create("other", b => b.Set("ok", true)))).Status);
    }

    [TestMethod]
    public async Task VisitorReportsNamedInitializers()
    {
        var visitor = new InitializerVisitor();
        await ServiceProvider.GetRequiredService<IStateMachinesRegister>().VisitStateMachinesAsync(visitor);
        Assert.AreEqual(3, visitor.Initializers.Count);
        Assert.IsTrue(visitor.Initializers.Any(i => i.Type == typeof(SomeEvent)));
        Assert.IsTrue(visitor.Initializers.Any(i => i.Type == typeof(Payload) && i.Names.SequenceEqual(["start"])));
        Assert.IsTrue(visitor.Initializers.Any(i => i.Type == typeof(Payload) && i.Names.SequenceEqual(["begin", "other"])));
    }

    [TestMethod]
    public void TypedPayloadInitializerIsRejected()
        => Assert.ThrowsExactly<StateMachineDefinitionException>(() => new DefinitionModel().Builder.AddInitializer<Payload>(async c => true));

    [TestMethod]
    [DataRow(new string[0])]
    [DataRow(new[] { "" })]
    [DataRow(new[] { " " })]
    [DataRow(new[] { "start", "start" })]
    public void InvalidNameSetsAreRejected(string[] names)
        => Assert.ThrowsExactly<ArgumentException>(() => ((IStateMachineBuilder)new DefinitionModel().Builder).AddPayloadInitializer(names, async c => true));
}
