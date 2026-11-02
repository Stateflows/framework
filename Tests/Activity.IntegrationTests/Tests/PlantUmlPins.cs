using System.Text.RegularExpressions;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class PlantUmlPins : StateflowsTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder) => builder
        .AddPlantUml()
        .AddActivities(activities => activities
            .AddActivity("diagram-pins", activity => activity
                .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>())
                .AddInputPin<string>(pin => pin.AddFlow<OutputPin<string>>())
                .AddOutputPin<int>().AddOutputPin<string>().AddOutputPin<decimal>())
            .AddActivity("diagram-nested-pins", activity => activity
                .AddInputPin<int>(pin => pin.AddFlow("left").AddFlow("right"))
                .AddStructuredActivity("left", scope => scope
                    .AddInputPin<int>(pin => pin.AddFlow("inner"))
                    .AddStructuredActivity("inner", inner => inner
                        .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>())
                        .AddOutputPin<int>()
                        .AddFlow<int, OutputPin<int>>())
                    .AddOutputPin<int>()
                    .AddFlow<int, OutputPin<int>>())
                .AddStructuredActivity("right", scope => scope
                    .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>())
                    .AddOutputPin<int>()
                    .AddFlow<int, OutputPin<int>>())
                .AddOutputPin<int>())
            .AddActivity("diagram-legacy", activity => activity
                .AddInput(input => input.AddFlow<int, OutputNode>())
                .AddOutput())
            .AddActivity("diagram-pin-alias", activity => activity
                .AddInputPin<int>(pin => pin.AddFlow("pin0"))
                .AddAction("pin0", context =>
                {
                    context.PassTokensOfTypeOn<int>();
                    return Task.CompletedTask;
                }, node => node.AddFlow<int, OutputPin<int>>())
                .AddOutputPin<int>())
            .AddActivity("diagram-generic-pin", activity => activity
                .AddInputPin<List<int>>(pin => pin.AddFlow<OutputPin<List<int>>>())
                .AddOutputPin<List<int>>())
            .AddActivity("diagram-expansion-pins", activity => activity
                .AddInputPin<int>(pin => pin.AddFlow("parallel").AddFlow("iterative"))
                .AddParallelActivity<int>("parallel", scope => scope
                    .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>())
                    .AddOutputPin<int>().AddFlow<int, OutputPin<int>>())
                .AddIterativeActivity<int>("iterative", scope => scope
                    .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>())
                    .AddOutputPin<int>().AddFlow<int, OutputPin<int>>())
                .AddOutputPin<int>()));

    private async Task<string> DiagramAsync(string name, bool execute = false)
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId(name, "diagram"), out var activity));
        using (activity)
        {
            if (execute)
            {
                var published = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                await using var watcher = await activity.WatchPlantUmlAsync(info => published.TrySetResult(info.PlantUml), immediateRequest: false);
                Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(new TokensInput().Add(42))).Status);
                return await published.Task.WaitAsync(TimeSpan.FromSeconds(5));
            }
            var result = await activity.GetPlantUmlAsync();
            Assert.AreEqual(EventStatus.Consumed, result.Status);
            return result.Response.PlantUml;
        }
    }

    [TestMethod]
    public async Task DeclaresTypedInputAndOutputPinsIncludingUnconnectedOutputs()
    {
        var diagram = await DiagramAsync("diagram-pins");
        StringAssert.Contains(diagram, "\"Input pin\\nInt32\" as pin0 -[hidden]-> pin0");
        StringAssert.Contains(diagram, "\"Input pin\\nString\" as pin1");
        StringAssert.Contains(diagram, "\"Output pin\\nInt32\" as pin2");
        StringAssert.Contains(diagram, "\"Output pin\\nString\" as pin3");
        StringAssert.Contains(diagram, "\"Output pin\\nDecimal\" as pin4");
        StringAssert.Contains(diagram, "pin0 -->[Int32] pin2");
        StringAssert.Contains(diagram, "pin1 -->[String] pin3");
        Assert.AreEqual(5, Regex.Matches(diagram, "\"(?:Input|Output) pin").Count);
    }

    [TestMethod]
    public async Task NestedAndSiblingPinsHaveDistinctAliasesAndStayInsidePartitions()
    {
        var diagram = await DiagramAsync("diagram-nested-pins");
        var declarations = Regex.Matches(diagram, "\"(?:Input|Output) pin\\\\nInt32\" as (pin\\d+)");
        Assert.AreEqual(8, declarations.Count);
        Assert.AreEqual(8, declarations.Select(match => match.Groups[1].Value).Distinct().Count());
        StringAssert.Contains(diagram, "partition \"StructuredActivity left\"");
        StringAssert.Contains(diagram, "partition \"StructuredActivity inner\"");
        StringAssert.Contains(diagram, "partition \"StructuredActivity right\"");
        var aliases = declarations.Select(match => match.Groups[1].Value).ToHashSet();
        foreach (Match match in Regex.Matches(diagram, "(pin\\d+) -->\\[Int32\\] (pin\\d+)"))
        {
            Assert.IsTrue(aliases.Contains(match.Groups[1].Value));
            Assert.IsTrue(aliases.Contains(match.Groups[2].Value));
            Assert.AreNotEqual(match.Groups[1].Value, match.Groups[2].Value);
        }
        StringAssert.Contains(diagram, "pin2 -->[Int32] pin3");
        StringAssert.Contains(diagram, "pin5 -->[Int32] pin6");
        Assert.IsTrue(diagram.IndexOf("\"Output pin\\nInt32\" as pin7", StringComparison.Ordinal)
            < diagram.IndexOf("pin0 -->[Int32]", StringComparison.Ordinal), "Declare pins before flows reference them.");
    }

    [TestMethod]
    public async Task PinFlowsPreserveExecutionStatusAndTokenCounts()
    {
        var diagram = await DiagramAsync("diagram-pins", execute: true);
        StringAssert.Contains(diagram, "pin0 -->[Int32 (passed 1)] pin2 <<executed>>");
        StringAssert.Contains(diagram, "ArrowColor green");
    }

    [TestMethod]
    public async Task DeprecatedUntypedBoundariesKeepTheirExistingRepresentation()
    {
        var diagram = await DiagramAsync("diagram-legacy");
        StringAssert.Contains(diagram, "\"InputNode\" -->[Int32] \"OutputNode\"");
        Assert.IsFalse(diagram.Contains("Input pin"));
        Assert.IsFalse(diagram.Contains("Output pin"));
    }

    [TestMethod]
    public async Task PinAliasesDoNotCollideWithUserNamedActions()
    {
        var diagram = await DiagramAsync("diagram-pin-alias");
        StringAssert.Contains(diagram, "\"Input pin\\nInt32\" as pin1");
        StringAssert.Contains(diagram, "pin1 -->[Int32] \"pin0\"");
        StringAssert.Contains(diagram, "\"pin0\" -->[Int32] pin2");
    }

    [TestMethod]
    public async Task GenericPinLabelsUseDeclaredTokenNames()
    {
        var diagram = await DiagramAsync("diagram-generic-pin");
        StringAssert.Contains(diagram, "\"Input pin\\nList<Int32>\" as pin0");
        StringAssert.Contains(diagram, "\"Output pin\\nList<Int32>\" as pin1");
        StringAssert.Contains(diagram, "pin0 -->[List<Int32>] pin1");
    }

    [TestMethod]
    public async Task ExpansionRegionPinsAreDeclaredInsideTheirOwnPartitions()
    {
        var diagram = await DiagramAsync("diagram-expansion-pins");
        StringAssert.Contains(diagram, "partition \"ParallelActivity parallel\" {" + Environment.NewLine + "  \"Input pin\\nInt32\" as pin1");
        StringAssert.Contains(diagram, "partition \"IterativeActivity iterative\" {" + Environment.NewLine + "  \"Input pin\\nInt32\" as pin3");
        StringAssert.Contains(diagram, "pin1 -->[Int32] pin2");
        StringAssert.Contains(diagram, "pin3 -->[Int32] pin4");
        StringAssert.Contains(diagram, "\"parallel\" -->[Int32] pin5");
        StringAssert.Contains(diagram, "\"iterative\" -->[Int32] pin5");
    }
}
