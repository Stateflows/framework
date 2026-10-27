using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Events;
using Stateflows.StateMachines;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests.ClearScript;

[TestClass]
public class ClearScriptStructuralEvents : StateflowsTestClass
{
    private const string Guard =
        "event.Name === 'submit' && " +
        "event.ContainsField('quantity') && " +
        "event.Fields.Item('quantity') === 3 && " +
        "event.Fields.Item.get('quantity') === 3 && " +
        "event.Fields.Item('address').Fields.Item('city') === 'London' && " +
        "event.Fields.Item('tags')[1] === 'b' && " +
        "event.Fields.Item('tags').Length === 2";

    private const string Effect =
        "if (event.Fields.Item('address').Fields.Item('city') !== 'London') throw new Error('nested field mismatch');";

    private IEventFactory Factory => ServiceProvider.GetRequiredService<IEventFactory>();

    [TestInitialize]
    public override void Initialize()
    {
        try
        {
            using var _ = new V8ScriptEngine();
        }
        catch (TypeLoadException e)
        {
            Assert.Inconclusive($"ClearScript V8 native library is not available on this platform: {e.Message}");
        }

        base.Initialize();
    }

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder
            .AddClearScript(_ => Task.FromResult<IScriptEngine>(new V8ScriptEngine()))
            .AddEvents(b => b
                .AddEvent("submit", b => b
                    .AddField("quantity", FieldType.Int32)
                    .AddObjectField("address", b => b.AddField("city", FieldType.String))
                    .AddCollectionField("tags", FieldType.String)))
            .AddStateMachines(b => b
                .AddStateMachine("scriptedStructural", b => b
                    .AddInitialState("waiting", b => b
                        .AddTransition("submit", "done", b => b
                            .AddGuard_ClearScript(Guard)
                            .AddEffect_ClearScript(Effect)))
                    .AddState("done")));

    private StructuralEvent Submit(int quantity, string city)
        => Factory.Create("submit", b => b
            .Set("quantity", quantity)
            .SetObject("address", b => b.Set("city", city))
            .Set("tags", new[] { "a", "b" }));

    [TestMethod]
    public async Task ScriptGuardAndEffectReadScalarNestedAndCollectionFields()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("scriptedStructural", "one"), out var machine));
        await machine.SendAsync(new Initialize());

        Assert.AreEqual(EventStatus.NotConsumed, (await machine.SendAsync(Submit(2, "London"))).Status);
        Assert.AreEqual(EventStatus.NotConsumed, (await machine.SendAsync(Submit(3, "Paris"))).Status);
        Assert.AreEqual("waiting", (await machine.GetStatusAsync()).Response.CurrentStates.Value);

        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(Submit(3, "London"))).Status);
        Assert.AreEqual("done", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
    }

    [TestMethod]
    public void JavaScriptIndexerAndGenericGetAreNotAvailableOnStructuralValues()
    {
        using var engine = new V8ScriptEngine();
        engine.AddRestrictedHostObject("event", HostItemFlags.None, Submit(3, "London"));

        Assert.IsInstanceOfType<Undefined>(engine.Evaluate("event.Fields['quantity']"));
        Assert.IsInstanceOfType<Undefined>(engine.Evaluate("event.Fields.quantity"));
        Assert.ThrowsExactly<ScriptEngineException>(() => engine.Evaluate("event.Get('quantity')"));
    }
}
