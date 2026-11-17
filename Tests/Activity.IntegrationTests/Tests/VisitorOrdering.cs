using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Activities.Registration;
using Stateflows.Common;
using Stateflows.StateMachines;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public class VisitorOrdering
{
    private sealed class Event;
    private sealed class Machine : IStateMachine
    {
        public static void Build(IStateMachineBuilder builder) => builder.AddInitialState("initial");
    }

    private sealed class Activity : IActivity
    {
        public static void Build(IActivityBuilder builder) => builder.AddInitial(_ => { });
    }

    private sealed class Action : IAction
    {
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    [TestMethod]
    public async Task TypedDefinitionsVisitTypeBeforeElementsAndFinishLast()
    {
        var services = new ServiceCollection();
        services.AddStateflows(stateflows => stateflows
            .AddStateMachines(machines => machines.AddStateMachine<Machine>("ordering-machine"))
            .AddActivities(activities => activities.AddActivity<Activity>("ordering-activity"))
            .AddActions(actions => actions.AddAction<Action>("ordering-action", buildAction: action => action.AddConsumedEvent<Event>())));
        using var provider = services.BuildServiceProvider();

        var machine = new MachineVisitor();
        await provider.GetRequiredService<IStateMachinesRegister>().VisitStateMachineAsync("ordering-machine", 1, machine);
        CollectionAssert.AreEqual(new[] { "begin", "type", "vertex", "end" }, machine.Events);

        var activity = new ActivityVisitor();
        await ((ActivitiesRegister)provider.GetRequiredService<IActivitiesRegister>())
            .VisitActivityAsync("ordering-activity", 1, activity);
        CollectionAssert.AreEqual(new[] { "begin", "type", "node", "end" }, activity.Events);

        var action = new ActionVisitor();
        await provider.GetRequiredService<IActionsRegister>().VisitActionsAsync(action);
        CollectionAssert.AreEqual(new[] { "begin", "type", "event", "end" }, action.Events);
    }

    private sealed class MachineVisitor : StateMachineVisitor
    {
        public List<string> Events { get; } = [];
        public override Task StateMachineAddingAsync(string id, string name, int version, BehaviorClass? owner, BehaviorClass? parent, bool hasDefaultInstance)
        {
            Events.Add("begin");
            return Task.CompletedTask;
        }
        public override Task StateMachineTypeAddedAsync<TStateMachine>(string id, string name, int version)
        {
            Events.Add("type");
            return Task.CompletedTask;
        }
        public override Task VertexAddedAsync(string id, string name, int version, string vertexName, VertexType vertexType,
            string? parentVertexName, string? regionId, int? regionIndex)
        {
            Events.Add("vertex");
            return Task.CompletedTask;
        }
        public override Task StateMachineAddedAsync(string id, string name, int version)
        {
            Events.Add("end");
            return Task.CompletedTask;
        }
    }

    private sealed class ActivityVisitor : Stateflows.Activities.ActivityVisitor
    {
        public List<string> Events { get; } = [];
        public override Task ActivityAddingAsync(string id, string name, int version, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
        {
            Events.Add("begin");
            return Task.CompletedTask;
        }
        public override Task ActivityTypeAddedAsync<TActivity>(string id, string name, int version)
        {
            Events.Add("type");
            return Task.CompletedTask;
        }
        public override Task NodeAddedAsync(string id, string name, int version, string nodeName, NodeType nodeType, string? parentNodeId)
        {
            Events.Add("node");
            return Task.CompletedTask;
        }
        public override Task ActivityAddedAsync(string id, string name, int version)
        {
            Events.Add("end");
            return Task.CompletedTask;
        }
    }

    private sealed class ActionVisitor : Stateflows.Actions.ActionVisitor
    {
        public List<string> Events { get; } = [];
        public override Task ActionAddingAsync(string id, string name, int version, string? behaviorClassType = null,
            BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
        {
            Events.Add("begin");
            return Task.CompletedTask;
        }
        public override Task ActionTypeAddedAsync<TAction>(string id, string name, int version)
        {
            Events.Add("type");
            return Task.CompletedTask;
        }
        public override Task CustomEventAddedAsync<TEvent>(string id, string name, int version, IReadOnlyList<string> eventNames, BehaviorStatus[] supportedStatuses)
        {
            Events.Add("event");
            return Task.CompletedTask;
        }
        public override Task ActionAddedAsync(string id, string name, int version)
        {
            Events.Add("end");
            return Task.CompletedTask;
        }
    }
}
