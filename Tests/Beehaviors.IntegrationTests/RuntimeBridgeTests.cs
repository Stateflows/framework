using Beehaviors;
using Microsoft.Extensions.DependencyInjection;
using Stateflows;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines;

namespace Beehaviors.IntegrationTests;

[TestClass]
public sealed class RuntimeBridgeTests
{
    [TestMethod]
    public async Task RegisteredBehaviorsExecuteThroughBeehaviorsRuntimeBridge()
    {
        var services = new ServiceCollection();
        var trace = new List<string>();
        services.AddLogging();

        services.AddBeehaviors(builder => builder
            .AddStateflows(stateflows => stateflows
                .AddActions(actions => actions.AddAction(
                    "bridge-action",
                    _ =>
                    {
                        trace.Add("action");
                        return Task.CompletedTask;
                    }))
                .AddActivities(activities => activities.AddActivity(
                    "bridge-activity",
                    activity => activity
                        .AddInitial(initial => initial.AddControlFlow("record"))
                        .AddAction(
                            "record",
                            _ =>
                            {
                                trace.Add("activity");
                                return Task.CompletedTask;
                            },
                            action => action.AddControlFlow<FinalNode>())
                        .AddFinal()))
                .AddStateMachines(machines => machines.AddStateMachine(
                    "bridge-machine",
                    machine => machine
                        .AddInitialState("ready", state => state.AddTransition<BridgeEvent>(
                            "done",
                            transition => transition.AddEffect(_ =>
                            {
                                trace.Add("state-machine");
                                return Task.CompletedTask;
                            })))
                        .AddState("done")))));

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var beehaviorsMetadata = scope.ServiceProvider.GetRequiredService<BeehaviorsRegistrationMetadata>();
        CollectionAssert.Contains(beehaviorsMetadata.RegisteredBeeTypes, "stateflows.runtime.behavior");

        var behaviorFactory = scope.ServiceProvider.GetRequiredService<IBehaviorFactory>();
        Assert.IsTrue(
            behaviorFactory.GetType().FullName?.Contains("BeeBehaviorFactory", StringComparison.Ordinal) == true,
            $"Expected BeeBehaviorFactory, got '{behaviorFactory.GetType().FullName}'.");

        var actionLocator = scope.ServiceProvider.GetRequiredService<IActionLocator>();
        var activityLocator = scope.ServiceProvider.GetRequiredService<IActivityLocator>();
        var stateMachineLocator = scope.ServiceProvider.GetRequiredService<IStateMachineLocator>();

        Assert.IsTrue(actionLocator.TryLocateAction(new ActionId("bridge-action", "x"), out var action));
        await action.ExecuteAsync();
        action.Dispose();

        Assert.IsTrue(activityLocator.TryLocateActivity(new ActivityId("bridge-activity", "x"), out var activity));
        var activityResult = await activity.SendAsync(new Initialize());
        Assert.AreEqual(EventStatus.Initialized, activityResult.Status);
        activity.Dispose();

        Assert.IsTrue(stateMachineLocator.TryLocateStateMachine(new StateMachineId("bridge-machine", "x"), out var stateMachine));
        var initialize = await stateMachine.SendAsync(new Initialize());
        Assert.AreEqual(EventStatus.Initialized, initialize.Status);
        Assert.AreEqual("ready", (await stateMachine.GetStatusAsync()).Response.CurrentStates.Value);
        var transition = await stateMachine.SendAsync(new BridgeEvent());
        Assert.AreEqual(EventStatus.Consumed, transition.Status);
        Assert.AreEqual("done", (await stateMachine.GetStatusAsync()).Response.CurrentStates.Value);
        stateMachine.Dispose();

        CollectionAssert.AreEqual(
            new[] { "action", "activity", "state-machine" },
            trace.ToArray());
    }

    public sealed class BridgeEvent;
}
